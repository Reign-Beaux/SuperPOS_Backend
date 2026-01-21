using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Roles.DTOs;
using Domain.Entities.Roles;
using MapsterMapper;

namespace Application.UseCases.Roles.CQRS.Commands.Create;

public sealed class CreateRoleHandler
    : IRequestHandler<CreateRoleCommand, OperationResult<RoleDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RoleRules _roleRules;

    public CreateRoleHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _roleRules = new RoleRules(unitOfWork);
    }

    public async Task<OperationResult<RoleDTO>> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Create Role entity from command
        var role = new Role
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty
        };

        // Validate uniqueness
        var validationResult = await _roleRules.EnsureUniquenessAsync(
            role,
            isUpdate: false,
            cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        // Add to repository
        _unitOfWork.Repository<Role>().Add(role);

        // Save changes
        var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affectedRows == 0)
            return Result.Error(
                ErrorResult.BadRequest,
                detail: RoleMessages.Create.Failed);

        // Map to DTO and return
        var roleDto = _mapper.Map<RoleDTO>(role);

        return new OperationResult<RoleDTO>(StatusResult.Created, roleDto);
    }
}
