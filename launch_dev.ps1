# Script to launch Valheim in modded developer mode with console enabled
$valheimExe = "D:\Steam\steamapps\common\Valheim\valheim.exe"

if (-not (Test-Path $valheimExe)) {
    Write-Error "Valheim executable not found at $valheimExe"
    exit 1
}

Write-Host "Launching Valheim in Developer Mode (-console)..." -ForegroundColor Cyan
Write-Host "Tip: Press F5 inside Valheim to open the console." -ForegroundColor Gray
Write-Host "Tip: Press F7 to quickly spawn a test worker." -ForegroundColor Gray

Start-Process -FilePath $valheimExe -ArgumentList "-console"
