#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/Build Out Moon 1 Splats (Mud / Stone / Grass / Tartarian Tile)
    /// Per docs/15 §7: 4 PBR splat layers painted by elevation + radial distance.
    ///   Layer 0 — Mud (Phase-1 dominant) at low+wet plaza center
    ///   Layer 1 — Stone (mid elevation, dry uplifts)
    ///   Layer 2 — Grass (mid-high, north & east)
    ///   Layer 3 — Tartarian Tile (decorative ring 30-50m around hero buildings)
    /// Each layer is a flat PBR Material+Texture pair created procedurally.
    /// </summary>
    public static class Moon1TerrainSplats
    {
        [MenuItem("Tartaria/1 Build/Moon 1 — Splats (4 PBR layers)", priority = 170)]
        public static void Run()
        {
            var tGO = GameObject.Find("Moon1_Terrain");
            if (tGO == null)
            {
                EditorUtility.DisplayDialog("No terrain",
                    "Run 'Build Out Moon 1 Terrain' first.", "OK"); return;
            }
            var terrain = tGO.GetComponent<Terrain>();
            var data = terrain.terrainData;
            // Build 4 TerrainLayers with procedural textures
            var layers = new TerrainLayer[4];
            layers[0] = MakeLayer("Mud", new Color(0.30f, 0.20f, 0.12f), 0.90f);
            layers[1] = MakeLayer("Stone", new Color(0.55f, 0.50f, 0.45f), 0.75f);
            layers[2] = MakeLayer("Grass", new Color(0.30f, 0.50f, 0.22f), 0.80f);
            layers[3] = MakeLayer("TartarianTile", new Color(0.75f, 0.65f, 0.35f), 0.40f);
            data.terrainLayers = layers;

            int w = data.alphamapWidth;
            int h = data.alphamapHeight;
            var splat = new float[w, h, 4];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float u = (x / (float)(w - 1)) * 2f - 1f;
                float v = (y / (float)(h - 1)) * 2f - 1f;
                float dist = Mathf.Sqrt(u * u + v * v);
                float heightAt = data.GetHeight((int)(x * data.heightmapResolution / (float)w),
                                                (int)(y * data.heightmapResolution / (float)h));
                float normH = heightAt / data.size.y;

                // Mud — radius < 0.20 of map AND height-low
                float mud = (1f - Mathf.SmoothStep(0.10f, 0.30f, dist)) * (1f - normH * 1.6f);
                // Tartarian tile — ring radius 0.10..0.18 (decorative)
                float tile = Mathf.SmoothStep(0.05f, 0.10f, dist) - Mathf.SmoothStep(0.16f, 0.22f, dist);
                tile = Mathf.Max(0f, tile);
                // Grass — radius > 0.25 AND normalH < 0.4
                float grass = Mathf.SmoothStep(0.20f, 0.35f, dist) * Mathf.SmoothStep(0.45f, 0.20f, normH);
                // Stone — high elevation (south ridge) OR remainder
                float stone = Mathf.SmoothStep(0.35f, 0.60f, normH) + (v > 0.55f ? 0.40f : 0f);
                // Normalize
                float sum = mud + tile + grass + stone + 0.001f;
                splat[y, x, 0] = mud / sum;
                splat[y, x, 1] = stone / sum;
                splat[y, x, 2] = grass / sum;
                splat[y, x, 3] = tile / sum;
            }
            data.SetAlphamaps(0, 0, splat);

            EditorUtility.DisplayDialog("Splats painted",
                "4 layers painted: Mud (center) / Stone (ridge) / Grass (perimeter) / Tartarian Tile (ring 30-50m).\n\n" +
                "Materials in Assets/_Project/Materials/Terrain/", "OK");
        }

        static TerrainLayer MakeLayer(string name, Color baseColor, float smoothness)
        {
            string matDir = "Assets/_Project/Materials/Terrain";
            System.IO.Directory.CreateDirectory(matDir);
            // Tiny 4x4 procedural texture (noise variant of base color)
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, true);
            var px = new Color[16];
            for (int i = 0; i < 16; i++)
            {
                float n = (i * 37 % 13) / 13f * 0.15f - 0.075f;
                px[i] = new Color(
                    Mathf.Clamp01(baseColor.r + n),
                    Mathf.Clamp01(baseColor.g + n),
                    Mathf.Clamp01(baseColor.b + n),
                    1f);
            }
            tex.SetPixels(px); tex.Apply();
            string texPath = matDir + "/T_" + name + ".asset";
            AssetDatabase.CreateAsset(tex, texPath);

            var layer = new TerrainLayer { diffuseTexture = tex, tileSize = new Vector2(8f, 8f) };
            string layerPath = matDir + "/Layer_" + name + ".terrainlayer";
            AssetDatabase.CreateAsset(layer, layerPath);
            return layer;
        }
    }
}
#endif
