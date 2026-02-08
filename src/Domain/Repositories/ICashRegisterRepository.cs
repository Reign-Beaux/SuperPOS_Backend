using Domain.Entities.CashRegisters;

namespace Domain.Repositories;

public interface ICashRegisterRepository : IRepositoryBase<CashRegister>
{
    Task<CashRegister?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashRegister>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashRegister>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashRegister>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<CashRegister?> GetLastClosingAsync(CancellationToken cancellationToken = default);
}
