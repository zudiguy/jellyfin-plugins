#!/bin/bash

# JellyTag Build Script

set -e

echo "Building JellyTag Plugin..."

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK is not installed"
    echo "Please install .NET 9.0 SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

# Clean previous builds
echo "Cleaning previous builds..."
dotnet clean Jellyfin.Plugin.JellyTag/Jellyfin.Plugin.JellyTag.csproj --configuration Release

# Build the plugin
echo "Building plugin..."
dotnet build Jellyfin.Plugin.JellyTag/Jellyfin.Plugin.JellyTag.csproj --configuration Release

# Display output location
echo ""
echo "Build complete!"
echo "Plugin DLL location: Jellyfin.Plugin.JellyTag/bin/Release/net9.0/Jellyfin.Plugin.JellyTag.dll"
echo ""
echo "To install:"
echo "1. Copy the DLL to your Jellyfin plugins folder"
echo "2. Restart Jellyfin"
echo "3. Navigate to Dashboard → Plugins → JellyTag to configure"
