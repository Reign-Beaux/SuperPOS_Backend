using Domain.Entities.Roles;
using Domain.Entities.Users;
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Users.DTOs;

namespace Application.UseCases.Users.CQRS.Queries.GetById;

public sealed class UserGetByIdHandler : IRequestHandler<UserGetByIdQuery, OperationResult<UserDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UserGetByIdHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<UserDTO>> Handle(
        UserGetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: UserMessages.NotFound.WithId(request.Id.ToString()));
        }

        // Load Role
        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(user.RoleId, cancellationToken);
        if (role != null)
            user.Role = role;

        var userDto = _mapper.Map<UserDTO>(user);

        return Result.Success(userDto);
    }
}
