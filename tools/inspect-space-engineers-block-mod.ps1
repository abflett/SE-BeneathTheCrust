[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $ModPath,
    [string] $OutputPath,
    [switch] $IncludeNonPublic
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

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
            Write-Warning "Skipping unreadable SBC file: $($file.FullName)"
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
            if (-not $IncludeNonPublic -and -not $isPublic) {
                continue
            }

            $rows.Add([pscustomobject]@{
                Key = $id.Key
                Type = $id.Type
                Subtype = $id.Subtype
                DisplayName = if ($definition.PSObject.Properties['DisplayName']) { Get-Text $definition.DisplayName } else { '' }
                BlockPairName = if ($definition.PSObject.Properties['BlockPairName']) { Get-Text $definition.BlockPairName } else { '' }
                CubeSize = if ($definition.PSObject.Properties['CubeSize']) { Get-Text $definition.CubeSize } else { '' }
                Public = $isPublic
                File = Get-RelativePath -BasePath $Root -TargetPath $file.FullName
            }) | Out-Null
        }
    }

    return @($rows | Sort-Object Key)
}

$resolvedModPath = (Resolve-Path -LiteralPath $ModPath).Path
$blocks = @(Get-BlockDefinitions -Root $resolvedModPath)

if ($blocks.Count -eq 0) {
    Write-Warning "No cube block definitions found under: $resolvedModPath"
}
else {
    $blocks | Format-Table Key, DisplayName, CubeSize, BlockPairName, File -AutoSize
}

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $outputFullPath = [System.IO.Path]::GetFullPath($OutputPath)
    $outputDirectory = Split-Path -Parent $outputFullPath
    if (-not [string]::IsNullOrWhiteSpace($outputDirectory) -and -not (Test-Path -LiteralPath $outputDirectory)) {
        New-Item -ItemType Directory -Path $outputDirectory | Out-Null
    }

    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllLines($outputFullPath, [string[]] @($blocks | ForEach-Object { $_.Key }), $encoding)
    Write-Host "Wrote block ID list: $outputFullPath"
}
