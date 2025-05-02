using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

public class ComandaRepository : Repository<Comanda>, IComandaRepository
{
    public ComandaRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Comanda>> GetByClientIdAsync(Guid clientId)
    {
        return await GetDbSet().Where(c => c.ClientId == clientId).ToListAsync();
    }

    public async Task<List<Comanda>> GetByAngajatIdAsync(Guid angajatId)
    {
        return await GetDbSet().Where(c => c.AngajatId == angajatId).ToListAsync();
    }

    public async Task<List<Comanda>> GetByStatusAsync(Status status)
    {
        return await GetDbSet().Where(c => c.Status == status).ToListAsync();
    }

    public override async Task<Comanda> AddAsync(Comanda entity)
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

    public override async Task<Comanda> UpdateAsync(Comanda entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        GetDbSet().Update(entity);
        await SaveChangesAsync();
        return entity;
    }

    public override async Task<List<Comanda>> GetAllAsync()
    {
        return await GetDbSet().ToListAsync();
    }

    public override async Task<Comanda?> GetByIdAsync(Guid id)
    {
        return await GetDbSet().FindAsync(id);
    }

    public override async Task<bool> SaveChangesAsync()
    {
        return await base.SaveChangesAsync();
    }
}

