namespace DefiPortfolioManager.Core.Settings
{
    public class AppSettings
    {
        public CacheSettings Cache { get; set; } = new CacheSettings();
        public ExternalApiSettings ExternalApis { get; set; } = new ExternalApiSettings();
        public RedisSettings Redis { get; set; } = new RedisSettings();
    }

    public class CacheSettings
    {
        public int DefaultExpiryMinutes { get; set; } = 5;
        public int PriceExpiryMinutes { get; set; } = 2;
        public int CoinListExpiryHours { get; set; } = 24;
        public bool UseRedisCache { get; set; } = false;
    }

    public class ExternalApiSettings
    {
        public string CoinGeckoBaseUrl { get; set; } = "https://api.coingecko.com/api/v3";
        public string EtherscanBaseUrl { get; set; } = "https://api.etherscan.io/api";
        public string PolygonscanBaseUrl { get; set; } = "https://api.polygonscan.com/api";
        public string BscscanBaseUrl { get; set; } = "https://api.bscscan.com/api";
    }
    
    public class RedisSettings
    {
        public string ConnectionString { get; set; } = "localhost:6379";
        public string InstanceName { get; set; } = "DefiPortfolioManager:";
        public int DatabaseId { get; set; } = 0;
    }
}