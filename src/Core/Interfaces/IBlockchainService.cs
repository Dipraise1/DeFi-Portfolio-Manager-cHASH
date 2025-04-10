using System.Collections.Generic;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Models;

namespace DefiPortfolioManager.Core.Interfaces
{
    /// <summary>
    /// Interface for interacting with blockchain networks
    /// </summary>
    public interface IBlockchainService
    {
        /// <summary>
        /// Gets the blockchain that this service supports
        /// </summary>
        Blockchain SupportedBlockchain { get; }
        
        /// <summary>
        /// Checks if a given blockchain is supported by this service
        /// </summary>
        bool SupportsBlockchain(BlockchainType blockchainType);
        
        /// <summary>
        /// Gets the balance of the native token (e.g., ETH, MATIC)
        /// </summary>
        Task<decimal> GetNativeTokenBalanceAsync(string walletAddress);
        
        /// <summary>
        /// Gets the balance of a specific token
        /// </summary>
        Task<decimal> GetTokenBalanceAsync(string walletAddress, string tokenContractAddress);
        
        /// <summary>
        /// Gets all token balances for a wallet address
        /// </summary>
        Task<List<TokenBalance>> GetAllTokenBalancesAsync(string walletAddress);
        
        /// <summary>
        /// Gets a specific token's information by contract address
        /// </summary>
        Task<Token> GetTokenInfoAsync(string contractAddress);
        
        /// <summary>
        /// Gets a list of popular tokens on this blockchain
        /// </summary>
        Task<List<Token>> GetPopularTokensAsync(int limit = 20);
        
        /// <summary>
        /// Gets a list of transactions for a wallet address
        /// </summary>
        Task<List<BlockchainTransaction>> GetTransactionsAsync(string walletAddress, int limit = 50);
        
        /// <summary>
        /// Calls a contract method and returns the result
        /// </summary>
        Task<T> CallContractMethodAsync<T>(string contractAddress, string methodName, params object[] parameters);
        
        /// <summary>
        /// Gets all yield positions for a wallet address
        /// </summary>
        Task<List<YieldPosition>> GetYieldPositionsAsync(string walletAddress);
        
        /// <summary>
        /// Gets yield protocols available on this blockchain
        /// </summary>
        Task<List<YieldProtocol>> GetSupportedYieldProtocolsAsync();
        
        /// <summary>
        /// Checks if a wallet address is valid for this blockchain
        /// </summary>
        bool IsValidAddress(string address);
        
        /// <summary>
        /// Gets the gas price in the native token's unit (e.g., Gwei for Ethereum)
        /// </summary>
        Task<decimal> GetGasPriceAsync();
        
        /// <summary>
        /// Gets the current block number
        /// </summary>
        Task<long> GetBlockNumberAsync();
    }
    
    /// <summary>
    /// Represents a blockchain transaction
    /// </summary>
    public class BlockchainTransaction
    {
        /// <summary>
        /// Transaction hash
        /// </summary>
        public string Hash { get; set; }
        
        /// <summary>
        /// Block number the transaction was included in
        /// </summary>
        public long BlockNumber { get; set; }
        
        /// <summary>
        /// Timestamp of the block
        /// </summary>
        public System.DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Sender address
        /// </summary>
        public string From { get; set; }
        
        /// <summary>
        /// Recipient address
        /// </summary>
        public string To { get; set; }
        
        /// <summary>
        /// Value transferred in the transaction in the native token (e.g., ETH)
        /// </summary>
        public decimal Value { get; set; }
        
        /// <summary>
        /// Gas used by the transaction
        /// </summary>
        public long GasUsed { get; set; }
        
        /// <summary>
        /// Gas price in the native token's smallest unit (e.g., Wei for Ethereum)
        /// </summary>
        public decimal GasPrice { get; set; }
        
        /// <summary>
        /// Transaction fee in the native token (e.g., ETH)
        /// </summary>
        public decimal Fee => GasUsed * GasPrice;
        
        /// <summary>
        /// Transaction data (encoded function call)
        /// </summary>
        public string Data { get; set; }
        
        /// <summary>
        /// Whether the transaction was successful
        /// </summary>
        public bool IsSuccessful { get; set; }
    }
} 