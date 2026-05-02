@echo off
echo Building SmitePnB...
echo.

dotnet publish SmitePnB\SmitePnB\SmitePnB.csproj --configuration Release --output build\

if %errorlevel% neq 0 (
    echo.
    echo Build failed. Make sure the .NET 8 SDK is installed:
    echo https://dotnet.microsoft.com/en-us/download/dotnet/8.0
    pause
    exit /b 1
)

echo.
echo Done. Executable is at:
echo   %~dp0build\SmitePnB.exe
echo.
pause
