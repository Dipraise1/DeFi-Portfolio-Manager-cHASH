using System;
using System.ComponentModel.DataAnnotations;

namespace DefiPortfolioManager.Core.Models
{
    /// <summary>
    /// Represents a DeFi protocol that offers yield opportunities
    /// </summary>
    public class YieldProtocol
    {
        /// <summary>
        /// Unique identifier for the protocol
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// Name of the protocol (e.g., Aave, Uniswap, Yearn)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// URL to the protocol's logo image
        /// </summary>
        [Url]
        public string LogoUrl { get; set; }

        /// <summary>
        /// Blockchain where the protocol operates
        /// </summary>
        [Required]
        public Blockchain Blockchain { get; set; }

        /// <summary>
        /// Website URL of the protocol
        /// </summary>
        [Url]
        public string Website { get; set; }

        /// <summary>
        /// Twitter handle of the protocol
        /// </summary>
        public string Twitter { get; set; }

        /// <summary>
        /// Type of the protocol (lending, liquidity pool, etc.)
        /// </summary>
        public YieldProtocolType Type { get; set; }

        /// <summary>
        /// Total Value Locked (TVL) in the protocol in USD
        /// </summary>
        public decimal? TotalValueLockedUsd { get; set; }

        /// <summary>
        /// Date when the protocol was launched
        /// </summary>
        public DateTime? LaunchDate { get; set; }

        /// <summary>
        /// Indicates if the protocol has been audited by a reputable firm
        /// </summary>
        public bool IsAudited { get; set; }

        /// <summary>
        /// URL to the audit report
        /// </summary>
        [Url]
        public string AuditUrl { get; set; }

        /// <summary>
        /// Risk score from 1 (lowest risk) to 10 (highest risk)
        /// </summary>
        [Range(1, 10)]
        public int RiskScore { get; set; }

        /// <summary>
        /// Brief description of the protocol
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Creates a new protocol with the specified parameters
        /// </summary>
        public YieldProtocol(string name, Blockchain blockchain, YieldProtocolType type)
        {
            Id = Guid.NewGuid().ToString();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Blockchain = blockchain ?? throw new ArgumentNullException(nameof(blockchain));
            Type = type;
        }

        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public YieldProtocol()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Returns a string that represents the current protocol
        /// </summary>
        public override string ToString() => $"{Name} ({Blockchain.Name})";
    }

    /// <summary>
    /// Types of yield-generating protocols
    /// </summary>
    public enum YieldProtocolType
    {
        /// <summary>
        /// Protocols like Aave, Compound where users lend assets
        /// </summary>
        LendingProtocol,

        /// <summary>
        /// Protocols like Uniswap, SushiSwap where users provide liquidity
        /// </summary>
        LiquidityPool,

        /// <summary>
        /// Protocols like Yearn, Beefy that automate yield strategies
        /// </summary>
        Vault,

        /// <summary>
        /// Protocols where users stake tokens for rewards
        /// </summary>
        Staking,

        /// <summary>
        /// Yield farming protocols
        /// </summary>
        FarmingPool,
        
        /// <summary>
        /// Options and derivatives protocols
        /// </summary>
        Derivatives,
        
        /// <summary>
        /// Protocols for real-world assets
        /// </summary>
        RealWorldAssets
    }
} 