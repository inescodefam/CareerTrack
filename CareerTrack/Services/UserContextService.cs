
// SINGLE RESPONSIBILITY PRINCIPLE (SRP) USER CONTEXT HANDLING

namespace CareerTrack.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;

        public UserContextService(IHttpContextAccessor httpContextAccessor, AppDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public int GetCurrentUserId()
        {
            var username = GetCurrentUsername();
            var user = _context.Users.FirstOrDefault(u => u.UserName == username);
            return user?.Id ?? throw new UnauthorizedAccessException("User not found");
        }

        public string GetCurrentUsername()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name
                ?? throw new UnauthorizedAccessException("User not authenticated");
        }
    }
}
