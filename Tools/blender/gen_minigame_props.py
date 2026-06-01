"""10 minigame props — cymatic tray, tuning bell set (3), resonance plate,
harmonic pattern tiles (3), frequency slider stand, waveform pillar,
mud pool resonance pad, skeleton key slot, ley line node, sand mandala plate.
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

brass = lambda n: make_material(n, (0.78, 0.60, 0.28), roughness=0.30, metallic=0.7)
wood = lambda n: make_material(n, (0.42, 0.28, 0.16), roughness=0.85)
stone = lambda n: make_material(n, (0.65, 0.62, 0.55), roughness=0.85)

# 1. Cymatic Tray (sand-on-plate vibrating dish)
reset_scene()
cyl("plate", 0.30, 0.02, (0, 0, 0.05), brass("CT_plate"), verts=22)
cyl("sand", 0.28, 0.01, (0, 0, 0.065), make_material("CT_sand", (0.92, 0.78, 0.50), roughness=0.85), verts=22)
# Stand
cyl("post", 0.04, 0.30, (0, 0, -0.10), wood("CT_post"), verts=10)
cube("base", (0, 0, -0.26), (0.15, 0.15, 0.03), wood("CT_base"))
finalize("CymaticTray", "Moon1")

# 2-4. Tuning Bell Set (Low / Mid / High pitch)
def bell(name, scale, tone_color, moon="Shared"):
    reset_scene()
    cone("body", 0.08*scale, 0.10*scale, 0.20*scale, (0, 0, 0.10*scale), brass(name+"_b"), verts=18)
    sphere("crown", 0.08*scale, (0, 0, 0.22*scale), brass(name+"_c"), segs=12, rings=10)
    torus("ring", 0.04*scale, 0.008*scale, (0, 0, 0.30*scale), brass(name+"_r"), mseg=14, miseg=4)
    # Clapper (small ball inside)
    sphere("clapper", 0.025*scale, (0, 0, 0.08*scale), make_material(name+"_clap", (0.30, 0.30, 0.30), roughness=0.50, metallic=0.5), segs=10, rings=8)
    # Resonance halo above
    sphere("halo", 0.05*scale, (0, 0, 0.40*scale), make_material(name+"_halo", tone_color, roughness=0.20, emission=tone_color, emission_strength=1.5), segs=12, rings=10)
    finalize(name, moon)

bell("TuningBell_Low", 1.3, (0.30, 0.55, 0.95), "Moon1")    # E3 cool blue
bell("TuningBell_Mid", 1.0, (0.95, 0.65, 0.20), "Moon1")    # A3 amber
bell("TuningBell_High", 0.8, (0.40, 0.85, 0.50), "Moon1")   # D4 green

# 5. Resonance Plate (geometric — flat plate with patterns)
reset_scene()
plate = make_material("RP_plate", (0.55, 0.50, 0.42), roughness=0.55, metallic=0.4)
glow = make_material("RP_glow", (0.0, 0.0, 0.0), roughness=0.20,
                     emission=(0.40, 0.70, 0.95), emission_strength=1.8)
cyl("disc", 0.40, 0.04, (0, 0, 0.02), plate, verts=24)
# Concentric rings of glow
for r in [0.10, 0.20, 0.30]:
    torus(f"ring_{r}", r, 0.008, (0, 0, 0.04), glow, mseg=24, miseg=4)
# 6-pointed star
for i in range(6):
    a = i*(math.pi/3)
    cube(f"point_{i}", (math.cos(a)*0.18, math.sin(a)*0.18, 0.045), (0.08, 0.02, 0.005), glow, rot=(0, 0, a))
finalize("ResonancePlate", "Moon1")

# 6-8. Harmonic Pattern Tiles (3 different sacred geometry variants)
def pattern_tile(name, variant, moon="Shared"):
    reset_scene()
    base = make_material(name+"_base", (0.92, 0.85, 0.70), roughness=0.65)
    accent = make_material(name+"_accent", (0.0, 0.0, 0.0), roughness=0.20,
                            emission=(0.55, 0.85, 0.45), emission_strength=1.4)
    cube("tile", (0, 0, 0.02), (0.30, 0.30, 0.02), base)
    if variant == "square":
        for i in range(8):
            a = i*(math.pi/4)
            cube(f"line_{i}", (math.cos(a)*0.15, math.sin(a)*0.15, 0.035),
                 (0.16, 0.015, 0.005), accent, rot=(0, 0, a))
    elif variant == "spiral":
        for i in range(12):
            t = i/12.0
            r = 0.25 * t
            a = t * 4 * math.pi
            sphere(f"dot_{i}", 0.012, (math.cos(a)*r, math.sin(a)*r, 0.035), accent, segs=8, rings=6)
    elif variant == "flower":
        torus("c", 0.15, 0.012, (0, 0, 0.035), accent, mseg=20, miseg=4)
        for i in range(6):
            a = i*(math.pi/3)
            torus(f"petal_{i}", 0.10, 0.010, (math.cos(a)*0.10, math.sin(a)*0.10, 0.035), accent, mseg=18, miseg=4)
    finalize(name, moon)

pattern_tile("HarmonicTile_Square", "square", "Moon1")
pattern_tile("HarmonicTile_Spiral", "spiral", "Moon1")
pattern_tile("HarmonicTile_Flower", "flower", "Moon1")

# 9. Frequency Slider Stand (interactive minigame pedestal)
reset_scene()
cube("base", (0, 0, 0.06), (0.30, 0.18, 0.06), wood("FSS_base"))
cube("track", (0, 0, 0.16), (0.28, 0.04, 0.04), wood("FSS_track"))
# Slider knob
cube("knob", (0.05, 0, 0.20), (0.04, 0.06, 0.04), brass("FSS_knob"))
# Marker ticks
for i in range(8):
    x = -0.13 + i*0.037
    cube(f"tick_{i}", (x, 0, 0.18), (0.005, 0.04, 0.005), brass(f"FSS_t{i}"))
finalize("FrequencySliderStand", "Moon1")

# 10. Waveform Visualizer Pillar
reset_scene()
post = wood("WV_post")
glow = make_material("WV_glow", (0.0, 0.0, 0.0), roughness=0.20,
                     emission=(0.40, 0.85, 0.95), emission_strength=2.0)
cube("base", (0, 0, 0.04), (0.20, 0.20, 0.04), post)
cyl("pillar", 0.06, 1.20, (0, 0, 0.68), post, verts=12)
# Glowing waveform on the pillar (12 horizontal bars varying height)
for i in range(12):
    z = 0.20 + i*0.10
    w = 0.10 + 0.06 * math.sin(i*0.7)
    cube(f"bar_{i}", (0, -0.06, z), (w, 0.005, 0.04), glow)
sphere("top", 0.08, (0, 0, 1.35), glow, segs=12, rings=10)
finalize("WaveformPillar", "Moon1")

# 11. Mud Pool Resonance Pad (Moon 1 mini-game stand at each pool)
reset_scene()
mud = make_material("MPP_mud", (0.35, 0.25, 0.15), roughness=0.95)
pad = make_material("MPP_pad", (0.55, 0.45, 0.30), roughness=0.65,
                    emission=(0.40, 0.30, 0.10), emission_strength=0.4)
sigil = make_material("MPP_sigil", (0.0, 0.0, 0.0), roughness=0.20,
                       emission=(0.95, 0.55, 0.20), emission_strength=1.6)
cyl("base", 0.50, 0.06, (0, 0, 0.03), mud, verts=20)
cyl("pad", 0.42, 0.05, (0, 0, 0.08), pad, verts=20)
torus("sigil_ring", 0.25, 0.012, (0, 0, 0.105), sigil, mseg=24, miseg=4)
for i in range(3):
    a = i*(2*math.pi/3)
    cube(f"node_{i}", (math.cos(a)*0.25, math.sin(a)*0.25, 0.105), (0.04, 0.04, 0.02), sigil)
finalize("MudPoolResonancePad", "Moon1")

# 12. Skeleton Key Slot (mounted on Cathedral door)
reset_scene()
brass2 = brass("SKS_brass")
wood2 = wood("SKS_wood")
inside = make_material("SKS_inside", (0.10, 0.08, 0.06), roughness=0.90)
cube("backplate", (0, 0.04, 0), (0.20, 0.04, 0.30), wood2)
# Brass keyhole plate
cube("plate", (0, 0, 0), (0.12, 0.02, 0.22), brass2)
# Keyhole — top round + lower slot
cyl("hole_top", 0.03, 0.04, (0, -0.025, 0.05), inside, rot=(1.5708, 0, 0), verts=12)
cube("hole_slot", (0, -0.025, -0.04), (0.012, 0.04, 0.10), inside)
# Decorative bolts
for x in (-0.05, 0.05):
    for y in (-0.10, 0.10):
        sphere(f"bolt_{x}_{y}", 0.012, (x, -0.005, y), brass2, segs=8, rings=6)
finalize("SkeletonKeySlot", "Moon1")

# 13. Ley Line Node (glowing earth-pin)
reset_scene()
node = make_material("LLN_node", (0.20, 0.55, 0.92), roughness=0.20, metallic=0.3,
                     emission=(0.40, 0.85, 1.0), emission_strength=2.5)
stone_m = make_material("LLN_stone", (0.50, 0.48, 0.42), roughness=0.85)
cyl("base", 0.40, 0.10, (0, 0, 0.05), stone_m, verts=18)
cone("crystal", 0.20, 0.04, 0.80, (0, 0, 0.45), node, verts=8)
# Halo
torus("halo", 0.50, 0.025, (0, 0, 0.20), node, mseg=24, miseg=6)
finalize("LeyLineNode")

print("done gen_minigame_props: 13 props")
