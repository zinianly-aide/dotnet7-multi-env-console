#!/bin/bash

# Configuration Validation Script
# Validates current environment configuration

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

# Color output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo "=========================================="
echo "Configuration Validation"
echo "=========================================="
echo ""

# Detect environment
if [ -f "$PROJECT_DIR/.env.production" ]; then
    ENV="Production"
elif [ -f "$PROJECT_DIR/.env.staging" ]; then
    ENV="Staging"
else
    ENV="Development"
fi

echo -e "${GREEN}Environment: $ENV${NC}"
echo ""

# Check required files
echo "Checking required files..."
REQUIRED_FILES=(
    "Dockerfile"
    "docker-compose.yml"
    "appsettings.json"
    ".env"
)

ALL_PRESENT=true
for file in "${REQUIRED_FILES[@]}"; do
    if [ -f "$PROJECT_DIR/$file" ]; then
        echo -e "${GREEN}✓${NC} $file"
    else
        echo -e "${RED}✗${NC} $file (missing)"
        ALL_PRESENT=false
    fi
done

# Check environment-specific files
echo ""
echo "Checking environment-specific files..."
ENV_FILES=(
    "appsettings.Development.json"
    "appsettings.Staging.json"
    "appsettings.Production.json"
)

for file in "${ENV_FILES[@]}"; do
    if [ -f "$PROJECT_DIR/MultiModuleApp.Console/$file" ]; then
        echo -e "${GREEN}✓${NC} MultiModuleApp.Console/$file"
    else
        echo -e "${YELLOW}⚠${NC} MultiModuleApp.Console/$file (optional)"
    fi
done

# Validate appsettings.json structure
echo ""
echo "Validating appsettings.json structure..."
if command -v jq &> /dev/null; then
    # Check for required sections
    REQUIRED_SECTIONS=(
        "ConnectionStrings"
        "Logging"
        "Serilog"
        "OpenTelemetry"
        "Audit"
    )

    for section in "${REQUIRED_SECTIONS[@]}"; do
        if jq -e ".${section}" "$PROJECT_DIR/MultiModuleApp.Console/appsettings.json" > /dev/null 2>&1; then
            echo -e "${GREEN}✓${NC} $section section found"
        else
            echo -e "${RED}✗${NC} $section section missing"
            ALL_PRESENT=false
        fi
    done
else
    echo -e "${YELLOW}⚠${NC} jq not installed, skipping JSON validation"
fi

# Validate Dockerfile
echo ""
echo "Validating Dockerfile..."
if grep -q "FROM mcr.microsoft.com/dotnet" "$PROJECT_DIR/Dockerfile"; then
    echo -e "${GREEN}✓${NC} Valid .NET base image"
else
    echo -e "${RED}✗${NC} Invalid or missing .NET base image"
    ALL_PRESENT=false
fi

# Validate .NET SDK availability
echo ""
echo "Validating .NET SDK availability..."
if command -v dotnet &> /dev/null; then
    SDK_VERSION=$(dotnet --version 2>/dev/null || echo "unknown")
    echo -e "${GREEN}✓${NC} .NET SDK found (version: $SDK_VERSION)"
else
    echo -e "${RED}✗${NC} .NET SDK not found"
    ALL_PRESENT=false
fi

# Summary
echo ""
echo "=========================================="
if [ "$ALL_PRESENT" = true ]; then
    echo -e "${GREEN}✓ All critical configurations are valid${NC}"
    exit 0
else
    echo -e "${RED}✗ Some configurations are missing or invalid${NC}"
    exit 1
fi
