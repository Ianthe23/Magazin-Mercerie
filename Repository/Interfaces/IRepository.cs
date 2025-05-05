using System.Collections.Generic;
using System.Threading.Tasks;
using System;

// Non-generic base interface
public interface IBaseRepository 
{
    Task<bool> DeleteAsync(Guid id);
    Task<bool> SaveChangesAsync();
}

// Generic interface for type-specific operations
public interface IRepository<T> : IBaseRepository where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
}
