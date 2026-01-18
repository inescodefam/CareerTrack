using CareerTrack.Models;

namespace CareerTrack.Security
{
    public class PremiumRoleResolver : IRoleResolver
    {
        public string ResolveRole(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return (user.IsAdmin ?? false) ? "Admin" : "User";
        }
    }
}
