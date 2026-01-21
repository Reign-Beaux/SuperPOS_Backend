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
        var user = _mapper.Map<User>(request);
        user.PasswordHashed = _userRules.HashPassword(request.Password);

        var validationResult = await _userRules.EnsureUniquenessAsync(
            user,
            isUpdate: false,
            cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        _unitOfWork.Repository<User>().Add(user);

        var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affectedRows == 0)
            return Result.Error(
                ErrorResult.BadRequest,
                detail: UserMessages.Create.Failed);

        var userDto = _mapper.Map<UserDTO>(user);

        return new OperationResult<UserDTO>(StatusResult.Created, userDto);
    }
}
