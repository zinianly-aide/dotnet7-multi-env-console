#!/bin/bash

# Environment Switcher Script
# Usage: ./scripts/switch-env.sh [development|staging|production]

set -e

ENV=$1
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

# Color output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

if [ -z "$ENV" ]; then
    echo -e "${RED}Error: Environment not specified${NC}"
    echo "Usage: $0 [development|staging|production]"
    exit 1
fi

case $ENV in
    development|dev)
        ENV_FILE=".env"
        COMPOSE_FILE="docker-compose.yml"
        echo -e "${GREEN}Switching to Development environment${NC}"
        ;;
    staging)
        ENV_FILE=".env.staging"
        COMPOSE_FILE="docker-compose.staging.yml"
        echo -e "${YELLOW}Switching to Staging environment${NC}"
        ;;
    production|prod)
        ENV_FILE=".env.production"
        COMPOSE_FILE="docker-compose.prod.yml"
        echo -e "${RED}Switching to Production environment${NC}"
        ;;
    *)
        echo -e "${RED}Error: Invalid environment '$ENV'${NC}"
        echo "Valid environments: development, staging, production"
        exit 1
        ;;
esac

# Change to project directory
cd "$PROJECT_DIR"

# Check if env file exists
if [ ! -f "$ENV_FILE" ]; then
    echo -e "${YELLOW}Warning: $ENV_FILE not found, creating from example${NC}"
    cp .env.example "$ENV_FILE"
    echo -e "${GREEN}Please update $ENV_FILE with your actual configuration${NC}"
fi

# Stop any running containers
echo "Stopping existing containers..."
docker-compose -f "$COMPOSE_FILE" down 2>/dev/null || true

# Build and start with new environment
echo "Building and starting containers..."
docker-compose -f "$COMPOSE_FILE" --env-file="$ENV_FILE" up -d --build

# Show status
echo ""
echo -e "${GREEN}Environment switched to $ENV${NC}"
echo "Compose file: $COMPOSE_FILE"
echo "Environment file: $ENV_FILE"
echo ""
echo "Running containers:"
docker-compose -f "$COMPOSE_FILE" ps
