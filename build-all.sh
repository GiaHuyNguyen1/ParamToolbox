#!/bin/bash
# ==============================================================================
# ParamToolbox - Automated Multi-Platform Build & Packaging Script
# Platforms: Windows x64, macOS ARM64 (Apple Silicon), macOS x64 (Intel)
# ==============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

OUTPUT_BASE="$SCRIPT_DIR/publish"
DIST_DIR="$SCRIPT_DIR/dist"

echo "=========================================================="
echo "🚀 BẮT ĐẦU ĐÓNG GÓI PARAMTOOLBOX ĐA NỀN TẢNG"
echo "=========================================================="

# 1. Clean previous build outputs
echo "🧹 Đang dọn dẹp thư mục build cũ..."
rm -rf "$OUTPUT_BASE" "$DIST_DIR"
mkdir -p "$DIST_DIR"

# Dọn dẹp metadata rác macOS
if command -v dot_clean >/dev/null 2>&1; then
    dot_clean "$SCRIPT_DIR" >/dev/null 2>&1 || true
fi

# 2. Build Windows x64
echo ""
echo "📦 [1/3] Đang build cho Windows (win-x64)..."
dotnet publish ParamToolbox.csproj \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o "$OUTPUT_BASE/win-x64"

# Tạo launcher cho Windows
cat << 'EOF' > "$OUTPUT_BASE/win-x64/Run_Tool.bat"
@echo off
title ParamToolbox
cls
"%~dp0ParamToolbox.exe"
pause
EOF

# 3. Build macOS Apple Silicon (osx-arm64)
echo ""
echo "📦 [2/3] Đang build cho macOS Apple Silicon M-series (osx-arm64)..."
dotnet publish ParamToolbox.csproj \
    -c Release \
    -r osx-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o "$OUTPUT_BASE/osx-arm64"

# Tạo launcher cho Mac ARM64
cat << 'EOF' > "$OUTPUT_BASE/osx-arm64/Run_Tool.command"
#!/bin/bash
DIR="$(cd "$(dirname "$0")" && pwd)"
chmod +x "$DIR/ParamToolbox"
"$DIR/ParamToolbox"
EOF
chmod +x "$OUTPUT_BASE/osx-arm64/Run_Tool.command"
chmod +x "$OUTPUT_BASE/osx-arm64/ParamToolbox"

# 4. Build macOS Intel (osx-x64)
echo ""
echo "📦 [3/3] Đang build cho macOS Intel (osx-x64)..."
dotnet publish ParamToolbox.csproj \
    -c Release \
    -r osx-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -o "$OUTPUT_BASE/osx-x64"

# Tạo launcher cho Mac x64
cat << 'EOF' > "$OUTPUT_BASE/osx-x64/Run_Tool.command"
#!/bin/bash
DIR="$(cd "$(dirname "$0")" && pwd)"
chmod +x "$DIR/ParamToolbox"
"$DIR/ParamToolbox"
EOF
chmod +x "$OUTPUT_BASE/osx-x64/Run_Tool.command"
chmod +x "$OUTPUT_BASE/osx-x64/ParamToolbox"

# 5. Nén file ZIP phân phối
echo ""
echo "🗜️  Đang nén các gói phát hành vào thư mục 'dist/'..."

cd "$OUTPUT_BASE/win-x64"
zip -q -r "$DIST_DIR/ParamToolbox_Win64.zip" ./*

cd "$OUTPUT_BASE/osx-arm64"
zip -q -r "$DIST_DIR/ParamToolbox_MacArm64.zip" ./*

cd "$OUTPUT_BASE/osx-x64"
zip -q -r "$DIST_DIR/ParamToolbox_MacX64.zip" ./*

cd "$SCRIPT_DIR"

echo ""
echo "=========================================================="
echo "✅ HOÀN TẤT ĐÓNG GÓI! Các file ZIP đã được tạo trong:"
echo "📁 $DIST_DIR"
ls -lh "$DIST_DIR"
echo "=========================================================="
