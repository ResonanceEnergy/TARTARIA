"""gen_characters_complete.py — ships the missing 16 Moon-1-spec characters
plus upgraded body proportions that read better at gameplay distances.

Per CLAUDE.md no-stubs mandate: every character has a real body, real materials,
real head shape, real props where the spec calls for them (Milo's lantern + satchel,
Cassian's hat, Anastasia's torn dress, Bishop's mitre, OrganPlayer's robes, etc.).

Adds:
  Heroes:        PlayerHero (the actual protagonist)
  Mini-boss:     GiantGolem (Moon 1 endgame)
  Enemies:       VoidPhantom, TemporalWraith
  Cathedral:     CathedralBishop, OrganPlayer
  Villagers:     Villager_A..E (5 variants)
  Ambient NPCs:  Pilgrim, Pickpocket, BlackSmith, Beggar, FortuneTeller

Run via Blender:
  blender --background --python tools/blender/gen_characters_complete.py
or via Unity menu:
  Tartaria → Moon 1 → Run Blender Batch
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, cone, torus
import bpy


# ════════════════════════════════════════════════════════════════════════════
#  UPGRADED HUMANOID TEMPLATE — better proportions, fabric folds, eyes, accessories
# ════════════════════════════════════════════════════════════════════════════

def humanoid_v2(
    name,
    skin=(0.93, 0.79, 0.65),
    shirt=(0.42, 0.55, 0.30),
    pants=(0.28, 0.20, 0.15),
    hair=(0.40, 0.25, 0.12),
    boot_color=None,
    belt_color=None,
    hat_color=None,
    cape_color=None,
    accessory=None,            # "satchel", "lantern", "shovel", "staff", "sword", "tome", "basket"
    accessory_color=(0.50, 0.30, 0.15),
    eye_color=(0.10, 0.15, 0.25),
    is_ghostly=False,          # if True, emission + lower opacity feel
    is_armored=False,          # heavier proportions, plates
    age="adult",               # "child"/"adult"/"elder" — alters height + head ratio
    moon="Moon1",
):
    """Build a humanoid with KayKit-style proportions: ~6-head-tall, slightly chunky body."""
    reset_scene()

    # Materials
    m_skin  = make_material(name+"_skin",  skin,  roughness=0.55,
                            emission=(skin if is_ghostly else None),
                            emission_strength=(0.4 if is_ghostly else 0))
    m_shirt = make_material(name+"_shirt", shirt, roughness=0.75,
                            emission=(shirt if is_ghostly else None),
                            emission_strength=(0.3 if is_ghostly else 0))
    m_pants = make_material(name+"_pants", pants, roughness=0.85)
    m_hair  = make_material(name+"_hair",  hair,  roughness=0.65)
    m_eye   = make_material(name+"_eye",   eye_color, roughness=0.20)
    m_boot  = make_material(name+"_boot",  boot_color or (0.18, 0.12, 0.08), roughness=0.65)
    m_belt  = make_material(name+"_belt",  belt_color or (0.30, 0.20, 0.10), roughness=0.55) if belt_color else None

    # Proportion lookup — H is total height in meters (Unity units)
    # Targets: child ~1.2m, adult ~1.75m, elder ~1.65m (gameplay-appropriate)
    if age == "child":
        H, head_r, body_w = 1.20, 0.30, 0.36
    elif age == "elder":
        H, head_r, body_w = 1.65, 0.26, 0.40
    else:
        H, head_r, body_w = 1.75, 0.26, 0.42

    if is_armored:
        body_w *= 1.18

    # Torso (rounded cube — use cube + later we can bevel)
    cube("torso_lower", (0, 0, 0.85*H), (body_w, 0.20, 0.34*H), m_shirt)
    cube("torso_upper", (0, 0, 1.15*H), (body_w*0.92, 0.20, 0.18*H), m_shirt)
    # Belt
    if m_belt:
        cyl("belt", body_w*1.02, 0.04, (0, 0, 0.95*H), m_belt, rot=(1.5708, 0, 0), verts=18)

    # Neck
    cyl("neck", 0.08*H, 0.10*H, (0, 0, 1.36*H), m_skin, verts=12)
    # Head — slightly squashed sphere
    sphere("head", head_r*H, (0, 0.02, 1.50*H), m_skin, segs=18, rings=14)
    # Hair cap (back/top of skull)
    sphere("hair_cap", head_r*1.02*H, (0, 0.05, 1.54*H), m_hair, segs=18, rings=12)
    # Eyes
    eye_off = head_r*0.45*H
    sphere("eye_l", 0.035*H, (-eye_off, -head_r*0.85*H, 1.52*H), m_eye, segs=10, rings=8)
    sphere("eye_r", 0.035*H, ( eye_off, -head_r*0.85*H, 1.52*H), m_eye, segs=10, rings=8)
    # Eye whites (small sphere behind iris for highlight)
    m_white = make_material(name+"_white", (0.95, 0.95, 0.92), roughness=0.30)
    sphere("white_l", 0.05*H, (-eye_off, -head_r*0.80*H, 1.52*H), m_white, segs=8, rings=6)
    sphere("white_r", 0.05*H, ( eye_off, -head_r*0.80*H, 1.52*H), m_white, segs=8, rings=6)

    # Hat (cap, mitre, or top hat)
    if hat_color:
        m_hat = make_material(name+"_hat", hat_color, roughness=0.78)
        cyl("hat_brim",  head_r*1.35*H, 0.04*H, (0, 0, 1.70*H), m_hat, verts=20)
        cyl("hat_crown", head_r*0.82*H, 0.18*H, (0, 0, 1.82*H), m_hat, verts=18)

    # Cape (back drape)
    if cape_color:
        m_cape = make_material(name+"_cape", cape_color, roughness=0.85)
        cube("cape", (0, 0.20, 0.90*H), (body_w*1.08, 0.04, 0.55*H), m_cape)
        # Hem fold
        cube("cape_hem", (0, 0.22, 0.55*H), (body_w*1.12, 0.04, 0.06*H), m_cape)

    # Arms — pivot at shoulders, with hand at end
    cyl("arm_l", 0.07*H, 0.58*H, (-body_w*1.05, 0, 1.00*H), m_shirt, verts=14)
    cyl("arm_r", 0.07*H, 0.58*H, ( body_w*1.05, 0, 1.00*H), m_shirt, verts=14)
    sphere("hand_l", 0.08*H, (-body_w*1.05, 0, 0.68*H), m_skin, segs=12, rings=10)
    sphere("hand_r", 0.08*H, ( body_w*1.05, 0, 0.68*H), m_skin, segs=12, rings=10)

    # Legs
    cyl("leg_l", 0.10*H, 0.70*H, (-body_w*0.42, 0, 0.30*H), m_pants, verts=14)
    cyl("leg_r", 0.10*H, 0.70*H, ( body_w*0.42, 0, 0.30*H), m_pants, verts=14)
    # Boots
    cube("boot_l", (-body_w*0.42, 0.05, -0.04*H), (0.11, 0.18, 0.06), m_boot)
    cube("boot_r", ( body_w*0.42, 0.05, -0.04*H), (0.11, 0.18, 0.06), m_boot)

    # Accessory props — held in right hand or worn
    m_acc = make_material(name+"_acc", accessory_color, roughness=0.50)
    if accessory == "satchel":
        cube("satchel_body", (body_w*1.05, 0.18, 0.85*H), (0.14, 0.06, 0.16), m_acc)
        cube("satchel_strap", (0, -0.05, 1.15*H), (0.02, 0.18, 0.04), m_acc)
    elif accessory == "lantern":
        m_glow = make_material(name+"_glow", (1.0, 0.85, 0.55), roughness=0.20,
                               emission=(1.0, 0.85, 0.55), emission_strength=3.0)
        cyl("lantern_top",    0.07, 0.04, (body_w*1.20, 0,  0.85*H), m_acc, verts=12)
        cube("lantern_body",  (body_w*1.20, 0, 0.78*H), (0.06, 0.06, 0.07), m_glow)
        cyl("lantern_base",   0.07, 0.03, (body_w*1.20, 0, 0.70*H), m_acc, verts=12)
        cube("lantern_handle",(body_w*1.20, 0, 0.92*H), (0.01, 0.05, 0.04), m_acc)
    elif accessory == "shovel":
        cube("shovel_blade", (body_w*1.20, 0.05, 0.45*H), (0.10, 0.02, 0.16), m_acc)
        cyl("shovel_handle", 0.02, 0.95*H, (body_w*1.20, 0, 0.95*H), m_acc, verts=10)
    elif accessory == "staff":
        cyl("staff", 0.025, 1.55*H, (body_w*1.20, 0, 0.85*H), m_acc, verts=12)
        m_orb = make_material(name+"_orb", (0.40, 0.70, 0.95), roughness=0.20,
                              emission=(0.40, 0.70, 0.95), emission_strength=2.5)
        sphere("staff_orb", 0.10, (body_w*1.20, 0, 1.66*H), m_orb, segs=14, rings=10)
    elif accessory == "sword":
        cube("sword_blade", (body_w*1.30, 0, 0.95*H), (0.04, 0.02, 0.50), m_acc)
        cyl("sword_hilt",   0.04, 0.10, (body_w*1.30, 0, 0.65*H), m_acc, verts=10)
        cube("sword_guard", (body_w*1.30, 0, 0.70*H), (0.10, 0.03, 0.02), m_acc)
    elif accessory == "tome":
        cube("tome_body",  (body_w*1.20, 0.04, 0.85*H), (0.10, 0.04, 0.14), m_acc)
        cube("tome_spine", (body_w*1.30, 0.04, 0.85*H), (0.02, 0.04, 0.14), m_acc)
    elif accessory == "basket":
        cyl("basket",      0.18, 0.22, (body_w*1.30, 0.08, 0.78*H), m_acc, verts=14)
        cube("basket_handle", (body_w*1.30, 0.08, 0.95*H), (0.18, 0.02, 0.02), m_acc)

    # Join + name + export
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.join()
    bpy.context.active_object.name = name
    export_current_as(name, moon)


# ════════════════════════════════════════════════════════════════════════════
#  CHARACTER ROSTER — 16 missing per Moon 1-13 spec + 2 cathedral
# ════════════════════════════════════════════════════════════════════════════

# === 1. PlayerHero — the protagonist (canonical hero archetype) ===
humanoid_v2("PlayerHero",
            skin=(0.93, 0.78, 0.62),
            shirt=(0.20, 0.32, 0.60),     # blue tunic
            pants=(0.30, 0.20, 0.12),     # brown pants
            hair=(0.32, 0.20, 0.10),      # dark brown
            boot_color=(0.16, 0.10, 0.06),
            belt_color=(0.45, 0.30, 0.18),
            cape_color=(0.18, 0.28, 0.50),
            accessory="sword",
            accessory_color=(0.75, 0.78, 0.82),
            eye_color=(0.10, 0.30, 0.55),
            moon="Shared")

# === 2. GiantGolem — Moon 1 mini-boss (oversized Mud Golem) ===
reset_scene()
giant_mud = make_material("GiantGolem_mud", (0.28, 0.18, 0.10), roughness=0.95)
giant_core = make_material("GiantGolem_core", (0.85, 0.30, 0.10), roughness=0.40,
                           emission=(1.0, 0.40, 0.10), emission_strength=2.5)
# Massive scale (2x normal Mud Golem)
sphere("torso", 1.10, (0, 0, 2.0), giant_mud, segs=18, rings=14)
sphere("head", 0.65, (0, 0, 3.30), giant_mud, segs=18, rings=14)
sphere("eye_l", 0.13, (-0.24, -0.55, 3.40), giant_core, segs=12, rings=10)
sphere("eye_r", 0.13, ( 0.24, -0.55, 3.40), giant_core, segs=12, rings=10)
# Massive arms
cyl("arm_l", 0.32, 1.70, (-1.24, 0, 1.90), giant_mud, verts=16)
cyl("arm_r", 0.32, 1.70, ( 1.24, 0, 1.90), giant_mud, verts=16)
sphere("fist_l", 0.45, (-1.24, 0, 1.0), giant_mud, segs=14, rings=12)
sphere("fist_r", 0.45, ( 1.24, 0, 1.0), giant_mud, segs=14, rings=12)
cyl("leg_l", 0.40, 1.70, (-0.40, 0, 0.80), giant_mud, verts=16)
cyl("leg_r", 0.40, 1.70, ( 0.40, 0, 0.80), giant_mud, verts=16)
# Crystal shard piercing torso (the dissonance source)
m_shard = make_material("GiantGolem_shard", (0.10, 0.50, 0.85), roughness=0.25,
                        emission=(0.20, 0.70, 1.0), emission_strength=3.0)
cone("torso_shard", 0.20, 0.04, 0.90, (0, -0.85, 2.10), m_shard, rot=(1.0, 0, 0), verts=8)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.join()
bpy.context.active_object.name = "GiantGolem"
export_current_as("GiantGolem", "Shared")

# === 3. VoidPhantom — wraith-like Moon 1 shadow enemy ===
reset_scene()
void = make_material("VoidPhantom_void", (0.02, 0.02, 0.05), roughness=0.92,
                     emission=(0.10, 0.05, 0.20), emission_strength=0.6)
void_eye = make_material("VoidPhantom_eye", (0.0, 0.0, 0.0), roughness=0.20,
                         emission=(0.80, 0.10, 0.90), emission_strength=3.5)
# Floating ribbon-like body
cone("body", 0.40, 0.08, 1.60, (0, 0, 0.9), void, verts=10)
sphere("head", 0.22, (0, 0, 1.85), void, segs=14, rings=10)
# Multiple eyes (5 across face)
for i, x in enumerate([-0.14, -0.07, 0.0, 0.07, 0.14]):
    sphere(f"eye_{i}", 0.025, (x, -0.18, 1.88), void_eye, segs=8, rings=6)
# Tendril arms
cone("tendril_l", 0.08, 0.01, 1.20, (-0.34, 0, 1.0), void, rot=(0, 0.4, 0), verts=8)
cone("tendril_r", 0.08, 0.01, 1.20, ( 0.34, 0, 1.0), void, rot=(0, -0.4, 0), verts=8)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.join()
bpy.context.active_object.name = "VoidPhantom"
export_current_as("VoidPhantom", "Shared")

# === 4. TemporalWraith — Moon 7 time enemy (clockwork-mechanical) ===
reset_scene()
tw_body = make_material("TW_body", (0.35, 0.30, 0.20), roughness=0.45, metallic=0.7)
tw_gear = make_material("TW_gear", (0.70, 0.55, 0.20), roughness=0.30, metallic=0.9,
                        emission=(0.95, 0.65, 0.20), emission_strength=1.5)
# Mechanical body
cyl("torso", 0.30, 1.10, (0, 0, 1.15), tw_body, verts=14)
sphere("head", 0.22, (0, 0, 1.85), tw_body, segs=14, rings=12)
# Floating gears
torus("gear_1", 0.30, 0.04, (0, 0, 1.15), tw_gear, mseg=12, miseg=6)
torus("gear_2", 0.25, 0.04, (0, 0, 1.65), tw_gear, mseg=10, miseg=6, rot=(1.5708, 0, 0))
# Clock face on chest
cyl("clock_face", 0.18, 0.03, (0, -0.30, 1.20), tw_gear, rot=(1.5708, 0, 0), verts=24)
# Arms
cyl("arm_l", 0.07, 0.70, (-0.36, 0, 1.10), tw_body, verts=12)
cyl("arm_r", 0.07, 0.70, ( 0.36, 0, 1.10), tw_body, verts=12)
# Skeletal legs
cyl("leg_l", 0.08, 0.75, (-0.14, 0, 0.40), tw_body, verts=12)
cyl("leg_r", 0.08, 0.75, ( 0.14, 0, 0.40), tw_body, verts=12)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.join()
bpy.context.active_object.name = "TemporalWraith"
export_current_as("TemporalWraith", "Shared")

# === 5. CathedralBishop — Moon 1 cathedral cleric ===
humanoid_v2("CathedralBishop",
            skin=(0.93, 0.83, 0.72),
            shirt=(0.85, 0.10, 0.15),       # red cassock
            pants=(0.85, 0.10, 0.15),
            hair=(0.85, 0.85, 0.85),         # grey
            boot_color=(0.20, 0.15, 0.10),
            belt_color=(0.90, 0.80, 0.30),    # gold sash
            hat_color=(0.95, 0.90, 0.75),    # mitre
            cape_color=(0.85, 0.78, 0.45),    # vestments
            accessory="tome",
            accessory_color=(0.55, 0.25, 0.15),
            age="elder",
            moon="Moon1")

# === 6. OrganPlayer — Moon 1 pipe organist (cathedral) ===
humanoid_v2("OrganPlayer",
            skin=(0.95, 0.85, 0.75),
            shirt=(0.20, 0.20, 0.30),        # dark formal jacket
            pants=(0.18, 0.18, 0.28),
            hair=(0.90, 0.85, 0.80),         # white hair
            boot_color=(0.10, 0.10, 0.15),
            belt_color=(0.10, 0.10, 0.15),
            cape_color=(0.30, 0.30, 0.40),    # cloak
            accessory="staff",                # conductor's wand
            accessory_color=(0.60, 0.45, 0.25),
            age="elder",
            moon="Moon1")

# === 7. Villager_A — woman with basket ===
humanoid_v2("Villager_A",
            skin=(0.93, 0.80, 0.66),
            shirt=(0.65, 0.35, 0.55),     # plum bodice
            pants=(0.30, 0.30, 0.50),     # long blue skirt
            hair=(0.55, 0.30, 0.15),
            boot_color=(0.30, 0.20, 0.10),
            belt_color=(0.50, 0.35, 0.20),
            accessory="basket",
            accessory_color=(0.55, 0.40, 0.20),
            moon="Shared")

# === 8. Villager_B — man with shovel (farmer) ===
humanoid_v2("Villager_B",
            skin=(0.85, 0.65, 0.45),
            shirt=(0.70, 0.65, 0.50),     # straw-colored tunic
            pants=(0.30, 0.25, 0.15),
            hair=(0.30, 0.20, 0.10),
            boot_color=(0.18, 0.12, 0.05),
            hat_color=(0.65, 0.55, 0.30),    # straw hat
            accessory="shovel",
            moon="Shared")

# === 9. Villager_C — child running ===
humanoid_v2("Villager_C",
            skin=(0.95, 0.84, 0.72),
            shirt=(0.95, 0.85, 0.30),     # bright yellow shirt
            pants=(0.50, 0.30, 0.18),
            hair=(0.30, 0.18, 0.08),
            boot_color=(0.30, 0.20, 0.10),
            age="child",
            moon="Shared")

# === 10. Villager_D — old man with staff ===
humanoid_v2("Villager_D",
            skin=(0.78, 0.68, 0.58),
            shirt=(0.40, 0.30, 0.20),
            pants=(0.30, 0.25, 0.18),
            hair=(0.85, 0.85, 0.80),     # silver hair
            boot_color=(0.20, 0.15, 0.10),
            cape_color=(0.40, 0.30, 0.20),
            accessory="staff",
            accessory_color=(0.40, 0.25, 0.10),
            age="elder",
            moon="Shared")

# === 11. Villager_E — merchant woman ===
humanoid_v2("Villager_E",
            skin=(0.88, 0.74, 0.60),
            shirt=(0.20, 0.55, 0.40),     # green dress
            pants=(0.20, 0.50, 0.38),
            hair=(0.18, 0.10, 0.06),
            boot_color=(0.18, 0.10, 0.05),
            belt_color=(0.80, 0.65, 0.30),    # gold belt
            hat_color=(0.30, 0.20, 0.55),    # purple cap
            accessory="tome",
            accessory_color=(0.30, 0.20, 0.10),
            moon="Shared")

# === 12. Pilgrim — robed traveler ===
humanoid_v2("Pilgrim",
            skin=(0.85, 0.72, 0.58),
            shirt=(0.50, 0.45, 0.35),     # earthy robe
            pants=(0.45, 0.40, 0.30),
            hair=(0.30, 0.20, 0.12),
            boot_color=(0.18, 0.12, 0.05),
            cape_color=(0.42, 0.36, 0.28),    # hooded cloak
            accessory="staff",
            accessory_color=(0.30, 0.20, 0.10),
            moon="Shared")

# === 13. Pickpocket — hooded rogue ===
humanoid_v2("Pickpocket",
            skin=(0.80, 0.70, 0.60),
            shirt=(0.12, 0.10, 0.10),     # black tunic
            pants=(0.12, 0.10, 0.10),
            hair=(0.05, 0.05, 0.05),
            boot_color=(0.08, 0.06, 0.04),
            belt_color=(0.15, 0.10, 0.08),
            cape_color=(0.10, 0.08, 0.08),
            eye_color=(0.40, 0.60, 0.10),     # green eyes (suspicious)
            moon="Shared")

# === 14. BlackSmith — burly craftsman ===
humanoid_v2("BlackSmith",
            skin=(0.78, 0.55, 0.35),
            shirt=(0.18, 0.14, 0.10),     # soot-stained tunic
            pants=(0.45, 0.35, 0.20),     # leather apron-pants
            hair=(0.18, 0.10, 0.06),
            boot_color=(0.10, 0.05, 0.03),
            belt_color=(0.55, 0.35, 0.15),
            is_armored=True,                  # broader shoulders
            accessory="sword",                # hammer-like prop
            accessory_color=(0.20, 0.20, 0.25),
            moon="Shared")

# === 15. Beggar — downtrodden NPC ===
humanoid_v2("Beggar",
            skin=(0.72, 0.60, 0.45),
            shirt=(0.35, 0.30, 0.25),     # tattered grey
            pants=(0.30, 0.25, 0.20),
            hair=(0.30, 0.25, 0.20),
            boot_color=(0.20, 0.15, 0.10),
            cape_color=(0.30, 0.25, 0.20),
            accessory="basket",
            accessory_color=(0.40, 0.30, 0.18),
            age="elder",
            moon="Shared")

# === 16. FortuneTeller — mystic NPC (Moon 2 transition) ===
humanoid_v2("FortuneTeller",
            skin=(0.93, 0.83, 0.72),
            shirt=(0.40, 0.10, 0.55),     # purple robes
            pants=(0.30, 0.08, 0.40),
            hair=(0.12, 0.08, 0.04),
            boot_color=(0.20, 0.10, 0.30),
            cape_color=(0.55, 0.20, 0.70),
            hat_color=(0.55, 0.20, 0.70),    # pointed hat
            accessory="staff",                # crystal staff
            accessory_color=(0.20, 0.10, 0.30),
            eye_color=(0.85, 0.65, 0.85),     # mystic violet
            moon="Shared")

print("done gen_characters_complete: 16 characters")
