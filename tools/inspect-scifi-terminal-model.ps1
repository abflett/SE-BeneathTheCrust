[CmdletBinding()]
param(
    [string]$BlenderPath = 'C:\Program Files\Blender Foundation\Blender 4.5\blender.exe',
    [string]$FbxPath = 'C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineersModSDK\OriginalContent\Models\Cubes\large\SciFiTerminal.fbx',
    [string]$MwmPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'mods\WorkingKnowledge\Models\Cubes\Large\SciFiTerminal.mwm')
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $BlenderPath)) {
    throw "Blender was not found at $BlenderPath"
}

if (-not (Test-Path -LiteralPath $FbxPath)) {
    throw "FBX was not found at $FbxPath"
}

if (Test-Path -LiteralPath $MwmPath) {
    $bytes = [System.IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $MwmPath))
    $text = [System.Text.Encoding]::ASCII.GetString($bytes)
    Write-Host "MWM: $MwmPath"
    foreach ($marker in @('CockpitFighter_Interior', 'CockpitScreen_01', 'CockpitScreen_02', 'CustomScreenOverlay', 'Screen_section_01', 'SciFiTerminal', 'hkpBoxShape')) {
        $count = [regex]::Matches($text, [regex]::Escape($marker)).Count
        Write-Host ("  {0}={1}" -f $marker, $count)
    }

    Write-Host ("  Length={0}" -f $bytes.Length)
}

$script = Join-Path ([System.IO.Path]::GetTempPath()) 'inspect_scifi_terminal_model.py'
@'
import sys
import bpy

fbx = sys.argv[-1]

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete()
bpy.ops.import_scene.fbx(filepath=fbx)

print("FBX:", fbx)
for obj in bpy.data.objects:
    if obj.type != "MESH":
        continue

    materials = [slot.material.name if slot.material else "<none>" for slot in obj.material_slots]
    usage = {}
    for polygon in obj.data.polygons:
        material = materials[polygon.material_index] if polygon.material_index < len(materials) else str(polygon.material_index)
        usage[material] = usage.get(material, 0) + 1

    interesting = (
        "screen" in obj.name.lower()
        or "overlay" in obj.name.lower()
        or any(material in usage for material in ("CockpitScreen_01", "CockpitScreen_02", "CockpitFighter_Interior", "Terminal", "TextAtlas"))
    )

    if not interesting:
        continue

    print("OBJECT", obj.name)
    print("  verts:", len(obj.data.vertices), "polys:", len(obj.data.polygons))
    print("  material usage:", usage)

    uv_layer = obj.data.uv_layers.active.data if obj.data.uv_layers.active else None
    if uv_layer and obj.name in ("Screen_section_01", "CustomScreenOverlay"):
        for polygon in obj.data.polygons:
            center = obj.matrix_world @ polygon.center
            normal = obj.matrix_world.to_3x3() @ polygon.normal
            uvs = [tuple(round(v, 5) for v in uv_layer[index].uv) for index in polygon.loop_indices]
            print(
                "  poly",
                polygon.index,
                "mat",
                polygon.material_index,
                "verts",
                list(polygon.vertices),
                "center",
                tuple(round(value, 5) for value in center),
                "normal",
                tuple(round(value, 5) for value in normal),
                "uv",
                uvs,
            )
'@ | Set-Content -LiteralPath $script -Encoding UTF8

& $BlenderPath --background --python $script -- $FbxPath
