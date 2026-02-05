using Domain.Entities.Users;

namespace Domain.Entities.Roles;

/// <summary>
/// Role aggregate root representing a system role for authorization.
/// </summary>
public class Role : BaseCatalog, IAggregateRoot
{
    public Role() { }

    // Navigation Properties
    public ICollection<User> Users { get; set; } = [];
}
