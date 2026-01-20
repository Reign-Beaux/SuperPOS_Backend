namespace Domain.Entities.Roles;

public class Role : BaseCatalog
{
    // Navigation Properties
    public ICollection<Users.UserRole> UserRoles { get; set; } = [];
}
