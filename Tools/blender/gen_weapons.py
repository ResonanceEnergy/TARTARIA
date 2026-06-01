"""15 weapons — dagger, short sword, long sword, war hammer, mace, axe, war pick,
bow, crossbow, quiver, arrow bundle, javelin, staff, scepter, round shield.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, cone, torus
import bpy, math

def finalize(name, moon="Shared"):
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.join()
    bpy.context.active_object.name = name
    export_current_as(name, moon)

steel = lambda n: make_material(n, (0.78, 0.78, 0.82), roughness=0.35, metallic=0.85)
iron = lambda n: make_material(n, (0.32, 0.30, 0.28), roughness=0.55, metallic=0.6)
wood = lambda n: make_material(n, (0.42, 0.28, 0.16), roughness=0.85)
leather = lambda n: make_material(n, (0.40, 0.22, 0.10), roughness=0.85)
gold = lambda n: make_material(n, (0.92, 0.75, 0.25), roughness=0.30, metallic=0.85)

# 1. Dagger
reset_scene()
cube("blade", (0, 0, 0.15), (0.02, 0.015, 0.15), steel("Dag_b"))
cone("tip", 0.02, 0.005, 0.04, (0, 0, 0.32), steel("Dag_tip"), verts=8)
cube("guard", (0, 0, 0.0), (0.05, 0.02, 0.012), iron("Dag_g"))
cyl("grip", 0.015, 0.08, (0, 0, -0.04), leather("Dag_grip"), verts=10)
sphere("pommel", 0.022, (0, 0, -0.09), gold("Dag_pom"), segs=10, rings=8)
finalize("Dagger")

# 2. Short Sword
reset_scene()
cube("blade", (0, 0, 0.30), (0.025, 0.018, 0.30), steel("SS_b"))
cone("tip", 0.025, 0.005, 0.06, (0, 0, 0.63), steel("SS_tip"), verts=8)
cube("guard", (0, 0, 0.0), (0.07, 0.02, 0.012), iron("SS_g"))
cyl("grip", 0.018, 0.10, (0, 0, -0.06), leather("SS_grip"), verts=10)
sphere("pommel", 0.025, (0, 0, -0.12), iron("SS_pom"), segs=10, rings=8)
finalize("ShortSword")

# 3. Long Sword (claymore-ish)
reset_scene()
cube("blade", (0, 0, 0.50), (0.03, 0.02, 0.50), steel("LS_b"))
cone("tip", 0.03, 0.005, 0.08, (0, 0, 1.04), steel("LS_tip"), verts=8)
cube("guard", (0, 0, 0.0), (0.10, 0.022, 0.012), gold("LS_g"))
cyl("grip", 0.020, 0.18, (0, 0, -0.10), leather("LS_grip"), verts=10)
sphere("pommel", 0.030, (0, 0, -0.21), gold("LS_pom"), segs=10, rings=8)
finalize("LongSword")

# 4. War Hammer
reset_scene()
cyl("handle", 0.025, 0.80, (0, 0, 0.40), wood("WH_h"), verts=12)
cube("head", (0, 0, 0.82), (0.10, 0.07, 0.12), iron("WH_head"))
# Spike on back
cone("spike", 0.04, 0.005, 0.10, (-0.10, 0, 0.82), iron("WH_spike"), rot=(0, -1.5708, 0), verts=8)
# Grip wrap
torus("wrap_1", 0.026, 0.005, (0, 0, 0.20), leather("WH_w1"), mseg=12, miseg=3)
torus("wrap_2", 0.026, 0.005, (0, 0, 0.10), leather("WH_w2"), mseg=12, miseg=3)
finalize("WarHammer")

# 5. Mace
reset_scene()
cyl("handle", 0.022, 0.65, (0, 0, 0.33), wood("Ma_h"), verts=12)
sphere("head", 0.10, (0, 0, 0.75), iron("Ma_head"), segs=14, rings=10)
# Flanges (6)
for i in range(6):
    a = i*(math.pi/3)
    cube(f"flange_{i}", (math.cos(a)*0.10, math.sin(a)*0.10, 0.75), (0.04, 0.015, 0.10), iron(f"Ma_f{i}"), rot=(0, 0, a))
# Pommel
sphere("pommel", 0.025, (0, 0, 0.0), iron("Ma_pom"), segs=10, rings=8)
finalize("Mace")

# 6. Axe (battle)
reset_scene()
cyl("handle", 0.025, 0.85, (0, 0, 0.43), wood("Ax_h"), verts=12)
# Blade head
cube("blade", (0.10, 0, 0.85), (0.06, 0.02, 0.18), steel("Ax_blade"))
cone("edge", 0.18, 0.02, 0.10, (0.18, 0, 0.85), steel("Ax_edge"), rot=(1.5708, 0, 0), verts=6)
# Back spike
cone("back", 0.04, 0.005, 0.08, (-0.10, 0, 0.85), iron("Ax_back"), rot=(0, -1.5708, 0), verts=8)
finalize("BattleAxe")

# 7. War Pick
reset_scene()
cyl("handle", 0.025, 0.85, (0, 0, 0.43), wood("WP_h"), verts=12)
cube("base", (0, 0, 0.85), (0.04, 0.04, 0.08), iron("WP_base"))
# Curved pick (single long spike)
cone("pick", 0.04, 0.005, 0.30, (0.20, 0, 0.92), iron("WP_pick"), rot=(0, -1.3, 0), verts=8)
# Counter-spike
cone("counter", 0.03, 0.005, 0.08, (-0.10, 0, 0.85), iron("WP_counter"), rot=(0, -1.5708, 0), verts=6)
finalize("WarPick")

# 8. Bow (longbow)
reset_scene()
# Curved bow shaft — 5 segments at angles
bow = wood("Bow_w")
zs = [-0.50, -0.30, -0.10, 0.10, 0.30, 0.50]
for i in range(5):
    z_mid = (zs[i] + zs[i+1])/2
    cyl(f"seg_{i}", 0.012, 0.20, (abs(z_mid)*0.5, 0, z_mid + 0.50), bow, rot=(0, 0.4 if z_mid > 0 else -0.4, 0), verts=8)
# Bowstring
cube("string", (0, 0, 0.50), (0.003, 0.003, 1.0), make_material("Bow_str", (0.92, 0.88, 0.75), roughness=0.50))
# Grip wrap
torus("grip", 0.014, 0.004, (0, 0, 0.50), leather("Bow_grip"), mseg=12, miseg=3)
finalize("Bow")

# 9. Crossbow
reset_scene()
# Stock (horizontal body)
cube("stock", (0, 0, 0.10), (0.40, 0.04, 0.04), wood("CB_st"))
# Bow piece (perpendicular)
cube("bow_arm", (0.15, 0, 0.10), (0.025, 0.40, 0.025), iron("CB_bow"))
# Trigger
cube("trigger", (-0.20, 0, 0.07), (0.04, 0.02, 0.03), iron("CB_trig"))
# Grip
cyl("grip", 0.018, 0.10, (-0.15, 0, 0.04), leather("CB_grip"), verts=10)
# Bolt resting in groove
cube("bolt", (0.20, 0, 0.13), (0.20, 0.005, 0.005), wood("CB_bolt"))
cone("bolt_tip", 0.008, 0.001, 0.02, (0.42, 0, 0.13), iron("CB_tip"), rot=(0, 1.5708, 0), verts=6)
# String taut
cube("string", (-0.05, 0, 0.10), (0.003, 0.40, 0.003), make_material("CB_str", (0.92, 0.88, 0.75), roughness=0.50))
finalize("Crossbow")

# 10. Quiver
reset_scene()
cyl("body", 0.08, 0.40, (0, 0, 0.20), leather("Q_b"), verts=14)
# Strap
cyl("strap", 0.008, 0.50, (0, 0.10, 0.30), leather("Q_strap"), rot=(0.5, 0, 0), verts=8)
# Arrows poking out top (4)
for i, (x, y) in enumerate([(-0.02, -0.02), (0.02, -0.02), (-0.02, 0.02), (0.02, 0.02)]):
    cyl(f"arrow_{i}", 0.004, 0.18, (x, y, 0.45), wood(f"Q_a{i}"), verts=6)
    cube(f"fletch_{i}", (x, y, 0.52), (0.012, 0.003, 0.018), make_material(f"Q_f{i}", (0.95, 0.20, 0.20), roughness=0.85))
finalize("Quiver")

# 11. Arrow Bundle (10 arrows tied)
reset_scene()
for i in range(7):
    a = i*(math.pi*2/7)
    cyl(f"arrow_{i}", 0.005, 0.50, (math.cos(a)*0.022, math.sin(a)*0.022, 0.25), wood(f"AB_a{i}"), verts=6)
    cone(f"tip_{i}", 0.008, 0.001, 0.025, (math.cos(a)*0.022, math.sin(a)*0.022, 0.51), steel(f"AB_t{i}"), verts=6)
    cube(f"fletch_{i}", (math.cos(a)*0.022, math.sin(a)*0.022, 0.04), (0.015, 0.003, 0.025), make_material(f"AB_f{i}", (0.92, 0.85, 0.30), roughness=0.85))
# Binding cord
torus("bind", 0.032, 0.003, (0, 0, 0.25), leather("AB_bind"), mseg=16, miseg=3)
finalize("ArrowBundle")

# 12. Javelin
reset_scene()
cyl("shaft", 0.012, 1.20, (0, 0, 0.60), wood("Jv_shaft"), verts=10)
cone("head", 0.025, 0.003, 0.18, (0, 0, 1.30), steel("Jv_head"), verts=8)
# Grip wrap
torus("grip", 0.013, 0.004, (0, 0, 0.60), leather("Jv_grip"), mseg=12, miseg=3)
finalize("Javelin")

# 13. Quarter Staff
reset_scene()
cyl("body", 0.022, 1.60, (0, 0, 0.80), wood("Stf_b"), verts=12)
# End caps
cyl("cap_t", 0.026, 0.04, (0, 0, 1.60), iron("Stf_ct"), verts=12)
cyl("cap_b", 0.026, 0.04, (0, 0, 0.02), iron("Stf_cb"), verts=12)
# Grip wrap mid
torus("grip", 0.024, 0.005, (0, 0, 0.80), leather("Stf_grip"), mseg=12, miseg=3)
finalize("QuarterStaff")

# 14. Scepter (royal)
reset_scene()
cyl("shaft", 0.018, 0.60, (0, 0, 0.30), gold("Sc_shaft"), verts=12)
# Crown of jewels at top
sphere("orb", 0.06, (0, 0, 0.65), make_material("Sc_orb", (0.30, 0.20, 0.55), roughness=0.20, metallic=0.3, emission=(0.40, 0.30, 0.85), emission_strength=1.0), segs=14, rings=10)
# Cross / sigil
cube("cross_v", (0, 0, 0.73), (0.008, 0.008, 0.04), gold("Sc_cv"))
cube("cross_h", (0, 0, 0.73), (0.025, 0.008, 0.008), gold("Sc_ch"))
# Decorative rings
for z in (0.50, 0.40, 0.30, 0.20):
    torus(f"ring_{z}", 0.019, 0.003, (0, 0, z), gold(f"Sc_r{z}"), mseg=12, miseg=3)
# Base sphere
sphere("base", 0.025, (0, 0, 0.0), gold("Sc_base"), segs=10, rings=8)
finalize("Scepter")

# 15. Round Shield (oak boss)
reset_scene()
plank = wood("RS_plank")
boss = iron("RS_boss")
rim = iron("RS_rim")
cyl("face", 0.40, 0.04, (0, 0, 0.02), plank, verts=24)
# Boss
sphere("boss", 0.08, (0, 0, 0.06), boss, segs=14, rings=10)
# Rim
torus("rim", 0.41, 0.015, (0, 0, 0.02), rim, mseg=24, miseg=4)
# Spokes painted (8 stripes)
for i in range(8):
    a = i*(math.pi/4)
    cube(f"stripe_{i}", (math.cos(a)*0.20, math.sin(a)*0.20, 0.045), (0.20, 0.025, 0.001), make_material(f"RS_s{i}", (0.55, 0.20, 0.20) if i%2 else (0.30, 0.50, 0.20), roughness=0.85), rot=(0, 0, a))
finalize("RoundShield")

print("done gen_weapons: 15 weapons")
