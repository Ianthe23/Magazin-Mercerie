using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using magazin_mercerie;

public class AngajatRepository : Repository<Angajat>, IAngajatRepository
{
    private readonly magazin_mercerie.AppDbContext _context;

    public AngajatRepository(magazin_mercerie.AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Angajat>> GetBySalariuAsync(int salariu)
    {
        return await GetDbSet().Where(a => a.Salariu == salariu).ToListAsync();
    }

    public async Task<List<AngajatMagazin>> GetByTipAsync(TipAngajat tip)
    {
        return await GetDbSet().Where(a => a.Tip == tip).Cast<AngajatMagazin>().ToListAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await GetDbSet().FirstOrDefaultAsync(a => a.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await GetDbSet().FirstOrDefaultAsync(a => a.Email == email);
    }

    public override async Task<Angajat> AddAsync(Angajat entity)
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

    public override async Task<Angajat> UpdateAsync(Angajat entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
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
        return await _context.SaveChangesAsync() > 0;
    }

}
