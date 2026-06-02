"""Master batch — Moon 1 Blender asset production. Run via:
  blender --background --python tools/blender/run_all_moon1.py
"""
import os
SCRIPTS = [
    # Tier 0 props (originally shipped)
    "gen_anastasia_chair.py",
    "gen_brazier.py",
    "gen_aether_crystals.py",
    "gen_bobs_inn.py",
    "gen_tuning_pedestal.py",
    # Tier 1 add-ons
    "gen_mud_pool_basin.py",
    "gen_lore_artifact_scroll.py",
    "gen_giant_skeleton_key.py",
    "gen_skeleton_remains.py",
    "gen_pipe_organ.py",
    # Tier 2 — character roster (2026-05-31 NATRIX mandate "build all the characters")
    "gen_characters_humanoid.py",      # 6 OG characters: Milo, Anastasia, Lirael, Cassian, Bob, Generic
    "gen_characters_enemies.py",       # 8 enemies: MudGolem, ResetScout, CrystalSentry, etc.
    "gen_characters_complete.py",      # 16 new: PlayerHero, GiantGolem, VoidPhantom, Bishop, OrganPlayer, 5 villagers, Pilgrim, Pickpocket, BlackSmith, Beggar, FortuneTeller
    # Sprint 8 Lane 8 — refined NPC humanoid stand-ins (canonical names Lirael/Anastasia/Cassian,
    # upgrade path from primitive capsule prefabs per docs/audits/MOON1_ACCEPTANCE_2026-06-02.md)
    "gen_npc_lirael.py",
    "gen_npc_anastasia.py",
    "gen_npc_cassian.py",
]
base = os.path.dirname(__file__)
for s in SCRIPTS:
    print(f"\n=== Running {s} ===")
    exec(compile(open(os.path.join(base, s)).read(), s, 'exec'))
print("\n[TARTARIA] All Moon 1 Blender assets generated.")
