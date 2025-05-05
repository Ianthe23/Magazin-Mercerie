using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using magazin_mercerie;

public class ClientRepository : Repository<Client>, IClientRepository
{
    private readonly magazin_mercerie.AppDbContext _context;

    public ClientRepository(magazin_mercerie.AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Client>> GetByNumeAsync(string nume)
    {
        return await GetDbSet().Where(c => c.Nume == nume).ToListAsync();
    }

    public async Task<List<Client>> GetByTelefonAsync(string telefon)
    {
        return await GetDbSet().Where(c => c.Telefon == telefon).ToListAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await GetDbSet().FirstOrDefaultAsync(c => c.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await GetDbSet().FirstOrDefaultAsync(c => c.Email == email);
    }

    public override async Task<Client> AddAsync(Client entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        await GetDbSet().AddAsync(entity);
        await SaveChangesAsync();
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
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
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
        return await _context.SaveChangesAsync() > 0;
    }
}