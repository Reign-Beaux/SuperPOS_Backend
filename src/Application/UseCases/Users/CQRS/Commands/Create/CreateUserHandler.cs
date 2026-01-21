using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.Interfaces.Services;
using Application.UseCases.Users.DTOs;
using Domain.Entities.Users;
using MapsterMapper;

namespace Application.UseCases.Users.CQRS.Commands.Create;

public sealed class CreateUserHandler
    : IRequestHandler<CreateUserCommand, OperationResult<UserDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserRules _userRules;

    public CreateUserHandler(IMapper mapper, IUnitOfWork unitOfWork, IEncryptionService encryptionService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _userRules = new UserRules(unitOfWork, encryptionService);
    }

    public async Task<OperationResult<UserDTO>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        // Create User entity from command
        var user = new User
        {
            Name = request.Name,
            FirstLastname = request.FirstLastname,
            SecondLastname = request.SecondLastname,
            Email = request.Email,
            PasswordHashed = _userRules.HashPassword(request.Password),
            Phone = request.Phone
        };

        // Validate uniqueness
        var validationResult = await _userRules.EnsureUniquenessAsync(
            user,
            isUpdate: false,
            cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        // Add to repository
        _unitOfWork.Repository<User>().Add(user);

        // Save changes
        var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affectedRows == 0)
            return Result.Error(
                ErrorResult.BadRequest,
                detail: UserMessages.Create.Failed);

        // Map to DTO and return
        var userDto = _mapper.Map<UserDTO>(user);

        return new OperationResult<UserDTO>(StatusResult.Created, userDto);
    }
}
