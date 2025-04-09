# DeFi Portfolio Manager

<div align="center">
  <h3>Multi-Chain | Yield Tracker | C# Backend | React Frontend</h3>
  <p>
    <b>A comprehensive DeFi portfolio tracking platform for modern crypto investors</b>
  </p>
  
  <p>
    <a href="#demo">View Demo</a>
    ·
    <a href="#installation">Installation</a>
    ·
    <a href="#usage">Usage</a>
    ·
    <a href="https://github.com/Dipraise1/DeFi-Portfolio-Manager-cHASH/issues">Report Bug</a>
  </p>
</div>

## Overview

DeFi Portfolio Manager is a robust application designed to help users track and analyze their crypto assets across multiple blockchains. Leveraging a modular C# backend with a React frontend, the platform provides real-time data on token balances, current valuations, and yield metrics from various DeFi protocols.

### Key Capabilities

- **Portfolio Tracking**: Aggregate all your crypto assets across multiple chains in one dashboard
- **Yield Monitoring**: Track APY/APR from various protocols and liquidity pools  
- **Performance Analytics**: View historical performance data and projected returns
- **Multi-Chain Support**: Track assets across Ethereum, Polygon, BSC and more

<div align="center">
  <p><i>Dashboard showing portfolio performance and yield positions</i></p>
</div>

## Core Features

- 🔗 **Multi-Chain Support** - Ethereum, Polygon, BSC, and more
- 👛 **Wallet Connect Integration** - Seamless login with MetaMask/WalletConnect
- 📊 **Token Balance Tracker** - ERC-20s & native coins with real-time valuations
- 💹 **Yield/APR Display** - Current and historical yield from DeFi positions
- 💱 **Token Price Conversion** - USD, EUR, and other fiat currencies
- 📈 **Performance Insights** - Total value, ROI, and projected yields
- 🔒 **Secure Caching** - Redis-powered caching with configurable expiry

## Tech Stack

| Layer | Technologies |
|-------|-------------|
| Frontend | React.js, Chart.js, Web3 React |
| Backend | C# / ASP.NET Core 6.0 |
| Blockchain | Nethereum, Web3 APIs |
| Price Feeds | CoinGecko API, Chainlink, 1inch API |
| DeFi Protocols | Aave, Uniswap, Yearn, Beefy, etc. |
| Wallet Auth | MetaMask / WalletConnect + SIWE |
| Caching | Redis (production), In-memory (development) |
| Deployment | Docker, Docker Compose, Nginx, Certbot |

## Project Structure

```
DefiPortfolioManager/
├── src/
│   ├── Api/               # ASP.NET Core Web API
│   ├── Core/              # Domain models, interfaces, business logic
│   ├── Infrastructure/    # External service implementations, DB context
│   └── Services/          # Application services, business logic implementation
├── tests/                 # Unit and integration tests
├── ClientApp/             # React frontend application
├── docker-compose.yml     # Development Docker configuration
└── docker-compose.prod.yml # Production Docker configuration
```

## Installation

### Prerequisites

- .NET 6.0 SDK or later
- Node.js 16+ and npm
- Docker and Docker Compose
- Git

### Quick Start (Docker)

1. Clone the repository:
   ```bash
   git clone https://github.com/Dipraise1/DeFi-Portfolio-Manager-cHASH.git
   cd DeFi-Portfolio-Manager-cHASH
   ```

2. Run the application with Docker Compose:
   ```bash
   docker-compose up -d
   ```

3. Access the application:
   - Frontend: http://localhost:3001
   - API: http://localhost:5001/api
   - Swagger UI: http://localhost:5001/swagger

### Manual Setup

1. Start Redis for caching:
   ```bash
   docker run -d -p 6379:6379 redis:alpine
   ```

2. Build and run the backend:
   ```bash
   dotnet restore
   dotnet build
   cd src/Api
   dotnet run
   ```

3. Run the frontend:
   ```bash
   cd ClientApp
   npm install
   npm start
   ```

## Production Deployment

### One-Command Deployment

Use our deployment script for a fully automated setup:

```bash
./deploy.sh yourdomain.com your@email.com
```

This will:
- Configure Nginx for your domain
- Obtain SSL certificates using Certbot
- Start all services in a secure Docker environment
- Make your application accessible via HTTPS

### Manual Deployment

For manual deployment, use the production Docker Compose file:

```bash
# Configure environment variables
cp .env.example .env
nano .env  # Edit with your settings

# Deploy the application
docker-compose -f docker-compose.prod.yml up -d
```

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /api/health` | Health check for API services |
| `GET /api/blockchain/supported` | List supported blockchains |
| `GET /api/portfolio/{address}` | Get portfolio for a wallet address |
| `GET /api/yield/protocols` | List supported yield protocols |

Complete API documentation is available via Swagger UI when running the application.

## Caching Strategy

The application implements a sophisticated caching strategy:

- **Memory Cache**: Used in development for quick iterations
- **Redis Cache**: Used in production for distributed caching
- **Tiered Expiration**: Configurable expiry times based on data type:
  - Price data: 1-5 minutes (frequently changing)
  - Token list data: 12-48 hours (relatively static)
  - Portfolio data: 5-10 minutes (balance between performance and accuracy)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [CoinGecko API](https://www.coingecko.com/en/api) for token pricing data
- [Nethereum](https://nethereum.com/) for Ethereum integration
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core) for backend framework
