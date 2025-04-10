using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DefiPortfolioManager.Core.Models
{
    /// <summary>
    /// Represents a balance of a specific token in a wallet
    /// </summary>
    public class TokenBalance
    {
        /// <summary>
        /// The token details
        /// </summary>
        [Required]
        public Token Token { get; set; }

        /// <summary>
        /// Address of the wallet holding the token
        /// </summary>
        [Required]
        [StringLength(66)]
        public string WalletAddress { get; set; }

        /// <summary>
        /// The balance of the token (in token units)
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Balance { get; set; }

        /// <summary>
        /// The balance in USD based on current token price
        /// </summary>
        public decimal BalanceInUsd => Balance * Token.CurrentPriceUsd;
        
        /// <summary>
        /// Balance formatted with the token's decimal places (e.g., "1.234 ETH")
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string FormattedBalance => $"{Balance.ToString($"F{Math.Min(Token.Decimals, 8)}")} {Token.Symbol}";
        
        /// <summary>
        /// Balance in USD formatted as currency (e.g., "$1,234.56")
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string FormattedBalanceUsd => $"${BalanceInUsd:N2}";
        
        /// <summary>
        /// Historical balances (date -> balance in token units)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Dictionary<DateTime, decimal> HistoricalBalances { get; set; } = new Dictionary<DateTime, decimal>();

        /// <summary>
        /// The date and time when the balance was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Creates a new token balance for the specified wallet
        /// </summary>
        public TokenBalance(Token token, string walletAddress, decimal balance)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            WalletAddress = walletAddress ?? throw new ArgumentNullException(nameof(walletAddress));
            Balance = balance >= 0 ? balance : throw new ArgumentOutOfRangeException(nameof(balance), "Balance cannot be negative");
            LastUpdated = DateTime.UtcNow;
            
            // Record the initial balance
            RecordBalance();
        }
        
        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public TokenBalance()
        {
            LastUpdated = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Updates the balance and records the change
        /// </summary>
        public void UpdateBalance(decimal newBalance)
        {
            if (newBalance < 0)
                throw new ArgumentOutOfRangeException(nameof(newBalance), "Balance cannot be negative");
                
            Balance = newBalance;
            LastUpdated = DateTime.UtcNow;
            
            // Record the new balance
            RecordBalance();
        }
        
        /// <summary>
        /// Records the current balance for historical tracking
        /// </summary>
        public void RecordBalance()
        {
            var now = DateTime.UtcNow;
            HistoricalBalances[now] = Balance;
        }
        
        /// <summary>
        /// Returns a string that represents the current token balance
        /// </summary>
        public override string ToString() => $"{FormattedBalance} (${BalanceInUsd:N2})";
    }
} 