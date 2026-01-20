using Domain.Entities.Users;

namespace Domain.Entities.Roles;

public class Role : BaseCatalog
{
    // Navigation Properties
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
