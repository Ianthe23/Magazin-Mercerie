using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using magazin_mercerie;

public class ComandaRepository : Repository<Comanda>, IComandaRepository
{
    public ComandaRepository(magazin_mercerie.AppDbContext context) : base(context)
    {
    }

    public async Task<List<Comanda>> GetByClientIdAsync(Guid clientId)
    {
        return await GetDbSet()
            .Include(c => c.Client)
            .Include(c => c.Angajat)
            .Include(c => c.ComandaProduse)
                .ThenInclude(cp => cp.Produs)
            .Where(c => c.ClientId == clientId)
            .ToListAsync();
    }

    public async Task<List<Comanda>> GetByAngajatIdAsync(Guid angajatId)
    {
        try
        {
            var query = GetDbSet()
                .Include(c => c.Client)
                .Include(c => c.Angajat)
                .Include(c => c.ComandaProduse)
                    .ThenInclude(cp => cp.Produs)
                .Where(c => c.AngajatId == angajatId);
                
            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error getting orders for employee {angajatId}: {ex.Message}", ex);
        }
    }

    public async Task<List<Comanda>> GetByStatusAsync(Status status)
    {
        return await GetDbSet()
            .Include(c => c.Client)
            .Include(c => c.Angajat)
            .Include(c => c.ComandaProduse)
                .ThenInclude(cp => cp.Produs)
            .Where(c => c.Status == status)
            .ToListAsync();
    }

    public override async Task<Comanda> AddAsync(Comanda entity)
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

    public override async Task<Comanda> UpdateAsync(Comanda entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        
        Console.WriteLine($"[DEBUG] Updating order {entity.Id} with status {entity.Status}");
        
        // Check if entity is being tracked
        var tracked = GetContext().Entry(entity);
        Console.WriteLine($"[DEBUG] Entity state before update: {tracked.State}");
        
        GetDbSet().Update(entity);
        
        var trackedAfter = GetContext().Entry(entity);
        Console.WriteLine($"[DEBUG] Entity state after update call: {trackedAfter.State}");
        
        var saveResult = await SaveChangesAsync();
        Console.WriteLine($"[DEBUG] SaveChangesAsync result: {saveResult}");
        
        // Verify the change was actually saved
        var savedEntity = await GetByIdAsync(entity.Id);
        Console.WriteLine($"[DEBUG] Status in database after save: {savedEntity?.Status}");
        
        return entity;
    }

    public override async Task<List<Comanda>> GetAllAsync()
    {
        return await GetDbSet()
            .Include(c => c.Client)
            .Include(c => c.Angajat)
            .Include(c => c.ComandaProduse)
                .ThenInclude(cp => cp.Produs)
            .ToListAsync();
    }

    public override async Task<Comanda?> GetByIdAsync(Guid id)
    {
        return await GetDbSet()
            .Include(c => c.Client)
            .Include(c => c.Angajat)
            .Include(c => c.ComandaProduse)
                .ThenInclude(cp => cp.Produs)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public override async Task<bool> SaveChangesAsync()
    {
        return await base.SaveChangesAsync();
    }

    public async Task<Comanda> CreateOrderWithProductsAsync(Comanda comanda, Dictionary<Guid, decimal> productQuantities)
    {
        try
        {
            // First, add the order without any navigation properties set
            await GetDbSet().AddAsync(comanda);
            await SaveChangesAsync();
            
            // Now add the ComandaProduse entries directly to the context
            foreach (var kvp in productQuantities)
            {
                var productId = kvp.Key;
                var quantity = kvp.Value;
                
                // Get the product to get its current price
                var product = await GetContext().Produse.FindAsync(productId);
                if (product != null)
                {
                    var comandaProdus = new ComandaProdus
                    {
                        ComandaId = comanda.Id,
                        ProdusId = productId,
                        CantitateComanda = quantity,
                        PretLaComanda = product.Pret
                    };
                    
                    await GetContext().ComandaProduse.AddAsync(comandaProdus);
                }
            }
            
            await GetContext().SaveChangesAsync();
            
            // Return the order with all its related data loaded
            return await GetDbSet()
                .Include(c => c.Client)
                .Include(c => c.Angajat)
                .Include(c => c.ComandaProduse)
                    .ThenInclude(cp => cp.Produs)
                .FirstAsync(c => c.Id == comanda.Id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating order with products: {ex.Message}", ex);
        }
    }
}

