@echo off
chcp 65001 >nul
setlocal

cd /d "%~dp0"
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0DockerStartOtomatikAcmaKur.ps1" -Kaldir

echo.
pause
