using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Audio;
using Tartaria.Input;

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

        // Cached for VFX event wiring
        readonly List<GameObject> _aetherShards = new();
        readonly List<ParticleSystem> _environmentalVFX = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            CancelInvoke(nameof(IntroduceMilo));     // Gap 2: prevent null-object call on early destroy
            CancelInvoke(nameof(IntroduceLirael));   // Gap 4: same safety for Lirael introduction
            UnsubscribeVFXEvents();
        }

        void Start()
        {
            SpawnMilo();
            SpawnLirael();                                              // Gap 3: Lirael first appearance
            SpawnCassian();
            SpawnCollectibles();
            SpawnEnvironmentalProps();
            SpawnCorruptionZones();
            SpawnParticleEffects();
            SpawnAmbientAudio();
            SpawnInitialEnemies();                                      // Spawn 2 golems at game start
            SetupEnemyWaveEncounters();
            RegisterEchohavenExcavationSites();                         // Gap 6: register dig sites
            PlaceDigSiteMarkers();                                      // Visual markers over dig sites
            SpawnAnastasia();                                           // Anastasia ghost companion
            SubscribeVFXEvents();
            ActivateStartingQuest();                                    // Gap 11: activate first quest on HUD
            AdaptiveMusicController.Instance?.SetZone(0);              // Gap 14: Moon 1 zone music
            CompanionManager.Instance?.CheckUnlocks(0);                // Gap 25: companion unlock check

            Debug.Log("[EchohavenContentSpawner] Zone content populated.");
        }

        void ActivateStartingQuest()
        {
            var qm = QuestManager.Instance;
            if (qm == null) return;

            // Activate the first Echohaven quest so player sees it on HUD immediately
            qm.ActivateQuest("quest_echohaven_awakening");

            // Populate the HUD objective panel with the active quest title
            var def = qm.GetQuestDefinition("quest_echohaven_awakening");
            if (def != null)
                UI.HUDController.Instance?.ShowObjective($"QUEST: {def.displayName}");

            Debug.Log("[EchohavenContentSpawner] Starting quest activated on HUD.");
        }

        // ─── Milo Companion (Gap 6) ─────────────────

        void SpawnMilo()
        {
            if (MiloController.Instance != null) return; // Already exists

            var playerSpawn = GameObject.Find("PlayerSpawn");
            Vector3 spawnPos = playerSpawn != null
                ? playerSpawn.transform.position + miloSpawnOffset
                : new Vector3(12f, 1f, 4f);

            // Try loading prefab
            GameObject miloGO = null;
            #if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Milo.prefab");
            if (prefab != null)
                miloGO = Instantiate(prefab, spawnPos, Quaternion.identity);
            #endif

            if (miloGO == null)
            {
                // Runtime fallback: create from primitives
                miloGO = CreateMiloFallback(spawnPos);
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

            var cassianGO = new GameObject("Cassian");
            cassianGO.transform.position = cassianPosition;

            // Visual: tall robed figure (capsule)
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

            Debug.Log("[EchohavenContentSpawner] Particle effects spawned.");
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
            // Wind ambience — central
            CreateAmbientSource("Ambient_Wind", Vector3.zero, "Wind", 0.25f, 80f);

            // Deep hum near buildings — low frequency resonance
            CreateAmbientSource("Ambient_Hum_Dome", new Vector3(30f, 2f, 20f), "DeepHum", 0.15f, 25f);
            CreateAmbientSource("Ambient_Hum_Fountain", new Vector3(-20f, 2f, 35f), "WaterAmbient", 0.2f, 20f);
            CreateAmbientSource("Ambient_Hum_Spire", new Vector3(0f, 5f, -30f), "CrystalHum", 0.15f, 25f);

            Debug.Log("[EchohavenContentSpawner] Ambient audio sources placed.");
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
                // Generate a procedural tone as placeholder
                src.clip = GenerateAmbientTone(sfxName);
                src.Play();
            }
        }

        AudioClip GenerateAmbientTone(string name)
        {
            int sampleRate = 44100;
            int duration = 5; // seconds
            int sampleCount = sampleRate * duration;
            var samples = new float[sampleCount];

            float freq = name switch
            {
                "Wind" => 120f,
                "DeepHum" => 60f,
                "WaterAmbient" => 200f,
                "CrystalHum" => 432f,
                _ => 100f
            };

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                // Low drone with slight variation
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.1f
                    + Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t) * 0.05f;
                // Fade in/out for seamless loop
                float env = Mathf.Min(t, duration - t, 0.5f) / 0.5f;
                samples[i] *= env;
            }

            var clip = AudioClip.Create($"Ambient_{name}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
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

        void SpawnMudGolem(Vector3 pos)
        {
            GameObject golem = null;

            #if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/MudGolem.prefab");
            if (prefab != null)
                golem = Instantiate(prefab, pos, Quaternion.identity);
            #endif

            if (golem == null)
            {
                // Runtime fallback
                golem = CreateMudGolemFallback(pos);
            }

            golem.name = "MudGolem";

            // Set enemy layer
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0)
                SetLayerRecursive(golem, enemyLayer);
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

            Debug.Log("[EchohavenContentSpawner] 3 Moon 1 excavation sites registered.");
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

            Debug.Log("[EchohavenContentSpawner] 3 dig site markers placed.");
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
            AddNameplate(marker, $"[E] Dig — {siteName}", new Color(0.3f, 1f, 0.4f));

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

            #if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Characters/Anastasia.prefab");
            if (prefab != null)
                anastasiaGO = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            #endif

            if (anastasiaGO == null)
                anastasiaGO = CreateAnastasiaPrimitiveFallback();

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

        public void TakeDamage(float amount)
        {
            if (_dead) return;
            CurrentHealth -= amount;
            AudioManager.Instance?.PlaySFX("CombatHit", transform.position);
            if (CurrentHealth <= 0f)
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

            Destroy(gameObject, 0.15f);
        }
    }

    /// <summary>
    /// Gap 12: IInteractable for dig site markers. E-key triggers dig feedback.
    /// </summary>
    public class DigSiteInteraction : MonoBehaviour, Input.IInteractable
    {
        public string SiteName;
        bool _excavated;

        public string GetInteractPrompt() => _excavated ? $"{SiteName} — already excavated" : $"[E] Excavate — {SiteName}";

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

}
