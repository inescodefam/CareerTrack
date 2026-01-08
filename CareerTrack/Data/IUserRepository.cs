using CareerTrack.Models;

namespace CareerTrack.Data
{
    public interface IUserRepository
    {
        Task<User?> FindByUsernameAsync(string username);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync (string email);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
