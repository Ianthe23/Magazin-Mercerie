using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

public class AngajatRepository : Repository<Angajat>, IAngajatRepository
{
    public AngajatRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Angajat>> GetByNumeAsync(string nume)
    {
        return await GetDbSet().Where(a => a.Nume == nume).ToListAsync();
    }

    public async Task<List<Angajat>> GetByEmailAsync(string email)
    {
        return await GetDbSet().Where(a => a.Email == email).ToListAsync();
    }

    public async Task<List<Angajat>> GetBySalariuAsync(int salariu)
    {
        return await GetDbSet().Where(a => a.Salariu == salariu).ToListAsync();
    }

    public override async Task<Angajat> AddAsync(Angajat entity)
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

    public override async Task<Angajat> UpdateAsync(Angajat entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        GetDbSet().Update(entity);
        await SaveChangesAsync();
        return entity;
    }

    public override async Task<List<Angajat>> GetAllAsync()
    {
        return await GetDbSet().ToListAsync();
    }

    public override async Task<Angajat?> GetByIdAsync(Guid id)
    {
        return await GetDbSet().FindAsync(id);
    }

    public override async Task<bool> SaveChangesAsync()
    {
        return await base.SaveChangesAsync();
    }
}

