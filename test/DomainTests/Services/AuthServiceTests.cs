using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Domain.Models.Auth;
using Domain.Services;
using FluentAssertions;
using Infrastructure.DTOs.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace DomainTests.Services
{
    public class AuthServiceTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly JwtConfig _jwtConfig;

        private readonly ILogger _logger =
            new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

        private readonly IAuthService _sut;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly UserRegistrationRequest _userRegistrationRequest;
        private readonly User _user;
        private readonly IUserManager _userManager = Substitute.For<IUserManager>();
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly IAuthRepository _authRepository = Substitute.For<IAuthRepository>();
        private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
        // private readonly ITokenService _tokenService;

        public AuthServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _jwtConfig = new JwtConfig
            {
                Secret = "verysecretkeythatislongerthan32chars",
                TokenLifetime = TimeSpan.Parse("00:00:10")
            };

            _userRegistrationRequest = _fixture
                .Create<UserRegistrationRequest>();

            _user = _fixture
                .Build<User>()
                .With(el => el.Email, _userRegistrationRequest.Email)
                .Create();

            // _tokenService = new TokenService(_logger, _authRepository, _jwtConfig, _tokenValidationParameters);

            _sut = new AuthService(_userManager, _jwtConfig, _logger, _tokenValidationParameters, _authRepository, _tokenService);
        }

        private ClaimsPrincipal GetClaimsPrincipal(int mins = 10)
        {
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            var time = DateTime.Now.AddMinutes(mins) - DateTime.Now;
            var date = time.Ticks;
            claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Exp, date.ToString()));
            var claimsPrincipal = new ClaimsPrincipal();
            claimsIdentity.AddClaim(new Claim("id", "userId"));
            claimsPrincipal.AddIdentity(claimsIdentity);

            return claimsPrincipal;
        }

        [Fact]
        public async Task RegisterUserTest()
        {
            _userManager.FindByEmailAsync(_userRegistrationRequest.Email)
                .ReturnsNull();

            _userManager.CreateAsync(Arg.Any<User>(), _userRegistrationRequest.Password)
                .Returns(IdentityResult.Success);

            var res = await _sut.RegisterAsync(_userRegistrationRequest.UserName, _userRegistrationRequest.Email, _userRegistrationRequest.Password);

            await _tokenService.Received().GenerateAuthenticationResultAsync(Arg.Any<User>());
        }

        [Fact]
        public async Task RegisterExistingUserTest()
        {
            _userManager.FindByEmailAsync(_userRegistrationRequest.Email)
                .Returns(new User(_userRegistrationRequest.UserName, _userRegistrationRequest.Email));

            var res = await _sut.RegisterAsync(_userRegistrationRequest.UserName, _userRegistrationRequest.Email, _userRegistrationRequest.Password);

            res.Should().NotBeNull();
            res.Errors.Should().NotBeNull();
            res.Errors.Count().Should().Be(1);
            res.Errors.FirstOrDefault().Should().Be("User with this email already exists");
            res.Success.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterUserFailedTest()
        {
            _userManager.FindByEmailAsync(_userRegistrationRequest.Email)
                .ReturnsNull();

            _userManager.CreateAsync(Arg.Any<User>(), _userRegistrationRequest.Password)
                .Returns(new IdentityResult());

            var res = await _sut.RegisterAsync(_userRegistrationRequest.UserName, _userRegistrationRequest.Email, _userRegistrationRequest.Password);

            res.Should().NotBeNull();
            res.Errors.Should().NotBeNull();
            res.Success.Should().BeFalse();
        }

        [Fact]
        public async Task LoginUser()
        {
            _userManager.FindByEmailAsync(_userRegistrationRequest.Email)
                .Returns(Task.FromResult(_user));

            _userManager.CheckPasswordAsync(_user, _userRegistrationRequest.Password)
                .Returns(Task.FromResult(true));
            
            await _sut.LoginAsync(_userRegistrationRequest.Email, _userRegistrationRequest.Password);

            await _tokenService.Received().GenerateAuthenticationResultAsync(Arg.Any<User>());
        }

        [Fact]
        public async Task LoginUserDoesNotExist()
        {
            _userManager.FindByEmailAsync(_userRegistrationRequest.Email)
                .ReturnsNull();

            var res = await _sut.LoginAsync(_userRegistrationRequest.Email, _userRegistrationRequest.Password);
            
            res.Should().NotBeNull();
            res.Errors.Should().NotBeNull();
            res.Errors.Count().Should().Be(1);
            res.Errors.FirstOrDefault().Should().Be("User does not exist");
            res.Success.Should().BeFalse();
        }

        [Fact]
        public async Task LoginUserWrongPassword()
        {
            _userManager.FindByEmailAsync(_userRegistrationRequest.Email)
                .Returns(Task.FromResult(_user));
            
            _userManager.CheckPasswordAsync(_user, _userRegistrationRequest.Password)
                .Returns(Task.FromResult(false));

            var res = await _sut.LoginAsync(_userRegistrationRequest.Email, _userRegistrationRequest.Password);
            
            res.Should().NotBeNull();
            res.Errors.Should().NotBeNull();
            res.Errors.Count().Should().Be(1);
            res.Errors.FirstOrDefault().Should().Be("User/password combination is wrong");
            res.Success.Should().BeFalse();
        }

        [Fact]
        public async Task RefreshToken()
        {
            var claims = GetClaimsPrincipal();
            var tokenId = claims.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
            
            _tokenService.GetPrincipalFromToken("token")
                .Returns(claims);
            
            var refreshToken = new RefreshToken
            {
                Invalidated = false,
                ExpiryDate = DateTime.Now.AddMinutes(10),
                JwtId = tokenId
            };

            _authRepository.GetRefreshTokenAsync("refreshToken")
                .Returns(refreshToken);

            _userManager.FindByIdAsync(Arg.Any<string>())
                .Returns(_user);

            await _sut.RefreshTokenAsync("token", "refreshToken");
            await _tokenService.Received().GenerateAuthenticationResultAsync(_user);
        }

        [Fact]
        public async Task RefreshInvalidToken()
        {
            _tokenService.GetPrincipalFromToken("token")
                .ReturnsNull();
            
            var res = await _sut.RefreshTokenAsync("token", "refreshToken");

            res.Should().NotBeNull();
            res.Success.Should().BeFalse();
            res.Errors.Should().NotBeNull();
            res.Errors.Count().Should().Be(1);
            res.Errors.FirstOrDefault().Should().Be("Invalid token");
            res.Token.Should().BeNull();
        }

        [Fact]
        public async Task RefreshExpiredToken()
        {
            _authRepository.GetRefreshTokenAsync(Arg.Any<string>())
                .Returns(Task.FromResult(new RefreshToken()));
            
            _tokenService.GetPrincipalFromToken("token")
                .Returns(GetClaimsPrincipal(-10));
            
            var res = await _sut.RefreshTokenAsync("token", "refreshToken");

            res.Should().NotBeNull();
            res.Success.Should().BeFalse();
            res.Errors.Should().NotBeNull();
            res.Errors.Count().Should().Be(1);
            res.Errors.FirstOrDefault().Should().Be("This refresh token has expired");
            res.Token.Should().BeNull();
        }
        
        [Fact]
        public async Task RefreshInvalidatedToken()
        {
            var token = new RefreshToken
            {
                Invalidated = true,
                ExpiryDate = DateTime.Now.AddMinutes(10)
            };

            _authRepository.GetRefreshTokenAsync(Arg.Any<string>())
                .Returns(Task.FromResult(token));
            
            _tokenService.GetPrincipalFromToken("token")
                .Returns(GetClaimsPrincipal());
            
            var res = await _sut.RefreshTokenAsync("token", "refreshToken");

            res.Should().NotBeNull();
            res.Success.Should().BeFalse();
            res.Errors.Should().NotBeNull();
            res.Errors.Count().Should().Be(1);
            res.Errors.FirstOrDefault().Should().Be("This refresh token has been invalidated");
            res.Token.Should().BeNull();
        }
        
        [Fact]
        public async Task RefreshUsedToken()
        {
            var token = new RefreshToken
            {
                Invalidated = false,
                Used = true,
                ExpiryDate = DateTime.Now.AddMinutes(10)
            };

            _authRepository.GetRefreshTokenAsync(Arg.Any<string>())
                .Returns(Task.FromResult(token));
            
            _tokenService.GetPrincipalFromToken("token")
                .Returns(GetClaimsPrincipal());
            
            var res = await _sut.RefreshTokenAsync("token", "refreshToken");

            res.Should().NotBeNull();
            res.Success.Should().BeFalse();
            res.Errors.Should().NotBeNull();
            res.Errors.Count().Should().Be(1);
            res.Errors.FirstOrDefault().Should().Be("This token has been used");
            res.Token.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenDoesNotExist()
        {
            _authRepository.GetRefreshTokenAsync(Arg.Any<string>())
                .ReturnsNull();
            
            _tokenService.GetPrincipalFromToken("token")
                .Returns(GetClaimsPrincipal(-10));
            
            var res = await _sut.RefreshTokenAsync("token", "refreshToken");

            res.Should().NotBeNull();
            res.Success.Should().BeFalse();
            res.Errors.Should().NotBeNull();
            res.Errors.Count().Should().Be(1);
            res.Errors.FirstOrDefault().Should().Be("This refresh token does not exist");
            res.Token.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenIdDoesNotMatchTokenId()
        {
            var claims = GetClaimsPrincipal();
            var tokenId = claims.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
            
            _tokenService.GetPrincipalFromToken("token")
                .Returns(claims);
            
            var refreshToken = new RefreshToken
            {
                Invalidated = false,
                ExpiryDate = DateTime.Now.AddMinutes(10),
                JwtId = tokenId + "1"
            };

            _authRepository.GetRefreshTokenAsync("refreshToken")
                .Returns(refreshToken);

            _userManager.FindByIdAsync(Arg.Any<string>())
                .Returns(_user);

            var res = await _sut.RefreshTokenAsync("token", "refreshToken");
            
            res.Should().NotBeNull();
            res.Success.Should().BeFalse();
            res.Errors.Should().NotBeNull();
            res.Errors.Count().Should().Be(1);
            res.Errors.FirstOrDefault().Should().Be("This refresh token does not match this JWT");
            res.Token.Should().BeNull();
        }
    }
}