#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 1 cathedral kit dressing tool.
    ///
    /// Per Audit 5 the cathedral kit had 0/18 pieces placed in the scene. This menu
    /// instantiates all 18 cathedral kit pieces (foundation, 8 dome segments, walls,
    /// columns, door, archway, rose window, spire stack) plus the pipe organ visual
    /// around the existing Building_echohaven_stardome anchor.
    ///
    /// Idempotent: a previous "Cathedral_KitDressing" parent is deleted and rebuilt.
    /// All placements use PrefabUtility.InstantiatePrefab to keep the prefab link.
    /// </summary>
    public static class Moon1CathedralKitDressing
    {
        private const string ParentName = "Cathedral_KitDressing";
        private const string AnchorName = "Building_echohaven_stardome";

        // -----------------------------------------------------------------
        // Prefab paths (22 total) — used both at runtime and reported in the
        // final summary dialog so an artist can see exactly what was attempted.
        // -----------------------------------------------------------------
        private const string KitRoot = "Assets/_Project/Prefabs/Moon1/Cathedral/";
        private const string BlenderRoot = "Assets/_Project/Prefabs/Moon1/Blender/";

        private static readonly string PathFoundation = KitRoot + "Foundation_16x16m.prefab";
        private static readonly string PathArchway = KitRoot + "Archway_4x7m.prefab";
        private static readonly string PathWallStone = KitRoot + "Wall_4x4m_Stone.prefab";
        private static readonly string PathWallCorner = KitRoot + "Wall_Corner_4x4m.prefab";
        private static readonly string PathColumn = KitRoot + "Column_Ornate_6.5m.prefab";
        private static readonly string PathDoor = KitRoot + "Door_Grand_3x6m.prefab";
        private static readonly string PathRoseWindow = KitRoot + "RoseWindow_4x4m.prefab";
        private static readonly string PathSpireBase = KitRoot + "Spire_Base_2x2m.prefab";
        private static readonly string PathSpireMid = KitRoot + "Spire_Mid_Taper.prefab";
        private static readonly string PathSpireTop = KitRoot + "Spire_Top_MercuryBall.prefab";
        private static readonly string PathPipeOrgan = BlenderRoot + "Architecture/PipeOrganCathedral.prefab";

        private static readonly string[] DomeSegmentNames =
        {
            "Dome_Segment_N",
            "Dome_Segment_NE",
            "Dome_Segment_E",
            "Dome_Segment_SE",
            "Dome_Segment_S",
            "Dome_Segment_SW",
            "Dome_Segment_W",
            "Dome_Segment_NW",
        };

        [MenuItem("Tartaria/1 Build/Dress Cathedral (Kit Pieces + Spire + Pipe Organ)", priority = 103)]
        public static void DressCathedral()
        {
            // ---- Locate the anchor in the active scene ---------------------
            Vector3 anchorPos = new Vector3(0f, 0f, 100f);
            GameObject anchor = GameObject.Find(AnchorName);
            if (anchor != null)
            {
                anchorPos = anchor.transform.position;
                Debug.Log($"[Moon1CathedralKitDressing] Anchor '{AnchorName}' found at {anchorPos}.");
            }
            else
            {
                Debug.LogWarning(
                    $"[Moon1CathedralKitDressing] Anchor '{AnchorName}' not found in scene. " +
                    $"Falling back to default position {anchorPos}.");
            }

            // ---- Idempotent rebuild: clear existing parent ----------------
            GameObject existing = GameObject.Find(ParentName);
            if (existing != null)
            {
                Debug.Log($"[Moon1CathedralKitDressing] Existing '{ParentName}' found — removing for rebuild.");
                Object.DestroyImmediate(existing);
            }

            GameObject parent = new GameObject(ParentName);
            parent.transform.position = anchorPos;
            parent.transform.rotation = Quaternion.identity;

            int attempted = 0;
            int placed = 0;
            List<string> missing = new List<string>();

            // ---- Foundation -----------------------------------------------
            attempted++;
            if (PlaceLocal(PathFoundation, parent.transform, new Vector3(0f, 0f, 0f), Quaternion.identity, missing))
                placed++;

            // ---- 8 dome segments in a ring at radius 8, height 6 ----------
            // Angles: N=0, NE=45, E=90, SE=135, S=180, SW=225, W=270, NW=315
            // Local-space angle convention: angle 0 = +Z (north). Each segment is
            // positioned on the ring and rotated to face the center.
            for (int i = 0; i < DomeSegmentNames.Length; i++)
            {
                attempted++;
                float angleDeg = i * 45f;
                float angleRad = angleDeg * Mathf.Deg2Rad;
                // North at angle 0 maps to +Z, east at 90 maps to +X.
                Vector3 localPos = new Vector3(
                    Mathf.Sin(angleRad) * 8f,
                    6f,
                    Mathf.Cos(angleRad) * 8f);

                Vector3 toCenter = (Vector3.zero - localPos);
                toCenter.y = 0f;
                Quaternion faceCenter = toCenter.sqrMagnitude > 0.0001f
                    ? Quaternion.LookRotation(toCenter.normalized, Vector3.up)
                    : Quaternion.identity;
                // Vary slot rotation by 45 deg per slot on top of facing.
                Quaternion segmentRot = faceCenter * Quaternion.Euler(0f, angleDeg, 0f);

                string segPath = KitRoot + DomeSegmentNames[i] + ".prefab";
                if (PlaceLocal(segPath, parent.transform, localPos, segmentRot, missing))
                    placed++;
            }

            // ---- South entrance: door + outer archway ---------------------
            attempted++;
            if (PlaceLocal(PathDoor, parent.transform, new Vector3(0f, 0f, -8f), Quaternion.identity, missing))
                placed++;

            attempted++;
            if (PlaceLocal(PathArchway, parent.transform, new Vector3(0f, 0f, -10f), Quaternion.identity, missing))
                placed++;

            // ---- 4 corner walls -------------------------------------------
            Vector3[] cornerPositions =
            {
                new Vector3( 8f, 0f,  8f),
                new Vector3(-8f, 0f,  8f),
                new Vector3( 8f, 0f, -8f),
                new Vector3(-8f, 0f, -8f),
            };
            float[] cornerYaw = { 0f, 90f, 270f, 180f };
            for (int i = 0; i < cornerPositions.Length; i++)
            {
                attempted++;
                if (PlaceLocal(PathWallCorner, parent.transform, cornerPositions[i],
                        Quaternion.Euler(0f, cornerYaw[i], 0f), missing))
                    placed++;
            }

            // ---- 4 wall segments on the cardinal mid-edges ----------------
            // (0, 0, +8) N edge, (0, 0, -8) S edge, (+8, 0, 0) E edge, (-8, 0, 0) W edge
            Vector3[] wallPositions =
            {
                new Vector3(0f, 0f,  8f),
                new Vector3(0f, 0f, -8f),
                new Vector3( 8f, 0f, 0f),
                new Vector3(-8f, 0f, 0f),
            };
            float[] wallYaw = { 0f, 180f, 90f, 270f };
            for (int i = 0; i < wallPositions.Length; i++)
            {
                attempted++;
                if (PlaceLocal(PathWallStone, parent.transform, wallPositions[i],
                        Quaternion.Euler(0f, wallYaw[i], 0f), missing))
                    placed++;
            }

            // ---- Center column --------------------------------------------
            attempted++;
            if (PlaceLocal(PathColumn, parent.transform, new Vector3(0f, 0f, 0f), Quaternion.identity, missing))
                placed++;

            // ---- Rose window (south-facing, raised) -----------------------
            attempted++;
            if (PlaceLocal(PathRoseWindow, parent.transform, new Vector3(0f, 5f, 0f),
                    Quaternion.Euler(0f, 180f, 0f), missing))
                placed++;

            // ---- Spire stack (offset to NE quadrant) ----------------------
            attempted++;
            if (PlaceLocal(PathSpireBase, parent.transform, new Vector3(8f, 5f, 5f), Quaternion.identity, missing))
                placed++;

            attempted++;
            if (PlaceLocal(PathSpireMid, parent.transform, new Vector3(8f, 10f, 5f), Quaternion.identity, missing))
                placed++;

            attempted++;
            if (PlaceLocal(PathSpireTop, parent.transform, new Vector3(8f, 15f, 5f), Quaternion.identity, missing))
                placed++;

            // ---- Pipe organ inside the dome -------------------------------
            attempted++;
            if (PlaceLocal(PathPipeOrgan, parent.transform, new Vector3(0f, 0.5f, 4f),
                    Quaternion.Euler(0f, 180f, 0f), missing))
                placed++;

            // ---- Finalize --------------------------------------------------
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = parent;

            int missingCount = missing.Count;
            string missingList = missingCount == 0
                ? "(none)"
                : string.Join("\n  - ", missing);

            string summary =
                $"Cathedral Kit Dressing complete.\n\n" +
                $"Anchor: {(anchor != null ? AnchorName : "DEFAULT (0,0,100)")}\n" +
                $"Parent: {ParentName}\n" +
                $"Attempted: {attempted}\n" +
                $"Placed:    {placed}\n" +
                $"Missing:   {missingCount}\n\n" +
                $"Missing prefabs:\n  - {missingList}";

            Debug.Log("[Moon1CathedralKitDressing] " + summary);
            EditorUtility.DisplayDialog("Cathedral Kit Dressing", summary, "OK");
        }

        /// <summary>
        /// Loads a prefab from path and instantiates it as a child of <paramref name="parent"/>
        /// at the given LOCAL position + rotation, preserving the prefab link.
        /// Returns true on success; on failure pushes the missing path into <paramref name="missing"/>.
        /// </summary>
        private static bool PlaceLocal(
            string prefabPath,
            Transform parent,
            Vector3 localPos,
            Quaternion localRot,
            List<string> missing)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[Moon1CathedralKitDressing] Missing prefab: {prefabPath}");
                missing.Add(prefabPath);
                return false;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            if (instance == null)
            {
                Debug.LogWarning($"[Moon1CathedralKitDressing] InstantiatePrefab returned null for: {prefabPath}");
                missing.Add(prefabPath);
                return false;
            }

            instance.transform.localPosition = localPos;
            instance.transform.localRotation = localRot;
            instance.transform.localScale = Vector3.one;
            return true;
        }
    }
}
#endif
