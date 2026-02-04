using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;
using Application.UseCases.Roles.DTOs;
using Domain.Entities.Roles;

namespace Application.UseCases.Roles.CQRS.Queries.GetAll;

public sealed class RoleGetAllHandler : IRequestHandler<RoleGetAllQuery, OperationResult<IEnumerable<RoleDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public RoleGetAllHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<IEnumerable<RoleDTO>>> Handle(
        RoleGetAllQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Roles.GetAllAsync(cancellationToken);

        var rolesDto = _mapper.Map<IEnumerable<RoleDTO>>(roles);

        return Result.Success(rolesDto);
    }
}
