using System;
using System.Linq;
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

            _sut = new AuthService(_userManager, _jwtConfig, _logger, _tokenValidationParameters, _authRepository);
        }

        [Fact]
        public async Task RegisterUserTest()
        {
            _userManager.FindByEmailAsync(_userRegistrationRequest.Email)
                .ReturnsNull();

            _userManager.CreateAsync(Arg.Any<User>(), _userRegistrationRequest.Password)
                .Returns(IdentityResult.Success);

            var res = await _sut.RegisterAsync(_userRegistrationRequest.UserName, _userRegistrationRequest.Email, _userRegistrationRequest.Password);

            res.Should().NotBeNull();
            res.Errors.Should().BeNull();
            res.Success.Should().BeTrue();
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
            
            var res = await _sut.LoginAsync(_userRegistrationRequest.Email, _userRegistrationRequest.Password);

            res.Should().NotBeNull();
            res.Errors.Should().BeNull();
            res.Success.Should().BeTrue();
            res.Token.Should().NotBeNull();
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
            var res = await _sut.RefreshTokenAsync("token", "refreshToken");
            _testOutputHelper.WriteLine("a");
        }

        [Fact]
        public async Task RefreshInvalidToken()
        {
            var res = await _sut.RefreshTokenAsync("token", "refreshToken");

            res.Should().NotBeNull();
            res.Success.Should().BeFalse();
            res.Errors.Should().NotBeNull();
            res.Errors.Count().Should().Be(1);
            res.Errors.FirstOrDefault().Should().Be("Invalid token");
            res.Token.Should().BeNull();
        }
    }
}