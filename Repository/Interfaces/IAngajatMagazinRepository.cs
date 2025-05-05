using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAngajatMagazinRepository : IUserRepository
{
    Task<List<AngajatMagazin>> GetByTipAsync(TipAngajat tip);
}



