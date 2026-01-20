using Domain.Entities.Users;

namespace Domain.Entities.Roles;

public class Role : BaseCatalog
{
    public Role() { }

    // Navigation Properties
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
