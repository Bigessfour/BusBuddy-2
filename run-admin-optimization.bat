@echo off
REM Dell Inspiron PowerShell 7.6.4 Admin Optimization Launcher
REM This batch file runs the optimization script with administrator privileges

echo.
echo ====================================================================
echo Dell Inspiron 16 5640 PowerShell 7.6.4 Optimization (Admin Mode)
echo ====================================================================
echo.

REM Check if running as administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo ✓ Running with Administrator privileges
    echo.
) else (
    echo ERROR: This script must be run as Administrator
    echo Right-click and select "Run as administrator"
    pause
    exit /b 1
)

REM Display menu options
echo Select optimization level:
echo.
echo 1. PowerShell Only (Profile, startup, performance)
echo 2. Hardware Only (CPU, memory, storage optimization)
echo 3. Dell Specific (Drivers, firmware, cleanup)
echo 4. Full Optimization (All of the above + Windows debloating)
echo 5. WhatIf Mode (Show what would be changed without making changes)
echo 6. Exit
echo.

set /p choice="Enter your choice (1-6): "

if "%choice%"=="1" goto powershell_only
if "%choice%"=="2" goto hardware_only
if "%choice%"=="3" goto dell_specific
if "%choice%"=="4" goto full_optimization
if "%choice%"=="5" goto whatif_mode
if "%choice%"=="6" goto exit
goto invalid_choice

:powershell_only
echo.
echo Running PowerShell-only optimizations...
powershell.exe -ExecutionPolicy Bypass -File "optimize-dell-inspiron-ps76.ps1" -PowerShellOnly
goto end

:hardware_only
echo.
echo Running hardware optimizations...
powershell.exe -ExecutionPolicy Bypass -File "optimize-dell-inspiron-ps76.ps1" -HardwareOnly
goto end

:dell_specific
echo.
echo Running Dell-specific optimizations...
powershell.exe -ExecutionPolicy Bypass -File "optimize-dell-inspiron-ps76.ps1" -DellSpecific
goto end

:full_optimization
echo.
echo Running full system optimization...
echo WARNING: This will make comprehensive system changes!
set /p confirm="Are you sure? (y/N): "
if /i "%confirm%"=="y" (
    powershell.exe -ExecutionPolicy Bypass -File "optimize-dell-inspiron-ps76.ps1" -FullOptimization
) else (
    echo Operation cancelled.
)
goto end

:whatif_mode
echo.
echo Running in WhatIf mode (no changes will be made)...
powershell.exe -ExecutionPolicy Bypass -File "optimize-dell-inspiron-ps76.ps1" -FullOptimization -WhatIf
goto end

:invalid_choice
echo.
echo Invalid choice. Please select 1-6.
goto end

:end
echo.
echo Optimization complete!
echo.
echo Next steps:
echo - Restart PowerShell to apply profile changes
echo - Run the PowerShell 7.6 compatibility test: .\test-powershell76-optimizations.ps1
echo - Monitor system performance with Test-DellPerformance command
echo.

:exit
pause
