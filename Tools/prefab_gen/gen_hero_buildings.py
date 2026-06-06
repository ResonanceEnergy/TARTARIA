#!/usr/bin/env python3
"""
Generate hero building prefabs by composing Cathedral kit pieces.

Writes 4 prefabs at Assets/_Project/Prefabs/Moon1/Buildings/:
  Echohaven_StarDome.prefab        (~25 kit pieces)
  Echohaven_CrystalSpire.prefab    (~12 kit pieces)
  Echohaven_HarmonicFountain.prefab (~10 kit pieces)
  Echohaven_Cathedral.prefab       (~30 kit pieces, NEW)

Each prefab structure:
  Root GameObject "Echohaven_<Name>"
   - Transform (id=ROOT_T)
      - HeroMesh_Kit GameObject
         - Transform (id=KIT_T)
            - <N PrefabInstance children>

All fileIDs are deterministic 19-digit ints generated from sha1 of (prefab_name, slot_index).
"""

import hashlib
import math
import os
import sys
from pathlib import Path

# === Cathedral kit GUIDs (from disk meta files) ===
GUID = {
    "Archway_4x7m":           "caead3f95df81b1e2f2d650765dd10e5",
    "Column_Ornate_6_5m":     "5f2845508c71e65a596fb8f10c5cb04d",
    "Dome_Segment_E":         "81cb20724dadfe0d6365ef2e295c3f84",
    "Dome_Segment_N":         "e95bed7cbb8ce7425697ab5a47c2c9af",
    "Dome_Segment_NE":        "765e8008a4859c32ae12cb827e9d51a8",
    "Dome_Segment_NW":        "f7e4c1034780530b75a1b56faf99ad7b",
    "Dome_Segment_S":         "5f0a8f780df96d12cf7b26f226f12ccc",
    "Dome_Segment_SE":        "0b01601eff46204db22e8b972755709c",
    "Dome_Segment_SW":        "8c77ba96baaabdfedc7d69355169f848",
    "Dome_Segment_W":         "fa46b3a45b9733d893fced17e5675d64",
    "Door_Grand_3x6m":        "89da8031047565921875f78e749f61ee",
    "Foundation_16x16m":      "54ceec06d2f94aa0127f34570c89fa9a",
    "RoseWindow_4x4m":        "41a4ed264177486dd4cc825132e57cd2",
    "Spire_Base_2x2m":        "c7b978c90a32cc905e706cb9505d8d85",
    "Spire_Mid_Taper":        "c518eeeadbd9ac8e173d42d79f0167a8",
    "Spire_Top_MercuryBall":  "7ea4d4451d596b5b3024ebf587c9cc2f",
    "Wall_4x4m_Stone":        "cff788adda2e0d32126133661cfb9ebf",
    "Wall_Corner_4x4m":       "704d988f5bc6eb93c9a924a916f017f0",
}


def stable_id(prefab_name: str, slot: str) -> int:
    """Deterministic 18-19 digit positive int from sha1, fits in Unity fileID space."""
    h = hashlib.sha1(f"{prefab_name}|{slot}".encode("utf-8")).hexdigest()
    # Take 16 hex chars -> 64-bit number, mask top bit to keep positive when shown as int64
    n = int(h[:16], 16) & 0x7FFFFFFFFFFFFFFF
    # Pad to look like Unity's IDs (15-19 digits)
    if n < 10**17:
        n = n * 10 + (int(h[16:18], 16) % 10)
    return n


def yaw_quat(yaw_deg: float):
    """Return (w, x, y, z) quaternion for Y-axis rotation in degrees."""
    rad = math.radians(yaw_deg)
    return (math.cos(rad / 2.0), 0.0, math.sin(rad / 2.0), 0.0)


def fmt(v):
    """Format float for Unity YAML (compact but precise)."""
    if isinstance(v, int):
        return str(v)
    if v == 0:
        return "0"
    if abs(v) < 1e-6:
        return "0"
    s = f"{v:.7f}".rstrip("0").rstrip(".")
    return s if s else "0"


PI_BLOCK = """--- !u!1001 &{pi_id}
PrefabInstance:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Modification:
    serializedVersion: 3
    m_TransformParent: {{fileID: {kit_t_id}}}
    m_Modifications:
    - target: {{fileID: 100000, guid: {guid}, type: 3}}
      propertyPath: m_Name
      value: {kit_name}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalScale.x
      value: {sx}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalScale.y
      value: {sy}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalScale.z
      value: {sz}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalPosition.x
      value: {px}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalPosition.y
      value: {py}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalPosition.z
      value: {pz}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalRotation.w
      value: {qw}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalRotation.x
      value: {qx}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalRotation.y
      value: {qy}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalRotation.z
      value: {qz}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalEulerAnglesHint.x
      value: 0
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalEulerAnglesHint.y
      value: {yaw}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 400000, guid: {guid}, type: 3}}
      propertyPath: m_LocalEulerAnglesHint.z
      value: 0
      objectReference: {{fileID: 0}}
    m_RemovedComponents: []
    m_RemovedGameObjects: []
    m_AddedGameObjects: []
    m_AddedComponents: []
  m_SourcePrefab: {{fileID: 100100000, guid: {guid}, type: 3}}
--- !u!4 &{t_id} stripped
Transform:
  m_CorrespondingSourceObject: {{fileID: 400000, guid: {guid}, type: 3}}
  m_PrefabInstance: {{fileID: {pi_id}}}
  m_PrefabAsset: {{fileID: 0}}
"""


def build_prefab(prefab_name: str, root_name: str, root_scale: float, pieces):
    """Compose a full Unity prefab YAML from a list of (kit_key, pos, yaw_deg, scale) tuples."""
    root_go_id = stable_id(prefab_name, "root_go")
    root_t_id  = stable_id(prefab_name, "root_t")
    kit_go_id  = stable_id(prefab_name, "kit_go")
    kit_t_id   = stable_id(prefab_name, "kit_t")

    # Generate per-piece IDs
    piece_records = []
    for idx, p in enumerate(pieces):
        kit_key, (px, py, pz), yaw, scale = p
        if isinstance(scale, (int, float)):
            sx = sy = sz = scale
        else:
            sx, sy, sz = scale
        pi_id = stable_id(prefab_name, f"pi_{idx}")
        t_id  = stable_id(prefab_name, f"t_{idx}")
        qw, qx, qy, qz = yaw_quat(yaw)
        piece_records.append({
            "pi_id": pi_id, "t_id": t_id, "guid": GUID[kit_key],
            "kit_name": kit_key.replace("_6_5m", "_6.5m"),  # restore the dot
            "px": fmt(px), "py": fmt(py), "pz": fmt(pz),
            "sx": fmt(sx), "sy": fmt(sy), "sz": fmt(sz),
            "qw": fmt(qw), "qx": fmt(qx), "qy": fmt(qy), "qz": fmt(qz),
            "yaw": fmt(yaw),
            "kit_t_id": kit_t_id,
        })

    # Build header
    out = []
    out.append("%YAML 1.1")
    out.append("%TAG !u! tag:unity3d.com,2011:")
    out.append(f"--- !u!1 &{root_go_id}")
    out.append("GameObject:")
    out.append("  m_ObjectHideFlags: 0")
    out.append("  m_CorrespondingSourceObject: {fileID: 0}")
    out.append("  m_PrefabInstance: {fileID: 0}")
    out.append("  m_PrefabAsset: {fileID: 0}")
    out.append("  serializedVersion: 6")
    out.append("  m_Component:")
    out.append(f"  - component: {{fileID: {root_t_id}}}")
    out.append("  m_Layer: 0")
    out.append(f"  m_Name: {root_name}")
    out.append("  m_TagString: Untagged")
    out.append("  m_Icon: {fileID: 0}")
    out.append("  m_NavMeshLayer: 0")
    out.append("  m_StaticEditorFlags: 0")
    out.append("  m_IsActive: 1")
    out.append(f"--- !u!4 &{root_t_id}")
    out.append("Transform:")
    out.append("  m_ObjectHideFlags: 0")
    out.append("  m_CorrespondingSourceObject: {fileID: 0}")
    out.append("  m_PrefabInstance: {fileID: 0}")
    out.append("  m_PrefabAsset: {fileID: 0}")
    out.append(f"  m_GameObject: {{fileID: {root_go_id}}}")
    out.append("  serializedVersion: 2")
    out.append("  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}")
    out.append("  m_LocalPosition: {x: 0, y: 0, z: 0}")
    out.append(f"  m_LocalScale: {{x: {fmt(root_scale)}, y: {fmt(root_scale)}, z: {fmt(root_scale)}}}")
    out.append("  m_ConstrainProportionsScale: 0")
    out.append("  m_Children:")
    out.append(f"  - {{fileID: {kit_t_id}}}")
    out.append("  m_Father: {fileID: 0}")
    out.append("  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}")
    out.append(f"--- !u!1 &{kit_go_id}")
    out.append("GameObject:")
    out.append("  m_ObjectHideFlags: 0")
    out.append("  m_CorrespondingSourceObject: {fileID: 0}")
    out.append("  m_PrefabInstance: {fileID: 0}")
    out.append("  m_PrefabAsset: {fileID: 0}")
    out.append("  serializedVersion: 6")
    out.append("  m_Component:")
    out.append(f"  - component: {{fileID: {kit_t_id}}}")
    out.append("  m_Layer: 0")
    out.append("  m_Name: HeroMesh_Kit")
    out.append("  m_TagString: Untagged")
    out.append("  m_Icon: {fileID: 0}")
    out.append("  m_NavMeshLayer: 0")
    out.append("  m_StaticEditorFlags: 0")
    out.append("  m_IsActive: 1")
    out.append(f"--- !u!4 &{kit_t_id}")
    out.append("Transform:")
    out.append("  m_ObjectHideFlags: 0")
    out.append("  m_CorrespondingSourceObject: {fileID: 0}")
    out.append("  m_PrefabInstance: {fileID: 0}")
    out.append("  m_PrefabAsset: {fileID: 0}")
    out.append(f"  m_GameObject: {{fileID: {kit_go_id}}}")
    out.append("  serializedVersion: 2")
    out.append("  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}")
    out.append("  m_LocalPosition: {x: 0, y: 0, z: 0}")
    out.append("  m_LocalScale: {x: 1, y: 1, z: 1}")
    out.append("  m_ConstrainProportionsScale: 0")
    out.append("  m_Children:")
    for rec in piece_records:
        out.append(f"  - {{fileID: {rec['t_id']}}}")
    out.append(f"  m_Father: {{fileID: {root_t_id}}}")
    out.append("  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}")

    for rec in piece_records:
        out.append(PI_BLOCK.format(**rec).rstrip())

    out.append("")  # trailing newline
    return "\n".join(out)


# ====================================================================
# Composition specs
# ====================================================================

def stardome_pieces():
    """StarDome: foundation + 4 corners + 8 columns ring + 8 dome + spire top + 4 archways."""
    pieces = []
    # Foundation
    pieces.append(("Foundation_16x16m", (0, 0, 0), 0, 1.0))
    # 4 Wall_Corner_4x4m at ±10, ±10 (cardinal corners)
    # Use 8 corners for full octagonal outline at radius ~7
    corner_configs = [
        ((7, 0, 7),   0),    ((-7, 0, 7),  90),
        ((-7, 0, -7), 180),  ((7, 0, -7), 270),
    ]
    for pos, yaw in corner_configs:
        pieces.append(("Wall_Corner_4x4m", pos, yaw, 1.0))
    # 8 Column_Ornate at radius 6, height 0
    for i in range(8):
        ang = i * 45.0
        r = 6.0
        x = r * math.cos(math.radians(ang))
        z = r * math.sin(math.radians(ang))
        pieces.append(("Column_Ornate_6_5m", (x, 0, z), ang, 1.0))
    # 8 Dome segments at height 7
    dome_keys = ["Dome_Segment_E", "Dome_Segment_NE", "Dome_Segment_N", "Dome_Segment_NW",
                 "Dome_Segment_W", "Dome_Segment_SW", "Dome_Segment_S", "Dome_Segment_SE"]
    for i, key in enumerate(dome_keys):
        ang = i * 45.0
        r = 2.8
        x = r * math.cos(math.radians(ang))
        z = r * math.sin(math.radians(ang))
        pieces.append((key, (x, 7.0, z), ang, 1.0))
    # Spire top at center
    pieces.append(("Spire_Top_MercuryBall", (0, 13, 0), 0, 1.2))
    # 4 Archways at cardinal directions
    for ang in (0, 90, 180, 270):
        r = 8.0
        x = r * math.cos(math.radians(ang))
        z = r * math.sin(math.radians(ang))
        pieces.append(("Archway_4x7m", (x, 0, z), ang, 1.0))
    return pieces


def crystalspire_pieces():
    """CrystalSpire: foundation, 4 stacked spire bases, 2 mid tapers, 1 top."""
    pieces = []
    pieces.append(("Foundation_16x16m", (0, 0, 0), 0, 0.3))  # smaller base
    # 4 Spire_Base stacked
    for i, y in enumerate([0, 3, 6, 9]):
        pieces.append(("Spire_Base_2x2m", (0, y, 0), i * 22.5, 1.0))
    # 2 mid tapers
    pieces.append(("Spire_Mid_Taper", (0, 12, 0), 0, 1.0))
    pieces.append(("Spire_Mid_Taper", (0, 13.5, 0), 45, 0.85))
    # Top
    pieces.append(("Spire_Top_MercuryBall", (0, 15, 0), 0, 1.0))
    # 4 small columns around base for support
    for ang in (45, 135, 225, 315):
        r = 2.0
        x = r * math.cos(math.radians(ang))
        z = r * math.sin(math.radians(ang))
        pieces.append(("Column_Ornate_6_5m", (x, 0, z), ang, 0.5))
    return pieces


def harmonicfountain_pieces():
    """Fountain: foundation, 8 short columns in ring, 1 central spire base, decorative pieces."""
    pieces = []
    pieces.append(("Foundation_16x16m", (0, 0, 0), 0, 0.5))
    # 8 Column_Ornate at radius 3 (short scale)
    for i in range(8):
        ang = i * 45.0
        r = 3.0
        x = r * math.cos(math.radians(ang))
        z = r * math.sin(math.radians(ang))
        pieces.append(("Column_Ornate_6_5m", (x, 0, z), ang, 0.55))
    # Central decorative spire base as fountain basin
    pieces.append(("Spire_Base_2x2m", (0, 0, 0), 0, 1.2))
    # Top: small mercury ball as water spout
    pieces.append(("Spire_Top_MercuryBall", (0, 3.5, 0), 0, 0.6))
    return pieces


def cathedral_pieces():
    """Cathedral: stacked foundation, nave columns 4x4, walls, archways, dome at east, spire at west."""
    pieces = []
    # 2 stacked Foundation_16x16m (extends nave length)
    pieces.append(("Foundation_16x16m", (0, 0, 0), 0, 1.0))
    pieces.append(("Foundation_16x16m", (16, 0, 0), 0, 1.0))
    # 16 Column_Ornate in nave layout (4 rows × 4 cols) along X 0..14, Z -3, -1, 1, 3 -> use 4x4 grid
    for ix in range(4):
        for iz in range(4):
            x = ix * 5.0 - 1.5
            z = iz * 4.0 - 6.0
            pieces.append(("Column_Ornate_6_5m", (x, 0, z), 0, 1.0))
    # 8 Wall_4x4m_Stone forming nave outer (4 each side)
    for ix in range(4):
        x = ix * 4.0
        pieces.append(("Wall_4x4m_Stone", (x, 0, -8.0),  0, 1.0))
        pieces.append(("Wall_4x4m_Stone", (x, 0,  8.0), 180, 1.0))
    # 4 Archway entries (one at each cardinal side)
    pieces.append(("Archway_4x7m", (-4, 0, 0), 90, 1.0))   # west entrance
    pieces.append(("Archway_4x7m", (8, 0, -8), 0, 1.0))    # south
    pieces.append(("Archway_4x7m", (8, 0,  8), 180, 1.0))  # north
    pieces.append(("Archway_4x7m", (20, 0, 0), 270, 1.0))  # east (apse approach)
    # Grand door at west
    pieces.append(("Door_Grand_3x6m", (-4, 0, 0), 90, 1.0))
    # 4 Dome_Segment over crossing at center (8,0,0)
    for key, ang in [("Dome_Segment_E", 0), ("Dome_Segment_N", 90),
                     ("Dome_Segment_W", 180), ("Dome_Segment_S", 270)]:
        r = 2.5
        cx = 8 + r * math.cos(math.radians(ang))
        cz = 0 + r * math.sin(math.radians(ang))
        pieces.append((key, (cx, 7, cz), ang, 1.0))
    # RoseWindow on west wall
    pieces.append(("RoseWindow_4x4m", (-4, 5, 0), 90, 1.0))
    # Spire composition at west (3 stacked + top + mid taper)
    pieces.append(("Spire_Base_2x2m", (-4, 0, 0),  0, 1.0))
    pieces.append(("Spire_Base_2x2m", (-4, 3, 0),  0, 1.0))
    pieces.append(("Spire_Base_2x2m", (-4, 6, 0),  0, 1.0))
    pieces.append(("Spire_Mid_Taper", (-4, 9, 0),  0, 1.0))
    pieces.append(("Spire_Top_MercuryBall", (-4, 12, 0), 0, 1.0))
    return pieces


# ====================================================================
# Run
# ====================================================================

def main():
    repo_root = Path(__file__).resolve().parents[2]
    buildings_dir = repo_root / "Assets/_Project/Prefabs/Moon1/Buildings"
    buildings_dir.mkdir(parents=True, exist_ok=True)

    specs = [
        ("Echohaven_StarDome",        stardome_pieces(),        3.2727273),
        ("Echohaven_CrystalSpire",    crystalspire_pieces(),    1.0714285),
        ("Echohaven_HarmonicFountain", harmonicfountain_pieces(), 1.0),
        ("Echohaven_Cathedral",       cathedral_pieces(),       1.0),
    ]

    for name, pieces, root_scale in specs:
        yaml_text = build_prefab(name, name, root_scale, pieces)
        out_path = buildings_dir / f"{name}.prefab"
        out_path.write_text(yaml_text, encoding="utf-8")
        line_count = yaml_text.count("\n") + 1
        print(f"WROTE  {out_path.relative_to(repo_root)}  pieces={len(pieces)}  lines={line_count}")

    print("DONE")


if __name__ == "__main__":
    main()
