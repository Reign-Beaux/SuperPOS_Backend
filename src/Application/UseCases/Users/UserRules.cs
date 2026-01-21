using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.Interfaces.Services;
using Domain.Entities.Users;

namespace Application.UseCases.Users;

internal sealed class UserRules
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;

    public UserRules(IUnitOfWork unitOfWork, IEncryptionService encryptionService)
    {
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
    }

    public async Task<OperationResult<VoidResult>> EnsureUniquenessAsync(
        User newUser,
        bool isUpdate,
        CancellationToken cancellationToken)
    {
        var currentUser = await _unitOfWork.Repository<User>().FirstOrDefaultAsync(
            predicate: isUpdate
                ? u => u.Id != newUser.Id && u.Email == newUser.Email
                : u => u.Email == newUser.Email,
            cancellationToken
        );

        if (currentUser is null)
            return Result.Success();

        if (currentUser.Email == newUser.Email)
            return Result.Error(
                ErrorResult.Exists,
                detail: UserMessages.AlreadyExists.WithEmail(newUser.Email));

        return Result.Success();
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<User>().GetByIdAsync(id, cancellationToken);
    }

    public string HashPassword(string password)
    {
        return _encryptionService.HashText(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return _encryptionService.VerifyText(password, hashedPassword);
    }
}
