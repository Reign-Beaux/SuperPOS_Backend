using System.Runtime.CompilerServices;

// Allow Infrastructure layer to access internal members
// This is necessary for EF Core and repository implementations to access
// protected internal setters on BaseEntity properties
[assembly: InternalsVisibleTo("Infrastructure")]
