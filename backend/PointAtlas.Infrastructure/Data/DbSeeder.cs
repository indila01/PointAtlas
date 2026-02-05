using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using PointAtlas.Core.Entities;

namespace PointAtlas.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedDatabaseAsync(
        PointAtlasDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Apply pending migrations
        await context.Database.MigrateAsync();

        // Seed roles
        await SeedRolesAsync(roleManager);

        // Seed users
        var (adminUser, regularUser) = await SeedUsersAsync(userManager);

        // Seed markers
        await SeedMarkersAsync(context, adminUser, regularUser);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task<(ApplicationUser AdminUser, ApplicationUser RegularUser)> SeedUsersAsync(
        UserManager<ApplicationUser> userManager)
    {
        // Seed admin user
        var adminEmail = "admin@pointatlas.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed regular user for demo purposes
        var userEmail = "demo@pointatlas.com";
        var regularUser = await userManager.FindByEmailAsync(userEmail);

        if (regularUser == null)
        {
            regularUser = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                DisplayName = "Demo User",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(regularUser, "Demo@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, "User");
            }
        }

        return (adminUser, regularUser);
    }

    private static async Task SeedMarkersAsync(
        PointAtlasDbContext context,
        ApplicationUser adminUser,
        ApplicationUser regularUser)
    {
        // Check if markers already exist
        if (await context.Markers.AnyAsync())
        {
            return; // Database already seeded
        }

        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);

        var markers = new List<Marker>
        {
            // New York City landmarks
            new Marker
            {
                Id = Guid.NewGuid(),
                Title = "Statue of Liberty",
                Description = "A colossal neoclassical sculpture on Liberty Island in New York Harbor. A symbol of freedom and democracy.",
                Category = "Landmark",
                Latitude = 40.6892,
                Longitude = -74.0445,
                Location = geometryFactory.CreatePoint(new Coordinate(-74.0445, 40.6892)),
                CreatedById = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30),
                Properties = new Dictionary<string, object>
                {
                    { "visitor_info", "Ferry required" },
                    { "entry_fee", "$24" },
                    { "opened", "1886" }
                }
            },
            new Marker
            {
                Id = Guid.NewGuid(),
                Title = "Central Park",
                Description = "An urban park in Manhattan, New York City. One of the most visited parks in the United States.",
                Category = "Park",
                Latitude = 40.7829,
                Longitude = -73.9654,
                Location = geometryFactory.CreatePoint(new Coordinate(-73.9654, 40.7829)),
                CreatedById = regularUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-25),
                Properties = new Dictionary<string, object>
                {
                    { "size", "843 acres" },
                    { "activities", "Walking, jogging, picnics" },
                    { "opened", "1857" }
                }
            },
            new Marker
            {
                Id = Guid.NewGuid(),
                Title = "Metropolitan Museum of Art",
                Description = "The largest art museum in the Americas. Known as 'The Met', it houses over 2 million works spanning 5,000 years.",
                Category = "Museum",
                Latitude = 40.7794,
                Longitude = -73.9632,
                Location = geometryFactory.CreatePoint(new Coordinate(-73.9632, 40.7794)),
                CreatedById = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-20),
                Properties = new Dictionary<string, object>
                {
                    { "entry_fee", "Pay what you wish" },
                    { "collections", "Art from ancient to contemporary" },
                    { "founded", "1870" }
                }
            },
            new Marker
            {
                Id = Guid.NewGuid(),
                Title = "Katz's Delicatessen",
                Description = "Famous Jewish deli established in 1888. Known for pastrami sandwiches and appeared in 'When Harry Met Sally'.",
                Category = "Restaurant",
                Latitude = 40.7223,
                Longitude = -73.9873,
                Location = geometryFactory.CreatePoint(new Coordinate(-73.9873, 40.7223)),
                CreatedById = regularUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15),
                Properties = new Dictionary<string, object>
                {
                    { "cuisine", "Jewish Deli" },
                    { "famous_for", "Pastrami on rye" },
                    { "price_range", "$$" }
                }
            },
            new Marker
            {
                Id = Guid.NewGuid(),
                Title = "Times Square",
                Description = "Major commercial intersection and tourist destination in Midtown Manhattan. Known for bright lights and Broadway theaters.",
                Category = "Landmark",
                Latitude = 40.7580,
                Longitude = -73.9855,
                Location = geometryFactory.CreatePoint(new Coordinate(-73.9855, 40.7580)),
                CreatedById = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                UpdatedAt = DateTime.UtcNow.AddDays(-12),
                Properties = new Dictionary<string, object>
                {
                    { "nickname", "The Crossroads of the World" },
                    { "annual_visitors", "50 million" },
                    { "screens", "3000+" }
                }
            },
            new Marker
            {
                Id = Guid.NewGuid(),
                Title = "Brooklyn Bridge",
                Description = "Historic hybrid cable-stayed/suspension bridge connecting Manhattan and Brooklyn over the East River.",
                Category = "Landmark",
                Latitude = 40.7061,
                Longitude = -73.9969,
                Location = geometryFactory.CreatePoint(new Coordinate(-73.9969, 40.7061)),
                CreatedById = regularUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10),
                Properties = new Dictionary<string, object>
                {
                    { "length", "5,989 feet" },
                    { "completed", "1883" },
                    { "architect", "John Roebling" }
                }
            },
            new Marker
            {
                Id = Guid.NewGuid(),
                Title = "Strand Bookstore",
                Description = "Independent bookstore with '18 Miles of Books'. NYC institution since 1927, known for rare and used books.",
                Category = "Shop",
                Latitude = 40.7336,
                Longitude = -73.9906,
                Location = geometryFactory.CreatePoint(new Coordinate(-73.9906, 40.7336)),
                CreatedById = regularUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-8),
                Properties = new Dictionary<string, object>
                {
                    { "books", "2.5 million" },
                    { "speciality", "Rare and used books" },
                    { "founded", "1927" }
                }
            },
            new Marker
            {
                Id = Guid.NewGuid(),
                Title = "High Line Park",
                Description = "Elevated linear park built on a historic freight rail line. Features gardens, art installations, and city views.",
                Category = "Park",
                Latitude = 40.7480,
                Longitude = -74.0048,
                Location = geometryFactory.CreatePoint(new Coordinate(-74.0048, 40.7480)),
                CreatedById = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                UpdatedAt = DateTime.UtcNow.AddDays(-6),
                Properties = new Dictionary<string, object>
                {
                    { "length", "1.45 miles" },
                    { "opened", "2009" },
                    { "features", "Gardens, art, food vendors" }
                }
            },
            new Marker
            {
                Id = Guid.NewGuid(),
                Title = "Joe's Pizza",
                Description = "Classic NYC pizzeria serving authentic New York-style pizza since 1975. Cash only, no-frills atmosphere.",
                Category = "Restaurant",
                Latitude = 40.7304,
                Longitude = -74.0014,
                Location = geometryFactory.CreatePoint(new Coordinate(-74.0014, 40.7304)),
                CreatedById = regularUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-4),
                Properties = new Dictionary<string, object>
                {
                    { "cuisine", "Pizza" },
                    { "famous_for", "Dollar slice" },
                    { "payment", "Cash only" }
                }
            },
            new Marker
            {
                Id = Guid.NewGuid(),
                Title = "9/11 Memorial & Museum",
                Description = "Memorial and museum honoring the victims of the September 11, 2001 attacks and 1993 World Trade Center bombing.",
                Category = "Museum",
                Latitude = 40.7115,
                Longitude = -74.0134,
                Location = geometryFactory.CreatePoint(new Coordinate(-74.0134, 40.7115)),
                CreatedById = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                Properties = new Dictionary<string, object>
                {
                    { "opened", "2011 (memorial), 2014 (museum)" },
                    { "pools", "Two reflecting pools" },
                    { "significance", "Commemorates 9/11 victims" }
                }
            },
            new Marker
            {
                Id = Guid.NewGuid(),
                Title = "Chelsea Market",
                Description = "Food hall, shopping mall, and television production facility in Manhattan's Chelsea neighborhood.",
                Category = "Shop",
                Latitude = 40.7425,
                Longitude = -74.0061,
                Location = geometryFactory.CreatePoint(new Coordinate(-74.0061, 40.7425)),
                CreatedById = regularUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Properties = new Dictionary<string, object>
                {
                    { "type", "Food hall & market" },
                    { "vendors", "35+" },
                    { "building", "Former Nabisco factory" }
                }
            },
            new Marker
            {
                Id = Guid.NewGuid(),
                Title = "Bryant Park",
                Description = "Public park in Midtown Manhattan. Known for free events, outdoor movie nights, and winter ice skating.",
                Category = "Park",
                Latitude = 40.7536,
                Longitude = -73.9832,
                Location = geometryFactory.CreatePoint(new Coordinate(-73.9832, 40.7536)),
                CreatedById = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Properties = new Dictionary<string, object>
                {
                    { "size", "9.6 acres" },
                    { "events", "Movies, concerts, markets" },
                    { "features", "Reading room, carousel" }
                }
            }
        };

        context.Markers.AddRange(markers);
        await context.SaveChangesAsync();
    }
}
