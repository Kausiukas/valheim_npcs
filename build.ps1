param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$msBuildPath = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
if (-not (Test-Path $msBuildPath)) {
    $vswhere = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $msBuildPath = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    }
}

if (-not (Test-Path $msBuildPath)) {
    Write-Error "MSBuild.exe was not found. Please verify Visual Studio 2022 installation."
    exit 1
}

$solutionPath = Join-Path $PSScriptRoot "ValheimWorkers\ValheimWorkers.sln"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Building ValheimWorkers ($Configuration)..." -ForegroundColor Cyan
Write-Host "MSBuild: $msBuildPath" -ForegroundColor DarkGray
Write-Host "=========================================" -ForegroundColor Cyan

& $msBuildPath $solutionPath /p:Configuration=$Configuration /v:minimal /nologo

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n[SUCCESS] ValheimWorkers built and deployed successfully!" -ForegroundColor Green
    
    $r2Plugin = "$env:APPDATA\r2modmanPlus-local\Valheim\profiles\Default\BepInEx\plugins\ValheimWorkers\ValheimWorkers.dll"
    if (Test-Path $r2Plugin) {
        $fileInfo = Get-Item $r2Plugin
        Write-Host "Active Plugin DLL: $r2Plugin" -ForegroundColor Gray
        Write-Host "Last Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
    }
} else {
    Write-Error "Build failed with exit code $LASTEXITCODE."
    exit $LASTEXITCODE
}
