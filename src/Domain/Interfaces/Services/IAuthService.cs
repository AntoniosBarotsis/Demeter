using System.Threading.Tasks;
using Domain.Models;
using Domain.Models.Auth;

namespace Domain.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthenticationResult> RegisterAsync(string username, string email, string password,
            UserType userType = UserType.User);

        Task<AuthenticationResult> LoginAsync(string email, string password);

        Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken);
    }
}