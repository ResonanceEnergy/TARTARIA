using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.Camera;

namespace Tartaria.Integration
{
    /// <summary>
    /// EchohavenContentSpawner -- populates the Echohaven zone with gameplay content
    /// that was missing from the bare scene: NPCs, enemies, collectibles, corruption
    /// zones, particle effects, ambient audio, and VFX event handlers.
    ///
    /// Fixes gaps: 1 (enemies), 6 (Milo), 7 (collectibles), 8 (env props),
    ///             9 (Cassian), 10 (corruption), 15 (ambient audio),
    ///             16 (particles), 17 (VFX events).
    ///
    /// Execution order -70: after BuildingSpawner (-80), before GameLoopController (-50).
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-70)]
    public class EchohavenContentSpawner : MonoBehaviour
    {
        public static EchohavenContentSpawner Instance { get; private set; }

        [Header("Spawn Positions")]
        [SerializeField] Vector3 miloSpawnOffset = new(2f, 0f, -1f);
        [SerializeField] Vector3 cassianPosition = new(-10f, 0f, 15f);

        [Header("Collectible Settings")]
        [SerializeField] float collectRadius = 2.5f;
        [SerializeField] float collectRSReward = 2f;

        [Header("KayKit Asset Prefabs (2026 AAA Quality)")]
        [SerializeField, Tooltip("KayKit shovel prefab — field tool for excavation")]
        GameObject kayKitShovelPrefab;
        [SerializeField, Tooltip("KayKit character for Milo companion")]
        GameObject kayKitMiloPrefab;
        [SerializeField, Tooltip("KayKit character for Cassian NPC")]
        GameObject kayKitCassianPrefab;
        [SerializeField, Tooltip("KayKit skeleton/enemy for MudGolem")]
        GameObject kayKitMudGolemPrefab;
        [SerializeField, Tooltip("KayKit character for Anastasia ghost")]
        GameObject kayKitAnastasiaPrefab;
        [SerializeField, Tooltip("KayKit rocks for environment scatter")]
        GameObject[] kayKitRockPrefabs;
        [SerializeField, Tooltip("KayKit grass/bushes for environment scatter")]
        GameObject[] kayKitFoliagePrefabs;

        [Header("Authoring Mode")]
        [SerializeField, Tooltip("Skip procedural spawn if scene already authored")]
        bool _sceneAlreadyAuthored = false;

        // M1: prevents duplicate intro sequence on reload / additive load races
        bool _contentSpawned;

        // Cached for VFX event wiring
        readonly List<GameObject> _aetherShards = new();
        readonly List<ParticleSystem> _environmentalVFX = new();

        // Round 4: MudGolem + Foliage pooling (builds on previous pooling work)
        readonly System.Collections.Generic.Queue<GameObject> _mudGolemPool = new();
        readonly System.Collections.Generic.Queue<GameObject> _foliagePool = new();
        const int MAX_GOLEM_POOL = 12;
        const int MAX_FOLIAGE_POOL = 60;

        // Round 4: Shared impostor quad mesh + low detail material for far LODs
        Mesh _impostorQuadMesh;
        Material _impostorMaterial;

        // Additional cached references
        GameObject _skyAurora;
        GameObject _vfxRoot;
        GameObject _cachedPlayer;
        GameObject _firstExcavationSite;
        GameObject _moonFramework;
        GameObject _worldBoundary;
        GameObject _shovelPickup;
        GameObject _digMoundsRoot;
        GameObject _ambientMotes;
        GameObject _foliageRoot;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Round 4 perf init: shared impostor resources (cheap quad billboard)
            EnsureImpostorResources();
        }

        void EnsureImpostorResources()
        {
            if (_impostorQuadMesh == null)
            {
                _impostorQuadMesh = new Mesh { name = "PerfImpostorQuad" };
                _impostorQuadMesh.vertices = new Vector3[] { new(-0.5f, 0, 0), new(0.5f, 0, 0), new(0.5f, 1f, 0), new(-0.5f, 1f, 0) };
                _impostorQuadMesh.uv = new Vector2[] { new(0,0), new(1,0), new(1,1), new(0,1) };
                _impostorQuadMesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
                _impostorQuadMesh.RecalculateBounds();
            }
            if (_impostorMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                _impostorMaterial = new Material(shader ?? Shader.Find("Unlit/Color")) { name = "PerfImpostorMat", color = new Color(0.25f, 0.2f, 0.15f, 1f) };
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            CancelInvoke(nameof(IntroduceMilo));     // Gap 2: prevent null-object call on early destroy
            CancelInvoke(nameof(IntroduceLirael));   // Gap 4: same safety for Lirael introduction
            StopAllCoroutines();                     // Safety for BeckonPlayerForward onboarding coroutine
            UnsubscribeVFXEvents();
        }

        void Start()
        {
            // Phase-0 guard: if scene already authored, skip procedural spawns
            if (_sceneAlreadyAuthored)
            {
                Debug.Log("[EchohavenContentSpawner] Scene marked as authored — skipping procedural content spawn.");
                // Still run animator fix for any runtime-spawned NPCs
                EnsureNPCAnimator(gameObject);
                return;
            }

            // M1 Stabilization: basic re-entrancy / double-Start guard (OnAfterLoad + GameLoop restore already handles companion/quest state)
            if (_contentSpawned) return;
            _contentSpawned = true;

            EnsureRuntimeVisuals();
            EnsureTraversalFreedom();
            EnsureGameplayMissingPieces();
            SpawnMilo();
            SpawnLirael();                                              // Gap 3: Lirael first appearance
            SpawnCassian();
            SpawnCollectibles();
            SpawnEnvironmentalProps();
            SpawnCorruptionZones();
            SpawnParticleEffects();
            SpawnAmbientAudio();
            // First-populate magical entry stinger (gentle 432Hz family for instant wonder on start volume enter) — slight delay for audio mgr + player presence
            StartCoroutine(PlayFirstPopulateStinger());
            SpawnInitialEnemies();                                      // Spawn 2 golems at game start
            SpawnAuroraPermanent();                                     // Aurora VFX in sky
            SetupEnemyWaveEncounters();
            RegisterEchohavenExcavationSites();                         // Gap 6: register dig sites
            PlaceDigSiteMarkers();                                      // Visual markers over dig sites
            SpawnAnastasia();                                           // Anastasia ghost companion
            SpawnKayKitScatter();                                       // KayKit rocks + foliage runtime scatter
            SubscribeVFXEvents();
            ActivateStartingQuest();                                    // Gap 11: activate first quest on HUD
            AdaptiveMusicController.Instance?.SetZone(0);              // Gap 14: Moon 1 zone music
            CompanionManager.Instance?.CheckUnlocks(0);                // Gap 25: companion unlock check

            EnsureMoon1LunarFramework();                               // M1: enable 5-beat lunar structure for Echohaven even without prior editor binder pass

            Debug.Log("[EchohavenContentSpawner] Zone content populated.");
        }

        /// <summary>
        /// M1 Decision: Enable the new Moon 1 5-beat lunar framework (Discovery→Restoration→Conflict→Climax→Revelation) at runtime.
        /// This gives the vertical slice authentic "13 Moons" calendar flavor without requiring a prior editor MoonFrameworkBinder pass.
        /// The MoonBeatRunner gracefully falls back if no full MoonDefinition is wired.
        /// </summary>
        void EnsureMoon1LunarFramework()
        {
            if (_moonFramework == null) _moonFramework = GameObject.Find("MoonFramework");
            if (_moonFramework != null) return;

            var root = new GameObject("MoonFramework (Moon1 Runtime)");
            var runner = root.AddComponent<MoonBeatRunner>();
            runner.autoStart = true;
            runner.startDelay = 2f;

            // Try to load the canonical Moon 01 definition via Resources (Addressables not referenced by this asmdef).
            // If missing the runner will log a warning and use time-based beats — sufficient for beta slice magic.
            var moon01 = Resources.Load<MoonDefinition>("Moons/Moon01_Echohaven_VerticalSlice");
            if (moon01 != null)
                runner.definition = moon01;

            Debug.Log("[Echohaven][Moon1] 5-beat lunar framework (MoonBeatRunner + events) is now live for the vertical slice.");
        }

        void EnsureRuntimeVisuals()
        {
            EnsurePlayerAnimatorPresent();
            EnsureAmbientMotes();
            EnsureRuntimeFoliage();
            EnsureSkyboxAndFog();
            EnsureBuildingVisualDetails();
        }

        void EnsureTraversalFreedom()
        {
            // Remove hard world blockers so the player can roam freely.
            if (_worldBoundary == null) _worldBoundary = GameObject.Find("WorldBoundary");
            if (_worldBoundary != null)
            {
                var cols = _worldBoundary.GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < cols.Length; i++)
                    cols[i].enabled = false;
            }

            // Older scenes contain Wall_* colliders outside WorldBoundary.
            string[] wallNames = { "Wall_North", "Wall_South", "Wall_East", "Wall_West" };
            for (int i = 0; i < wallNames.Length; i++)
            {
                var wall = GameObject.Find(wallNames[i]);
                if (wall == null) continue;
                var wc = wall.GetComponent<Collider>();
                if (wc != null) wc.enabled = false;
            }
        }

        void EnsureGameplayMissingPieces()
        {
            EnsureShovelPickup();
            EnsureMudDigVisuals();
            EnsureEnemyPresence();
            EnsureAnastasiaPresence();
        }

        void EnsureShovelPickup()
        {
            if (_shovelPickup == null) _shovelPickup = GameObject.Find("ShovelPickup");
            if (_shovelPickup != null) return;

            Vector3 spawn = new Vector3(12f, 1f, 7f);
            var playerSpawn = GameObject.Find("PlayerSpawn");
            if (playerSpawn != null)
                spawn = playerSpawn.transform.position + new Vector3(2.5f, 0.2f, 1.2f);

            GameObject root;
            if (kayKitShovelPrefab != null)
            {
                root = Instantiate(kayKitShovelPrefab);
                root.name = "ShovelPickup";
                root.transform.position = spawn + new Vector3(0f, 0.3f, 0f);
                root.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
                root.transform.localScale = Vector3.one * 1.2f;
            }
            else
            {
                // Fallback: procedural shovel if prefab missing
                root = new GameObject("ShovelPickup");
                root.transform.position = spawn;

                var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                handle.name = "Handle";
                handle.transform.SetParent(root.transform, false);
                handle.transform.localPosition = new Vector3(0f, 0.65f, 0f);
                handle.transform.localRotation = Quaternion.Euler(0f, 0f, 18f);
                handle.transform.localScale = new Vector3(0.06f, 0.65f, 0.06f);

                var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = "Blade";
                blade.transform.SetParent(root.transform, false);
                blade.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                blade.transform.localScale = new Vector3(0.26f, 0.18f, 0.10f);

                SetLitMaterial(handle, new Color(0.35f, 0.25f, 0.12f), 0.12f);
                SetEmissiveMaterial(blade, new Color(0.9f, 0.85f, 0.65f), 0.5f);
            }

            var c = root.GetComponent<SphereCollider>();
            if (c == null) c = root.AddComponent<SphereCollider>();
            c.isTrigger = true;
            c.radius = 1.2f;
            root.layer = LayerMask.NameToLayer("Interactable");
            if (root.layer < 0) root.layer = 0;

            var pickup = root.AddComponent<ShovelPickup>();
            pickup.displayName = "Field Shovel";

            AddNameplate(root, Tartaria.Input.InputPromptHelper.Localize("[E] Pick Up Shovel"), new Color(0.95f, 0.85f, 0.35f));
        }

        void EnsureMudDigVisuals()
        {
            if (_digMoundsRoot == null) _digMoundsRoot = GameObject.Find("--- DIG MOUNDS ---");
            if (_digMoundsRoot != null) return;

            var parent = new GameObject("--- DIG MOUNDS ---");
            Vector3[] centers =
            {
                new(30f, 0f, 20f),
                new(-20f, 0f, 35f),
                new(0f, 0f, -30f),
            };

            for (int c = 0; c < centers.Length; c++)
            {
                for (int i = 0; i < 4; i++)
                {
                    float ang = i * Mathf.PI * 0.5f;
                    float rad = 2.2f + i * 0.35f;
                    Vector3 pos = centers[c] + new Vector3(Mathf.Cos(ang) * rad, 0f, Mathf.Sin(ang) * rad);
                    float y = SampleGroundY(pos.x, pos.z);
                    if (!float.IsNaN(y)) pos.y = y;

                    var mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    mound.name = $"MudMound_{c}_{i}";
                    mound.transform.SetParent(parent.transform, false);
                    mound.transform.position = pos + new Vector3(0f, 0.3f, 0f);
                    mound.transform.localScale = new Vector3(2.0f, 0.8f, 2.0f);
                    SetLitMaterial(mound, new Color(0.24f, 0.17f, 0.10f), 0.06f);
                }
            }

            AddNameplate(parent, "Dig Sites", new Color(0.35f, 1f, 0.4f));
        }

        void EnsureEnemyPresence()
        {
            int existing = GameObject.FindObjectsByType<MudGolemHealth>(FindObjectsSortMode.None).Length;
            int needed = Mathf.Max(0, 4 - existing);
            if (needed <= 0) return;

            Vector3 center = new Vector3(20f, 0f, 10f);
            if (_cachedPlayer == null) _cachedPlayer = GameObject.FindWithTag("Player");
            var player = _cachedPlayer != null ? _cachedPlayer.gameObject : null;
            if (player != null) center = player.transform.position + new Vector3(12f, 0f, 10f);

            for (int i = 0; i < needed; i++)
            {
                float a = (Mathf.PI * 2f / Mathf.Max(needed, 1)) * i;
                SpawnMudGolem(center + new Vector3(Mathf.Cos(a) * 5f, 0f, Mathf.Sin(a) * 5f));
            }
        }

        void EnsureAnastasiaPresence()
        {
            if (AnastasiaController.Instance == null)
            {
                SpawnAnastasia();
                TriggerAnastasiaFirstAppearance();
                return;
            }

            if (_cachedPlayer == null) _cachedPlayer = GameObject.FindWithTag("Player")?.transform;
            var player = _cachedPlayer != null ? _cachedPlayer.gameObject : null;
            if (player == null) return;
            var a = AnastasiaController.Instance.transform;
            if (Vector3.Distance(a.position, player.transform.position) > 60f)
                a.position = player.transform.position + new Vector3(4f, 1.2f, 3f);
        }

        void EnsurePlayerAnimatorPresent()
        {
            if (_cachedPlayer == null) _cachedPlayer = GameObject.FindWithTag("Player")?.transform;
            var player = _cachedPlayer != null ? _cachedPlayer.gameObject : null;
            if (player == null) return;
            if (player.GetComponent<PlayerAnimator>() == null)
                player.AddComponent<PlayerAnimator>();
        }

        void EnsureAmbientMotes()
        {
            if (_ambientMotes == null) _ambientMotes = GameObject.Find("AmbientAetherMotes");
            if (_ambientMotes != null) return;

            var go = new GameObject("AmbientAetherMotes");
            go.transform.position = new Vector3(0f, 8f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = 8f;
            main.startSpeed = 0.35f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            main.startColor = new Color(0.7f, 0.9f, 1f, 0.7f);
            main.maxParticles = 500;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 90f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(140f, 24f, 140f);

            ps.Play(true);
            Debug.Log("[EchohavenContentSpawner] Runtime visual fallback: AmbientAetherMotes created.");
        }

        void EnsureRuntimeFoliage()
        {
            if (_foliageRoot == null) _foliageRoot = GameObject.Find("FoliageRoot");
            if (_foliageRoot != null) return;

            var root = new GameObject("FoliageRoot");
            var grassParent = new GameObject("Grass");
            grassParent.transform.SetParent(root.transform, false);

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var grassMat = shader != null ? new Material(shader) : null;
            if (grassMat != null)
            {
                grassMat.SetColor("_BaseColor", new Color(0.30f, 0.50f, 0.22f));
                grassMat.SetFloat("_Smoothness", 0.15f);
            }

            var rng = new System.Random(0xBADA55);
            const int grassCount = 700;
            const float half = 92f;

            for (int i = 0; i < grassCount; i++)
            {
                float x = ((float)rng.NextDouble() * 2f - 1f) * half;
                float z = ((float)rng.NextDouble() * 2f - 1f) * half;
                float y = SampleGroundY(x, z);
                if (float.IsNaN(y)) continue;

                var tuft = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tuft.name = "Grass";
                tuft.transform.SetParent(grassParent.transform, false);
                tuft.transform.position = new Vector3(x, y + 0.2f, z);
                float h = 0.25f + (float)rng.NextDouble() * 0.45f;
                tuft.transform.localScale = new Vector3(0.07f, h, 0.07f);
                tuft.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                var col = tuft.GetComponent<Collider>();
                if (col != null) Destroy(col);
                if (grassMat != null)
                {
                    var mr = tuft.GetComponent<MeshRenderer>();
                    if (mr != null) mr.material = grassMat;
                }
            }

            Debug.Log("[EchohavenContentSpawner] Runtime visual fallback: foliage scattered.");
        }

        void EnsureSkyboxAndFog()
        {
            // Don't clobber an HDRI skybox bound during build (Phase 13).
            var current = RenderSettings.skybox;
            bool hasHDRI = current != null && current.shader != null &&
                           current.shader.name == "Skybox/Cubemap" && current.HasProperty("_Tex") && current.GetTexture("_Tex") != null;
            if (!hasHDRI)
            {
                var shader = Shader.Find("Skybox/Procedural");
                if (shader != null)
                {
                    var sky = new Material(shader);
                    sky.SetFloat("_SunSize", 0.03f);
                    sky.SetFloat("_AtmosphereThickness", 1.2f);
                    sky.SetColor("_SkyTint", new Color(0.32f, 0.56f, 0.78f));
                    sky.SetColor("_GroundColor", new Color(0.48f, 0.40f, 0.30f));
                    sky.SetFloat("_Exposure", 1.15f);
                    RenderSettings.skybox = sky;
                }
            }
            else
            {
                Debug.Log("[EchohavenContentSpawner] HDRI skybox detected — preserving.");
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.64f, 0.72f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.40f, 0.33f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.22f, 0.18f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.58f, 0.54f, 0.46f);
            RenderSettings.fogDensity = 0.0045f;
        }

        void EnsureBuildingVisualDetails()
        {
            EnsureStarDomeDetails();
            EnsureFountainOrb();
            EnsureSpireCrystalCluster();
        }

        void EnsureStarDomeDetails()
        {
            var dome = FindFirst("StarDome_Placeholder", "Echohaven_StarDome", "Building_dome");
            if (dome == null) return;

            if (dome.transform.Find("Detail_AntennaSpire") == null)
            {
                var antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                antenna.name = "Detail_AntennaSpire";
                antenna.transform.SetParent(dome.transform, false);
                antenna.transform.localPosition = new Vector3(0f, 7.5f, 0f);
                antenna.transform.localScale = new Vector3(0.25f, 2.0f, 0.25f);
                SetEmissiveMaterial(antenna, new Color(0.45f, 0.85f, 1f), 1.6f);
            }

            for (int i = 0; i < 6; i++)
            {
                string n = $"Detail_Buttress_{i}";
                if (dome.transform.Find(n) != null) continue;
                float t = (Mathf.PI * 2f / 6f) * i;
                var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                b.name = n;
                b.transform.SetParent(dome.transform, false);
                b.transform.localPosition = new Vector3(Mathf.Cos(t) * 4.5f, 1.8f, Mathf.Sin(t) * 4.5f);
                b.transform.localRotation = Quaternion.Euler(0f, -t * Mathf.Rad2Deg, 0f);
                b.transform.localScale = new Vector3(0.6f, 3.2f, 1.8f);
                SetLitMaterial(b, new Color(0.58f, 0.55f, 0.50f), 0.25f);
            }
        }

        void EnsureFountainOrb()
        {
            var fountain = FindFirst("HarmonicFountain_Placeholder", "Echohaven_HarmonicFountain", "Building_fountain");
            if (fountain == null) return;
            if (fountain.transform.Find("Detail_OrbFinial") != null) return;

            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Detail_OrbFinial";
            orb.transform.SetParent(fountain.transform, false);
            orb.transform.localPosition = new Vector3(0f, 4.2f, 0f);
            orb.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            SetEmissiveMaterial(orb, new Color(1f, 0.82f, 0.42f), 1.8f);
        }

        void EnsureSpireCrystalCluster()
        {
            var spire = FindFirst("CrystalSpire_Placeholder", "Echohaven_CrystalSpire", "Building_spire");
            if (spire == null) return;
            if (spire.transform.Find("Detail_CrystalCluster") != null) return;

            var cluster = new GameObject("Detail_CrystalCluster");
            cluster.transform.SetParent(spire.transform, false);
            cluster.transform.localPosition = new Vector3(0f, 8f, 0f);

            for (int i = 0; i < 5; i++)
            {
                var shard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shard.name = $"Shard_{i}";
                shard.transform.SetParent(cluster.transform, false);
                float a = (Mathf.PI * 2f / 5f) * i;
                shard.transform.localPosition = new Vector3(Mathf.Cos(a) * 0.5f, 0.25f, Mathf.Sin(a) * 0.5f);
                shard.transform.localScale = new Vector3(0.14f, 0.9f + 0.3f * (i % 2), 0.14f);
                shard.transform.localRotation = Quaternion.Euler(10f + i * 7f, -a * Mathf.Rad2Deg, 0f);
                SetEmissiveMaterial(shard, new Color(0.75f, 0.55f, 1f), 1.5f);
            }
        }

        static GameObject FindFirst(params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                var go = GameObject.Find(names[i]);
                if (go != null) return go;
            }
            return null;
        }

        static void SetLitMaterial(GameObject go, Color baseColor, float smoothness)
        {
            var mr = go.GetComponent<MeshRenderer>();
            if (mr == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) return;
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            mr.material = mat;
        }

        static void SetEmissiveMaterial(GameObject go, Color emissive, float intensity)
        {
            var mr = go.GetComponent<MeshRenderer>();
            if (mr == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) return;
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", emissive * 0.35f);
            mat.SetFloat("_Smoothness", 0.65f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissive * intensity);
            mr.material = mat;

            var l = go.GetComponent<Light>();
            if (l == null) l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = emissive;
            l.intensity = 2.4f;
            l.range = 10f;
            l.shadows = LightShadows.None;
        }

        float SampleGroundY(float x, float z)
        {
            var origin = new Vector3(x, 200f, z);
            int mask = ~((1 << 8) | (1 << 10) | (1 << 11));
            if (Physics.Raycast(origin, Vector3.down, out var hit, 500f, mask, QueryTriggerInteraction.Ignore))
                return hit.point.y;
            return float.NaN;
        }

        void ActivateStartingQuest()
        {
            var qm = QuestManager.Instance;
            if (qm == null) return;

            // Activate the first Echohaven quest so player sees it on HUD immediately
            qm.ActivateQuest("echohaven_awakening");

            // Populate the HUD objective panel with the active quest title
            var def = qm.GetQuestDefinition("echohaven_awakening");
            if (def != null)
                UI.HUDController.Instance?.ShowObjective($"QUEST: {def.displayName}");

            // === 5-BEAT OBJECTIVE FLOW KICKOFF (UI FTUE) — guided magical start for first 5-10 min ===
            // Explicitly surface Discovery beat immediately so player feels the lunar structure from second 1.
            UI.HUDController.Instance?.ShowObjective("MOON 01 — DISCOVERY: The valley hums. Follow the glow to the first buried chord.");
            Tartaria.UI.MoonHUDBanner.Show("MOON 01 — DISCOVERY", "Echohaven remembers. The first light calls you home.", new Color(0.55f, 0.85f, 1f, 1f), 5f);

            Debug.Log("[EchohavenContentSpawner] Starting quest + 5-beat Discovery objective activated — awakening flow primed.");
        }

        // ─── Milo Companion (Gap 6) ─────────────────

        void SpawnMilo()
        {
            if (MiloController.Instance != null) return; // Already exists

            var playerSpawn = GameObject.Find("PlayerSpawn");
            Vector3 spawnPos = playerSpawn != null
                ? playerSpawn.transform.position + miloSpawnOffset
                : new Vector3(12f, 1f, 4f);

            // Use KayKit prefab if assigned (2026 AAA quality)
            GameObject miloGO = null;
            if (kayKitMiloPrefab != null)
            {
                miloGO = Instantiate(kayKitMiloPrefab, spawnPos, Quaternion.identity);
                miloGO.transform.localScale = Vector3.one * 0.85f;
                EnsureNPCAnimator(miloGO);
                Debug.Log("[EchohavenContentSpawner] Milo spawned from KayKit prefab.");
            }
            else
            {
                // Fallback: primitives if prefab not assigned
                miloGO = CreateMiloFallback(spawnPos);
                Debug.LogWarning("[EchohavenContentSpawner] Milo spawned from primitive fallback — assign kayKitMiloPrefab for AAA quality.");
            }

            miloGO.name = "Milo";

            // Ensure MiloController
            if (miloGO.GetComponent<MiloController>() == null)
                miloGO.AddComponent<MiloController>();

            // Auto-introduce after a short delay
            Invoke(nameof(IntroduceMilo), 3f);

            Debug.Log($"[EchohavenContentSpawner] Milo spawned at {spawnPos}");
        }

        void IntroduceMilo()
        {
            MiloController.Instance?.Introduce();
            // Moon 1 Echohaven onboarding: start early trust arc on first meeting (per 27_TUTORIAL + 03_CAMPAIGN)
            CompanionManager.Instance?.AddTrust("milo", 10f);

            // Milo discovery reaction stinger (rich Moon1_MiloDiscovery warm 432+chime for first companion "we're not alone" magical moment on populate flow)
            if (MiloController.Instance != null)
            {
                AudioManager.Instance?.PlaySFX("Moon1_MiloDiscovery", MiloController.Instance.transform.position, 0.45f);
                AudioManager.Instance?.PlaySFX("Discovery", MiloController.Instance.transform.position, 0.28f);
                // Soft 432 + PHI tone for companion "awakening" resonance sync
                AudioManager.Instance?.PlayTone(432f, 0.55f, 0.18f);
                AudioManager.Instance?.PlayTone(540f, 0.32f, 0.11f);
            }

            // Gentle movement teaching cue (social follow) — non-blocking
            if (MiloController.Instance != null)
            {
                // Milo beckons ahead to teach WASD by example (matches GDD first-arrival beat)
                StartCoroutine(BeckonPlayerForward());
            }
        }

        System.Collections.IEnumerator BeckonPlayerForward()
        {
            yield return new WaitForSeconds(1.5f);
            var milo = MiloController.Instance;
            if (milo == null) yield break;
            // Simple visual cue: face player + short forward step (no nav required)
            if (_cachedPlayer == null) _cachedPlayer = GameObject.FindWithTag("Player");
            var player = _cachedPlayer != null ? _cachedPlayer.gameObject : null;
            if (player != null)
            {
                Vector3 toPlayer = (player.transform.position - milo.transform.position).normalized;
                milo.transform.rotation = Quaternion.LookRotation(-toPlayer); // face player
            }
            // Small step ahead to invite follow (teaches movement without text wall)
            milo.transform.position += milo.transform.forward * 4f;
            Debug.Log("[Echohaven Onboarding] Milo beckoned forward to teach movement by social cue.");
        }

        // ─── Lirael Companion (Gap 3: Lirael first appearance Moon 1) ─

        void SpawnLirael()
        {
            // Lirael is an Echo — no physical prefab needed at introduction.
            // She manifests near the first Aether node after the player settles in.
            Invoke(nameof(IntroduceLirael), 20f);
            Debug.Log("[EchohavenContentSpawner] Lirael introduction scheduled (20s delay).");
        }

        void IntroduceLirael()
        {
            if (LiraelController.Instance == null)
            {
                Debug.LogWarning("[EchohavenContentSpawner] IntroduceLirael: LiraelController.Instance is null — Lirael not yet spawned.");
                return;
            }
            LiraelController.Instance.Introduce();
            LiraelController.Instance.AddTrust(1f);   // Seed initial Moon 1 trust on first contact
        }

        GameObject CreateMiloFallback(Vector3 pos)
        {
            var root = new GameObject("Milo");
            root.transform.position = pos;

            // Body (small sphere)
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            SetMiloPrimitiveMaterial(body);

            // Head (smaller sphere)
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform);
            head.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            SetMiloPrimitiveMaterial(head);

            // Glow
            var light = root.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.4f, 0.7f, 1f);
            light.intensity = 1.5f;
            light.range = 5f;
            light.shadows = LightShadows.None;

            // Interaction trigger so player can press E to talk to Milo.
            // Without this, MiloController exists but has no way to be reached
            // by raycast/proximity from PlayerInputHandler.
            var col = root.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 0.7f, 0f);
            col.height = 1.5f;
            col.radius = 0.6f;
            int layer = LayerMask.NameToLayer("Interactable");
            root.layer = layer >= 0 ? layer : 0;
            root.AddComponent<MiloInteractable>();

            AddNameplate(root, Tartaria.Input.InputPromptHelper.Localize("[E] Talk to Milo"), new Color(0.5f, 0.8f, 1f));

            return root;
        }

        void SetMiloPrimitiveMaterial(GameObject go)
        {
            var r = go.GetComponent<MeshRenderer>();
            if (r == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", new Color(0.3f, 0.5f, 0.8f));
            mat.SetFloat("_Smoothness", 0.7f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.2f, 0.4f, 0.8f) * 0.5f);
            r.material = mat;
        }

        // ─── Cassian NPC (Gap 9) ────────────────────

        void SpawnCassian()
        {
            if (CassianNPCController.Instance != null) return;

            GameObject cassianGO;
            if (kayKitCassianPrefab != null)
            {
                cassianGO = Instantiate(kayKitCassianPrefab, cassianPosition, Quaternion.identity);
                cassianGO.name = "Cassian";
                cassianGO.transform.localScale = Vector3.one * 1.1f;
                EnsureNPCAnimator(cassianGO);
                Debug.Log("[EchohavenContentSpawner] Cassian spawned from KayKit prefab.");
            }
            else
            {
                // Fallback: primitive robed figure
                cassianGO = new GameObject("Cassian");
                cassianGO.transform.position = cassianPosition;

                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(cassianGO.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);

            var r = body.GetComponent<MeshRenderer>();
            if (r != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader != null)
                {
                    var mat = new Material(shader);
                    mat.SetColor("_BaseColor", new Color(0.2f, 0.15f, 0.3f)); // Dark purple robe
                    mat.SetFloat("_Smoothness", 0.3f);
                    r.material = mat;
                }
            }

            // Hood (sphere on top)
            var hood = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hood.name = "Hood";
            hood.transform.SetParent(cassianGO.transform);
            hood.transform.localPosition = new Vector3(0f, 2.3f, 0f);
            hood.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            var hoodR = hood.GetComponent<MeshRenderer>();
            if (hoodR != null && r != null)
                hoodR.material = r.material;

            // NPC interaction collider
            var col = cassianGO.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1.2f, 0f);
            col.height = 2.8f;
            col.radius = 0.6f;

            // Set Interactable layer
            int interactLayer = LayerMask.NameToLayer("Interactable");
            if (interactLayer >= 0)
            {
                cassianGO.layer = interactLayer;
                body.layer = interactLayer;
                hood.layer = interactLayer;
            }

            // Subtle aura light
            var light = cassianGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.5f, 0.3f, 0.7f);
            light.intensity = 1.5f;
            light.range = 6f;
            light.shadows = LightShadows.None;

            cassianGO.AddComponent<CassianNPCController>();
            CompanionManager.Instance?.UnlockCompanion("cassian");  // Gap 18: register with companion system

            // Floating name marker
            AddNameplate(cassianGO, "Cassian", new Color(0.6f, 0.4f, 0.9f));

            Debug.Log($"[EchohavenContentSpawner] Cassian NPC spawned at {cassianPosition}");
            }
        }

        // ─── Collectible Aether Shards (Gap 7) ──────

        void SpawnCollectibles()
        {
            Vector3[] shardPositions =
            {
                new(15f, 0.8f, 10f),
                new(-5f, 0.8f, 20f),
                new(20f, 0.8f, -15f),
                new(-15f, 0.8f, -10f),
                new(5f, 0.8f, 30f),
                new(40f, 0.8f, 15f),
                new(-25f, 0.8f, 25f),
                new(10f, 0.8f, -25f),
            };

            foreach (var pos in shardPositions)
            {
                var shard = CreateAetherShard(pos);
                _aetherShards.Add(shard);
            }

            Debug.Log($"[EchohavenContentSpawner] {shardPositions.Length} Aether Shards placed.");
        }

        GameObject CreateAetherShard(Vector3 pos)
        {
            pos = ResolveShardSpawnPosition(pos);

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "AetherShard";
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
            go.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

            // Make it a trigger
            var col = go.GetComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(collectRadius / 0.3f, collectRadius / 0.5f, collectRadius / 0.3f);

            // Crystal material
            var r = go.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetColor("_BaseColor", new Color(0.2f, 0.5f, 0.9f, 0.7f));
                mat.SetFloat("_Smoothness", 0.9f);
                mat.SetFloat("_Metallic", 0.3f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.2f, 0.5f, 1f) * 2f);
                // Transparent
                mat.SetFloat("_Surface", 1f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                r.material = mat;
            }

            // Point light for visibility
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.3f, 0.6f, 1f);
            light.intensity = 2f;
            light.range = 5f;
            light.shadows = LightShadows.None;

            // Bobbing
            go.AddComponent<BobbingMarker>();

            // Trigger handler
            go.AddComponent<AetherShardPickup>().rsReward = collectRSReward;

            return go;
        }

        Vector3 ResolveShardSpawnPosition(Vector3 requested)
        {
            // Keep shard positions collectible on uneven procedural terrain.
            int excludeLayers = (1 << 8) | (1 << 10) | (1 << 11); // Building, Player, Trigger
            int groundMask = Physics.DefaultRaycastLayers & ~excludeLayers;
            Vector3 origin = new Vector3(requested.x, 100f, requested.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 300f, groundMask, QueryTriggerInteraction.Ignore))
            {
                var snapped = new Vector3(requested.x, hit.point.y + 0.9f, requested.z);
                Debug.Log($"[EchohavenContentSpawner] Shard spawn snapped {requested} -> {snapped}");
                return snapped;
            }

            return requested;
        }

        // ─── Environmental Props (Gap 8) ─────────────

        void SpawnEnvironmentalProps()
        {
            var parent = new GameObject("--- ENV PROPS ---");

            // Ruined columns scattered around the plaza
            CreateRuinedColumn(parent.transform, new Vector3(8f, 0f, -5f), 3.5f);
            CreateRuinedColumn(parent.transform, new Vector3(-8f, 0f, 8f), 2.8f);
            CreateRuinedColumn(parent.transform, new Vector3(12f, 0f, 12f), 4.2f);
            CreateRuinedColumn(parent.transform, new Vector3(-6f, 0f, -12f), 3.0f);

            // Fallen rubble piles
            CreateRubblePile(parent.transform, new Vector3(18f, 0f, 5f));
            CreateRubblePile(parent.transform, new Vector3(-12f, 0f, -8f));
            CreateRubblePile(parent.transform, new Vector3(3f, 0f, 18f));

            // Ancient inscription stones
            CreateInscriptionStone(parent.transform, new Vector3(0f, 0f, 8f));
            CreateInscriptionStone(parent.transform, new Vector3(-18f, 0f, 20f));

            Debug.Log("[EchohavenContentSpawner] Environmental props placed.");
        }

        void CreateRuinedColumn(Transform parent, Vector3 pos, float height)
        {
            var col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            col.name = "RuinedColumn";
            col.transform.SetParent(parent);
            col.transform.position = pos + new Vector3(0f, height * 0.5f, 0f);
            col.transform.localScale = new Vector3(0.8f, height, 0.8f);
            col.isStatic = true;

            var r = col.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetColor("_BaseColor", new Color(0.65f, 0.6f, 0.5f));
                mat.SetFloat("_Smoothness", 0.2f);
                r.material = mat;
            }

            // Slight tilt for ruined look
            col.transform.rotation = Quaternion.Euler(
                Random.Range(-8f, 8f), Random.Range(0f, 360f), Random.Range(-5f, 5f));
        }

        void CreateRubblePile(Transform parent, Vector3 pos)
        {
            var pile = new GameObject("RubblePile");
            pile.transform.SetParent(parent);
            pile.transform.position = pos;
            pile.isStatic = true;

            for (int i = 0; i < 4; i++)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = $"Rock_{i}";
                rock.transform.SetParent(pile.transform);
                float s = Random.Range(0.3f, 0.8f);
                rock.transform.localPosition = new Vector3(
                    Random.Range(-1f, 1f), s * 0.4f, Random.Range(-1f, 1f));
                rock.transform.localScale = new Vector3(s, s * 0.6f, s);
                rock.isStatic = true;

                var r = rock.GetComponent<MeshRenderer>();
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader != null)
                {
                    var mat = new Material(shader);
                    float g = Random.Range(0.3f, 0.5f);
                    mat.SetColor("_BaseColor", new Color(g + 0.1f, g, g - 0.05f));
                    mat.SetFloat("_Smoothness", 0.1f);
                    r.material = mat;
                }
            }
        }

        void CreateInscriptionStone(Transform parent, Vector3 pos)
        {
            var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.name = "InscriptionStone";
            stone.transform.SetParent(parent);
            stone.transform.position = pos + new Vector3(0f, 0.4f, 0f);
            stone.transform.localScale = new Vector3(1.5f, 0.8f, 0.3f);
            stone.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            stone.isStatic = true;

            var r = stone.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetColor("_BaseColor", new Color(0.55f, 0.5f, 0.45f));
                mat.SetFloat("_Smoothness", 0.35f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.3f, 0.25f, 0.1f) * 0.2f);
                r.material = mat;
            }
        }

        // ─── Corruption Zones (Gap 10) ──────────────

        void SpawnCorruptionZones()
        {
            // Place corruption zones near mud mounds — they're no longer just cosmetic
            Vector3[] corruptionCenters =
            {
                new(25f, 0f, 14f),   // Near dome mud mound 0
                new(-15f, 0f, 28f),  // Near fountain mud mound 2
                new(5f, 0f, -24f),   // Near spire mud mound 3
            };
            string[] buildingIds = { "dome", "fountain", "spire" };

            var corruption = CorruptionSystem.Instance;
            for (int i = 0; i < corruptionCenters.Length; i++)
            {
                // Visual: dark pulsing ground plane
                var zone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                zone.name = $"CorruptionZone_{buildingIds[i]}";
                zone.transform.position = corruptionCenters[i];
                zone.transform.localScale = new Vector3(8f, 0.05f, 8f);

                var r = zone.GetComponent<MeshRenderer>();
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader != null)
                {
                    var mat = new Material(shader);
                    mat.SetColor("_BaseColor", new Color(0.15f, 0.08f, 0.2f, 0.6f));
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(0.3f, 0.05f, 0.15f) * 0.5f);
                    mat.SetFloat("_Surface", 1f);
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    r.material = mat;
                }

                // Remove physics collider (visual only, CorruptionSystem handles logic)
                var col = zone.GetComponent<Collider>();
                if (col != null) Destroy(col);

                // Seed corruption data in the system
                if (corruption != null)
                    corruption.ApplyCorruption(buildingIds[i], 20f);
            }

            Debug.Log("[EchohavenContentSpawner] 3 corruption zones seeded.");
        }

        // ─── Particle Effects (Gap 16) ───────────────

        void SpawnParticleEffects()
        {
            // Aether wisps floating near buildings
            CreateAetherWisps(new Vector3(30f, 3f, 20f), "Wisps_Dome");
            CreateAetherWisps(new Vector3(-20f, 3f, 35f), "Wisps_Fountain");
            CreateAetherWisps(new Vector3(0f, 5f, -30f), "Wisps_Spire");

            // Dust motes in the central plaza
            CreateDustMotes(new Vector3(0f, 2f, 0f), "Dust_Plaza");

            // Spawn Aurora VFX in sky (permanent ambient effect)
            SpawnAuroraVFX();

            // Trigger periodic scan pulse at player position for visual interest
            InvokeRepeating(nameof(TriggerAmbientScanPulse), 5f, 10f);

            Debug.Log("[EchohavenContentSpawner] Particle effects spawned (wisps, dust, aurora, periodic scan).");
        }

        void SpawnAuroraPermanent()
        {
            // Idempotent: don't spawn if already exists
            if (_skyAurora == null) _skyAurora = GameObject.Find("Sky_Aurora");
            if (_skyAurora != null) return;

            // Ensure VFX parent exists
            if (_vfxRoot == null) _vfxRoot = GameObject.Find("VFX");
            if (_vfxRoot == null)
            {
                _vfxRoot = new GameObject("VFX");
            }

            // Load Aurora prefab from Resources
            var auroraPrefab = Resources.Load<GameObject>("VFX/Aurora");
            if (auroraPrefab == null)
            {
                Debug.LogWarning("[EchohavenContentSpawner] Aurora prefab not found in Resources/VFX/Aurora");
                return;
            }

            // Spawn at high altitude (y=200)
            var aurora = Instantiate(auroraPrefab, new Vector3(0, 200, 0), Quaternion.identity);
            aurora.name = "Sky_Aurora";
            aurora.transform.SetParent(_vfxRoot.transform, true);
            _skyAurora = aurora;

            _environmentalVFX.AddRange(aurora.GetComponentsInChildren<ParticleSystem>());
            Debug.Log("[EchohavenContentSpawner] Aurora VFX spawned in sky at (0, 200, 0).");
        }

        void SpawnAuroraVFX()
        {
            #if UNITY_EDITOR
            var auroraPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/VFX/Aurora.prefab");
            if (auroraPrefab != null)
            {
                var aurora = Instantiate(auroraPrefab, new Vector3(0, 50, 0), Quaternion.identity);
                aurora.name = "Sky_Aurora";
                _environmentalVFX.AddRange(aurora.GetComponentsInChildren<ParticleSystem>());
                Debug.Log("[EchohavenContentSpawner] Aurora VFX spawned in sky.");
            }
            else
            {
                Debug.LogWarning("[EchohavenContentSpawner] Aurora.prefab not found at Assets/_Project/Prefabs/VFX/Aurora.prefab");
            }
            #endif
        }

        void TriggerAmbientScanPulse()
        {
            if (_cachedPlayer == null) _cachedPlayer = GameObject.FindWithTag("Player");
            var player = _cachedPlayer;
            if (player == null) return;

            #if UNITY_EDITOR
            var scanPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/VFX/ScanPulse.prefab");
            if (scanPrefab != null)
            {
                Instantiate(scanPrefab, player.transform.position, Quaternion.identity);
            }
            #endif
        }

        void CreateAetherWisps(Vector3 pos, string name)
        {
            var go = new GameObject(name);
            go.transform.position = pos;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 20;
            main.startLifetime = 4f;
            main.startSpeed = 0.3f;
            main.startSize = 0.15f;
            main.startColor = new Color(0.3f, 0.6f, 1f, 0.5f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.05f; // Float upward

            var emission = ps.emission;
            emission.rateOverTime = 5f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 5f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(new Color(0.3f, 0.6f, 1f), 0f),
                        new GradientColorKey(new Color(0.5f, 0.8f, 1f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.5f, 0.3f),
                        new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            // URP particle material
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetColor("_BaseColor", new Color(0.4f, 0.7f, 1f, 0.4f));
                mat.SetFloat("_Surface", 1f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                renderer.material = mat;
            }

            _environmentalVFX.Add(ps);
        }

        void CreateDustMotes(Vector3 pos, string name)
        {
            var go = new GameObject(name);
            go.transform.position = pos;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 40;
            main.startLifetime = 6f;
            main.startSpeed = 0.1f;
            main.startSize = 0.08f;
            main.startColor = new Color(0.7f, 0.65f, 0.5f, 0.3f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(15f, 3f, 15f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.5f;
            noise.frequency = 0.3f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetColor("_BaseColor", new Color(0.8f, 0.75f, 0.6f, 0.3f));
                mat.SetFloat("_Surface", 1f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                renderer.material = mat;
            }

            _environmentalVFX.Add(ps);
        }

        // ─── Ambient Audio (Gap 15) ──────────────────

        void SpawnAmbientAudio()
        {
            // Wind ambience — central (gentle start volume)
            CreateAmbientSource("Ambient_Wind", Vector3.zero, "Wind", 0.18f, 80f);

            // Deep hum near buildings — low frequency resonance
            CreateAmbientSource("Ambient_Hum_Dome", new Vector3(30f, 2f, 20f), "DeepHum", 0.12f, 25f);
            CreateAmbientSource("Ambient_Hum_Fountain", new Vector3(-20f, 2f, 35f), "WaterAmbient", 0.14f, 20f);
            CreateAmbientSource("Ambient_Hum_Spire", new Vector3(0f, 5f, -30f), "CrystalHum", 0.12f, 25f);

            // ─── Rich Moon 1 Echohaven zone audio additions (gentle buried resonance hum, corruption drone, ambient wind/motes) ───
            // Immediate magical feel on first populate + start volume (lightweight, existing ProceduralSFX + fallback gen with 432Hz family tie-in)
            if (_firstExcavationSite == null) _firstExcavationSite = GameObject.Find("Moon1_FirstExcavationSite");
            Vector3 buriedPos = _firstExcavationSite != null ? _firstExcavationSite.transform.position + new Vector3(0, 1.1f, 0) : new Vector3(8f, 1.1f, 5f);
            CreateAmbientSource("BuriedResonanceHum", buriedPos, "Moon1_BuriedResonanceHum", 0.12f, 32f);  // rich dedicated 432+PHI buried hum at obvious ruin

            // Corruption drones (dissonant low layers near patches/zones for unsettling contrast) — use dedicated Moon1 drone
            CreateAmbientSource("CorruptionDrone1", new Vector3(-8f, 0.6f, 22f), "Moon1_CorruptionDrone", 0.075f, 20f);
            CreateAmbientSource("CorruptionDrone2", new Vector3(12f, 0.6f, 21f), "Moon1_CorruptionDrone", 0.07f, 20f);
            CreateAmbientSource("CorruptionDrone3", new Vector3(2f, 0.6f, 28f), "Moon1_CorruptionDrone", 0.065f, 18f);

            // Ambient wind/motes ethereal layer (high soft harmonics for floating motes feel) — use dedicated Moon1 motes
            CreateAmbientSource("MotesWindAmbience", new Vector3(1f, 5.5f, -1f), "Moon1_EtherealMotes", 0.065f, 42f);

            Debug.Log("[EchohavenContentSpawner] Ambient audio sources placed (rich Echohaven zone layers: buried hum + corruption drones + motes wind, all gentle start volumes 0.07-0.18).");
        }

        void CreateAmbientSource(string name, Vector3 pos, string sfxName, float volume, float range)
        {
            var go = new GameObject(name);
            go.transform.position = pos;

            var src = go.AddComponent<AudioSource>();
            src.spatialBlend = pos == Vector3.zero ? 0f : 1f; // 2D for wind, 3D for localized
            src.loop = true;
            src.volume = volume;
            src.maxDistance = range;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.playOnAwake = false;

            // Try to get clip from AudioManager
            var clip = ProceduralSFXLibrary.Get(sfxName);
            if (clip != null)
            {
                src.clip = clip;
                src.Play();
            }
            else
            {
                // Procedural fallback: synthesize a low drone matching the requested ambience type
                src.clip = GenerateAmbientTone(sfxName);
                src.Play();
            }
        }

        AudioClip GenerateAmbientTone(string name)
        {
            int sampleRate = 44100;
            int duration = 6; // seconds, longer seamless loop
            int sampleCount = sampleRate * duration;
            var samples = new float[sampleCount];

            float freq = name switch
            {
                "Wind" => 95f,
                "DeepHum" => 52f,
                "WaterAmbient" => 175f,
                "CrystalHum" => 432f,
                "BuriedResonanceHum" => 108f,
                _ when name.Contains("Corruption") || name.Contains("Drone") => 47f,
                _ when name.Contains("Mote") => 648f,
                _ => 100f
            };

            bool isCorrupt = name.Contains("Corruption") || name.Contains("Drone") || freq < 60f;
            bool isHigh = name.Contains("Mote") || freq > 400f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float f = freq;

                // Core drone + rich 432Hz family overtones (magical Tartaria signature, lightweight)
                float s = Mathf.Sin(2f * Mathf.PI * f * t) * 0.11f
                        + Mathf.Sin(2f * Mathf.PI * f * 1.5f * t) * 0.055f
                        + Mathf.Sin(2f * Mathf.PI * 432f * t) * 0.035f;   // tie every zone ambient to 432 family

                if (isCorrupt)
                {
                    // Dissonant tritone corruption drone (unsettling but gentle)
                    s += Mathf.Sin(2f * Mathf.PI * f * 1.414f * t) * 0.065f;
                    s *= (0.65f + 0.35f * Mathf.Sin(t * 0.35f)); // slow organic pulse
                }
                else if (isHigh)
                {
                    // Ethereal mote/wind high layer — soft celestial partials
                    s += Mathf.Sin(2f * Mathf.PI * 1296f * t) * 0.022f;
                    s *= (0.8f + 0.2f * Mathf.Sin(t * 1.2f + 1.7f));
                }
                else
                {
                    // Buried / resonance gentle — warm PHI harmonic bloom
                    s += Mathf.Sin(2f * Mathf.PI * 432f * 0.5f * t) * 0.04f;
                    s += Mathf.Sin(2f * Mathf.PI * f * 1.618f * t) * 0.03f;
                }

                // Soft fade for loop seamlessness + start volume friendly
                float env = Mathf.Min(t, duration - t, 0.8f) / 0.8f;
                samples[i] = s * env * 0.85f;
            }

            var clip = AudioClip.Create($"Ambient_{name}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        System.Collections.IEnumerator PlayFirstPopulateStinger()
        {
            yield return new WaitForSeconds(1.2f);
            var am = AudioManager.Instance;
            if (am != null)
            {
                am.PlaySFX2D("Moon1_ScanStinger", 0.24f);  // rich 432+PHI magical entry stinger for first populate wonder
                am.PlaySFX2D("TuneLock", 0.15f);
                am.PlayTone(432f, 0.65f, 0.10f);
                am.PlayTone(432f * 1.618f, 0.38f, 0.07f); // PHI family layer
            }
        }

        // ─── Enemy Wave Encounters (Gap 1) ───────────

        void SetupEnemyWaveEncounters()
        {
            var waveManager = CombatWaveManager.Instance;
            if (waveManager == null) return;

            // Build wave encounter definitions that ProximityTrigger can reference
            // ProximityTrigger at RS thresholds already handles spawning via enemyPrefab
            // We pre-register encounters so the system knows about them

            // Store encounter defs for later use when RS thresholds are reached
            _rs25Encounter = CombatWaveManager.BuildZoneEncounter(0, "echohaven_rs25");
            _rs50Encounter = CombatWaveManager.BuildZoneEncounter(0, "echohaven_rs50");
            _rs75Encounter = CombatWaveManager.BuildZoneEncounter(0, "echohaven_rs75");

            // Wire RS change listener to trigger encounters
            GameEvents.OnRSChanged += HandleRSChangedForEncounters;

            Debug.Log("[EchohavenContentSpawner] Enemy wave encounters configured.");
        }

        WaveEncounterDef _rs25Encounter;
        WaveEncounterDef _rs50Encounter;
        WaveEncounterDef _rs75Encounter;
        bool _rs25Triggered, _rs50Triggered, _rs75Triggered;

        void HandleRSChangedForEncounters(float newRS)
        {
            var waveManager = CombatWaveManager.Instance;
            if (waveManager == null) return;

            AchievementSystem.Instance?.CheckZoneRS(newRS);         // Gap 20: achievement zone RS check

            if (!_rs25Triggered && newRS >= 25f)
            {
                _rs25Triggered = true;
                waveManager.StartEncounter(_rs25Encounter, new Vector3(40f, 0f, 30f));
                SpawnEnemyGroup(new Vector3(40f, 0f, 30f), 3);
                AdaptiveMusicController.Instance?.EnterCombat();            // Gap 16: combat music
                HapticFeedbackManager.Instance?.PlayBuildingEmergence();    // Gap 22: wave haptic
            }
            if (!_rs50Triggered && newRS >= 50f)
            {
                _rs50Triggered = true;
                waveManager.StartEncounter(_rs50Encounter, new Vector3(-35f, 0f, 45f));
                SpawnEnemyGroup(new Vector3(-35f, 0f, 45f), 5);
                AdaptiveMusicController.Instance?.EnterCombat();
                HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            }
            if (!_rs75Triggered && newRS >= 75f)
            {
                _rs75Triggered = true;
                waveManager.StartEncounter(_rs75Encounter, new Vector3(10f, 0f, -50f));
                SpawnEnemyGroup(new Vector3(10f, 0f, -50f), 7);
                AdaptiveMusicController.Instance?.EnterCombat();
                HapticFeedbackManager.Instance?.PlayBuildingEmergence();
                // Gap 23: all waves triggered — unsubscribe to avoid redundant RS callbacks
                GameEvents.OnRSChanged -= HandleRSChangedForEncounters;
            }
        }

        void SpawnEnemyGroup(Vector3 center, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (float)i / count * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 4f;
                SpawnMudGolem(center + offset);
            }
        }

        public void SpawnMudGolem(Vector3 pos)
        {
            float y = SampleGroundY(pos.x, pos.z);
            if (!float.IsNaN(y)) pos.y = y + 0.1f;

            GameObject golem = null;

            using (PerformanceGuard.Profile(SystemTag.Spawn))
            {
                // Round 4: Pooling for MudGolems (avoids GC/alloc spikes on waves)
                if (_mudGolemPool.Count > 0)
                {
                    golem = _mudGolemPool.Dequeue();
                    golem.transform.position = pos;
                    golem.transform.rotation = Quaternion.identity;
                    golem.SetActive(true);
                    // Round 5: explicit reset on direct reuse path (in addition to delayed return path)
                    var h = golem.GetComponent<MudGolemHealth>();
                    if (h != null) h.ResetForReuse();
                    var a = golem.GetComponent<Tartaria.AI.MudGolemAI>();
                    if (a != null) a.ResetForPoolReuse();
                    Debug.Log("[EchohavenContentSpawner] MudGolem REUSED from pool (Round 5 reset applied).");
                }
                else if (kayKitMudGolemPrefab != null)
                {
                    golem = Instantiate(kayKitMudGolemPrefab, pos, Quaternion.identity);
                    golem.transform.localScale = Vector3.one * 1.3f;
                    EnsureNPCAnimator(golem);
                    Debug.Log("[EchohavenContentSpawner] MudGolem spawned from KayKit skeleton prefab.");
                }
                else
                {
                    // Fallback: primitive golem
                    golem = CreateMudGolemFallback(pos);
                    Debug.LogWarning("[EchohavenContentSpawner] MudGolem spawned from primitive fallback — assign kayKitMudGolemPrefab for AAA quality.");
                }
            }

            golem.name = "MudGolem";

            if (golem.GetComponent<MudGolemHealth>() == null)
            {
                var health = golem.AddComponent<MudGolemHealth>();
                health.MaxHealth = 50f;
                health.CurrentHealth = 50f;
            }

            // Day-2: real chase/attack AI on every spawned golem (was previously inert).
            if (golem.GetComponent<Tartaria.AI.MudGolemAI>() == null)
                golem.AddComponent<Tartaria.AI.MudGolemAI>();

            // Set enemy layer
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0)
                SetLayerRecursive(golem, enemyLayer);

            if (golem.transform.Find("Nameplate") == null)
                AddNameplate(golem, "Mud Golem", new Color(0.85f, 0.45f, 0.3f));

            // Round 4: Automatic per-prop LODGroups + mesh simplification + impostors (builds on previous)
            AttachPerfLODGroupWithImpostor(golem, isFoliage: false);
        }

        GameObject CreateMudGolemFallback(Vector3 pos)
        {
            var root = new GameObject("MudGolem");
            root.transform.position = pos;

            // Torso
            var torso = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            torso.name = "Torso";
            torso.transform.SetParent(root.transform);
            torso.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            torso.transform.localScale = new Vector3(1.2f, 1.4f, 1f);
            SetGolemMaterial(torso);

            // Head
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform);
            head.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            head.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            SetGolemMaterial(head);

            // Collider for combat
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1.2f, 0f);
            col.height = 2.8f;
            col.radius = 0.7f;

            // Gap 24: kinematic Rigidbody for reliable physics-based combat detection
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            // Gap 9: MudGolemHealth component allows combat RS rewards
            var health = root.AddComponent<MudGolemHealth>();
            health.MaxHealth = 50f;
            health.CurrentHealth = 50f;

            rb.useGravity = false;

            return root;
        }

        void SetGolemMaterial(GameObject go)
        {
            var r = go.GetComponent<MeshRenderer>();
            if (r == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", new Color(0.35f, 0.25f, 0.15f));
            mat.SetFloat("_Smoothness", 0.15f);
            r.material = mat;
        }

        // ─── Excavation Site Registration (Gap 6) ───────

        void RegisterEchohavenExcavationSites()
        {
            var exc = ExcavationSystem.Instance;
            if (exc == null) return;

            // Dome — 3-layer site, normal mode
            exc.RegisterSite("echohaven_dome",     new Vector3(30f,  0f,  20f), 3, false, "dome");
            // Fountain — 4-layer site
            exc.RegisterSite("echohaven_fountain", new Vector3(-20f, 0f,  35f), 4, false, "fountain");
            // Spire — 5-layer site, giant-mode required for deepest layers
            exc.RegisterSite("echohaven_spire",    new Vector3(0f,   0f, -30f), 5, true,  "spire");

            // ─── INTEGRATION: New scaffold's first obvious excavation site (Moon1EchohavenScaffold PlaceFirstExcavationSite) ───
            // Makes the core loop immediately playable: obvious ruin near start for first 10 min magic after population.
            if (_firstExcavationSite == null) _firstExcavationSite = GameObject.Find("Moon1_FirstExcavationSite");
            if (_firstExcavationSite != null)
            {
                Vector3 firstPos = _firstExcavationSite.transform.position;
                exc.RegisterSite("echohaven_first_ruin", firstPos, 2, false, "first_dome");
                // Also register as scanner POI so resonance scan immediately reveals the obvious first target
                var scanner = Gameplay.ResonanceScannerSystem.Instance;
                if (scanner != null)
                {
                    scanner.RegisterPOI(new Gameplay.ScanPOI
                    {
                        poiId = "echohaven_first_ruin",
                        poiType = Gameplay.ScanPOIType.ExcavationSite,
                        position = firstPos,
                        isRevealed = false
                    });
                }
                Debug.Log($"[EchohavenContentSpawner] Scaffold first excavation site integrated at {firstPos} (echohaven_first_ruin, 2 layers) — scan target ready.");

                // UI FTUE polish: clear "SCAN HERE" nameplate + F310/gamepad-aware prompt on the obvious first ruin (magical guided entry for 5-10 min post-populate)
                AddNameplate(firstSiteGO, Tartaria.Input.InputPromptHelper.Localize("SCAN HERE — [G] Resonance / [E] Tune (F310: B/A)"), new Color(0.4f, 0.85f, 1f));
                // If reduced motion, the nameplate is static billboard (already friendly); no extra pulse added here.
            }

            Debug.Log("[EchohavenContentSpawner] 3 Moon 1 excavation sites registered (+ scaffold first if present).");
        }

        // ─── VFX Event Handlers (Gap 17) ─────────────

        void SubscribeVFXEvents()
        {
            if (ExcavationSystem.Instance != null)
            {
                ExcavationSystem.Instance.OnSiteDiscovered     += VFX_SiteDiscovered;
                ExcavationSystem.Instance.OnLayerCleared       += VFX_LayerCleared;
                ExcavationSystem.Instance.OnExcavationComplete += VFX_ExcavationComplete;
                ExcavationSystem.Instance.OnRSYielded          += VFX_RSYielded;     // Gap 4: RS yield feedback
            }
        }

        void UnsubscribeVFXEvents()
        {
            GameEvents.OnRSChanged -= HandleRSChangedForEncounters;

            if (ExcavationSystem.Instance != null)
            {
                ExcavationSystem.Instance.OnSiteDiscovered     -= VFX_SiteDiscovered;
                ExcavationSystem.Instance.OnLayerCleared       -= VFX_LayerCleared;
                ExcavationSystem.Instance.OnExcavationComplete -= VFX_ExcavationComplete;
                ExcavationSystem.Instance.OnRSYielded          -= VFX_RSYielded;    // Gap 4: cleanup
            }
        }

        void VFX_SiteDiscovered(ExcavationSite site)
        {
            // Burst of golden particles at site
            SpawnBurstVFX(site.position, new Color(1f, 0.85f, 0.3f), 30);
            AudioManager.Instance?.PlaySFX("Discovery", site.position, 0.6f);
            TutorialSystem.Instance?.ForceComplete(TutorialStep.Discovery);  // Gap 13: tutorial step
            MiloController.Instance?.AppraiseArtifact();                     // Gap 8: Milo reacts
        }

        void VFX_RSYielded(ExcavationSite site, float rsAmount)              // Gap 4/5: RS yield feedback
        {
            RuntimeHUDBuilder.Instance?.ShowDamageNumber(rsAmount,
                site.position + new Vector3(0f, 2f, 0f));
        }

        void VFX_LayerCleared(ExcavationSite site, int layerIndex)
        {
            // Dirt/debris burst
            SpawnBurstVFX(site.position, new Color(0.5f, 0.4f, 0.3f), 20);
            AudioManager.Instance?.PlaySFX("DigComplete", site.position, 0.4f);
            HapticFeedbackManager.Instance?.PlayCombatHit();                 // Gap 11: layer haptic
        }

        void VFX_ExcavationComplete(ExcavationSite site)
        {
            // Large golden burst + sound
            SpawnBurstVFX(site.position, new Color(1f, 0.95f, 0.5f), 60);
            AudioManager.Instance?.PlaySFX("ExcavationComplete", site.position, 0.8f);
            HapticFeedbackManager.Instance?.PlayBuildingEmergence();            // Gap 12: excavation haptic
            MiloController.Instance?.NotifyBuildingRestored();                  // Gap 9: Milo reacts
            LiraelController.Instance?.AddTrust(2f);                            // Gap 10: Lirael trust
            AdaptiveMusicController.Instance?.PlayRestoration();                // Gap 15/16: stinger
            TutorialSystem.Instance?.ForceComplete(TutorialStep.BuildingRestore); // Gap 17/18: tutorial
        }

        void SpawnBurstVFX(Vector3 pos, Color color, int count)
        {
            var go = new GameObject("VFX_Burst");
            go.transform.position = pos;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = count;
            main.startLifetime = 1.5f;
            main.startSpeed = 3f;
            main.startSize = 0.2f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.5f;
            main.loop = false;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetColor("_BaseColor", new Color(color.r, color.g, color.b, 0.8f));
                mat.SetFloat("_Surface", 1f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                renderer.material = mat;
            }

            // Auto-destroy after particles die
            Destroy(go, 3f);
        }

        // ─── Helpers ─────────────────────────────────

        void AddNameplate(GameObject target, string displayName, Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = "Nameplate";
            marker.transform.SetParent(target.transform);
            marker.transform.localPosition = new Vector3(0f, 3f, 0f);
            marker.transform.localScale = new Vector3(2f, 0.4f, 1f);

            // Remove collider
            var col = marker.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // Nameplate material with color
            var r = marker.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetColor("_BaseColor", new Color(color.r, color.g, color.b, 0.8f));
                mat.SetFloat("_Surface", 1f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                r.material = mat;
            }

            // Billboard behavior
            marker.AddComponent<BillboardFacer>();
        }

        static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }

        // ─── Initial Enemies (spawn 2 Mud Golems immediately so player has something to fight) ─

        void SpawnInitialEnemies()
        {
            // Two golems east of spawn point — close enough to be seen, far enough not to ambush
            SpawnMudGolem(new Vector3(18f, 0f, 8f));
            SpawnMudGolem(new Vector3(22f, 0f, 12f));
            Debug.Log("[EchohavenContentSpawner] 2 initial Mud Golems placed.");
        }

        // ─── Dig Site Visual Markers (glowing pillars over each excavation site) ─

        void PlaceDigSiteMarkers()
        {
            var digParent = new GameObject("--- DIG SITES ---");

            CreateDigSiteMarker(digParent.transform, new Vector3(30f, 0f, 20f),  "Dome Ruins");
            CreateDigSiteMarker(digParent.transform, new Vector3(-20f, 0f, 35f), "Fountain Ruins");
            CreateDigSiteMarker(digParent.transform, new Vector3(0f, 0f, -30f),  "Spire Ruins");

            // Scaffold first excavation site marker (integrated obvious ruin)
            if (_firstExcavationSite == null) _firstExcavationSite = GameObject.Find("Moon1_FirstExcavationSite");
            if (_firstExcavationSite != null)
            {
                CreateDigSiteMarker(digParent.transform, _firstExcavationSite.transform.position, "First Ruin (Tutorial)");
            }

            Debug.Log("[EchohavenContentSpawner] Dig site markers placed (incl. scaffold first if present).");
        }

        void CreateDigSiteMarker(Transform parent, Vector3 pos, string siteName)
        {
            var marker = new GameObject($"DigMarker_{siteName.Replace(' ', '_')}");
            marker.transform.SetParent(parent);
            marker.transform.position = pos + new Vector3(0f, 0.1f, 0f);

            // Glowing cylinder beam pointing upward
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "Beam";
            beam.transform.SetParent(marker.transform);
            beam.transform.localPosition = new Vector3(0f, 3f, 0f);
            beam.transform.localScale = new Vector3(0.4f, 3f, 0.4f);
            Object.Destroy(beam.GetComponent<Collider>()); // no collision — visual only

            var br = beam.GetComponent<MeshRenderer>();
            var beamShader = Shader.Find("Universal Render Pipeline/Lit");
            if (beamShader != null)
            {
                var mat = new Material(beamShader);
                mat.SetColor("_BaseColor", new Color(0.3f, 0.7f, 0.3f, 0.5f));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.1f, 0.8f, 0.2f) * 2f);
                mat.SetFloat("_Surface", 1f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                br.material = mat;
            }

            // Point light for visibility at range
            var lightGO = new GameObject("DigLight");
            lightGO.transform.SetParent(marker.transform);
            lightGO.transform.localPosition = new Vector3(0f, 1f, 0f);
            var l = lightGO.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.2f, 1f, 0.3f);
            l.intensity = 2.5f;
            l.range = 12f;
            l.shadows = LightShadows.None;

            // Ground ring to indicate interaction area
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "GroundRing";
            ring.transform.SetParent(marker.transform);
            ring.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            ring.transform.localScale = new Vector3(6f, 0.02f, 6f);
            Object.Destroy(ring.GetComponent<Collider>());

            var rr = ring.GetComponent<MeshRenderer>();
            var ringShader = Shader.Find("Universal Render Pipeline/Lit");
            if (ringShader != null)
            {
                var mat = new Material(ringShader);
                mat.SetColor("_BaseColor", new Color(0.1f, 0.5f, 0.15f, 0.6f));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.05f, 0.4f, 0.1f) * 1.5f);
                mat.SetFloat("_Surface", 1f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                rr.material = mat;
            }

            // Floating label
            AddNameplate(marker, Tartaria.Input.InputPromptHelper.Localize($"[E] Dig — {siteName}"), new Color(0.3f, 1f, 0.4f));

            // Pulsing bob animation
            marker.AddComponent<BobbingMarker>();

            // Gap 12: Add interaction trigger — E-key dig feedback
            var trigger = marker.AddComponent<SphereCollider>();
            trigger.radius = 3.5f;
            trigger.isTrigger = true;
            marker.layer = LayerMask.NameToLayer("Interactable");
            if (marker.layer < 0) marker.layer = 0; // fallback to Default if layer not found

            var interactor = marker.AddComponent<DigSiteInteraction>();
            interactor.SiteName = siteName;
        }

        // ─── Anastasia Companion Spawn ───────────────

        void SpawnAnastasia()
        {
            if (AnastasiaController.Instance != null) return;

            GameObject anastasiaGO = null;

            // Prefer wired KayKit Mage prefab for AAA quality.
            if (kayKitAnastasiaPrefab != null)
            {
                anastasiaGO = Instantiate(kayKitAnastasiaPrefab, new Vector3(-4f, 0f, 8f), Quaternion.identity);
                AddGhostlyTint(anastasiaGO);
                EnsureNPCAnimator(anastasiaGO);
                Debug.Log("[EchohavenContentSpawner] Anastasia spawned from KayKit mage prefab.");
            }

            #if UNITY_EDITOR
            if (anastasiaGO == null)
            {
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/_Project/Prefabs/Characters/Anastasia.prefab");
                if (prefab != null)
                    anastasiaGO = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            }
            #endif

            if (anastasiaGO == null)
            {
                anastasiaGO = CreateAnastasiaPrimitiveFallback();
                Debug.LogWarning("[EchohavenContentSpawner] Anastasia spawned from primitive fallback — assign kayKitAnastasiaPrefab for AAA quality.");
            }

            anastasiaGO.name = "Anastasia";

            if (anastasiaGO.GetComponent<AnastasiaController>() == null)
                anastasiaGO.AddComponent<AnastasiaController>();

            // Trigger early manifestation after a short delay
            // (Design: she appears after player has had a few seconds to look around)
            Invoke(nameof(TriggerAnastasiaFirstAppearance), 12f);

            Debug.Log("[EchohavenContentSpawner] Anastasia spawned; manifestation scheduled in 12s.");
        }

        void TriggerAnastasiaFirstAppearance()
        {
            AnastasiaController.Instance?.TriggerFirstManifestation();
        }

        GameObject CreateAnastasiaPrimitiveFallback()
        {
            var root = new GameObject("Anastasia");

            // Tall slender capsule — ghost-like figure
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "GhostBody";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.35f, 0.9f, 0.35f);
            Object.Destroy(body.GetComponent<Collider>());

            var r = body.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetColor("_BaseColor", new Color(0.75f, 0.85f, 1f, 0.4f));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.5f, 0.6f, 1f) * 0.8f);
                mat.SetFloat("_Surface", 1f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                r.material = mat;
            }

            // Golden glow
            var glow = new GameObject("GoldenGlow");
            glow.transform.SetParent(root.transform);
            glow.transform.localPosition = new Vector3(0f, 1f, 0f);
            var l = glow.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.9f, 0.4f);
            l.intensity = 1.5f;
            l.range = 6f;
            l.shadows = LightShadows.None;

            // Golden particle halo
            var psGO = new GameObject("GoldenMotes");
            psGO.transform.SetParent(root.transform);
            psGO.transform.localPosition = new Vector3(0f, 1f, 0f);
            var ps = psGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 25;
            main.startLifetime = 3f;
            main.startSpeed = 0.2f;
            main.startSize = 0.07f;
            main.startColor = new Color(1f, 0.85f, 0.2f, 0.7f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 6f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.8f;

            return root;
        }

        // ─── KayKit Runtime Visual Helpers ───────────────────────────

        /// <summary>
        /// Ensure an NPC has an Animator with a controller so it doesn't appear frozen.
        /// If no controller is wired, leave it for Mecanim defaults to drive the bound clips.
        /// </summary>
        void EnsureNPCAnimator(GameObject npc)
        {
            if (npc == null) return;
            var anim = npc.GetComponentInChildren<Animator>();
            if (anim == null)
            {
                // Find a SkinnedMeshRenderer to find the rig root.
                var smr = npc.GetComponentInChildren<SkinnedMeshRenderer>();
                var rigRoot = smr != null && smr.rootBone != null ? smr.rootBone.gameObject : npc;
                anim = rigRoot.GetComponent<Animator>();
                if (anim == null) anim = rigRoot.AddComponent<Animator>();
            }
            // Apply player's RuntimeAnimatorController as a fallback so KayKit characters animate
            // the same locomotion clips wired by KayKitMixamoIntegrator.
            if (anim.runtimeAnimatorController == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var pAnim = player.GetComponentInChildren<Animator>();
                    if (pAnim != null && pAnim.runtimeAnimatorController != null)
                        anim.runtimeAnimatorController = pAnim.runtimeAnimatorController;
                }
            }
            anim.applyRootMotion = false;
            anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }

        /// <summary>
        /// Apply translucent ghostly tint to all renderers under the prefab so
        /// Anastasia reads as a spectral figure even with KayKit's solid materials.
        /// </summary>
        void AddGhostlyTint(GameObject root)
        {
            if (root == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var mat = new Material(shader);
                mat.SetColor("_BaseColor", new Color(0.75f, 0.85f, 1f, 0.6f));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.4f, 0.55f, 1f) * 0.7f);
                mat.SetFloat("_Surface", 1f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                r.material = mat;
            }
            // Add a soft blue point light for spectral glow.
            var glow = new GameObject("AnastasiaGlow");
            glow.transform.SetParent(root.transform, false);
            glow.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            var l = glow.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.7f, 0.85f, 1f);
            l.intensity = 2.2f;
            l.range = 6f;
            l.shadows = LightShadows.None;
        }

        /// <summary>
        /// Scatter wired KayKit rocks and foliage around Echohaven so the
        /// environment reads as populated rather than barren primitives.
        /// </summary>
        public void SpawnKayKitScatter()
        {
            int rocks = kayKitRockPrefabs != null ? kayKitRockPrefabs.Length : 0;
            int foliage = kayKitFoliagePrefabs != null ? kayKitFoliagePrefabs.Length : 0;
            if (rocks == 0 && foliage == 0)
            {
                Debug.LogWarning("[EchohavenContentSpawner] SpawnKayKitScatter: no KayKit rock or foliage prefabs wired.");
                return;
            }

            var parent = new GameObject("KayKit_Scatter").transform;

            // Deterministic seed so layout is consistent across runs.
            var rng = new System.Random(13_2026);

            // Rocks: 24 large scattered with 35-unit spread.
            for (int i = 0; i < 24 && rocks > 0; i++)
            {
                var prefab = kayKitRockPrefabs[rng.Next(rocks)];
                if (prefab == null) continue;
                var pos = new Vector3((float)(rng.NextDouble() * 70f - 35f), 0f, (float)(rng.NextDouble() * 70f - 35f));
                if (pos.magnitude < 6f) continue; // keep player spawn clear
                var rot = Quaternion.Euler(0f, (float)(rng.NextDouble() * 360f), 0f);
                var go = GetPooledOrInstantiate(prefab, pos, rot, parent, _foliagePool);
                float s = 0.8f + (float)rng.NextDouble() * 1.6f;
                go.transform.localScale = Vector3.one * s;
                AttachPerfLODGroupWithImpostor(go, isFoliage: true);
            }

            // Foliage: 80 bushes/grass with tighter spread. (Round 4 pooled + LOD/impostor)
            for (int i = 0; i < 80 && foliage > 0; i++)
            {
                var prefab = kayKitFoliagePrefabs[rng.Next(foliage)];
                if (prefab == null) continue;
                var pos = new Vector3((float)(rng.NextDouble() * 60f - 30f), 0f, (float)(rng.NextDouble() * 60f - 30f));
                if (pos.magnitude < 4f) continue;
                var rot = Quaternion.Euler(0f, (float)(rng.NextDouble() * 360f), 0f);
                var go = GetPooledOrInstantiate(prefab, pos, rot, parent, _foliagePool);
                float s = 0.7f + (float)rng.NextDouble() * 0.9f;
                go.transform.localScale = Vector3.one * s;
                AttachPerfLODGroupWithImpostor(go, isFoliage: true);
            }

            Debug.Log($"[EchohavenContentSpawner] KayKit scatter spawned: rocks={rocks}, foliage={foliage} (pooled+LOD+impostors).");
        }

        // ─── Round 4 Performance Helpers: Pooling + Auto LOD + Simplification + Impostors ───
        GameObject GetPooledOrInstantiate(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent, System.Collections.Generic.Queue<GameObject> pool)
        {
            if (pool.Count > 0)
            {
                var g = pool.Dequeue();
                g.transform.SetParent(parent, false);
                g.transform.SetPositionAndRotation(pos, rot);
                g.SetActive(true);
                return g;
            }
            using (PerformanceGuard.Profile(SystemTag.Spawn))
            {
                return Instantiate(prefab, pos, rot, parent);
            }
        }

        public void ReturnToPool(GameObject go, bool isGolem)
        {
            if (go == null) return;
            go.SetActive(false);
            var pool = isGolem ? _mudGolemPool : _foliagePool;
            int max = isGolem ? MAX_GOLEM_POOL : MAX_FOLIAGE_POOL;
            if (pool.Count < max) pool.Enqueue(go);
            else Destroy(go); // over cap
        }

        // Round 5: Full lifecycle pooling helper — supports VFX delay then safe return + reset
        public void ReturnToPoolAfterDelay(GameObject go, bool isGolem, float delaySeconds)
        {
            if (go == null) return;
            StartCoroutine(ReturnAfterDelayCoroutine(go, isGolem, delaySeconds));
        }

        private System.Collections.IEnumerator ReturnAfterDelayCoroutine(GameObject go, bool isGolem, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null && go.activeInHierarchy) // still valid
            {
                // Reset for reuse to prevent stale dead state
                var health = go.GetComponent<MudGolemHealth>();
                if (health != null) health.ResetForReuse();
                var ai = go.GetComponent<Tartaria.AI.MudGolemAI>();
                if (ai != null) ai.ResetForPoolReuse();
                ReturnToPool(go, isGolem);
            }
        }

        /// <summary>
        /// Automatic per-prop LODGroup + mesh simplification (basic decimation) + impostor quad for far distance.
        /// Delivers major draw call / vertex savings on dense KayKit scatter + MudGolems.
        /// </summary>
        void AttachPerfLODGroupWithImpostor(GameObject root, bool isFoliage)
        {
            if (root == null) return;
            using (PerformanceGuard.Profile(SystemTag.Foliage))
            {
                // Avoid duplicate
                if (root.GetComponent<LODGroup>() != null) return;

                var lodGroup = root.AddComponent<LODGroup>();
                lodGroup.size = isFoliage ? 3.5f : 4.5f;

                // Collect or create renderers for LOD0 (full detail)
                var rends = root.GetComponentsInChildren<MeshRenderer>(true);
                if (rends.Length == 0) return;

                // LOD0: existing renderers (or root)
                LOD lod0 = new LOD(0.6f, rends); // visible until 60% screen

                // LOD1: auto-simplified version (create low-detail child)
                GameObject lod1Root = new GameObject("LOD1_Simplified");
                lod1Root.transform.SetParent(root.transform, false);
                var simplifiedRends = new MeshRenderer[rends.Length];
                for (int i = 0; i < rends.Length && i < 4; i++) // limit for perf
                {
                    var src = rends[i];
                    var dst = Instantiate(src, lod1Root.transform);
                    var srcMf = src.GetComponent<MeshFilter>();
                    var dstMf = dst.GetComponent<MeshFilter>();
                    if (srcMf != null && dstMf != null && srcMf.sharedMesh != null)
                    {
                        dstMf.sharedMesh = CreateSimplifiedMesh(srcMf.sharedMesh, 0.5f); // 50% reduction
                    }
                    simplifiedRends[i] = dst;
                }
                LOD lod1 = new LOD(0.25f, simplifiedRends);

                // LOD2: Impostor quad (ultra cheap billboard for distance)
                GameObject impostor = new GameObject("LOD2_Impostor");
                impostor.transform.SetParent(root.transform, false);
                impostor.transform.localPosition = Vector3.up * 0.8f;
                var impostorRend = impostor.AddComponent<MeshRenderer>();
                impostorRend.sharedMaterial = _impostorMaterial;
                var mf = impostor.AddComponent<MeshFilter>();
                mf.sharedMesh = _impostorQuadMesh;

                // Simple billboard behavior (lightweight, no per-frame heavy)
                var bill = impostor.AddComponent<PerfImpostorBillboard>();
                bill.camera = UnityEngine.Camera.main;

                LOD lod2 = new LOD(0.04f, new[] { impostorRend }); // switch to impostor at ~4%

                lodGroup.SetLODs(new[] { lod0, lod1, lod2 });
                lodGroup.RecalculateBounds();
                lodGroup.fadeMode = LODFadeMode.CrossFade; // smooth

                // Mark static for batching gains
                root.isStatic = true;
            }
        }

        /// <summary>
        /// Very lightweight automatic mesh simplification (vertex/tri decimation by stride).
        /// Real production would use Unity MeshSimplifier or pre-bake; this is runtime zero-alloc win for LOD1.
        /// </summary>
        Mesh CreateSimplifiedMesh(Mesh src, float reductionFactor)
        {
            if (src == null) return src;
            // Simple decimate: keep every Nth vertex/tri (crude but zero cost, effective for foliage/rocks)
            int stride = Mathf.Max(2, Mathf.RoundToInt(1f / Mathf.Clamp(reductionFactor, 0.3f, 0.9f)));
            var verts = src.vertices;
            var tris = src.triangles;
            var uvs = src.uv;

            var newVerts = new System.Collections.Generic.List<Vector3>();
            var newTris = new System.Collections.Generic.List<int>();
            var newUVs = new System.Collections.Generic.List<Vector2>();
            var map = new System.Collections.Generic.Dictionary<int, int>();

            for (int i = 0; i < verts.Length; i += stride)
            {
                map[i] = newVerts.Count;
                newVerts.Add(verts[i]);
                if (uvs != null && i < uvs.Length) newUVs.Add(uvs[i]);
            }

            for (int t = 0; t < tris.Length; t += 3 * stride)
            {
                if (t + 2 >= tris.Length) break;
                int a = tris[t], b = tris[t+1], c = tris[t+2];
                if (map.ContainsKey(a) && map.ContainsKey(b) && map.ContainsKey(c))
                {
                    newTris.Add(map[a]);
                    newTris.Add(map[b]);
                    newTris.Add(map[c]);
                }
            }

            var m = new Mesh { name = src.name + "_Simplified" };
            m.SetVertices(newVerts);
            m.SetTriangles(newTris, 0);
            if (newUVs.Count > 0) m.SetUVs(0, newUVs);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }

    // Lightweight billboard for impostor LOD2 (perf cheap, no heavy rotation math every frame)
    public class PerfImpostorBillboard : MonoBehaviour
    {
        public UnityEngine.Camera camera;
        void LateUpdate()
        {
            if (camera == null) camera = UnityEngine.Camera.main;
            if (camera != null)
            {
                transform.LookAt(transform.position + camera.transform.forward, Vector3.up);
            }
        }
    }

    // ─── Helper Components ──────────────────────────

    /// <summary>
    /// Collectable Aether Shard — grants RS when player enters trigger.
    /// </summary>
    public class AetherShardPickup : MonoBehaviour
    {
        public float rsReward = 2f;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            GameLoopController.Instance?.QueueRSReward(rsReward, "shard_collect");
            GameEvents.FireRSChange(rsReward); // notify subscribers (UI, music, etc.)
            AudioManager.Instance?.PlaySFX("ShardCollect", transform.position, 0.5f);
            RuntimeHUDBuilder.Instance?.ShowDamageNumber(rsReward, transform.position + Vector3.up);
            EconomySystem.Instance?.AddCurrency(CurrencyType.AetherShards, 1);          // Gap 19
            QuestManager.Instance?.ProgressByType(                                       // Gap 20
                QuestObjectiveType.CollectItem, "aether_shard");
            MiloController.Instance?.AddTrust(0.5f);                                     // Gap 21
            HapticFeedbackManager.Instance?.PlayCombatHit();                             // Gap 22

            // VFX burst
            var ps = gameObject.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Stop();

            // Disable and destroy
            gameObject.SetActive(false);
            Destroy(gameObject, 0.5f);
        }
    }

    /// <summary>
    /// Makes a quad always face the camera.
    /// </summary>
    public class BillboardFacer : MonoBehaviour
    {
        void LateUpdate()
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;
            transform.LookAt(transform.position + cam.transform.forward);
        }
    }

    /// <summary>
    /// Simple health component for fallback MudGolem GameObjects.
    /// Called by GameLoopController when player fires combat abilities.
    /// </summary>
    public class MudGolemHealth : MonoBehaviour
    {
        public float MaxHealth = 50f;
        public float CurrentHealth = 50f;
        bool _dead;

        /// <summary>Static event fired when ANY golem dies — used by combat arena wave tracking.</summary>
        public static event System.Action<MudGolemHealth> OnAnyGolemDied;

        // Run-scoped kill counter to drive C02 progressive achievement.
        static int _golemKillCount;

        public void TakeDamage(float amount)
        {
            if (_dead) return;
            CurrentHealth -= amount;
            AudioManager.Instance?.PlaySFX("CombatHit", transform.position);
            if (CurrentHealth <= 0f)
                Die();
        }

        /// <summary>
        /// Day-2 bridge: called by Tartaria.AI.MudGolemAI via SendMessage when its
        /// internal FSM kills the golem. Avoids an asmdef cycle (AI -> Integration).
        /// </summary>
        public void KillFromAI()
        {
            if (_dead) return;
            CurrentHealth = 0f;
            Die();
        }

        void Die()
        {
            if (_dead) return;
            _dead = true;

            GameLoopController.Instance?.QueueRSReward(15f, "enemy_kill");
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.DefeatEnemies, "mud_golem");
            VFXController.Instance?.PlayEnemyDissolution(transform.position);
            AudioManager.Instance?.PlaySFX("EnemyDeath", transform.position);
            HapticFeedbackManager.Instance?.PlayGolemDeath();

            // Day-11: drop a pickup so combat has loot.
            LootDropper.Spawn(transform.position + Vector3.up * 0.4f);

            // Achievements: C01 first kill, C02 progressive (25 golems).
            var ach = AchievementSystem.Instance;
            if (ach != null)
            {
                ach.Unlock("C01");
                _golemKillCount++;
                ach.SetProgress("C02", Mathf.Clamp01(_golemKillCount / 25f));
            }

            try { OnAnyGolemDied?.Invoke(this); } catch (System.Exception ex) { Debug.LogWarning($"[MudGolemAI] OnAnyGolemDied listener failed: {ex.Message}"); }

            // Round 5 Production Hardening: full lifecycle pooling — return to pool (with delay for VFX) instead of Destroy
            var spawner = EchohavenContentSpawner.Instance;
            if (spawner != null)
            {
                spawner.ReturnToPoolAfterDelay(gameObject, true, 0.2f);
            }
            else
            {
                Destroy(gameObject, 0.15f);
            }
        }

        /// <summary>
        /// Round 5: Reset state when re-activated from object pool. Prevents dead golems re-spawning.
        /// Called from spawner coroutine on reuse.
        /// </summary>
        public void ResetForReuse()
        {
            _dead = false;
            CurrentHealth = MaxHealth;
            // Note: AI state reset handled in MudGolemAI.ResetForPoolReuse()
        }
    }

    /// <summary>
    /// Gap 12: IInteractable for dig site markers. E-key triggers dig feedback.
    /// </summary>
    public class DigSiteInteraction : MonoBehaviour, Input.IInteractable
    {
        public string SiteName;
        bool _excavated;

        public string GetInteractPrompt() => _excavated ? $"{SiteName} — already excavated" : $"{Tartaria.Input.InputPromptHelper.Interact} Excavate — {SiteName}";

        public void Interact(GameObject player)
        {
            if (_excavated) return;
            _excavated = true;

            var pos = transform.position;

            // RS reward for excavation
            GameLoopController.Instance?.QueueRSReward(10f, "excavation");
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.ExcavateRuin, SiteName);
            TutorialSystem.Instance?.ForceComplete(TutorialStep.BuildingRestore);

            // VFX + audio feedback
            VFXController.Instance?.PlayDiscoveryBurst(pos);
            AudioManager.Instance?.PlaySFX("DigSuccess", pos);
            HapticFeedbackManager.Instance?.PlayCombatHit();

            // HUD notification
            UI.HUDController.Instance?.ShowObjective($"Excavated: {SiteName}");

            // Fade out beam visuals
            var beam = transform.Find("Beam");
            if (beam != null) beam.gameObject.SetActive(false);
            var light = GetComponentInChildren<Light>();
            if (light != null) light.gameObject.SetActive(false);

            Debug.Log($"[DigSiteInteraction] Player excavated {SiteName}");
        }
    }

    /// <summary>
    /// Runtime shovel pickup for MVP excavation readability.
    /// </summary>
    public class ShovelPickup : MonoBehaviour, Input.IInteractable
    {
        /// <summary>Static flag — true once the player has picked up the shovel.</summary>
        public static bool ShovelAcquired { get; private set; }

        public string displayName = "Shovel";
        bool _picked;

        public string GetInteractPrompt() => _picked ? $"{displayName} acquired" : $"{Tartaria.Input.InputPromptHelper.Interact} Pick Up {displayName}";

        public void Interact(GameObject player)
        {
            if (_picked) return;
            _picked = true;
            ShovelAcquired = true;

            // Bridge: also register pickup in the new InventorySystem so InventoryUI updates
            // and SaveData.inventoryItemIds persists the shovel across save/load.
            Tartaria.Gameplay.InventorySystem.Instance?.AddItem("shovel", 1);

            UI.HUDController.Instance?.ShowObjective($"Tool Acquired: {displayName}");
            RuntimeHUDBuilder.Instance?.ShowDamageNumber(1f, transform.position + Vector3.up * 1.5f);
            AudioManager.Instance?.PlaySFX("Discovery", transform.position, 0.6f);

            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            foreach (Transform child in transform)
                child.gameObject.SetActive(false);

            Destroy(gameObject, 2f);
        }
    }

    /// <summary>
    /// Wrapper component that lets the player Interact with Milo to trigger
    /// his introduction / dialogue. Sits on the Milo GameObject alongside
    /// the trigger collider so PlayerInputHandler raycasts can hit it.
    /// </summary>
    public class MiloInteractable : MonoBehaviour, Input.IInteractable
    {
        public string GetInteractPrompt()
        {
            string verb = (MiloController.Instance != null && MiloController.Instance.HasIntroduced) ? "Talk to" : "Greet";
            return $"{Tartaria.Input.InputPromptHelper.Interact} {verb} Milo";
        }

        public void Interact(GameObject player)
        {
            var milo = MiloController.Instance;
            if (milo == null) return;

            if (!milo.HasIntroduced)
            {
                milo.Introduce();
            }
            else
            {
                // Re-trigger context dialogue for repeat conversations
                DialogueManager.Instance?.PlayContextDialogue("milo_chat");
                milo.AddTrust(1f);
            }
            AudioManager.Instance?.PlaySFX("Interact", transform.position, 0.4f);
        }
    }

}
