using CareerTrack.Models;

namespace CareerTrack.Security
{
    public class PremiumRoleResolver : IRoleResolver
    {
        public string ResolveRole(User user)
        {
            if (user.IsAdmin ?? false)
                return "Admin";

            if (user.Email.EndsWith("@premium.com"))
                return "PremiumUser";

            return "User";
        }
    }
}
