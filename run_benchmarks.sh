#!/bin/bash

# Sannr Benchmark Script
# This script runs performance benchmarks for Sannr validation library

echo "Running Sannr Validation Benchmarks..."
echo "======================================"

# Get machine info
echo "Machine Configuration:"
echo "CPU: $(sysctl -n machdep.cpu.brand_string)"
echo "RAM: $(echo "$(sysctl -n hw.memsize) / 1024 / 1024 / 1024" | bc) GB"
echo "OS: $(sw_vers -productName) $(sw_vers -productVersion)"
echo ".NET Version: $(dotnet --version)"
echo ""

# Run benchmarks
cd /Users/digvijay/source/github/Sannr
dotnet run --project src/Sannr.Benchmarks/ --configuration Release

echo ""
echo "Benchmark complete. Results shown above."