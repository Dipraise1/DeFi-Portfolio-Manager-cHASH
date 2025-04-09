using System;
using System.Text.Json;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Interfaces;
using DefiPortfolioManager.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DefiPortfolioManager.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly TimeSpan _defaultExpiry;
        private readonly ILogger<RedisCacheService> _logger;
        
        public RedisCacheService(
            IConnectionMultiplexer redis, 
            IOptions<AppSettings> options = null, 
            ILogger<RedisCacheService> logger = null)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
            _logger = logger;
            
            var settings = options?.Value ?? new AppSettings();
            _defaultExpiry = TimeSpan.FromMinutes(settings.Cache.DefaultExpiryMinutes);
            
            _logger?.LogInformation(
                "Redis cache initialized with connection to {connectionString}, default expiry of {expiryMinutes} minutes",
                _redis.Configuration,
                settings.Cache.DefaultExpiryMinutes);
        }
        
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            var value = await GetAsync<T>(key);
            
            if (value != null)
            {
                _logger?.LogDebug("Cache hit for key: {key}", key);
                return value;
            }
                
            _logger?.LogDebug("Cache miss for key: {key}, calling factory", key);
            value = await factory();
            await SetAsync(key, value, expiry);
            
            return value;
        }
        
        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            
            if (value.IsNullOrEmpty)
                return default;
                
            try
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                // If deserialization fails, remove the item from cache
                _logger?.LogWarning(ex, "Failed to deserialize Redis cache item with key: {key}", key);
                await _database.KeyDeleteAsync(key);
                return default;
            }
        }
        
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var expiryTime = expiry.HasValue ? expiry.Value : _defaultExpiry;
            
            await _database.StringSetAsync(key, serializedValue, expiryTime);
            _logger?.LogDebug("Added item to Redis cache with key: {key}, expires in: {expiry}", key, expiryTime);
        }
        
        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
            _logger?.LogDebug("Removed item from Redis cache with key: {key}", key);
        }
        
        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }
    }
}