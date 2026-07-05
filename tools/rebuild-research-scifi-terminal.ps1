[CmdletBinding()]
param(
    [string]$BlenderPath = 'C:\Program Files\Blender Foundation\Blender 4.5\blender.exe',
    [string]$ModSdkRoot = 'C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineersModSDK',
    [switch]$KeepScratch
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$stage = Join-Path $repoRoot '.tmp\ResearchSciFiTerminalBuild'
$stageFull = [System.IO.Path]::GetFullPath($stage)
$repoFull = [System.IO.Path]::GetFullPath($repoRoot)

if (-not $stageFull.StartsWith($repoFull, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to rebuild outside repo workspace: $stageFull"
}

$builder = Join-Path $PSScriptRoot 'MWMBuilder\MwmBuilder.exe'
if (-not (Test-Path -LiteralPath $builder)) {
    & (Join-Path $PSScriptRoot 'install-mwmbuilder.ps1')
}

if (-not (Test-Path -LiteralPath $BlenderPath)) {
    throw "Blender was not found at $BlenderPath"
}

$originalContent = Join-Path $ModSdkRoot 'OriginalContent'
if (-not (Test-Path -LiteralPath $originalContent)) {
    throw "Space Engineers ModSDK OriginalContent was not found at $originalContent"
}

if (Test-Path -LiteralPath $stage) {
    Remove-Item -LiteralPath $stage -Recurse -Force
}

$content = Join-Path $stage 'Content'
$modelDir = Join-Path $content 'Models\Cubes\Large'
New-Item -ItemType Directory -Force -Path $modelDir | Out-Null

$python = Join-Path $stage 'rebuild_research_scifi_terminal.py'
@'
import os
import shutil
import bpy

repo = os.environ["WkKn_REPO_ROOT"]
sdk = os.environ["WkKn_MODSDK_ORIGINAL_CONTENT"]
stage = os.environ["WkKn_STAGE_CONTENT"]
models = os.path.join(stage, "Models", "Cubes", "Large")

source_fbx = os.path.join(sdk, "Models", "Cubes", "large", "SciFiTerminal.fbx")
source_xml = os.path.join(sdk, "Models", "Cubes", "large", "SciFiTerminal.xml")
source_hkt = os.path.join(sdk, "Models", "Cubes", "large", "SciFiTerminal.hkt")
target_fbx = os.path.join(models, "SciFiTerminal.fbx")
target_xml = os.path.join(models, "SciFiTerminal.xml")
target_hkt = os.path.join(models, "SciFiTerminal.hkt")

shutil.copyfile(source_xml, target_xml)
if os.path.exists(source_hkt):
    shutil.copyfile(source_hkt, target_hkt)

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete()
bpy.ops.import_scene.fbx(filepath=source_fbx)

source_screen = bpy.data.objects.get("Screen_section_01")
if source_screen is None:
    raise RuntimeError("Screen_section_01 was not found in the SDK SciFiTerminal FBX.")

source_matrix = source_screen.matrix_world.copy()
source_vertices = [vertex.co.copy() for vertex in source_screen.data.vertices]
source_polygons = [list(polygon.vertices) for polygon in source_screen.data.polygons]
source_normals = [polygon.normal.copy() for polygon in source_screen.data.polygons]

source_material = None
for slot in source_screen.material_slots:
    if slot.material and slot.material.name == "CockpitFighter_Interior":
        source_material = slot.material
        break

if source_material is None:
    raise RuntimeError("CockpitFighter_Interior material was not found on Screen_section_01.")

def screen_material(name):
    existing = bpy.data.materials.get(name)
    if existing is not None:
        bpy.data.materials.remove(existing, do_unlink=True)

    material = source_material.copy()
    material.name = name
    return material

cockpit_01 = screen_material("CockpitScreen_01")
cockpit_02 = screen_material("CockpitScreen_02")

def build_screen_object(name, source_polygon_indices, uv_by_source_vertex, materials):
    vertex_map = {}
    vertices = []
    faces = []
    face_source_vertices = []
    face_material_indices = []

    for polygon_index in source_polygon_indices:
        polygon_vertices = source_polygons[polygon_index]
        face = []
        face_vertices = []
        for source_vertex in polygon_vertices:
            if source_vertex not in vertex_map:
                vertex_map[source_vertex] = len(vertices)
                # Keep the LCD overlay just above the vanilla decorative screen
                # faces so it renders cleanly without fighting the original mesh.
                vertices.append(source_vertices[source_vertex].copy() + (source_normals[polygon_index] * 0.1))
            face.append(vertex_map[source_vertex])
            face_vertices.append(source_vertex)
        faces.append(face)
        face_source_vertices.append(face_vertices)
        face_material_indices.append(0 if polygon_index in (0, 1) else 1)

    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    for material in materials:
        mesh.materials.append(material)

    uv_layer = mesh.uv_layers.new(name="UVMap")
    for polygon, source_face, material_index in zip(mesh.polygons, face_source_vertices, face_material_indices):
        polygon.material_index = material_index
        for loop_index, source_vertex in zip(polygon.loop_indices, source_face):
            uv_layer.data[loop_index].uv = uv_by_source_vertex[source_vertex]

    obj = bpy.data.objects.new(name, mesh)
    obj.matrix_world = source_matrix.copy()
    bpy.context.collection.objects.link(obj)
    if source_screen.parent is not None:
        obj.parent = source_screen.parent
        obj.matrix_parent_inverse = source_screen.parent.matrix_world.inverted()
    return obj

# The SDK model ships the visible monitor/keyboard faces as a decorative mesh.
# Keep that mesh intact because MWMBuilder derives Screen_section_01 metadata
# from it, then add a real LCD overlay under the same parent branch.
build_screen_object(
    "CustomScreenOverlay",
    (0, 1, 2, 3),
    {
        # MWMBuilder imports FBX UVs with V = 1 - sourceV. These source UVs
        # intentionally account for that conversion so the MWM render targets
        # are upright on both the main and keyboard displays.
        0: (1.0, 1.0),
        1: (0.0, 1.0),
        2: (0.0, 0.0),
        3: (1.0, 0.0),
        4: (0.0, 0.0),
        5: (1.0, 0.0),
        6: (1.0, 1.0),
        7: (0.0, 1.0),
    },
    (cockpit_01, cockpit_02),
)

for obj in bpy.data.objects:
    obj.select_set(True)

bpy.ops.export_scene.fbx(
    filepath=target_fbx,
    use_selection=True,
    object_types={"EMPTY", "MESH"},
    apply_unit_scale=True,
    add_leaf_bones=False,
    use_custom_props=True,
    path_mode="AUTO",
)
'@ | Set-Content -LiteralPath $python -Encoding UTF8

$env:WkKn_REPO_ROOT = $repoFull
$env:WkKn_MODSDK_ORIGINAL_CONTENT = [System.IO.Path]::GetFullPath($originalContent)
$env:WkKn_STAGE_CONTENT = [System.IO.Path]::GetFullPath($content)

& $BlenderPath --background --python $python

Copy-Item -LiteralPath (Join-Path $originalContent 'Materials') -Destination $content -Recurse -Force

& $builder /f "/s:$modelDir" "/m:SciFiTerminal*.fbx" "/o:$modelDir" "/x:$(Join-Path $content 'Materials')" "/l:$(Join-Path $stage 'build.log')"

$builtModel = Join-Path $modelDir 'SciFiTerminal.mwm'
if (-not (Test-Path -LiteralPath $builtModel)) {
    throw "MWMBuilder did not produce $builtModel"
}

$modModel = Join-Path $repoRoot 'mods\WorkingKnowledge\Models\Cubes\Large\SciFiTerminal.mwm'
Copy-Item -LiteralPath $builtModel -Destination $modModel -Force

Write-Host "Rebuilt SciFiTerminal.mwm -> $modModel"

if (-not $KeepScratch -and (Test-Path -LiteralPath $stage)) {
    Remove-Item -LiteralPath $stage -Recurse -Force
}
