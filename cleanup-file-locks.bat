@echo off
echo MSB3027/MSB3021 File Lock Resolution - Manual Cleanup
echo ====================================================

echo Step 1: Terminating problematic processes...
taskkill /f /im dotnet.exe >nul 2>&1
taskkill /f /im pwsh.exe >nul 2>&1
taskkill /f /im Code.exe >nul 2>&1

echo Step 2: Waiting for process cleanup...
timeout /t 3 /nobreak >nul

echo Step 3: Removing build directories...
if exist "BusBuddy.Core\bin" rmdir /s /q "BusBuddy.Core\bin"
if exist "BusBuddy.Core\obj" rmdir /s /q "BusBuddy.Core\obj"
if exist "BusBuddy.WPF\bin" rmdir /s /q "BusBuddy.WPF\bin"
if exist "BusBuddy.WPF\obj" rmdir /s /q "BusBuddy.WPF\obj"
if exist "BusBuddy.Tests\bin" rmdir /s /q "BusBuddy.Tests\bin"
if exist "BusBuddy.Tests\obj" rmdir /s /q "BusBuddy.Tests\obj"
if exist "BusBuddy.UITests\bin" rmdir /s /q "BusBuddy.UITests\bin"
if exist "BusBuddy.UITests\obj" rmdir /s /q "BusBuddy.UITests\obj"

echo Step 4: Verification...
set FOUND_DIRS=0
if exist "BusBuddy.Core\bin" set FOUND_DIRS=1
if exist "BusBuddy.Core\obj" set FOUND_DIRS=1
if exist "BusBuddy.WPF\bin" set FOUND_DIRS=1
if exist "BusBuddy.WPF\obj" set FOUND_DIRS=1
if exist "BusBuddy.Tests\bin" set FOUND_DIRS=1
if exist "BusBuddy.Tests\obj" set FOUND_DIRS=1
if exist "BusBuddy.UITests\bin" set FOUND_DIRS=1
if exist "BusBuddy.UITests\obj" set FOUND_DIRS=1

if %FOUND_DIRS%==0 (
    echo SUCCESS: All build directories removed
    echo You can now try: dotnet clean BusBuddy.sln
    echo Then try: dotnet build BusBuddy.sln
) else (
    echo WARNING: Some directories still exist
    echo Close VS Code completely and run this batch file again
)

echo.
echo File lock prevention has been added to all project files.
echo VS Code settings updated to prevent auto-build.
echo.
pause
