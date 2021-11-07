using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;

namespace Domain.Services
{
    public class UserService : IUserService
    {
        private readonly ICacheService _cache;
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository, ICacheService cache)
        {
            _userRepository = userRepository;
            _cache = cache;
        }

        public async Task<List<User>> FindAll(CancellationToken cancellationToken)
        {
            var cachedRes = await _cache.GetCacheValueAsync("users:all");

            if (cachedRes is not null) return JsonSerializer.Deserialize<List<User>>(cachedRes);

            var res = await _userRepository.FindAll(cancellationToken);

            await _cache.SetCacheValueAsync("users:all", res);

            return res;
        }
    }
}