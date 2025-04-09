using System.Collections.Generic;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Models;

namespace DefiPortfolioManager.Core.Interfaces
{
    public interface IPortfolioService
    {
        Task<Portfolio> GetPortfolioAsync(string walletAddress);
        Task<List<TokenBalance>> GetTokenBalancesAsync(string walletAddress);
        Task<List<YieldPosition>> GetYieldPositionsAsync(string walletAddress);
        Task<decimal> GetTotalPortfolioValueAsync(string walletAddress, string currency = "USD");
        Task<Portfolio> RefreshPortfolioAsync(string walletAddress);
    }
} 