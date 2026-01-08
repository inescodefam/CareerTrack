using CareerTrack.Models;

namespace CareerTrack.Security
{
    public class BadRoleResolver : IRoleResolver
    {
        public string ResolveRole(User user)
        {
            if (String.IsNullOrEmpty(user.Email))
                throw new Exception("Email is required for role resolving");

            if (user.Email.EndsWith("@premium.com"))
                return "PremiumUser";

            return null;
        }
    }
}
