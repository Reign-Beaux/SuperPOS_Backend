using Domain.Entities.Users;

namespace Domain.Entities.CashRegisters;

public class CashRegister : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public DateTime OpeningDate { get; private set; }
    public DateTime ClosingDate { get; private set; }
    public decimal InitialCash { get; private set; }
    public decimal FinalCash { get; private set; }
    public decimal TotalSales { get; private set; }
    public int TotalTransactions { get; private set; }
    public int TotalItemsSold { get; private set; }
    public decimal Difference { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public User? User { get; set; }

    // Constructor privado para EF Core
    private CashRegister() { }

    // Factory method
    public static CashRegister Create(
        Guid userId,
        DateTime openingDate,
        DateTime closingDate,
        decimal initialCash,
        decimal finalCash,
        decimal totalSales,
        int totalTransactions,
        int totalItemsSold,
        string? notes = null)
    {
        var cashRegister = new CashRegister
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            OpeningDate = openingDate,
            ClosingDate = closingDate,
            InitialCash = initialCash,
            FinalCash = finalCash,
            TotalSales = totalSales,
            TotalTransactions = totalTransactions,
            TotalItemsSold = totalItemsSold,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        cashRegister.CalculateDifference();
        return cashRegister;
    }

    // Calcula la diferencia entre lo esperado y lo real
    private void CalculateDifference()
    {
        // Diferencia = (Efectivo final - Efectivo inicial) - Total de ventas
        // Si es 0: cuadra perfecto
        // Si es positivo: sobra dinero
        // Si es negativo: falta dinero
        Difference = (FinalCash - InitialCash) - TotalSales;
    }
}
