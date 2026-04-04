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
        /// Also builds building prefabs using the procedural meshes.
        /// </summary>
        public static void ApplyVisualUpgrade()
        {
            UpgradeScene();
            UpgradePrefabs();
            AssetFactoryWizard.BuildBuildingPrefabs();
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
                    tris.Add(a); tris.Add(b); tris.Add(a + 1);
                    tris.Add(a + 1); tris.Add(b); tris.Add(b + 1);
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
            // TartarianStone variants
            CreateStone("M_Stone_Weathered", new Color(0.55f, 0.50f, 0.42f),
                0.45f, 0.3f, 0f, 0f);
            CreateStone("M_Stone_Active", new Color(0.72f, 0.68f, 0.58f),
                0.3f, 0.2f, 0.3f, 0.2f);
            CreateStone("M_Stone_Golden", new Color(0.78f, 0.72f, 0.55f),
                0.15f, 0.35f, 0.8f, 0.6f);
            CreateStone("M_Stone_Plaza", new Color(0.60f, 0.56f, 0.48f),
                0.5f, 0.2f, 0.1f, 0.05f);

            // MudDissolution variants
            CreateMud("M_Mud_Fresh", new Color(0.35f, 0.25f, 0.15f), 0f);
            CreateMud("M_Mud_Cracking", new Color(0.42f, 0.32f, 0.20f), 0.4f);
            CreateMud("M_Mud_Dissolving", new Color(0.50f, 0.38f, 0.25f), 0.7f);

            // AetherFlow
            CreateAether("M_Aether_Flow", new Color(0.2f, 0.5f, 0.9f, 0.3f), 1.5f);
            CreateAether("M_Aether_Bright", new Color(0.3f, 0.7f, 1f, 0.5f), 3f);
            CreateAether("M_Aether_Water", new Color(0.15f, 0.4f, 0.7f, 0.4f), 0.8f);

            // CorruptionPulse
            CreateCorruption("M_Corruption", new Color(0.15f, 0.05f, 0.2f, 0.7f), 1f);

            // GhostOpacity for Anastasia
            CreateGhost("M_Ghost_Anastasia", new Color(0.75f, 0.85f, 1f, 0.35f), 0.35f);

            // Ground — custom stone for terrain
            CreateStone("M_Ground_Terrain", new Color(0.38f, 0.35f, 0.28f),
                0.65f, 0f, 0f, 0f);

            // Gold accents
            CreateStone("M_Gold_Ornament", new Color(0.9f, 0.78f, 0.3f),
                0f, 0.92f, 1f, 1.2f);

            // Crystal
            CreateAether("M_Crystal", new Color(0.7f, 0.8f, 1f, 0.6f), 2f);

            Debug.Log("[Tartaria] 16 shader-based materials created.");
        }

        static void CreateStone(string name, Color baseColor,
            float weathering, float smoothness, float goldenStr, float emissionStr)
        {
            string path = $"{MatPath}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;

            var shader = Shader.Find("Tartaria/TartarianStone");
            if (shader == null) { Debug.LogWarning($"TartarianStone shader not found for {name}"); return; }

            var mat = new Material(shader);
            mat.name = name;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_WeatheringAmount", weathering);
            mat.SetFloat("_GoldenStrength", goldenStr);
            mat.SetFloat("_EmissionStrength", emissionStr);
            mat.SetFloat("_AetherPulse", 1.618f);
            mat.SetFloat("_RestorationProgress", 1f);
            AssetDatabase.CreateAsset(mat, path);
        }

        static void CreateMud(string name, Color baseColor, float dissolution)
        {
            string path = $"{MatPath}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;

            var shader = Shader.Find("Tartaria/MudDissolution");
            if (shader == null) { Debug.LogWarning($"MudDissolution shader not found for {name}"); return; }

            var mat = new Material(shader);
            mat.name = name;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_DissolutionProgress", dissolution);
            mat.SetFloat("_EdgeWidth", 0.03f);
            mat.SetColor("_EdgeColor", new Color(0.9f, 0.75f, 0.3f));
            mat.SetFloat("_RumbleIntensity", 0.05f);
            AssetDatabase.CreateAsset(mat, path);
        }

        static void CreateAether(string name, Color baseColor, float intensity)
        {
            string path = $"{MatPath}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;

            var shader = Shader.Find("Tartaria/AetherFlow");
            if (shader == null) { Debug.LogWarning($"AetherFlow shader not found for {name}"); return; }

            var mat = new Material(shader);
            mat.name = name;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Intensity", intensity);
            mat.SetFloat("_FlowSpeed", 1f);
            mat.SetFloat("_PulseSpeed", 1.618f);
            mat.SetFloat("_FresnelPower", 3f);
            AssetDatabase.CreateAsset(mat, path);
        }

        static void CreateCorruption(string name, Color baseColor, float intensity)
        {
            string path = $"{MatPath}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;

            var shader = Shader.Find("Tartaria/CorruptionPulse");
            if (shader == null) { Debug.LogWarning($"CorruptionPulse shader not found for {name}"); return; }

            var mat = new Material(shader);
            mat.name = name;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_CorruptionIntensity", intensity);
            mat.SetFloat("_PulseSpeed", 2f);
            mat.SetFloat("_FresnelPower", 3f);
            AssetDatabase.CreateAsset(mat, path);
        }

        static void CreateGhost(string name, Color baseColor, float opacity)
        {
            string path = $"{MatPath}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;

            var shader = Shader.Find("Tartaria/GhostOpacity");
            if (shader == null) { Debug.LogWarning($"GhostOpacity shader not found for {name}"); return; }

            var mat = new Material(shader);
            mat.name = name;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Opacity", opacity);
            mat.SetFloat("_FresnelPower", 2f);
            mat.SetFloat("_PulseSpeed", 1f);
            mat.SetColor("_EmissionColor", new Color(0.3f, 0.4f, 0.6f));
            mat.SetFloat("_EmissionStrength", 0.5f);
            AssetDatabase.CreateAsset(mat, path);
        }

        // ═══════════════════════════════════════════════
        //  SKYBOX
        // ═══════════════════════════════════════════════

        static void BuildSkyboxMaterial()
        {
            string path = $"{MatPath}/M_Skybox_Tartaria.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
            {
                RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(path);
                return;
            }

            var shader = Shader.Find("Tartaria/SkyGradient");
            if (shader == null) { Debug.LogWarning("SkyGradient shader not found"); return; }

            var mat = new Material(shader);
            mat.name = "M_Skybox_Tartaria";
            mat.SetColor("_TopColor", new Color(0.12f, 0.20f, 0.45f));
            mat.SetColor("_HorizonColor", new Color(0.70f, 0.58f, 0.42f));
            mat.SetColor("_GroundColor", new Color(0.18f, 0.15f, 0.12f));
            mat.SetColor("_SunColor", new Color(1f, 0.85f, 0.5f));
            mat.SetFloat("_SunSize", 0.04f);
            mat.SetFloat("_SunGlowFalloff", 12f);
            mat.SetFloat("_CloudDensity", 0.35f);
            mat.SetFloat("_CloudSpeed", 0.015f);
            mat.SetFloat("_TimeOfDay", 0.45f);
            mat.SetFloat("_RSProgress", 0.3f);
            AssetDatabase.CreateAsset(mat, path);

            RenderSettings.skybox = mat;
            Debug.Log("[Tartaria] Skybox assigned: Tartaria/SkyGradient.");
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

            // ── Additional atmospheric lights ──
            AddPointLight("Light_StarDome", new Vector3(30f, 6f, 20f),
                new Color(0.9f, 0.8f, 0.3f), 4f, 20f);
            AddPointLight("Light_Spire", new Vector3(0f, 8f, -30f),
                new Color(0.4f, 0.6f, 1f), 3f, 15f);
            AddPointLight("Light_Fountain", new Vector3(-20f, 2f, 35f),
                new Color(0.2f, 0.5f, 0.8f), 2.5f, 12f);

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
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.5f;

            var parent = GameObject.Find("Echohaven_Lighting");
            if (parent != null) go.transform.SetParent(parent.transform);
        }

        static Material LoadMat(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>($"{MatPath}/{name}.mat");
        }

        // ═══════════════════════════════════════════════
        //  PREFAB UPGRADE
        // ═══════════════════════════════════════════════

        static void UpgradePrefabs()
        {
            UpgradePlayerPrefab();
            UpgradeMiloPrefab();
            Debug.Log("[Tartaria] Character prefabs upgraded with shader materials.");
        }

        static void UpgradePlayerPrefab()
        {
            string path = "Assets/_Project/Prefabs/Characters/Player.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var aetherMat = LoadMat("M_Aether_Flow");
                var stoneMat = LoadMat("M_Stone_Active");

                // Body + Head get stone material
                ApplyMatToChild(instance, "Body", stoneMat);
                ApplyMatToChild(instance, "Head", stoneMat);
                // AetherCore gets bright glow
                ApplyMatToChild(instance, "AetherCore", LoadMat("M_Aether_Bright"));

                PrefabUtility.SaveAsPrefabAsset(instance, path);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
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
