using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Domain.Entities.Returns;

namespace Application.UseCases.Returns.CQRS.Commands.Reject;

public class RejectReturnHandler : IRequestHandler<RejectReturnCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RejectReturnHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<VoidResult>> Handle(RejectReturnCommand request, CancellationToken cancellationToken)
    {
        // 1. Get return
        var returnEntity = await _unitOfWork.Returns.GetByIdWithDetailsAsync(request.ReturnId, cancellationToken);
        if (returnEntity == null)
            return Result.Error(ErrorResult.NotFound, detail: ReturnMessages.NotFound);

        // 2. Reject
        try
        {
            returnEntity.Reject(request.RejectedByUserId, request.RejectionReason);
        }
        catch (global::Domain.Exceptions.BusinessRuleViolationException ex)
        {
            return Result.Error(ErrorResult.BadRequest, detail: ex.Message);
        }

        // 3. Save
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
