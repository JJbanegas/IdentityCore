using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public class IdentityCoreDbContext : IdentityDbContext
{
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
    }
}