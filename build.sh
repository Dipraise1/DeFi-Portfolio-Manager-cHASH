#!/bin/bash

# Exit on error
set -e

echo "=== Building DeFi Portfolio Manager ==="

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Build solution
echo "Building solution..."
dotnet build --configuration Release --no-restore

# Run tests
echo "Running tests..."
dotnet test --configuration Release --no-build

# Publish API
echo "Publishing API..."
dotnet publish src/Api/Api.csproj --configuration Release --no-build -o ./publish

echo "=== Build completed successfully ==="
echo "To run the application: dotnet ./publish/DefiPortfolioManager.Api.dll"
echo "To run with Docker: docker-compose up -d" 