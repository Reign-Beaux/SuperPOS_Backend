using Domain.Entities;
using Domain.Entities.Articles;
using Domain.Entities.Customers;
using Domain.Entities.Inventories;
using Domain.Entities.Roles;
using Domain.Entities.Sales;
using Domain.Entities.Users;

namespace Infrastructure.Persistence.Context;

public class SuperPOSDbContext(DbContextOptions<SuperPOSDbContext> options) : DbContext(options)
{
    public DbSet<Article> Articles { get; set; } = null!;
    public DbSet<Inventory> Inventory { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<Sale> Sales { get; set; } = null!;
    public DbSet<SaleDetail> SaleDetails { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SuperPOSDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
}
