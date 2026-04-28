using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// Generates procedural meshes and styled materials using the custom Tartaria shaders,
    /// replacing all placeholder primitives in the Echohaven scene.
    ///
    /// Menu: Tartaria > Build Assets > Visual Upgrade
    /// </summary>
    public static class VisualUpgradeBuilder
    {
        const string MeshPath = "Assets/_Project/Models/Generated";
        const string MatPath = "Assets/_Project/Materials";
        const string PrefabPath = "Assets/_Project/Prefabs/Buildings";

        [MenuItem("Tartaria/Build Assets/Visual Upgrade", false, 9)]
        public static void BuildVisualUpgrade()
        {
            BuildVisualAssets();
            ApplyVisualUpgrade();
            Debug.Log("[Tartaria] Visual Upgrade complete — procedural meshes, shader materials, skybox applied.");
        }

        /// <summary>
        /// Phase 2b: Generate procedural meshes, shader materials, and skybox.
        /// Safe to run before scene geometry exists.
        /// </summary>
        public static void BuildVisualAssets()
        {
            EnsureDirs();
            BuildMeshAssets();
            BuildShaderMaterials();
            BuildSkyboxMaterial();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Phase 7b: Apply generated meshes and materials to scene objects and prefabs.
        /// Must run AFTER EchohavenScenePopulator has populated the scene.
        /// </summary>
        public static void ApplyVisualUpgrade()
        {
            UpgradeScene();
            UpgradePrefabs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ═══════════════════════════════════════════════
        //  DIRECTORIES
        // ═══════════════════════════════════════════════

        static void EnsureDirs()
        {
            string[] dirs = { MeshPath, MatPath, PrefabPath, "Assets/_Project/Models" };
            foreach (var d in dirs)
            {
                string full = Path.Combine(Application.dataPath, "..", d);
                if (!Directory.Exists(full)) Directory.CreateDirectory(full);
            }
            AssetDatabase.Refresh();
        }

        // ═══════════════════════════════════════════════
        //  PROCEDURAL MESH GENERATION
        // ═══════════════════════════════════════════════

        static void BuildMeshAssets()
        {
            SaveMeshAsset(GenerateDomeMesh(32, 16), "Dome");
            SaveMeshAsset(GenerateSpireMesh(16, 24), "Spire");
            SaveMeshAsset(GenerateFountainMesh(24), "Fountain");
            SaveMeshAsset(GeneratePillarMesh(8, 12), "RuinedPillar");
            SaveMeshAsset(GenerateTerrainMesh(64, 200f), "Terrain");
            SaveMeshAsset(GenerateArchMesh(12), "Arch");
            SaveMeshAsset(GenerateStepPyramidMesh(5), "StepPyramid");
            Debug.Log("[Tartaria] 7 procedural meshes generated.");
        }

        static void SaveMeshAsset(Mesh mesh, string name)
        {
            mesh.name = name;
            string path = $"{MeshPath}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                existing.Clear();
                EditorUtility.CopySerialized(mesh, existing);
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, path);
            }
        }

        // ── Dome: hemisphere with ornamental ring at base ──

        static Mesh GenerateDomeMesh(int lonSegments, int latSegments)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            // Hemisphere
            for (int lat = 0; lat <= latSegments; lat++)
            {
                float theta = Mathf.PI * 0.5f * lat / latSegments; // 0 to PI/2
                float sinT = Mathf.Sin(theta);
                float cosT = Mathf.Cos(theta);
                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float phi = 2f * Mathf.PI * lon / lonSegments;
                    float x = sinT * Mathf.Cos(phi);
                    float y = cosT;
                    float z = sinT * Mathf.Sin(phi);
                    verts.Add(new Vector3(x, y, z));
                    normals.Add(new Vector3(x, y, z).normalized);
                    uvs.Add(new Vector2((float)lon / lonSegments, 1f - (float)lat / latSegments));
                }
            }

            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int a = lat * (lonSegments + 1) + lon;
                    int b = a + lonSegments + 1;
                    tris.Add(a); tris.Add(b); tris.Add(a + 1);
                    tris.Add(a + 1); tris.Add(b); tris.Add(b + 1);
                }
            }

            // Base ring (torus-like rim)
            int ringBase = verts.Count;
            int ringSegs = lonSegments;
            int tubeSegs = 6;
            float ringR = 1.05f;
            float tubeR = 0.08f;
            for (int i = 0; i <= ringSegs; i++)
            {
                float phi = 2f * Mathf.PI * i / ringSegs;
                float cx = ringR * Mathf.Cos(phi);
                float cz = ringR * Mathf.Sin(phi);
                for (int j = 0; j <= tubeSegs; j++)
                {
                    float psi = 2f * Mathf.PI * j / tubeSegs;
                    float rx = (ringR + tubeR * Mathf.Cos(psi)) * Mathf.Cos(phi);
                    float ry = tubeR * Mathf.Sin(psi);
                    float rz = (ringR + tubeR * Mathf.Cos(psi)) * Mathf.Sin(phi);
                    verts.Add(new Vector3(rx, ry, rz));
                    Vector3 n = new Vector3(rx - cx, ry, rz - cz).normalized;
                    normals.Add(n);
                    uvs.Add(new Vector2((float)i / ringSegs, (float)j / tubeSegs));
                }
            }
            for (int i = 0; i < ringSegs; i++)
            {
                for (int j = 0; j < tubeSegs; j++)
                {
                    int a = ringBase + i * (tubeSegs + 1) + j;
                    int b = a + tubeSegs + 1;
                    tris.Add(a); tris.Add(b); tris.Add(a + 1);
                    tris.Add(a + 1); tris.Add(b); tris.Add(b + 1);
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Spire: tapered cylinder with ornamental ridges ──

        static Mesh GenerateSpireMesh(int radialSegs, int heightSegs)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            float height = 1f;
            for (int y = 0; y <= heightSegs; y++)
            {
                float t = (float)y / heightSegs;
                float h = t * height;
                // Taper from base radius 1 to tip 0.15 with slight bulge
                float r = Mathf.Lerp(1f, 0.15f, t) + 0.08f * Mathf.Sin(t * Mathf.PI * 3f);
                // Ornamental ridges
                for (int i = 0; i <= radialSegs; i++)
                {
                    float angle = 2f * Mathf.PI * i / radialSegs;
                    float ridge = 1f + 0.05f * Mathf.Sin(i * 4f * Mathf.PI / radialSegs) *
                                  Mathf.Sin(t * Mathf.PI * 6f);
                    float x = Mathf.Cos(angle) * r * ridge;
                    float z = Mathf.Sin(angle) * r * ridge;
                    verts.Add(new Vector3(x, h, z));
                    normals.Add(new Vector3(Mathf.Cos(angle), 0.3f, Mathf.Sin(angle)).normalized);
                    uvs.Add(new Vector2((float)i / radialSegs, t));
                }
            }

            for (int y = 0; y < heightSegs; y++)
            {
                for (int i = 0; i < radialSegs; i++)
                {
                    int a = y * (radialSegs + 1) + i;
                    int b = a + radialSegs + 1;
                    tris.Add(a); tris.Add(b); tris.Add(a + 1);
                    tris.Add(a + 1); tris.Add(b); tris.Add(b + 1);
                }
            }

            // Cap at top
            int capCenter = verts.Count;
            verts.Add(new Vector3(0f, height, 0f));
            normals.Add(Vector3.up);
            uvs.Add(new Vector2(0.5f, 1f));
            int topRow = heightSegs * (radialSegs + 1);
            for (int i = 0; i < radialSegs; i++)
            {
                tris.Add(topRow + i);
                tris.Add(capCenter);
                tris.Add(topRow + i + 1);
            }

            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Fountain: basin + central column + water surface ──

        static Mesh GenerateFountainMesh(int segs)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            // Basin outer wall (cylinder)
            AddCylinderSection(verts, tris, uvs, normals, segs,
                1f, 0.95f, 0f, 0.4f, 0);
            // Basin inner wall
            int offset = verts.Count;
            AddCylinderSection(verts, tris, uvs, normals, segs,
                0.85f, 0.9f, 0.05f, 0.38f, offset);
            // Basin lip (torus rim)
            offset = verts.Count;
            AddTorusRing(verts, tris, uvs, normals, segs, 4,
                0.95f, 0.05f, 0.4f, offset);
            // Central column
            offset = verts.Count;
            AddCylinderSection(verts, tris, uvs, normals, segs,
                0.15f, 0.12f, 0f, 0.8f, offset);
            // Column top ornament (sphere-ish)
            offset = verts.Count;
            AddHemisphere(verts, tris, uvs, normals, 12, 6,
                new Vector3(0f, 0.8f, 0f), 0.18f, offset);
            // Water surface disc
            offset = verts.Count;
            AddDisc(verts, tris, uvs, normals, segs,
                new Vector3(0f, 0.25f, 0f), 0.83f, offset);

            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Ruined Pillar: tapered column with broken top ──

        static Mesh GeneratePillarMesh(int segs, int heightSegs)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            for (int y = 0; y <= heightSegs; y++)
            {
                float t = (float)y / heightSegs;
                float h = t;
                // Slight taper and random broken top
                float r = Mathf.Lerp(1f, 0.85f, t);
                // Fluting (concave channels like Greek columns)
                for (int i = 0; i <= segs; i++)
                {
                    float angle = 2f * Mathf.PI * i / segs;
                    float flute = 1f - 0.06f * Mathf.Abs(Mathf.Sin(i * Mathf.PI));
                    // Broken/jagged top
                    float breakage = t > 0.85f
                        ? 1f - (t - 0.85f) * (0.5f + 0.5f * Mathf.Sin(i * 3.7f)) * 3f
                        : 1f;
                    if (breakage < 0f) breakage = 0f;
                    float rx = Mathf.Cos(angle) * r * flute;
                    float rz = Mathf.Sin(angle) * r * flute;
                    verts.Add(new Vector3(rx, h * breakage, rz));
                    normals.Add(new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized);
                    uvs.Add(new Vector2((float)i / segs, t));
                }
            }

            for (int y = 0; y < heightSegs; y++)
            {
                for (int i = 0; i < segs; i++)
                {
                    int a = y * (segs + 1) + i;
                    int b = a + segs + 1;
                    tris.Add(a); tris.Add(b); tris.Add(a + 1);
                    tris.Add(a + 1); tris.Add(b); tris.Add(b + 1);
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Terrain: undulating plane with paths ──

        static Mesh GenerateTerrainMesh(int res, float size)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            float half = size * 0.5f;
            for (int z = 0; z <= res; z++)
            {
                for (int x = 0; x <= res; x++)
                {
                    float px = -half + (float)x / res * size;
                    float pz = -half + (float)z / res * size;
                    // Rolling hills
                    float h = 0f;
                    h += Mathf.Sin(px * 0.03f) * Mathf.Cos(pz * 0.04f) * 2f;
                    h += Mathf.Sin(px * 0.07f + 1.3f) * Mathf.Sin(pz * 0.06f + 0.8f) * 0.8f;
                    h += Mathf.PerlinNoise(px * 0.02f + 100f, pz * 0.02f + 100f) * 3f;
                    // Flatten central area for plaza
                    float distCenter = Mathf.Sqrt(px * px + pz * pz);
                    float plazaBlend = Mathf.Clamp01((distCenter - 15f) / 10f);
                    h *= plazaBlend;
                    // Carve paths (radial from center)
                    float angle = Mathf.Atan2(pz, px);
                    float pathWave = Mathf.Abs(Mathf.Sin(angle * 2f));
                    if (pathWave < 0.15f && distCenter > 10f)
                        h -= 0.3f;
                    verts.Add(new Vector3(px, h, pz));
                    uvs.Add(new Vector2((float)x / res, (float)z / res));
                }
            }

            for (int z = 0; z < res; z++)
            {
                for (int x = 0; x < res; x++)
                {
                    int a = z * (res + 1) + x;
                    int b = a + res + 1;
                    // Clockwise winding viewed from above (+Y) so terrain is
                    // visible from above — Unity culls back faces by default.
                    tris.Add(a); tris.Add(a + 1); tris.Add(b);
                    tris.Add(a + 1); tris.Add(b + 1); tris.Add(b);
                }
            }

            // Compute normals from triangles
            var normArr = new Vector3[verts.Count];
            for (int i = 0; i < tris.Count; i += 3)
            {
                var e1 = verts[tris[i + 1]] - verts[tris[i]];
                var e2 = verts[tris[i + 2]] - verts[tris[i]];
                var n = Vector3.Cross(e1, e2).normalized;
                normArr[tris[i]] += n;
                normArr[tris[i + 1]] += n;
                normArr[tris[i + 2]] += n;
            }
            for (int i = 0; i < normArr.Length; i++)
                normals.Add(normArr[i].normalized);

            var mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Arch: semicircular arch gateway ──

        static Mesh GenerateArchMesh(int segs)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            float width = 1f;
            float thickness = 0.15f;
            float depth = 0.3f;

            // Left pillar
            AddBox(verts, tris, uvs, normals,
                new Vector3(-width * 0.5f, 0.5f, 0f),
                new Vector3(thickness, 1f, depth), verts.Count);
            // Right pillar
            AddBox(verts, tris, uvs, normals,
                new Vector3(width * 0.5f, 0.5f, 0f),
                new Vector3(thickness, 1f, depth), verts.Count);
            // Arch curve
            int archBase = verts.Count;
            float outerR = width * 0.5f + thickness * 0.5f;
            float innerR = width * 0.5f - thickness * 0.5f;
            for (int i = 0; i <= segs; i++)
            {
                float t = (float)i / segs;
                float angle = Mathf.PI * t;
                float cosA = Mathf.Cos(angle);
                float sinA = Mathf.Sin(angle);
                // Front face
                verts.Add(new Vector3(cosA * outerR, 1f + sinA * outerR, depth * 0.5f));
                verts.Add(new Vector3(cosA * innerR, 1f + sinA * innerR, depth * 0.5f));
                // Back face
                verts.Add(new Vector3(cosA * outerR, 1f + sinA * outerR, -depth * 0.5f));
                verts.Add(new Vector3(cosA * innerR, 1f + sinA * innerR, -depth * 0.5f));
                Vector3 n = new Vector3(cosA, sinA, 0f).normalized;
                normals.Add(n); normals.Add(n); normals.Add(n); normals.Add(n);
                uvs.Add(new Vector2(t, 1f)); uvs.Add(new Vector2(t, 0.8f));
                uvs.Add(new Vector2(t, 0.2f)); uvs.Add(new Vector2(t, 0f));
            }
            for (int i = 0; i < segs; i++)
            {
                int b = archBase + i * 4;
                // Front quad (outer-inner)
                tris.Add(b); tris.Add(b + 4); tris.Add(b + 1);
                tris.Add(b + 1); tris.Add(b + 4); tris.Add(b + 5);
                // Back quad
                tris.Add(b + 3); tris.Add(b + 7); tris.Add(b + 2);
                tris.Add(b + 2); tris.Add(b + 7); tris.Add(b + 6);
                // Top (outer)
                tris.Add(b); tris.Add(b + 2); tris.Add(b + 4);
                tris.Add(b + 4); tris.Add(b + 2); tris.Add(b + 6);
                // Bottom (inner)
                tris.Add(b + 1); tris.Add(b + 5); tris.Add(b + 3);
                tris.Add(b + 3); tris.Add(b + 5); tris.Add(b + 7);
            }

            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Step Pyramid: 5-tier ziggurat ──

        static Mesh GenerateStepPyramidMesh(int tiers)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            float tierHeight = 1f / tiers;
            for (int tier = 0; tier < tiers; tier++)
            {
                float y = tier * tierHeight;
                float scale = 1f - (float)tier / tiers * 0.7f;
                float halfW = scale * 0.5f;
                AddBox(verts, tris, uvs, normals,
                    new Vector3(0f, y + tierHeight * 0.5f, 0f),
                    new Vector3(halfW * 2f, tierHeight, halfW * 2f),
                    verts.Count);
            }

            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            return mesh;
        }

        // ═══════════════════════════════════════════════
        //  MESH HELPERS
        // ═══════════════════════════════════════════════

        static void AddCylinderSection(List<Vector3> v, List<int> t, List<Vector2> u, List<Vector3> n,
            int segs, float radiusBot, float radiusTop, float yBot, float yTop, int offset)
        {
            for (int i = 0; i <= segs; i++)
            {
                float angle = 2f * Mathf.PI * i / segs;
                float c = Mathf.Cos(angle), s = Mathf.Sin(angle);
                v.Add(new Vector3(c * radiusBot, yBot, s * radiusBot));
                v.Add(new Vector3(c * radiusTop, yTop, s * radiusTop));
                Vector3 norm = new Vector3(c, 0f, s).normalized;
                n.Add(norm); n.Add(norm);
                u.Add(new Vector2((float)i / segs, 0f));
                u.Add(new Vector2((float)i / segs, 1f));
            }
            for (int i = 0; i < segs; i++)
            {
                int a = offset + i * 2;
                t.Add(a); t.Add(a + 2); t.Add(a + 1);
                t.Add(a + 1); t.Add(a + 2); t.Add(a + 3);
            }
        }

        static void AddTorusRing(List<Vector3> v, List<int> t, List<Vector2> u, List<Vector3> n,
            int ringSegs, int tubeSegs, float ringR, float tubeR, float yOffset, int offset)
        {
            for (int i = 0; i <= ringSegs; i++)
            {
                float phi = 2f * Mathf.PI * i / ringSegs;
                float cx = ringR * Mathf.Cos(phi);
                float cz = ringR * Mathf.Sin(phi);
                for (int j = 0; j <= tubeSegs; j++)
                {
                    float psi = 2f * Mathf.PI * j / tubeSegs;
                    float rx = (ringR + tubeR * Mathf.Cos(psi)) * Mathf.Cos(phi);
                    float ry = yOffset + tubeR * Mathf.Sin(psi);
                    float rz = (ringR + tubeR * Mathf.Cos(psi)) * Mathf.Sin(phi);
                    v.Add(new Vector3(rx, ry, rz));
                    n.Add(new Vector3(rx - cx, ry - yOffset, rz - cz).normalized);
                    u.Add(new Vector2((float)i / ringSegs, (float)j / tubeSegs));
                }
            }
            for (int i = 0; i < ringSegs; i++)
            {
                for (int j = 0; j < tubeSegs; j++)
                {
                    int a = offset + i * (tubeSegs + 1) + j;
                    int b = a + tubeSegs + 1;
                    t.Add(a); t.Add(b); t.Add(a + 1);
                    t.Add(a + 1); t.Add(b); t.Add(b + 1);
                }
            }
        }

        static void AddHemisphere(List<Vector3> v, List<int> t, List<Vector2> u, List<Vector3> n,
            int lonSegs, int latSegs, Vector3 center, float radius, int offset)
        {
            for (int lat = 0; lat <= latSegs; lat++)
            {
                float theta = Mathf.PI * 0.5f * lat / latSegs;
                float sinT = Mathf.Sin(theta), cosT = Mathf.Cos(theta);
                for (int lon = 0; lon <= lonSegs; lon++)
                {
                    float phi = 2f * Mathf.PI * lon / lonSegs;
                    Vector3 p = new Vector3(sinT * Mathf.Cos(phi), cosT, sinT * Mathf.Sin(phi));
                    v.Add(center + p * radius);
                    n.Add(p.normalized);
                    u.Add(new Vector2((float)lon / lonSegs, 1f - (float)lat / latSegs));
                }
            }
            for (int lat = 0; lat < latSegs; lat++)
            {
                for (int lon = 0; lon < lonSegs; lon++)
                {
                    int a = offset + lat * (lonSegs + 1) + lon;
                    int b = a + lonSegs + 1;
                    t.Add(a); t.Add(b); t.Add(a + 1);
                    t.Add(a + 1); t.Add(b); t.Add(b + 1);
                }
            }
        }

        static void AddDisc(List<Vector3> v, List<int> t, List<Vector2> u, List<Vector3> n,
            int segs, Vector3 center, float radius, int offset)
        {
            int ci = v.Count;
            v.Add(center); n.Add(Vector3.up); u.Add(new Vector2(0.5f, 0.5f));
            for (int i = 0; i <= segs; i++)
            {
                float angle = 2f * Mathf.PI * i / segs;
                v.Add(center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
                n.Add(Vector3.up);
                u.Add(new Vector2(0.5f + 0.5f * Mathf.Cos(angle), 0.5f + 0.5f * Mathf.Sin(angle)));
            }
            for (int i = 0; i < segs; i++)
            {
                t.Add(ci); t.Add(ci + 1 + i); t.Add(ci + 2 + i);
            }
        }

        static void AddBox(List<Vector3> v, List<int> t, List<Vector2> u, List<Vector3> n,
            Vector3 center, Vector3 size, int offset)
        {
            float hx = size.x * 0.5f, hy = size.y * 0.5f, hz = size.z * 0.5f;
            Vector3[] corners = {
                center + new Vector3(-hx, -hy, -hz), center + new Vector3( hx, -hy, -hz),
                center + new Vector3( hx,  hy, -hz), center + new Vector3(-hx,  hy, -hz),
                center + new Vector3(-hx, -hy,  hz), center + new Vector3( hx, -hy,  hz),
                center + new Vector3( hx,  hy,  hz), center + new Vector3(-hx,  hy,  hz),
            };
            int[][] faces = {
                new[]{0,1,2,3}, new[]{5,4,7,6}, // front, back
                new[]{3,2,6,7}, new[]{4,5,1,0}, // top, bottom
                new[]{4,0,3,7}, new[]{1,5,6,2}, // left, right
            };
            Vector3[] faceNormals = {
                Vector3.back, Vector3.forward, Vector3.up, Vector3.down, Vector3.left, Vector3.right
            };
            for (int f = 0; f < 6; f++)
            {
                int b = v.Count;
                for (int i = 0; i < 4; i++)
                {
                    v.Add(corners[faces[f][i]]);
                    n.Add(faceNormals[f]);
                }
                u.Add(new Vector2(0,0)); u.Add(new Vector2(1,0));
                u.Add(new Vector2(1,1)); u.Add(new Vector2(0,1));
                t.Add(b); t.Add(b+2); t.Add(b+1);
                t.Add(b); t.Add(b+3); t.Add(b+2);
            }
        }

        // ═══════════════════════════════════════════════
        //  SHADER MATERIALS
        // ═══════════════════════════════════════════════

        static void BuildShaderMaterials()
        {
            // Generate procedural detail textures
            BuildProceduralTextures();

            // Stone variants — lighter, more varied palette
            CreateStone("M_Stone_Weathered", new Color(0.62f, 0.58f, 0.50f),
                0.45f, 0.3f, 0f, 0f, "Tex_StoneNoise");
            CreateStone("M_Stone_Active", new Color(0.82f, 0.78f, 0.70f),
                0.3f, 0.4f, 0.3f, 0.4f, "Tex_StoneNoise");
            CreateStone("M_Stone_Golden", new Color(0.88f, 0.80f, 0.50f),
                0.15f, 0.5f, 0.8f, 0.8f, "Tex_StoneNoise");
            CreateStone("M_Stone_Plaza", new Color(0.75f, 0.72f, 0.65f),
                0.5f, 0.3f, 0.1f, 0.05f, "Tex_StoneTile");

            // Mud variants — with organic noise
            CreateMud("M_Mud_Fresh", new Color(0.30f, 0.20f, 0.12f), 0f);
            CreateMud("M_Mud_Cracking", new Color(0.38f, 0.28f, 0.16f), 0.4f);
            CreateMud("M_Mud_Dissolving", new Color(0.45f, 0.35f, 0.22f), 0.7f);

            // Aether — bright blue/cyan energy (transparent + emissive)
            CreateAether("M_Aether_Flow", new Color(0.1f, 0.4f, 0.95f, 0.4f), 2f);
            CreateAether("M_Aether_Bright", new Color(0.2f, 0.75f, 1f, 0.6f), 4f);
            CreateAether("M_Aether_Water", new Color(0.05f, 0.35f, 0.7f, 0.5f), 1.2f);

            // Corruption (transparent dark purple)
            CreateCorruption("M_Corruption", new Color(0.25f, 0.05f, 0.3f, 0.7f), 1.5f);

            // Ghost (transparent pale blue)
            CreateGhost("M_Ghost_Anastasia", new Color(0.8f, 0.88f, 1f, 0.4f), 0.5f);

            // Ground — green-brown terrain with spatial color map
            CreateStone("M_Ground_Terrain", new Color(0.85f, 0.85f, 0.85f),
                0.65f, 0.1f, 0f, 0f, "Tex_GrassNoise", 1f);

            // Gold accents — bright warm gold
            CreateStone("M_Gold_Ornament", new Color(0.95f, 0.82f, 0.30f),
                0f, 0.85f, 1f, 1.5f);

            // Crystal — ice blue (transparent)
            CreateAether("M_Crystal", new Color(0.6f, 0.82f, 1f, 0.65f), 2.5f);

            // Character-specific materials — more contrast, distinct colors
            CreateStone("Player_Aether", new Color(0.25f, 0.35f, 0.55f),
                0f, 0.65f, 0.15f, 0.6f);  // deeper blue, subtle glow
            CreateStone("Player_Head", new Color(0.70f, 0.75f, 0.88f),
                0f, 0.7f, 0.1f, 0.8f);    // much lighter, stands out
            CreateStone("Player_Limbs", new Color(0.18f, 0.25f, 0.42f),
                0f, 0.5f, 0.1f, 0.3f);    // dark, high contrast with body
            CreateStone("Aether_Glow", new Color(0.1f, 0.55f, 1f),
                0f, 0.3f, 0f, 3f);        // intense blue emission
            CreateStone("Crystal_Active", new Color(0.7f, 0.9f, 1f),
                0f, 0.8f, 0f, 2.5f);      // bright white-blue, like eyes
            CreateStone("MudGolem_Body", new Color(0.35f, 0.25f, 0.15f),
                0f, 0.15f, 0f, 0f, "Tex_MudNoise");

            Debug.Log("[Tartaria] 22 URP/Lit materials created/refreshed.");
        }

        // ─── Procedural Texture Generation ───

        static void BuildProceduralTextures()
        {
            GenerateNoiseTexture("Tex_StoneNoise", 256, 0.85f, 1.15f, Color.white, 8f);
            GenerateNoiseTexture("Tex_MudNoise", 256, 0.7f, 1.0f,
                new Color(0.9f, 0.85f, 0.8f), 5f);
            GenerateTerrainColorMap("Tex_GrassNoise", 512, 200f);
            GenerateTileTexture("Tex_StoneTile", 256, 8, new Color(0.9f, 0.88f, 0.85f),
                new Color(0.6f, 0.58f, 0.55f));
        }

        static void GenerateNoiseTexture(string name, int size, float minBright,
            float maxBright, Color tint, float noiseScale)
        {
            string path = $"{MatPath}/{name}.png";
            if (File.Exists(Path.Combine(Application.dataPath, "..", path))) return;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x / noiseScale + 0.5f, y / noiseScale + 0.5f);
                    // Add second octave for detail
                    n += Mathf.PerlinNoise(x / (noiseScale * 0.5f) + 100f,
                        y / (noiseScale * 0.5f) + 100f) * 0.5f;
                    n /= 1.5f;
                    float brightness = Mathf.Lerp(minBright, maxBright, n);
                    tex.SetPixel(x, y, tint * brightness);
                }
            }
            tex.Apply();
            File.WriteAllBytes(Path.Combine(Application.dataPath, "..", path), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            // Set texture import settings
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
        }

        static void GenerateTerrainColorMap(string name, int size, float terrainSize)
        {
            string path = $"{MatPath}/{name}.png";
            if (File.Exists(Path.Combine(Application.dataPath, "..", path))) return;

            Color grassDark = new Color(0.22f, 0.38f, 0.15f);
            Color grassLight = new Color(0.35f, 0.52f, 0.25f);
            Color dirtBrown = new Color(0.42f, 0.32f, 0.20f);
            Color pathTan = new Color(0.55f, 0.48f, 0.38f);
            Color plazaGrey = new Color(0.58f, 0.56f, 0.52f);

            float half = terrainSize * 0.5f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Map pixel to world position
                    float wx = -half + (float)x / size * terrainSize;
                    float wz = -half + (float)y / size * terrainSize;
                    float dist = Mathf.Sqrt(wx * wx + wz * wz);

                    // Determine path
                    float angle = Mathf.Atan2(wz, wx);
                    float pathWave = Mathf.Abs(Mathf.Sin(angle * 2f));
                    bool isPath = (pathWave < 0.15f && dist > 10f);

                    Color c;
                    if (dist < 12f)
                    {
                        // Plaza center — stone
                        c = plazaGrey;
                    }
                    else if (dist < 20f)
                    {
                        // Plaza edge — blend to grass
                        float t = (dist - 12f) / 8f;
                        c = Color.Lerp(plazaGrey, grassLight, t);
                    }
                    else if (isPath)
                    {
                        c = pathTan;
                    }
                    else
                    {
                        // Grass with noise variation
                        float n1 = Mathf.PerlinNoise(wx * 0.05f + 50f, wz * 0.05f + 50f);
                        float n2 = Mathf.PerlinNoise(wx * 0.12f + 80f, wz * 0.12f + 80f);
                        c = Color.Lerp(grassDark, grassLight, n1);
                        c = Color.Lerp(c, dirtBrown, n2 * 0.35f);
                    }
                    // Add subtle per-pixel noise for variation
                    float micro = Mathf.PerlinNoise(x * 0.3f + 200f, y * 0.3f + 200f);
                    c *= Mathf.Lerp(0.9f, 1.1f, micro);
                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            File.WriteAllBytes(Path.Combine(Application.dataPath, "..", path), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
        }

        static void GenerateTileTexture(string name, int size, int tiles,
            Color fillColor, Color lineColor)
        {
            string path = $"{MatPath}/{name}.png";
            if (File.Exists(Path.Combine(Application.dataPath, "..", path))) return;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            int tileSize = size / tiles;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isEdge = (x % tileSize < 2) || (y % tileSize < 2);
                    float noise = Mathf.PerlinNoise(x / 4f + 50f, y / 4f + 50f);
                    Color c = isEdge ? lineColor : Color.Lerp(fillColor,
                        fillColor * 0.85f, noise);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            File.WriteAllBytes(Path.Combine(Application.dataPath, "..", path), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
        }

        static void CreateStone(string name, Color baseColor,
            float weathering, float smoothness, float goldenStr, float emissionStr,
            string textureName = null, float textureScale = 4f)
        {
            var mat = SetupURPMaterial(name);
            if (mat == null) return;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", goldenStr > 0.5f ? 0.6f : 0.1f);
            if (emissionStr > 0f)
            {
                Color emColor = Color.Lerp(new Color(0.9f, 0.75f, 0.3f), baseColor, 0.3f) * emissionStr;
                mat.SetColor("_EmissionColor", emColor);
                mat.EnableKeyword("_EMISSION");
            }
            // Apply procedural detail texture if specified
            if (!string.IsNullOrEmpty(textureName))
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{MatPath}/{textureName}.png");
                if (tex != null)
                {
                    mat.SetTexture("_BaseMap", tex);
                    mat.SetTextureScale("_BaseMap", new Vector2(textureScale, textureScale));
                }
            }
            EditorUtility.SetDirty(mat);
        }

        static void CreateMud(string name, Color baseColor, float dissolution)
        {
            var mat = SetupURPMaterial(name);
            if (mat == null) return;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", 0.1f);
            mat.SetFloat("_Metallic", 0f);
            // Apply mud noise texture for organic look
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{MatPath}/Tex_MudNoise.png");
            if (tex != null)
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTextureScale("_BaseMap", new Vector2(3f, 3f));
            }
            // Cracking/dissolving mud gets subtle emission to show energy underneath
            if (dissolution > 0.2f)
            {
                Color emColor = new Color(0.6f, 0.4f, 0.1f) * dissolution * 0.5f;
                mat.SetColor("_EmissionColor", emColor);
                mat.EnableKeyword("_EMISSION");
            }
            EditorUtility.SetDirty(mat);
        }

        static void CreateAether(string name, Color baseColor, float intensity)
        {
            var mat = SetupURPMaterial(name);
            if (mat == null) return;
            mat.SetColor("_BaseColor", baseColor);
            SetTransparent(mat);
            Color emColor = new Color(baseColor.r, baseColor.g, baseColor.b) * intensity;
            mat.SetColor("_EmissionColor", emColor);
            mat.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(mat);
        }

        static void CreateCorruption(string name, Color baseColor, float intensity)
        {
            var mat = SetupURPMaterial(name);
            if (mat == null) return;
            mat.SetColor("_BaseColor", baseColor);
            SetTransparent(mat);
            Color emColor = new Color(0.5f, 0.1f, 0.6f) * intensity;
            mat.SetColor("_EmissionColor", emColor);
            mat.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(mat);
        }

        static void CreateGhost(string name, Color baseColor, float opacity)
        {
            var mat = SetupURPMaterial(name);
            if (mat == null) return;
            mat.SetColor("_BaseColor", baseColor);
            SetTransparent(mat);
            Color emColor = new Color(0.3f, 0.4f, 0.6f) * 0.5f;
            mat.SetColor("_EmissionColor", emColor);
            mat.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(mat);
        }

        // ═══════════════════════════════════════════════
        //  SKYBOX
        // ═══════════════════════════════════════════════

        static void BuildSkyboxMaterial()
        {
            string path = $"{MatPath}/M_Skybox_Tartaria.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            // Use built-in Skybox/Procedural for reliability
            var shader = Shader.Find("Skybox/Procedural");
            if (shader == null) { Debug.LogWarning("Skybox/Procedural shader not found"); return; }

            if (mat == null)
            {
                mat = new Material(shader);
                mat.name = "M_Skybox_Tartaria";
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }
            mat.SetColor("_SkyTint", new Color(0.35f, 0.45f, 0.65f));
            mat.SetColor("_GroundColor", new Color(0.22f, 0.20f, 0.18f));
            mat.SetFloat("_Exposure", 1.2f);
            mat.SetFloat("_AtmosphereThickness", 1.0f);
            mat.SetFloat("_SunDisk", 2f); // High quality
            mat.SetFloat("_SunSize", 0.04f);
            mat.SetFloat("_SunSizeConvergence", 5f);
            EditorUtility.SetDirty(mat);

            RenderSettings.skybox = mat;
            Debug.Log("[Tartaria] Skybox assigned: Skybox/Procedural.");
        }

        // ═══════════════════════════════════════════════
        //  SCENE UPGRADE — replace primitives with meshes
        // ═══════════════════════════════════════════════

        static void UpgradeScene()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid()) return;

            // Load meshes
            var domeMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshPath}/Dome.asset");
            var spireMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshPath}/Spire.asset");
            var fountainMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshPath}/Fountain.asset");
            var pillarMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshPath}/RuinedPillar.asset");
            var terrainMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshPath}/Terrain.asset");
            var archMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshPath}/Arch.asset");
            var pyramidMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshPath}/StepPyramid.asset");

            // Load materials
            var stoneActive = LoadMat("M_Stone_Active");
            var stoneWeathered = LoadMat("M_Stone_Weathered");
            var stoneGolden = LoadMat("M_Stone_Golden");
            var stonePlaza = LoadMat("M_Stone_Plaza");
            var mudFresh = LoadMat("M_Mud_Fresh");
            var mudCracking = LoadMat("M_Mud_Cracking");
            var aetherFlow = LoadMat("M_Aether_Flow");
            var aetherBright = LoadMat("M_Aether_Bright");
            var aetherWater = LoadMat("M_Aether_Water");
            var groundMat = LoadMat("M_Ground_Terrain");
            var goldMat = LoadMat("M_Gold_Ornament");
            var crystalMat = LoadMat("M_Crystal");
            var corruption = LoadMat("M_Corruption");

            // ── Replace GroundPlane with terrain mesh ──
            ReplaceMesh("GroundPlane", terrainMesh, groundMat, Vector3.zero,
                Vector3.one, Quaternion.identity);

            // ── Replace CentralPlaza ──
            ReplaceMesh("CentralPlaza", null, stonePlaza, null, null, null);
            var plaza = GameObject.Find("CentralPlaza");
            if (plaza != null)
            {
                // Keep cylinder but apply stone material
                var r = plaza.GetComponent<MeshRenderer>();
                if (r != null && stonePlaza != null) r.sharedMaterial = stonePlaza;
            }

            // ── Replace StarDome_Placeholder ──
            if (domeMesh != null)
            {
                ReplaceMesh("StarDome_Placeholder", domeMesh, stoneActive,
                    new Vector3(30f, 0f, 20f), new Vector3(12f, 8f, 12f), Quaternion.identity);
                // Add golden ring at base
                AddDecoration("StarDome_GoldRing", new Vector3(30f, 0.2f, 20f),
                    domeMesh, goldMat, new Vector3(12.5f, 0.5f, 12.5f));
                // Add aether glow inside
                AddDecoration("StarDome_AetherGlow", new Vector3(30f, 2f, 20f),
                    domeMesh, aetherFlow, new Vector3(10f, 6f, 10f));
            }

            // ── Replace HarmonicFountain_Placeholder ──
            if (fountainMesh != null)
            {
                ReplaceMesh("HarmonicFountain_Placeholder", fountainMesh, stoneWeathered,
                    new Vector3(-20f, 0f, 35f), new Vector3(6f, 3f, 6f), Quaternion.identity);
                // Water surface glow
                AddDecoration("Fountain_Water", new Vector3(-20f, 0.8f, 35f),
                    null, aetherWater, new Vector3(5f, 0.1f, 5f));
            }

            // ── Replace CrystalSpire_Placeholder ──
            if (spireMesh != null)
            {
                ReplaceMesh("CrystalSpire_Placeholder", spireMesh, crystalMat,
                    new Vector3(0f, 0f, -30f), new Vector3(3f, 15f, 3f), Quaternion.identity);
                // Aether glow at base
                AddDecoration("Spire_AetherBase", new Vector3(0f, 0.5f, -30f),
                    null, aetherBright, new Vector3(4f, 1f, 4f));
            }

            // ── Replace RuinedPillars ──
            if (pillarMesh != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    string name = $"RuinedPillar_{i}";
                    var go = GameObject.Find(name);
                    if (go != null)
                    {
                        var mf = go.GetComponent<MeshFilter>();
                        if (mf != null) mf.sharedMesh = pillarMesh;
                        var mr = go.GetComponent<MeshRenderer>();
                        if (mr != null) mr.sharedMaterial = stoneWeathered;
                    }
                }
            }

            // ── Replace MudMounds ──
            for (int i = 0; i < 4; i++)
            {
                var mound = GameObject.Find($"MudMound_{i}");
                if (mound != null)
                {
                    var mr = mound.GetComponent<MeshRenderer>();
                    if (mr != null) mr.sharedMaterial = (i % 2 == 0) ? mudFresh : mudCracking;
                }
            }

            // ── Add architectural details ──
            // 4 arches around the plaza
            if (archMesh != null)
            {
                float[] archAngles = { 0f, 90f, 180f, 270f };
                for (int i = 0; i < 4; i++)
                {
                    float rad = archAngles[i] * Mathf.Deg2Rad;
                    Vector3 pos = new Vector3(Mathf.Sin(rad) * 16f, 0f, Mathf.Cos(rad) * 16f);
                    Quaternion rot = Quaternion.Euler(0f, archAngles[i], 0f);
                    AddDecoration($"Plaza_Arch_{i}", pos, archMesh, stoneWeathered,
                        new Vector3(5f, 6f, 2f), rot);
                }
            }

            // Step pyramid at a distance
            if (pyramidMesh != null)
            {
                AddDecoration("DistantPyramid", new Vector3(-50f, 0f, -40f),
                    pyramidMesh, stoneGolden, new Vector3(15f, 8f, 15f));
            }

            // ── Corruption tendrils around enemy spawns ──
            string[] spawns = { "GolemSpawn_RS25", "GolemSpawn_RS50", "GolemSpawn_RS75" };
            foreach (var spawnName in spawns)
            {
                var spawn = GameObject.Find(spawnName);
                if (spawn != null && corruption != null)
                {
                    var ring = spawn.transform.Find("SpawnRing");
                    if (ring != null)
                    {
                        var mr = ring.GetComponent<MeshRenderer>();
                        if (mr != null) mr.sharedMaterial = corruption;
                    }
                }
            }

            // ── Aether ley lines on ground (flat quads) ──
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f * Mathf.Deg2Rad;
                Vector3 start = Vector3.zero;
                Vector3 end = new Vector3(Mathf.Sin(angle) * 50f, 0f, Mathf.Cos(angle) * 50f);
                string name = $"LeyLine_{i}";
                if (GameObject.Find(name) != null) continue;

                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name = name;
                go.transform.position = (start + end) * 0.5f + Vector3.up * 0.05f;
                go.transform.rotation = Quaternion.Euler(90f, i * 120f, 0f);
                go.transform.localScale = new Vector3(1.5f, 50f, 1f);
                go.isStatic = true;
                Object.DestroyImmediate(go.GetComponent<MeshCollider>());
                var mr = go.GetComponent<MeshRenderer>();
                if (mr != null && aetherFlow != null) mr.sharedMaterial = aetherFlow;

                var parent = GameObject.Find("EchohavenTerrain");
                if (parent != null) go.transform.SetParent(parent.transform);
            }

            // ── Spawn marker upgrades ──
            var spawnMarker = GameObject.Find("SpawnMarker");
            if (spawnMarker != null && aetherBright != null)
            {
                var mr = spawnMarker.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = aetherBright;
            }

            // ── Upgrade Player placeholder mesh ──
            var playerPlaceholder = GameObject.Find("PlayerMesh_Placeholder");
            if (playerPlaceholder != null)
            {
                var mr = playerPlaceholder.GetComponent<MeshRenderer>();
                if (mr != null && aetherFlow != null) mr.sharedMaterial = aetherFlow;
            }

            // ── Additional atmospheric lights (bright enough to see from spawn) ──
            AddPointLight("Light_StarDome", new Vector3(30f, 8f, 20f),
                new Color(0.95f, 0.85f, 0.3f), 8f, 35f);
            AddPointLight("Light_Spire", new Vector3(0f, 12f, -30f),
                new Color(0.4f, 0.65f, 1f), 6f, 30f);
            AddPointLight("Light_Fountain", new Vector3(-20f, 4f, 35f),
                new Color(0.3f, 0.6f, 0.9f), 6f, 25f);

            // ── Assign custom skybox ──
            var skyMat = LoadMat("M_Skybox_Tartaria");
            if (skyMat != null)
            {
                RenderSettings.skybox = skyMat;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = new Color(0.25f, 0.35f, 0.55f);
                RenderSettings.ambientEquatorColor = new Color(0.45f, 0.40f, 0.35f);
                RenderSettings.ambientGroundColor = new Color(0.18f, 0.15f, 0.12f);
                DynamicGI.UpdateEnvironment();
            }

            // ── Remove AudioListeners from gameplay scene ──
            // Boot camera owns the sole listener; SceneLoader manages handoff at runtime.
            var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (listeners.Length > 0)
            {
                foreach (var l in listeners)
                    Object.DestroyImmediate(l);
                Debug.Log($"[Tartaria] Removed {listeners.Length} AudioListener(s) from Echohaven (SceneLoader manages at runtime).");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[Tartaria] Scene upgraded — procedural architecture + shader materials applied.");
        }

        static void ReplaceMesh(string goName, Mesh mesh, Material mat,
            Vector3? position, Vector3? scale, Quaternion? rotation)
        {
            var go = GameObject.Find(goName);
            if (go == null) return;

            if (mesh != null)
            {
                var mf = go.GetComponent<MeshFilter>();
                if (mf != null) mf.sharedMesh = mesh;
            }
            if (mat != null)
            {
                var mr = go.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = mat;
            }
            if (position.HasValue) go.transform.position = position.Value;
            if (scale.HasValue) go.transform.localScale = scale.Value;
            if (rotation.HasValue) go.transform.rotation = rotation.Value;
        }

        static void AddDecoration(string name, Vector3 position, Mesh mesh, Material mat,
            Vector3 scale, Quaternion? rotation = null)
        {
            if (mat == null) return; // Skip — no material means it would render pink
            if (GameObject.Find(name) != null) return;

            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = scale;
            if (rotation.HasValue) go.transform.rotation = rotation.Value;
            go.isStatic = true;

            var mf = go.AddComponent<MeshFilter>();
            if (mesh != null) mf.sharedMesh = mesh;
            else
            {
                // Use a flat plane for effects
                var tempPlane = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mf.sharedMesh = tempPlane.GetComponent<MeshFilter>().sharedMesh;
                Object.DestroyImmediate(tempPlane);
            }

            var mr = go.AddComponent<MeshRenderer>();
            if (mat != null) mr.sharedMaterial = mat;

            var parent = GameObject.Find("--- BUILDINGS ---");
            if (parent != null) go.transform.SetParent(parent.transform);
        }

        static void AddPointLight(string name, Vector3 position, Color color, float intensity, float range)
        {
            if (GameObject.Find(name) != null) return;

            var go = new GameObject(name);
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            // Atmospheric fill lights — no shadows to avoid atlas overflow (shadow atlas is 2048x2048)
            light.shadows = LightShadows.None;

            var parent = GameObject.Find("Echohaven_Lighting");
            if (parent != null) go.transform.SetParent(parent.transform);
        }

        static Material LoadMat(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>($"{MatPath}/{name}.mat");
        }

        /// <summary>
        /// Creates or retrieves a material and forces it to use URP/Lit shader.
        /// Always updates shader even on existing materials to fix pink/broken rendering.
        /// </summary>
        static Material SetupURPMaterial(string name)
        {
            string path = $"{MatPath}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) { Debug.LogWarning($"[Tartaria] URP/Lit not found for {name}"); return null; }
            if (mat == null)
            {
                mat = new Material(shader);
                mat.name = name;
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }
            return mat;
        }

        /// <summary>
        /// Configures a URP/Lit material for transparent rendering.
        /// </summary>
        static void SetTransparent(Material mat)
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        // ═══════════════════════════════════════════════
        //  PREFAB UPGRADE
        // ═══════════════════════════════════════════════

        static void UpgradePrefabs()
        {
            UpgradePlayerPrefab();
            UpgradeMiloPrefab();
            UpgradeMudGolemPrefab();
            UpgradeAnastasiaPrefab();
            Debug.Log("[Tartaria] All character prefabs upgraded with shader materials.");
        }

        static void UpgradePlayerPrefab()
        {
            string path = "Assets/_Project/Prefabs/Characters/Player.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var playerMat = LoadMat("Player_Aether");
                var headMat = LoadMat("Player_Head");
                var limbMat = LoadMat("Player_Limbs");
                var stoneMat = LoadMat("M_Stone_Active");
                var bodyMat = playerMat != null ? playerMat : stoneMat;
                var useLimbMat = limbMat != null ? limbMat : bodyMat;
                var useHeadMat = headMat != null ? headMat : bodyMat;

                // Body — core material (deep blue)
                ApplyMatToChild(instance, "Body", bodyMat);
                // Head — lighter, clearly distinct from body
                ApplyMatToChild(instance, "Head", useHeadMat);
                // Eyes — brilliant white-blue crystals
                var headT = instance.transform.Find("Head");
                if (headT != null)
                {
                    var crystalMat = LoadMat("Crystal_Active");
                    var aetherGlow = LoadMat("Aether_Glow");
                    ApplyMatToChild(headT.gameObject, "Eye_L", crystalMat);
                    ApplyMatToChild(headT.gameObject, "Eye_R", crystalMat);
                    ApplyMatToChild(headT.gameObject, "Visor", aetherGlow);
                    // Add point lights in eyes for glow effect
                    AddChildLight(headT, "Eye_L", new Color(0.4f, 0.7f, 1f), 1.5f, 2f);
                    AddChildLight(headT, "Eye_R", new Color(0.4f, 0.7f, 1f), 1.5f, 2f);
                }
                // Arms + Legs — dark limbs for silhouette contrast
                ApplyMatToChild(instance, "Arm_L", useLimbMat);
                ApplyMatToChild(instance, "Arm_R", useLimbMat);
                ApplyMatToChild(instance, "Leg_L", useLimbMat);
                ApplyMatToChild(instance, "Leg_R", useLimbMat);
                ApplyMatToChild(instance, "Foot_L", useLimbMat);
                ApplyMatToChild(instance, "Foot_R", useLimbMat);
                // Shoulders — warm gold accent
                var goldMat = LoadMat("M_Gold_Ornament");
                ApplyMatToChild(instance, "Shoulder_L", goldMat);
                ApplyMatToChild(instance, "Shoulder_R", goldMat);
                // Belt — gold accent
                ApplyMatToChild(instance, "Belt", goldMat);
                // AetherCore — very bright glow with a light source
                var aetherBright = LoadMat("M_Aether_Bright");
                ApplyMatToChild(instance, "AetherCore", aetherBright);
                var coreT = instance.transform.Find("AetherCore");
                if (coreT != null)
                {
                    var coreLight = coreT.gameObject.GetComponent<Light>();
                    if (coreLight == null) coreLight = coreT.gameObject.AddComponent<Light>();
                    coreLight.type = LightType.Point;
                    coreLight.color = new Color(0.2f, 0.7f, 1f);
                    coreLight.intensity = 3f;
                    coreLight.range = 4f;
                    coreLight.shadows = LightShadows.None;
                }

                PrefabUtility.SaveAsPrefabAsset(instance, path);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        static void AddChildLight(Transform parent, string childName,
            Color color, float intensity, float range)
        {
            var child = parent.Find(childName);
            if (child == null) return;
            var light = child.GetComponent<Light>();
            if (light == null) light = child.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        static void UpgradeMiloPrefab()
        {
            string path = "Assets/_Project/Prefabs/Characters/Milo.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var aetherMat = LoadMat("M_Aether_Bright");
                var crystalMat = LoadMat("M_Crystal");

                ApplyMatToChild(instance, "Body", aetherMat);
                ApplyMatToChild(instance, "GlowCore", crystalMat);
                foreach (var r in instance.GetComponentsInChildren<MeshRenderer>())
                {
                    if (r.gameObject.name.StartsWith("Eye_"))
                        r.sharedMaterial = crystalMat;
                }

                PrefabUtility.SaveAsPrefabAsset(instance, path);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        static void UpgradeMudGolemPrefab()
        {
            string path = "Assets/_Project/Prefabs/Characters/MudGolem.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var golemMat = LoadMat("MudGolem_Body");
                var stoneMat = LoadMat("M_Stone_Active");
                var useMat = golemMat != null ? golemMat : stoneMat;
                var crystalMat = LoadMat("Crystal_Active");

                // Apply dark earth material to all mesh renderers
                foreach (var r in instance.GetComponentsInChildren<MeshRenderer>())
                {
                    if (r.gameObject.name.StartsWith("Eye_") || r.gameObject.name.Contains("Core"))
                        r.sharedMaterial = crystalMat != null ? crystalMat : useMat;
                    else
                        r.sharedMaterial = useMat;
                }

                // Glowing red eyes for threat readability
                AddChildLight(instance.transform, "Eye_L", new Color(1f, 0.3f, 0.1f), 1.2f, 2.5f);
                AddChildLight(instance.transform, "Eye_R", new Color(1f, 0.3f, 0.1f), 1.2f, 2.5f);

                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Debug.Log("[VisualUpgrade] MudGolem prefab upgraded.");
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        static void UpgradeAnastasiaPrefab()
        {
            string path = "Assets/_Project/Prefabs/Characters/Anastasia.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var ghostMat = LoadMat("M_Anastasia_Ghost");
                var glowMat  = LoadMat("M_Anastasia_Glow");

                // Apply ghost transparency to main body parts
                foreach (var r in instance.GetComponentsInChildren<MeshRenderer>())
                {
                    string n = r.gameObject.name;
                    if (n == "Crown" || n.StartsWith("Eye"))
                        r.sharedMaterial = glowMat != null ? glowMat : ghostMat;
                    else
                        r.sharedMaterial = ghostMat;
                }

                // Ensure ethereal glow light has no shadows
                var glow = instance.transform.Find("EtherealGlow");
                if (glow != null)
                {
                    var l = glow.GetComponent<Light>();
                    if (l != null) { l.shadows = LightShadows.None; l.intensity = 0.9f; l.range = 5f; }
                }

                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Debug.Log("[VisualUpgrade] Anastasia prefab upgraded.");
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        static void ApplyMatToChild(GameObject root, string childName, Material mat)
        {
            if (mat == null) return;
            var t = root.transform.Find(childName);
            if (t == null) return;
            var r = t.GetComponent<MeshRenderer>();
            if (r != null) r.sharedMaterial = mat;
        }
    }
}
