using Domain.Entities.Users;
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Users.DTOs;
using Domain.Repositories;

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

        var userDto = _mapper.Map<UserDTO>(user);

        return Result.Success(userDto);
    }
}
