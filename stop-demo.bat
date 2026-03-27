@echo off
setlocal

cd /d "%~dp0scripts"

docker compose -f app.yml down
docker compose -f infra.yml down
