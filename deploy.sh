#!/bin/bash

# Exit on error
set -e

# Colors for terminal output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== DeFi Portfolio Manager Deployment Script ===${NC}"

# Check if domain is provided
if [ -z "$1" ]; then
  echo -e "${RED}Error: Domain name is required.${NC}"
  echo "Usage: ./deploy.sh yourdomain.com [email@example.com]"
  exit 1
fi

DOMAIN=$1
EMAIL=${2:-"admin@$DOMAIN"}

# Update domain in Nginx config
echo -e "${YELLOW}Configuring Nginx for domain: $DOMAIN${NC}"
sed -i "s/portfolio.example.com/$DOMAIN/g" nginx/conf/app.conf

# Create required directories
mkdir -p nginx/certbot/conf
mkdir -p nginx/certbot/www

# Create docker-compose.override.yml with environment variables
cat > docker-compose.override.yml << EOL
version: '3.8'

services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - REDIS_PASSWORD=${REDIS_PASSWORD:-strongpassword}
EOL

# Check if docker and docker-compose are installed
if ! command -v docker &> /dev/null; then
  echo -e "${RED}Docker is not installed. Please install Docker first.${NC}"
  exit 1
fi

if ! command -v docker-compose &> /dev/null; then
  echo -e "${RED}Docker Compose is not installed. Please install Docker Compose first.${NC}"
  exit 1
fi

# Initial deployment without SSL
echo -e "${YELLOW}Starting initial deployment to obtain SSL certificates...${NC}"
docker-compose -f docker-compose.prod.yml up -d nginx

# Get SSL certificates
echo -e "${YELLOW}Obtaining SSL certificates with Certbot...${NC}"
docker-compose -f docker-compose.prod.yml run --rm certbot certonly --webroot --webroot-path=/var/www/certbot --email $EMAIL --agree-tos --no-eff-email -d $DOMAIN

# Reload nginx to apply certificates
echo -e "${YELLOW}Reloading Nginx to apply SSL certificates...${NC}"
docker-compose -f docker-compose.prod.yml exec nginx nginx -s reload

# Start all services
echo -e "${YELLOW}Starting all services...${NC}"
docker-compose -f docker-compose.prod.yml up -d

echo -e "${GREEN}Deployment completed successfully!${NC}"
echo -e "Your DeFi Portfolio Manager is now running at https://$DOMAIN"
echo -e "${YELLOW}Note: It may take a few minutes for the services to fully start.${NC}" 