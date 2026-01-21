using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.Interfaces.Services;
using Domain.Entities.Users;

namespace Application.UseCases.Users.CQRS.Commands.Delete;

public sealed class UserDeleteHandler
    : IRequestHandler<UserDeleteCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserRules _userRules;

    public UserDeleteHandler(IUnitOfWork unitOfWork, IEncryptionService encryptionService)
    {
        _unitOfWork = unitOfWork;
        _userRules = new UserRules(unitOfWork, encryptionService);
    }

    public async Task<OperationResult<VoidResult>> Handle(
        UserDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRules.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: UserMessages.NotFound.WithId(request.Id.ToString()));
        }

        _unitOfWork.Repository<User>().Delete(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
