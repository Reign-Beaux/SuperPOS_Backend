using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Domain.Entities.Returns;

namespace Application.UseCases.Returns.CQRS.Commands.Approve;

public class ApproveReturnHandler : IRequestHandler<ApproveReturnCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApproveReturnHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<VoidResult>> Handle(ApproveReturnCommand request, CancellationToken cancellationToken)
    {
        // 1. Get return
        var returnEntity = await _unitOfWork.Returns.GetByIdWithDetailsAsync(request.ReturnId, cancellationToken);
        if (returnEntity == null)
            return Result.Error(ErrorResult.NotFound, detail: ReturnMessages.NotFound);

        // 2. Approve (this raises ReturnApprovedEvent for inventory restoration)
        try
        {
            returnEntity.Approve(request.ApprovedByUserId);
        }
        catch (global::Domain.Exceptions.BusinessRuleViolationException ex)
        {
            return Result.Error(ErrorResult.BadRequest, detail: ex.Message);
        }

        // 3. Save (will dispatch event)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
