param(
    [switch]$Kaldir
)

$ErrorActionPreference = 'Stop'
$runKeyPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$runValueName = 'EnvanterUygulamamDockerStartWatcher'
$watcherPath = Join-Path $PSScriptRoot 'DockerStartTarayiciIzleyici.ps1'

if ($Kaldir) {
    Remove-ItemProperty `
        -Path $runKeyPath `
        -Name $runValueName `
        -ErrorAction SilentlyContinue

    Get-CimInstance Win32_Process |
        Where-Object {
            $_.ProcessId -ne $PID -and
            $_.CommandLine -like "*$([IO.Path]::GetFileName($watcherPath))*"
        } |
        ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }

    Write-Host 'Docker Desktop otomatik tarayici acma yardimcisi kaldirildi.'
    exit 0
}

if (-not (Test-Path -LiteralPath $watcherPath -PathType Leaf)) {
    throw "Izleyici dosyasi bulunamadi: $watcherPath"
}

$runCommand = "powershell.exe -NoLogo -NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$watcherPath`""

New-Item -Path $runKeyPath -Force | Out-Null
Set-ItemProperty -Path $runKeyPath -Name $runValueName -Value $runCommand

Start-Process `
    -FilePath 'powershell.exe' `
    -ArgumentList @(
        '-NoLogo',
        '-NoProfile',
        '-WindowStyle', 'Hidden',
        '-ExecutionPolicy', 'Bypass',
        '-File', "`"$watcherPath`""
    ) `
    -WindowStyle Hidden

Write-Host 'Kurulum tamamlandi.'
Write-Host 'Bundan sonra Docker Desktop icinde Envanter-Uygulamam grubuna Start dediginizde uygulama ve API tarayicida acilacak.'
