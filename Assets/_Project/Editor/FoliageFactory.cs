using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Scatters foliage and rocks across the GroundPlane terrain. Uses two simple
    /// procedural meshes (a cross-quad grass tuft and a low-poly rock) and combines
    /// them into static-batched parent objects so it doesn't tank performance.
    /// </summary>
    public static class FoliageFactory
    {
        const string GrassMeshPath = "Assets/_Project/Models/Generated/GrassTuft.asset";
        const string RockMeshPath  = "Assets/_Project/Models/Generated/Rock.asset";
        const string GrassMatPath  = "Assets/_Project/Materials/M_Grass.mat";
        const string RockMatPath   = "Assets/_Project/Materials/M_Rock.mat";

        const float TerrainSize = 200f; // matches VisualUpgradeBuilder
        const int   GrassCount  = 1500;
        const int   RockCount   = 80;

        public static void BuildAndScatter()
        {
            EnsureMeshes();
            EnsureMaterials();
            ScatterIntoScene();
        }

        // ── Meshes ────────────────────────────────────────────────────────────
        static void EnsureMeshes()
        {
            if (AssetDatabase.LoadAssetAtPath<Mesh>(GrassMeshPath) == null)
                AssetDatabase.CreateAsset(BuildGrassTuftMesh(), GrassMeshPath);
            if (AssetDatabase.LoadAssetAtPath<Mesh>(RockMeshPath) == null)
                AssetDatabase.CreateAsset(BuildRockMesh(), RockMeshPath);
            AssetDatabase.SaveAssets();
        }

        static Mesh BuildGrassTuftMesh()
        {
            // Cross of 3 quads for a billboarded "tuft" feel
            var verts = new System.Collections.Generic.List<Vector3>();
            var tris = new System.Collections.Generic.List<int>();
            var uvs = new System.Collections.Generic.List<Vector2>();
            float w = 0.4f, h = 0.6f;
            for (int i = 0; i < 3; i++)
            {
                float ang = i * Mathf.PI / 3f;
                Vector3 right = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * w * 0.5f;
                int b = verts.Count;
                verts.Add(-right);                      // bottom-left
                verts.Add(right);                       // bottom-right
                verts.Add(right + Vector3.up * h);      // top-right
                verts.Add(-right + Vector3.up * h);     // top-left
                uvs.Add(new Vector2(0,0)); uvs.Add(new Vector2(1,0));
                uvs.Add(new Vector2(1,1)); uvs.Add(new Vector2(0,1));
                tris.Add(b); tris.Add(b+2); tris.Add(b+1);
                tris.Add(b); tris.Add(b+3); tris.Add(b+2);
                // back-faces
                tris.Add(b); tris.Add(b+1); tris.Add(b+2);
                tris.Add(b); tris.Add(b+2); tris.Add(b+3);
            }
            var mesh = new Mesh { name = "GrassTuft" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static Mesh BuildRockMesh()
        {
            // Icosphere-ish: start with octahedron and randomly perturb verts
            var verts = new Vector3[] {
                Vector3.up, Vector3.down,
                Vector3.left, Vector3.right,
                Vector3.forward, Vector3.back
            };
            var tris = new int[] {
                0,2,4, 0,4,3, 0,3,5, 0,5,2,
                1,4,2, 1,3,4, 1,5,3, 1,2,5
            };
            var rng = new System.Random(424242);
            for (int i = 0; i < verts.Length; i++)
            {
                float jitter = 0.7f + (float)rng.NextDouble() * 0.6f;
                verts[i] *= jitter;
                verts[i].y *= 0.6f; // squash so rocks sit flatter
            }
            var mesh = new Mesh { name = "Rock" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Materials ─────────────────────────────────────────────────────────
        static void EnsureMaterials()
        {
            EnsureLitMaterial(GrassMatPath, new Color(0.35f, 0.55f, 0.25f), emission: new Color(0f,0.05f,0f));
            EnsureLitMaterial(RockMatPath,  new Color(0.45f, 0.42f, 0.38f), emission: Color.black);
        }

        static void EnsureLitMaterial(string path, Color baseColor, Color emission)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);
            if (emission != Color.black)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
            }
            EditorUtility.SetDirty(mat);
        }

        // ── Scatter ──────────────────────────────────────────────────────────
        static void ScatterIntoScene()
        {
            var grassMesh = AssetDatabase.LoadAssetAtPath<Mesh>(GrassMeshPath);
            var rockMesh  = AssetDatabase.LoadAssetAtPath<Mesh>(RockMeshPath);
            var grassMat  = AssetDatabase.LoadAssetAtPath<Material>(GrassMatPath);
            var rockMat   = AssetDatabase.LoadAssetAtPath<Material>(RockMatPath);

            var prevGrass = GameObject.Find("FoliageRoot");
            if (prevGrass != null) Object.DestroyImmediate(prevGrass);

            var root = new GameObject("FoliageRoot");
            root.isStatic = true;

            var grassParent = new GameObject("Grass");
            grassParent.transform.SetParent(root.transform, false);
            grassParent.isStatic = true;

            var rockParent = new GameObject("Rocks");
            rockParent.transform.SetParent(root.transform, false);
            rockParent.isStatic = true;

            // Find ground height by raycasting downward from above
            var rng = new System.Random(0xC0FFEE);
            float half = TerrainSize * 0.5f - 4f;

            for (int i = 0; i < GrassCount; i++)
            {
                float x = ((float)rng.NextDouble() * 2f - 1f) * half;
                float z = ((float)rng.NextDouble() * 2f - 1f) * half;
                float y = SampleGroundY(x, z);
                if (float.IsNaN(y)) continue;
                var go = new GameObject("Grass") { isStatic = true };
                go.transform.SetParent(grassParent.transform, false);
                go.transform.position = new Vector3(x, y, z);
                go.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                float scale = 0.7f + (float)rng.NextDouble() * 0.6f;
                go.transform.localScale = new Vector3(scale, scale, scale);
                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = grassMesh;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = grassMat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            for (int i = 0; i < RockCount; i++)
            {
                float x = ((float)rng.NextDouble() * 2f - 1f) * half;
                float z = ((float)rng.NextDouble() * 2f - 1f) * half;
                float y = SampleGroundY(x, z);
                if (float.IsNaN(y)) continue;
                var go = new GameObject("Rock") { isStatic = true };
                go.transform.SetParent(rockParent.transform, false);
                go.transform.position = new Vector3(x, y, z);
                go.transform.rotation = Quaternion.Euler(
                    (float)rng.NextDouble()*30f, (float)rng.NextDouble()*360f, (float)rng.NextDouble()*30f);
                float scale = 0.5f + (float)rng.NextDouble() * 1.6f;
                go.transform.localScale = new Vector3(scale, scale*0.7f, scale);
                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = rockMesh;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = rockMat;
            }
            Debug.Log($"[Tartaria] Scattered foliage: {GrassCount} grass, {RockCount} rocks.");
        }

        static float SampleGroundY(float x, float z)
        {
            var origin = new Vector3(x, 200f, z);
            int mask = ~((1 << 8) | (1 << 10) | (1 << 11));
            if (Physics.Raycast(origin, Vector3.down, out var hit, 500f, mask, QueryTriggerInteraction.Ignore))
                return hit.point.y;
            return float.NaN;
        }
    }
}
