using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PointAtlas.Core.Entities;

namespace PointAtlas.Infrastructure.Data;

public class PointAtlasDbContext : IdentityDbContext<ApplicationUser>
{
    public PointAtlasDbContext(DbContextOptions<PointAtlasDbContext> options)
        : base(options)
    {
    }

    public DbSet<Marker> Markers => Set<Marker>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PointAtlasDbContext).Assembly);
    }
}
