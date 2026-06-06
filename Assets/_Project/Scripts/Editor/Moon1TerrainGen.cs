#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/Build Out Moon 1 Terrain (500m radius + central depression)
    /// Per docs/15 §7: 500 m radius zone, central depression rising 30 m to south ridge.
    /// Generates a Terrain with heightmap that satisfies the spec — flat-ish low center
    /// for the buried-buildings plaza, gentle rise to perimeter, taller south ridge.
    /// </summary>
    public static class Moon1TerrainGen
    {
        const int RES = 513;           // heightmap resolution (513 = ~0.97m/pixel at 500m)
        const float SIZE_M = 500f;     // terrain side length
        const float MAX_HEIGHT = 35f;  // tallest peak (south ridge)
        const float DEPRESSION_DEPTH = 8f;

        [MenuItem("Tartaria/1 Build/Moon 1 — Terrain (500m + Depression)", priority = 160)]
        public static void Run()
        {
            var existing = GameObject.Find("Moon1_Terrain");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Rebuild terrain?",
                    "Moon1_Terrain exists. Replace?", "Replace", "Cancel")) return;
                Object.DestroyImmediate(existing);
            }

            var data = new TerrainData
            {
                heightmapResolution = RES,
                size = new Vector3(SIZE_M, MAX_HEIGHT, SIZE_M),
                alphamapResolution = 256,
                baseMapResolution = 512
            };
            float[,] heights = new float[RES, RES];
            for (int y = 0; y < RES; y++)
            for (int x = 0; x < RES; x++)
            {
                // Normalize -1..1 from center
                float u = (x / (RES - 1f)) * 2f - 1f;
                float v = (y / (RES - 1f)) * 2f - 1f;
                float dist = Mathf.Sqrt(u * u + v * v);

                // Base layer: gentle radial dome from center to edge
                float baseH = Mathf.SmoothStep(0.05f, 0.45f, dist);
                // Central depression: dig down within radius 0.15
                float depFactor = 1f - Mathf.SmoothStep(0f, 0.20f, dist);
                float depression = depFactor * (DEPRESSION_DEPTH / MAX_HEIGHT);
                // South ridge: rise where v > 0.6 (south = +Z)
                float ridge = Mathf.Max(0f, (v - 0.55f) / 0.45f);
                ridge = ridge * ridge * 0.55f;
                // Mild perlin noise so it's not perfectly smooth
                float n = Mathf.PerlinNoise(x * 0.025f, y * 0.025f) * 0.06f;
                float h = Mathf.Clamp01(baseH - depression + ridge + n);
                heights[y, x] = h;
            }
            data.SetHeights(0, 0, heights);

            string assetDir = "Assets/_Project/Terrain";
            System.IO.Directory.CreateDirectory(assetDir);
            string assetPath = assetDir + "/Moon1_Terrain.asset";
            AssetDatabase.CreateAsset(data, assetPath);

            var tGO = Terrain.CreateTerrainGameObject(data);
            tGO.name = "Moon1_Terrain";
            tGO.transform.position = new Vector3(-SIZE_M / 2f, -1f, -SIZE_M / 2f);
            Undo.RegisterCreatedObjectUndo(tGO, "Create Moon1_Terrain");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Terrain generated",
                $"Moon1_Terrain created at origin, {SIZE_M}×{SIZE_M}m, max {MAX_HEIGHT}m.\n" +
                "Central depression ~8m below plaza, south ridge rises ~19m.\n\n" +
                "Next: Tartaria → Build Out Moon 1 Splats to paint 4 PBR layers.", "OK");
        }
    }
}
#endif
