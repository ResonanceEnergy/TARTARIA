import bpy, os, sys
PATHS = [
    "MiloBoy.fbx",
    "Anastasia.fbx",
    "Lirael.fbx",
    "Cassian.fbx",
    "BobsInn.fbx",
    "VillageBakery.fbx",
    "Apothecary.fbx",
    "TownHall.fbx",
    "Watchtower.fbx",
    "VillageCottageA.fbx",
    "VillageMill.fbx",
    "VillageSmithy.fbx",
    "VillageInn.fbx",
    "AnastasiaPrincess.fbx",
    "LiraelGuardian.fbx",
    "CassianCarter.fbx",
]
ROOT = r"C:\dev\TARTARIA_new\Assets\_Project\Models\Blender\Moon1"
def reset():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
for f in PATHS:
    full = os.path.join(ROOT, f)
    if not os.path.exists(full):
        print("MISSING %s" % f); continue
    reset()
    bpy.ops.import_scene.fbx(filepath=full)
    objs = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    if not objs:
        print("%s NO_MESHES" % f); continue
    mins = [1e9,1e9,1e9]; maxs = [-1e9,-1e9,-1e9]
    for o in objs:
        for v in o.bound_box:
            wv = o.matrix_world @ type(o.matrix_world.translation)((v[0],v[1],v[2]))
            for i in range(3):
                if wv[i] < mins[i]: mins[i] = wv[i]
                if wv[i] > maxs[i]: maxs[i] = wv[i]
    sx = maxs[0]-mins[0]; sy = maxs[1]-mins[1]; sz = maxs[2]-mins[2]
    print("BBOX %s X=%.2fm Y=%.2fm Z=%.2fm MAX=%.2fm" % (f, sx, sy, sz, max(sx,sy,sz)))
