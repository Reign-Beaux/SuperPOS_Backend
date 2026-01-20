using Domain.Entities.Sales;

namespace Domain.Entities.Users;

public class User : BaseEntity
{
    public User() { }

    public string Name { get; set; } = string.Empty;
    public string FirstLastname { get; set; } = string.Empty;
    public string? SecondLastname { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHashed { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // Navigation Properties
    public ICollection<Sale> Sales { get; set; } = [];
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
