using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Domain.Models.Auth;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Domain.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly ILogger _logger;
        private readonly IUserManager _userManager;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly IAuthRepository _authRepository;

        public AuthService(IUserManager userManager, JwtConfig jwtConfig, ILogger logger, TokenValidationParameters tokenValidationParameters, IAuthRepository authRepository)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig;
            _logger = logger;
            _tokenValidationParameters = tokenValidationParameters;
            _authRepository = authRepository;
        }

        public async Task<AuthenticationResult> RegisterAsync(string username, string email, string password,
            UserType userType = UserType.User)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser is not null)
                return new AuthenticationResult
                {
                    Errors = new List<string> { "User with this email already exists" }
                };

            var newUser = new User(username, email);

            var createdUser = await _userManager.CreateAsync(newUser, password);

            if (!createdUser.Succeeded)
                return new AuthenticationResult
                {
                    Errors = createdUser.Errors.Select(x => x.Description)
                };

            return await GenerateAuthenticationResultAsync(newUser);
        }
        
        public async Task<AuthenticationResult> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "User does not exist" }
                };
            }

            var userHasValidPassword = await _userManager.CheckPasswordAsync(user, password);

            if (!userHasValidPassword)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "User/password combination is wrong" }
                };
            }

            return await GenerateAuthenticationResultAsync(user);
        }

        public async Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken)
        {
            var validatedToken = GetPrincipalFromToken(token);

            if (validatedToken is null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "Invalid token" }
                };
            }

            var expiryDateUnix =
                long.Parse(
                    validatedToken
                        .Claims
                        .Single(x => x.Type == JwtRegisteredClaimNames.Exp)
                        .Value
                    );
            var expiryDateUtc = DateTime.UnixEpoch.AddSeconds(expiryDateUnix);
            
            if (expiryDateUtc > DateTime.Now)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "This token hasn't expired yet" }
                };
            }

            var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var storedRefreshToken = await _authRepository.GetRefreshTokenAsync(refreshToken);

            if (storedRefreshToken is null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "This refresh token does not exist" }
                };
            }

            if (DateTime.Now > storedRefreshToken.ExpiryDate)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "This refresh token has expired" }
                };
            }

            if (storedRefreshToken.Invalidated)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "This refresh token has been invalidated" }
                };
            }

            if (storedRefreshToken.Used)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "This token has been used" }
                };
            }

            if (storedRefreshToken.JwtId != jti)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "This refresh token does not match this JWT" }
                };
            }

            storedRefreshToken.Used = true;
            _authRepository.Update(storedRefreshToken);
            await _authRepository.SaveChangesAsync();

            var userId = validatedToken
                .Claims
                .Single(x => x.Type == "id")
                .Value;

            var user = await _userManager.FindByIdAsync(userId);

            return await GenerateAuthenticationResultAsync(user);
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
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

        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken securityToken)
        {
            return securityToken is JwtSecurityToken jwtSecurityToken &&
                   jwtSecurityToken.Header.Alg
                       .Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
        }

        private async Task<AuthenticationResult> GenerateAuthenticationResultAsync(User newUser)
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