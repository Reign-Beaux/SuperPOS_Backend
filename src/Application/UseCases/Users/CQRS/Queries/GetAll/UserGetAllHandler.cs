using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;
using Application.UseCases.Users.DTOs;
using Domain.Entities.Users;

namespace Application.UseCases.Users.CQRS.Queries.GetAll;

public sealed class UserGetAllHandler : IRequestHandler<UserGetAllQuery, OperationResult<IEnumerable<UserDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UserGetAllHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<IEnumerable<UserDTO>>> Handle(
        UserGetAllQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);

        var usersDto = _mapper.Map<IEnumerable<UserDTO>>(users);

        return Result.Success(usersDto);
    }
}
