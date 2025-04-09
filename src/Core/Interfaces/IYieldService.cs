using System.Collections.Generic;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Models;

namespace DefiPortfolioManager.Core.Interfaces
{
    public interface IYieldService
    {
        Task<List<YieldProtocol>> GetSupportedProtocolsAsync(Blockchain blockchain);
        Task<List<YieldPosition>> GetUserPositionsAsync(string walletAddress, Blockchain blockchain);
        Task<decimal> GetCurrentApyAsync(string poolAddress, YieldProtocol protocol);
        Task<decimal> GetEstimatedDailyYieldAsync(YieldPosition position);
        Task<decimal> GetEstimatedAnnualYieldAsync(YieldPosition position);
    }
} 