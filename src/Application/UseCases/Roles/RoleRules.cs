using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Roles;

namespace Application.UseCases.Roles;

internal sealed class RoleRules(IUnitOfWork unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<OperationResult<VoidResult>> EnsureUniquenessAsync(
        Role newRole,
        bool isUpdate,
        CancellationToken cancellationToken)
    {
        var currentRole = await _unitOfWork.Repository<Role>().FirstOrDefaultAsync(
            predicate: isUpdate
                ? r => r.Id != newRole.Id && r.Name == newRole.Name
                : r => r.Name == newRole.Name,
            cancellationToken
        );

        if (currentRole is null)
            return Result.Success();

        if (currentRole.Name == newRole.Name)
            return Result.Error(
                ErrorResult.Exists,
                detail: RoleMessages.AlreadyExists.WithName(newRole.Name));

        return Result.Success();
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Role>().GetByIdAsync(id, cancellationToken);
    }
}
