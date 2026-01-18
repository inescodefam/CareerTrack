using CareerTrack.Models;

namespace CareerTrack.Security
{
    public class PremiumRoleResolver : IRoleResolver
    {
        public string ResolveRole(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            return (user.IsAdmin ?? false) ? "Admin" : "User";
        }
    }
}
