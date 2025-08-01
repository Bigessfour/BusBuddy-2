@echo off
setlocal

REM === Batch tool multiplexer: run, build, test ===
if "%1"=="build" goto BUILD
if "%1"=="test" goto TEST

echo ========================================
echo  BusBuddy Runtime Error Capture v1.1
echo ========================================
echo.
echo Running BusBuddy with error capture...

REM Create timestamp for log rotation using a simpler method
set "timestamp=%date:~-4,4%%date:~-10,2%%date:~-7,2%-%time:~0,2%%time:~3,2%%time:~6,2%"
set "timestamp=%timestamp: =0%"

REM Make sure the paths use proper slashes for PowerShell compatibility
set PROJECT_PATH=BusBuddy.WPF/BusBuddy.WPF.csproj
set OUTPUT_LOG=logs\app_output_%timestamp%.log
set ERROR_LOG=logs\app_errors_%timestamp%.log

REM Create logs directory if it doesn't exist
if not exist logs mkdir logs

REM Clear existing log files
echo. > "%OUTPUT_LOG%"
echo. > "%ERROR_LOG%"

REM Run the application and capture all output
echo Command: dotnet run --project "%PROJECT_PATH%"

REM Use cmd.exe explicitly to ensure batch-style redirection works
cmd.exe /c dotnet run --project "%PROJECT_PATH%" > "%OUTPUT_LOG%" 2> "%ERROR_LOG%"

echo.
echo Complete. Check %OUTPUT_LOG% for standard output and %ERROR_LOG% for errors.
echo.

REM Display any errors found
echo ERROR SUMMARY:
echo -----------------------------
echo Searching for all types of errors...

REM Find common error patterns with multiple search terms
set "ERROR_PATTERNS=/C:exception /C:error: /C:fatal /C:fail /C:crash /C:stack trace /C:system. /C:microsoft. /C:unhandled /C:unexpected"
set "UI_ERROR_PATTERNS=/C:XamlParseException /C:BindingExpression /C:Syncfusion. /C:DependencyProperty /C:InvalidOperationException /C:NullReferenceException"
set "DB_ERROR_PATTERNS=/C:DbUpdateException /C:EntityValidationErrors /C:SqlException /C:InvalidCastException /C:BusBuddyContext"

set FOUND_ERRORS=0

echo Checking error log for exceptions and errors...
findstr /I %ERROR_PATTERNS% "%ERROR_LOG%" 2>nul
if %ERRORLEVEL% EQU 0 set FOUND_ERRORS=1

echo Checking error log for UI/XAML related issues...
findstr /I %UI_ERROR_PATTERNS% "%ERROR_LOG%" 2>nul
if %ERRORLEVEL% EQU 0 set FOUND_ERRORS=1

echo Checking error log for database related issues...
findstr /I %DB_ERROR_PATTERNS% "%ERROR_LOG%" 2>nul
if %ERRORLEVEL% EQU 0 set FOUND_ERRORS=1

if %FOUND_ERRORS% EQU 1 (
    echo Above errors were found in the error log.
) else (
    echo No common errors found in error log, checking output log...

    set FOUND_ERRORS=0

    echo Checking output log for exceptions and errors...
    findstr /I %ERROR_PATTERNS% "%OUTPUT_LOG%" 2>nul
    if %ERRORLEVEL% EQU 0 set FOUND_ERRORS=1

    echo Checking output log for UI/XAML related issues...
    findstr /I %UI_ERROR_PATTERNS% "%OUTPUT_LOG%" 2>nul
    if %ERRORLEVEL% EQU 0 set FOUND_ERRORS=1

    echo Checking output log for database related issues...
    findstr /I %DB_ERROR_PATTERNS% "%OUTPUT_LOG%" 2>nul
    if %ERRORLEVEL% EQU 0 set FOUND_ERRORS=1

    if %FOUND_ERRORS% EQU 1 (
        echo Above errors were found in the output log.
    ) else (
        echo No obvious errors detected in either log file.
    )
)

echo.
echo -----------------------------
echo DETAILED ERROR CHECK:
echo -----------------------------
echo Type "type %ERROR_LOG%" to see all error output
echo Type "findstr /I error %OUTPUT_LOG%" to search output for errors
echo.

echo -----------------------------
echo LOG FILES:
echo -----------------------------
echo Output log: %OUTPUT_LOG%
echo Error log:  %ERROR_LOG%
echo.

REM Set exit code based on error detection
if %FOUND_ERRORS% EQU 1 (
    echo [!] Errors detected - Exit Code 1
    endlocal & exit /b 1
) else (
    echo [√] No errors detected - Exit Code 0
    endlocal & exit /b 0
)

:BUILD
echo ========================================
echo  BusBuddy Build (Batch)
echo ========================================
echo Building BusBuddy...
dotnet build BusBuddy.sln > logs/build_output.log 2> logs/build_errors.log
findstr /I /C:"error" logs/build_errors.log
if %ERRORLEVEL% EQU 0 (
    echo Build errors found. Check logs/build_errors.log
    endlocal & exit /b 1
) else (
    echo Build succeeded.
    endlocal & exit /b 0
)
goto :EOF

:TEST
echo ========================================
echo  BusBuddy Test (Batch)
echo ========================================
echo Running tests...
dotnet test BusBuddy.sln > logs/test_output.log 2> logs/test_errors.log
findstr /I /C:"failed" logs/test_errors.log
if %ERRORLEVEL% EQU 0 (
    echo Tests failed. Check logs/test_errors.log
    endlocal & exit /b 1
) else (
    echo Tests passed.
    endlocal & exit /b 0
)
goto :EOF
