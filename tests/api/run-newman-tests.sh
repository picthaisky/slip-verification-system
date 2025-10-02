#!/bin/bash

# Newman Test Runner Script
# Runs Postman collections using Newman for CI/CD automation

set -e

# Configuration
COLLECTION_FILE="${COLLECTION_FILE:-tests/api/postman-collection.json}"
ENVIRONMENT_FILE="${ENVIRONMENT_FILE:-tests/api/postman-environment.json}"
BASE_URL="${BASE_URL:-http://localhost:5000/api/v1}"
REPORTERS="${REPORTERS:-cli,json,html}"
REPORT_DIR="${REPORT_DIR:-./test-results/api}"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== Newman API Test Runner ===${NC}"
echo "Collection: $COLLECTION_FILE"
echo "Base URL: $BASE_URL"
echo ""

# Create report directory
mkdir -p "$REPORT_DIR"

# Check if Newman is installed
if ! command -v newman &> /dev/null; then
    echo -e "${YELLOW}Newman is not installed. Installing...${NC}"
    npm install -g newman newman-reporter-htmlextra
fi

# Check if collection file exists
if [ ! -f "$COLLECTION_FILE" ]; then
    echo -e "${RED}Error: Collection file not found: $COLLECTION_FILE${NC}"
    exit 1
fi

# Check if target is reachable
echo -e "${YELLOW}Checking if API is reachable...${NC}"
if ! curl -s -o /dev/null -w "%{http_code}" "${BASE_URL%/api/v1}/health" | grep -q "200\|404"; then
    echo -e "${RED}Warning: API may not be reachable at $BASE_URL${NC}"
    echo "Please ensure the API is running"
    read -p "Continue anyway? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

# Run Newman tests
echo -e "${GREEN}Running API tests with Newman...${NC}"
echo ""

# Build Newman command
NEWMAN_CMD="newman run \"$COLLECTION_FILE\""

# Add environment file if it exists
if [ -f "$ENVIRONMENT_FILE" ]; then
    NEWMAN_CMD="$NEWMAN_CMD -e \"$ENVIRONMENT_FILE\""
fi

# Add global variables
NEWMAN_CMD="$NEWMAN_CMD --global-var \"baseUrl=$BASE_URL\""

# Add reporters
NEWMAN_CMD="$NEWMAN_CMD --reporters $REPORTERS"
NEWMAN_CMD="$NEWMAN_CMD --reporter-json-export \"$REPORT_DIR/newman-report.json\""
NEWMAN_CMD="$NEWMAN_CMD --reporter-html-export \"$REPORT_DIR/newman-report.html\""

# Add timeout and iterations
NEWMAN_CMD="$NEWMAN_CMD --timeout-request 10000"
NEWMAN_CMD="$NEWMAN_CMD --bail"

# Execute Newman
eval $NEWMAN_CMD

echo ""
echo -e "${GREEN}=== API Tests Complete ===${NC}"
echo "Reports saved to: $REPORT_DIR"
echo ""

# Display summary
if [ -f "$REPORT_DIR/newman-report.json" ]; then
    echo -e "${YELLOW}Test Summary:${NC}"
    echo "View detailed report at: $REPORT_DIR/newman-report.html"
fi

echo ""
