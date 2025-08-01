@echo off
echo ========================================
echo  ULTRA SIMPLE BusBuddy Error Capture
echo ========================================
echo.
echo Running application...
echo Press Ctrl+C to stop the application when done.
echo.
echo All console output will be displayed AND saved to logs.
echo.

REM Create a timestamp for log files
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "YYYY=%dt:~0,4%"
set "MM=%dt:~4,2%"
set "DD=%dt:~6,2%"
set "HH=%dt:~8,2%"
set "Min=%dt:~10,2%"
set "Sec=%dt:~12,2%"
set "timestamp=%YYYY%%MM%%DD%-%HH%%Min%%Sec%"

REM Create logs folder if it doesn't exist
if not exist logs mkdir logs

REM Create log filenames
set "COMBINED_LOG=logs\busbuddy-%timestamp%.log"

echo Log file: %COMBINED_LOG%
echo.
echo ========================================
echo.

REM Run the application and capture output
echo Starting application at %time%...
echo Starting application at %time%... > "%COMBINED_LOG%"

REM Run with output both to console AND log file using the TEE approach
dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj 2>&1 | wtee -a "%COMBINED_LOG%"

echo.
echo Application closed.
echo All output has been saved to %COMBINED_LOG%
echo.
echo Press any key to view summary of errors...
pause > nul

echo.
echo ========== ERROR SUMMARY ==========
echo.
findstr /I /C:"exception" /C:"error:" /C:"fail" /C:"critical" "%COMBINED_LOG%"
echo.
echo ========== END SUMMARY ==========
echo.
echo Full log saved to: %COMBINED_LOG%
echo.
pause
