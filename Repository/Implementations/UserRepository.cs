using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await GetDbSet().FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await GetDbSet().FirstOrDefaultAsync(u => u.Email == email);
    }

    public override async Task<User> AddAsync(User entity)
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

    public override async Task<User> UpdateAsync(User entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        GetDbSet().Update(entity);
        await SaveChangesAsync();
        return entity;
    }

    public override async Task<List<User>> GetAllAsync()
    {
        return await GetDbSet().ToListAsync();
    }

    public override async Task<User?> GetByIdAsync(Guid id)
    {
        return await GetDbSet().FindAsync(id);
    }

    public override async Task<bool> SaveChangesAsync()
    {
        return await base.SaveChangesAsync();
    }
}

