using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Roles.DTOs;
using Domain.Entities.Roles;
using Domain.Repositories;

namespace Application.UseCases.Roles.CQRS.Commands.Create;

public sealed class CreateRoleHandler
    : IRequestHandler<CreateRoleCommand, OperationResult<RoleDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<RoleDTO>> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Check name uniqueness
        var nameExists = await _unitOfWork.Roles.ExistsByNameAsync(
            request.Name,
            cancellationToken: cancellationToken);

        if (nameExists)
            return Result.Error(
                ErrorResult.Exists,
                detail: RoleMessages.AlreadyExists.WithName(request.Name));

        var role = _mapper.Map<Role>(request);

        _unitOfWork.Roles.Add(role);

        var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affectedRows == 0)
            return Result.Error(
                ErrorResult.BadRequest,
                detail: RoleMessages.Create.Failed);

        var roleDto = _mapper.Map<RoleDTO>(role);

        return new OperationResult<RoleDTO>(StatusResult.Created, roleDto);
    }
}
