using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DefiPortfolioManager.Core.Models
{
    /// <summary>
    /// Represents a blockchain with its properties and configuration
    /// </summary>
    public class Blockchain
    {
        /// <summary>
        /// Unique identifier of the blockchain
        /// </summary>
        [Required]
        public BlockchainType Type { get; set; }

        /// <summary>
        /// Name of the blockchain
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Chain ID of the blockchain
        /// </summary>
        public int ChainId { get; set; }

        /// <summary>
        /// Symbol of the native currency (e.g., ETH, MATIC)
        /// </summary>
        [Required]
        public string NativeCurrencySymbol { get; set; }

        /// <summary>
        /// Name of the native currency (e.g., Ethereum, Polygon)
        /// </summary>
        [Required]
        public string NativeCurrencyName { get; set; }

        /// <summary>
        /// Decimals of the native currency
        /// </summary>
        [Range(0, 36)]
        public int NativeCurrencyDecimals { get; set; }

        /// <summary>
        /// RPC URL for connecting to the blockchain
        /// </summary>
        [Url]
        public string RpcUrl { get; set; }

        /// <summary>
        /// URL of the blockchain explorer
        /// </summary>
        [Url]
        public string ExplorerUrl { get; set; }

        /// <summary>
        /// URL of the blockchain logo
        /// </summary>
        [Url]
        public string LogoUrl { get; set; }

        /// <summary>
        /// Indicates if the blockchain supports EVM
        /// </summary>
        public bool IsEvm { get; set; }

        /// <summary>
        /// Average block time in seconds
        /// </summary>
        public decimal AverageBlockTimeSeconds { get; set; }

        /// <summary>
        /// Creates a blockchain with the specified parameters
        /// </summary>
        public Blockchain(BlockchainType type, string name, int chainId, string nativeCurrencySymbol, string nativeCurrencyName, int nativeCurrencyDecimals)
        {
            Type = type;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ChainId = chainId;
            NativeCurrencySymbol = nativeCurrencySymbol ?? throw new ArgumentNullException(nameof(nativeCurrencySymbol));
            NativeCurrencyName = nativeCurrencyName ?? throw new ArgumentNullException(nameof(nativeCurrencyName));
            NativeCurrencyDecimals = nativeCurrencyDecimals;
            IsEvm = DetermineIfEvm(type);
        }

        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public Blockchain() { }

        /// <summary>
        /// Returns a string that represents the current blockchain
        /// </summary>
        public override string ToString() => Name;
        
        /// <summary>
        /// Returns a token representing the native currency of this blockchain
        /// </summary>
        public Token GetNativeToken()
        {
            return new Token(NativeCurrencySymbol, NativeCurrencyName)
            {
                Blockchain = this,
                IsNative = true,
                Decimals = NativeCurrencyDecimals,
                LogoUrl = LogoUrl
            };
        }
        
        private bool DetermineIfEvm(BlockchainType type)
        {
            return type switch
            {
                BlockchainType.Ethereum => true,
                BlockchainType.Polygon => true,
                BlockchainType.BinanceSmartChain => true,
                BlockchainType.Avalanche => true,
                BlockchainType.Arbitrum => true,
                BlockchainType.Optimism => true,
                BlockchainType.Fantom => true,
                BlockchainType.Solana => false,
                _ => false,
            };
        }
        
        /// <summary>
        /// Gets a list of all supported blockchains
        /// </summary>
        public static List<Blockchain> GetSupportedBlockchains()
        {
            return new List<Blockchain>
            {
                new Blockchain(BlockchainType.Ethereum, "Ethereum", 1, "ETH", "Ethereum", 18)
                {
                    RpcUrl = "https://mainnet.infura.io/v3/your-api-key",
                    ExplorerUrl = "https://etherscan.io",
                    LogoUrl = "https://cryptologos.cc/logos/ethereum-eth-logo.png",
                    AverageBlockTimeSeconds = 13.2m
                },
                new Blockchain(BlockchainType.Polygon, "Polygon", 137, "MATIC", "Polygon", 18)
                {
                    RpcUrl = "https://polygon-rpc.com",
                    ExplorerUrl = "https://polygonscan.com",
                    LogoUrl = "https://cryptologos.cc/logos/polygon-matic-logo.png",
                    AverageBlockTimeSeconds = 2.1m
                },
                new Blockchain(BlockchainType.BinanceSmartChain, "BNB Chain", 56, "BNB", "BNB", 18)
                {
                    RpcUrl = "https://bsc-dataseed.binance.org",
                    ExplorerUrl = "https://bscscan.com",
                    LogoUrl = "https://cryptologos.cc/logos/bnb-bnb-logo.png",
                    AverageBlockTimeSeconds = 3m
                },
                new Blockchain(BlockchainType.Arbitrum, "Arbitrum", 42161, "ETH", "Ethereum", 18)
                {
                    RpcUrl = "https://arb1.arbitrum.io/rpc",
                    ExplorerUrl = "https://arbiscan.io",
                    LogoUrl = "https://cryptologos.cc/logos/arbitrum-arb-logo.png",
                    AverageBlockTimeSeconds = 0.25m
                }
            };
        }
    }
    
    /// <summary>
    /// Enumeration of blockchain types
    /// </summary>
    public enum BlockchainType
    {
        Ethereum,
        Polygon,
        BinanceSmartChain,
        Avalanche,
        Arbitrum,
        Optimism,
        Solana,
        Fantom
    }
} 