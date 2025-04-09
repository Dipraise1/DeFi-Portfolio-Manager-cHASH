using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Interfaces;
using DefiPortfolioManager.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DefiPortfolioManager.Infrastructure.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();
        private readonly TimeSpan _defaultExpiry;
        private readonly ILogger<MemoryCacheService> _logger;

        private class CacheItem
        {
            public string Value { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
        
        public MemoryCacheService(IOptions<AppSettings> options = null, ILogger<MemoryCacheService> logger = null)
        {
            var settings = options?.Value ?? new AppSettings();
            _defaultExpiry = TimeSpan.FromMinutes(settings.Cache.DefaultExpiryMinutes);
            _logger = logger;
            
            _logger?.LogInformation("Memory cache initialized with default expiry of {expiryMinutes} minutes", settings.Cache.DefaultExpiryMinutes);
        }

        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            return GetAsync<T>(key).ContinueWith(task =>
            {
                if (task.Result != null && !task.IsFaulted)
                {
                    _logger?.LogDebug("Cache hit for key: {key}", key);
                    return Task.FromResult(task.Result);
                }

                _logger?.LogDebug("Cache miss for key: {key}, calling factory", key);
                return factory().ContinueWith(factoryTask =>
                {
                    var result = factoryTask.Result;
                    SetAsync(key, result, expiry);
                    return result;
                });
            }).Unwrap();
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (_cache.TryGetValue(key, out var cacheItem))
            {
                if (DateTime.UtcNow < cacheItem.ExpiresAt)
                {
                    try
                    {
                        var value = JsonSerializer.Deserialize<T>(cacheItem.Value);
                        return Task.FromResult(value);
                    }
                    catch (Exception ex)
                    {
                        // If deserialization fails, remove the item from cache
                        _logger?.LogWarning(ex, "Failed to deserialize cache item with key: {key}", key);
                        _cache.TryRemove(key, out _);
                    }
                }
                else
                {
                    // Remove expired items
                    _logger?.LogDebug("Removing expired cache item with key: {key}", key);
                    _cache.TryRemove(key, out _);
                }
            }

            return Task.FromResult<T>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var expiryTime = expiry.HasValue ? expiry.Value : _defaultExpiry;
            var serializedValue = JsonSerializer.Serialize(value);

            var cacheItem = new CacheItem
            {
                Value = serializedValue,
                ExpiresAt = DateTime.UtcNow.Add(expiryTime)
            };

            _cache[key] = cacheItem;
            _logger?.LogDebug("Added item to cache with key: {key}, expires at: {expiry}", key, cacheItem.ExpiresAt);
            
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _cache.TryRemove(key, out _);
            _logger?.LogDebug("Removed item from cache with key: {key}", key);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            if (_cache.TryGetValue(key, out var cacheItem))
            {
                if (DateTime.UtcNow < cacheItem.ExpiresAt)
                {
                    return Task.FromResult(true);
                }
                
                // Remove expired items
                _logger?.LogDebug("Removing expired cache item during exists check with key: {key}", key);
                _cache.TryRemove(key, out _);
            }

            return Task.FromResult(false);
        }
    }
} 