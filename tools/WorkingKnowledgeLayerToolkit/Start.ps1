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

function Get-WorkshopItemId {
    param([Parameter(Mandatory = $true)][string] $Root)

    $leaf = Split-Path -Leaf $Root
    if ($leaf -match '^\d{5,}$') {
        return $leaf
    }

    return $null
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

function Get-ModInfoDisplayName {
    param([Parameter(Mandatory = $true)][string] $Root)

    $modinfoPath = Join-Path $Root 'modinfo.sbc'
    if (Test-Path -LiteralPath $modinfoPath) {
        try {
            [xml] $modinfoXml = Get-Content -LiteralPath $modinfoPath -Raw
            if ($modinfoXml.ModItem.Name) {
                $parsedName = ([string] $modinfoXml.ModItem.Name).Trim()
                if (-not [string]::IsNullOrWhiteSpace($parsedName)) {
                    return $parsedName
                }
            }
        }
        catch {
        }
    }

    return $null
}

function Get-SteamWorkshopTitles {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]] $PublishedFileIds,
        [int] $TimeoutSeconds = 5
    )

    $titles = @{}
    $ids = @($PublishedFileIds | Where-Object { $_ -match '^\d+$' } | Sort-Object -Unique)
    if ($ids.Count -eq 0) {
        return $titles
    }

    Write-Host ("Looking up Steam Workshop names for {0} item(s)." -f $ids.Count)
    $endpoint = 'https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/'
    $chunkSize = 100

    for ($offset = 0; $offset -lt $ids.Count; $offset += $chunkSize) {
        $chunk = @($ids | Select-Object -Skip $offset -First $chunkSize)
        $body = @{
            itemcount = $chunk.Count
        }
        for ($i = 0; $i -lt $chunk.Count; $i++) {
            $body["publishedfileids[$i]"] = $chunk[$i]
        }

        try {
            $response = Invoke-RestMethod -Uri $endpoint -Method Post -Body $body -ContentType 'application/x-www-form-urlencoded' -TimeoutSec $TimeoutSeconds
            foreach ($detail in @($response.response.publishedfiledetails)) {
                $id = [string] $detail.publishedfileid
                $title = ([string] $detail.title).Trim()
                if ([string] $detail.result -eq '1' -and -not [string]::IsNullOrWhiteSpace($id) -and -not [string]::IsNullOrWhiteSpace($title)) {
                    $titles[$id] = $title
                }
            }
        }
        catch {
            Write-Host 'Steam Workshop name lookup failed or timed out. Falling back to modinfo.sbc or folder names.'
            return $titles
        }
    }

    return $titles
}

function Get-ModDisplayName {
    param(
        [Parameter(Mandatory = $true)][string] $Root,
        [hashtable] $WorkshopTitles = @{}
    )

    $workshopId = Get-WorkshopItemId -Root $Root
    if ($workshopId -and $WorkshopTitles.ContainsKey($workshopId)) {
        return [string] $WorkshopTitles[$workshopId]
    }

    $modinfoName = Get-ModInfoDisplayName -Root $Root
    if (-not [string]::IsNullOrWhiteSpace($modinfoName)) {
        return $modinfoName
    }

    return (Split-Path -Leaf $Root)
}

function Test-LooksLikeModRoot {
    param([Parameter(Mandatory = $true)][string] $Path)

    return (Test-Path -LiteralPath (Join-Path $Path 'Data')) -or
        (Test-Path -LiteralPath (Join-Path $Path 'modinfo.sbc')) -or
        ($null -ne (Get-ChildItem -LiteralPath $Path -File -Filter '*.sbc' -ErrorAction SilentlyContinue | Select-Object -First 1))
}

function Get-BlockSetCandidates {
    param([Parameter(Mandatory = $true)][string] $ScanRoot)

    $candidateRoots = [System.Collections.Generic.List[string]]::new()
    $seenRoots = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    $scanRootIsMod = Test-LooksLikeModRoot -Path $ScanRoot
    if ($scanRootIsMod) {
        if ($seenRoots.Add($ScanRoot)) {
            $candidateRoots.Add($ScanRoot) | Out-Null
        }
    }

    if (-not $scanRootIsMod) {
        foreach ($directory in @(Get-ChildItem -LiteralPath $ScanRoot -Directory -ErrorAction SilentlyContinue | Sort-Object Name)) {
            if ($seenRoots.Add($directory.FullName)) {
                $candidateRoots.Add($directory.FullName) | Out-Null
            }
        }
    }

    $workshopIds = @($candidateRoots | ForEach-Object { Get-WorkshopItemId -Root $_ } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $workshopTitles = Get-SteamWorkshopTitles -PublishedFileIds $workshopIds

    $sets = [System.Collections.Generic.List[object]]::new()
    foreach ($candidateRoot in $candidateRoots) {
        $name = Get-ModDisplayName -Root $candidateRoot -WorkshopTitles $workshopTitles
        Write-Host ("Scanning block set: {0}" -f $name)
        $blocks = @(Get-BlockDefinitions -Root $candidateRoot)
        if ($blocks.Count -eq 0) {
            continue
        }

        $sets.Add([pscustomobject]@{
            Name = $name
            Path = $candidateRoot
            Blocks = $blocks
            BlockCount = $blocks.Count
        }) | Out-Null
    }

    return @($sets | Sort-Object Name)
}

function Convert-ToSelectionNumbers {
    param(
        [Parameter(Mandatory = $true)][string] $Text,
        [Parameter(Mandatory = $true)][int] $MaxValue
    )

    $numbers = [System.Collections.Generic.List[int]]::new()
    foreach ($part in @($Text -split '[,\s]+' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })) {
        $number = 0
        if (-not [int]::TryParse($part, [ref] $number)) {
            return $null
        }
        if ($number -lt 1 -or $number -gt $MaxValue) {
            return $null
        }
        if (-not $numbers.Contains($number)) {
            $numbers.Add($number) | Out-Null
        }
    }

    return @($numbers)
}

function Get-CandidateModRoots {
    $roots = [System.Collections.Generic.List[object]]::new()
    $seenPaths = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    $localMods = Join-Path $env:APPDATA 'SpaceEngineers\Mods'
    if ((Test-Path -LiteralPath $localMods) -and $seenPaths.Add($localMods)) {
        $roots.Add([pscustomobject]@{
            Label = 'Local Space Engineers mods folder'
            Path = $localMods
            Note = 'Local/manual mods and test deploys.'
        }) | Out-Null
    }

    $steamRoots = @(
        ${env:ProgramFiles(x86)},
        $env:ProgramFiles,
        'C:\Program Files (x86)',
        'C:\Program Files'
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique

    foreach ($root in $steamRoots) {
        $candidate = Join-Path $root 'Steam\steamapps\workshop\content\244850'
        if ((Test-Path -LiteralPath $candidate) -and $seenPaths.Add($candidate)) {
            $roots.Add([pscustomobject]@{
                Label = 'Steam Workshop Space Engineers mods folder'
                Path = $candidate
                Note = '244850 is the Steam App ID for Space Engineers.'
            }) | Out-Null
        }
    }

    return @($roots)
}

function Select-Path {
    Write-Host ''
    Write-Host 'Where should the toolkit scan for source mods?'
    $roots = @(Get-CandidateModRoots)
    for ($i = 0; $i -lt $roots.Count; $i++) {
        Write-Host ("[{0}] {1}" -f ($i + 1), $roots[$i].Label)
        Write-Host ("    {0}" -f $roots[$i].Path)
        Write-Host ("    {0}" -f $roots[$i].Note)
    }
    $customOption = $roots.Count + 1
    Write-Host ("[{0}] Enter a custom path" -f $customOption)

    while ($true) {
        $choice = Read-Host 'Select folder'
        $index = 0
        if ([int]::TryParse($choice, [ref] $index) -and $index -eq $customOption) {
            $custom = Read-Host 'Enter mod folder or parent folder path'
            if (Test-Path -LiteralPath $custom) {
                return (Resolve-Path -LiteralPath $custom).Path
            }
            Write-Host 'Path not found.'
            continue
        }

        if ([int]::TryParse($choice, [ref] $index) -and $index -ge 1 -and $index -le $roots.Count) {
            return $roots[$index - 1].Path
        }

        Write-Host 'Enter one of the shown numbers.'
    }
}

function Select-BlockSets {
    param([Parameter(Mandatory = $true)][object[]] $Sets)

    Write-Host ''
    Write-Host 'Select which block sets to include in this layer:'
    for ($i = 0; $i -lt $Sets.Count; $i++) {
        Write-Host ("[{0}] {1} - contains {2} public blocks" -f ($i + 1), $Sets[$i].Name, $Sets[$i].BlockCount)
        Write-Host ("    {0}" -f $Sets[$i].Path)
    }
    $allOption = $Sets.Count + 1
    Write-Host ("[{0}] Select all block sets and all blocks" -f $allOption)

    $defaultSelection = if ($Sets.Count -eq 1) { '1' } else { [string] $allOption }

    while ($true) {
        $choice = Read-Host "Please select block sets [default $defaultSelection] (example: 1 3)"
        if ([string]::IsNullOrWhiteSpace($choice)) {
            $choice = $defaultSelection
        }

        $selectedNumbers = @(Convert-ToSelectionNumbers -Text $choice -MaxValue $allOption)
        if ($null -eq $selectedNumbers -or $selectedNumbers.Count -eq 0) {
            Write-Host 'Enter one or more shown numbers separated by spaces, or choose the all option.'
            continue
        }

        if ($selectedNumbers -contains $allOption) {
            return @($Sets)
        }

        return @($selectedNumbers | ForEach-Object { $Sets[$_ - 1] })
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
Write-Host ''
Write-Host 'Scanning for block sets. This can take a moment for large Workshop folders.'
$blockSets = @(Get-BlockSetCandidates -ScanRoot $scanRoot)
if ($blockSets.Count -eq 0) {
    throw 'No block sets with public cube block definitions were found in the selected folder.'
}

$selectedSets = @(Select-BlockSets -Sets $blockSets)
$sourceModName = if ($selectedSets.Count -eq 1) {
    [string] $selectedSets[0].Name
}
elseif ($selectedSets.Count -le 3) {
    [string] (($selectedSets | ForEach-Object { $_.Name }) -join ', ')
}
else {
    'Selected Space Engineers block mods'
}

$blocks = @($selectedSets | ForEach-Object { $_.Blocks } | Sort-Object Key -Unique)
Write-Host ''
Write-Host ("Selected {0} block set(s), containing {1} unique public block definitions." -f $selectedSets.Count, $blocks.Count)
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
