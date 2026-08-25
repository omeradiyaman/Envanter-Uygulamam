param(
    [ValidateRange(30, 900)]
    [int]$TimeoutSeconds = 180
)

$ErrorActionPreference = 'SilentlyContinue'
$applicationUrl = 'http://localhost:8081'
$apiUrl = 'http://localhost:8080/api/system/status?client=docker-desktop-start'
$containerName = 'inventory-system-frontend'
$minimumOpenInterval = [TimeSpan]::FromSeconds(30)
$lastOpenedAt = [DateTimeOffset]::MinValue

$mutex = [System.Threading.Mutex]::new(
    $true,
    'Local\EnvanterUygulamamDockerStartTarayiciIzleyici',
    [ref]$createdNew
)

if (-not $createdNew) {
    exit 0
}

function Test-HttpReady {
    param([Parameter(Mandatory = $true)][string]$Url)

    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 3
        return $response.StatusCode -eq 200
    }
    catch {
        return $false
    }
}

function Open-ApplicationWhenReady {
    $deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)

    while ([DateTimeOffset]::UtcNow -lt $deadline) {
        if ((Test-HttpReady -Url $applicationUrl) -and (Test-HttpReady -Url $apiUrl)) {
            Start-Process $applicationUrl
            Start-Sleep -Milliseconds 500
            Start-Process $apiUrl
            return $true
        }

        Start-Sleep -Seconds 2
    }

    return $false
}

try {
    while ($true) {
        if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
            Start-Sleep -Seconds 10
            continue
        }

        docker events `
            --filter 'type=container' `
            --filter "container=$containerName" `
            --filter 'event=start' `
            --format '{{.TimeNano}}' 2>$null |
            ForEach-Object {
                $now = [DateTimeOffset]::UtcNow

                if (($now - $lastOpenedAt) -ge $minimumOpenInterval) {
                    if (Open-ApplicationWhenReady) {
                        $lastOpenedAt = [DateTimeOffset]::UtcNow
                    }
                }
            }

        # Docker Desktop kapatılırsa event akışı sona erer. Yeniden açılmasını bekle.
        Start-Sleep -Seconds 5
    }
}
finally {
    $mutex.ReleaseMutex()
    $mutex.Dispose()
}
