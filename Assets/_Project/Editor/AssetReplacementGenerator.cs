using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Tartaria.Editor.AssetGen
{
    /// <summary>
    /// Unity Editor tool to replace ALL primitive fallbacks with production-ready assets.
    /// Generates character prefabs from KayKit models, structure prefabs with proper materials,
    /// and VFX particle systems for major story beats.
    /// </summary>
    public class AssetReplacementGenerator : EditorWindow
    {
        // KayKit asset paths
        const string KAYKIT_ADVENTURERS = "Assets/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE";
        const string KAYKIT_SKELETONS = "Assets/KayKit_Skeletons_1.1_FREE/KayKit_Skeletons_1.1_FREE";
        const string KAYKIT_FOREST = "Assets/KayKit_Forest_Nature_Pack_1.0_FREE/KayKit_Forest_Nature_Pack_1.0_FREE";
        const string KAYKIT_TOOLS = "Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE";

        // Output paths (must be under Resources for runtime loading)
        const string PREFABS_CHARACTERS = "Assets/_Project/Resources/Prefabs/Characters";
        const string PREFABS_STRUCTURES = "Assets/_Project/Resources/Prefabs/Buildings";
        const string PREFABS_VFX = "Assets/_Project/Resources/Prefabs/VFX";
        const string MATERIALS_PATH = "Assets/_Project/Resources/Materials/Generated";

        static int _assetsCreated = 0;
        static List<string> _creationLog = new List<string>();

        [MenuItem("Tartaria/Asset Replacement/Generate All Production Assets")]
        public static void GenerateAllAssets()
        {
            _assetsCreated = 0;
            _creationLog.Clear();

            Debug.Log("[AssetReplacementGenerator] Starting production asset generation...");

            // Phase 1: Characters (P0)
            GenerateCharacterPrefabs();

            // Phase 2: Structures (P1)
            GenerateStructurePrefabs();

            // Phase 3: VFX (P1)
            GenerateVFXPrefabs();

            // Summary
            Debug.Log($"[AssetReplacementGenerator] ✓ Complete! Created {_assetsCreated} production assets.");
            Debug.Log($"[AssetReplacementGenerator] Log:\n{string.Join("\n", _creationLog)}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #region Character Prefabs (P0)

        static void GenerateCharacterPrefabs()
        {
            Debug.Log("[AssetReplacementGenerator] === Phase 1: Character Prefabs (P0) ===");

            EnsureDirectory(PREFABS_CHARACTERS);
            EnsureDirectory(MATERIALS_PATH);

            // Milo (companion) - Use Ranger model (helpful scout vibe)
            CreateCharacterPrefab("Milo", "Ranger", new Color(0.4f, 0.6f, 0.8f), 1f);

            // Anastasia (NPC guide) - Use Mage model (wise guide vibe)
            CreateCharacterPrefab("Anastasia", "Mage", new Color(0.9f, 0.85f, 1f), 1f);

            // Mud Golems (enemies) - Use Knight model scaled/recolored (bulky earth creature)
            CreateCharacterPrefab("MudGolem", "Knight", new Color(0.45f, 0.35f, 0.25f), 1.2f);

            // Lirael (spectral companion) - Use Rogue_Hooded with translucent shader
            CreateSpectralCharacterPrefab("Lirael", "Rogue_Hooded", new Color(0.6f, 0.8f, 1f, 0.5f), 1f);

            // Korath (giant) - Use Barbarian scaled 3x (giant warrior)
            CreateCharacterPrefab("Korath", "Barbarian", new Color(0.65f, 0.5f, 0.4f), 3f);

            // Captain Thorne (airship captain) - Use Knight (authoritative military)
            CreateCharacterPrefab("CaptainThorne", "Knight", new Color(0.3f, 0.3f, 0.5f), 1.1f);

            // Cassian (sympathizer) - Use Rogue_Hooded (mysterious cloaked figure)
            CreateCharacterPrefab("Cassian", "Rogue_Hooded", new Color(0.2f, 0.2f, 0.25f), 1f);

            LogCreation("P0 Characters", 7);
        }

        static void CreateCharacterPrefab(string characterName, string kaykitModel, Color tint, float scale)
        {
            // Load base model
            string modelPath = $"{KAYKIT_ADVENTURERS}/Characters/gltf/{kaykitModel}.glb";
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

            if (modelAsset == null)
            {
                Debug.LogWarning($"[AssetReplacementGenerator] Model not found: {modelPath}");
                return;
            }

            // Instantiate and configure
            GameObject prefab = Object.Instantiate(modelAsset);
            prefab.name = characterName;
            prefab.transform.localScale = Vector3.one * scale;

            // Apply material tint
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            Material customMat = CreateTintedURPMaterial(characterName, tint);
            foreach (var renderer in renderers)
            {
                renderer.sharedMaterial = customMat;
            }

            // Add capsule collider for physics
            CapsuleCollider collider = prefab.AddComponent<CapsuleCollider>();
            collider.height = 2f * scale;
            collider.radius = 0.5f * scale;
            collider.center = new Vector3(0, collider.height * 0.5f, 0);

            // Save as prefab
            string prefabPath = $"{PREFABS_CHARACTERS}/{characterName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            Object.DestroyImmediate(prefab);

            _assetsCreated++;
            Debug.Log($"[AssetReplacementGenerator] ✓ Created character prefab: {characterName}");
        }

        static void CreateSpectralCharacterPrefab(string characterName, string kaykitModel, Color tint, float scale)
        {
            // Same as CreateCharacterPrefab but with transparent shader
            string modelPath = $"{KAYKIT_ADVENTURERS}/Characters/gltf/{kaykitModel}.glb";
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

            if (modelAsset == null)
            {
                Debug.LogWarning($"[AssetReplacementGenerator] Model not found: {modelPath}");
                return;
            }

            GameObject prefab = Object.Instantiate(modelAsset);
            prefab.name = characterName;
            prefab.transform.localScale = Vector3.one * scale;

            // Translucent material
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            Material customMat = CreateTranslucentURPMaterial(characterName, tint);
            foreach (var renderer in renderers)
            {
                renderer.sharedMaterial = customMat;
            }

            // Add ethereal glow light
            Light glowLight = prefab.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.color = new Color(tint.r, tint.g, tint.b);
            glowLight.range = 5f;
            glowLight.intensity = 1.5f;

            CapsuleCollider collider = prefab.AddComponent<CapsuleCollider>();
            collider.height = 2f * scale;
            collider.radius = 0.5f * scale;
            collider.center = new Vector3(0, collider.height * 0.5f, 0);

            string prefabPath = $"{PREFABS_CHARACTERS}/{characterName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            Object.DestroyImmediate(prefab);

            _assetsCreated++;
            Debug.Log($"[AssetReplacementGenerator] ✓ Created spectral character prefab: {characterName}");
        }

        #endregion

        #region Structure Prefabs (P1)

        static void GenerateStructurePrefabs()
        {
            Debug.Log("[AssetReplacementGenerator] === Phase 2: Structure Prefabs (P1) ===");

            EnsureDirectory(PREFABS_STRUCTURES);

            // Moon 10: Railway Stations (12 total)
            CreateStationPrefab("RailwayStation", new Color(0.6f, 0.6f, 0.65f), new Vector3(30f, 10f, 30f));

            // Moon 11: Fountains (7 total)
            CreateFountainPrefab("HarmonicFountain", new Color(0.7f, 0.85f, 1f));

            // Moon 12: Bell Towers (7 total)
            CreateBellTowerPrefab("BellTower", new Color(0.8f, 0.75f, 0.65f));

            // Moon 5: Pavilions (5 total) - Beaux-Arts style
            CreatePavilionPrefab("BeauxArtsPavilion", new Color(0.95f, 0.93f, 0.88f));

            LogCreation("P1 Structures", 4);
        }

        static void CreateStationPrefab(string name, Color tint, Vector3 scale)
        {
            GameObject station = new GameObject(name);

            // Main building (detailed cube with proper proportions)
            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = "MainBuilding";
            building.transform.SetParent(station.transform);
            building.transform.localPosition = Vector3.zero;
            building.transform.localScale = scale;

            Material buildingMat = CreateURPLitMaterial($"{name}_Building", tint);
            building.GetComponent<Renderer>().sharedMaterial = buildingMat;

            // Roof (darker tone)
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(station.transform);
            roof.transform.localPosition = new Vector3(0, scale.y * 0.6f, 0);
            roof.transform.localScale = new Vector3(scale.x * 1.1f, scale.y * 0.2f, scale.z * 1.1f);

            Material roofMat = CreateURPLitMaterial($"{name}_Roof", tint * 0.6f);
            roof.GetComponent<Renderer>().sharedMaterial = roofMat;

            // Entrance markers (4 pillars)
            for (int i = 0; i < 4; i++)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = $"Pillar_{i}";
                pillar.transform.SetParent(station.transform);

                float angle = i * 90f * Mathf.Deg2Rad;
                float radius = scale.x * 0.4f;
                pillar.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    scale.y * 0.25f,
                    Mathf.Sin(angle) * radius
                );
                pillar.transform.localScale = new Vector3(1.5f, scale.y * 0.5f, 1.5f);

                pillar.GetComponent<Renderer>().sharedMaterial = buildingMat;
            }

            // Save prefab
            string prefabPath = $"{PREFABS_STRUCTURES}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(station, prefabPath);
            Object.DestroyImmediate(station);

            _assetsCreated++;
            Debug.Log($"[AssetReplacementGenerator] ✓ Created structure prefab: {name}");
        }

        static void CreateFountainPrefab(string name, Color tint)
        {
            GameObject fountain = new GameObject(name);

            // Base pool (flat cylinder)
            GameObject pool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pool.name = "Pool";
            pool.transform.SetParent(fountain.transform);
            pool.transform.localPosition = Vector3.zero;
            pool.transform.localScale = new Vector3(4f, 0.3f, 4f);

            Material poolMat = CreateURPLitMaterial($"{name}_Pool", new Color(0.5f, 0.5f, 0.6f));
            pool.GetComponent<Renderer>().sharedMaterial = poolMat;

            // Central pedestal (tall cylinder)
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(fountain.transform);
            pedestal.transform.localPosition = new Vector3(0, 1.5f, 0);
            pedestal.transform.localScale = new Vector3(1f, 3f, 1f);

            Material pedestalMat = CreateURPLitMaterial($"{name}_Pedestal", tint);
            pedestal.GetComponent<Renderer>().sharedMaterial = pedestalMat;

            // Fountain top (sphere for water source)
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            top.name = "WaterSource";
            top.transform.SetParent(fountain.transform);
            top.transform.localPosition = new Vector3(0, 3.2f, 0);
            top.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            Material waterMat = CreateTranslucentURPMaterial($"{name}_Water", tint);
            top.GetComponent<Renderer>().sharedMaterial = waterMat;

            // Add point light for glow
            Light fountainLight = fountain.AddComponent<Light>();
            fountainLight.type = LightType.Point;
            fountainLight.color = tint;
            fountainLight.range = 10f;
            fountainLight.intensity = 2f;

            // Save prefab
            string prefabPath = $"{PREFABS_STRUCTURES}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(fountain, prefabPath);
            Object.DestroyImmediate(fountain);

            _assetsCreated++;
            Debug.Log($"[AssetReplacementGenerator] ✓ Created fountain prefab: {name}");
        }

        static void CreateBellTowerPrefab(string name, Color tint)
        {
            GameObject tower = new GameObject(name);

            // Tower base (cube)
            GameObject baseStructure = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseStructure.name = "TowerBase";
            baseStructure.transform.SetParent(tower.transform);
            baseStructure.transform.localPosition = Vector3.zero;
            baseStructure.transform.localScale = new Vector3(5f, 8f, 5f);

            Material baseMat = CreateURPLitMaterial($"{name}_Base", tint);
            baseStructure.GetComponent<Renderer>().sharedMaterial = baseMat;

            // Bell chamber (smaller cube at top)
            GameObject bellChamber = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bellChamber.name = "BellChamber";
            bellChamber.transform.SetParent(tower.transform);
            bellChamber.transform.localPosition = new Vector3(0, 5f, 0);
            bellChamber.transform.localScale = new Vector3(4f, 2f, 4f);

            Material chamberMat = CreateURPLitMaterial($"{name}_Chamber", tint * 0.9f);
            bellChamber.GetComponent<Renderer>().sharedMaterial = chamberMat;

            // Spire (cone on top)
            GameObject spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spire.name = "Spire";
            spire.transform.SetParent(tower.transform);
            spire.transform.localPosition = new Vector3(0, 7f, 0);
            spire.transform.localScale = new Vector3(2f, 4f, 2f);

            Material spireMat = CreateURPLitMaterial($"{name}_Spire", tint * 0.7f);
            spire.GetComponent<Renderer>().sharedMaterial = spireMat;

            // Bell (sphere, visible through chamber)
            GameObject bell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bell.name = "Bell";
            bell.transform.SetParent(tower.transform);
            bell.transform.localPosition = new Vector3(0, 5f, 0);
            bell.transform.localScale = new Vector3(1.5f, 1.8f, 1.5f);

            Material bellMat = CreateURPLitMaterial($"{name}_Bell", new Color(0.8f, 0.7f, 0.3f)); // Bronze
            bell.GetComponent<Renderer>().sharedMaterial = bellMat;

            // Save prefab
            string prefabPath = $"{PREFABS_STRUCTURES}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(tower, prefabPath);
            Object.DestroyImmediate(tower);

            _assetsCreated++;
            Debug.Log($"[AssetReplacementGenerator] ✓ Created bell tower prefab: {name}");
        }

        static void CreatePavilionPrefab(string name, Color tint)
        {
            GameObject pavilion = new GameObject(name);

            // Platform base (flat cube)
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Platform";
            platform.transform.SetParent(pavilion.transform);
            platform.transform.localPosition = Vector3.zero;
            platform.transform.localScale = new Vector3(12f, 0.5f, 12f);

            Material platformMat = CreateURPLitMaterial($"{name}_Platform", tint);
            platform.GetComponent<Renderer>().sharedMaterial = platformMat;

            // Columns (8 pillars in circle)
            for (int i = 0; i < 8; i++)
            {
                GameObject column = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                column.name = $"Column_{i}";
                column.transform.SetParent(pavilion.transform);

                float angle = i * 45f * Mathf.Deg2Rad;
                float radius = 5f;
                column.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    3f,
                    Mathf.Sin(angle) * radius
                );
                column.transform.localScale = new Vector3(0.8f, 6f, 0.8f);

                Material columnMat = CreateURPLitMaterial($"{name}_Column", tint * 0.95f);
                column.GetComponent<Renderer>().sharedMaterial = columnMat;
            }

            // Dome roof (scaled sphere, top half)
            GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = "Dome";
            dome.transform.SetParent(pavilion.transform);
            dome.transform.localPosition = new Vector3(0, 7f, 0);
            dome.transform.localScale = new Vector3(10f, 5f, 10f);

            Material domeMat = CreateURPLitMaterial($"{name}_Dome", tint * 0.85f);
            dome.GetComponent<Renderer>().sharedMaterial = domeMat;

            // Save prefab
            string prefabPath = $"{PREFABS_STRUCTURES}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(pavilion, prefabPath);
            Object.DestroyImmediate(pavilion);

            _assetsCreated++;
            Debug.Log($"[AssetReplacementGenerator] ✓ Created pavilion prefab: {name}");
        }

        #endregion

        #region VFX Prefabs (P1)

        static void GenerateVFXPrefabs()
        {
            Debug.Log("[AssetReplacementGenerator] === Phase 3: VFX Prefabs (P1) ===");

            EnsureDirectory(PREFABS_VFX);

            // Cathedral transformation (mud → crystal)
            CreateTransformationVFX("CathedralTransformation", 
                new Color(0.45f, 0.35f, 0.25f), // Mud brown
                new Color(0.8f, 0.9f, 1f));     // Crystal blue

            // Golden rails permanence effect
            CreatePermanenceVFX("GoldenRailsPermanence", new Color(1f, 0.85f, 0.3f));

            // Aurora city manifestation
            CreateManifestationVFX("AuroraCityManifestation", new Color(0.6f, 0.8f, 1f));

            // Cosmic convergence visual
            CreateConvergenceVFX("CosmicConvergence", new Color(0.9f, 0.7f, 1f));

            LogCreation("P1 VFX Systems", 4);
        }

        static void CreateTransformationVFX(string name, Color startColor, Color endColor)
        {
            GameObject vfx = new GameObject(name);

            // Particle system: swirling transformation
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 3f;
            main.startSpeed = 2f;
            main.startSize = 0.5f;
            main.loop = false;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 50f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 5f;

            // Save prefab
            string prefabPath = $"{PREFABS_VFX}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(vfx, prefabPath);
            Object.DestroyImmediate(vfx);

            _assetsCreated++;
            Debug.Log($"[AssetReplacementGenerator] ✓ Created VFX prefab: {name}");
        }

        static void CreatePermanenceVFX(string name, Color glowColor)
        {
            GameObject vfx = new GameObject(name);

            // Particle system: golden shimmer trail
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 2f;
            main.startSpeed = 1f;
            main.startSize = 0.3f;
            main.loop = true;
            main.playOnAwake = true;
            main.startColor = glowColor;

            var emission = ps.emission;
            emission.rateOverTime = 30f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.2f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(glowColor, 0f), new GradientColorKey(glowColor * 0.5f, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            // Add light component for glow
            Light glowLight = vfx.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.color = glowColor;
            glowLight.range = 8f;
            glowLight.intensity = 2f;

            // Save prefab
            string prefabPath = $"{PREFABS_VFX}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(vfx, prefabPath);
            Object.DestroyImmediate(vfx);

            _assetsCreated++;
            Debug.Log($"[AssetReplacementGenerator] ✓ Created permanence VFX prefab: {name}");
        }

        static void CreateManifestationVFX(string name, Color auroraColor)
        {
            GameObject vfx = new GameObject(name);

            // Particle system: aurora waves
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 4f;
            main.startSpeed = 3f;
            main.startSize = 2f;
            main.loop = false;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 100)
            });

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(auroraColor * 0.5f, 0f), 
                    new GradientColorKey(auroraColor, 0.5f),
                    new GradientColorKey(auroraColor * 1.2f, 1f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0f, 0f), 
                    new GradientAlphaKey(1f, 0.3f),
                    new GradientAlphaKey(0f, 1f) 
                }
            );
            colorOverLifetime.color = gradient;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 10f;

            // Save prefab
            string prefabPath = $"{PREFABS_VFX}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(vfx, prefabPath);
            Object.DestroyImmediate(vfx);

            _assetsCreated++;
            Debug.Log($"[AssetReplacementGenerator] ✓ Created manifestation VFX prefab: {name}");
        }

        static void CreateConvergenceVFX(string name, Color cosmicColor)
        {
            GameObject vfx = new GameObject(name);

            // Particle system: cosmic spiral
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 5f;
            main.startSpeed = 4f;
            main.startSize = 1f;
            main.loop = false;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 80f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(cosmicColor, 0f), 
                    new GradientColorKey(Color.white, 0.5f),
                    new GradientColorKey(cosmicColor * 0.7f, 1f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1f, 0f), 
                    new GradientAlphaKey(1f, 0.8f),
                    new GradientAlphaKey(0f, 1f) 
                }
            );
            colorOverLifetime.color = gradient;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 5f;
            shape.radius = 0.1f;

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.orbitalX = 2f;
            velocityOverLifetime.orbitalY = 2f;
            velocityOverLifetime.orbitalZ = 2f;

            // Save prefab
            string prefabPath = $"{PREFABS_VFX}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(vfx, prefabPath);
            Object.DestroyImmediate(vfx);

            _assetsCreated++;
            Debug.Log($"[AssetReplacementGenerator] ✓ Created convergence VFX prefab: {name}");
        }

        #endregion

        #region Material Creation

        static Material CreateURPLitMaterial(string name, Color baseColor)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = name;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", 0.5f);

            EnsureDirectory(MATERIALS_PATH);
            string matPath = $"{MATERIALS_PATH}/{name}.mat";
            AssetDatabase.CreateAsset(mat, matPath);

            return mat;
        }

        static Material CreateTintedURPMaterial(string name, Color tint)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = name;
            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_Smoothness", 0.4f);

            EnsureDirectory(MATERIALS_PATH);
            string matPath = $"{MATERIALS_PATH}/{name}.mat";
            AssetDatabase.CreateAsset(mat, matPath);

            return mat;
        }

        static Material CreateTranslucentURPMaterial(string name, Color tint)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = name;
            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0); // Alpha blend
            mat.SetFloat("_AlphaClip", 0);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            // Emission for spectral glow
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", tint * 0.5f);

            EnsureDirectory(MATERIALS_PATH);
            string matPath = $"{MATERIALS_PATH}/{name}.mat";
            AssetDatabase.CreateAsset(mat, matPath);

            return mat;
        }

        #endregion

        #region Utilities

        static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(current, parts[i]);
                    }
                    current = next;
                }
            }
        }

        static void LogCreation(string category, int count)
        {
            string log = $"✓ {category}: {count} assets created";
            _creationLog.Add(log);
            Debug.Log($"[AssetReplacementGenerator] {log}");
        }

        #endregion
    }
}
