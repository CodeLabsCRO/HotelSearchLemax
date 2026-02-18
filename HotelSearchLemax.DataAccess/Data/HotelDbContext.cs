using HotelSearchLemax.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelSearchLemax.DataAccess.Data;

public class HotelDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
    {
    }

    public HotelDbContext(DbContextOptions<HotelDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges();
    }

    private void SetAuditFields()
    {
        var currentUser = GetCurrentUser();
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUser;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedAt = now;
                    entry.Entity.ModifiedBy = currentUser;
                    break;
            }
        }
    }

    private string? GetCurrentUser()
    {
        return _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    }
}
