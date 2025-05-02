using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

public class PatronRepository : Repository<Patron>, IPatronRepository
{
    public PatronRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Patron>> GetByNumeAsync(string nume)
    {
        return await GetDbSet().Where(p => p.Nume == nume).ToListAsync();
    }

    public async Task<List<Patron>> GetByEmailAsync(string email)
    {
        return await GetDbSet().Where(p => p.Email == email).ToListAsync();
    }

    public async Task<List<Patron>> GetByTelefonAsync(string telefon)
    {
        return await GetDbSet().Where(p => p.Telefon == telefon).ToListAsync();
    }

    public override async Task<Patron> AddAsync(Patron entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        await GetDbSet().AddAsync(entity);
        return entity;
    }

    public override async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        GetDbSet().Remove(entity);
        return await SaveChangesAsync();
    }

    public override async Task<Patron> UpdateAsync(Patron entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        GetDbSet().Update(entity);
        await SaveChangesAsync();
        return entity;
    }

    public override async Task<List<Patron>> GetAllAsync()
    {
        return await GetDbSet().ToListAsync();
    }

    public override async Task<Patron?> GetByIdAsync(Guid id)
    {
        return await GetDbSet().FindAsync(id);
    }

    public override async Task<bool> SaveChangesAsync()
    {
        return await base.SaveChangesAsync();
    }
}

