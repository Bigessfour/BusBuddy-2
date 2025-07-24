@echo off
title Dell Inspiron PowerShell 7.6.4 Optimizer
echo.
echo ================================================================
echo    Dell Inspiron 16 5640 PowerShell 7.6.4 Optimizer
echo ================================================================
echo.
echo This will optimize your Dell Inspiron for PowerShell 7.6.4
echo performance including:
echo.
echo   * PowerShell startup and profile optimization
echo   * Intel Core i5-1334U performance tuning
echo   * 16GB DDR5 RAM optimization
echo   * NVMe SSD performance improvements
echo   * Dell-specific driver checks
echo   * Windows 11 bloatware removal
echo.
echo Administrator privileges are required.
echo.
pause

REM Check if running from correct directory
if not exist "optimize-dell-inspiron-ps76.ps1" (
    echo ERROR: optimize-dell-inspiron-ps76.ps1 not found!
    echo Make sure you're running this from the correct directory.
    pause
    exit /b 1
)

REM Run the PowerShell launcher
echo Starting optimization...
echo.
powershell -ExecutionPolicy Bypass -File "run-dell-optimization.ps1"

echo.
echo Optimization process completed.
pause
