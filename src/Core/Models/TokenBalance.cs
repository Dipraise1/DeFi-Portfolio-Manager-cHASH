using System;

namespace DefiPortfolioManager.Core.Models
{
    public class TokenBalance
    {
        public Token Token { get; set; }
        public string WalletAddress { get; set; }
        public decimal Balance { get; set; }
        public decimal BalanceInUsd => Balance * Token.CurrentPriceUsd;
        public DateTime LastUpdated { get; set; }
    }
} 