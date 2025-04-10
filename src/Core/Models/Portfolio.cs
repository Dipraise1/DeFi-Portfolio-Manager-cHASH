using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace DefiPortfolioManager.Core.Models
{
    /// <summary>
    /// Represents a user's DeFi portfolio across multiple blockchains
    /// </summary>
    public class Portfolio
    {
        /// <summary>
        /// Wallet address of the portfolio owner
        /// </summary>
        [Required]
        [StringLength(66)]
        public string WalletAddress { get; set; }

        /// <summary>
        /// List of token balances in the portfolio
        /// </summary>
        public List<TokenBalance> TokenBalances { get; set; } = new List<TokenBalance>();

        /// <summary>
        /// List of yield positions in the portfolio
        /// </summary>
        public List<YieldPosition> YieldPositions { get; set; } = new List<YieldPosition>();
        
        /// <summary>
        /// Historical portfolio values over time
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Dictionary<DateTime, decimal> HistoricalValues { get; set; } = new Dictionary<DateTime, decimal>();
        
        // Calculated properties
        
        /// <summary>
        /// Total value of all tokens in USD
        /// </summary>
        public decimal TotalTokenValueUsd => TokenBalances.Sum(t => t.BalanceInUsd);
        
        /// <summary>
        /// Total value of all yield positions in USD
        /// </summary>
        public decimal TotalYieldValueUsd => YieldPositions.Sum(y => y.TotalValueUsd);
        
        /// <summary>
        /// Total value of the entire portfolio in USD
        /// </summary>
        public decimal TotalPortfolioValueUsd => TotalTokenValueUsd + TotalYieldValueUsd;
        
        /// <summary>
        /// Estimated daily yield from all positions in USD
        /// </summary>
        public decimal EstimatedDailyYieldUsd => YieldPositions.Sum(y => y.DailyYieldUsd);
        
        /// <summary>
        /// Estimated annual yield from all positions in USD
        /// </summary>
        public decimal EstimatedAnnualYieldUsd => EstimatedDailyYieldUsd * 365;
        
        /// <summary>
        /// Average APY across the entire portfolio (weighted by position value)
        /// </summary>
        public decimal AveragePortfolioApy => TotalYieldValueUsd > 0 
            ? (EstimatedAnnualYieldUsd / TotalYieldValueUsd) * 100 
            : 0;
        
        /// <summary>
        /// Distribution of assets by blockchain (blockchain name -> USD value)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Dictionary<string, decimal> BlockchainDistribution => GetBlockchainDistribution();
        
        /// <summary>
        /// Distribution of assets by token (token symbol -> USD value)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Dictionary<string, decimal> TokenDistribution => GetTokenDistribution();
        
        /// <summary>
        /// Distribution of yield by protocol (protocol name -> USD value)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Dictionary<string, decimal> ProtocolDistribution => GetProtocolDistribution();

        /// <summary>
        /// The date and time when the portfolio was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Creates a new portfolio for the specified wallet address
        /// </summary>
        public Portfolio(string walletAddress)
        {
            WalletAddress = walletAddress ?? throw new ArgumentNullException(nameof(walletAddress));
            LastUpdated = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public Portfolio()
        {
            LastUpdated = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Adds a token balance to the portfolio
        /// </summary>
        public void AddTokenBalance(TokenBalance tokenBalance)
        {
            if (tokenBalance == null)
                throw new ArgumentNullException(nameof(tokenBalance));
                
            // Check if token already exists in the portfolio
            var existingToken = TokenBalances.FirstOrDefault(t => 
                t.Token.Symbol == tokenBalance.Token.Symbol && 
                t.Token.Blockchain.Type == tokenBalance.Token.Blockchain.Type);
            
            if (existingToken != null)
            {
                // Update existing token balance
                existingToken.Balance += tokenBalance.Balance;
            }
            else
            {
                // Add new token balance
                TokenBalances.Add(tokenBalance);
            }
            
            LastUpdated = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Adds a yield position to the portfolio
        /// </summary>
        public void AddYieldPosition(YieldPosition yieldPosition)
        {
            if (yieldPosition == null)
                throw new ArgumentNullException(nameof(yieldPosition));
                
            YieldPositions.Add(yieldPosition);
            LastUpdated = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Records the current portfolio value for historical tracking
        /// </summary>
        public void RecordHistoricalValue()
        {
            var now = DateTime.UtcNow;
            HistoricalValues[now] = TotalPortfolioValueUsd;
        }
        
        /// <summary>
        /// Gets the distribution of assets by blockchain
        /// </summary>
        private Dictionary<string, decimal> GetBlockchainDistribution()
        {
            var distribution = new Dictionary<string, decimal>();
            
            // Add token balances by blockchain
            foreach (var token in TokenBalances)
            {
                var blockchain = token.Token.Blockchain.Name;
                if (!distribution.ContainsKey(blockchain))
                    distribution[blockchain] = 0;
                
                distribution[blockchain] += token.BalanceInUsd;
            }
            
            // Add yield positions by blockchain
            foreach (var position in YieldPositions)
            {
                var blockchain = position.Protocol.Blockchain.Name;
                if (!distribution.ContainsKey(blockchain))
                    distribution[blockchain] = 0;
                
                distribution[blockchain] += position.TotalValueUsd;
            }
            
            return distribution;
        }
        
        /// <summary>
        /// Gets the distribution of assets by token
        /// </summary>
        private Dictionary<string, decimal> GetTokenDistribution()
        {
            var distribution = new Dictionary<string, decimal>();
            
            // Add token balances
            foreach (var token in TokenBalances)
            {
                var symbol = token.Token.Symbol;
                if (!distribution.ContainsKey(symbol))
                    distribution[symbol] = 0;
                
                distribution[symbol] += token.BalanceInUsd;
            }
            
            // Add tokens in yield positions
            foreach (var position in YieldPositions)
            {
                foreach (var token in position.DepositedTokens)
                {
                    var symbol = token.Token.Symbol;
                    if (!distribution.ContainsKey(symbol))
                        distribution[symbol] = 0;
                    
                    distribution[symbol] += token.BalanceInUsd;
                }
            }
            
            return distribution;
        }
        
        /// <summary>
        /// Gets the distribution of yield by protocol
        /// </summary>
        private Dictionary<string, decimal> GetProtocolDistribution()
        {
            var distribution = new Dictionary<string, decimal>();
            
            foreach (var position in YieldPositions)
            {
                var protocol = position.Protocol.Name;
                if (!distribution.ContainsKey(protocol))
                    distribution[protocol] = 0;
                
                distribution[protocol] += position.TotalValueUsd;
            }
            
            return distribution;
        }
        
        /// <summary>
        /// Calculate portfolio performance metrics
        /// </summary>
        public PortfolioPerformance CalculatePerformance()
        {
            // For a real implementation, this would use historical values
            // to calculate actual performance metrics
            
            var performance = new PortfolioPerformance
            {
                CurrentValue = TotalPortfolioValueUsd,
                DailyChange = 0,
                WeeklyChange = 0,
                MonthlyChange = 0,
                YearlyChange = 0
            };
            
            // Only calculate if we have enough historical data
            if (HistoricalValues.Count > 0)
            {
                var orderedValues = HistoricalValues.OrderBy(h => h.Key).ToList();
                var mostRecent = orderedValues.Last().Value;
                
                // Find values for different time periods
                var dayAgo = orderedValues.LastOrDefault(h => h.Key <= DateTime.UtcNow.AddDays(-1)).Value;
                var weekAgo = orderedValues.LastOrDefault(h => h.Key <= DateTime.UtcNow.AddDays(-7)).Value;
                var monthAgo = orderedValues.LastOrDefault(h => h.Key <= DateTime.UtcNow.AddMonths(-1)).Value;
                var yearAgo = orderedValues.LastOrDefault(h => h.Key <= DateTime.UtcNow.AddYears(-1)).Value;
                
                // Calculate percentage changes
                if (dayAgo > 0) performance.DailyChange = (mostRecent - dayAgo) / dayAgo * 100;
                if (weekAgo > 0) performance.WeeklyChange = (mostRecent - weekAgo) / weekAgo * 100;
                if (monthAgo > 0) performance.MonthlyChange = (mostRecent - monthAgo) / monthAgo * 100;
                if (yearAgo > 0) performance.YearlyChange = (mostRecent - yearAgo) / yearAgo * 100;
            }
            
            return performance;
        }
    }
    
    /// <summary>
    /// Performance metrics for a portfolio
    /// </summary>
    public class PortfolioPerformance
    {
        /// <summary>
        /// Current value of the portfolio in USD
        /// </summary>
        public decimal CurrentValue { get; set; }
        
        /// <summary>
        /// Percentage change in the last 24 hours
        /// </summary>
        public decimal DailyChange { get; set; }
        
        /// <summary>
        /// Percentage change in the last 7 days
        /// </summary>
        public decimal WeeklyChange { get; set; }
        
        /// <summary>
        /// Percentage change in the last 30 days
        /// </summary>
        public decimal MonthlyChange { get; set; }
        
        /// <summary>
        /// Percentage change in the last year
        /// </summary>
        public decimal YearlyChange { get; set; }
    }
} 