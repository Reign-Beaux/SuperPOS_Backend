using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Roles;

namespace Application.UseCases.Roles.CQRS.Commands.Update;

public sealed class RoleUpdateHandler
    : IRequestHandler<RoleUpdateCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly RoleRules _roleRules;

    public RoleUpdateHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _roleRules = new RoleRules(unitOfWork);
    }

    public async Task<OperationResult<VoidResult>> Handle(
        RoleUpdateCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _roleRules.GetByIdAsync(request.Id, cancellationToken);

        if (role is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: RoleMessages.NotFound.WithId(request.Id.ToString()));
        }

        request.Adapt(role);

        var validationResult = await _roleRules.EnsureUniquenessAsync(
            role,
            isUpdate: true,
            cancellationToken);

        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        _unitOfWork.Repository<Role>().Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
