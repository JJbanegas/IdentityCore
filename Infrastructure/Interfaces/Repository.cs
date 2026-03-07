using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Interfaces;

public class Repository<T> : IRepository<T> where T : class
{

    private readonly IdentityCoreDbContext _dbContext;
    
    public Repository(IdentityCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }
    
    public async Task AddAsync(T entity)
    {
        await _dbContext.Set<T>().AddAsync(entity);
    }
    
    public void Update(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
    }
    
    public async Task<IEnumerable<T>> GetByPredicateAsync(Func<T, bool> predicate)
    {
        return await Task.FromResult(_dbContext.Set<T>().Where(predicate).ToList());
    }
}