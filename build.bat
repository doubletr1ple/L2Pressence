@echo off
setlocal
cd /d "%~dp0"
dotnet restore
if errorlevel 1 goto :error
dotnet build -c Release
if errorlevel 1 goto :error
echo.
echo Build complete.
echo Output: bin\Release\net8.0-windows\
pause
exit /b 0

:error
echo.
echo Build failed.
pause
exit /b 1
