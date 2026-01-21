using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Users.DTOs;
using Domain.Entities.Users;

namespace Application.UseCases.Users.CQRS.Queries.GetById;

public sealed class UserGetByIdHandler : IRequestHandler<UserGetByIdQuery, OperationResult<UserDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<User> _userRepository;

    public UserGetByIdHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _userRepository = unitOfWork.Repository<User>();
    }

    public async Task<OperationResult<UserDTO>> Handle(
        UserGetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: UserMessages.NotFound.WithId(request.Id.ToString()));
        }

        var userDto = _mapper.Map<UserDTO>(user);

        return Result.Success(userDto);
    }
}
