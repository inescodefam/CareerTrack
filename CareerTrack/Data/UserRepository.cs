using CareerTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(User user)
        {
            _context.Users.Add(user);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByEmailAsync(string email)
        {
            return _context.Users.AnyAsync(u => u.Email == email);
        }

        public Task<bool> ExistsByUsernameAsync(string username)
        {
            return _context.Users.AnyAsync(u => u.UserName == username);
        }

        public Task<User?> FindByUsernameAsync(string username)
        {
            return _context.Users.
                FirstOrDefaultAsync(u => u.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase));
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
