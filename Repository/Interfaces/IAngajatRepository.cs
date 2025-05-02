using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAngajatRepository : IRepository<Angajat>
{
    Task<List<Angajat>> GetBySalariuAsync(int salariu);
    Task<List<Angajat>> GetByNumeAsync(string nume);
    Task<List<Angajat>> GetByEmailAsync(string email);
}


