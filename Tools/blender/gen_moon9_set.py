"""Moon 9 — Star Fort Bastion: 5 assets."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon9"

def asset_StarFortBastion():
    reset_scene()
    stone = make_material("Sf_Stone", (0.42,0.40,0.38), roughness=0.85)
    iron = make_material("Sf_Iron", (0.25,0.22,0.20), roughness=0.6, metallic=0.5)
    # 5-point star base (5 cubes around center)
    cyl("Center", 1.0, 1.0, (0,0,0.5), stone, verts=20)
    for i in range(5):
        a = i * 2*math.pi/5
        x,y = math.cos(a)*1.5, math.sin(a)*1.5
        cube(f"Pt_{i}", (x, y, 0.5), (0.7, 0.7, 1.0), stone, rot=(0,0,a))
    cyl("Tower", 0.8, 2.0, (0,0,2.0), stone, verts=20)
    cyl("Crown", 1.0, 0.4, (0,0,3.2), stone, verts=24)
    # Crenellations
    for i in range(8):
        a = i * math.pi/4
        cube(f"Cren_{i}", (math.cos(a)*0.9, math.sin(a)*0.9, 3.5), (0.15, 0.15, 0.30), stone)
    export_current_as("StarFortBastion", MOON)

def asset_RampartCannon():
    reset_scene()
    iron = make_material("Rc_Iron", (0.18,0.18,0.20), roughness=0.4, metallic=0.6)
    wood = make_material("Rc_Wood", (0.32,0.20,0.12), roughness=0.7)
    cyl("Barrel", 0.10, 1.2, (0,0,0.5), iron, rot=(0,math.pi/2,0))
    cone("Muzzle", 0.12, 0.10, 0.12, (0.65, 0, 0.5), iron, verts=12, rot=(0,math.pi/2,0))
    # Carriage
    cube("Carriage", (-0.20, 0, 0.30), (0.5, 0.4, 0.20), wood)
    # 2 wheels
    for sy in [-0.25, 0.25]:
        cyl(f"Wh_{sy}", 0.20, 0.05, (-0.20, sy, 0.15), iron, rot=(math.pi/2,0,0), verts=16)
    export_current_as("RampartCannon", MOON)

def asset_BastionGate():
    reset_scene()
    iron = make_material("Bg_Iron", (0.25,0.22,0.20), roughness=0.5, metallic=0.5)
    wood = make_material("Bg_Wood", (0.32,0.20,0.12), roughness=0.7)
    stone = make_material("Bg_Stone", (0.42,0.40,0.38), roughness=0.85)
    # Wall around
    cube("PortalL", (-1.0, 0, 1.5), (0.4, 0.5, 3.0), stone)
    cube("PortalR", ( 1.0, 0, 1.5), (0.4, 0.5, 3.0), stone)
    cube("Lintel", (0, 0, 2.8), (1.6, 0.5, 0.4), stone)
    # Wood door
    cube("DoorL", (-0.35, 0.20, 1.3), (0.35, 0.05, 2.5), wood)
    cube("DoorR", ( 0.35, 0.20, 1.3), (0.35, 0.05, 2.5), wood)
    # Iron bands
    for h in [0.5, 1.5, 2.5]:
        cube(f"BandL_{h}", (-0.35, 0.25, h), (0.40, 0.02, 0.10), iron)
        cube(f"BandR_{h}", (0.35, 0.25, h), (0.40, 0.02, 0.10), iron)
    export_current_as("BastionGate", MOON)

def asset_SoldierStatue():
    reset_scene()
    bronze = make_material("Ss_Bronze", (0.50,0.35,0.20), roughness=0.4, metallic=0.7)
    stone = make_material("Ss_Stone", (0.55,0.52,0.48), roughness=0.85)
    cube("Plinth", (0,0,0.35), (0.5,0.5,0.7), stone)
    cube("Body", (0,0,1.40), (0.30, 0.20, 0.70), bronze)
    sphere("Head", 0.16, (0,0,2.0), bronze)
    cube("ArmL", (-0.20, 0.05, 1.40), (0.08, 0.08, 0.80), bronze)
    cube("ArmR", ( 0.20, 0.05, 1.40), (0.08, 0.08, 0.80), bronze)
    cube("LegL", (-0.10, 0, 0.95), (0.10, 0.12, 0.50), bronze)
    cube("LegR", ( 0.10, 0, 0.95), (0.10, 0.12, 0.50), bronze)
    # Sword down
    cube("Sword", (0.30, 0, 1.0), (0.04, 0.04, 1.0), bronze)
    export_current_as("SoldierStatue", MOON)

def asset_BellTowerNarrow():
    reset_scene()
    stone = make_material("Bt_Stone", (0.45,0.42,0.38), roughness=0.85)
    brass = make_material("Bt_Brass", (0.75,0.62,0.30), roughness=0.4, metallic=0.85)
    cube("Base", (0,0,0.5), (0.8,0.8,1.0), stone)
    cube("Mid", (0,0,2.0), (0.6,0.6,2.0), stone)
    cube("Top", (0,0,3.5), (0.7,0.7,1.0), stone)
    # Hanging bell
    cyl("Bell", 0.28, 0.36, (0,0,3.5), brass, verts=20)
    sphere("BellTop", 0.10, (0,0,3.75), brass)
    # Roof
    cone("Roof", 0.5, 0.0, 0.6, (0,0,4.3), stone, verts=4)
    export_current_as("BellTowerNarrow", MOON)

for fn in [asset_StarFortBastion, asset_RampartCannon, asset_BastionGate,
           asset_SoldierStatue, asset_BellTowerNarrow]:
    fn()
print("[TARTARIA] Moon 9 set done (5 assets).")
