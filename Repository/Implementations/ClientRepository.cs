using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

public class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Client>> GetByNumeAsync(string nume)
    {
        return await GetDbSet().Where(c => c.Nume == nume).ToListAsync();
    }

    public async Task<List<Client>> GetByEmailAsync(string email)
    {
        return await GetDbSet().Where(c => c.Email == email).ToListAsync();
    }

    public async Task<List<Client>> GetByTelefonAsync(string telefon)
    {
        return await GetDbSet().Where(c => c.Telefon == telefon).ToListAsync();
    }

    public override async Task<Client> AddAsync(Client entity)
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

    public override async Task<Client> UpdateAsync(Client entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        GetDbSet().Update(entity);
        await SaveChangesAsync();
        return entity;
    }    

    public override async Task<List<Client>> GetAllAsync()
    {
        return await GetDbSet().ToListAsync();
    }

    public override async Task<Client?> GetByIdAsync(Guid id)
    {
        return await GetDbSet().FindAsync(id);
    }

    public override async Task<bool> SaveChangesAsync()
    {
        return await base.SaveChangesAsync();
    }
}

