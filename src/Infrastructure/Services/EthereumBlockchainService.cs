using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Interfaces;
using DefiPortfolioManager.Core.Models;
using DefiPortfolioManager.Core.Settings;
using DefiPortfolioManager.Core.Utils;
using DefiPortfolioManager.Core.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Net.Http;

namespace DefiPortfolioManager.Infrastructure.Services
{
    /// <summary>
    /// Service for interacting with the Ethereum blockchain and related networks
    /// </summary>
    public class EthereumBlockchainService : IBlockchainService
    {
        private readonly IPriceService _priceService;
        private readonly Blockchain _blockchain;
        private readonly Web3 _web3;
        private readonly AppSettings _settings;
        private readonly RateLimiter _rateLimiter;
        private readonly ILogger<EthereumBlockchainService> _logger;
        
        // Standard ERC20 ABI elements for common operations
        private const string ERC20ABI = @"[
            {""constant"":true,""inputs"":[],""name"":""name"",""outputs"":[{""name"":"""",""type"":""string""}],""type"":""function""},
            {""constant"":true,""inputs"":[],""name"":""symbol"",""outputs"":[{""name"":"""",""type"":""string""}],""type"":""function""},
            {""constant"":true,""inputs"":[],""name"":""decimals"",""outputs"":[{""name"":"""",""type"":""uint8""}],""type"":""function""},
            {""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],""type"":""function""}
        ]";
        
        /// <summary>
        /// Creates a new Ethereum blockchain service
        /// </summary>
        public EthereumBlockchainService(
            IPriceService priceService, 
            IOptions<AppSettings> options,
            ILogger<EthereumBlockchainService> logger = null)
        {
            _priceService = priceService ?? throw new ArgumentNullException(nameof(priceService));
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
            
            _blockchain = new Blockchain(BlockchainType.Ethereum, "Ethereum", 1, "ETH", "Ethereum", 18)
            {
                RpcUrl = _settings.Blockchain.EthereumRpcUrl,
                ExplorerUrl = "https://etherscan.io",
                LogoUrl = "https://cryptologos.cc/logos/ethereum-eth-logo.png",
                AverageBlockTimeSeconds = 13.2m,
                IsEvm = true
            };
            
            // Initialize Web3 with the configured RPC URL
            _web3 = new Web3(_settings.Blockchain.EthereumRpcUrl);
            
            // Initialize rate limiter
            _rateLimiter = new RateLimiter();
            
            // Configure rate limits
            _rateLimiter.Configure("infura", 5); // 5 requests per second for Infura
            _rateLimiter.Configure("etherscan", 3); // 3 requests per second for Etherscan
            
            _logger?.LogInformation("EthereumBlockchainService initialized with RPC URL: {RpcUrl}", 
                MaskApiKey(_settings.Blockchain.EthereumRpcUrl));
        }
        
        /// <summary>
        /// Gets the blockchain that this service supports
        /// </summary>
        public Blockchain SupportedBlockchain => _blockchain;
        
        /// <summary>
        /// Checks if a given blockchain is supported by this service
        /// </summary>
        public bool SupportsBlockchain(BlockchainType blockchainType)
        {
            return blockchainType == BlockchainType.Ethereum;
        }

        /// <summary>
        /// Gets the balance of the native token (ETH)
        /// </summary>
        public async Task<decimal> GetNativeTokenBalanceAsync(string walletAddress)
        {
            if (!IsValidAddress(walletAddress))
                throw new ArgumentException("Invalid Ethereum address", nameof(walletAddress));
                
            try
            {
                _logger?.LogDebug("Getting ETH balance for wallet {Wallet}", walletAddress);
                
                return await _rateLimiter.ExecuteAsync("infura", async () =>
                {
                    var balanceWei = await _web3.Eth.GetBalance.SendRequestAsync(walletAddress);
                    var balanceEth = Web3.Convert.FromWei(balanceWei.Value);
                    
                    _logger?.LogDebug("ETH balance for wallet {Wallet}: {Balance}", 
                        walletAddress, balanceEth);
                        
                    return (decimal)balanceEth;
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting ETH balance for wallet {Wallet}", walletAddress);
                throw new BlockchainException("Failed to get ETH balance", ex);
            }
        }

        /// <summary>
        /// Gets the balance of a specific token
        /// </summary>
        public async Task<decimal> GetTokenBalanceAsync(string walletAddress, string tokenContractAddress)
        {
            if (!IsValidAddress(walletAddress))
                throw new ArgumentException("Invalid Ethereum address", nameof(walletAddress));
                
            if (!IsValidAddress(tokenContractAddress))
                throw new ArgumentException("Invalid token contract address", nameof(tokenContractAddress));
                
            try
            {
                return await _rateLimiter.ExecuteAsync("infura", async () =>
                {
                    // Get the token contract
                    var contract = _web3.Eth.GetContract(ERC20ABI, tokenContractAddress);
                    
                    // Call balanceOf function
                    var balanceFunction = contract.GetFunction("balanceOf");
                    var balance = await balanceFunction.CallAsync<BigInteger>(walletAddress);
                    
                    // Get token decimals
                    var decimalsFunction = contract.GetFunction("decimals");
                    var decimals = await decimalsFunction.CallAsync<byte>();
                    
                    // Convert to decimal with proper decimal places
                    return (decimal)(balance / BigInteger.Pow(10, decimals));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting token balance: {ex.Message}");
                throw new Exception("Failed to get token balance", ex);
            }
        }

        /// <summary>
        /// Gets all token balances for a wallet address
        /// </summary>
        public async Task<List<TokenBalance>> GetAllTokenBalancesAsync(string walletAddress)
        {
            if (!IsValidAddress(walletAddress))
                throw new ArgumentException("Invalid Ethereum address", nameof(walletAddress));
                
            var result = new List<TokenBalance>();
            
            try
            {
                // First get the native ETH balance
                var ethBalance = await GetNativeTokenBalanceAsync(walletAddress);
                var ethToken = _blockchain.GetNativeToken();
                
                try
                {
                    ethToken.CurrentPriceUsd = await _priceService.GetTokenPriceAsync(ethToken.Symbol);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not get ETH price: {ex.Message}");
                }
                
                result.Add(new TokenBalance(ethToken, walletAddress, ethBalance));
                
                // Use Etherscan API to get ERC20 token balances
                using var httpClient = new HttpClient();
                string apiUrl = $"{_settings.ExternalApis.EtherscanBaseUrl}?module=account&action=tokentx&address={walletAddress}&sort=asc&apikey={_settings.Blockchain.EtherscanApiKey}";
                
                // Get token transfer history to identify tokens
                var response = await httpClient.GetStringAsync(apiUrl);
                
                // We would normally use a proper JSON parser here
                // For now, we'll extract the token contract addresses
                var uniqueTokens = new HashSet<string>();
                var tokenCache = new Dictionary<string, Token>();
                
                // Parse JSON response - this is a simplified approach
                // In production, use a proper JSON library like Newtonsoft.Json or System.Text.Json
                foreach (var line in response.Split('\n'))
                {
                    if (line.Contains("\"contractAddress\":"))
                    {
                        var start = line.IndexOf("\"") + 1;
                        var end = line.IndexOf("\"", start);
                        var contractAddress = line.Substring(start, end - start).Trim().ToLowerInvariant();
                        
                        if (!string.IsNullOrEmpty(contractAddress) && IsValidAddress(contractAddress))
                        {
                            uniqueTokens.Add(contractAddress);
                        }
                    }
                }
                
                // Now get the balance for each token
                foreach (var contractAddress in uniqueTokens)
                {
                    try
                    {
                        // Get token info first
                        Token token;
                        if (!tokenCache.TryGetValue(contractAddress, out token))
                        {
                            token = await GetTokenInfoAsync(contractAddress);
                            tokenCache[contractAddress] = token;
                        }
                        
                        // Get the balance
                        var balance = await GetTokenBalanceAsync(walletAddress, contractAddress);
                        
                        // Only add if balance > 0
                        if (balance > 0)
                        {
                            result.Add(new TokenBalance(token, walletAddress, balance));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting balance for token {contractAddress}: {ex.Message}");
                        // Continue with other tokens even if one fails
                    }
                }
                
                // Update token prices in batches for better performance
                var tokens = result.Select(b => b.Token).Where(t => t.CurrentPriceUsd <= 0).ToList();
                if (tokens.Count > 0)
                {
                    try
                    {
                        var prices = await _priceService.GetTokenPricesAsync(tokens);
                        
                        foreach (var balance in result)
                        {
                            if (prices.TryGetValue(balance.Token, out var price) && balance.Token.CurrentPriceUsd <= 0)
                            {
                                balance.Token.CurrentPriceUsd = price;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Error getting token prices: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all token balances: {ex.Message}");
                throw new Exception("Failed to get all token balances", ex);
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets a specific token's information by contract address
        /// </summary>
        public async Task<Token> GetTokenInfoAsync(string contractAddress)
        {
            if (!IsValidAddress(contractAddress))
                throw new ArgumentException("Invalid token contract address", nameof(contractAddress));
                
            try
            {
                // Get the token contract
                var contract = _web3.Eth.GetContract(ERC20ABI, contractAddress);
                
                // Get token name
                var nameFunction = contract.GetFunction("name");
                var name = await nameFunction.CallAsync<string>();
                
                // Get token symbol
                var symbolFunction = contract.GetFunction("symbol");
                var symbol = await symbolFunction.CallAsync<string>();
                
                // Get token decimals
                var decimalsFunction = contract.GetFunction("decimals");
                var decimals = await decimalsFunction.CallAsync<byte>();
                
                // Create token object
                var token = new Token(symbol, name)
                {
                    ContractAddress = contractAddress,
                    Blockchain = _blockchain,
                    Decimals = decimals,
                    IsNative = false
                };
                
                // Try to get token price and other market data
                try
                {
                    var price = await _priceService.GetTokenPriceAsync(token);
                    token.CurrentPriceUsd = price;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not get price for token {symbol}: {ex.Message}");
                }
                
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting token info: {ex.Message}");
                throw new Exception("Failed to get token information", ex);
            }
        }
        
        /// <summary>
        /// Gets a list of popular tokens on Ethereum
        /// </summary>
        public async Task<List<Token>> GetPopularTokensAsync(int limit = 20)
        {
            // In a real implementation, this would use an API like CoinGecko or CoinMarketCap
            // to get popular Ethereum tokens
            // Placeholder implementation
            
            // Simulate API call
            await Task.Delay(200);
            
            var popularTokens = new List<Token>
            {
                _blockchain.GetNativeToken(),
                new Token("LINK", "Chainlink")
                {
                    ContractAddress = "0x514910771af9ca656af840dff83e8264ecf986ca",
                    Blockchain = _blockchain,
                    Decimals = 18,
                    LogoUrl = "https://cryptologos.cc/logos/chainlink-link-logo.png",
                    CoingeckoId = "chainlink"
                },
                new Token("USDT", "Tether")
                {
                    ContractAddress = "0xdac17f958d2ee523a2206206994597c13d831ec7",
                    Blockchain = _blockchain,
                    Decimals = 6,
                    LogoUrl = "https://cryptologos.cc/logos/tether-usdt-logo.png",
                    CoingeckoId = "tether"
                },
                new Token("USDC", "USD Coin")
                {
                    ContractAddress = "0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48",
                    Blockchain = _blockchain,
                    Decimals = 6,
                    LogoUrl = "https://cryptologos.cc/logos/usd-coin-usdc-logo.png",
                    CoingeckoId = "usd-coin"
                },
                new Token("UNI", "Uniswap")
                {
                    ContractAddress = "0x1f9840a85d5af5bf1d1762f925bdaddc4201f984",
                    Blockchain = _blockchain,
                    Decimals = 18,
                    LogoUrl = "https://cryptologos.cc/logos/uniswap-uni-logo.png",
                    CoingeckoId = "uniswap"
                }
            };
            
            // Update token prices
            var prices = await _priceService.GetTokenPricesAsync(popularTokens);
            foreach (var token in popularTokens)
            {
                if (prices.TryGetValue(token, out var price))
                {
                    token.CurrentPriceUsd = price;
                }
            }
            
            return popularTokens.Take(Math.Min(limit, popularTokens.Count)).ToList();
        }
        
        /// <summary>
        /// Gets a list of transactions for a wallet address
        /// </summary>
        public async Task<List<BlockchainTransaction>> GetTransactionsAsync(string walletAddress, int limit = 50)
        {
            if (!IsValidAddress(walletAddress))
                throw new ArgumentException("Invalid Ethereum address", nameof(walletAddress));
                
            // In a real implementation, this would use an Ethereum scanner API (like Etherscan)
            // to get transactions for a wallet
            // Placeholder implementation
            
            // Simulate API call
            await Task.Delay(300);
            
            // Create dummy transactions for demonstration
            var result = new List<BlockchainTransaction>
            {
                new BlockchainTransaction
                {
                    Hash = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
                    BlockNumber = 12345678,
                    Timestamp = DateTime.UtcNow.AddDays(-1),
                    From = walletAddress,
                    To = "0xd8da6bf26964af9d7eed9e03e53415d37aa96045",
                    Value = 0.1m,
                    GasUsed = 21000,
                    GasPrice = 20_000_000_000, // 20 Gwei
                    Data = "0x",
                    IsSuccessful = true
                },
                new BlockchainTransaction
                {
                    Hash = "0xabcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890",
                    BlockNumber = 12345670,
                    Timestamp = DateTime.UtcNow.AddDays(-2),
                    From = "0x06012c8cf97bead5deae237070f9587f8e7a266d",
                    To = walletAddress,
                    Value = 0.5m,
                    GasUsed = 21000,
                    GasPrice = 25_000_000_000, // 25 Gwei
                    Data = "0x",
                    IsSuccessful = true
                },
                new BlockchainTransaction
                {
                    Hash = "0x0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                    BlockNumber = 12345660,
                    Timestamp = DateTime.UtcNow.AddDays(-3),
                    From = walletAddress,
                    To = "0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48", // USDC contract
                    Value = 0,
                    GasUsed = 65000,
                    GasPrice = 30_000_000_000, // 30 Gwei
                    Data = "0xa9059cbb000000000000000000000000d8da6bf26964af9d7eed9e03e53415d37aa9604500000000000000000000000000000000000000000000000000000000000003e8", // transfer 1000 USDC
                    IsSuccessful = true
                }
            };
            
            return result.Take(Math.Min(limit, result.Count)).ToList();
        }

        /// <summary>
        /// Calls a contract method and returns the result
        /// </summary>
        public async Task<T> CallContractMethodAsync<T>(string contractAddress, string methodName, params object[] parameters)
        {
            if (!IsValidAddress(contractAddress))
                throw new ArgumentException("Invalid contract address", nameof(contractAddress));
                
            // In a real implementation, this would use Nethereum to call a contract method
            // Placeholder implementation
            Console.WriteLine($"Calling contract method: {methodName} on {contractAddress} with {parameters.Length} parameters");
            
            // Simulate contract call
            await Task.Delay(200);
            
            // Return a dummy value of type T
            return default;
        }
        
        /// <summary>
        /// Gets all yield positions for a wallet address
        /// </summary>
        public async Task<List<YieldPosition>> GetYieldPositionsAsync(string walletAddress)
        {
            if (!IsValidAddress(walletAddress))
                throw new ArgumentException("Invalid Ethereum address", nameof(walletAddress));
                
            // In a real implementation, this would use smart contract calls or subgraph queries
            // to get yield positions from various protocols
            // Placeholder implementation
            
            // Simulate API/contract calls
            await Task.Delay(400);
            
            // Create dummy yield positions for demonstration
            var aaveProtocol = new YieldProtocol("Aave", _blockchain, YieldProtocolType.LendingProtocol)
            {
                LogoUrl = "https://cryptologos.cc/logos/aave-aave-logo.png",
                Website = "https://aave.com",
                Description = "Aave is an open source and non-custodial liquidity protocol for earning interest on deposits and borrowing assets.",
                IsAudited = true,
                RiskScore = 2
            };
            
            var uniswapProtocol = new YieldProtocol("Uniswap", _blockchain, YieldProtocolType.LiquidityPool)
            {
                LogoUrl = "https://cryptologos.cc/logos/uniswap-uni-logo.png",
                Website = "https://uniswap.org",
                Description = "Uniswap is a protocol for automated token exchange on Ethereum.",
                IsAudited = true,
                RiskScore = 3
            };
            
            // Get token information
            var usdcToken = await GetTokenInfoAsync("0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48");
            
            // Create positions
            var aavePosition = new YieldPosition(walletAddress, aaveProtocol, "USDC Lending")
            {
                PoolAddress = "0x3d9819210a31b4961b30ef54be2aed79b9c9cd3b", // Lending Pool
                Apy = 3.5m,
                EntryTime = DateTime.UtcNow.AddDays(-30)
            };
            
            // Add deposited tokens
            aavePosition.AddToken(new TokenBalance(usdcToken, walletAddress, 1000));
            
            // Create Uniswap LP position
            var uniswapPosition = new YieldPosition(walletAddress, uniswapProtocol, "ETH-USDT LP")
            {
                PoolAddress = "0x0d4a11d5eeaac28ec3f61d100daf4d40471f1852", // ETH-USDT pair
                Apy = 15.0m,
                EntryTime = DateTime.UtcNow.AddDays(-60)
            };
            
            // Add deposited tokens
            uniswapPosition.AddToken(new TokenBalance(_blockchain.GetNativeToken(), walletAddress, 0.5m));
            uniswapPosition.AddToken(new TokenBalance(await GetTokenInfoAsync("0xdac17f958d2ee523a2206206994597c13d831ec7"), walletAddress, 1000));
            
            return new List<YieldPosition> { aavePosition, uniswapPosition };
        }
        
        /// <summary>
        /// Gets yield protocols available on this blockchain
        /// </summary>
        public async Task<List<YieldProtocol>> GetSupportedYieldProtocolsAsync()
        {
            // In a real implementation, this would return a list of supported protocols
            // Placeholder implementation
            
            // Simulate API call
            await Task.Delay(100);
            
            // Create dummy protocols
            return new List<YieldProtocol>
            {
                new YieldProtocol("Aave", _blockchain, YieldProtocolType.LendingProtocol)
                {
                    LogoUrl = "https://cryptologos.cc/logos/aave-aave-logo.png",
                    Website = "https://aave.com",
                    Description = "Aave is an open source and non-custodial liquidity protocol for earning interest on deposits and borrowing assets.",
                    IsAudited = true,
                    RiskScore = 2
                },
                new YieldProtocol("Uniswap", _blockchain, YieldProtocolType.LiquidityPool)
                {
                    LogoUrl = "https://cryptologos.cc/logos/uniswap-uni-logo.png",
                    Website = "https://uniswap.org",
                    Description = "Uniswap is a protocol for automated token exchange on Ethereum.",
                    IsAudited = true,
                    RiskScore = 3
                },
                new YieldProtocol("Yearn Finance", _blockchain, YieldProtocolType.Vault)
                {
                    LogoUrl = "https://cryptologos.cc/logos/yearn-finance-yfi-logo.png",
                    Website = "https://yearn.finance",
                    Description = "Yearn Finance is a suite of products in DeFi that optimizes yield farming through automation.",
                    IsAudited = true,
                    RiskScore = 4
                },
                new YieldProtocol("Lido", _blockchain, YieldProtocolType.Staking)
                {
                    LogoUrl = "https://cryptologos.cc/logos/lido-dao-ldo-logo.png",
                    Website = "https://lido.fi",
                    Description = "Lido is a liquid staking solution for Ethereum and other PoS blockchains.",
                    IsAudited = true,
                    RiskScore = 3
                }
            };
        }
        
        /// <summary>
        /// Checks if a wallet address is valid for this blockchain
        /// </summary>
        public bool IsValidAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return false;
                
            // Basic Ethereum address validation (starts with 0x and is 42 characters in length)
            return address.StartsWith("0x") && address.Length == 42 && IsHexString(address.Substring(2));
        }
        
        /// <summary>
        /// Gets the gas price in Gwei
        /// </summary>
        public async Task<decimal> GetGasPriceAsync()
        {
            try
            {
                var gasPriceWei = await _web3.Eth.GasPrice.SendRequestAsync();
                return (decimal)Web3.Convert.FromWei(gasPriceWei.Value, UnitConversion.EthUnit.Gwei);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting gas price: {ex.Message}");
                throw new Exception("Failed to get gas price", ex);
            }
        }
        
        /// <summary>
        /// Gets the current block number
        /// </summary>
        public async Task<long> GetBlockNumberAsync()
        {
            try
            {
                var blockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                return (long)blockNumber.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting block number: {ex.Message}");
                throw new Exception("Failed to get block number", ex);
            }
        }
        
        /// <summary>
        /// Discovers ERC20 tokens held by a wallet address
        /// </summary>
        public async Task<List<string>> DiscoverTokensAsync(string walletAddress)
        {
            if (!IsValidAddress(walletAddress))
                throw new ArgumentException("Invalid Ethereum address", nameof(walletAddress));
            
            try
            {
                return await _rateLimiter.ExecuteAsync("etherscan", async () =>
                {
                    using var httpClient = new HttpClient();
                    string apiUrl = $"{_settings.ExternalApis.EtherscanBaseUrl}?module=account&action=tokentx&address={walletAddress}&sort=asc&apikey={_settings.Blockchain.EtherscanApiKey}";
                    
                    // Get token transfer history to identify tokens
                    var response = await httpClient.GetStringAsync(apiUrl);
                    
                    // Extract token contract addresses
                    var uniqueTokens = new HashSet<string>();
                    
                    // Simple JSON parsing approach
                    // In production, use a proper JSON library
                    foreach (var line in response.Split('\n'))
                    {
                        if (line.Contains("\"contractAddress\":"))
                        {
                            var parts = line.Split(new[] { "\"contractAddress\":" }, StringSplitOptions.None);
                            if (parts.Length > 1)
                            {
                                var addressPart = parts[1].Trim();
                                var start = addressPart.IndexOf("\"") + 1;
                                var end = addressPart.IndexOf("\"", start);
                                
                                if (start >= 0 && end > start)
                                {
                                    var contractAddress = addressPart.Substring(start, end - start).Trim().ToLowerInvariant();
                                    if (!string.IsNullOrEmpty(contractAddress) && IsValidAddress(contractAddress))
                                    {
                                        uniqueTokens.Add(contractAddress);
                                    }
                                }
                            }
                        }
                    }
                    
                    return uniqueTokens.ToList();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error discovering tokens: {ex.Message}");
                throw new Exception("Failed to discover tokens", ex);
            }
        }
        
        /// <summary>
        /// Checks if a string contains only hexadecimal characters
        /// </summary>
        private bool IsHexString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
                
            foreach (char c in input)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Masks API keys in URLs or strings for secure logging
        /// </summary>
        private string MaskApiKey(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            // Check if it's an Infura URL
            if (input.Contains("infura.io"))
            {
                var parts = input.Split('/');
                if (parts.Length > 0)
                {
                    // Replace API key with masked version
                    parts[parts.Length - 1] = "****" + parts[parts.Length - 1].Substring(Math.Min(4, parts[parts.Length - 1].Length));
                    return string.Join("/", parts);
                }
            }
            
            // Handle other API keys
            if (input.Length > 6)
            {
                return input.Substring(0, 4) + "****" + input.Substring(input.Length - 2);
            }
            
            return "******";
        }
    }
} 