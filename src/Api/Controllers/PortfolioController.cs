using System.Threading.Tasks;
using DefiPortfolioManager.Core.Interfaces;
using DefiPortfolioManager.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DefiPortfolioManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;
        
        public PortfolioController(IPortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }
        
        [HttpGet("{walletAddress}")]
        public async Task<ActionResult<Portfolio>> GetPortfolio(string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
                return BadRequest("Wallet address is required");
                
            var portfolio = await _portfolioService.GetPortfolioAsync(walletAddress);
            return Ok(portfolio);
        }
        
        [HttpGet("{walletAddress}/tokens")]
        public async Task<ActionResult> GetTokenBalances(string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
                return BadRequest("Wallet address is required");
                
            var tokenBalances = await _portfolioService.GetTokenBalancesAsync(walletAddress);
            return Ok(tokenBalances);
        }
        
        [HttpGet("{walletAddress}/yield")]
        public async Task<ActionResult> GetYieldPositions(string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
                return BadRequest("Wallet address is required");
                
            var yieldPositions = await _portfolioService.GetYieldPositionsAsync(walletAddress);
            return Ok(yieldPositions);
        }
        
        [HttpGet("{walletAddress}/value")]
        public async Task<ActionResult> GetTotalPortfolioValue(string walletAddress, [FromQuery] string currency = "USD")
        {
            if (string.IsNullOrEmpty(walletAddress))
                return BadRequest("Wallet address is required");
                
            var value = await _portfolioService.GetTotalPortfolioValueAsync(walletAddress, currency);
            return Ok(new { Value = value, Currency = currency });
        }
        
        [HttpPost("{walletAddress}/refresh")]
        public async Task<ActionResult> RefreshPortfolio(string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
                return BadRequest("Wallet address is required");
                
            var portfolio = await _portfolioService.RefreshPortfolioAsync(walletAddress);
            return Ok(portfolio);
        }
    }
} 