@echo off
setlocal
cd /d "%~dp0"
dotnet restore
if errorlevel 1 goto :error
dotnet publish L2Presence.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
if errorlevel 1 goto :error
echo.
echo Publish complete.
echo Output: dist\L2Presence.exe
pause
exit /b 0

:error
echo.
echo Publish failed.
pause
exit /b 1
