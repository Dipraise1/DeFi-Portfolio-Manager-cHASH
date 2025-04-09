using System;
using DefiPortfolioManager.Core.Interfaces;
using DefiPortfolioManager.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DefiPortfolioManager.Infrastructure.Services
{
    public class CacheFactory
    {
        private readonly AppSettings _settings;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ILogger<CacheFactory> _logger;
        private readonly ILoggerFactory _loggerFactory;
        
        public CacheFactory(
            IOptions<AppSettings> options, 
            ILoggerFactory loggerFactory,
            IConnectionMultiplexer redisConnection = null)
        {
            _settings = options.Value;
            _redisConnection = redisConnection;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<CacheFactory>();
        }
        
        public ICacheService CreateCacheService()
        {
            if (_settings.Cache.UseRedisCache && _redisConnection != null)
            {
                _logger.LogInformation("Creating Redis cache service");
                return new RedisCacheService(
                    _redisConnection, 
                    Options.Create(_settings),
                    _loggerFactory.CreateLogger<RedisCacheService>());
            }
            
            _logger.LogInformation("Creating memory cache service");
            return new MemoryCacheService(
                Options.Create(_settings),
                _loggerFactory.CreateLogger<MemoryCacheService>());
        }
    }
} 