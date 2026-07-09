[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $LayerName,
    [Parameter(Mandatory = $true)][string] $SourceModName,
    [string] $FolderName,
    [string] $OutputRoot = (Join-Path (Split-Path -Parent $PSScriptRoot) 'mods'),
    [string] $Version = '1.0.0',
    [string] $Author = 'Beneath the Crust',
    [string] $BlockListPath,
    [string] $DefaultSchematicId = 'structure.industrial',
    [switch] $Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Set-TextFileNoBom {
    param(
        [Parameter(Mandatory = $true)][string] $Path,
        [Parameter(Mandatory = $true)][AllowEmptyString()][AllowEmptyCollection()][string[]] $Lines
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory) -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory | Out-Null
    }

    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllLines($Path, $Lines, $encoding)
}

function Set-RawTextFileNoBom {
    param(
        [Parameter(Mandatory = $true)][string] $Path,
        [Parameter(Mandatory = $true)][string] $Content
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory) -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory | Out-Null
    }

    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Content, $encoding)
}

function Get-SafeFolderName {
    param([Parameter(Mandatory = $true)][string] $Name)

    $safe = $Name -replace '^Working Knowledge Layer\s*-\s*', 'WKL-'
    $safe = $safe -replace '[^A-Za-z0-9_.-]+', ''
    if ([string]::IsNullOrWhiteSpace($safe)) {
        return 'WKL-NewLayer'
    }

    if (-not $safe.StartsWith('WKL-', [System.StringComparison]::OrdinalIgnoreCase)) {
        return "WKL-$safe"
    }

    return $safe
}

function Get-KnownSchematicIds {
    $repoRoot = Split-Path -Parent $PSScriptRoot
    $catalogPath = Join-Path $repoRoot 'mods\WorkingKnowledge\Data\Scripts\WorkingKnowledge\Application\Research\Catalog\ResearchCatalog.generated.cs'
    if (-not (Test-Path -LiteralPath $catalogPath)) {
        return @()
    }

    $content = Get-Content -LiteralPath $catalogPath -Raw
    $match = [regex]::Match($content, 'private const string ResearchMetadataData =\s*@\"\r?\n(?<data>.*?)\r?\n\";', [System.Text.RegularExpressions.RegexOptions]::Singleline)
    if (-not $match.Success) {
        return @()
    }

    return @($match.Groups['data'].Value.Split([char[]]@("`r", "`n"), [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object {
        ($_ -split '\|')[0]
    })
}

function Read-BlockMappings {
    param(
        [string] $Path,
        [string] $FallbackSchematicId
    )

    $rows = [System.Collections.Generic.List[object]]::new()
    if ([string]::IsNullOrWhiteSpace($Path)) {
        $rows.Add([pscustomobject]@{ Key = 'CubeBlock/ExampleBlockSubtype'; SchematicId = $FallbackSchematicId }) | Out-Null
        return @($rows)
    }

    foreach ($rawLine in Get-Content -LiteralPath $Path) {
        $line = (($rawLine -split '#', 2)[0]).Trim()
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        $key = $line
        $schematicId = $FallbackSchematicId
        $equalsIndex = $line.IndexOf('=')
        if ($equalsIndex -ge 0) {
            $key = $line.Substring(0, $equalsIndex).Trim()
            $schematicId = $line.Substring($equalsIndex + 1).Trim()
        }

        if ($key -notmatch '^[^/\s]+/[^/\s]+$') {
            throw "Invalid block ID '$key'. Expected Type/Subtype."
        }
        if ([string]::IsNullOrWhiteSpace($schematicId)) {
            throw "Missing schematic ID for block '$key'."
        }

        $rows.Add([pscustomobject]@{
            Key = $key
            SchematicId = $schematicId
        }) | Out-Null
    }

    return @($rows)
}

function Convert-ToResearchBlockXml {
    param([Parameter(Mandatory = $true)][object[]] $Mappings)

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('<?xml version="1.0" encoding="utf-8"?>') | Out-Null
    $lines.Add('<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">') | Out-Null
    $lines.Add('  <ResearchBlocks>') | Out-Null
    foreach ($mapping in $Mappings) {
        $parts = $mapping.Key.Split('/')
        $type = [System.Security.SecurityElement]::Escape($parts[0])
        $subtype = [System.Security.SecurityElement]::Escape($parts[1])
        $lines.Add('    <ResearchBlock xsi:type="ResearchBlock">') | Out-Null
        $lines.Add(("      <Id Type=""{0}"" Subtype=""{1}"" />" -f $type, $subtype)) | Out-Null
        $lines.Add('      <UnlockedByGroups />') | Out-Null
        $lines.Add('    </ResearchBlock>') | Out-Null
    }
    $lines.Add('  </ResearchBlocks>') | Out-Null
    $lines.Add('</Definitions>') | Out-Null
    return [string[]] $lines
}

function Convert-ToMappingLines {
    param(
        [Parameter(Mandatory = $true)][object[]] $Mappings,
        [Parameter(Mandatory = $true)][string] $SourceName
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('# Working Knowledge Layer mapping file.') | Out-Null
    $lines.Add('# Format:') | Out-Null
    $lines.Add('# Type/Subtype = working.knowledge.schematic.id') | Out-Null
    $lines.Add('#') | Out-Null
    $lines.Add("# $SourceName mappings.") | Out-Null
    $lines.Add('') | Out-Null
    foreach ($mapping in $Mappings) {
        $lines.Add(('{0} = {1}' -f $mapping.Key, $mapping.SchematicId)) | Out-Null
    }

    return [string[]] $lines
}

function Expand-Template {
    param(
        [Parameter(Mandatory = $true)][string] $Text,
        [Parameter(Mandatory = $true)][hashtable] $Tokens
    )

    $expanded = $Text
    foreach ($key in $Tokens.Keys) {
        $expanded = $expanded.Replace("{{$key}}", [string] $Tokens[$key])
    }

    return $expanded
}

if ([string]::IsNullOrWhiteSpace($FolderName)) {
    $FolderName = Get-SafeFolderName -Name $LayerName
}

$knownIds = @(Get-KnownSchematicIds)
$knownSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($id in $knownIds) {
    [void] $knownSet.Add($id)
}
if ($knownSet.Count -gt 0 -and -not $knownSet.Contains($DefaultSchematicId)) {
    throw "Unknown default schematic ID '$DefaultSchematicId'. See docs/WorkingKnowledge/layer_authoring.md."
}

$mappings = @(Read-BlockMappings -Path $BlockListPath -FallbackSchematicId $DefaultSchematicId)
foreach ($mapping in $mappings) {
    if ($knownSet.Count -gt 0 -and -not $knownSet.Contains($mapping.SchematicId)) {
        throw "Unknown schematic ID '$($mapping.SchematicId)' for '$($mapping.Key)'. See docs/WorkingKnowledge/layer_authoring.md."
    }
}

$outputPath = Join-Path ([System.IO.Path]::GetFullPath($OutputRoot)) $FolderName
if ((Test-Path -LiteralPath $outputPath) -and -not $Force) {
    throw "Output folder already exists: $outputPath. Use -Force to overwrite generated files."
}

$tokens = @{
    LAYER_NAME = $LayerName
    SOURCE_MOD_NAME = $SourceModName
    VERSION = $Version
    AUTHOR = $Author
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$templateRoot = Join-Path $repoRoot 'tools\WorkingKnowledgeLayerToolkit\Data\Template'

$readmeTemplate = Get-Content -LiteralPath (Join-Path $templateRoot 'README.md.template') -Raw
$modinfoTemplate = Get-Content -LiteralPath (Join-Path $templateRoot 'modinfo.sbc.template') -Raw
$changelogTemplate = Get-Content -LiteralPath (Join-Path $templateRoot 'Publishing\changelog.md.template') -Raw
$workshopTemplate = Get-Content -LiteralPath (Join-Path $templateRoot 'Publishing\workshop_description_bbcode.txt.template') -Raw

$tokens['RESEARCH_BLOCKS'] = ((Convert-ToResearchBlockXml -Mappings $mappings) | Select-Object -Skip 3 | Select-Object -SkipLast 2) -join [Environment]::NewLine
$tokens['BLOCK_MAPPINGS'] = ((Convert-ToMappingLines -Mappings $mappings -SourceName $SourceModName) | Select-Object -Skip 6) -join [Environment]::NewLine

Set-RawTextFileNoBom -Path (Join-Path $outputPath 'README.md') -Content (Expand-Template -Text $readmeTemplate -Tokens $tokens)
Set-RawTextFileNoBom -Path (Join-Path $outputPath 'modinfo.sbc') -Content (Expand-Template -Text $modinfoTemplate -Tokens $tokens)
Set-RawTextFileNoBom -Path (Join-Path $outputPath 'Data\ResearchBlocks.sbc') -Content (Expand-Template -Text (Get-Content -LiteralPath (Join-Path $templateRoot 'Data\ResearchBlocks.sbc.template') -Raw) -Tokens $tokens)
Set-RawTextFileNoBom -Path (Join-Path $outputPath 'Data\WorkingKnowledge\block_mappings.txt') -Content (Expand-Template -Text (Get-Content -LiteralPath (Join-Path $templateRoot 'Data\WorkingKnowledge\block_mappings.txt.template') -Raw) -Tokens $tokens)
Set-RawTextFileNoBom -Path (Join-Path $outputPath 'Publishing\changelog.md') -Content (Expand-Template -Text $changelogTemplate -Tokens $tokens)
Set-RawTextFileNoBom -Path (Join-Path $outputPath 'Publishing\workshop_description_bbcode.txt') -Content (Expand-Template -Text $workshopTemplate -Tokens $tokens)

Write-Host "Created Working Knowledge layer starter: $outputPath"
Write-Host "Block mappings: $($mappings.Count)"
Write-Host "Review Data/ResearchBlocks.sbc and Data/WorkingKnowledge/block_mappings.txt before publishing."
