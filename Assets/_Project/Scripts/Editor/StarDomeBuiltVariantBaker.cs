// Hammer Lane 4 — Phase 6.3 — Sprint 11 L8 50ff78ea
// Compose Echohaven_StarDome_Built.prefab from Cathedral kit pieces so the StarDome
// "post-restoration built state" lives as a single authored asset instead of being
// rebuilt every Play from 30+ runtime Instantiate calls in BuildingSpawner.
//
// Menu: Tartaria/6 Bake/Bake StarDome Built Variant
//
// The Cathedral kit GUIDs (Foundation_16x16m / Wall_4x4m_Stone / Column_Ornate_6.5m /
// Dome_Segment_* / Spire_*) are grep-verified in this branch — see
// Assets/_Project/Prefabs/Moon1/Cathedral/*.meta. All emit text-mode YAML because
// ProjectSettings/EditorSettings.asset m_SerializationMode = 2 (ForceText), which
// was the blocker P5.L3 ran into with the binary Buildings/Echohaven_StarDome.prefab.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.EditorTools.Moon1
{
    public static class StarDomeBuiltVariantBaker
    {
        const string OutputPath  = "Assets/_Project/Prefabs/Moon1/Buildings/Echohaven_StarDome_Built.prefab";
        const string KitFolder   = "Assets/_Project/Prefabs/Moon1/Cathedral";

        // Layout matches BuildingSpawner.CreateModularDungeonStarDome (40m diameter / R=20).
        const float Radius        = 20f;
        const int   WallSegments  = 12;
        const int   ColumnCount   = 8;   // alternating with walls (every 45 deg)
        const int   DomeSegments  = 8;   // E/N/NE/NW/S/SE/SW/W
        const float DomeHeight    = 8f;

        [MenuItem("Tartaria/6 Bake/Bake StarDome Built Variant", priority = 640)]
        public static void Bake()
        {
            // -------- Load kit prefabs --------
            var foundation = LoadKit("Foundation_16x16m");
            var wall       = LoadKit("Wall_4x4m_Stone");
            var column     = LoadKit("Column_Ornate_6.5m");
            var archway    = LoadKit("Archway_4x7m");
            var doorGrand  = LoadKit("Door_Grand_3x6m");
            var roseWindow = LoadKit("RoseWindow_4x4m");
            var spireBase  = LoadKit("Spire_Base_2x2m");
            var spireMid   = LoadKit("Spire_Mid_Taper");
            var spireTop   = LoadKit("Spire_Top_MercuryBall");

            var domeSegmentNames = new[]
            {
                "Dome_Segment_N", "Dome_Segment_NE", "Dome_Segment_E", "Dome_Segment_SE",
                "Dome_Segment_S", "Dome_Segment_SW", "Dome_Segment_W", "Dome_Segment_NW",
            };
            var domeSegments = new GameObject[domeSegmentNames.Length];
            for (int i = 0; i < domeSegmentNames.Length; i++)
                domeSegments[i] = LoadKit(domeSegmentNames[i]);

            if (foundation == null || wall == null || column == null)
            {
                Debug.LogError("[StarDomeBuiltVariantBaker] Cathedral kit missing — check " + KitFolder);
                return;
            }

            // -------- Compose hierarchy --------
            var root = new GameObject("Echohaven_StarDome_Built");
            root.transform.position = Vector3.zero;

            // Foundation slab — center.
            var foundationGO = (GameObject)PrefabUtility.InstantiatePrefab(foundation, root.transform);
            foundationGO.transform.localPosition = new Vector3(0f, 0f, 0f);
            foundationGO.transform.localScale    = new Vector3(10f, 1f, 10f); // 40x4x40 mass
            foundationGO.name = "Foundation_Slab";

            // Outer wall ring — 12 segments, every 30 degrees.
            var wallsParent = new GameObject("Walls").transform;
            wallsParent.SetParent(root.transform, false);
            for (int i = 0; i < WallSegments; i++)
            {
                float angleDeg = i * (360f / WallSegments);
                float rad = angleDeg * Mathf.Deg2Rad;
                var w = (GameObject)PrefabUtility.InstantiatePrefab(wall, wallsParent);
                w.transform.localPosition = new Vector3(Radius * Mathf.Cos(rad), 0f, Radius * Mathf.Sin(rad));
                w.transform.localRotation = Quaternion.Euler(0f, -angleDeg + 90f, 0f);
                w.name = $"Wall_{i:D2}";
            }

            // Columns — 8, every 45 degrees, just inside the wall ring.
            var columnsParent = new GameObject("Columns").transform;
            columnsParent.SetParent(root.transform, false);
            for (int i = 0; i < ColumnCount; i++)
            {
                float angleDeg = i * (360f / ColumnCount) + 22.5f;
                float rad = angleDeg * Mathf.Deg2Rad;
                float r = Radius - 2f;
                var c = (GameObject)PrefabUtility.InstantiatePrefab(column, columnsParent);
                c.transform.localPosition = new Vector3(r * Mathf.Cos(rad), 0f, r * Mathf.Sin(rad));
                c.name = $"Column_{i:D2}";
            }

            // Dome cap — 8 segments arranged radially above the slab.
            var domeParent = new GameObject("DomeCap").transform;
            domeParent.SetParent(root.transform, false);
            domeParent.localPosition = new Vector3(0f, DomeHeight, 0f);
            for (int i = 0; i < DomeSegments; i++)
            {
                if (domeSegments[i] == null) continue;
                float angleDeg = i * (360f / DomeSegments);
                var d = (GameObject)PrefabUtility.InstantiatePrefab(domeSegments[i], domeParent);
                d.transform.localPosition = Vector3.zero;
                d.transform.localRotation = Quaternion.Euler(0f, angleDeg, 0f);
                d.name = $"Dome_{domeSegmentNames[i].Substring("Dome_Segment_".Length)}";
            }

            // Cardinal architectural ornaments — Door (S), RoseWindow (N), Archways (E/W).
            if (doorGrand != null)
            {
                var d = (GameObject)PrefabUtility.InstantiatePrefab(doorGrand, root.transform);
                d.transform.localPosition = new Vector3(0f, 0f, -Radius);
                d.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                d.name = "Door_South";
            }
            if (roseWindow != null)
            {
                var r = (GameObject)PrefabUtility.InstantiatePrefab(roseWindow, root.transform);
                r.transform.localPosition = new Vector3(0f, 4f, Radius);
                r.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                r.name = "RoseWindow_North";
            }
            if (archway != null)
            {
                var ae = (GameObject)PrefabUtility.InstantiatePrefab(archway, root.transform);
                ae.transform.localPosition = new Vector3(Radius, 0f, 0f);
                ae.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                ae.name = "Archway_East";

                var aw = (GameObject)PrefabUtility.InstantiatePrefab(archway, root.transform);
                aw.transform.localPosition = new Vector3(-Radius, 0f, 0f);
                aw.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                aw.name = "Archway_West";
            }

            // Crown spire — base + taper + mercury ball, stacked on top.
            if (spireBase != null && spireMid != null && spireTop != null)
            {
                var spireParent = new GameObject("Spire").transform;
                spireParent.SetParent(root.transform, false);
                spireParent.localPosition = new Vector3(0f, DomeHeight + 4f, 0f);

                var sb = (GameObject)PrefabUtility.InstantiatePrefab(spireBase, spireParent);
                sb.transform.localPosition = new Vector3(0f, 0f, 0f);
                sb.name = "Spire_Base";

                var sm = (GameObject)PrefabUtility.InstantiatePrefab(spireMid, spireParent);
                sm.transform.localPosition = new Vector3(0f, 2f, 0f);
                sm.name = "Spire_Mid";

                var st = (GameObject)PrefabUtility.InstantiatePrefab(spireTop, spireParent);
                st.transform.localPosition = new Vector3(0f, 6f, 0f);
                st.name = "Spire_Top";
            }

            // -------- Save as prefab (text mode courtesy of m_SerializationMode = 2) --------
            var dir = Path.GetDirectoryName(OutputPath);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            var saved = PrefabUtility.SaveAsPrefabAsset(root, OutputPath, out bool success);
            Object.DestroyImmediate(root);

            if (!success || saved == null)
            {
                Debug.LogError($"[StarDomeBuiltVariantBaker] FAILED to save prefab at {OutputPath}");
                return;
            }

            AssetDatabase.SaveAssets();

            int childCount = 1                     // foundation
                            + WallSegments         // 12 walls
                            + ColumnCount          // 8 columns
                            + 8                    // 8 dome segments (DomeCap children)
                            + 4                    // door, rose, 2 archways
                            + 3;                   // spire base + mid + top
            Debug.Log($"[StarDomeBuiltVariantBaker] Baked {OutputPath} — {childCount} kit children (12 walls / 8 columns / 8 dome / 4 ornaments / 3 spire / 1 foundation).");
        }

        static GameObject LoadKit(string name)
        {
            var path = $"{KitFolder}/{name}.prefab";
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) Debug.LogWarning($"[StarDomeBuiltVariantBaker] Kit piece missing: {path}");
            return go;
        }
    }
}
