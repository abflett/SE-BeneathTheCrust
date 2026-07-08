[CmdletBinding()]
param(
    [string] $DestinationRoot = (Join-Path $env:APPDATA 'SpaceEngineers\Mods'),
    [switch] $NoClean
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$buildScript = Join-Path $PSScriptRoot 'build.ps1'
if ($NoClean) {
    & $buildScript -ModName WkKn -DestinationRoot $DestinationRoot -NoClean
}
else {
    & $buildScript -ModName WkKn -DestinationRoot $DestinationRoot
}
