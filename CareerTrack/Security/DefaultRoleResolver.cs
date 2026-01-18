using CareerTrack.Models;

namespace CareerTrack.Security
{
    public class DefaultRoleResolver : IRoleResolver
    {
        public string ResolveRole(User user)
        {
            return (user.IsAdmin ?? false) ? "Admin" : "User";
        }
    }
}
