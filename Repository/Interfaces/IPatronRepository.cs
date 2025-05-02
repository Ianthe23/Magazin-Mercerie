using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPatronRepository : IRepository<Patron>
{
    Task<List<Patron>> GetByNumeAsync(string nume);
    Task<List<Patron>> GetByEmailAsync(string email);
    Task<List<Patron>> GetByTelefonAsync(string telefon);
}

