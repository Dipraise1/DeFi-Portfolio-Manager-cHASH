using System;
using System.Threading.Tasks;
using DefiPortfolioManager.Core.Interfaces;
using DefiPortfolioManager.Core.Models;
using DefiPortfolioManager.Core.Settings;
using DefiPortfolioManager.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DefiPortfolioManager.Infrastructure.Tests
{
    public class EthereumBlockchainServiceTests
    {
        private readonly Mock<IPriceService> _mockPriceService;
        private readonly Mock<IOptions<AppSettings>> _mockOptions;
        private readonly AppSettings _settings;
        private readonly EthereumBlockchainService _service;

        public EthereumBlockchainServiceTests()
        {
            _mockPriceService = new Mock<IPriceService>();
            
            // Set up mock settings
            _settings = new AppSettings
            {
                Blockchain = new BlockchainSettings
                {
                    EthereumRpcUrl = "https://mainnet.infura.io/v3/test-key",
                    EtherscanApiKey = "test-etherscan-key"
                },
                ExternalApis = new ExternalApiSettings
                {
                    EtherscanBaseUrl = "https://api.etherscan.io/api"
                }
            };
            
            _mockOptions = new Mock<IOptions<AppSettings>>();
            _mockOptions.Setup(m => m.Value).Returns(_settings);
            
            // Create service with mocks
            _service = new EthereumBlockchainService(_mockPriceService.Object, _mockOptions.Object);
        }

        [Fact]
        public void SupportsBlockchain_ReturnsTrue_ForEthereum()
        {
            // Act
            var result = _service.SupportsBlockchain(BlockchainType.Ethereum);
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public void SupportsBlockchain_ReturnsFalse_ForNonEthereum()
        {
            // Act
            var result = _service.SupportsBlockchain(BlockchainType.Solana);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void SupportedBlockchain_ReturnsEthereum()
        {
            // Act
            var blockchain = _service.SupportedBlockchain;
            
            // Assert
            Assert.Equal(BlockchainType.Ethereum, blockchain.Type);
            Assert.Equal("ETH", blockchain.NativeCurrencySymbol);
            Assert.Equal(18, blockchain.NativeCurrencyDecimals);
        }
        
        [Theory]
        [InlineData("0x742d35Cc6634C0532925a3b844Bc454e4438f44e", true)]
        [InlineData("0x742d35cc6634C0532925a3b844Bc454e4438f44e", true)] // Case insensitive
        [InlineData("742d35Cc6634C0532925a3b844Bc454e4438f44e", false)] // Missing 0x prefix
        [InlineData("0x742d35Cc6634C0532925a3b844Bc454e4438f44", false)] // Too short
        [InlineData("0x742d35Cc6634C0532925a3b844Bc454e4438f44ef", false)] // Too long
        [InlineData("0x742d35Cc6634C0532925a3b844Bc454e4438f44g", false)] // Invalid character
        [InlineData("", false)] // Empty string
        [InlineData(null, false)] // Null
        public void IsValidAddress_ValidatesCorrectly(string address, bool expected)
        {
            // Act
            var result = _service.IsValidAddress(address);
            
            // Assert
            Assert.Equal(expected, result);
        }
        
        [Fact]
        public async Task GetTokenInfoAsync_ThrowsException_ForInvalidAddress()
        {
            // Arrange
            var invalidAddress = "not-an-address";
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.GetTokenInfoAsync(invalidAddress));
        }
        
        [Fact]
        public async Task GetTokenBalanceAsync_ThrowsException_ForInvalidWalletAddress()
        {
            // Arrange
            var invalidWalletAddress = "not-an-address";
            var validTokenAddress = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e";
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.GetTokenBalanceAsync(invalidWalletAddress, validTokenAddress));
        }
        
        [Fact]
        public async Task GetTokenBalanceAsync_ThrowsException_ForInvalidTokenAddress()
        {
            // Arrange
            var validWalletAddress = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e";
            var invalidTokenAddress = "not-an-address";
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.GetTokenBalanceAsync(validWalletAddress, invalidTokenAddress));
        }
        
        [Fact]
        public async Task GetAllTokenBalancesAsync_ThrowsException_ForInvalidAddress()
        {
            // Arrange
            var invalidAddress = "not-an-address";
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.GetAllTokenBalancesAsync(invalidAddress));
        }
    }
} 