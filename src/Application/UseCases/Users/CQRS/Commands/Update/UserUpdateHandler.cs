using Domain.Entities.Roles;
using Domain.Entities.Users;
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Services;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.UseCases.Users.CQRS.Commands.Update;

public sealed class UserUpdateHandler
    : IRequestHandler<UserUpdateCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserUniquenessChecker _uniquenessChecker;
    private readonly IEncryptionService _encryptionService;

    public UserUpdateHandler(
        IUnitOfWork unitOfWork,
        IUserUniquenessChecker uniquenessChecker,
        IEncryptionService encryptionService)
    {
        _unitOfWork = unitOfWork;
        _uniquenessChecker = uniquenessChecker;
        _encryptionService = encryptionService;
    }

    public async Task<OperationResult<VoidResult>> Handle(
        UserUpdateCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: UserMessages.NotFound.WithId(request.Id.ToString()));
        }

        // Validate role exists if changed
        if (request.RoleId != user.RoleId)
        {
            var role = await _unitOfWork.Repository<Role>().GetByIdAsync(request.RoleId, cancellationToken);
            if (role is null)
                return Result.Error(
                    ErrorResult.NotFound,
                    detail: $"Role with ID {request.RoleId} not found");
        }

        // Check email uniqueness if email changed
        if (request.Email != user.Email)
        {
            var isEmailUnique = await _uniquenessChecker.IsEmailUniqueAsync(
                request.Email,
                excludeId: user.Id,
                cancellationToken);

            if (!isEmailUnique)
            {
                return Result.Error(
                    ErrorResult.Exists,
                    detail: UserMessages.AlreadyExists.WithEmail(request.Email));
            }
        }

        // Update user info using domain methods
        var name = PersonName.Create(request.Name, request.FirstLastname, request.SecondLastname);
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : PhoneNumber.Create(request.Phone);
        user.UpdateInfo(name, phone);

        // Update email if changed
        if (request.Email != user.Email)
        {
            var email = Email.Create(request.Email);
            user.UpdateEmail(email);
        }

        // Update role if changed
        if (request.RoleId != user.RoleId)
        {
            user.UpdateRole(request.RoleId);
        }

        // Change password if provided
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            // Validate password complexity
            Password password;
            try
            {
                password = Password.Create(request.Password);
            }
            catch (InvalidValueObjectException ex)
            {
                return Result.Error(ErrorResult.BadRequest, detail: ex.Message);
            }

            var hashedPassword = _encryptionService.HashText(password.Value);
            user.ChangePassword(hashedPassword);
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
