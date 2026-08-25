@echo off
chcp 65001 >nul
setlocal

cd /d "%~dp0"
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0DockerStartOtomatikAcmaKur.ps1"

if errorlevel 1 (
    echo.
    echo Kurulum basarisiz oldu. Yukaridaki hata mesajini kontrol edin.
    pause
    exit /b 1
)

echo.
echo Docker Desktop Start otomasyonu hazir.
pause
