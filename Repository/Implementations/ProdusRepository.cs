using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

public class ProdusRepository : Repository<Produs>, IProdusRepository
{
    public ProdusRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Produs>> GetByNumeAsync(string nume)
    {
        return await GetDbSet().Where(p => p.Nume == nume).ToListAsync();
    }

    public async Task<List<Produs>> GetByTipAsync(TipProdus tip)
    {
        return await GetDbSet().Where(p => p.Tip == tip).ToListAsync();
    }

    public async Task<List<Produs>> GetByPretAsync(decimal pret)
    {
        return await GetDbSet().Where(p => p.Pret == pret).ToListAsync();
    }

    public async Task<List<Produs>> GetByCantitateAsync(decimal cantitate)
    {
        return await GetDbSet().Where(p => p.Cantitate == cantitate).ToListAsync();
    }

    public override async Task<Produs> AddAsync(Produs entity)
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

    public override async Task<Produs> UpdateAsync(Produs entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        GetDbSet().Update(entity);
        await SaveChangesAsync();
        return entity;
    }

    public override async Task<List<Produs>> GetAllAsync()
    {
        return await GetDbSet().ToListAsync();
    }

    public override async Task<Produs?> GetByIdAsync(Guid id)
    {
        return await GetDbSet().FindAsync(id);
    }

    public override async Task<bool> SaveChangesAsync()
    {
        return await base.SaveChangesAsync();
    }
}
