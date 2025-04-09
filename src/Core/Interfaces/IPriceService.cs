using System.Collections.Generic;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Models;

namespace DefiPortfolioManager.Core.Interfaces
{
    public interface IPriceService
    {
        Task<decimal> GetTokenPriceAsync(string tokenSymbol, string currency = "USD");
        Task<decimal> GetTokenPriceAsync(Token token, string currency = "USD");
        Task<Dictionary<string, decimal>> GetTokenPricesAsync(List<string> tokenSymbols, string currency = "USD");
        Task<Dictionary<Token, decimal>> GetTokenPricesAsync(List<Token> tokens, string currency = "USD");
        Task<decimal> GetPriceChangePercentageAsync(string tokenSymbol, int days = 1);
    }
} 