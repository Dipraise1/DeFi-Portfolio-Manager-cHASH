using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Interfaces;
using DefiPortfolioManager.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DefiPortfolioManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly ICacheService _cacheService;
        private readonly AppSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConnectionMultiplexer _redisConnection;
        
        public HealthController(
            ILogger<HealthController> logger,
            ICacheService cacheService,
            IOptions<AppSettings> options,
            IHttpClientFactory httpClientFactory,
            IConnectionMultiplexer redisConnection = null)
        {
            _logger = logger;
            _cacheService = cacheService;
            _settings = options.Value;
            _httpClientFactory = httpClientFactory;
            _redisConnection = redisConnection;
        }
        
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var healthStatus = new Dictionary<string, string>
            {
                { "Status", "Healthy" },
                { "Timestamp", DateTime.UtcNow.ToString("o") },
                { "CacheType", _settings.Cache.UseRedisCache ? "Redis" : "Memory" }
            };
            
            try
            {
                // Check cache service
                string cacheKey = "health_check";
                await _cacheService.SetAsync(cacheKey, "ok", TimeSpan.FromSeconds(30));
                var cacheValue = await _cacheService.GetAsync<string>(cacheKey);
                healthStatus.Add("Cache", cacheValue == "ok" ? "Operational" : "Failed");
                
                // Check CoinGecko API
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_settings.ExternalApis.CoinGeckoBaseUrl);
                var response = await httpClient.GetAsync("/ping");
                healthStatus.Add("CoinGeckoApi", response.IsSuccessStatusCode ? "Connected" : "Failed");
                
                // Check Redis if enabled
                if (_settings.Cache.UseRedisCache && _redisConnection != null)
                {
                    healthStatus.Add("Redis", _redisConnection.IsConnected ? "Connected" : "Disconnected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                healthStatus["Status"] = "Degraded";
                healthStatus.Add("Error", ex.Message);
            }
            
            return Ok(healthStatus);
        }
        
        [HttpGet("ping")]
        public ActionResult Ping()
        {
            return Ok(new { Status = "OK", Timestamp = DateTime.UtcNow });
        }
    }
} 