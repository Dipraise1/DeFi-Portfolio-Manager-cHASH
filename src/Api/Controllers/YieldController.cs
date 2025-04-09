using System;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Interfaces;
using DefiPortfolioManager.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DefiPortfolioManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YieldController : ControllerBase
    {
        private readonly IYieldService _yieldService;
        
        public YieldController(IYieldService yieldService)
        {
            _yieldService = yieldService;
        }
        
        [HttpGet("protocols/{blockchain}")]
        public async Task<ActionResult> GetSupportedProtocols(string blockchain)
        {
            if (!Enum.TryParse<Blockchain>(blockchain, true, out var chain))
                return BadRequest("Invalid blockchain specified");
                
            var protocols = await _yieldService.GetSupportedProtocolsAsync(chain);
            return Ok(protocols);
        }
        
        [HttpGet("positions/{walletAddress}/{blockchain}")]
        public async Task<ActionResult> GetYieldPositions(string walletAddress, string blockchain)
        {
            if (string.IsNullOrEmpty(walletAddress))
                return BadRequest("Wallet address is required");
                
            if (!Enum.TryParse<Blockchain>(blockchain, true, out var chain))
                return BadRequest("Invalid blockchain specified");
                
            var positions = await _yieldService.GetUserPositionsAsync(walletAddress, chain);
            return Ok(positions);
        }
        
        [HttpGet("apy/{protocol}/{poolAddress}")]
        public async Task<ActionResult> GetCurrentApy(string protocol, string poolAddress)
        {
            if (string.IsNullOrEmpty(poolAddress))
                return BadRequest("Pool address is required");
                
            // This is a simplified example - in a real implementation, we would
            // look up the protocol by name or ID
            
            var dummyProtocol = new YieldProtocol
            {
                Name = protocol,
                Type = DetermineProtocolType(protocol)
            };
            
            var apy = await _yieldService.GetCurrentApyAsync(poolAddress, dummyProtocol);
            return Ok(new { Apy = apy });
        }
        
        private YieldProtocolType DetermineProtocolType(string protocolName)
        {
            // Simple mapping of protocol names to their types
            return protocolName.ToLower() switch
            {
                "aave" => YieldProtocolType.LendingProtocol,
                "compound" => YieldProtocolType.LendingProtocol,
                "uniswap" => YieldProtocolType.LiquidityPool,
                "sushiswap" => YieldProtocolType.LiquidityPool,
                "quickswap" => YieldProtocolType.LiquidityPool,
                "pancakeswap" => YieldProtocolType.FarmingPool,
                "yearn" => YieldProtocolType.Vault,
                "curve" => YieldProtocolType.LiquidityPool,
                "beefy" => YieldProtocolType.Vault,
                _ => YieldProtocolType.FarmingPool
            };
        }
    }
} 