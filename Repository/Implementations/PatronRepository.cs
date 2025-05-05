using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using magazin_mercerie;

public class PatronRepository : Repository<Patron>, IPatronRepository
{
    private readonly magazin_mercerie.AppDbContext _context;
    
    public PatronRepository(magazin_mercerie.AppDbContext context) : base(context)
    {
        _context = context;
    }

    // IPatronRepository specific methods
    public async Task<List<Patron>> GetByNumeAsync(string nume)
    {
        return await GetDbSet().Where(p => p.Nume == nume).ToListAsync();
    }

    public async Task<List<Patron>> GetByTelefonAsync(string telefon)
    {
        return await GetDbSet().Where(p => p.Telefon == telefon).ToListAsync();
    }

    // IAngajatMagazinRepository methods
    public async Task<List<AngajatMagazin>> GetByTipAsync(TipAngajat tip)
    {
        return await GetDbSet().Where(p => p.Tip == tip).Cast<AngajatMagazin>().ToListAsync();
    }

    // IUserRepository methods
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await GetDbSet().FirstOrDefaultAsync(p => p.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await GetDbSet().FirstOrDefaultAsync(p => p.Email == email);
    }

    // Repository<Patron> implementation
    public override async Task<Patron> AddAsync(Patron entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        await GetDbSet().AddAsync(entity);
        await _context.SaveChangesAsync();
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
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
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
        return await _context.SaveChangesAsync() > 0;
    }
} 