#!/bin/sh
set -eu

cd "$(dirname "$0")"

if [ "$#" -ne 3 ]; then
    echo "Usage: sh run-mac.sh <file.jpg> <squareSize> <S|M>"
    exit 1
fi

APP_DIR=".runtime/mac-arm64"
APP="$APP_DIR/ImagePixelation"

if [ ! -f "$APP" ]; then
    mkdir -p "$APP_DIR"

    URL="https://github.com/usuleymanli16281/advanced-software-paradigms-a4/releases/download/v1.0.0/ImagePixelation-macOS-ARM64.zip"

    echo "Downloading macOS application..."
    curl -fL "$URL" -o "$APP_DIR/app.zip"

    echo "Extracting application..."
    unzip -oq "$APP_DIR/app.zip" -d "$APP_DIR"
    rm "$APP_DIR/app.zip"
fi

chmod +x "$APP"

exec "$APP" "$@"
