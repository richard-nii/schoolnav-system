using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolNav.Api.Models;

namespace SchoolNav.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Node> Nodes => Set<Node>();
    public DbSet<Edge> Edges => Set<Edge>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Location>()
            .HasOne(l => l.NearestNode)
            .WithMany()
            .HasForeignKey(l => l.NearestNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Edge>()
            .HasOne(e => e.NodeA)
            .WithMany(n => n.EdgesFrom)
            .HasForeignKey(e => e.NodeAId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Edge>()
            .HasOne(e => e.NodeB)
            .WithMany(n => n.EdgesTo)
            .HasForeignKey(e => e.NodeBId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Location>().HasIndex(l => l.Name);
    }
}
