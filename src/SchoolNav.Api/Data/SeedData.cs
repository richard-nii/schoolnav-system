using Microsoft.AspNetCore.Identity;
using SchoolNav.Api.Models;

namespace SchoolNav.Api.Data;

// Seeds a realistic starting dataset for the University of Ghana, Legon campus.
// NOTE: Coordinates below are approximate placeholders for well-known landmarks,
// based on Legon's general layout. Use the Admin dashboard's "click on map to
// place" tool to correct any pin so it matches the real building precisely.
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.EnsureCreatedAsync();

        // --- Roles & default admin account ---
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (await userManager.FindByEmailAsync("admin@ug.edu.gh") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@ug.edu.gh",
                Email = "admin@ug.edu.gh",
                FullName = "Campus Admin",
                EmailConfirmed = true
            };
            // CHANGE THIS PASSWORD after first login.
            var result = await userManager.CreateAsync(admin, "Admin@12345");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (context.Nodes.Any()) return; // already seeded

        // --- Walkway graph nodes (approximate Legon layout) ---
        var mainGate = new Node { Latitude = 5.6465, Longitude = -0.1895, Type = NodeType.Entrance, Label = "Main Gate" };
        var n1 = new Node { Latitude = 5.6478, Longitude = -0.1888, Type = NodeType.Junction, Label = "University Ave / Volta Junction" };
        var n2 = new Node { Latitude = 5.6493, Longitude = -0.1878, Type = NodeType.Junction, Label = "Near Akuafo Hall" };
        var n3 = new Node { Latitude = 5.6499, Longitude = -0.1870, Type = NodeType.Junction, Label = "Balme Library Roundabout" };
        var n4 = new Node { Latitude = 5.6508, Longitude = -0.1863, Type = NodeType.Junction, Label = "Near UGCS" };
        var n5 = new Node { Latitude = 5.6520, Longitude = -0.1860, Type = NodeType.Junction, Label = "Legon Hill Path" };
        var n6 = new Node { Latitude = 5.6489, Longitude = -0.1857, Type = NodeType.Junction, Label = "Night Market Junction" };
        var n7 = new Node { Latitude = 5.6472, Longitude = -0.1880, Type = NodeType.Junction, Label = "Near Business School" };

        context.Nodes.AddRange(mainGate, n1, n2, n3, n4, n5, n6, n7);
        await context.SaveChangesAsync();

        // --- Walkway edges (bidirectional; distance auto-computed) ---
        var edges = new[]
        {
            (mainGate, n1), (n1, n7), (n1, n2), (n2, n3),
            (n3, n4), (n4, n5), (n3, n6), (n6, n5), (n7, n3)
        };

        foreach (var (a, b) in edges)
        {
            context.Edges.Add(new Edge
            {
                NodeAId = a.Id,
                NodeBId = b.Id,
                DistanceMeters = Services.PathfindingService.HaversineMeters(a.Latitude, a.Longitude, b.Latitude, b.Longitude)
            });
        }
        await context.SaveChangesAsync();

        // --- Locations (searchable destinations) ---
        context.Locations.AddRange(
            new Location { Name = "Balme Library", Category = LocationCategory.Library, Description = "Main university library.", Latitude = 5.6502, Longitude = -0.1870, NearestNodeId = n3.Id },
            new Location { Name = "Great Hall", Category = LocationCategory.Hall, Description = "Main auditorium, seats 1,500.", Latitude = 5.6522, Longitude = -0.1863, NearestNodeId = n5.Id },
            new Location { Name = "Commonwealth Hall", Category = LocationCategory.Hall, Description = "Hall of residence.", Latitude = 5.6531, Longitude = -0.1862, NearestNodeId = n5.Id },
            new Location { Name = "University of Ghana Business School (UGBS)", Category = LocationCategory.AdminBuilding, Description = "Business School building.", Latitude = 5.6472, Longitude = -0.1878, NearestNodeId = n7.Id },
            new Location { Name = "UG Computing Systems (UGCS)", Category = LocationCategory.Laboratory, Description = "Campus IT and computing services.", Latitude = 5.6516, Longitude = -0.1858, NearestNodeId = n4.Id },
            new Location { Name = "Volta Hall", Category = LocationCategory.Hall, Description = "Hall of residence.", Latitude = 5.6478, Longitude = -0.1875, NearestNodeId = n1.Id },
            new Location { Name = "Akuafo Hall", Category = LocationCategory.Hall, Description = "Hall of residence.", Latitude = 5.6497, Longitude = -0.1889, NearestNodeId = n2.Id },
            new Location { Name = "Department of Computer Science", Category = LocationCategory.Classroom, Description = "Lecture rooms and offices.", Latitude = 5.6493, Longitude = -0.1863, NearestNodeId = n3.Id },
            new Location { Name = "Night Market", Category = LocationCategory.Cafeteria, Description = "Food vendors and student hangout spot.", Latitude = 5.6489, Longitude = -0.1855, NearestNodeId = n6.Id },
            new Location { Name = "Main Gate", Category = LocationCategory.Other, Description = "Primary campus entrance.", Latitude = 5.6465, Longitude = -0.1895, NearestNodeId = mainGate.Id }
        );

        await context.SaveChangesAsync();
    }
}
