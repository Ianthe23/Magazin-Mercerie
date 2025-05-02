using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

public class AngajatMagazinRepository : Repository<AngajatMagazin>, IAngajatMagazinRepository
{
    public AngajatMagazinRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<AngajatMagazin>> GetByTipAsync(TipAngajat tip)
    {
        return await GetDbSet().Where(a => a.Tip == tip).ToListAsync();
    }

    public override async Task<AngajatMagazin> AddAsync(AngajatMagazin entity)
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

    public override async Task<AngajatMagazin> UpdateAsync(AngajatMagazin entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        GetDbSet().Update(entity);
        await SaveChangesAsync();
        return entity;
    }

    public override async Task<List<AngajatMagazin>> GetAllAsync()
    {
        return await GetDbSet().ToListAsync();
    }

    public override async Task<AngajatMagazin?> GetByIdAsync(Guid id)
    {
        return await GetDbSet().FindAsync(id);
    }

    public override async Task<bool> SaveChangesAsync()
    {
        return await base.SaveChangesAsync();
    }

}

