using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Interfaces;
using Domain.Models;
using Serilog;
using StackExchange.Redis;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

            if (res.HasValue)
            {
                _logger.Information("Cache [HIT] for {Key}", key);
                _logger.Debug("Result was {Result}", res);
                
                var data = JsonSerializer.Deserialize<List<User>>(res)?[0];
                var username = JsonSerializer.Deserialize<List<User>>(res)?[0].UserName;
                
                _logger.Debug(
                    "User {Username} has {PastOrderCount} past orders",
                    username, 
                    data?.PastOrders?.Count
                );
            }
            else
            {
                _logger.Information("Cache [MISS] for {Key}", key);
            }

            return res.HasValue ? res.ToString() : null;
        }

        public async Task SetCacheValueAsync(ICacheService.Entity entity, ICacheService.Type type, object value)
        {
            var key = CreateKey(entity, type);
            
            _logger.Debug("Serialized object: {Object}", JsonSerializer.Serialize(value));

            await _cache.StringSetAsync(key, JsonSerializer.Serialize(value),
                new TimeSpan(0, 0, 0, 5));
        }

        public string CreateKey(ICacheService.Entity entity, ICacheService.Type type, int? id = null)
        {
            var idPart = id is null ? "" : $":{id}";

            var key = $"{entity.ToString().ToLower()}:" +
                   $"{type.ToString().ToLower()}" +
                   $"{idPart}";
            
            return key;
        }
    }
}