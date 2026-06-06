"""Quick bbox audit — read FBX via Blender import + report dims."""
import bpy, os, sys, json

TARGETS = [
    ("Moon1", "ResetScout"),
    ("Moon1", "WaveformPillar"),
    ("Moon1", "TuningBell_High"),
    ("Moon1", "TuningBell_Mid"),
    ("Moon1", "TuningBell_Low"),
    ("Moon1", "FrequencySliderStand"),
    ("Moon1", "Apothecary"),
    ("Moon1", "Watchtower"),
]

BASE = r"C:\dev\TARTARIA_new\Assets\_Project\Models\Blender"
results = {}

for moon, name in TARGETS:
    path = os.path.join(BASE, moon, name + ".fbx")
    if not os.path.isfile(path):
        results[name] = {"err": "missing"}
        continue
    # Fresh scene per FBX
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for b in list(bpy.data.meshes): bpy.data.meshes.remove(b)
    try:
        bpy.ops.import_scene.fbx(filepath=path)
    except Exception as e:
        results[name] = {"err": str(e)[:60]}
        continue
    # Compute combined bbox of all mesh objects
    minv = [1e9, 1e9, 1e9]
    maxv = [-1e9, -1e9, -1e9]
    found_mesh = False
    for ob in bpy.context.scene.objects:
        if ob.type != "MESH":
            continue
        found_mesh = True
        for v in ob.bound_box:
            world = ob.matrix_world @ __import__("mathutils").Vector(v)
            for i in range(3):
                if world[i] < minv[i]: minv[i] = world[i]
                if world[i] > maxv[i]: maxv[i] = world[i]
    if not found_mesh:
        results[name] = {"err": "no_mesh"}
        continue
    sx = round(maxv[0] - minv[0], 3)
    sy = round(maxv[1] - minv[1], 3)
    sz = round(maxv[2] - minv[2], 3)
    results[name] = {"sx": sx, "sy": sy, "sz": sz}
    print(f"  {name:30s}  WxHxD = {sx:7.3f} x {sy:7.3f} x {sz:7.3f}")

OUT = r"C:\Users\gripa\AppData\Local\Temp\fbx_bbox_postfix.json"
with open(OUT, "w") as f:
    json.dump(results, f, indent=2)
print("[TARTARIA] Wrote " + OUT)
