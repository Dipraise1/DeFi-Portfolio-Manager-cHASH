using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DefiPortfolioManager.Core.Models
{
    /// <summary>
    /// Represents a cryptocurrency token or coin with its properties and price information
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Unique identifier for the token (e.g., BTC, ETH, LINK)
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Symbol { get; set; }

        /// <summary>
        /// Full name of the token (e.g., Bitcoin, Ethereum, Chainlink)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Contract address of the token (null for native coins like ETH)
        /// </summary>
        [StringLength(66)]
        public string ContractAddress { get; set; }

        /// <summary>
        /// Blockchain where this token exists
        /// </summary>
        [Required]
        public Blockchain Blockchain { get; set; }

        /// <summary>
        /// Number of decimal places for the token (e.g., 18 for ETH, 6 for USDC)
        /// </summary>
        [Range(0, 36)]
        public int Decimals { get; set; }

        /// <summary>
        /// URL to the token's logo image
        /// </summary>
        [Url]
        public string LogoUrl { get; set; }

        /// <summary>
        /// Indicates if this is a native token of the blockchain (e.g., ETH for Ethereum)
        /// </summary>
        public bool IsNative { get; set; }
        
        /// <summary>
        /// Current price in USD
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CurrentPriceUsd { get; set; }

        /// <summary>
        /// 24-hour price change percentage
        /// </summary>
        public decimal PriceChangePercentage24h { get; set; }
        
        /// <summary>
        /// 7-day price change percentage
        /// </summary>
        public decimal PriceChangePercentage7d { get; set; }
        
        /// <summary>
        /// All-time high price in USD
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? AllTimeHighUsd { get; set; }
        
        /// <summary>
        /// Date when the all-time high was reached
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? AllTimeHighDate { get; set; }
        
        /// <summary>
        /// Market capitalization in USD
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? MarketCapUsd { get; set; }
        
        /// <summary>
        /// 24-hour trading volume in USD
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Volume24hUsd { get; set; }

        /// <summary>
        /// Identifier used for CoinGecko API
        /// </summary>
        public string CoingeckoId { get; set; }
        
        /// <summary>
        /// Last time the price data was updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Creates a token with the specified symbol and name
        /// </summary>
        public Token(string symbol, string name)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        
        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public Token() { }
        
        /// <summary>
        /// Returns a string that represents the current token
        /// </summary>
        public override string ToString() => $"{Symbol} ({Name})";
    }
} 