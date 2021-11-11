using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Domain.Services;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;
using Xunit.Abstractions;

namespace DomainTests.Services
{
    public class UserServiceTests
    {
        private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
        private readonly CancellationToken _cancellationToken = new();
        private readonly IFixture _fixture = new Fixture();
        private readonly IUserService _sut;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly User _user1;
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

        public UserServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _sut = new UserService(_userRepository, _cacheService);
            _user1 = _fixture.Create<User>();
        }

        [Fact]
        public async Task FindAllUsersTest_CacheHit()
        {
            _cacheService
                .GetCacheValueAsync(ICacheService.Entity.Users, ICacheService.Type.FindAll)
                .Returns(JsonSerializer.Serialize(new List<User> { _user1 }));

            var res = await _sut.FindAll(_cancellationToken);
            
            res.Should().NotBeNull();
            res.FirstOrDefault().Should().BeEquivalentTo(_user1);
            await _cacheService.Received(1).GetCacheValueAsync(ICacheService.Entity.Users, ICacheService.Type.FindAll);
        }

        [Fact]
        public async Task FindAllUsersTest_CacheMiss()
        {
            _cacheService
                .GetCacheValueAsync(ICacheService.Entity.Users, ICacheService.Type.FindAll)
                .ReturnsNull();

            _userRepository
                .FindAll(_cancellationToken)
                .Returns(new List<User> { _user1 });

            var res = await _sut.FindAll(_cancellationToken);

            res.Should().NotBeNull();
            res.FirstOrDefault().Should().BeEquivalentTo(_user1);

            await _userRepository.Received(1).FindAll(_cancellationToken);
            await _cacheService.Received(1)
                .SetCacheValueAsync(ICacheService.Entity.Users, ICacheService.Type.FindAll, res);
            await _cacheService.Received(1).GetCacheValueAsync(ICacheService.Entity.Users, ICacheService.Type.FindAll);
        }
    }
}