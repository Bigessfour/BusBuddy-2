@echo off
echo === Emergency Build - No PowerShell ===
cd /d "C:\Users\steve.mckitrick\Desktop\BusBuddy"
echo Cleaning project...
dotnet clean BusBuddy.sln --verbosity minimal
echo Restoring packages...
dotnet restore BusBuddy.sln --force --no-cache
echo Building project...
dotnet build BusBuddy.sln --verbosity minimal --nologo
echo Build complete!
pause
