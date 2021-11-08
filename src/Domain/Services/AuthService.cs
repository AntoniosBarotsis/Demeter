using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces.Services;
using Domain.Models;
using Domain.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Domain.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly ILogger _logger;
        private readonly UserManager<User> _userManager;

        public AuthService(UserManager<User> userManager, JwtConfig jwtConfig, ILogger logger)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig;
            _logger = logger;
        }

        public async Task<AuthenticationResult> RegisterAsync(string username, string email, string password, bool isOwner = false)
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

            var role = isOwner ? "Owner" : "User";

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
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }
    }
}