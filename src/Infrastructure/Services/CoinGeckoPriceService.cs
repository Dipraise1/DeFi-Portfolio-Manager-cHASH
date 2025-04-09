using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Interfaces;
using DefiPortfolioManager.Core.Models;
using DefiPortfolioManager.Core.Settings;
using Microsoft.Extensions.Options;

namespace DefiPortfolioManager.Infrastructure.Services
{
    public class CoinGeckoPriceService : IPriceService
    {
        private readonly HttpClient _httpClient;
        private readonly ICacheService _cacheService;
        private readonly AppSettings _settings;
        private readonly TimeSpan _priceCacheExpiry;
        private readonly TimeSpan _coinListCacheExpiry;
        
        public CoinGeckoPriceService(HttpClient httpClient, ICacheService cacheService, IOptions<AppSettings> options = null)
        {
            _httpClient = httpClient;
            _cacheService = cacheService;
            
            // If settings are provided, use them, otherwise use default values
            _settings = options?.Value ?? new AppSettings();
            
            // Initialize cache expiry values from settings
            _priceCacheExpiry = TimeSpan.FromMinutes(_settings.Cache.PriceExpiryMinutes);
            _coinListCacheExpiry = TimeSpan.FromHours(_settings.Cache.CoinListExpiryHours);
            
            // Set the base URL if not already set
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_settings.ExternalApis.CoinGeckoBaseUrl);
            }
        }
        
        public async Task<decimal> GetTokenPriceAsync(string tokenSymbol, string currency = "USD")
        {
            string cacheKey = $"price:{tokenSymbol}:{currency}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () => 
            {
                var coinId = await GetCoinIdFromSymbolAsync(tokenSymbol);
                if (string.IsNullOrEmpty(coinId))
                    return 0;
                    
                var response = await _httpClient.GetAsync($"/simple/price?ids={coinId}&vs_currencies={currency.ToLower()}");
                
                if (!response.IsSuccessStatusCode)
                    return 0;
                    
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Dictionary<string, decimal>>>();
                if (content != null && content.TryGetValue(coinId, out var prices) && 
                    prices.TryGetValue(currency.ToLower(), out var price))
                {
                    return price;
                }
                
                return 0;
            }, _priceCacheExpiry);
        }

        public async Task<decimal> GetTokenPriceAsync(Token token, string currency = "USD")
        {
            string cacheKey = $"price:{token.CoingeckoId ?? token.Symbol}:{currency}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () => 
            {
                if (!string.IsNullOrEmpty(token.CoingeckoId))
                {
                    var response = await _httpClient.GetAsync($"/simple/price?ids={token.CoingeckoId}&vs_currencies={currency.ToLower()}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Dictionary<string, decimal>>>();
                        if (content != null && content.TryGetValue(token.CoingeckoId, out var prices) && 
                            prices.TryGetValue(currency.ToLower(), out var price))
                        {
                            return price;
                        }
                    }
                }
                
                return await GetTokenPriceAsync(token.Symbol, currency);
            }, _priceCacheExpiry);
        }

        public async Task<Dictionary<string, decimal>> GetTokenPricesAsync(List<string> tokenSymbols, string currency = "USD")
        {
            string cacheKey = $"prices:{string.Join(",", tokenSymbols)}:{currency}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () => 
            {
                var result = new Dictionary<string, decimal>();
                var coinIds = new List<string>();
                
                foreach (var symbol in tokenSymbols)
                {
                    var coinId = await GetCoinIdFromSymbolAsync(symbol);
                    if (!string.IsNullOrEmpty(coinId))
                        coinIds.Add(coinId);
                }
                
                if (coinIds.Count == 0)
                    return result;
                    
                var idsParam = string.Join(",", coinIds);
                var response = await _httpClient.GetAsync($"/simple/price?ids={idsParam}&vs_currencies={currency.ToLower()}");
                
                if (!response.IsSuccessStatusCode)
                    return result;
                    
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Dictionary<string, decimal>>>();
                
                if (content == null)
                    return result;
                    
                foreach (var symbol in tokenSymbols)
                {
                    var coinId = await GetCoinIdFromSymbolAsync(symbol);
                    if (!string.IsNullOrEmpty(coinId) && 
                        content.TryGetValue(coinId, out var prices) && 
                        prices.TryGetValue(currency.ToLower(), out var price))
                    {
                        result.Add(symbol, price);
                    }
                    else
                    {
                        result.Add(symbol, 0);
                    }
                }
                
                return result;
            }, _priceCacheExpiry);
        }

        public async Task<Dictionary<Token, decimal>> GetTokenPricesAsync(List<Token> tokens, string currency = "USD")
        {
            // Create a compound cache key with token ids
            var tokenIds = tokens.Select(t => t.CoingeckoId ?? t.Symbol).ToList();
            string cacheKey = $"tokenPrices:{string.Join(",", tokenIds)}:{currency}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () => 
            {
                var result = new Dictionary<Token, decimal>();
                var coingeckoIds = tokens
                    .Where(t => !string.IsNullOrEmpty(t.CoingeckoId))
                    .Select(t => t.CoingeckoId)
                    .ToList();
                    
                if (coingeckoIds.Count == 0)
                    return result;
                    
                var idsParam = string.Join(",", coingeckoIds);
                var response = await _httpClient.GetAsync($"/simple/price?ids={idsParam}&vs_currencies={currency.ToLower()}");
                
                if (!response.IsSuccessStatusCode)
                    return result;
                    
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Dictionary<string, decimal>>>();
                
                if (content == null)
                    return result;
                    
                foreach (var token in tokens)
                {
                    if (!string.IsNullOrEmpty(token.CoingeckoId) && 
                        content.TryGetValue(token.CoingeckoId, out var prices) && 
                        prices.TryGetValue(currency.ToLower(), out var price))
                    {
                        result.Add(token, price);
                    }
                    else
                    {
                        var tokenPrice = await GetTokenPriceAsync(token.Symbol, currency);
                        result.Add(token, tokenPrice);
                    }
                }
                
                return result;
            }, _priceCacheExpiry);
        }

        public async Task<decimal> GetPriceChangePercentageAsync(string tokenSymbol, int days = 1)
        {
            string cacheKey = $"priceChange:{tokenSymbol}:{days}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () => 
            {
                var coinId = await GetCoinIdFromSymbolAsync(tokenSymbol);
                if (string.IsNullOrEmpty(coinId))
                    return 0;
                    
                var response = await _httpClient.GetAsync($"/coins/{coinId}/market_chart?vs_currency=usd&days={days}");
                
                if (!response.IsSuccessStatusCode)
                    return 0;
                    
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
                
                if (content == null || !content.TryGetValue("prices", out var priceData))
                    return 0;
                    
                List<List<double>> prices = new List<List<double>>();
                foreach (var item in priceData.EnumerateArray())
                {
                    var pricePoint = new List<double>();
                    foreach (var value in item.EnumerateArray())
                    {
                        pricePoint.Add(value.GetDouble());
                    }
                    prices.Add(pricePoint);
                }
                
                if (prices.Count < 2)
                    return 0;
                    
                var initialPrice = prices.First()[1];
                var currentPrice = prices.Last()[1];
                
                return (decimal)(((currentPrice - initialPrice) / initialPrice) * 100);
            }, _priceCacheExpiry);
        }
        
        private async Task<string> GetCoinIdFromSymbolAsync(string symbol)
        {
            // Cache the entire coin list to minimize API calls
            string cacheKey = "coinGeckoList";
            
            var coins = await _cacheService.GetOrSetAsync<List<CoinListItem>>(cacheKey, async () => 
            {
                var response = await _httpClient.GetAsync("/coins/list");
                if (!response.IsSuccessStatusCode)
                    return new List<CoinListItem>();
                    
                return await response.Content.ReadFromJsonAsync<List<CoinListItem>>() ?? new List<CoinListItem>();
            }, _coinListCacheExpiry);
            
            // Also cache individual symbol lookups
            string symbolCacheKey = $"coinId:{symbol.ToLower()}";
            
            return await _cacheService.GetOrSetAsync<string>(symbolCacheKey, async () => 
            {
                var matchingCoin = coins?.FirstOrDefault(c => 
                    c.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
                    
                return matchingCoin?.Id ?? string.Empty;
            }, _coinListCacheExpiry);
        }
        
        private class CoinListItem
        {
            public string Id { get; set; }
            public string Symbol { get; set; }
            public string Name { get; set; }
        }
    }
} 