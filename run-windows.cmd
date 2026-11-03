@echo off
setlocal

if not "%~3"=="" goto check
echo Usage: run-windows.cmd file.jpg squareSize S^|M
exit /b 1

:check
if not "%~4"=="" (
    echo Error: Expected exactly three arguments.
    exit /b 1
)

cd /d "%~dp0"

set "APP_DIR=.runtime\windows"
set "APP=%APP_DIR%\ImagePixelation.exe"

if not exist "%APP%" (
    if not exist "%APP_DIR%" mkdir "%APP_DIR%"

    echo Downloading Windows application...

    curl.exe -fL "https://github.com/usuleymanli16281/advanced-software-paradigms-a4/releases/download/v1.0.0/ImagePixelation.exe" -o "%APP%"

    if errorlevel 1 (
        echo Error: Download failed.
        exit /b 1
    )
)

"%APP%" "%~1" %2 %3
exit /b %errorlevel%
