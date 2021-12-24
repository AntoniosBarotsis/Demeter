using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Domain.Models.Auth;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Domain.Services
{
    public class TokenService: ITokenService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly ILogger _logger;
        private readonly IAuthRepository _authRepository;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public TokenService(ILogger logger, IAuthRepository authRepository, JwtConfig jwtConfig, TokenValidationParameters tokenValidationParameters)
        {
            _logger = logger;
            _authRepository = authRepository;
            _jwtConfig = jwtConfig;
            _tokenValidationParameters = tokenValidationParameters;
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                _tokenValidationParameters.ValidateLifetime = false;
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
                _tokenValidationParameters.ValidateLifetime = true;

                return !IsJwtWithValidSecurityAlgorithm(validatedToken) ? null : principal;
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                return null;
            }
        }

        public bool IsJwtWithValidSecurityAlgorithm(SecurityToken securityToken)
        {
            return securityToken is JwtSecurityToken jwtSecurityToken &&
                   jwtSecurityToken.Header.Alg
                       .Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
        }

        public async Task<AuthenticationResult> GenerateAuthenticationResultAsync(User newUser)
        {
            var role = newUser.UserType.ToString();
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, newUser.Email),
                    new Claim(JwtRegisteredClaimNames.Email, newUser.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("id", newUser.Id),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddSeconds(_jwtConfig.TokenLifetime.TotalSeconds),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                UserId = newUser.Id,
                CreationDate = DateTime.Now,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };
            
            await _authRepository.AddAsync(refreshToken);
            await _authRepository.SaveChangesAsync();
            
            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken.Token
            };
        }
    }
}