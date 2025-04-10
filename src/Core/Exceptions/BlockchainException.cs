using System;

namespace DefiPortfolioManager.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when errors occur during blockchain operations
    /// </summary>
    public class BlockchainException : Exception
    {
        /// <summary>
        /// Creates a new BlockchainException with the specified message
        /// </summary>
        public BlockchainException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Creates a new BlockchainException with the specified message and inner exception
        /// </summary>
        public BlockchainException(string message, Exception innerException) : base(message, innerException)
        {
        }
        
        /// <summary>
        /// The blockchain network where the error occurred
        /// </summary>
        public string Blockchain { get; set; }
        
        /// <summary>
        /// The wallet address involved (if applicable)
        /// </summary>
        public string WalletAddress { get; set; }
        
        /// <summary>
        /// The token contract address involved (if applicable)
        /// </summary>
        public string ContractAddress { get; set; }
        
        /// <summary>
        /// The type of blockchain operation that failed
        /// </summary>
        public string OperationType { get; set; }
    }
} 