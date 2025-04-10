using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DefiPortfolioManager.Core.Models
{
    /// <summary>
    /// Represents a yield-generating position in a DeFi protocol
    /// </summary>
    public class YieldPosition
    {
        /// <summary>
        /// Unique identifier for the position
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// Address of the wallet holding the position
        /// </summary>
        [Required]
        [StringLength(66)]
        public string WalletAddress { get; set; }

        /// <summary>
        /// The protocol where the position is held
        /// </summary>
        [Required]
        public YieldProtocol Protocol { get; set; }

        /// <summary>
        /// Name of the pool or vault
        /// </summary>
        [Required]
        public string PoolName { get; set; }

        /// <summary>
        /// Address of the pool or vault contract
        /// </summary>
        [StringLength(66)]
        public string PoolAddress { get; set; }

        /// <summary>
        /// Tokens deposited into the position
        /// </summary>
        [Required]
        public List<TokenBalance> DepositedTokens { get; set; } = new List<TokenBalance>();

        /// <summary>
        /// Total value of the position in USD
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalValueUsd { get; private set; }

        /// <summary>
        /// Annual Percentage Yield (APY) of the position
        /// </summary>
        public decimal Apy { get; set; }

        /// <summary>
        /// Estimated daily yield in USD based on current APY
        /// </summary>
        public decimal DailyYieldUsd { get; private set; }

        /// <summary>
        /// Estimated monthly yield in USD based on current APY
        /// </summary>
        public decimal MonthlyYieldUsd { get; private set; }

        /// <summary>
        /// Estimated yearly yield in USD based on current APY
        /// </summary>
        public decimal YearlyYieldUsd { get; private set; }

        /// <summary>
        /// Historical APY values (date -> APY)
        /// </summary>
        public Dictionary<DateTime, decimal> HistoricalApy { get; set; } = new Dictionary<DateTime, decimal>();

        /// <summary>
        /// Date when the position was entered
        /// </summary>
        public DateTime EntryTime { get; set; }

        /// <summary>
        /// Last time the position data was updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Returns the total earnings since entry in USD
        /// </summary>
        public decimal TotalEarningsSinceEntryUsd
        {
            get
            {
                // If we don't have historical data, do a simple calculation
                if (HistoricalApy.Count == 0)
                {
                    var daysInPosition = (DateTime.UtcNow - EntryTime).TotalDays;
                    return TotalValueUsd * (decimal)(daysInPosition) * (Apy / 365m / 100m);
                }

                // With historical data, we could do a more accurate calculation
                // (not implemented here for simplicity)
                return HistoricalApy.Sum(h => TotalValueUsd * (Apy / 365m / 100m));
            }
        }

        /// <summary>
        /// Calculates and updates the yield values based on the current APY and total value
        /// </summary>
        public void UpdateYieldValues()
        {
            TotalValueUsd = DepositedTokens.Sum(t => t.BalanceInUsd);
            DailyYieldUsd = TotalValueUsd * (Apy / 365m / 100m);
            MonthlyYieldUsd = DailyYieldUsd * 30.42m; // Average days in a month
            YearlyYieldUsd = TotalValueUsd * (Apy / 100m);
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor for creating a new yield position
        /// </summary>
        public YieldPosition(string walletAddress, YieldProtocol protocol, string poolName)
        {
            Id = Guid.NewGuid().ToString();
            WalletAddress = walletAddress ?? throw new ArgumentNullException(nameof(walletAddress));
            Protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            PoolName = poolName ?? throw new ArgumentNullException(nameof(poolName));
            EntryTime = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public YieldPosition() 
        {
            Id = Guid.NewGuid().ToString();
            EntryTime = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds a token to the position
        /// </summary>
        public void AddToken(TokenBalance tokenBalance)
        {
            if (tokenBalance == null)
                throw new ArgumentNullException(nameof(tokenBalance));

            DepositedTokens.Add(tokenBalance);
            UpdateYieldValues();
        }

        /// <summary>
        /// Returns a string that represents the current position
        /// </summary>
        public override string ToString() => $"{Protocol.Name} - {PoolName} (${TotalValueUsd:N2}, {Apy}% APY)";
    }
} 