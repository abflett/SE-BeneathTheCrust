[CmdletBinding()]
param(
    [switch] $SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$DataRoot = Join-Path $ScriptRoot 'Data'
$TemplateRoot = Join-Path $DataRoot 'Template'
$GroupDataPath = Join-Path $DataRoot 'schematic_groups.json'
$KnownWorkingKnowledgeBlocksPath = Join-Path $DataRoot 'working_knowledge_block_keys.txt'
$WorkingKnowledgeWorkshopIds = @('3758066250')

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

function Read-BlockKeySet {
    param([Parameter(Mandatory = $true)][string] $Path)

    $keys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    if (-not (Test-Path -LiteralPath $Path)) {
        return $keys
    }

    foreach ($rawLine in [System.IO.File]::ReadLines($Path)) {
        $line = $rawLine
        $commentIndex = $line.IndexOf('#')
        if ($commentIndex -ge 0) {
            $line = $line.Substring(0, $commentIndex)
        }

        $line = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        [void] $keys.Add($line)
    }

    return $keys
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

function Get-SafeSubtypeToken {
    param([Parameter(Mandatory = $true)][string] $Value)

    $token = ($Value -replace '[^A-Za-z0-9]+', '_').Trim('_')
    if ([string]::IsNullOrWhiteSpace($token)) {
        return 'custom'
    }
    return $token
}

function Get-Text {
    param([AllowNull()][object] $Node)

    if ($null -eq $Node) {
        return ''
    }

    if ($Node -is [System.Xml.XmlNode]) {
        return ([string] $Node.InnerText).Trim()
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

function Test-ElementTextEquals {
    param(
        [Parameter(Mandatory = $true)][object] $Node,
        [Parameter(Mandatory = $true)][string] $Name,
        [Parameter(Mandatory = $true)][string] $Value
    )

    if (-not $Node.PSObject.Properties[$Name]) {
        return $false
    }

    return (Get-Text $Node.$Name).Equals($Value, [System.StringComparison]::OrdinalIgnoreCase)
}

function Test-HasOnlyNoVoxelPlacement {
    param([Parameter(Mandatory = $true)][object] $Definition)

    if (-not $Definition.PSObject.Properties['VoxelPlacement']) {
        return $false
    }

    $placementModes = @($Definition.VoxelPlacement.SelectNodes('.//*[local-name()="PlacementMode"]') | ForEach-Object { Get-Text $_ })
    if ($placementModes.Count -eq 0) {
        return $false
    }

    foreach ($mode in $placementModes) {
        if (-not $mode.Equals('None', [System.StringComparison]::OrdinalIgnoreCase)) {
            return $false
        }
    }

    return $true
}

function Test-IsUnplaceableSupportBlock {
    param([Parameter(Mandatory = $true)][object] $Definition)

    return (Test-ElementTextEquals -Node $Definition -Name 'GuiVisible' -Value 'false') -and
        (Test-ElementTextEquals -Node $Definition -Name 'IsStandAlone' -Value 'false') -and
        (Test-ElementTextEquals -Node $Definition -Name 'HasPhysics' -Value 'false') -and
        (Test-HasOnlyNoVoxelPlacement -Definition $Definition)
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
            if ($id.Subtype.StartsWith('WkKnUnlocker_', [System.StringComparison]::OrdinalIgnoreCase)) {
                continue
            }

            $isPublic = $true
            if ($definition.PSObject.Properties['Public']) {
                $isPublic = -not ([string] $definition.Public).Trim().Equals('false', [System.StringComparison]::OrdinalIgnoreCase)
            }
            if (-not $isPublic) {
                continue
            }
            if (Test-IsUnplaceableSupportBlock -Definition $definition) {
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

function Test-IsWorkingKnowledgeSourceMod {
    param(
        [Parameter(Mandatory = $true)][string] $Root,
        [Parameter(Mandatory = $true)][string] $DisplayName
    )

    $workshopId = Get-WorkshopItemId -Root $Root
    if ($workshopId -and $WorkingKnowledgeWorkshopIds -contains $workshopId) {
        return $true
    }

    if ($DisplayName.Equals('Working Knowledge', [System.StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    return $false
}

function Test-LooksLikeModRoot {
    param([Parameter(Mandatory = $true)][string] $Path)

    return (Test-Path -LiteralPath (Join-Path $Path 'Data')) -or
        (Test-Path -LiteralPath (Join-Path $Path 'modinfo.sbc')) -or
        ($null -ne (Get-ChildItem -LiteralPath $Path -File -Filter '*.sbc' -ErrorAction SilentlyContinue | Select-Object -First 1))
}

function Get-BlockSetCandidates {
    param(
        [Parameter(Mandatory = $true)][string] $ScanRoot,
        [Parameter(Mandatory = $true)][System.Collections.Generic.HashSet[string]] $KnownWorkingKnowledgeBlockKeys
    )

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
        if (Test-IsWorkingKnowledgeSourceMod -Root $candidateRoot -DisplayName $name) {
            Write-Host ("Skipping block set: {0} - this is Working Knowledge itself." -f $name)
            continue
        }

        Write-Host ("Scanning block set: {0}" -f $name)
        $allBlocks = @(Get-BlockDefinitions -Root $candidateRoot)
        if ($allBlocks.Count -eq 0) {
            continue
        }

        $blocks = @($allBlocks | Where-Object { -not $KnownWorkingKnowledgeBlockKeys.Contains($_.Key) })
        $coveredBlockCount = $allBlocks.Count - $blocks.Count
        if ($blocks.Count -eq 0) {
            Write-Host ("Found block set: {0} - all {1} public blocks are already covered and can be explicitly remapped." -f $name, $allBlocks.Count)
        }
        if ($coveredBlockCount -gt 0) {
            Write-Host ("Ignoring {0} block(s) already covered by Working Knowledge." -f $coveredBlockCount)
        }

        $sets.Add([pscustomobject]@{
            Name = $name
            Path = $candidateRoot
            Blocks = $blocks
            AllBlocks = $allBlocks
            BlockCount = $blocks.Count
            PublicBlockCount = $allBlocks.Count
            CoveredBlockCount = $coveredBlockCount
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

function Get-VisibleBlockSets {
    param(
        [Parameter(Mandatory = $true)][object[]] $Sets,
        [Parameter(Mandatory = $true)][bool] $IncludeCovered
    )

    if ($IncludeCovered) {
        return @($Sets)
    }

    return @($Sets | Where-Object { $_.BlockCount -gt 0 })
}

function Select-BlockSets {
    param([Parameter(Mandatory = $true)][object[]] $Sets)

    $includeCovered = $false
    while ($true) {
        $visibleSets = @(Get-VisibleBlockSets -Sets $Sets -IncludeCovered $includeCovered)
        Write-Host ''
        Write-Host 'Select which block sets to include in this layer:'
        for ($i = 0; $i -lt $visibleSets.Count; $i++) {
            if ($includeCovered) {
                Write-Host ("[{0}] {1} - contains {2} public blocks ({3} new, {4} already covered)" -f
                    ($i + 1), $visibleSets[$i].Name, $visibleSets[$i].PublicBlockCount, $visibleSets[$i].BlockCount, $visibleSets[$i].CoveredBlockCount)
            }
            else {
                Write-Host ("[{0}] {1} - contains {2} new public blocks" -f ($i + 1), $visibleSets[$i].Name, $visibleSets[$i].BlockCount)
            }
            Write-Host ("    {0}" -f $visibleSets[$i].Path)
        }

        $nextOption = $visibleSets.Count + 1
        $allOption = 0
        if ($visibleSets.Count -gt 1) {
            $allOption = $nextOption
            $allLabel = if ($includeCovered) { 'Select all shown block sets and all public blocks' } else { 'Select all shown block sets' }
            Write-Host ("[{0}] {1}" -f $allOption, $allLabel)
            $nextOption++
        }

        $toggleOption = $nextOption
        $toggleLabel = if ($includeCovered) {
            'Show only block sets with new blocks'
        }
        else {
            'Show all block sets, including already-covered blocks for explicit remapping'
        }
        Write-Host ("[{0}] {1}" -f $toggleOption, $toggleLabel)

        $defaultSelection = if ($visibleSets.Count -eq 1) {
            '1'
        }
        elseif ($allOption -gt 0) {
            [string] $allOption
        }
        else {
            [string] $toggleOption
        }

        $choice = Read-Host "Please select block sets [default $defaultSelection] (example: 1 3)"
        if ([string]::IsNullOrWhiteSpace($choice)) {
            $choice = $defaultSelection
        }

        $selectedNumbers = @(Convert-ToSelectionNumbers -Text $choice -MaxValue $toggleOption)
        if ($null -eq $selectedNumbers -or $selectedNumbers.Count -eq 0) {
            Write-Host 'Enter one or more shown numbers separated by spaces, or choose the all option.'
            continue
        }

        if ($selectedNumbers -contains $toggleOption) {
            $includeCovered = -not $includeCovered
            continue
        }

        $selectedSets = if ($allOption -gt 0 -and $selectedNumbers -contains $allOption) {
            @($visibleSets)
        }
        else {
            @($selectedNumbers | ForEach-Object { $visibleSets[$_ - 1] })
        }

        return [pscustomobject]@{
            SelectedSets = @($selectedSets)
            IncludeCovered = $includeCovered
        }
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

function Resolve-OutlierAction {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string] $Choice)

    if ([string]::IsNullOrWhiteSpace($Choice)) {
        return 'Keep'
    }

    switch ($Choice.Trim()) {
        '1' { return 'Change' }
        '2' { return 'Stop' }
        '3' { return 'Keep' }
        default { return $null }
    }
}

function Select-OutlierAction {
    while ($true) {
        Write-Host '[1] Choose another schematic group'
        Write-Host '[2] Stop reviewing outliers'
        Write-Host '[3] Keep the current assignment (default)'
        $choice = Read-Host 'Choose action [3]'
        $action = Resolve-OutlierAction -Choice $choice
        if ($null -ne $action) {
            return $action
        }

        Write-Host 'Enter 1, 2, or 3.'
    }
}

function Select-SchematicTier {
    $tiers = @('Common', 'Uncommon', 'Rare', 'Prototech', 'None')
    while ($true) {
        Write-Host ''
        for ($i = 0; $i -lt $tiers.Count; $i++) {
            Write-Host ("[{0}] {1}" -f ($i + 1), $tiers[$i])
        }
        $choice = Read-Host 'Choose custom group tier [Common]'
        if ([string]::IsNullOrWhiteSpace($choice)) {
            return 'Common'
        }
        $index = 0
        if ([int]::TryParse($choice, [ref] $index) -and $index -ge 1 -and $index -le $tiers.Count) {
            return $tiers[$index - 1]
        }
        $match = @($tiers | Where-Object { $_ -ieq $choice })
        if ($match.Count -eq 1) {
            return $match[0]
        }
        Write-Host 'Enter a shown number or tier name.'
    }
}

function New-CustomSchematicGroups {
    param([Parameter(Mandatory = $true)][string] $Namespace)

    $groups = [System.Collections.Generic.List[object]]::new()
    $answer = Read-Host 'Define custom schematic groups for this layer? [y/N]'
    if ($answer -notmatch '^[Yy]') {
        return @()
    }

    $namespaceToken = Get-SafeSubtypeToken -Value $Namespace
    while ($true) {
        Write-Host ''
        $id = (Read-Host 'Stable custom schematic ID (example: examplemod.power)').Trim()
        if ($id -notmatch '^[A-Za-z0-9](?:[A-Za-z0-9._-]*[A-Za-z0-9])?$') {
            Write-Host 'Use letters, digits, dots, underscores, or hyphens; begin and end with a letter or digit.'
            continue
        }
        if (@($groups | Where-Object { $_.id -ieq $id }).Count -gt 0) {
            Write-Host 'That custom schematic ID is already present.'
            continue
        }

        $defaultDisplayName = (($id -split '[._-]+' | ForEach-Object {
            if ($_.Length -eq 0) { return }
            $_.Substring(0, 1).ToUpperInvariant() + $_.Substring(1)
        }) -join ' ') + ' Schematics'
        $displayName = Read-Host "Display name [$defaultDisplayName]"
        if ([string]::IsNullOrWhiteSpace($displayName)) {
            $displayName = $defaultDisplayName
        }
        if ($displayName.Contains('|') -or $displayName.Contains('#')) {
            Write-Host "Display names cannot contain '|' or '#'."
            continue
        }

        $tier = Select-SchematicTier
        $descriptionSubject = ($displayName -replace ' Schematics$', '').ToLowerInvariant()
        $defaultDescription = "Schematics for $descriptionSubject systems supplied by this compatibility layer."
        $description = Read-Host "Description [$defaultDescription]"
        if ([string]::IsNullOrWhiteSpace($description)) {
            $description = $defaultDescription
        }
        if ($description.Contains('|') -or $description.Contains('#')) {
            Write-Host "Descriptions cannot contain '|' or '#'."
            continue
        }
        $idToken = Get-SafeSubtypeToken -Value $id
        $groups.Add([pscustomobject]@{
            id = $id
            displayName = $displayName
            tier = $tier
            hint = 'Custom schematic group defined by this layer.'
            description = $description
            groupSubtype = "WkKnLayer_${namespaceToken}_${idToken}"
            unlockerSubtype = "WkKnUnlocker_${namespaceToken}_${idToken}"
            isCustom = $true
        }) | Out-Null

        $more = Read-Host 'Add another custom schematic group? [y/N]'
        if ($more -notmatch '^[Yy]') {
            break
        }
    }

    return @($groups)
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
        $prefix = if ($mapping.IsOverride) { 'override ' } else { '' }
        $lines.Add(('{0}{1} = {2}' -f $prefix, $mapping.Key, $mapping.SchematicId)) | Out-Null
    }

    return ($lines -join [Environment]::NewLine)
}

function Convert-ToCustomGroupLines {
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]] $Groups)

    return (($Groups | ForEach-Object {
        '{0} | {1} | {2} | {3} | {4} | {5}' -f $_.id, $_.displayName, $_.tier, $_.groupSubtype, $_.unlockerSubtype, $_.description
    }) -join [Environment]::NewLine)
}

function Convert-ToResearchGroupEntries {
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]] $Groups)

    return (($Groups | ForEach-Object {
@"
    <ResearchGroup xsi:type="ResearchGroup">
      <Id Type="MyObjectBuilder_ResearchGroupDefinition" Subtype="$([System.Security.SecurityElement]::Escape($_.groupSubtype))" />
      <Members />
    </ResearchGroup>
"@
    }) -join [Environment]::NewLine).TrimEnd()
}

function Convert-ToUnlockerEntries {
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]] $Groups)

    return (($Groups | ForEach-Object {
        $subtype = [System.Security.SecurityElement]::Escape($_.unlockerSubtype)
        $displayName = [System.Security.SecurityElement]::Escape($_.displayName)
@"
    <Definition>
      <Id><TypeId>CubeBlock</TypeId><SubtypeId>$subtype</SubtypeId></Id>
      <DisplayName>$displayName</DisplayName>
      <Icon>Textures\GUI\Icons\Items\Datapad_Item.dds</Icon>
      <Public>true</Public><GuiVisible>false</GuiVisible>
      <Description>$([System.Security.SecurityElement]::Escape($_.description))</Description>
      <BlockPairName>$subtype</BlockPairName>
      <CubeSize>Small</CubeSize><BlockTopology>TriangleMesh</BlockTopology>
      <Size x="1" y="1" z="1" /><ModelOffset x="0" y="0" z="0" />
      <Model>Models\Items\Datapad_Item.mwm</Model>
      <Components><Component Subtype="WkKnDataFragmentComponent" Count="100" /></Components>
      <CriticalComponent Subtype="WkKnDataFragmentComponent" Index="0" />
      <MountPoints>
        <MountPoint Side="Front" StartX="0" StartY="0" EndX="1" EndY="1" />
        <MountPoint Side="Back" StartX="0" StartY="0" EndX="1" EndY="1" />
        <MountPoint Side="Left" StartX="0" StartY="0" EndX="1" EndY="1" />
        <MountPoint Side="Right" StartX="0" StartY="0" EndX="1" EndY="1" />
        <MountPoint Side="Bottom" StartX="0" StartY="0" EndX="1" EndY="1" />
        <MountPoint Side="Top" StartX="0" StartY="0" EndX="1" EndY="1" />
      </MountPoints>
      <BuildProgressModels><Model BuildPercentUpperBound="1.00" File="Models\Items\Datapad_Item.mwm" /></BuildProgressModels>
      <DeformationRatio>0.32</DeformationRatio><EdgeType>Light</EdgeType>
      <BuildTimeSeconds>3</BuildTimeSeconds><PCU>1</PCU><IsAirTight>false</IsAirTight>
    </Definition>
"@
    }) -join [Environment]::NewLine).TrimEnd()
}

function Convert-ToSchematicItemEntries {
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]] $Groups)

    return (($Groups | ForEach-Object {
        $subtype = 'WkKnSchematic_' + (Get-SafeSubtypeToken -Value $_.id)
        $displayName = [System.Security.SecurityElement]::Escape(($_.displayName -replace ' Schematics$', '') + ' Data Schematic')
        $description = [System.Security.SecurityElement]::Escape("$($_.description) This durable data schematic is returned after use so it can be shared.")
@"
    <PhysicalItem xsi:type="MyObjectBuilder_ConsumableItemDefinition">
      <Id><TypeId>ConsumableItem</TypeId><SubtypeId>$subtype</SubtypeId></Id>
      <DisplayName>$displayName</DisplayName><Description>$description</Description>
      <Icon>Textures\GUI\Icons\Items\Datapad_Item.dds</Icon>
      <Size><X>0.2</X><Y>0.1</Y><Z>0.2</Z></Size><Mass>0.2</Mass><Volume>0.05</Volume>
      <Model>Models\Items\Datapad_Item.mwm</Model><PhysicalMaterial>Metal</PhysicalMaterial>
      <Stats><Stat Name="RadiationImmunity" Value="1" Time="1" /></Stats><UseSound>PlayUsePowerKit</UseSound>
      <DepositAllEnabled>false</DepositAllEnabled><CanPlayerOrder>false</CanPlayerOrder><CanPlayerOffer>false</CanPlayerOffer>
    </PhysicalItem>
"@
    }) -join [Environment]::NewLine).TrimEnd()
}

function Remove-GeneratedCustomGroupFiles {
    param([Parameter(Mandatory = $true)][string] $OutputPath)

    $generatedFiles = @(
        'Data\WorkingKnowledge\schematic_groups.txt',
        'Data\ResearchUnlockerGroups.sbc',
        'Data\ResearchUnlockers.sbc',
        'Data\PhysicalItems_ResearchSchematics.sbc'
    )

    foreach ($relativePath in $generatedFiles) {
        $path = Join-Path $OutputPath $relativePath
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            Remove-Item -LiteralPath $path -Force
        }
    }
}

function Validate-GeneratedLayer {
    param(
        [Parameter(Mandatory = $true)][string] $OutputPath,
        [Parameter(Mandatory = $true)][object[]] $Mappings,
        [Parameter(Mandatory = $true)][object[]] $Groups,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]] $CustomGroups
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

    if ($CustomGroups.Count -gt 0) {
        foreach ($relativePath in @('Data\ResearchUnlockerGroups.sbc', 'Data\ResearchUnlockers.sbc', 'Data\PhysicalItems_ResearchSchematics.sbc')) {
            [xml] (Get-Content -LiteralPath (Join-Path $OutputPath $relativePath) -Raw) | Out-Null
        }
        $groupFile = Get-Content -LiteralPath (Join-Path $OutputPath 'Data\WorkingKnowledge\schematic_groups.txt')
        $versionLines = @($groupFile | Where-Object { $_ -match '^\s*version\s*=\s*1\s*$' })
        if ($versionLines.Count -ne 1) {
            throw 'Generated schematic_groups.txt does not declare version = 1.'
        }
    }
}

if ($SelfTest) {
    $examplePath = Join-Path $ScriptRoot 'ExampleMod'
    $exampleGroups = @(
        [pscustomobject]@{ id = 'example.truss_system' },
        [pscustomobject]@{ id = 'example.power_storage' }
    )
    $exampleMappings = @(
        [pscustomobject]@{ SchematicId = 'example.truss_system' },
        [pscustomobject]@{ SchematicId = 'example.power_storage' }
    )
    $oneCustomGroup = @([pscustomobject]@{ id = 'example.truss_system' })

    Validate-GeneratedLayer `
        -OutputPath $examplePath `
        -Mappings $exampleMappings `
        -Groups $exampleGroups `
        -CustomGroups $oneCustomGroup

    $visibilityFixtures = @(
        [pscustomobject]@{ Name = 'New'; BlockCount = 2; CoveredBlockCount = 0 },
        [pscustomobject]@{ Name = 'CoveredOnly'; BlockCount = 0; CoveredBlockCount = 3 },
        [pscustomobject]@{ Name = 'Mixed'; BlockCount = 4; CoveredBlockCount = 1 }
    )
    $defaultVisible = @(Get-VisibleBlockSets -Sets $visibilityFixtures -IncludeCovered $false)
    $advancedVisible = @(Get-VisibleBlockSets -Sets $visibilityFixtures -IncludeCovered $true)
    if ($defaultVisible.Count -ne 2 -or @($defaultVisible | Where-Object { $_.Name -eq 'CoveredOnly' }).Count -ne 0) {
        throw 'Toolkit self-test failed: default block-set selection did not hide covered-only sets.'
    }
    if ($advancedVisible.Count -ne 3) {
        throw 'Toolkit self-test failed: explicit remapping did not reveal covered-only sets.'
    }
    $emptyGroups = @()
    $emptyOutputs = @(
        (Convert-ToCustomGroupLines -Groups $emptyGroups),
        (Convert-ToResearchGroupEntries -Groups $emptyGroups),
        (Convert-ToUnlockerEntries -Groups $emptyGroups),
        (Convert-ToSchematicItemEntries -Groups $emptyGroups)
    )
    if (@($emptyOutputs | Where-Object { -not [string]::IsNullOrEmpty([string] $_) }).Count -gt 0) {
        throw 'Toolkit self-test failed: zero custom groups did not produce empty template sections.'
    }
    $cleanupFixture = Join-Path ([System.IO.Path]::GetTempPath()) ("WorkingKnowledgeLayerToolkit-{0}" -f [System.Guid]::NewGuid().ToString('N'))
    try {
        foreach ($relativePath in @(
            'Data\WorkingKnowledge\schematic_groups.txt',
            'Data\ResearchUnlockerGroups.sbc',
            'Data\ResearchUnlockers.sbc',
            'Data\PhysicalItems_ResearchSchematics.sbc',
            'Data\WorkingKnowledge\manual_notes.txt'
        )) {
            Write-TextNoBom -Path (Join-Path $cleanupFixture $relativePath) -Text 'fixture'
        }
        Remove-GeneratedCustomGroupFiles -OutputPath $cleanupFixture
        foreach ($relativePath in @(
            'Data\WorkingKnowledge\schematic_groups.txt',
            'Data\ResearchUnlockerGroups.sbc',
            'Data\ResearchUnlockers.sbc',
            'Data\PhysicalItems_ResearchSchematics.sbc'
        )) {
            if (Test-Path -LiteralPath (Join-Path $cleanupFixture $relativePath)) {
                throw "Toolkit self-test failed: obsolete generated file was not removed: $relativePath"
            }
        }
        if (-not (Test-Path -LiteralPath (Join-Path $cleanupFixture 'Data\WorkingKnowledge\manual_notes.txt') -PathType Leaf)) {
            throw 'Toolkit self-test failed: custom-group cleanup removed an unrelated file.'
        }
    }
    finally {
        if (Test-Path -LiteralPath $cleanupFixture) {
            Remove-Item -LiteralPath $cleanupFixture -Recurse -Force
        }
    }
    if ((Resolve-OutlierAction -Choice '') -ne 'Keep' -or
        (Resolve-OutlierAction -Choice '1') -ne 'Change' -or
        (Resolve-OutlierAction -Choice '2') -ne 'Stop' -or
        (Resolve-OutlierAction -Choice '3') -ne 'Keep' -or
        $null -ne (Resolve-OutlierAction -Choice 'invalid')) {
        throw 'Toolkit self-test failed: outlier action choices are not mapped correctly.'
    }
    Write-Host 'Working Knowledge Layer Toolkit generator self-test passed.'
    return
}

Write-Host 'Working Knowledge Layer Toolkit'
Write-Host 'Creates a compatibility layer for a Space Engineers block mod.'

$rawGroups = Get-Content -LiteralPath $GroupDataPath -Raw | ConvertFrom-Json
$groups = @($rawGroups | ForEach-Object { $_ })
if ($groups.Count -eq 0) {
    throw "No schematic groups found in $GroupDataPath"
}
$knownWorkingKnowledgeBlockKeys = Read-BlockKeySet -Path $KnownWorkingKnowledgeBlocksPath

$scanRoot = Select-Path
Write-Host ''
Write-Host 'Scanning for block sets. This can take a moment for large Workshop folders.'
$blockSets = @(Get-BlockSetCandidates -ScanRoot $scanRoot -KnownWorkingKnowledgeBlockKeys $knownWorkingKnowledgeBlockKeys)
if ($blockSets.Count -eq 0) {
    throw 'No block sets with public cube block definitions were found in the selected folder.'
}

$selection = Select-BlockSets -Sets $blockSets
$selectedSets = @($selection.SelectedSets)
$useOverrides = [bool] $selection.IncludeCovered
$sourceModName = if ($selectedSets.Count -eq 1) {
    [string] $selectedSets[0].Name
}
elseif ($selectedSets.Count -le 3) {
    [string] (($selectedSets | ForEach-Object { $_.Name }) -join ', ')
}
else {
    'Selected Space Engineers block mods'
}

$blocks = if ($useOverrides) {
    @($selectedSets | ForEach-Object { $_.AllBlocks } | Sort-Object Key -Unique)
}
else {
    @($selectedSets | ForEach-Object { $_.Blocks } | Sort-Object Key -Unique)
}
if ($blocks.Count -eq 0) {
    throw 'The selected sets contain no blocks for the chosen mapping mode.'
}
Write-Host ''
Write-Host ("Selected {0} block set(s), containing {1} unique public block definitions." -f $selectedSets.Count, $blocks.Count)
$blocks | Select-Object -First 20 Key, DisplayName, CubeSize, BlockPairName | Format-Table -AutoSize
if ($blocks.Count -gt 20) {
    Write-Host "...and $($blocks.Count - 20) more."
}

$customGroups = @(New-CustomSchematicGroups -Namespace $sourceModName)
foreach ($customGroup in $customGroups) {
    if (@($groups | Where-Object { $_.id -ieq $customGroup.id }).Count -gt 0) {
        throw "Custom schematic ID '$($customGroup.id)' conflicts with a built-in group. Choose a namespaced ID."
    }
}
$groups = @($groups + $customGroups)

$defaultGroupId = Select-SchematicGroup -Groups $groups -Prompt 'Choose the default schematic group for these blocks.' -DefaultId 'structure.industrial'

$mappings = @($blocks | ForEach-Object {
    [pscustomobject]@{
        Key = $_.Key
        SchematicId = $defaultGroupId
        DisplayName = $_.DisplayName
        CubeSize = $_.CubeSize
        BlockPairName = $_.BlockPairName
        IsOverride = $knownWorkingKnowledgeBlockKeys.Contains($_.Key)
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
        $action = Select-OutlierAction
        if ($action -eq 'Stop') {
            break
        }
        if ($action -eq 'Change') {
            $mapping.SchematicId = Select-SchematicGroup -Groups $groups -Prompt 'Choose replacement schematic group.' -DefaultId $mapping.SchematicId
        }
    }
}

$defaultLayerName = "Working Knowledge Layer - $sourceModName"
$layerName = Read-Host "Layer display name [$defaultLayerName]"
if ([string]::IsNullOrWhiteSpace($layerName)) {
    $layerName = $defaultLayerName
}

$defaultAuthor = if ([string]::IsNullOrWhiteSpace($env:USERNAME)) { 'Your Name' } else { $env:USERNAME }
$author = Read-Host "Layer author / maker name [$defaultAuthor]"
if ([string]::IsNullOrWhiteSpace($author)) {
    $author = $defaultAuthor
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
    AUTHOR = $author
    RESEARCH_BLOCKS = Convert-ToResearchBlockEntries -Mappings $mappings
    BLOCK_MAPPINGS = Convert-ToMappingLines -Mappings $mappings
    SCHEMATIC_GROUPS = Convert-ToCustomGroupLines -Groups $customGroups
    RESEARCH_GROUPS = Convert-ToResearchGroupEntries -Groups $customGroups
    RESEARCH_UNLOCKERS = Convert-ToUnlockerEntries -Groups $customGroups
    SCHEMATIC_ITEMS = Convert-ToSchematicItemEntries -Groups $customGroups
}

Write-Host ''
Write-Host "Creating layer: $outputPath"

Write-TextNoBom -Path (Join-Path $outputPath 'README.md') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'README.md.template')) -Tokens $tokens)
Write-TextNoBom -Path (Join-Path $outputPath 'modinfo.sbc') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'modinfo.sbc.template')) -Tokens $tokens)
Write-TextNoBom -Path (Join-Path $outputPath 'Data\ResearchBlocks.sbc') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'Data\ResearchBlocks.sbc.template')) -Tokens $tokens)
Write-TextNoBom -Path (Join-Path $outputPath 'Data\WorkingKnowledge\block_mappings.txt') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'Data\WorkingKnowledge\block_mappings.txt.template')) -Tokens $tokens)
if ($customGroups.Count -gt 0) {
    Write-TextNoBom -Path (Join-Path $outputPath 'Data\WorkingKnowledge\schematic_groups.txt') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'Data\WorkingKnowledge\schematic_groups.txt.template')) -Tokens $tokens)
    Write-TextNoBom -Path (Join-Path $outputPath 'Data\ResearchUnlockerGroups.sbc') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'Data\ResearchUnlockerGroups.sbc.template')) -Tokens $tokens)
    Write-TextNoBom -Path (Join-Path $outputPath 'Data\ResearchUnlockers.sbc') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'Data\ResearchUnlockers.sbc.template')) -Tokens $tokens)
    Write-TextNoBom -Path (Join-Path $outputPath 'Data\PhysicalItems_ResearchSchematics.sbc') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'Data\PhysicalItems_ResearchSchematics.sbc.template')) -Tokens $tokens)
}
else {
    Remove-GeneratedCustomGroupFiles -OutputPath $outputPath
}
Write-TextNoBom -Path (Join-Path $outputPath 'Publishing\changelog.md') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'Publishing\changelog.md.template')) -Tokens $tokens)
Write-TextNoBom -Path (Join-Path $outputPath 'Publishing\workshop_description_bbcode.txt') -Text (Expand-Template -Text (Read-TextNoBom (Join-Path $TemplateRoot 'Publishing\workshop_description_bbcode.txt.template')) -Tokens $tokens)

Validate-GeneratedLayer -OutputPath $outputPath -Mappings $mappings -Groups $groups -CustomGroups $customGroups
& (Join-Path $ScriptRoot 'Validate.ps1') -LayerPath $outputPath

Write-Host ''
Write-Host 'Done.'
Write-Host "Generated layer: $outputPath"
Write-Host "Mapped blocks: $($mappings.Count)"
Write-Host "Custom schematic groups: $($customGroups.Count)"
Write-Host 'Run .\Validate.ps1 -LayerPath <generated folder> after any manual edits.'
