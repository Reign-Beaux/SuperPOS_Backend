using Domain.Entities.Articles;

namespace Application.Interfaces.Persistence.Context;

public interface ISuperPOSDbContext
{
    DbSet<Article> Articles { get; set; }

    DbSet<T> Set<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
