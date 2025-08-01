@echo off
if not exist logs mkdir logs
echo ========================================
echo  BusBuddy Health Check (Batch)
echo ========================================
echo Checking .NET SDK version...
dotnet --version > logs/health.log
echo Checking package restore...
dotnet restore BusBuddy.sln >> logs/health.log 2> logs/health_errors.log
findstr /I /C:"error" logs/health_errors.log
if %ERRORLEVEL% EQU 0 (
    echo Restore failed. Check logs/health_errors.log
) else (
    echo Restore OK.
)
echo Health check complete. See logs/health.log.

:EOF
