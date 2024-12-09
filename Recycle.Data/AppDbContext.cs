using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Recycle.Data.Entities;
using Recycle.Data.Entities.Identity;

namespace Recycle.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<TrashCan> TrashCans { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<Location> Locations { get; set; }

    public DbSet<TrashCanMaterialLocation> TrashCansMaterialLocations { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<IdentityUserLogin<Guid>>();
        modelBuilder.Ignore<IdentityUserToken<Guid>>();
        modelBuilder.Ignore<IdentityRoleClaim<Guid>>();

        base.OnModelCreating(modelBuilder);
    }
}
