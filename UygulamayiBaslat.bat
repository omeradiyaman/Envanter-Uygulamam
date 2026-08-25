@echo off
chcp 65001 >nul
setlocal

cd /d "%~dp0"
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0UygulamayiBaslat.ps1"

if errorlevel 1 (
    echo.
    echo Uygulama baslatilamadi. Yukaridaki hata mesajini kontrol edin.
    pause
    exit /b 1
)

endlocal
exit /b 0
