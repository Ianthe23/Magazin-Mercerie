using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using magazin_mercerie;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly magazin_mercerie.AppDbContext _context;

    public Repository(magazin_mercerie.AppDbContext context)
    {
        _context = context;
    }

    protected DbSet<T> GetDbSet()
    {
        return _context.Set<T>();
    }
    
    protected magazin_mercerie.AppDbContext GetContext()
    {
        return _context;
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await GetDbSet().ToListAsync();
    }
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await GetDbSet().FindAsync(id);
    }
    public virtual async Task<T> AddAsync(T entity)
    {
        await GetDbSet().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public virtual async Task<T> UpdateAsync(T entity)
    {
        GetDbSet().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        GetDbSet().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
    public virtual async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() >= 0;
    }
}