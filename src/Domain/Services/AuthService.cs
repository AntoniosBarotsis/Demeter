using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
        private readonly ITokenService _tokenService;

        public AuthService(IUserManager userManager, JwtConfig jwtConfig, ILogger logger, TokenValidationParameters tokenValidationParameters, IAuthRepository authRepository, ITokenService tokenService)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig;
            _logger = logger;
            _tokenValidationParameters = tokenValidationParameters;
            _authRepository = authRepository;
            _tokenService = tokenService;
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

            return await _tokenService.GenerateAuthenticationResultAsync(newUser);
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

            return await _tokenService.GenerateAuthenticationResultAsync(user);
        }

        public async Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken)
        {
            var validatedToken = _tokenService.GetPrincipalFromToken(token);

            if (validatedToken is null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "Invalid token" }
                };
            }

            // var expiryDateUnix =
            //     long.Parse(
            //         validatedToken
            //             .Claims
            //             .Single(x => x.Type == JwtRegisteredClaimNames.Exp)
            //             .Value
            //         );
            // var expiryDateUtc = DateTime.UnixEpoch.AddSeconds(expiryDateUnix);
            //
            // if (expiryDateUtc > DateTime.Now)
            // {
            //     return new AuthenticationResult
            //     {
            //         Errors = new[] { "This token hasn't expired yet" }
            //     };
            // }

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

            return await _tokenService.GenerateAuthenticationResultAsync(user);
        }
    }
}