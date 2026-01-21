using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Roles.DTOs;
using Domain.Entities.Roles;

namespace Application.UseCases.Roles.CQRS.Queries.GetById;

public sealed class RoleGetByIdHandler : IRequestHandler<RoleGetByIdQuery, OperationResult<RoleDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Role> _roleRepository;

    public RoleGetByIdHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _roleRepository = unitOfWork.Repository<Role>();
    }

    public async Task<OperationResult<RoleDTO>> Handle(
        RoleGetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);

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
