param(
    [string]$GameRoot = "C:\Program Files (x86)\Steam\steamapps\common\Nuclear Option",
    [string]$Configuration = "DEV_SDK",
    [string]$PatchToolConfiguration = "",
    [switch]$SkipPatchTool
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path $PSScriptRoot -Parent
$noloaderRoot = Join-Path (Split-Path $repoRoot -Parent) "NOLoader_Engine"

if (-not (Test-Path $noloaderRoot)) {
    Write-Error "NOLoader_Engine not found at $noloaderRoot"
}

if (Get-Process -Name "NuclearOption" -ErrorAction SilentlyContinue) {
    Write-Error "Close Nuclear Option before deploy (PatchTool needs Managed DLLs unlocked)."
}

$proj = Join-Path $repoRoot "NOLoader.AviationCareer\NOLoader.AviationCareer.csproj"
$modConfigProj = Join-Path $noloaderRoot "DEV.SDK\shared\NOLoader.ModConfig\NOLoader.ModConfig.csproj"
$dll = Join-Path $repoRoot "NOLoader.AviationCareer\bin\$Configuration\net48\NOLoader.AviationCareer.dll"
$modConfigDll = Join-Path $noloaderRoot "DEV.SDK\shared\NOLoader.ModConfig\bin\$Configuration\net48\NOLoader.ModConfig.dll"
$deployMods = Join-Path $repoRoot "deploy\NOLoader\mods\AviationCareer"
$gameMods = Join-Path $GameRoot "NOLoader\mods\AviationCareer"

dotnet build $modConfigProj -c $Configuration --verbosity minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet build $proj -c $Configuration --verbosity minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not (Test-Path $dll)) {
    Write-Error "Build output missing: $dll"
}

New-Item -ItemType Directory -Force -Path $deployMods, $gameMods | Out-Null
Copy-Item $dll $deployMods -Force
Copy-Item $modConfigDll $deployMods -Force
Copy-Item (Join-Path $repoRoot "NOLoader.AviationCareer\mod.json") $deployMods -Force
Copy-Item (Join-Path $repoRoot "NOLoader.AviationCareer\mod_config.ini") $deployMods -Force
Copy-Item "$deployMods\*" $gameMods -Force

Get-Item (Join-Path $gameMods "NOLoader.AviationCareer.dll") | Format-List FullName, Length, LastWriteTime

if (-not $SkipPatchTool) {
    $patchCfg = if ($PatchToolConfiguration) { $PatchToolConfiguration } else { $Configuration }
    Write-Host "Applying mod IL patches via PatchTool ($patchCfg)..."
    dotnet run --project (Join-Path $noloaderRoot "src\NOLoader.PatchTool\NOLoader.PatchTool.csproj") -c $patchCfg -- $GameRoot
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host "Aviation Career Tracker deployed to $gameMods"
