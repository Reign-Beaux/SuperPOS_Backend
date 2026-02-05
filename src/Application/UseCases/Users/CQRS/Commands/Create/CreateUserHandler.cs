using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Services;
using Application.UseCases.Users.DTOs;
using Domain.Entities.Roles;
using Domain.Entities.Users;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.UseCases.Users.CQRS.Commands.Create;

public sealed class CreateUserHandler
    : IRequestHandler<CreateUserCommand, OperationResult<UserDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserUniquenessChecker _uniquenessChecker;
    private readonly IEncryptionService _encryptionService;

    public CreateUserHandler(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IUserUniquenessChecker uniquenessChecker,
        IEncryptionService encryptionService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _uniquenessChecker = uniquenessChecker;
        _encryptionService = encryptionService;
    }

    public async Task<OperationResult<UserDTO>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        // Validate role exists
        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
            return new OperationResult<UserDTO>(
                StatusResult.NotFound,
                default!,
                new Error { Title = "Not Found", Detail = $"Role with ID {request.RoleId} not found" });

        // Validate email uniqueness
        var isEmailUnique = await _uniquenessChecker.IsEmailUniqueAsync(
            request.Email,
            cancellationToken: cancellationToken);

        if (!isEmailUnique)
            return Result.Error(
                ErrorResult.Exists,
                detail: UserMessages.AlreadyExists.WithEmail(request.Email));

        // Hash password
        var hashedPassword = _encryptionService.HashText(request.Password);

        // Create value objects
        var name = PersonName.Create(request.Name, request.FirstLastname, request.SecondLastname);
        var email = Email.Create(request.Email);

        var phone = string.IsNullOrWhiteSpace(request.Phone)
            ? null
            : PhoneNumber.Create(request.Phone);

        // Use domain factory method
        var user = User.Create(name, email, hashedPassword, request.RoleId, phone);

        // Use specific repository
        _unitOfWork.Users.Add(user);

        var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affectedRows == 0)
            return Result.Error(
                ErrorResult.BadRequest,
                detail: UserMessages.Create.Failed);

        var userDto = _mapper.Map<UserDTO>(user);

        return new OperationResult<UserDTO>(StatusResult.Created, userDto);
    }
}
