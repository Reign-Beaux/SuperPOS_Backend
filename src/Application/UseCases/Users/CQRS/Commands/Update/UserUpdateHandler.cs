using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.Interfaces.Services;
using Domain.Entities.Users;

namespace Application.UseCases.Users.CQRS.Commands.Update;

public sealed class UserUpdateHandler
    : IRequestHandler<UserUpdateCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserRules _userRules;

    public UserUpdateHandler(IUnitOfWork unitOfWork, IEncryptionService encryptionService)
    {
        _unitOfWork = unitOfWork;
        _userRules = new UserRules(unitOfWork, encryptionService);
    }

    public async Task<OperationResult<VoidResult>> Handle(
        UserUpdateCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRules.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: UserMessages.NotFound.WithId(request.Id.ToString()));
        }

        request.Adapt(user);

        // Hash password if provided
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHashed = _userRules.HashPassword(request.Password);
        }

        var validationResult = await _userRules.EnsureUniquenessAsync(
            user,
            isUpdate: true,
            cancellationToken);

        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
