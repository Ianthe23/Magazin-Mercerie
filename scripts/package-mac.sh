#!/usr/bin/env bash
set -euo pipefail

APP_NAME="magazin-mercerie"
RUNTIME="osx-arm64" # change to osx-x64 for Intel Macs
PUBLISH_DIR="bin/Release/net9.0/${RUNTIME}/publish"
DIST_DIR="dist/macos"
APP_DIR="${DIST_DIR}/${APP_NAME}.app"
CONTENTS="${APP_DIR}/Contents"
MACOS_DIR="${CONTENTS}/MacOS"
RES_DIR="${CONTENTS}/Resources"

# Build self-contained single-file for macOS
dotnet publish "${APP_NAME}.csproj" -c Release -r "${RUNTIME}" \
  -p:PublishSingleFile=true -p:SelfContained=true -p:IncludeNativeLibrariesForSelfExtract=true

# Create bundle structure
rm -rf "${APP_DIR}"
mkdir -p "${MACOS_DIR}" "${RES_DIR}"

# Copy executable
cp "${PUBLISH_DIR}/${APP_NAME}" "${MACOS_DIR}/${APP_NAME}"
chmod +x "${MACOS_DIR}/${APP_NAME}"

# Copy Info.plist
cp Packaging/macos/Info.plist "${CONTENTS}/Info.plist"

# Optional: convert .ico to .icns or provide a prebuilt .icns
# If you have avalonia-logo.icns, copy it:
# cp Assets/avalonia-logo.icns "${RES_DIR}/avalonia-logo.icns"

echo "App bundle created at ${APP_DIR}"
echo "Zip with: (cd dist/macos && zip -r ${APP_NAME}-macOS.zip ${APP_NAME}.app)"