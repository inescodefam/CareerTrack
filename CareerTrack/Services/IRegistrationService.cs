using CareerTrack.ViewModels;

namespace CareerTrack.Services
{
    public interface IRegistrationService
    {
        Task<AuthResult> RegisterAsync(UserRegisterVM vm);
    }
}
