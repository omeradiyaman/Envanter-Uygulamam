param(
    [ValidateRange(30, 900)]
    [int]$TimeoutSeconds = 180,
    [switch]$TarayiciyiAcma,
    [switch]$ApiyiAcma
)

$ErrorActionPreference = 'Stop'
$applicationUrl = 'http://localhost:8081'
$proxiedHealthUrl = "$applicationUrl/api/system/status?client=windows-launcher"
$apiUrl = 'http://localhost:8080/api/system/status?client=windows-launcher-browser'

function Test-HttpReady {
    param([Parameter(Mandatory = $true)][string]$Url)

    try {
        $response = Invoke-WebRequest `
            -Uri $Url `
            -UseBasicParsing `
            -TimeoutSec 3

        return $response.StatusCode -eq 200
    }
    catch {
        return $false
    }
}

Push-Location $PSScriptRoot
try {
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        throw 'Docker komutu bulunamadi. Docker Desktop kurulu olmali.'
    }

    docker info --format '{{.ServerVersion}}' | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw 'Docker Desktop calismiyor. Docker Desktop uygulamasini acip tekrar deneyin.'
    }

    Write-Host 'Envanter-Uygulamam servisleri baslatiliyor...'
    docker compose up -d
    if ($LASTEXITCODE -ne 0) {
        throw 'Docker Compose servisleri baslatilamadi.'
    }

    Write-Host 'PostgreSQL, backend ve frontend hazir olana kadar bekleniyor...'
    $deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)
    $ready = $false

    while ([DateTimeOffset]::UtcNow -lt $deadline) {
        $postgresStatus = docker inspect `
            --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' `
            inventory-system-postgres 2>$null

        if ($LASTEXITCODE -eq 0 `
            -and $postgresStatus -eq 'healthy' `
            -and (Test-HttpReady -Url $proxiedHealthUrl) `
            -and (Test-HttpReady -Url $apiUrl) `
            -and (Test-HttpReady -Url $applicationUrl)) {
            $ready = $true
            break
        }

        Start-Sleep -Seconds 2
    }

    if (-not $ready) {
        docker compose ps
        throw "Uygulama $TimeoutSeconds saniye icinde hazir duruma gelmedi."
    }

    Write-Host "Uygulama hazir: $applicationUrl"
    Write-Host "API saglik adresi: $apiUrl"
    if (-not $TarayiciyiAcma) {
        Start-Process $applicationUrl

        if (-not $ApiyiAcma) {
            Start-Sleep -Milliseconds 500
            Start-Process $apiUrl
        }
    }
}
finally {
    Pop-Location
}
