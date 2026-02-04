using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;
using Application.UseCases.Roles.DTOs;
using Domain.Entities.Roles;

namespace Application.UseCases.Roles.CQRS.Queries.GetById;

public sealed class RoleGetByIdHandler : IRequestHandler<RoleGetByIdQuery, OperationResult<RoleDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public RoleGetByIdHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<RoleDTO>> Handle(
        RoleGetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(request.Id, cancellationToken);

        if (role is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: RoleMessages.NotFound.WithId(request.Id.ToString()));
        }

        var roleDto = _mapper.Map<RoleDTO>(role);

        return Result.Success(roleDto);
    }
}
