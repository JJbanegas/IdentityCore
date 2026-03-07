namespace Infrastructure.Interfaces;

public interface IUnitOfWork
{
    IRepository<T> GetRepository<T>() where T : class;
    
    Task SaveChangesAsync();
    
    void Rollback(); 
}