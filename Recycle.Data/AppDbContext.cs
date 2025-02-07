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

    public new DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<TrashCan> TrashCans { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<EmailMessage> Emails { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<TrashCanMaterial> TrashCansMaterials { get; set; }
    public DbSet<ProductPart> ProductParts { get; set; }
    public DbSet<PartMaterial> PartMaterials { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<IdentityUserLogin<Guid>>();
        modelBuilder.Ignore<IdentityUserToken<Guid>>();
        modelBuilder.Ignore<IdentityRoleClaim<Guid>>();

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
    .HasMany(p => p.ProductParts)
    .WithOne(p => p.Product)
    .HasForeignKey(pp => pp.ProductId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Part>()
    .HasMany(p => p.PartMaterials)
    .WithOne(p => p.Part)
    .HasForeignKey(pp => pp.PartId)
    .OnDelete(DeleteBehavior.Cascade);
    }
}
