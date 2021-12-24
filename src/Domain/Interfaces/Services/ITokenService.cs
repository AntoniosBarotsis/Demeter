using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Models.Auth;
using Microsoft.IdentityModel.Tokens;

namespace Domain.Interfaces.Services
{
    public interface ITokenService
    {
        ClaimsPrincipal GetPrincipalFromToken(string token);
        bool IsJwtWithValidSecurityAlgorithm(SecurityToken securityToken);
        Task<AuthenticationResult> GenerateAuthenticationResultAsync(User newUser);
    }
}