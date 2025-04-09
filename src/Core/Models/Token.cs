namespace DefiPortfolioManager.Core.Models
{
    public class Token
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string ContractAddress { get; set; }
        public Blockchain Blockchain { get; set; }
        public int Decimals { get; set; }
        public string LogoUrl { get; set; }
        public bool IsNative { get; set; }
        
        // Price information
        public decimal CurrentPriceUsd { get; set; }
        public decimal PriceChangePercentage24h { get; set; }
        
        // Token metadata
        public string CoingeckoId { get; set; }
    }
} 