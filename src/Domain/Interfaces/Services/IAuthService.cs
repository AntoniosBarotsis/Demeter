using System.Threading.Tasks;
using Domain.Models.Auth;

namespace Domain.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthenticationResult> RegisterAsync(string username, string email, string password);
    }
}