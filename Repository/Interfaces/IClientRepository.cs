using System.Collections.Generic;
using System.Threading.Tasks;

public interface IClientRepository : IRepository<Client>
{
    Task<List<Client>> GetByNumeAsync(string nume);
    Task<List<Client>> GetByEmailAsync(string email);
    Task<List<Client>> GetByTelefonAsync(string telefon);
}


