using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Interfaces;
using DefiPortfolioManager.Core.Models;

namespace DefiPortfolioManager.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly Dictionary<Blockchain, IBlockchainService> _blockchainServices;
        private readonly IPriceService _priceService;
        private readonly IYieldService _yieldService;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan _defaultCacheExpiry = TimeSpan.FromMinutes(5);

        public PortfolioService(
            IEnumerable<IBlockchainService> blockchainServices,
            IPriceService priceService,
            IYieldService yieldService,
            ICacheService cacheService)
        {
            _blockchainServices = blockchainServices.ToDictionary(s => s.SupportedBlockchain);
            _priceService = priceService;
            _yieldService = yieldService;
            _cacheService = cacheService;
        }

        public async Task<Portfolio> GetPortfolioAsync(string walletAddress)
        {
            string cacheKey = $"portfolio:{walletAddress}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var portfolio = new Portfolio
                {
                    WalletAddress = walletAddress,
                    TokenBalances = new List<TokenBalance>(),
                    YieldPositions = new List<YieldPosition>(),
                    LastUpdated = DateTime.UtcNow
                };

                // Get token balances from all supported blockchains
                var tokenBalances = await GetTokenBalancesAsync(walletAddress);
                portfolio.TokenBalances.AddRange(tokenBalances);

                // Get yield positions from all supported blockchains
                var yieldPositions = await GetYieldPositionsAsync(walletAddress);
                portfolio.YieldPositions.AddRange(yieldPositions);

                return portfolio;
            }, _defaultCacheExpiry);
        }

        public async Task<List<TokenBalance>> GetTokenBalancesAsync(string walletAddress)
        {
            string cacheKey = $"tokenBalances:{walletAddress}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var tokenBalances = new List<TokenBalance>();

                // Get token balances from all supported blockchains
                var tokenBalanceTasks = new List<Task<List<TokenBalance>>>();
                foreach (var blockchainService in _blockchainServices.Values)
                {
                    tokenBalanceTasks.Add(blockchainService.GetAllTokenBalancesAsync(walletAddress));
                }

                var tokenBalanceResults = await Task.WhenAll(tokenBalanceTasks);
                foreach (var balances in tokenBalanceResults)
                {
                    tokenBalances.AddRange(balances);
                }

                return tokenBalances;
            }, _defaultCacheExpiry);
        }

        public async Task<List<YieldPosition>> GetYieldPositionsAsync(string walletAddress)
        {
            string cacheKey = $"yieldPositions:{walletAddress}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var yieldPositions = new List<YieldPosition>();

                // Get yield positions from all supported blockchains
                var yieldPositionTasks = new List<Task<List<YieldPosition>>>();
                foreach (var blockchain in _blockchainServices.Keys)
                {
                    yieldPositionTasks.Add(_yieldService.GetUserPositionsAsync(walletAddress, blockchain));
                }

                var yieldPositionResults = await Task.WhenAll(yieldPositionTasks);
                foreach (var positions in yieldPositionResults)
                {
                    yieldPositions.AddRange(positions);
                }

                return yieldPositions;
            }, _defaultCacheExpiry);
        }

        public async Task<decimal> GetTotalPortfolioValueAsync(string walletAddress, string currency = "USD")
        {
            string cacheKey = $"portfolioValue:{walletAddress}:{currency}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var portfolio = await GetPortfolioAsync(walletAddress);
                return portfolio.TotalPortfolioValueUsd;
            }, _defaultCacheExpiry);
        }

        public async Task<Portfolio> RefreshPortfolioAsync(string walletAddress)
        {
            // Clear all cached data for this wallet
            await ClearWalletCacheAsync(walletAddress);
            
            // Fetch fresh data
            return await GetPortfolioAsync(walletAddress);
        }
        
        private async Task ClearWalletCacheAsync(string walletAddress)
        {
            // Clear all cached data related to this wallet
            await _cacheService.RemoveAsync($"portfolio:{walletAddress}");
            await _cacheService.RemoveAsync($"tokenBalances:{walletAddress}");
            await _cacheService.RemoveAsync($"yieldPositions:{walletAddress}");
            await _cacheService.RemoveAsync($"portfolioValue:{walletAddress}:USD");
        }
    }
} 