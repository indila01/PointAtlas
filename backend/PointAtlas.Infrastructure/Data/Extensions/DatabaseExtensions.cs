using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PointAtlas.Core.Entities;

namespace PointAtlas.Infrastructure.Data.Extensions;

public static class DatabaseExtensions
{
    /// <summary>
    /// Seeds the database with initial data including roles, users, and sample markers.
    /// This method should be called during application startup.
    /// </summary>
    public static async Task<IApplicationBuilder> SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<PointAtlasDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = services.GetRequiredService<ILogger<PointAtlasDbContext>>();

            logger.LogInformation("Starting database seeding...");

            await DbSeeder.SeedDatabaseAsync(context, userManager, roleManager);

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<PointAtlasDbContext>>();
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }

        return app;
    }

    /// <summary>
    /// Ensures the database is created and all migrations are applied.
    /// </summary>
    public static async Task<IApplicationBuilder> MigrateDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<PointAtlasDbContext>();
            var logger = services.GetRequiredService<ILogger<PointAtlasDbContext>>();

            logger.LogInformation("Applying database migrations...");

            await context.Database.MigrateAsync();

            logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<PointAtlasDbContext>>();
            logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }

        return app;
    }
}
