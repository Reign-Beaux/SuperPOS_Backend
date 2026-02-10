using Domain.Entities.Roles;
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;

namespace Application.UseCases.Roles.CQRS.Commands.Update;

public sealed class RoleUpdateHandler
    : IRequestHandler<RoleUpdateCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleUpdateHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<VoidResult>> Handle(
        RoleUpdateCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(request.Id, cancellationToken);

        if (role is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: RoleMessages.NotFound.WithId(request.Id.ToString()));
        }

        // Check name uniqueness if name changed
        if (request.Name != role.Name)
        {
            var nameExists = await _unitOfWork.Roles.ExistsByNameAsync(
                request.Name,
                excludeId: role.Id,
                cancellationToken);

            if (nameExists)
            {
                return Result.Error(
                    ErrorResult.Exists,
                    detail: RoleMessages.AlreadyExists.WithName(request.Name));
            }
        }

        request.Adapt(role);

        _unitOfWork.Roles.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
