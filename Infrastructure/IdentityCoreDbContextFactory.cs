using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;

public class IdentityCoreDbContextFactory : IDesignTimeDbContextFactory<IdentityCoreDbContext>
{
    public IdentityCoreDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityCoreDbContext>();
        optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("connectionstring"));

        return new IdentityCoreDbContext(optionsBuilder.Options);
    }
}