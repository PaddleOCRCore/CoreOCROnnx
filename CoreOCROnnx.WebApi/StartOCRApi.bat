@echo off
set CURRENT_DIR=%~dp0
CHCP 65001
echo Starting CoreOCROnnx.WebApi.dll..
dotnet "%CURRENT_DIR%CoreOCROnnx.WebApi.dll" --urls http://*:5000
pause