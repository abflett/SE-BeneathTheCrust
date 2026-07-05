[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$version = '0.3.3'
$url = "https://github.com/stolliemods/MWMBuilder/releases/download/v$version/StollieMWMBuilder_$version.zip"
$toolsRoot = $PSScriptRoot
$target = Join-Path $toolsRoot 'MWMBuilder'
$repoRoot = Split-Path -Parent $toolsRoot
$tmpRoot = Join-Path $repoRoot '.tmp'
$zip = Join-Path $tmpRoot "StollieMWMBuilder_$version.zip"

if ((Test-Path -LiteralPath (Join-Path $target 'MwmBuilder.exe')) -and -not $Force) {
    Write-Host "MWMBuilder already installed at $target"
    return
}

$targetFull = [System.IO.Path]::GetFullPath($target)
$toolsFull = [System.IO.Path]::GetFullPath($toolsRoot)
if (-not $targetFull.StartsWith($toolsFull, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to install outside tools folder: $targetFull"
}

New-Item -ItemType Directory -Force -Path $tmpRoot | Out-Null

Write-Host "Downloading Stollie MWMBuilder $version..."
Invoke-WebRequest -Uri $url -OutFile $zip

if (Test-Path -LiteralPath $target) {
    Remove-Item -LiteralPath $target -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $target | Out-Null
Expand-Archive -LiteralPath $zip -DestinationPath $target -Force
Remove-Item -LiteralPath $zip -Force

if (-not (Test-Path -LiteralPath (Join-Path $target 'MwmBuilder.exe'))) {
    throw "MWMBuilder install did not produce MwmBuilder.exe"
}

Write-Host "Installed MWMBuilder -> $target"
