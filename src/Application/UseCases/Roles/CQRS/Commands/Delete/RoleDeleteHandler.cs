using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Roles;

namespace Application.UseCases.Roles.CQRS.Commands.Delete;

public sealed class RoleDeleteHandler
    : IRequestHandler<RoleDeleteCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly RoleRules _roleRules;

    public RoleDeleteHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _roleRules = new RoleRules(unitOfWork);
    }

    public async Task<OperationResult<VoidResult>> Handle(
        RoleDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _roleRules.GetByIdAsync(request.Id, cancellationToken);

        if (role is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: RoleMessages.NotFound.WithId(request.Id.ToString()));
        }

        _unitOfWork.Repository<Role>().Delete(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
