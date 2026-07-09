[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$DataRoot = Join-Path $ScriptRoot 'Data'
$TemplateRoot = Join-Path $DataRoot 'Template'
$GroupDataPath = Join-Path $DataRoot 'schematic_groups.json'

function Read-TextNoBom {
    param([Parameter(Mandatory = $true)][string] $Path)

    return [System.IO.File]::ReadAllText($Path)
}

function Write-TextNoBom {
    param(
        [Parameter(Mandatory = $true)][string] $Path,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string] $Text
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory) -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory | Out-Null
    }

    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Text, $encoding)
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

function Get-SafeName {
    param(
        [Parameter(Mandatory = $true)][string] $Name,
        [string] $Fallback = 'ExampleBlockMod'
    )

    $safe = $Name -replace '[^A-Za-z0-9_.-]+', ''
    if ([string]::IsNullOrWhiteSpace($safe)) {
        return $Fallback
    }

    return $safe
}

function Get-SafeLayerFolderName {
    param([Parameter(Mandatory = $true)][string] $Name)

    $safe = $Name -replace '^Working Knowledge Layer\s*-\s*', 'WKL-'
    $safe = Get-SafeName -Name $safe -Fallback 'WKL-NewLayer'
    if (-not $safe.StartsWith('WKL-', [System.StringComparison]::OrdinalIgnoreCase)) {
        return "WKL-$safe"
    }

    return $safe
}

function Get-Text {
    param([AllowNull()][object] $Node)

    if ($null -eq $Node) {
        return ''
    }

    return ([string] $Node).Trim()
}

function Get-DefinitionId {
    param([Parameter(Mandatory = $true)][object] $IdNode)

    $type = ''
    $subtype = ''

    if ($IdNode.PSObject.Properties['Type']) {
        $type = Get-Text $IdNode.Type
    }
    if ($IdNode.PSObject.Properties['Subtype']) {
        $subtype = Get-Text $IdNode.Subtype
    }
    if ([string]::IsNullOrWhiteSpace($type) -and $IdNode.PSObject.Properties['TypeId']) {
        $type = Get-Text $IdNode.TypeId
    }
    if ([string]::IsNullOrWhiteSpace($subtype) -and $IdNode.PSObject.Properties['SubtypeId']) {
        $subtype = Get-Text $IdNode.SubtypeId
    }

    if ($type.StartsWith('MyObjectBuilder_', [System.StringComparison]::OrdinalIgnoreCase)) {
        $type = $type.Substring('MyObjectBuilder_'.Length)
    }

    if ([string]::IsNullOrWhiteSpace($type) -or [string]::IsNullOrWhiteSpace($subtype)) {
        return $null
    }

    return [pscustomobject]@{
        Type = $type
        Subtype = $subtype
        Key = "$type/$subtype"
    }
}

function Get-RelativePath {
    param(
        [Parameter(Mandatory = $true)][string] $BasePath,
        [Parameter(Mandatory = $true)][string] $TargetPath
    )

    $baseFullPath = [System.IO.Path]::GetFullPath($BasePath).TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    $targetFullPath = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = [System.Uri]::new($baseFullPath)
    $targetUri = [System.Uri]::new($targetFullPath)
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
}

function Get-BlockDefinitions {
    param([Parameter(Mandatory = $true)][string] $Root)

    $rows = [System.Collections.Generic.List[object]]::new()
    foreach ($file in Get-ChildItem -LiteralPath $Root -Recurse -File -Filter '*.sbc') {
        try {
            [xml] $xml = Get-Content -LiteralPath $file.FullName -Raw
        }
        catch {
            continue
        }

        if (-not $xml.PSObject.Properties['Definitions'] -or -not $xml.Definitions.PSObject.Properties['CubeBlocks']) {
            continue
        }
        if (-not $xml.Definitions.CubeBlocks.PSObject.Properties['Definition']) {
            continue
        }

        foreach ($definition in @($xml.Definitions.CubeBlocks.Definition)) {
            if ($null -eq $definition -or $null -eq $definition.Id) {
                continue
            }

            $id = Get-DefinitionId $definition.Id
            if ($null -eq $id) {
                continue
            }

            $isPublic = $true
            if ($definition.PSObject.Properties['Public']) {
                $isPublic = -not ([string] $definition.Public).Trim().Equals('false', [System.StringComparison]::OrdinalIgnoreCase)
            }
            if (-not $isPublic) {
                continue
            }

            $rows.Add([pscustomobject]@{
                Key = $id.Key
                Type = $id.Type
                Subtype = $id.Subtype
                DisplayName = if ($definition.PSObject.Properties['DisplayName']) { Get-Text $definition.DisplayName } else { '' }
                BlockPairName = if ($definition.PSObject.Properties['BlockPairName']) { Get-Text $definition.BlockPairName } else { '' }
                CubeSize = if ($definition.PSObject.Properties['CubeSize']) { Get-Text $definition.CubeSize } else { '' }
                File = Get-RelativePath -BasePath $Root -TargetPath $file.FullName
            }) | Out-Null
        }
    }

    return @($rows | Sort-Object Key -Unique)
}

function Get-CandidateModRoots {
    $roots = [System.Collections.Generic.List[string]]::new()

    $localMods = Join-Path $env:APPDATA 'SpaceEngineers\Mods'
    if (Test-Path -LiteralPath $localMods) {
        $roots.Add($localMods) | Out-Null
    }

    $steamRoots = @(
        ${env:ProgramFiles(x86)},
        $env:ProgramFiles,
        'C:\Program Files (x86)',
        'C:\Program Files'
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique

    foreach ($root in $steamRoots) {
        $candidate = Join-Path $root 'Steam\steamapps\workshop\content\244850'
        if (Test-Path -LiteralPath $candidate) {
            $roots.Add($candidate) | Out-Null
        }
    }

    return @($roots | Select-Object -Unique)
}

function Select-Path {
    Write-Host ''
    Write-Host 'Where should the toolkit scan for source mods?'
    $roots = @(Get-CandidateModRoots)
    for ($i = 0; $i -lt $roots.Count; $i++) {
        Write-Host ("[{0}] {1}" -f ($i + 1), $roots[$i])
    }
    Write-Host '[C] Enter a custom path'

    while ($true) {
        $choice = Read-Host 'Select folder'
        if ($choice -match '^[Cc]$') {
            $custom = Read-Host 'Enter mod folder or parent folder path'
            if (Test-Path -LiteralPath $custom) {
                return (Resolve-Path -LiteralPath $custom).Path
            }
            Write-Host 'Path not found.'
            continue
        }

        $index = 0
        if ([int]::TryParse($choice, [ref] $index) -and $index -ge 1 -and $index -le $roots.Count) {
            return $roots[$index - 1]
        }

        Write-Host 'Enter one of the shown numbers, or C.'
    }
}

function Select-SourceMod {
    param([Parameter(Mandatory = $true)][string] $ScanRoot)

    if ((Test-Path -LiteralPath (Join-Path $ScanRoot 'Data')) -or (Get-ChildItem -LiteralPath $ScanRoot -File -Filter '*.sbc' -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1)) {
        $useRoot = Read-Host "Use this folder as the source mod? $ScanRoot [Y/n]"
        if ([string]::IsNullOrWhiteSpace($useRoot) -or $useRoot -match '^[Yy]') {
            return $ScanRoot
        }
    }

    $mods = @(Get-ChildItem -LiteralPath $ScanRoot -Directory | Sort-Object Name)
    if ($mods.Count -eq 0) {
        throw "No child mod folders found under: $ScanRoot"
    }

    Write-Host ''
    Write-Host 'Select the source block mod:'
    for ($i = 0; $i -lt $mods.Count; $i++) {
        $label = $mods[$i].Name
        $modinfo = Join-Path $mods[$i].FullName 'modinfo.sbc'
        if (Test-Path -LiteralPath $modinfo) {
            try {
                [xml] $xml = Get-Content -LiteralPath $modinfo -Raw
                if ($xml.ModItem.Name) {
                    $label = "{0} ({1})" -f $xml.ModItem.Name, $mods[$i].Name
                }
            }
            catch {
            }
        }
        Write-Host ("[{0}] {1}" -f ($i + 1), $label)
    }
    Write-Host '[C] Enter a custom source mod path'

    while ($true) {
        $choice = Read-Host 'Select mod'
        if ($choice -match '^[Cc]$') {
            $custom = Read-Host 'Enter source mod path'
            if (Test-Path -LiteralPath $custom) {
                return (Resolve-Path -LiteralPath $custom).Path
            }
            Write-Host 'Path not found.'
            continue
        }

        $index = 0
        if ([int]::TryParse($choice, [ref] $index) -and $index -ge 1 -and $index -le $mods.Count) {
            return $mods[$index - 1].FullName
        }

        Write-Host 'Enter one of the shown numbers, or C.'
    }
}

function Select-SchematicGroup {
    param(
        [Parameter(Mandatory = $true)][object[]] $Groups,
        [Parameter(Mandatory = $true)][string] $Prompt,
        [string] $DefaultId
    )

    Write-Host ''
    Write-Host $Prompt
    for ($i = 0; $i -lt $Groups.Count; $i++) {
        $group = $Groups[$i]
        Write-Host ("[{0}] {1} ({2})" -f ($i + 1), $group.displayName, $group.id)
        if ($group.hint) {
            Write-Host ("    {0}" -f $group.hint)
        }
    }

    while ($true) {
        $suffix = if ([string]::IsNullOrWhiteSpace($DefaultId)) { '' } else { " [$DefaultId]" }
        $choice = Read-Host "Choose group number or ID$suffix"
        if ([string]::IsNullOrWhiteSpace($choice) -and -not [string]::IsNullOrWhiteSpace($DefaultId)) {
            return $DefaultId
        }

        $index = 0
        if ([int]::TryParse($choice, [ref] $index) -and $index -ge 1 -and $index -le $Groups.Count) {
            return [string] $Groups[$index - 1].id
        }

        $match = @($Groups | Where-Object { $_.id -ieq $choice -or $_.displayName -ieq $choice })
        if ($match.Count -eq 1) {
            return [string] $match[0].id
        }

        Write-Host 'Enter a valid number or schematic ID.'
    }
}

function Convert-ToResearchBlockEntries {
    param([Parameter(Mandatory = $true)][object[]] $Mappings)

    $lines = [System.Collections.Generic.List[string]]::new()
    foreach ($mapping in $Mappings) {
        $parts = $mapping.Key.Split('/')
        $type = [System.Security.SecurityElement]::Escape($parts[0])
        $subtype = [System.Security.SecurityElement]::Escape($parts[1])
        $lines.Add('    <ResearchBlock xsi:type="ResearchBlock">') | Out-Null
        $lines.Add(("      <Id Type=""{0}"" Subtype=""{1}"" />" -f $type, $subtype)) | Out-Null
        $lines.Add('      <UnlockedByGroups />') | Out-Null
        $lines.Add('    </ResearchBlock>') | Out-Null
    }

    return ($lines -join [Environment]::NewLine)
}

function Convert-ToMappingLines {
    param([Parameter(Mandatory = $true)][object[]] $Mappings)

    $lines = [System.Collections.Generic.List[string]]::new()
    foreach ($mapping in $Mappings) {
        $lines.Add(('{0} = {1}' -f $mapping.Key, $mapping.SchematicId)) | Out-Null
    }

    return ($lines -join [Environment]::NewLine)
}

function Validate-GeneratedLayer {
    param(
        [Parameter(Mandatory = $true)][string] $OutputPath,
        [Parameter(Mandatory = $true)][object[]] $Mappings,
        [Parameter(Mandatory = $true)][object[]] $Groups
    )

    [xml] $researchXml = Get-Content -LiteralPath (Join-Path $OutputPath 'Data\ResearchBlocks.sbc') -Raw
    if (-not $researchXml.Definitions.ResearchBlocks) {
        throw 'Generated ResearchBlocks.sbc does not contain Definitions/ResearchBlocks.'
    }

    $knownIds = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($group in $Groups) {
        [void] $knownIds.Add([string] $group.id)
    }

    foreach ($mapping in $Mappings) {
        if (-not $knownIds.Contains($mapping.SchematicId)) {
            throw "Unknown schematic ID in generated mapping: $($mapping.SchematicId)"
        }
    }
}

Write-Host 'Working Knowledge Layer Toolkit'
Write-Host 'Creates a compatibility layer for a Space Engineers block mod.'

$rawGroups = Get-Content -LiteralPath $GroupDataPath -Raw | ConvertFrom-Json
$groups = @($rawGroups | ForEach-Object { $_ })
if ($groups.Count -eq 0) {
    throw "No schematic groups found in $GroupDataPath"
}

$scanRoot = Select-Path
$sourceModPath = Select-SourceMod -ScanRoot $scanRoot
$sourceModName = Split-Path -Leaf $sourceModPath

$modinfoPath = Join-Path $sourceModPath 'modinfo.sbc'
if (Test-Path -LiteralPath $modinfoPath) {
    try {
        [xml] $modinfoXml = Get-Content -LiteralPath $modinfoPath -Raw
        if ($modinfoXml.ModItem.Name) {
            $sourceModName = [string] $modinfoXml.ModItem.Name
        }
    }
    catch {
    }
}

Write-Host ''
Write-Host "Scanning: $sourceModPath"
$blocks = @(Get-BlockDefinitions -Root $sourceModPath)
if ($blocks.Count -eq 0) {
    throw 'No public cube block definitions were found in the selected source mod.'
}

Write-Host "Found $($blocks.Count) public block definitions."
$blocks | Select-Object -First 20 Key, DisplayName, CubeSize, BlockPairName | Format-Table -AutoSize
if ($blocks.Count -gt 20) {
    Write-Host "...and $($blocks.Count - 20) more."
}

$defaultGroupId = Select-SchematicGroup -Groups $groups -Prompt 'Choose the default schematic group for these blocks.' -DefaultId 'structure.industrial'

$mappings = @($blocks | ForEach-Object {
    [pscustomobject]@{
        Key = $_.Key
        SchematicId = $defaultGroupId
        DisplayName = $_.DisplayName
        CubeSize = $_.CubeSize
        BlockPairName = $_.BlockPairName
    }
})

$override = Read-Host 'Override outlier blocks one by one? [y/N]'
if ($override -match '^[Yy]') {
    foreach ($mapping in $mappings) {
        Write-Host ''
        Write-Host ("Block: {0}" -f $mapping.Key)
        if ($mapping.DisplayName) {
            Write-Host ("Display: {0}" -f $mapping.DisplayName)
        }
        if ($mapping.BlockPairName) {
            Write-Host ("Pair: {0}" -f $mapping.BlockPairName)
        }
        Write-Host ("Current: {0}" -f $mapping.SchematicId)
        $answer = Read-Host 'Press Enter to keep, O to choose another group, or S to stop overrides'
        if ($answer -match '^[Ss]$') {
            break
        }
        if ($answer -match '^[Oo]$') {
            $mapping.SchematicId = Select-SchematicGroup -Groups $groups -Prompt 'Choose replacement schematic group.' -DefaultId $mapping.SchematicId
        }
    }
}

$defaultLayerName = "Working Knowledge Layer - $sourceModName"
$layerName = Read-Host "Layer display name [$defaultLayerName]"
if ([string]::IsNullOrWhiteSpace($layerName)) {
    $layerName = $defaultLayerName
}

$defaultFolderName = Get-SafeLayerFolderName -Name $layerName
$folderName = Read-Host "Layer folder name [$defaultFolderName]"
if ([string]::IsNullOrWhiteSpace($folderName)) {
    $folderName = $defaultFolderName
}
$folderName = Get-SafeLayerFolderName -Name $folderName

$defaultOutputRoot = Join-Path $env:APPDATA 'SpaceEngineers\Mods'
if (-not (Test-Path -LiteralPath $defaultOutputRoot)) {
    $defaultOutputRoot = (Get-Location).Path
}
$outputRoot = Read-Host "Output parent folder [$defaultOutputRoot]"
if ([string]::IsNullOrWhiteSpace($outputRoot)) {
    $outputRoot = $defaultOutputRoot
}

$outputRoot = [System.IO.Path]::GetFullPath($outputRoot)
$outputPath = Join-Path $outputRoot $folderName
if (Test-Path -LiteralPath $outputPath) {
    $replace = Read-Host "Output folder exists. Overwrite generated files in ${outputPath}? [y/N]"
    if ($replace -notmatch '^[Yy]') {
        throw 'Output cancelled.'
    }
}

$tokens = @{
    LAYER_NAME = $layerName
    SOURCE_MOD_NAME = $sourceModName
    VERSION = '1.0.0'
    AUTHOR = 'Beneath the Crust'
    RESEARCH_BLOCKS = Convert-ToResearchBlockEntries -Mappings $mappings
    BLOCK_MAPPINGS = Convert-ToMappingLines -Mappings $mappings
}

Write-Host ''
Write-Host "Creating layer: $outputPath"

Write-TextNoBom -Path (Join-Path $outputPath 'README.md') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'README.md.template')) -Tokens $tokens)
Write-TextNoBom -Path (Join-Path $outputPath 'modinfo.sbc') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'modinfo.sbc.template')) -Tokens $tokens)
Write-TextNoBom -Path (Join-Path $outputPath 'Data\ResearchBlocks.sbc') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'Data\ResearchBlocks.sbc.template')) -Tokens $tokens)
Write-TextNoBom -Path (Join-Path $outputPath 'Data\WorkingKnowledge\block_mappings.txt') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'Data\WorkingKnowledge\block_mappings.txt.template')) -Tokens $tokens)
Write-TextNoBom -Path (Join-Path $outputPath 'Publishing\changelog.md') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'Publishing\changelog.md.template')) -Tokens $tokens)
Write-TextNoBom -Path (Join-Path $outputPath 'Publishing\workshop_description_bbcode.txt') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'Publishing\workshop_description_bbcode.txt.template')) -Tokens $tokens)

Validate-GeneratedLayer -OutputPath $outputPath -Mappings $mappings -Groups $groups

Write-Host ''
Write-Host 'Done.'
Write-Host "Generated layer: $outputPath"
Write-Host "Mapped blocks: $($mappings.Count)"
Write-Host 'Review Data/WorkingKnowledge/block_mappings.txt before publishing.'
