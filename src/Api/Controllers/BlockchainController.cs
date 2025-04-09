using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Interfaces;
using DefiPortfolioManager.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DefiPortfolioManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlockchainController : ControllerBase
    {
        private readonly Dictionary<Blockchain, IBlockchainService> _blockchainServices;
        
        public BlockchainController(IEnumerable<IBlockchainService> blockchainServices)
        {
            _blockchainServices = blockchainServices.ToDictionary(s => s.SupportedBlockchain);
        }
        
        [HttpGet("supported")]
        public ActionResult<IEnumerable<string>> GetSupportedBlockchains()
        {
            var supportedChains = _blockchainServices.Keys
                .Select(b => b.ToString())
                .ToList();
                
            return Ok(supportedChains);
        }
        
        [HttpGet("{blockchain}/{walletAddress}/balance")]
        public async Task<ActionResult> GetNativeTokenBalance(string blockchain, string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
                return BadRequest("Wallet address is required");
                
            if (!Enum.TryParse<Blockchain>(blockchain, true, out var chain))
                return BadRequest("Invalid blockchain specified");
                
            if (!_blockchainServices.TryGetValue(chain, out var service))
                return NotFound($"Blockchain {blockchain} is not supported");
                
            var balance = await service.GetNativeTokenBalanceAsync(walletAddress);
            
            return Ok(new { Balance = balance, Symbol = GetNativeTokenSymbol(chain) });
        }
        
        [HttpGet("{blockchain}/{walletAddress}/token/{tokenAddress}")]
        public async Task<ActionResult> GetTokenBalance(string blockchain, string walletAddress, string tokenAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
                return BadRequest("Wallet address is required");
                
            if (string.IsNullOrEmpty(tokenAddress))
                return BadRequest("Token address is required");
                
            if (!Enum.TryParse<Blockchain>(blockchain, true, out var chain))
                return BadRequest("Invalid blockchain specified");
                
            if (!_blockchainServices.TryGetValue(chain, out var service))
                return NotFound($"Blockchain {blockchain} is not supported");
                
            var balance = await service.GetTokenBalanceAsync(walletAddress, tokenAddress);
            
            return Ok(new { Balance = balance, TokenAddress = tokenAddress });
        }
        
        [HttpGet("{blockchain}/{walletAddress}/tokens")]
        public async Task<ActionResult> GetAllTokenBalances(string blockchain, string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
                return BadRequest("Wallet address is required");
                
            if (!Enum.TryParse<Blockchain>(blockchain, true, out var chain))
                return BadRequest("Invalid blockchain specified");
                
            if (!_blockchainServices.TryGetValue(chain, out var service))
                return NotFound($"Blockchain {blockchain} is not supported");
                
            var balances = await service.GetAllTokenBalancesAsync(walletAddress);
            
            return Ok(balances);
        }
        
        private string GetNativeTokenSymbol(Blockchain blockchain)
        {
            return blockchain switch
            {
                Blockchain.Ethereum => "ETH",
                Blockchain.Polygon => "MATIC",
                Blockchain.BinanceSmartChain => "BNB",
                Blockchain.Avalanche => "AVAX",
                Blockchain.Arbitrum => "ETH",
                Blockchain.Optimism => "ETH",
                Blockchain.Solana => "SOL",
                Blockchain.Fantom => "FTM",
                _ => "Unknown"
            };
        }
    }
} 