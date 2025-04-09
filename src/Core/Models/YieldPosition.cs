using System;
using System.Collections.Generic;

namespace DefiPortfolioManager.Core.Models
{
    public class YieldPosition
    {
        public string Id { get; set; }
        public string WalletAddress { get; set; }
        public YieldProtocol Protocol { get; set; }
        public string PoolName { get; set; }
        public string PoolAddress { get; set; }
        public List<TokenBalance> DepositedTokens { get; set; } = new List<TokenBalance>();
        public decimal TotalValueUsd { get; set; }
        public decimal Apy { get; set; }
        public decimal DailyYieldUsd { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime LastUpdated { get; set; }
    }
} 