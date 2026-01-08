using CareerTrack.Models;
using CareerTrack.ViewModels;

namespace CareerTrack.Services
{
    public interface ILoginService
    {
        Task<AuthResult> LoginAsync(UserLoginVM userLoginVM);
    }
}
