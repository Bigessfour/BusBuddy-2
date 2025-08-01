@echo off
setlocal enabledelayedexpansion
color 0B

:menu
cls
echo ========================================
echo        BusBuddy Error Capture Tool
echo ========================================
echo.
echo  1. Run and Capture Errors (Batch)
echo  2. Run and Capture Errors (PowerShell with colors)
echo  3. Analyze Last Error Log
echo  4. View All Log Files
echo  5. Exit
echo.
echo ========================================
echo.
set /p choice="Enter your choice (1-5): "

if "%choice%"=="1" goto run_batch
if "%choice%"=="2" goto run_powershell
if "%choice%"=="3" goto analyze
if "%choice%"=="4" goto view_logs
if "%choice%"=="5" goto end

echo Invalid choice. Please try again.
timeout /t 2 >nul
goto menu

:run_batch
cls
echo Running application with batch capture...
call run-capture-errors.bat
pause
goto menu

:run_powershell
cls
echo Running application with PowerShell capture...
powershell -ExecutionPolicy Bypass -File run-capture-errors.ps1
pause
goto menu

:analyze
cls
call analyze-errors.bat
goto menu

:view_logs
cls
echo ========================================
echo            Available Log Files
echo ========================================
echo.
dir /b /o-d logs\*.log 2>nul
echo.
echo ========================================
echo.
set /p logfile="Enter log filename to view (or BACK to return): "
if /i "%logfile%"=="BACK" goto menu
if exist "logs\%logfile%" (
    more "logs\%logfile%"
) else (
    echo File not found: logs\%logfile%
    timeout /t 2 >nul
)
pause
goto view_logs

:end
echo Goodbye!
exit /b 0
