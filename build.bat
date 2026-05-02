@echo off
setlocal

set "ROOT=%~dp0"
set "PROJECT=%ROOT%SmitePnB\SmitePnB\SmitePnB.csproj"
set "PUBLISH_DIR=%ROOT%build\publish"
set "BUNDLE_DIR=%ROOT%build\SmitePnB"
set "RESOURCES_DIR=%ROOT%SmitePnB\Resources"

echo Building SmitePnB...
echo.

if not exist "%PROJECT%" (
    echo Project file not found:
    echo   %PROJECT%
    echo.
    pause
    exit /b 1
)

if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
if exist "%BUNDLE_DIR%" rmdir /s /q "%BUNDLE_DIR%"

dotnet publish "%PROJECT%" --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true --output "%PUBLISH_DIR%"

if %errorlevel% neq 0 (
    echo.
    echo Build failed. Make sure the .NET 8 SDK is installed:
    echo https://dotnet.microsoft.com/en-us/download/dotnet/8.0
    pause
    exit /b 1
)

mkdir "%BUNDLE_DIR%" >nul
copy /y "%PUBLISH_DIR%\SmitePnB.exe" "%BUNDLE_DIR%\SmitePnB.exe" >nul
xcopy "%RESOURCES_DIR%" "%BUNDLE_DIR%\Resources\" /e /i /y >nul

echo.
echo Done. Launch from:
echo   %BUNDLE_DIR%\SmitePnB.exe
echo.
pause
