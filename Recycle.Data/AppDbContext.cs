using Microsoft.EntityFrameworkCore;
using Recycle.Data.Entities;

namespace Recycle.Data;

public class AppDbContext : DbContext
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
}
