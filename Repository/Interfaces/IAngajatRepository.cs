using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAngajatRepository : IAngajatMagazinRepository
{
    Task<List<Angajat>> GetBySalariuAsync(int salariu);
}


