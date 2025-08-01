@echo off
REM This is a simple wrapper for the Windows tee command

REM Usage: wtee [-a] filename
REM   -a: Append to file instead of overwriting

setlocal EnableDelayedExpansion

REM Check if append mode is requested
set APPEND=
if "%1"=="-a" (
    set APPEND=1
    shift
)

REM Get filename
set FILENAME=%1

if "%APPEND%"=="1" (
    REM Append mode
    for /F "tokens=*" %%a in ('findstr /N "^"') do (
        set "line=%%a"
        set "line=!line:*:=!"
        echo !line!
        echo !line!>> %FILENAME%
    )
) else (
    REM Overwrite mode
    for /F "tokens=*" %%a in ('findstr /N "^"') do (
        set "line=%%a"
        set "line=!line:*:=!"
        echo !line!
        echo !line!> %FILENAME%
        set APPEND=1
    )
)

endlocal
