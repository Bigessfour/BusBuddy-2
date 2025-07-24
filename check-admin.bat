@echo off
echo Checking administrator privileges...
net session >nul 2>&1
if %errorLevel% == 0 (
    echo ✓ Administrator privileges confirmed
    echo.
    echo You can now run any of these optimization options:
    echo.
    echo 1. Right-click on "start-admin-optimization.ps1" and select "Run with PowerShell"
    echo 2. Open an elevated PowerShell window and run: .\start-admin-optimization.ps1
    echo 3. Double-click "run-admin-optimization.bat" for a simple menu
    echo.
    echo Recommended: Use option 1 for the best interactive experience
) else (
    echo ✗ Not running as Administrator
    echo.
    echo To get administrator privileges:
    echo 1. Right-click on PowerShell and select "Run as administrator"
    echo 2. Or right-click on this file and select "Run as administrator"
)
echo.
pause
