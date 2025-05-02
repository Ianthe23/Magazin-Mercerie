using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAngajatMagazinRepository : IRepository<AngajatMagazin>
{
    Task<List<AngajatMagazin>> GetByTipAsync(TipAngajat tip);
}



