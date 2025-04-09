using System;
using System.Collections.Generic;
using System.Linq;

namespace DefiPortfolioManager.Core.Models
{
    public class Portfolio
    {
        public string WalletAddress { get; set; }
        public List<TokenBalance> TokenBalances { get; set; } = new List<TokenBalance>();
        public List<YieldPosition> YieldPositions { get; set; } = new List<YieldPosition>();
        
        // Calculated properties
        public decimal TotalTokenValueUsd => TokenBalances.Sum(t => t.BalanceInUsd);
        public decimal TotalYieldValueUsd => YieldPositions.Sum(y => y.TotalValueUsd);
        public decimal TotalPortfolioValueUsd => TotalTokenValueUsd + TotalYieldValueUsd;
        public decimal EstimatedDailyYieldUsd => YieldPositions.Sum(y => y.DailyYieldUsd);
        public decimal EstimatedAnnualYieldUsd => EstimatedDailyYieldUsd * 365;
        public decimal AveragePortfolioApy => TotalYieldValueUsd > 0 
            ? (EstimatedAnnualYieldUsd / TotalYieldValueUsd) * 100 
            : 0;
        
        public DateTime LastUpdated { get; set; }
    }
} 