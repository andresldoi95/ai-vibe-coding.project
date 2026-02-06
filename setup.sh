#!/bin/bash

# Setup script for Linux/macOS
# Run this script to initialize the frontend project

echo "🚀 SaaS Billing & Inventory - Frontend Setup"
echo "============================================="
echo ""

# Check Node.js version
echo "Checking Node.js version..."
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed!"
    echo "Please install Node.js 20+ from https://nodejs.org/"
    exit 1
fi

NODE_VERSION=$(node --version)
echo "✅ Node.js version: $NODE_VERSION"

# Navigate to frontend directory
cd frontend

# Check if node_modules exists
if [ -d "node_modules" ]; then
    echo "📦 node_modules already exists"
    read -p "Do you want to reinstall dependencies? (y/N): " response
    if [[ "$response" =~ ^[Yy]$ ]]; then
        echo "Removing node_modules..."
        rm -rf node_modules
        echo "Installing dependencies..."
        npm install
    fi
else
    echo "📦 Installing dependencies..."
    npm install
    if [ $? -ne 0 ]; then
        echo "❌ Failed to install dependencies!"
        exit 1
    fi
    echo "✅ Dependencies installed successfully"
fi

# Create .env file if it doesn't exist
if [ ! -f ".env" ]; then
    echo "📝 Creating .env file..."
    cp .env.example .env
    echo "✅ .env file created"
else
    echo "✅ .env file already exists"
fi

echo ""
echo "🎉 Setup complete!"
echo ""
echo "Next steps:"
echo "  1. Review and update the .env file if needed"
echo "  2. Run 'npm run dev' to start the development server"
echo "  3. Open http://localhost:3000 in your browser"
echo ""
echo "Or use Docker:"
echo "  cd .."
echo "  docker-compose up"
echo ""

# Ask if user wants to start dev server
read -p "Start development server now? (y/N): " startDev
if [[ "$startDev" =~ ^[Yy]$ ]]; then
    echo ""
    echo "🚀 Starting development server..."
    echo ""
    npm run dev
fi
