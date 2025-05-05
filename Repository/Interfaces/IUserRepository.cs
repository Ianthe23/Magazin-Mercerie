using System.Threading.Tasks;

// IUserRepository no longer extends IRepository<User> but defines specific User operations
public interface IUserRepository : IBaseRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
}
