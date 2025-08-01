@echo off
setlocal enabledelayedexpansion

echo === BusBuddy Error Analyzer ===
echo This script analyzes the most recent error log file.
echo.

:: Find most recent error log
set LATEST_LOG=
for /f "delims=" %%F in ('dir /b /a-d /o-d logs\error-*.log 2^>nul') do (
    set LATEST_LOG=logs\%%F
    goto :found
)

:found
if "%LATEST_LOG%"=="" (
    echo No error logs found.
    goto :eof
)

echo Analyzing: %LATEST_LOG%
echo.

:: Count errors by type
set NULL_REF=0
set ARG_ERR=0
set IO_ERR=0
set DB_ERR=0
set XAML_ERR=0
set OTHER_ERR=0

for /f "delims=" %%L in ('type "%LATEST_LOG%" ^| findstr /i /c:"Exception" ^| findstr /v /c:"handled exception"') do (
    set LINE=%%L

    if "!LINE!" == "" goto :next

    if "!LINE!" neq "!LINE:NullReferenceException=!" (
        set /a NULL_REF+=1
    ) else if "!LINE!" neq "!LINE:ArgumentException=!" (
        set /a ARG_ERR+=1
    ) else if "!LINE!" neq "!LINE:IOException=!" (
        set /a IO_ERR+=1
    ) else if "!LINE!" neq "!LINE:SqlException=!" (
        set /a DB_ERR+=1
    ) else if "!LINE!" neq "!LINE:XamlParseException=!" (
        set /a XAML_ERR+=1
    ) else (
        set /a OTHER_ERR+=1
    )
)

:next
echo Error Summary:
echo   NullReferenceException: %NULL_REF%
echo   ArgumentException:      %ARG_ERR%
echo   IO/File Exceptions:     %IO_ERR%
echo   Database Exceptions:    %DB_ERR%
echo   XAML Exceptions:        %XAML_ERR%
echo   Other Exceptions:       %OTHER_ERR%
echo.

set /a TOTAL=%NULL_REF%+%ARG_ERR%+%IO_ERR%+%DB_ERR%+%XAML_ERR%+%OTHER_ERR%

if %TOTAL% gtr 0 (
    echo Found %TOTAL% exceptions. Review the log for details.
) else (
    echo No exceptions found.
)

echo.
echo Do you want to view the full error log? (Y/N)
set /p CHOICE="> "

if /i "%CHOICE%"=="Y" (
    type "%LATEST_LOG%"
)

echo.
echo Done!
pause
