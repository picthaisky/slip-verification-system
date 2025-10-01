#!/bin/bash

###############################################################################
# OWASP ZAP Security Scan Script
# 
# This script runs automated security testing using OWASP ZAP
###############################################################################

set -e

# Configuration
TARGET_URL="${TARGET_URL:-http://localhost:5000}"
ZAP_PORT="${ZAP_PORT:-8080}"
REPORT_DIR="./security-reports"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}OWASP ZAP Security Testing${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo "Target: $TARGET_URL"
echo "Report Directory: $REPORT_DIR"
echo ""

# Create report directory
mkdir -p "$REPORT_DIR"

# Check if Docker is available
if ! command -v docker &> /dev/null; then
    echo -e "${RED}Error: Docker is not installed${NC}"
    exit 1
fi

# Check if target is reachable
echo -e "${YELLOW}Checking if target is reachable...${NC}"
if ! curl -s -o /dev/null -w "%{http_code}" "$TARGET_URL/health" | grep -q "200\|401"; then
    echo -e "${RED}Warning: Target may not be reachable at $TARGET_URL${NC}"
fi

echo ""
echo -e "${GREEN}Running ZAP Baseline Scan...${NC}"
echo "This will test for common vulnerabilities"
echo ""

# Run ZAP baseline scan
docker run --rm \
    --network host \
    -v "$PWD/$REPORT_DIR:/zap/wrk:rw" \
    owasp/zap2docker-stable zap-baseline.py \
    -t "$TARGET_URL" \
    -r "baseline-report-${TIMESTAMP}.html" \
    -J "baseline-report-${TIMESTAMP}.json" \
    -w "baseline-report-${TIMESTAMP}.md" \
    -d

echo ""
echo -e "${GREEN}Running ZAP Full Scan...${NC}"
echo "This will perform a comprehensive security test (may take longer)"
echo ""

# Run ZAP full scan
docker run --rm \
    --network host \
    -v "$PWD/$REPORT_DIR:/zap/wrk:rw" \
    owasp/zap2docker-stable zap-full-scan.py \
    -t "$TARGET_URL" \
    -r "full-report-${TIMESTAMP}.html" \
    -J "full-report-${TIMESTAMP}.json" \
    -w "full-report-${TIMESTAMP}.md" \
    -d

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Security Scan Complete${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo "Reports generated in: $REPORT_DIR"
echo "  - baseline-report-${TIMESTAMP}.html"
echo "  - baseline-report-${TIMESTAMP}.json"
echo "  - full-report-${TIMESTAMP}.html"
echo "  - full-report-${TIMESTAMP}.json"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Review the HTML reports in a browser"
echo "2. Address any High or Critical vulnerabilities"
echo "3. Document findings and remediation plans"
echo ""
