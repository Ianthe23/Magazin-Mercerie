using System.Collections.Generic;
using System.Threading.Tasks;

public interface IProdusRepository : IRepository<Produs>
{
    Task<List<Produs>> GetByNumeAsync(string nume);
    Task<List<Produs>> GetByTipAsync(TipProdus tip);
    Task<List<Produs>> GetByPretAsync(decimal pret);
    Task<List<Produs>> GetByCantitateAsync(decimal cantitate);
}

