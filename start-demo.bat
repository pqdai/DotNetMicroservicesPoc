@echo off
setlocal

cd /d "%~dp0scripts"

echo Starting infrastructure...
docker compose -f infra.yml up -d --build
if errorlevel 1 goto :error

echo Starting applications...
docker compose -f app.yml up -d --build
if errorlevel 1 goto :error

echo.
echo Demo started.
echo Eureka: http://localhost:8761
echo Web:    http://localhost:8080
exit /b 0

:error
echo.
echo Failed to start the demo. Check Docker Desktop and compose logs.
exit /b 1
