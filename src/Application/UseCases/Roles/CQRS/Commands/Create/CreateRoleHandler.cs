using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Entities.Roles;
using Domain.Repositories;

namespace Application.UseCases.Roles.CQRS.Commands.Create;

public sealed class CreateRoleHandler
    : IRequestHandler<CreateRoleCommand, OperationResult<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleHandler(IUnitOfWork unitOfWork)
    {
        
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<Guid>> Handle(
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

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description
        };

        _unitOfWork.Roles.Add(role);

        var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affectedRows == 0)
            return Result.Error(
                ErrorResult.BadRequest,
                detail: RoleMessages.Create.Failed);

        return Result.Created(role.Id);
    }
}
