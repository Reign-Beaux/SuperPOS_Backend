using Domain.Entities.Sales;

namespace Domain.Entities.Customers;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string FirstLastname { get; set; } = string.Empty;
    public string? SecondLastname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime? BirthDate { get; set; }

    // Navigation Properties
    public ICollection<Sale> Sales { get; set; } = [];
}
