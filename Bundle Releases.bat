@echo off
echo Building for win-x64...
dotnet publish "SlimeVR-Tracker-Proxy\SlimeVR-Tracker-Proxy.csproj" -c Release -r win-x64 -o ".\Release_Builds\Build-win-x64"

echo.
echo Building for win-arm64...
dotnet publish "SlimeVR-Tracker-Proxy\SlimeVR-Tracker-Proxy.csproj" -c Release -r win-arm64 -o ".\Release_Builds\Build-win-arm64"

echo.
echo Building for linux-x64...
dotnet publish "SlimeVR-Tracker-Proxy\SlimeVR-Tracker-Proxy.csproj" -c Release -r linux-x64 -o ".\Release_Builds\Build-linux-x64"

echo.
echo Building for linux-arm64...
dotnet publish "SlimeVR-Tracker-Proxy\SlimeVR-Tracker-Proxy.csproj" -c Release -r linux-arm64 -o ".\Release_Builds\Build-linux-arm64"

echo.
echo Building for osx-x64...
dotnet publish "SlimeVR-Tracker-Proxy\SlimeVR-Tracker-Proxy.csproj" -c Release -r osx-x64 -o ".\Release_Builds\Build-osx-x64"

echo.
echo Building for osx-arm64...
dotnet publish "SlimeVR-Tracker-Proxy\SlimeVR-Tracker-Proxy.csproj" -c Release -r osx-arm64 -o ".\Release_Builds\Build-osx-arm64"

echo.
echo Packing releases...
rmdir /S /Q ".\Releases"
mkdir ".\Releases"

7z a -mx9 ".\Releases\SlimeFwd_win-x64.zip" ".\LICENSE" ".\Release_Builds\Build-win-x64\slimefwd.exe"
7z a -mx9 ".\Releases\SlimeFwd_win-arm64.zip" ".\LICENSE" ".\Release_Builds\Build-win-arm64\slimefwd.exe"
7z a -mx9 ".\Releases\SlimeFwd_linux-x64.zip" ".\LICENSE" ".\Release_Builds\Build-linux-x64\slimefwd"
7z a -mx9 ".\Releases\SlimeFwd_linux-arm64.zip" ".\LICENSE" ".\Release_Builds\Build-linux-arm64\slimefwd"
7z a -mx9 ".\Releases\SlimeFwd_osx-x64.zip" ".\LICENSE" ".\Release_Builds\Build-osx-x64\slimefwd"
7z a -mx9 ".\Releases\SlimeFwd_osx-arm64.zip" ".\LICENSE" ".\Release_Builds\Build-osx-arm64\slimefwd"

pause
