using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Users.DTOs;
using Domain.Entities.Users;

namespace Application.UseCases.Users.CQRS.Queries.GetAll;

public sealed class UserGetAllHandler : IRequestHandler<UserGetAllQuery, OperationResult<IEnumerable<UserDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<User> _userRepository;

    public UserGetAllHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _userRepository = unitOfWork.Repository<User>();
    }

    public async Task<OperationResult<IEnumerable<UserDTO>>> Handle(
        UserGetAllQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        var usersDto = _mapper.Map<IEnumerable<UserDTO>>(users);

        return Result.Success(usersDto);
    }
}
