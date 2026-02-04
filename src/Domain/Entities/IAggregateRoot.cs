namespace Domain.Entities;

/// <summary>
/// Marker interface for aggregate roots.
/// Only aggregate roots can be retrieved directly from repositories.
/// This enforces proper aggregate boundaries and prevents bypassing the aggregate root
/// to access internal entities directly.
///
/// Aggregate roots are the entry points to their aggregates and are responsible for
/// maintaining consistency of all entities within their boundary.
/// </summary>
public interface IAggregateRoot
{
    // This is a marker interface with no members.
    // Its purpose is to identify which entities are aggregate roots
    // so that repository access can be restricted to them only.
}
