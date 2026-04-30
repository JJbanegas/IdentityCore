using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class IdentityCoreDbContext : IdentityDbContext
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public IdentityCoreDbContext(DbContextOptions<IdentityCoreDbContext> options) : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        /*optionsBuilder.UseLazyLoadingProxies()
            .LogTo(Console.WriteLine, LogLevel);*/
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Token).IsRequired().HasMaxLength(500);
            entity.Property(r => r.UserId).IsRequired();
            entity.HasIndex(r => r.Token).IsUnique();
            entity.ToTable("RefreshTokens");
        });
    }
}