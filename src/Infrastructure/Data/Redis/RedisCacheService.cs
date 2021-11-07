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

        public async Task<string> GetCacheValueAsync(ICacheService.Entity entity, ICacheService.Type type)
        {
            var key = CreateKey(entity, type);
            var res = await _cache.StringGetAsync(key);

            _logger.Information(res.HasValue ? $"Cache hit for {key}" : $"Cache miss for {key}");

            return res.HasValue ? res.ToString() : null;
        }

        public async Task SetCacheValueAsync(ICacheService.Entity entity, ICacheService.Type type, object value)
        {
            var key = CreateKey(entity, type);

            await _cache.StringSetAsync(key, JsonSerializer.Serialize(value),
                new TimeSpan(0, 0, 0, 5));
        }

        public string CreateKey(ICacheService.Entity entity, ICacheService.Type type, int? id = null)
        {
            var idPart = id is null ? "" : $":{id}";

            return $"{entity.ToString().ToLower()}:" +
                   $"{type.ToString().ToLower()}" +
                   $"{idPart}";
        }
    }
}