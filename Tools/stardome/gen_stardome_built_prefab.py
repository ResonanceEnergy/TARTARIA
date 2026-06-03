#!/usr/bin/env python3
"""
gen_stardome_built_prefab.py — Hammer Lane 4 (Phase 6.3 / Sprint 11 L8 50ff78ea).

Generate text-mode YAML for Echohaven_StarDome_Built.prefab from the Cathedral kit.
Mirrors StarDomeBuiltVariantBaker.cs (the Editor menu) so the prefab ships on the
branch even before someone runs Unity. Re-running the Editor menu produces an
equivalent (though re-anchored) prefab via PrefabUtility.SaveAsPrefabAsset.

Layout: 40m diameter (R = 20m)
  - 1 Foundation (slab, scaled 10x1x10)
  - 12 Walls (every 30 degrees on the ring)
  - 8 Columns (every 45 degrees, 2m inside the ring)
  - 8 Dome segments (radial, 8m up)
  - 1 Grand Door (S), 1 RoseWindow (N), 2 Archways (E,W)
  - 3 Spire pieces (base/mid/top) stacked above the dome cap
Total: 1 + 12 + 8 + 8 + 4 + 3 = 36 kit children under 5 grouping parents.

Run from repo root:
    python tools/stardome/gen_stardome_built_prefab.py
"""

import math
import os
import sys
from textwrap import indent

# ---- Cathedral kit GUIDs (grep-verified — see Assets/_Project/Prefabs/Moon1/Cathedral/*.meta)
KIT = {
    "Archway_4x7m":          "caead3f95df81b1e2f2d650765dd10e5",
    "Column_Ornate_6.5m":    "5f2845508c71e65a596fb8f10c5cb04d",
    "Dome_Segment_E":        "81cb20724dadfe0d6365ef2e295c3f84",
    "Dome_Segment_N":        "e95bed7cbb8ce7425697ab5a47c2c9af",
    "Dome_Segment_NE":       "765e8008a4859c32ae12cb827e9d51a8",
    "Dome_Segment_NW":       "f7e4c1034780530b75a1b56faf99ad7b",
    "Dome_Segment_S":        "5f0a8f780df96d12cf7b26f226f12ccc",
    "Dome_Segment_SE":       "0b01601eff46204db22e8b972755709c",
    "Dome_Segment_SW":       "8c77ba96baaabdfedc7d69355169f848",
    "Dome_Segment_W":        "fa46b3a45b9733d893fced17e5675d64",
    "Door_Grand_3x6m":       "89da8031047565921875f78e749f61ee",
    "Foundation_16x16m":     "54ceec06d2f94aa0127f34570c89fa9a",
    "RoseWindow_4x4m":       "41a4ed264177486dd4cc825132e57cd2",
    "Spire_Base_2x2m":       "c7b978c90a32cc905e706cb9505d8d85",
    "Spire_Mid_Taper":       "c518eeeadbd9ac8e173d42d79f0167a8",
    "Spire_Top_MercuryBall": "7ea4d4451d596b5b3024ebf587c9cc2f",
    "Wall_4x4m_Stone":       "cff788adda2e0d32126133661cfb9ebf",
}

RADIUS = 20.0
WALL_SEGMENTS = 12
COLUMN_COUNT = 8
DOME_HEIGHT = 8.0

# ---- fileID allocator -------------------------------------------------------------------
# We need stable, non-colliding fileIDs for:
#   * the root GameObject + Transform (we use 100000, 400000 to match the kit convention,
#     except those are used by kit prefabs themselves so we reserve a fresh range).
#   * each PrefabInstance block (uses 64-bit IDs).
#   * each "stripped" GameObject + Transform block per child (also 64-bit).
#   * grouping (Walls/Columns/DomeCap/Spire) GameObjects + Transforms (12-digit IDs).
#
# Strategy: use deterministic counters seeded from a large prime so IDs don't collide
# with kit fileIDs (which are 100000 / 400000 / 2300000 / etc).
_next_id = 1000000000000000000  # 19 digits, far away from kit fileID space.

def alloc():
    global _next_id
    val = _next_id
    _next_id += 1
    return val

# ---- YAML emitters ----------------------------------------------------------------------

def emit_header():
    return "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n"

def emit_root_gameobject(go_id, transform_id, name, child_transform_ids):
    children = "\n".join(f"  - {{fileID: {tid}}}" for tid in child_transform_ids)
    if not children:
        children_block = "  m_Children: []"
    else:
        children_block = f"  m_Children:\n{children}"
    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {transform_id}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{transform_id}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
{children_block}
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
"""

def emit_group_gameobject(go_id, transform_id, parent_transform_id, name,
                          child_transform_ids, local_pos=(0.0, 0.0, 0.0)):
    px, py, pz = local_pos
    children = "\n".join(f"  - {{fileID: {tid}}}" for tid in child_transform_ids)
    if not children:
        children_block = "  m_Children: []"
    else:
        children_block = f"  m_Children:\n{children}"
    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {transform_id}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{transform_id}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: {px:g}, y: {py:g}, z: {pz:g}}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
{children_block}
  m_Father: {{fileID: {parent_transform_id}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
"""

def emit_prefab_instance(instance_id, stripped_go_id, stripped_xform_id,
                          parent_xform_id, kit_guid, name,
                          local_pos=(0.0, 0.0, 0.0),
                          local_rot_quat=(0.0, 0.0, 0.0, 1.0),
                          local_euler_hint=(0.0, 0.0, 0.0),
                          local_scale=(1.0, 1.0, 1.0)):
    """
    Emit a !u!1001 PrefabInstance + a 'stripped' root GameObject + 'stripped' Transform
    that point back into the kit prefab via guid+fileID(100000 / 400000).
    """
    # In each kit prefab, root GameObject = fileID 100000, root Transform = fileID 400000.
    KIT_ROOT_GO = 100000
    KIT_ROOT_XFORM = 400000

    px, py, pz = local_pos
    qx, qy, qz, qw = local_rot_quat
    ex, ey, ez = local_euler_hint
    sx, sy, sz = local_scale

    mods = []
    for prop, val in [("m_LocalPosition.x", px), ("m_LocalPosition.y", py), ("m_LocalPosition.z", pz),
                      ("m_LocalRotation.x", qx), ("m_LocalRotation.y", qy), ("m_LocalRotation.z", qz),
                      ("m_LocalRotation.w", qw),
                      ("m_LocalEulerAnglesHint.x", ex), ("m_LocalEulerAnglesHint.y", ey),
                      ("m_LocalEulerAnglesHint.z", ez),
                      ("m_LocalScale.x", sx), ("m_LocalScale.y", sy), ("m_LocalScale.z", sz)]:
        mods.append(f"""    - target: {{fileID: {KIT_ROOT_XFORM}, guid: {kit_guid}, type: 3}}
      propertyPath: {prop}
      value: {val:g}
      objectReference: {{fileID: 0}}""")
    mods.append(f"""    - target: {{fileID: {KIT_ROOT_GO}, guid: {kit_guid}, type: 3}}
      propertyPath: m_Name
      value: {name}
      objectReference: {{fileID: 0}}""")
    mods_block = "\n".join(mods)

    return f"""--- !u!1001 &{instance_id}
PrefabInstance:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Modification:
    serializedVersion: 3
    m_TransformParent: {{fileID: {parent_xform_id}}}
    m_Modifications:
{mods_block}
    m_RemovedComponents: []
    m_RemovedGameObjects: []
    m_AddedGameObjects: []
    m_AddedComponents: []
  m_SourcePrefab: {{fileID: 100100000, guid: {kit_guid}, type: 3}}
--- !u!1 &{stripped_go_id} stripped
GameObject:
  m_CorrespondingSourceObject: {{fileID: {KIT_ROOT_GO}, guid: {kit_guid}, type: 3}}
  m_PrefabInstance: {{fileID: {instance_id}}}
  m_PrefabAsset: {{fileID: 0}}
--- !u!4 &{stripped_xform_id} stripped
Transform:
  m_CorrespondingSourceObject: {{fileID: {KIT_ROOT_XFORM}, guid: {kit_guid}, type: 3}}
  m_PrefabInstance: {{fileID: {instance_id}}}
  m_PrefabAsset: {{fileID: 0}}
"""

# ---- Layout ------------------------------------------------------------------------------

def yaw_quat(angle_deg):
    """Return (x,y,z,w) for a Y-axis rotation in degrees."""
    half = math.radians(angle_deg) / 2.0
    return (0.0, math.sin(half), 0.0, math.cos(half))

def build():
    # Allocate root + grouping IDs.
    root_go     = alloc(); root_xform     = alloc()
    walls_go    = alloc(); walls_xform    = alloc()
    columns_go  = alloc(); columns_xform  = alloc()
    domecap_go  = alloc(); domecap_xform  = alloc()
    spire_go    = alloc(); spire_xform    = alloc()

    # Each kit child needs: PrefabInstance + stripped GO + stripped Transform.
    def kit_child(parent_xform, kit_name, name, pos, rot_deg=0.0, scale=(1.0,1.0,1.0)):
        inst = alloc(); sgo = alloc(); sxf = alloc()
        rot = yaw_quat(rot_deg)
        euler = (0.0, rot_deg, 0.0)
        return {
            "instance": inst, "stripped_go": sgo, "stripped_xform": sxf,
            "parent_xform": parent_xform, "guid": KIT[kit_name], "name": name,
            "pos": pos, "rot": rot, "euler": euler, "scale": scale,
        }

    children = []

    # Foundation slab — directly under root, scaled to ~40x4x40.
    children.append(kit_child(root_xform, "Foundation_16x16m", "Foundation_Slab",
                              (0.0, 0.0, 0.0), 0.0, (10.0, 1.0, 10.0)))

    # 12 walls every 30 degrees, facing inward.
    walls_kids = []
    for i in range(WALL_SEGMENTS):
        ang = i * (360.0 / WALL_SEGMENTS)
        rad = math.radians(ang)
        pos = (RADIUS * math.cos(rad), 0.0, RADIUS * math.sin(rad))
        walls_kids.append(kit_child(walls_xform, "Wall_4x4m_Stone", f"Wall_{i:02d}",
                                    pos, -ang + 90.0))

    # 8 columns every 45 degrees, 2m inside the ring.
    columns_kids = []
    for i in range(COLUMN_COUNT):
        ang = i * (360.0 / COLUMN_COUNT) + 22.5
        rad = math.radians(ang)
        r = RADIUS - 2.0
        pos = (r * math.cos(rad), 0.0, r * math.sin(rad))
        columns_kids.append(kit_child(columns_xform, "Column_Ornate_6.5m", f"Column_{i:02d}", pos))

    # 8 dome segments, radial, on the dome cap (8m up).
    dome_kids = []
    dome_segment_names = [
        "Dome_Segment_N", "Dome_Segment_NE", "Dome_Segment_E", "Dome_Segment_SE",
        "Dome_Segment_S", "Dome_Segment_SW", "Dome_Segment_W", "Dome_Segment_NW",
    ]
    for i, seg_name in enumerate(dome_segment_names):
        ang = i * (360.0 / len(dome_segment_names))
        short = seg_name[len("Dome_Segment_"):]
        dome_kids.append(kit_child(domecap_xform, seg_name, f"Dome_{short}",
                                   (0.0, 0.0, 0.0), ang))

    # Architectural ornaments — direct under root.
    children.append(kit_child(root_xform, "Door_Grand_3x6m", "Door_South",
                              (0.0, 0.0, -RADIUS), 0.0))
    children.append(kit_child(root_xform, "RoseWindow_4x4m", "RoseWindow_North",
                              (0.0, 4.0, RADIUS), 180.0))
    children.append(kit_child(root_xform, "Archway_4x7m", "Archway_East",
                              (RADIUS, 0.0, 0.0), -90.0))
    children.append(kit_child(root_xform, "Archway_4x7m", "Archway_West",
                              (-RADIUS, 0.0, 0.0), 90.0))

    # Spire stack on top.
    spire_kids = [
        kit_child(spire_xform, "Spire_Base_2x2m",      "Spire_Base", (0.0, 0.0, 0.0)),
        kit_child(spire_xform, "Spire_Mid_Taper",      "Spire_Mid",  (0.0, 2.0, 0.0)),
        kit_child(spire_xform, "Spire_Top_MercuryBall","Spire_Top",  (0.0, 6.0, 0.0)),
    ]

    # ---- Emit ---------------------------------------------------------------------------
    out = [emit_header()]

    # Root GameObject — children = foundation slab + 4 ornaments + 4 group containers.
    root_child_xforms = (
        [c["stripped_xform"] for c in children]
        + [walls_xform, columns_xform, domecap_xform, spire_xform]
    )
    out.append(emit_root_gameobject(root_go, root_xform, "Echohaven_StarDome_Built",
                                     root_child_xforms))

    # Grouping containers.
    out.append(emit_group_gameobject(walls_go, walls_xform, root_xform, "Walls",
                                      [c["stripped_xform"] for c in walls_kids]))
    out.append(emit_group_gameobject(columns_go, columns_xform, root_xform, "Columns",
                                      [c["stripped_xform"] for c in columns_kids]))
    out.append(emit_group_gameobject(domecap_go, domecap_xform, root_xform, "DomeCap",
                                      [c["stripped_xform"] for c in dome_kids],
                                      local_pos=(0.0, DOME_HEIGHT, 0.0)))
    out.append(emit_group_gameobject(spire_go, spire_xform, root_xform, "Spire",
                                      [c["stripped_xform"] for c in spire_kids],
                                      local_pos=(0.0, DOME_HEIGHT + 4.0, 0.0)))

    # All kit PrefabInstances + their stripped roots.
    for group in (children, walls_kids, columns_kids, dome_kids, spire_kids):
        for c in group:
            out.append(emit_prefab_instance(
                c["instance"], c["stripped_go"], c["stripped_xform"],
                c["parent_xform"], c["guid"], c["name"],
                local_pos=c["pos"], local_rot_quat=c["rot"],
                local_euler_hint=c["euler"], local_scale=c["scale"],
            ))

    return "".join(out), 1 + len(walls_kids) + len(columns_kids) + len(dome_kids) + 4 + len(spire_kids)


def main():
    here = os.path.dirname(os.path.abspath(__file__))
    repo_root = os.path.abspath(os.path.join(here, "..", ".."))
    out_path = os.path.join(repo_root, "Assets", "_Project", "Prefabs", "Moon1",
                            "Echohaven_StarDome_Built.prefab")

    yaml_text, child_count = build()
    with open(out_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(yaml_text)

    print(f"Wrote {out_path}")
    print(f"Kit children: {child_count}")
    print(f"  - 1 Foundation slab")
    print(f"  - {WALL_SEGMENTS} Walls")
    print(f"  - {COLUMN_COUNT} Columns")
    print(f"  - 8 Dome segments")
    print(f"  - 4 Ornaments (Door / Rose / 2x Archway)")
    print(f"  - 3 Spire pieces")


if __name__ == "__main__":
    main()
