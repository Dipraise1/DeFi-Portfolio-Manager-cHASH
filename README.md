# DeFi Portfolio Manager

Multi-Chain | Yield Tracker | C# Backend | React Frontend

## Overview

DeFi Portfolio Manager is a comprehensive platform that allows users to view, track, and analyze their crypto assets across multiple blockchains (Ethereum, Polygon, BNB Chain, etc.). The platform displays token balances, current token values (in USD), and real-time yield/APR from popular LPs (liquidity pools) and vaults.

## Core Features

- 🔗 Multi-Chain Support (Ethereum, Polygon, BSC, etc.)
- 👛 Wallet Connect (MetaMask/WalletConnect)
- 📊 Token Balance Tracker (ERC-20s & native coins)
- 💹 Yield/APR Display from LPs and vaults
- 💱 Token Price Conversion (USD, EUR, etc.)
- 📈 Performance Insights (total value, ROI, projected yields)
- 🔒 Local Portfolio Caching + Secure Session Storage

## Tech Stack

| Layer | Tech |
|-------|------|
| Frontend | React.js with Chart.js |
| Backend | C# / ASP.NET Core |
| Blockchain | Nethereum, Web3 APIs |
| Price Feeds | CoinGecko API / Chainlink / 1inch API |
| DeFi Protocols | Aave, Uniswap, Yearn, Beefy, etc. |
| Wallet Auth | MetaMask / WalletConnect + SIWE |
| Storage | Redis (for caching) |

## Project Structure

```
DefiPortfolioManager/
├── src/
│   ├── Api/               # ASP.NET Core Web API
│   ├── Core/              # Domain models, interfaces, business logic
│   ├── Infrastructure/    # External services implementation, DB context
│   └── Services/          # Application services, business logic implementation
├── tests/                 # Unit and integration tests
└── ClientApp/             # React frontend application
```

## Local Development Setup

### Prerequisites

- .NET 6.0 SDK or later
- Node.js 16 or later & npm
- Docker and Docker Compose
- Git

### Running Locally with Docker

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/defi-portfolio-manager.git
   cd defi-portfolio-manager
   ```

2. Run the application using Docker Compose:
   ```bash
   docker-compose up -d
   ```

3. Access the application:
   - Frontend: http://localhost:3001
   - API: http://localhost:5001/api
   - Swagger UI: http://localhost:5001/swagger

### Running Locally Without Docker

1. Start Redis (required for caching):
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

### Deploying to a VPS or Cloud Server

1. Provision a server with Docker and Docker Compose installed
2. Clone the repository to your server
3. Set up environment variables:
   ```bash
   export REDIS_PASSWORD=your_secure_password
   ```

4. Run the deployment script with your domain:
   ```bash
   ./deploy.sh yourdomain.com your@email.com
   ```

### Manual Deployment Steps

If you prefer to deploy manually:

1. Configure your domain in `nginx/conf/app.conf`

2. Create the necessary directories:
   ```bash
   mkdir -p nginx/certbot/conf nginx/certbot/www
   ```

3. Deploy using the production compose file:
   ```bash
   docker-compose -f docker-compose.prod.yml up -d
   ```

## Maintenance

- SSL certificates are auto-renewed by Certbot
- To update the application:
  ```bash
  git pull
  docker-compose -f docker-compose.prod.yml build
  docker-compose -f docker-compose.prod.yml up -d
  ```

## Caching Strategy

The application implements a flexible caching strategy to improve performance:

- **Memory Cache**: For development and testing
- **Redis Cache**: For production
- **Configurable Expiry**: Different cache expiry times for different data types:
  - Price data: Short expiry (1-5 minutes)
  - Token list data: Longer expiry (12-48 hours)
  - Portfolio data: Medium expiry (5-10 minutes)

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. # DeFi-Portfolio-Manager-cHASH
