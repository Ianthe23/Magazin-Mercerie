using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IComandaRepository : IRepository<Comanda>
{
    Task<List<Comanda>> GetByClientIdAsync(Guid clientId);
    Task<List<Comanda>> GetByAngajatIdAsync(Guid angajatId);
    Task<List<Comanda>> GetByStatusAsync(Status status);
    Task<Comanda> CreateOrderWithProductsAsync(Comanda comanda, Dictionary<Guid, decimal> productQuantities);
}

