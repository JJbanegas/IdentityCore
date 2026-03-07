namespace Infrastructure.Interfaces;

public class UnitOfWork : IUnitOfWork
{
    private readonly IdentityCoreDbContext _dbContext;

    public UnitOfWork(IdentityCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<T> GetRepository<T>() where T : class
    {
        return new Repository<T>(_dbContext);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }


    public void Rollback()
    {
        _dbContext.ChangeTracker.Clear();
    }

    private void Dispose()
    {
        _dbContext.Dispose();
    }
}