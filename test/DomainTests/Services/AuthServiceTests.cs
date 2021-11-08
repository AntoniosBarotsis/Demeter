using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Domain.Interfaces;
using Domain.Interfaces.Services;
using Domain.Models;
using Domain.Models.Auth;
using Domain.Services;
using FluentAssertions;
using Infrastructure.DTOs.Request;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserRegistrationRequest _user;
        private readonly IUserManager _userManager = Substitute.For<IUserManager>();

        public AuthServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _jwtConfig = new JwtConfig
            {
                Secret = "verysecretkeythatislongerthan32chars",
                TokenLifetime = TimeSpan.Parse("00:00:10")
            };

            _user = _fixture
                .Create<UserRegistrationRequest>();

            _sut = new AuthService(_userManager, _jwtConfig, _logger);
        }

        [Fact]
        public async Task RegisterUserTest()
        {
            _userManager.FindByEmailAsync(_user.Email)
                .ReturnsNull();

            _userManager.CreateAsync(Arg.Any<User>(), _user.Password)
                .Returns(IdentityResult.Success);

            var res = await _sut.RegisterAsync(_user.UserName, _user.Email, _user.Password);

            res.Should().NotBeNull();
            res.Errors.Should().BeNull();
            res.Success.Should().BeTrue();
        }

        [Fact]
        public async Task RegisterExistingUserTest()
        {
            _userManager.FindByEmailAsync(_user.Email)
                .Returns(Task.FromResult(new User(_user.UserName, _user.Password)));

            var res = await _sut.RegisterAsync(_user.UserName, _user.Email, _user.Password);

            res.Should().NotBeNull();
            res.Errors.Should().NotBeNull();
            res.Errors.Count().Should().Be(1);
            res.Errors.FirstOrDefault().Should().Be("User with this email already exists");
            res.Success.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterUserFailedTest()
        {
            _userManager.FindByEmailAsync(_user.Email)
                .ReturnsNull();

            _userManager.CreateAsync(Arg.Any<User>(), _user.Password)
                .Returns(new IdentityResult());

            var res = await _sut.RegisterAsync(_user.UserName, _user.Email, _user.Password);

            res.Should().NotBeNull();
            res.Errors.Should().NotBeNull();
            res.Success.Should().BeFalse();
        }
    }
}