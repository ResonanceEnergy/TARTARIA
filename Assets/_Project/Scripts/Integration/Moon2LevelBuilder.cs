using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Level Builder — The Resonant Caverns
    /// Underground temple complex with crystal formations and ancient machinery
    /// Contrast to Moon 1: Dark, enclosed, vertical exploration
    /// Theme: Acoustic resonance, echo chambers, harmonic puzzles
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon2LevelBuilder : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] Vector3 cavernCenter = Vector3.zero;
        [SerializeField] float cavernRadius = 120f;
        [SerializeField] float cavernHeight = 60f;

        [Header("Cave Architecture")]
        [SerializeField] GameObject[] stalactitePrefabs;
        [SerializeField] GameObject[] stalagmitePrefabs;
        [SerializeField] GameObject[] crystalFormationPrefabs;
        [SerializeField] GameObject[] pillarRockPrefabs;

        [Header("Materials")]
        [SerializeField] Material caveStoneMaterial;
        [SerializeField] Material wetStoneMaterial;
        [SerializeField] Material crystalMaterial;
        [SerializeField] Material glowingCrystalMaterial;
        [SerializeField] Material ancientMetalMaterial;

        [Header("Temple Chambers (5 rooms)")]
        [SerializeField] Vector3 entranceChamberPos = new Vector3(0f, 0f, -80f);
        [SerializeField] Vector3 echoHallPos = new Vector3(-50f, -10f, 0f);
        [SerializeField] Vector3 resonanceChamberPos = new Vector3(0f, -20f, 50f);
        [SerializeField] Vector3 crystalGrottoPos = new Vector3(60f, -15f, 20f);
        [SerializeField] Vector3 harmonicSanctumPos = new Vector3(0f, -35f, 0f); // Deepest level

        const float PHI = 1.618033988749895f;

        void Start()
        {
            BuildCavern();
        }

        void BuildCavern()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 2: THE RESONANT CAVERNS — BUILDING");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            LoadMaterials();

            var cavernParent = new GameObject("Moon2_ResonantCaverns");
            cavernParent.transform.position = cavernCenter;

            // Build main cavern shell
            CreateCavernShell(cavernParent);

            // Build 5 temple chambers
            CreateEntranceChamber(cavernParent);
            CreateEchoHall(cavernParent);
            CreateResonanceChamber(cavernParent);
            CreateCrystalGrotto(cavernParent);
            CreateHarmonicSanctum(cavernParent);

            // Add cave decorations
            AddCaveDecorations(cavernParent);

            // Create connecting tunnels
            CreateTunnels(cavernParent);

            Debug.Log("[Moon2LevelBuilder] ✅ Resonant Caverns complete!");
            Debug.Log($"  • Cavern: {cavernRadius}m radius × {cavernHeight}m height");
            Debug.Log("  • 5 temple chambers");
            Debug.Log("  • Crystal formations + acoustic architecture");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        void LoadMaterials()
        {
            if (caveStoneMaterial == null)
                caveStoneMaterial = Resources.Load<Material>("Materials/PBR/Rock035");
            if (wetStoneMaterial == null)
                wetStoneMaterial = Resources.Load<Material>("Materials/PBR/Rock035_Wet");
            if (crystalMaterial == null)
                crystalMaterial = Resources.Load<Material>("Materials/M_Crystal_Aether");
            if (glowingCrystalMaterial == null)
                glowingCrystalMaterial = Resources.Load<Material>("Materials/M_Crystal_Glowing");
            if (ancientMetalMaterial == null)
                ancientMetalMaterial = Resources.Load<Material>("Materials/PBR/Metal048A");

            // Fallbacks
            if (caveStoneMaterial == null)
            {
                caveStoneMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                caveStoneMaterial.color = new Color(0.3f, 0.25f, 0.2f);
            }
        }

        void CreateCavernShell(GameObject parent)
        {
            var shell = new GameObject("Cavern_Shell");
            shell.transform.SetParent(parent.transform);
            shell.transform.localPosition = Vector3.zero;

            // Create inverted hemisphere for cave ceiling
            var ceiling = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ceiling.name = "Cave_Ceiling";
            ceiling.transform.SetParent(shell.transform);
            ceiling.transform.localPosition = new Vector3(0f, cavernHeight * 0.5f, 0f);
            ceiling.transform.localScale = new Vector3(cavernRadius * 2f, cavernHeight, cavernRadius * 2f);
            ceiling.GetComponent<Renderer>().material = caveStoneMaterial;

            // Invert normals (render from inside)
            InvertMeshNormals(ceiling.GetComponent<MeshFilter>());

            // Cave floor (irregular)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Cave_Floor";
            floor.transform.SetParent(shell.transform);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(cavernRadius * 0.2f, 1f, cavernRadius * 0.2f);
            floor.GetComponent<Renderer>().material = wetStoneMaterial;

            Debug.Log($"  ✓ Cavern shell: {cavernRadius}m radius × {cavernHeight}m height");
        }

        void CreateEntranceChamber(GameObject parent)
        {
            var chamber = new GameObject("01_EntranceChamber");
            chamber.transform.SetParent(parent.transform);
            chamber.transform.localPosition = entranceChamberPos;

            // Large opening chamber: 40m × 30m × 20m high
            var room = GameObject.CreatePrimitive(PrimitiveType.Cube);
            room.name = "Chamber_Volume";
            room.transform.SetParent(chamber.transform);
            room.transform.localPosition = new Vector3(0f, 10f, 0f);
            room.transform.localScale = new Vector3(40f, 20f, 30f);
            room.GetComponent<Renderer>().material = caveStoneMaterial;
            InvertMeshNormals(room.GetComponent<MeshFilter>());

            // 4 Welcome pillars
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f + 45f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(12f, 0f, 0f);
                CreatePillar(chamber, offset, 15f, ancientMetalMaterial);
            }

            // Crystal light clusters (6)
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(15f, 12f, 0f);
                CreateCrystalCluster(chamber, offset, 2f);
            }

            Debug.Log("  ✓ Entrance Chamber (40×30×20m)");
        }

        void CreateEchoHall(GameObject parent)
        {
            var hall = new GameObject("02_EchoHall");
            hall.transform.SetParent(parent.transform);
            hall.transform.localPosition = echoHallPos;

            // Long corridor with acoustic properties: 60m × 15m × 12m
            var corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corridor.name = "Hall_Volume";
            corridor.transform.SetParent(hall.transform);
            corridor.transform.localPosition = new Vector3(0f, 6f, 0f);
            corridor.transform.localScale = new Vector3(15f, 12f, 60f);
            corridor.GetComponent<Renderer>().material = caveStoneMaterial;
            InvertMeshNormals(corridor.GetComponent<MeshFilter>());

            // Pillars along both sides (10 per side)
            for (int side = 0; side < 2; side++)
            {
                float xOffset = side == 0 ? -6f : 6f;
                for (int i = 0; i < 10; i++)
                {
                    float zOffset = (i - 4.5f) * 6f;
                    CreatePillar(hall, new Vector3(xOffset, 0f, zOffset), 10f, ancientMetalMaterial);
                }
            }

            Debug.Log("  ✓ Echo Hall (60×15×12m corridor)");
        }

        void CreateResonanceChamber(GameObject parent)
        {
            var chamber = new GameObject("03_ResonanceChamber");
            chamber.transform.SetParent(parent.transform);
            chamber.transform.localPosition = resonanceChamberPos;

            // Circular chamber for main puzzle: 50m diameter × 25m high
            var room = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            room.name = "Chamber_Volume";
            room.transform.SetParent(chamber.transform);
            room.transform.localPosition = new Vector3(0f, 12.5f, 0f);
            room.transform.localScale = new Vector3(50f, 12.5f, 50f);
            room.GetComponent<Renderer>().material = caveStoneMaterial;
            InvertMeshNormals(room.GetComponent<MeshFilter>());

            // 8 Resonance pillars in circle
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(20f, 0f, 0f);
                CreatePillar(chamber, offset, 20f, glowingCrystalMaterial);
            }

            // Central altar
            var altar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            altar.name = "Resonance_Altar";
            altar.transform.SetParent(chamber.transform);
            altar.transform.localPosition = new Vector3(0f, 2f, 0f);
            altar.transform.localScale = new Vector3(6f, 2f, 6f);
            altar.GetComponent<Renderer>().material = ancientMetalMaterial;

            Debug.Log("  ✓ Resonance Chamber (50m diameter × 25m high, main puzzle room)");
        }

        void CreateCrystalGrotto(GameObject parent)
        {
            var grotto = new GameObject("04_CrystalGrotto");
            grotto.transform.SetParent(parent.transform);
            grotto.transform.localPosition = crystalGrottoPos;

            // Irregular cave space: 35m × 35m × 18m
            var cave = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cave.name = "Grotto_Volume";
            cave.transform.SetParent(grotto.transform);
            cave.transform.localPosition = new Vector3(0f, 9f, 0f);
            cave.transform.localScale = new Vector3(35f, 18f, 35f);
            cave.GetComponent<Renderer>().material = caveStoneMaterial;
            InvertMeshNormals(cave.GetComponent<MeshFilter>());

            // Many crystal formations (20)
            for (int i = 0; i < 20; i++)
            {
                float angle = Random.Range(0f, 360f);
                float distance = Random.Range(5f, 15f);
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(distance, Random.Range(0f, 8f), 0f);
                CreateCrystalCluster(grotto, offset, Random.Range(1.5f, 4f));
            }

            Debug.Log("  ✓ Crystal Grotto (35×35×18m, 20 crystal formations)");
        }

        void CreateHarmonicSanctum(GameObject parent)
        {
            var sanctum = new GameObject("05_HarmonicSanctum");
            sanctum.transform.SetParent(parent.transform);
            sanctum.transform.localPosition = harmonicSanctumPos;

            // Perfect spherical chamber (ancient acoustic engineering): 40m diameter
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Sanctum_Volume";
            sphere.transform.SetParent(sanctum.transform);
            sphere.transform.localPosition = new Vector3(0f, 20f, 0f);
            sphere.transform.localScale = Vector3.one * 40f;
            sphere.GetComponent<Renderer>().material = wetStoneMaterial;
            InvertMeshNormals(sphere.GetComponent<MeshFilter>());

            // 12 Harmonic crystals (dodecahedron arrangement)
            for (int i = 0; i < 12; i++)
            {
                float phi = Mathf.PI * (3f - Mathf.Sqrt(5f)); // Golden angle
                float theta = phi * i;
                float radius = 15f;
                float y = Mathf.Sin(theta) * radius;
                float r = Mathf.Cos(theta) * radius;
                float x = Mathf.Cos(i * phi) * r;
                float z = Mathf.Sin(i * phi) * r;

                Vector3 offset = new Vector3(x, y, z);
                CreateCrystalCluster(sanctum, offset, 3f);
            }

            // Central harmonic core
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "Harmonic_Core";
            core.transform.SetParent(sanctum.transform);
            core.transform.localPosition = new Vector3(0f, 20f, 0f);
            core.transform.localScale = Vector3.one * 5f;
            core.GetComponent<Renderer>().material = glowingCrystalMaterial;

            Debug.Log("  ✓ Harmonic Sanctum (40m diameter sphere, 12 crystals, final chamber)");
        }

        void AddCaveDecorations(GameObject parent)
        {
            var decorations = new GameObject("Cave_Decorations");
            decorations.transform.SetParent(parent.transform);

            // Add stalactites (hanging from ceiling)
            for (int i = 0; i < 30; i++)
            {
                float angle = Random.Range(0f, 360f);
                float distance = Random.Range(20f, cavernRadius * 0.8f);
                Vector3 position = Quaternion.Euler(0f, angle, 0f) * new Vector3(distance, cavernHeight * 0.9f, 0f);
                CreateStalactite(decorations, position);
            }

            // Add stalagmites (rising from floor)
            for (int i = 0; i < 40; i++)
            {
                float angle = Random.Range(0f, 360f);
                float distance = Random.Range(30f, cavernRadius * 0.9f);
                Vector3 position = Quaternion.Euler(0f, angle, 0f) * new Vector3(distance, 0f, 0f);
                CreateStalagmite(decorations, position);
            }

            Debug.Log("  ✓ Cave decorations: 30 stalactites + 40 stalagmites");
        }

        void CreateTunnels(GameObject parent)
        {
            var tunnels = new GameObject("Connecting_Tunnels");
            tunnels.transform.SetParent(parent.transform);

            // Create 4 tunnels connecting chambers
            CreateTunnel(tunnels, entranceChamberPos, echoHallPos, 8f);
            CreateTunnel(tunnels, echoHallPos, resonanceChamberPos, 8f);
            CreateTunnel(tunnels, resonanceChamberPos, crystalGrottoPos, 6f);
            CreateTunnel(tunnels, resonanceChamberPos, harmonicSanctumPos, 10f);

            Debug.Log("  ✓ 4 connecting tunnels");
        }

        void CreateTunnel(GameObject parent, Vector3 start, Vector3 end, float radius)
        {
            var tunnel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tunnel.transform.SetParent(parent.transform);

            Vector3 center = (start + end) / 2f;
            float length = Vector3.Distance(start, end);

            tunnel.transform.position = center;
            tunnel.transform.localScale = new Vector3(radius * 2f, length / 2f, radius * 2f);
            tunnel.transform.LookAt(end);
            tunnel.transform.Rotate(90f, 0f, 0f);
            tunnel.GetComponent<Renderer>().material = caveStoneMaterial;
            InvertMeshNormals(tunnel.GetComponent<MeshFilter>());
        }

        void CreatePillar(GameObject parent, Vector3 localPosition, float height, Material material)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar";
            pillar.transform.SetParent(parent.transform);
            pillar.transform.localPosition = localPosition;
            pillar.transform.localScale = new Vector3(1f, height / 2f, 1f);
            pillar.GetComponent<Renderer>().material = material != null ? material : caveStoneMaterial;
            Destroy(pillar.GetComponent<Collider>());
        }

        void CreateCrystalCluster(GameObject parent, Vector3 localPosition, float scale)
        {
            var cluster = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cluster.name = "Crystal_Cluster";
            cluster.transform.SetParent(parent.transform);
            cluster.transform.localPosition = localPosition;
            cluster.transform.localScale = Vector3.one * scale;
            cluster.GetComponent<Renderer>().material = glowingCrystalMaterial != null ? glowingCrystalMaterial : crystalMaterial;
            Destroy(cluster.GetComponent<Collider>());

            // Add point light
            var light = cluster.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.6f, 0.8f, 1f); // Cool blue crystal glow
            light.intensity = scale * 2f;
            light.range = scale * 8f;
        }

        void CreateStalactite(GameObject parent, Vector3 position)
        {
            var stalactite = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stalactite.name = "Stalactite";
            stalactite.transform.SetParent(parent.transform);
            stalactite.transform.position = position;
            stalactite.transform.localScale = new Vector3(1f, Random.Range(2f, 6f), 1f);
            stalactite.transform.Rotate(180f, 0f, 0f); // Point down
            stalactite.GetComponent<Renderer>().material = wetStoneMaterial;
            Destroy(stalactite.GetComponent<Collider>());
        }

        void CreateStalagmite(GameObject parent, Vector3 position)
        {
            var stalagmite = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stalagmite.name = "Stalagmite";
            stalagmite.transform.SetParent(parent.transform);
            stalagmite.transform.position = position;
            stalagmite.transform.localScale = new Vector3(1.5f, Random.Range(1f, 4f), 1.5f);
            stalagmite.GetComponent<Renderer>().material = caveStoneMaterial;
            Destroy(stalagmite.GetComponent<Collider>());
        }

        void InvertMeshNormals(MeshFilter meshFilter)
        {
            if (meshFilter == null) return;

            Mesh mesh = meshFilter.mesh;
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -normals[i];
            }
            mesh.normals = normals;

            // Flip triangles
            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                int[] triangles = mesh.GetTriangles(m);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                mesh.SetTriangles(triangles, m);
            }
        }
    }
}
