using Domain.Entities.Roles;
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;

namespace Application.UseCases.Roles.CQRS.Commands.Delete;

public sealed class RoleDeleteHandler
    : IRequestHandler<RoleDeleteCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleDeleteHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<VoidResult>> Handle(
        RoleDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(request.Id, cancellationToken);

        if (role is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: RoleMessages.NotFound.WithId(request.Id.ToString()));
        }

        _unitOfWork.Roles.Delete(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
