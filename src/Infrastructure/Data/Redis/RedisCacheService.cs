using System;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Interfaces;
using Serilog;
using StackExchange.Redis;

namespace Infrastructure.Data.Redis
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _cache;
        private readonly ILogger _logger;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger logger)
        {
            _logger = logger;
            _cache = connectionMultiplexer.GetDatabase();
        }

        public async Task<string> GetCacheValueAsync(string key)
        {
            var res = await _cache.StringGetAsync(key);

            _logger.Information(res.HasValue ? $"Cache hit for {key}" : $"Cache miss for {key}");

            return res.HasValue ? res.ToString() : null;
        }

        public async Task SetCacheValueAsync(string key, object value)
        {
            await _cache.StringSetAsync(key, JsonSerializer.Serialize(value),
                new TimeSpan(0, 0, 0, 5));
        }
    }
}