"""Moon 1 furniture — 12 assets for villager dressing: tables, chairs, benches, bookcases, etc."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus

MOON = "Moon1"
OAK = lambda: make_material("F_Oak", (0.42, 0.28, 0.16), roughness=0.65)
DARKWOOD = lambda: make_material("F_DarkWood", (0.22, 0.14, 0.08), roughness=0.65)
LINEN = lambda: make_material("F_Linen", (0.85, 0.78, 0.65), roughness=0.9)
IRON = lambda: make_material("F_Iron", (0.25, 0.22, 0.20), roughness=0.6, metallic=0.5)
CANDLE = lambda: make_material("F_Candle", (0.95, 0.90, 0.80), roughness=0.5, emission=(1.0,0.85,0.50), emission_strength=2.5)

def asset_RoundTable():
    reset_scene(); oak = OAK()
    cyl("Top", 0.75, 0.06, (0,0,0.78), oak)
    cyl("Pillar", 0.10, 0.7, (0,0,0.40), oak)
    cyl("Base", 0.40, 0.05, (0,0,0.05), oak)
    cube("BraceA", (0.20,0,0.10), (0.30,0.04,0.04), oak)
    cube("BraceB", (-0.20,0,0.10), (0.30,0.04,0.04), oak)
    cube("BraceC", (0,0.20,0.10), (0.04,0.30,0.04), oak)
    cube("BraceD", (0,-0.20,0.10), (0.04,0.30,0.04), oak)
    export_current_as("RoundTable", MOON)

def asset_LongDiningTable():
    reset_scene(); oak = OAK()
    cube("Top", (0,0,0.78), (1.4, 0.5, 0.06), oak)
    for (x,y) in [(-1.2,-0.4),(1.2,-0.4),(-1.2,0.4),(1.2,0.4)]:
        cube(f"Leg_{x}_{y}", (x,y,0.40), (0.08, 0.08, 0.8), oak)
    cube("Brace", (0,0,0.10), (2.4, 0.06, 0.06), oak)
    export_current_as("LongDiningTable", MOON)

def asset_PeasantChair():
    reset_scene(); oak = OAK()
    cube("Seat", (0,0,0.40), (0.40, 0.40, 0.05), oak)
    for (x,y) in [(-0.17,-0.17),(0.17,-0.17),(-0.17,0.17),(0.17,0.17)]:
        cube(f"Leg_{x}_{y}", (x,y,0.20), (0.04, 0.04, 0.4), oak)
    # 3 slat back
    for s in range(3):
        cube(f"BackSlat_{s}", (-0.05 + s*0.05, -0.18, 0.70), (0.03, 0.03, 0.50), oak)
    cube("TopRail", (0, -0.18, 0.95), (0.40, 0.04, 0.04), oak)
    export_current_as("PeasantChair", MOON)

def asset_LongBench():
    reset_scene(); oak = OAK()
    cube("Seat", (0,0,0.42), (1.2, 0.30, 0.06), oak)
    for x in [-1.0, 1.0]:
        cube(f"Leg_{x}_L", (x, -0.10, 0.21), (0.06, 0.06, 0.42), oak)
        cube(f"Leg_{x}_R", (x,  0.10, 0.21), (0.06, 0.06, 0.42), oak)
    cube("CrossL", (-1.0, 0, 0.10), (0.06, 0.25, 0.04), oak)
    cube("CrossR", ( 1.0, 0, 0.10), (0.06, 0.25, 0.04), oak)
    export_current_as("LongBench", MOON)

def asset_Bookshelf():
    reset_scene(); dark = DARKWOOD(); oak = OAK()
    cube("Frame", (0,0,1.1), (0.7, 0.20, 1.1), dark)
    for h in [0.30, 0.60, 0.90, 1.20, 1.50, 1.80]:
        cube(f"Shelf_{h}", (0, 0.02, h), (0.65, 0.18, 0.03), oak)
    # Books — varied colors via OAK
    for h, color_h in [(0.35, 0), (0.65, 0.1), (0.95, 0.2), (1.25, 0), (1.55, 0.1), (1.85, 0.2)]:
        for b in range(7):
            book_mat = make_material(f"Book_{h}_{b}", (0.5 + (b%3)*0.15, 0.25 + color_h, 0.20), roughness=0.6)
            cube(f"Book_{h}_{b}", (-0.30 + b*0.10, 0.06, h), (0.04, 0.13, 0.18), book_mat)
    export_current_as("Bookshelf", MOON)

def asset_Bed():
    reset_scene(); oak = OAK(); linen = LINEN()
    cube("Frame", (0,0,0.25), (1.0, 2.0, 0.10), oak)
    cube("Mattress", (0,0,0.36), (0.95, 1.95, 0.12), linen)
    cube("Pillow", (0,-0.85,0.46), (0.85, 0.30, 0.08), linen)
    cube("HeadBoard", (0,-1.0,0.7), (1.0, 0.05, 0.6), oak)
    cube("FootBoard", (0,1.0,0.5), (1.0, 0.05, 0.3), oak)
    for (x,y) in [(-0.45,-0.95),(0.45,-0.95),(-0.45,0.95),(0.45,0.95)]:
        cube(f"Post_{x}_{y}", (x,y,0.15), (0.08, 0.08, 0.30), oak)
    export_current_as("WoodenBed", MOON)

def asset_NightStand():
    reset_scene(); oak = OAK(); candle = CANDLE()
    cube("Body", (0,0,0.30), (0.40, 0.35, 0.55), oak)
    cube("Drawer", (0, 0.18, 0.30), (0.30, 0.02, 0.18), oak)
    sphere("Knob", 0.03, (0, 0.20, 0.30), oak)
    # Candle on top
    cyl("Candle", 0.04, 0.18, (0.10, 0, 0.70), candle, verts=8)
    cyl("CandleHolder", 0.06, 0.04, (0.10, 0, 0.58), oak)
    sphere("Flame", 0.02, (0.10, 0, 0.80), candle)
    export_current_as("NightStand", MOON)

def asset_Stool():
    reset_scene(); oak = OAK()
    cyl("Seat", 0.18, 0.04, (0,0,0.45), oak)
    for i in range(3):
        a = i * 2*math.pi/3
        cube(f"Leg_{i}", (math.cos(a)*0.13, math.sin(a)*0.13, 0.22), (0.03, 0.03, 0.45), oak, rot=(0,0,a))
    export_current_as("ThreeLeggedStool", MOON)

def asset_Hearth():
    reset_scene(); stone = make_material("H_Stone", (0.45, 0.42, 0.38), roughness=0.85)
    ember = make_material("H_Ember", (1.0, 0.4, 0.1), roughness=0.3, emission=(1.0,0.4,0.1), emission_strength=3.0)
    soot = make_material("H_Soot", (0.08, 0.06, 0.05), roughness=0.95)
    # Surround
    cube("Hearth", (0,0,0.5), (1.5, 0.7, 1.0), stone)
    # Opening (smaller dark inset)
    cube("Opening", (0,0.05,0.45), (1.1, 0.5, 0.7), soot)
    # Mantel
    cube("Mantel", (0, -0.35, 1.0), (1.7, 0.18, 0.10), stone)
    # Logs
    for i in range(3):
        cyl(f"Log_{i}", 0.07, 0.6, (-0.2 + i*0.2, -0.05, 0.35), oak := make_material(f"H_Log_{i}", (0.30,0.18,0.10),0.7), rot=(math.pi/2,0,0))
    # Flame
    sphere("Flame", 0.15, (0, -0.05, 0.45), ember)
    export_current_as("FireplaceHearth", MOON)

def asset_StorageChest():
    reset_scene(); dark = DARKWOOD(); iron = IRON()
    cube("Body", (0,0,0.30), (0.7, 0.4, 0.45), dark)
    cube("Lid", (0,0,0.56), (0.72, 0.42, 0.06), dark)
    # Iron banding
    for x in [-0.30, 0.30]:
        cube(f"Strap_{x}", (x, 0, 0.30), (0.04, 0.42, 0.50), iron)
    # Lock
    cube("Lock", (0, 0.21, 0.50), (0.08, 0.02, 0.10), iron)
    cyl("LockKeyhole", 0.012, 0.025, (0, 0.22, 0.50), iron, rot=(math.pi/2,0,0))
    export_current_as("StorageChest", MOON)

def asset_RugWoven():
    reset_scene(); fabric_a = make_material("Rug_RedA", (0.55,0.18,0.12), roughness=0.9)
    fabric_b = make_material("Rug_Gold", (0.85, 0.65, 0.30), roughness=0.9)
    cube("Rug", (0,0,0.005), (1.5, 1.0, 0.005), fabric_a)
    cube("Border", (0,0,0.006), (1.4, 0.9, 0.005), fabric_b)
    cube("BorderInner", (0,0,0.007), (1.3, 0.8, 0.005), fabric_a)
    # Center medallion
    cyl("Medallion", 0.30, 0.005, (0,0,0.01), fabric_b, verts=16)
    export_current_as("RugWoven", MOON)

def asset_WoodenLectern():
    reset_scene(); oak = OAK()
    cyl("Post", 0.05, 1.4, (0,0,0.7), oak)
    cyl("Base", 0.18, 0.04, (0,0,0.04), oak)
    cube("Top", (0,0,1.4), (0.40, 0.30, 0.05), oak, rot=(math.radians(15),0,0))
    cube("BookLedge", (0,-0.13,1.36), (0.40, 0.04, 0.03), oak)
    export_current_as("WoodenLectern", MOON)

# Build all 12
for fn in [asset_RoundTable, asset_LongDiningTable, asset_PeasantChair, asset_LongBench,
           asset_Bookshelf, asset_Bed, asset_NightStand, asset_Stool, asset_Hearth,
           asset_StorageChest, asset_RugWoven, asset_WoodenLectern]:
    fn()
print("[TARTARIA] Moon 1 furniture set done (12 assets).")
