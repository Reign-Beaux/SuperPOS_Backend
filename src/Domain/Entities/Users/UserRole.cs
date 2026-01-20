namespace Domain.Entities.Users;

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public Roles.Role Role { get; set; } = null!;
}
