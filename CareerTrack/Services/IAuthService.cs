using CareerTrack.ViewModels;
using Npgsql.Replication.PgOutput.Messages;

namespace CareerTrack.Services
{
    public interface IAuthService
    {
        //ISP narušen
        Task<AuthResult> LoginAsync(UserLoginVM loginVM);
        Task<AuthResult> RegisterAsync(UserRegisterVM registerVM);
        Task LogoutAsync();
        Task RequestPasswordResetAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);
        Task ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task VerifyEmailAsync(string token);
    }
}
