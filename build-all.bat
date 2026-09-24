@echo off
REM ==============================================================================
REM ParamToolbox - Automated Multi-Platform Build & Packaging Script for Windows
REM ==============================================================================

setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"
cd /d "%SCRIPT_DIR%"

set "OUTPUT_BASE=%SCRIPT_DIR%publish"
set "DIST_DIR=%SCRIPT_DIR%dist"

echo ==========================================================
echo [ParamToolbox] DANG DONG GOI DA NEN TANG...
echo ==========================================================

if exist "%OUTPUT_BASE%" rmdir /s /q "%OUTPUT_BASE%"
if exist "%DIST_DIR%" rmdir /s /q "%DIST_DIR%"
mkdir "%DIST_DIR%"

echo.
echo [1/3] Build Windows x64...
dotnet publish ParamToolbox.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o "%OUTPUT_BASE%\win-x64"

(
echo @echo off
echo title ParamToolbox
echo cls
echo "%%~dp0ParamToolbox.exe"
echo pause
) > "%OUTPUT_BASE%\win-x64\Run_Tool.bat"

echo.
echo [2/3] Build macOS Apple Silicon (osx-arm64)...
dotnet publish ParamToolbox.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o "%OUTPUT_BASE%\osx-arm64"

(
echo #!/bin/bash
echo DIR="$(cd "$(dirname "$0")" && pwd)"
echo chmod +x "$DIR/ParamToolbox"
echo "$DIR/ParamToolbox"
) > "%OUTPUT_BASE%\osx-arm64\Run_Tool.command"

echo.
echo [3/3] Build macOS Intel (osx-x64)...
dotnet publish ParamToolbox.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o "%OUTPUT_BASE%\osx-x64"

(
echo #!/bin/bash
echo DIR="$(cd "$(dirname "$0")" && pwd)"
echo chmod +x "$DIR/ParamToolbox"
echo "$DIR/ParamToolbox"
) > "%OUTPUT_BASE%\osx-x64\Run_Tool.command"

echo.
echo [Compress] Dang nen file ZIP vao dist...
powershell -Command "Compress-Archive -Path '%OUTPUT_BASE%\win-x64\*' -DestinationPath '%DIST_DIR%\ParamToolbox_Win64.zip' -Force"
powershell -Command "Compress-Archive -Path '%OUTPUT_BASE%\osx-arm64\*' -DestinationPath '%DIST_DIR%\ParamToolbox_MacArm64.zip' -Force"
powershell -Command "Compress-Archive -Path '%OUTPUT_BASE%\osx-x64\*' -DestinationPath '%DIST_DIR%\ParamToolbox_MacX64.zip' -Force"

echo.
echo ==========================================================
echo [THANH CONG] File phat hanh da duoc tao tai: %DIST_DIR%
echo ==========================================================
pause
