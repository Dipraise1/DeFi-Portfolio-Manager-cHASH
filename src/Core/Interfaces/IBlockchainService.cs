using System.Collections.Generic;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Models;

namespace DefiPortfolioManager.Core.Interfaces
{
    public interface IBlockchainService
    {
        Blockchain SupportedBlockchain { get; }
        
        // Token balance operations
        Task<decimal> GetNativeTokenBalanceAsync(string walletAddress);
        Task<decimal> GetTokenBalanceAsync(string walletAddress, string tokenContractAddress);
        Task<List<TokenBalance>> GetAllTokenBalancesAsync(string walletAddress);
        
        // Contract interaction
        Task<T> CallContractMethodAsync<T>(string contractAddress, string methodName, params object[] parameters);
    }
} 