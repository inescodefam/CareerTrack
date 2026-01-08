using CareerTrack.Models;

namespace CareerTrack.Security
{
    public interface IRoleResolver
    {
        string ResolveRole(User user);
    }
}
