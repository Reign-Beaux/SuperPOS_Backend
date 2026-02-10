using Domain.Entities.Users;
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;

namespace Application.UseCases.Users.CQRS.Commands.Delete;

public sealed class UserDeleteHandler
    : IRequestHandler<UserDeleteCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UserDeleteHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<VoidResult>> Handle(
        UserDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: UserMessages.NotFound.WithId(request.Id.ToString()));
        }

        _unitOfWork.Users.Delete(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
