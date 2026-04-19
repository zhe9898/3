param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [int]$WaitSeconds = 5
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repoRoot 'Zongzu.sln'

function Get-RepoTestHostProcesses {
    $escapedRoot = [Regex]::Escape($repoRoot)

    Get-CimInstance Win32_Process |
        Where-Object {
            ($_.Name -ieq 'testhost.exe' -or $_.Name -ieq 'testhost') -and
            $_.CommandLine -and
            $_.CommandLine -match $escapedRoot
        }
}

function Wait-ForRepoTestHostsToExit {
    param(
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $processes = @(Get-RepoTestHostProcesses)
        if ($processes.Count -eq 0) {
            return
        }

        Start-Sleep -Milliseconds 500
    }
}

function Stop-RepoTestHosts {
    $processes = @(Get-RepoTestHostProcesses)
    if ($processes.Count -eq 0) {
        return
    }

    $processes | ForEach-Object {
        Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "==> Waiting for repo-scoped testhost processes to exit"
Wait-ForRepoTestHostsToExit -TimeoutSeconds $WaitSeconds

$remaining = @(Get-RepoTestHostProcesses)
if ($remaining.Count -gt 0) {
    Write-Host "==> Stopping lingering repo-scoped testhost processes"
    Stop-RepoTestHosts
}

Write-Host "==> dotnet build ($Configuration)"
& dotnet build $solutionPath -c $Configuration -m:1
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "==> dotnet test ($Configuration, --no-build)"
& dotnet test $solutionPath -c $Configuration -m:1 --no-build
exit $LASTEXITCODE
