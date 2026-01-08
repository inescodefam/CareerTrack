using CareerTrack.Models;
using System.Drawing;

namespace CareerTrack.Services
{
    public interface IAuthCookieService
    {
        Task SignInAsync(User user, String role);
        Task SignOutAsync();

    }
}
