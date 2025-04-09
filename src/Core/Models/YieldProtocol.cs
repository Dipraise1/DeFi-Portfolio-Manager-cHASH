namespace DefiPortfolioManager.Core.Models
{
    public class YieldProtocol
    {
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public Blockchain Blockchain { get; set; }
        public string Website { get; set; }
        public YieldProtocolType Type { get; set; }
    }

    public enum YieldProtocolType
    {
        LendingProtocol,
        LiquidityPool,
        Vault,
        Staking,
        FarmingPool
    }
} 