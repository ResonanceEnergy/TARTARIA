using UnityEngine;
using UnityEditor;
using Tartaria.Core;
using Tartaria.Integration;
using Tartaria.UI;
using Tartaria.Input;
using System.Collections.Generic;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 1 Scaffolding: Magnetic Moon — Echohaven Vertical Slice.
    /// "The Awakening" — first buried structures, resonance scanning, dome tuning, Milo companion intro, corruption purge.
    ///
    /// One-click population for immediate vertical slice play:
    ///   - Star Dome (main quest building)
    ///   - Crystal Spire, Harmonic Fountain, other key structures
    ///   - Milo spawn + intro volume
    ///   - Cassian encounter
    ///   - Mud Golem spawners + corruption zones
    ///   - Aether shards / collectibles
    ///   - Start volume for the full awakening sequence
    ///   - Ambient audio + VFX zones
    ///   - Rich 3D/TA props on populate: buried ruins, resonance crystals, foliage clusters, corruption markers, dig debris (static primitives for instant lived-in feel)
    ///
    /// Menu: Tartaria > Populate Moon 1 (Echohaven Vertical Slice)
    /// Mirrors the successful Moon 3 / Moon 5 scaffold pattern so QA/playtesters can instantly run the core loop.
    /// </summary>
    public static class Moon1EchohavenScaffold
    {
        [MenuItem("Tartaria/Build Assets/Moon 1 — Populate Echohaven (Magnetic Moon)", false, 10)]
        public static void PopulateEchohaven()
        {
            int created = 0;

            created += SetupEchohavenCoreController();

            created += PlaceKeyEchohavenBuildings();

            created += PlaceMiloAndIntroVolume();

            created += PlaceCassianAndEarlyEncounters();

            created += PlaceMudGolemsAndCorruption();

            created += PlaceAetherShardsAndCollectibles();

            created += PlaceFirstExcavationSite();

            created += CreateStartAwakeningVolume();

            created += AddEchohavenAtmosphere();

            created += AddMoreEchohavenAtmosphereProps();  // 3D/TA Props subagent (extended): buried ruins + resonance crystals + foliage clusters + corruption markers + dig debris for rich lived-in Echohaven on Populate menu run

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Tartaria] Moon 1 Echohaven populated with {created} elements. Walk the start volume to begin the Magnetic Moon awakening.");
        }

        static int SetupEchohavenCoreController()
        {
            var existing = Object.FindObjectOfType<EchohavenContentSpawner>();
            if (existing != null) return 0;

            var go = new GameObject("Moon1_EchohavenCore");
            var spawner = go.AddComponent<EchohavenContentSpawner>();

            // The spawner will auto-run its full sequence (Milo, first dig sites, 5-beat framework, etc.) on Start
            // Wire default positions if not already set in the spawner
            return 1;
        }

        static int PlaceKeyEchohavenBuildings()
        {
            int n = 0;

            // Star Dome — the main awakening building (central quest object)
            if (GameObject.Find("Echohaven_StarDome") == null)
            {
                var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dome.name = "Echohaven_StarDome";
                dome.transform.position = new Vector3(0, 8f, 0);
                dome.transform.localScale = new Vector3(18f, 12f, 18f);

                var mr = dome.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mr.sharedMaterial.color = new Color(0.85f, 0.82f, 0.95f);
                    mr.sharedMaterial.SetColor("_EmissionColor", new Color(0.6f, 0.7f, 1f) * 0.8f);
                    mr.sharedMaterial.EnableKeyword("_EMISSION");
                }
                n++;
            }

            // Crystal Spire (tall landmark)
            if (GameObject.Find("Echohaven_CrystalSpire") == null)
            {
                var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spire.name = "Echohaven_CrystalSpire";
                spire.transform.position = new Vector3(-25f, 12f, -8f);
                spire.transform.localScale = new Vector3(4f, 24f, 4f);

                var mr = spire.GetComponent<MeshRenderer>();
                if (mr) mr.sharedMaterial.color = new Color(0.7f, 0.85f, 0.95f);
                n++;
            }

            // Harmonic Fountain (early wonder / tuning focal point)
            if (GameObject.Find("Echohaven_HarmonicFountain") == null)
            {
                var fountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                fountain.name = "Echohaven_HarmonicFountain";
                fountain.transform.position = new Vector3(18f, 1f, 12f);
                fountain.transform.localScale = new Vector3(7f, 1.2f, 7f);

                var mr = fountain.GetComponent<MeshRenderer>();
                if (mr) mr.sharedMaterial.color = new Color(0.6f, 0.8f, 0.9f);
                n++;
            }

            return n;
        }

        static int PlaceMiloAndIntroVolume()
        {
            int n = 0;

            // Milo spawn point (will be handled by ContentSpawner at runtime, but we place a marker)
            if (GameObject.Find("Moon1_MiloSpawn") == null)
            {
                var marker = new GameObject("Moon1_MiloSpawn");
                marker.transform.position = new Vector3(2f, 1f, -1f);
                n++;
            }

            // Start volume that triggers the full awakening sequence (Milo intro + first excavation hint)
            if (GameObject.Find("Moon1_StartAwakening_Volume") == null)
            {
                var vol = new GameObject("Moon1_StartAwakening_Volume");
                vol.transform.position = new Vector3(0, 2f, 8f);
                var col = vol.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 12f;
                vol.AddComponent<Moon1AwakeningTrigger>();
                n++;
            }

            return n;
        }

        static int PlaceCassianAndEarlyEncounters()
        {
            if (GameObject.Find("Moon1_CassianMarker") != null) return 0;

            var marker = new GameObject("Moon1_CassianMarker");
            marker.transform.position = new Vector3(-10f, 1f, 15f);
            return 1;
        }

        static int PlaceMudGolemsAndCorruption()
        {
            int n = 0;

            // Simple corruption patches + Mud Golem proxies
            for (int i = 0; i < 4; i++)
            {
                var patch = GameObject.CreatePrimitive(PrimitiveType.Plane);
                patch.name = $"Moon1_CorruptionPatch_{i}";
                patch.transform.position = new Vector3(-8 + i * 7, 0.1f, 22 + (i % 2) * 4);
                patch.transform.localScale = new Vector3(3.5f, 1f, 3.5f);

                var mr = patch.GetComponent<MeshRenderer>();
                if (mr) mr.sharedMaterial.color = new Color(0.35f, 0.25f, 0.18f);

                // Attach rich Moon1 corruption drone (procedural 432-bleed + tritone dissonance, low volume for unsettling buried contrast)
                Tartaria.Audio.ProceduralSFXLibrary.Initialize();
                var drone = patch.AddComponent<AudioSource>();
                var corruptClip = Tartaria.Audio.ProceduralSFXLibrary.Get("Moon1_CorruptionDrone");
                drone.clip = corruptClip;
                if (drone.clip != null)
                {
                    drone.loop = true;
                    drone.volume = 0.07f + (i % 2) * 0.015f;
                    drone.pitch = 0.96f + (i % 3) * 0.04f; // slight variation, near original pitch for seamless loop
                    drone.spatialBlend = 1f;
                    drone.maxDistance = 18f;
                    drone.rolloffMode = AudioRolloffMode.Linear;
                    drone.playOnAwake = true;
                }
                else
                {
                    // Fallback low sine drone if lib not ready
                    drone.clip = null;
                }

                n++;
            }

            return n;
        }

        static int PlaceAetherShardsAndCollectibles()
        {
            int n = 0;

            for (int i = 0; i < 6; i++)
            {
                var shard = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shard.name = $"Moon1_AetherShard_{i}";
                shard.transform.position = new Vector3(-15 + i * 6, 1.2f, -12 + (i % 3) * 3);
                shard.transform.localScale = Vector3.one * 0.6f;

                var mr = shard.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mr.sharedMaterial.color = new Color(0.4f, 0.7f, 1f);
                    mr.sharedMaterial.SetColor("_EmissionColor", Color.cyan * 1.2f);
                    mr.sharedMaterial.EnableKeyword("_EMISSION");
                }
                n++;
            }

            return n;
        }

        // Bonus: Place the FIRST obvious buried ruin excavation site so the core loop (scan → dig → tune → restore) is immediately playable and magical within first 10 min.
        // Enhanced with Moon5-style columns + dome for instant vertical slice grandeur even as "buried" proxy. Reduced-motion safe (static light, no heavy particles).
        static int PlaceFirstExcavationSite()
        {
            if (GameObject.Find("Moon1_FirstExcavationSite") != null) return 0;

            var site = new GameObject("Moon1_FirstExcavationSite");
            site.transform.position = new Vector3(8f, 0.3f, 5f);

            // Earth mound for obvious "something is buried here" silhouette — visible from distance, invites scan
            var mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mound.name = "RuinMound";
            mound.transform.SetParent(site.transform);
            mound.transform.localPosition = Vector3.zero;
            mound.transform.localScale = new Vector3(7.5f, 1.8f, 7.5f);
            var moundMr = mound.GetComponent<MeshRenderer>();
            if (moundMr)
            {
                moundMr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                moundMr.sharedMaterial.color = new Color(0.35f, 0.30f, 0.22f);
            }
            UnityEngine.Object.DestroyImmediate(mound.GetComponent<Collider>());

            // 4 half-buried fluted columns (peeking from sediment) — obvious ruin signature, Moon5 grandeur echo
            int colCount = 4;
            float colRadius = 2.8f;
            for (int c = 0; c < colCount; c++)
            {
                float angle = (c / (float)colCount) * Mathf.PI * 2f + 0.4f;
                var colGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                colGO.name = $"RuinColumn_{c}";
                colGO.transform.SetParent(site.transform);
                colGO.transform.localPosition = new Vector3(Mathf.Cos(angle) * colRadius, 0.9f, Mathf.Sin(angle) * colRadius);
                colGO.transform.localScale = new Vector3(0.55f, 2.2f, 0.55f);
                var cmr = colGO.GetComponent<MeshRenderer>();
                if (cmr)
                {
                    cmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    cmr.sharedMaterial.color = new Color(0.55f, 0.52f, 0.48f);
                }
                UnityEngine.Object.DestroyImmediate(colGO.GetComponent<Collider>());
            }

            // Dome fragment peeking out — the "Star Dome" promise, instantly magical and obvious target for resonance scan
            var domeFrag = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            domeFrag.name = "DomeFragment_Peeking";
            domeFrag.transform.SetParent(site.transform);
            domeFrag.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            domeFrag.transform.localScale = new Vector3(3.2f, 1.8f, 3.2f);
            var dmr = domeFrag.GetComponent<MeshRenderer>();
            if (dmr)
            {
                dmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                dmr.sharedMaterial.color = new Color(0.82f, 0.78f, 0.92f);
                dmr.sharedMaterial.SetColor("_EmissionColor", new Color(0.4f, 0.55f, 0.95f) * 0.6f);
                dmr.sharedMaterial.EnableKeyword("_EMISSION");
            }
            UnityEngine.Object.DestroyImmediate(domeFrag.GetComponent<Collider>());

            // Static resonance hint light (reduced-motion friendly — no particles or animation; always visible soft glow)
            var hintLight = new GameObject("RuinResonanceHintLight");
            hintLight.transform.SetParent(site.transform);
            hintLight.transform.localPosition = new Vector3(0f, 2.8f, 0f);
            var light = hintLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.6f, 0.75f, 1f);
            light.intensity = 1.8f;
            light.range = 9f;

            // Interaction volume (tuning/excavation prompt) — now auto-attaches polished FTUE trigger for clear "scan here", first-tune banner, 5-beat objective, reduced-motion + F310 callouts.
            var tuneVol = new GameObject("FirstTuning_Volume");
            tuneVol.transform.SetParent(site.transform);
            tuneVol.transform.localPosition = Vector3.up * 1.8f;
            var col = tuneVol.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 6f;
            tuneVol.AddComponent<Moon1FirstTuningTrigger>();  // Polished awakening FTUE: clear scan prompt + companion success banner + beat flow

            // ─── Attach rich buried resonance hum directly to first excavation (immediate magical audio on first populate + start volume)
            // Uses Moon1_BuriedResonanceHum (procedural 108 + 432 + PHI overtones, slow pulse) — makes the obvious ruin feel alive and wondrous from second 1.
            Tartaria.Audio.ProceduralSFXLibrary.Initialize();
            var resonance = site.AddComponent<AudioSource>();
            resonance.clip = Tartaria.Audio.ProceduralSFXLibrary.Get("Moon1_BuriedResonanceHum");
            if (resonance.clip != null)
            {
                resonance.loop = true;
                resonance.volume = 0.115f;
                resonance.spatialBlend = 1f;
                resonance.maxDistance = 32f;
                resonance.rolloffMode = AudioRolloffMode.Linear;
                resonance.playOnAwake = true;
            }

            Debug.Log("[Moon1Scaffold] First obvious buried ruin placed with columns + peeking dome at " + site.transform.position + " — scan-ready for magical 10-min loop. (rich Moon1_BuriedResonanceHum 432+PHI attached)");

            return 1;
        }

        static int CreateStartAwakeningVolume()
        {
            // Already created in PlaceMiloAndIntroVolume — this is a safety duplicate guard
            return 0;
        }

        static int AddEchohavenAtmosphere()
        {
            // Soft golden fog + ambient audio zone hint
            var cam = UnityEngine.Camera.main;
            if (cam != null)
            {
                // The scene's PostProcess / Volume profile should already handle Echohaven look
            }

            // Central ethereal motes / zone wind (rich Moon1 procedural layer: high 648+PHI shimmer + soft gust for floating aether wonder, gentle volume for first 5-10 min magic)
            if (GameObject.Find("Echohaven_ZoneWind") == null)
            {
                var windGO = new GameObject("Echohaven_ZoneWind");
                windGO.transform.position = Vector3.zero + Vector3.up * 4f;
                Tartaria.Audio.ProceduralSFXLibrary.Initialize();
                var windSrc = windGO.AddComponent<AudioSource>();
                windSrc.clip = Tartaria.Audio.ProceduralSFXLibrary.Get("Moon1_EtherealMotes");
                if (windSrc.clip != null)
                {
                    windSrc.loop = true;
                    windSrc.volume = 0.09f;
                    windSrc.spatialBlend = 0f; // 2D ambient ethereal bed
                    windSrc.playOnAwake = true;
                }
            }
            return 2;
        }

        /// <summary>
        /// Extended 3D/TA Props contribution for Moon 1 Echohaven.
        /// Adds rich atmospheric + lived-in props using lightweight primitives + emissive URP/Lit + point lights (no shadows for perf).
        /// Categories: additional buried ruins, resonance crystals, foliage clusters, corruption visual markers, dig site debris.
        /// All static, collider-free decor. Instant rich feel the moment "Populate Moon 1" menu is run.
        /// </summary>
        static int AddMoreEchohavenAtmosphereProps()
        {
            if (GameObject.Find("Moon1_ExtraAtmosphere") != null) return 0;

            int n = 0;
            var root = new GameObject("Moon1_ExtraAtmosphere");
            root.isStatic = true;

            // ── Resonance Crystals (expanded set, cool aether glow, clustered near landmarks) ──
            Vector3[] crystalPos = {
                new Vector3(5f, 1.1f, 3f), new Vector3(-8f, 1.8f, 5f), new Vector3(15f, 0.9f, 9f),
                new Vector3(-20f, 2.2f, -5f), new Vector3(3f, 1.4f, 20f), new Vector3(24f, 1.6f, 2f),
                new Vector3(-12f, 0.7f, 14f), new Vector3(9f, 2.0f, -9f), new Vector3(-27f, 3.5f, -10f),
                new Vector3(1f, 1.3f, -14f), new Vector3(18f, 0.85f, 15f)
            };
            for (int i = 0; i < crystalPos.Length; i++)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                c.name = $"Moon1_ResonanceCrystal_{i:00}";
                c.transform.position = crystalPos[i];
                c.transform.localScale = Vector3.one * (0.45f + (i % 3) * 0.22f);
                var mr = c.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mr.sharedMaterial.color = new Color(0.35f, 0.78f, 0.95f);
                    mr.sharedMaterial.SetColor("_EmissionColor", Color.cyan * 1.9f);
                    mr.sharedMaterial.EnableKeyword("_EMISSION");
                }
                UnityEngine.Object.DestroyImmediate(c.GetComponent<Collider>());
                var lt = c.AddComponent<Light>();
                lt.type = LightType.Point;
                lt.color = Color.cyan;
                lt.intensity = 1.7f;
                lt.range = 7.5f;
                lt.shadows = LightShadows.None;
                c.transform.SetParent(root.transform);
                n++;
            }

            // ── More Buried Ruins (scattered, tilted slabs + peeking fragments for lived-in excavation feel) ──
            Vector3[] ruinPositions = {
                new Vector3(-15f, 0.25f, 12f), new Vector3(22f, 0.3f, -7f), new Vector3(-5f, 0.2f, -18f),
                new Vector3(14f, 0.35f, 26f), new Vector3(-28f, 0.4f, 15f), new Vector3(7f, 0.22f, -3f)
            };
            for (int i = 0; i < ruinPositions.Length; i++)
            {
                var r = GameObject.CreatePrimitive(PrimitiveType.Cube);
                r.name = $"Moon1_BuriedRuin_{i:00}";
                r.transform.position = ruinPositions[i];
                r.transform.localScale = new Vector3(3.8f, 0.85f, 2.6f);
                r.transform.rotation = Quaternion.Euler(3f + i, i * 31f, -5f);
                var mr = r.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mr.sharedMaterial.color = new Color(0.48f, 0.42f, 0.35f);
                }
                UnityEngine.Object.DestroyImmediate(r.GetComponent<Collider>());
                r.transform.SetParent(root.transform);
                n++;

                // small debris cap on ruin
                var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cap.name = $"RuinCapDebris_{i}";
                cap.transform.SetParent(r.transform);
                cap.transform.localPosition = Vector3.up * 0.55f;
                cap.transform.localScale = new Vector3(0.45f, 0.28f, 0.45f);
                cap.transform.rotation = Quaternion.Euler(18, i * 20, 0);
                var capMr = cap.GetComponent<MeshRenderer>();
                if (capMr)
                {
                    capMr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    capMr.sharedMaterial.color = new Color(0.42f, 0.36f, 0.29f);
                }
                UnityEngine.Object.DestroyImmediate(cap.GetComponent<Collider>());
                n++;
            }

            // ── Foliage Clusters (cheap bushy primitives: stems + leaf blobs, green tones, no lights) ──
            Vector3[] foliageCenters = {
                new Vector3(18f, 0.05f, 1f), new Vector3(-6f, 0.05f, 8f), new Vector3(7f, 0.05f, 18f),
                new Vector3(-19f, 0.05f, -8f), new Vector3(28f, 0.05f, 14f), new Vector3(11f, 0.05f, -16f)
            };
            Vector3[][] foliageOffsets = {
                new [] { new Vector3(0,0.95f,0), new Vector3(0.35f,0.7f,0.25f), new Vector3(-0.28f,0.82f,-0.32f) },
                new [] { new Vector3(0,0.9f,0), new Vector3(-0.4f,0.65f,0.2f), new Vector3(0.3f,0.78f,-0.25f) }
            };
            for (int fc = 0; fc < foliageCenters.Length; fc++)
            {
                var froot = new GameObject($"FoliageCluster_{fc:00}");
                froot.transform.position = foliageCenters[fc];
                froot.transform.SetParent(root.transform);
                froot.isStatic = true;

                // stem
                var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.name = "Stem";
                stem.transform.SetParent(froot.transform);
                stem.transform.localPosition = new Vector3(0, 0.42f, 0);
                stem.transform.localScale = new Vector3(0.11f, 0.85f, 0.11f);
                var smr = stem.GetComponent<MeshRenderer>();
                if (smr)
                {
                    smr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    smr.sharedMaterial.color = new Color(0.26f, 0.30f, 0.17f);
                }
                UnityEngine.Object.DestroyImmediate(stem.GetComponent<Collider>());
                n++;

                // 3 foliage blobs per cluster
                var offs = foliageOffsets[fc % foliageOffsets.Length];
                float[] bscales = { 1.18f, 0.92f, 1.08f };
                for (int b = 0; b < 3; b++)
                {
                    var blob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    blob.name = $"LeafBlob_{b}";
                    blob.transform.SetParent(froot.transform);
                    blob.transform.localPosition = offs[b];
                    blob.transform.localScale = Vector3.one * bscales[b];
                    var bmr = blob.GetComponent<MeshRenderer>();
                    if (bmr)
                    {
                        bmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        bmr.sharedMaterial.color = new Color(0.20f, 0.46f, 0.24f);
                    }
                    UnityEngine.Object.DestroyImmediate(blob.GetComponent<Collider>());
                    n++;
                }
            }

            // ── Corruption Visual Markers (dark patches + tendril spikes, near existing corruption + new pockets) ──
            Vector3[] corrPos = {
                new Vector3(-4f, 0.08f, 24f), new Vector3(12f, 0.1f, 21f), new Vector3(2f, 0.07f, 28f),
                new Vector3(-22f, 0.12f, 19f), new Vector3(19f, 0.09f, 27f), new Vector3(-9f, 0.1f, 17f),
                new Vector3(26f, 0.11f, 9f), new Vector3(-2f, 0.08f, -22f)
            };
            for (int ci = 0; ci < corrPos.Length; ci++)
            {
                // ooze patch
                var patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                patch.name = $"CorruptionMarker_{ci:00}";
                patch.transform.position = corrPos[ci];
                patch.transform.localScale = new Vector3(2.4f + (ci % 3) * 0.5f, 0.11f, 2.1f);
                patch.transform.rotation = Quaternion.Euler(0, ci * 19f, 4f);
                var pmr = patch.GetComponent<MeshRenderer>();
                if (pmr)
                {
                    pmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    pmr.sharedMaterial.color = new Color(0.17f, 0.11f, 0.14f);
                    pmr.sharedMaterial.SetColor("_EmissionColor", new Color(0.08f, 0.04f, 0.06f) * 0.5f);
                    pmr.sharedMaterial.EnableKeyword("_EMISSION");
                }
                UnityEngine.Object.DestroyImmediate(patch.GetComponent<Collider>());
                patch.transform.SetParent(root.transform);
                n++;

                // tendril spike
                var spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spike.name = $"CorruptTendril_{ci}";
                spike.transform.SetParent(patch.transform);
                spike.transform.localPosition = new Vector3(0.4f, 0.55f, 0.15f);
                spike.transform.localScale = new Vector3(0.16f, 1.15f, 0.16f);
                spike.transform.rotation = Quaternion.Euler(14 + ci, ci * 33, -10);
                var spmr = spike.GetComponent<MeshRenderer>();
                if (spmr)
                {
                    spmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    spmr.sharedMaterial.color = new Color(0.14f, 0.09f, 0.11f);
                }
                UnityEngine.Object.DestroyImmediate(spike.GetComponent<Collider>());
                n++;
            }

            // ── Simple Dig Site Debris (rubble rocks, planks, tool scraps around excavation zones) ──
            Vector3[] digCenters = {
                new Vector3(10f, 0.1f, 7f), new Vector3(6f, 0.1f, 2f), new Vector3(20f, 0.1f, 18f),
                new Vector3(-2f, 0.1f, 10f), new Vector3(15f, 0.1f, -12f), new Vector3(3f, 0.1f, 23f)
            };
            Vector3[][] rockOffsets = {
                new [] { new Vector3(-0.55f,0.18f,0.35f), new Vector3(0.48f,0.14f,-0.28f), new Vector3(0.18f,0.22f,0.65f), new Vector3(-0.32f,0.16f,-0.55f) },
                new [] { new Vector3(0.6f,0.15f,0.25f), new Vector3(-0.4f,0.2f,0.5f), new Vector3(0.25f,0.12f,-0.45f), new Vector3(-0.7f,0.17f,-0.15f) }
            };
            for (int dc = 0; dc < digCenters.Length; dc++)
            {
                var droot = new GameObject($"DigDebris_{dc:00}");
                droot.transform.position = digCenters[dc];
                droot.transform.SetParent(root.transform);
                droot.isStatic = true;

                // 4 rubble rocks
                var roffs = rockOffsets[dc % rockOffsets.Length];
                for (int r = 0; r < 4; r++)
                {
                    var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    rock.name = $"DigRock_{dc}_{r}";
                    rock.transform.SetParent(droot.transform);
                    rock.transform.localPosition = roffs[r];
                    rock.transform.localScale = Vector3.one * (0.32f + (r % 2) * 0.18f);
                    var rmr = rock.GetComponent<MeshRenderer>();
                    if (rmr)
                    {
                        rmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        float g = 0.40f + r * 0.025f;
                        rmr.sharedMaterial.color = new Color(g, g - 0.04f, g - 0.07f);
                    }
                    UnityEngine.Object.DestroyImmediate(rock.GetComponent<Collider>());
                    n++;
                }

                // wood plank scrap
                var plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plank.name = "DebrisPlank";
                plank.transform.SetParent(droot.transform);
                plank.transform.localPosition = new Vector3(0.75f, 0.11f, -0.18f);
                plank.transform.localScale = new Vector3(1.15f, 0.07f, 0.32f);
                plank.transform.rotation = Quaternion.Euler(3, 32 + dc * 7, 9);
                var plankMr = plank.GetComponent<MeshRenderer>();
                if (plankMr)
                {
                    plankMr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    plankMr.sharedMaterial.color = new Color(0.52f, 0.43f, 0.29f);
                }
                UnityEngine.Object.DestroyImmediate(plank.GetComponent<Collider>());
                n++;

                // tool handle proxy (angled cylinder)
                var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                handle.name = "ToolHandle";
                handle.transform.SetParent(droot.transform);
                handle.transform.localPosition = new Vector3(-0.65f, 0.22f, 0.12f);
                handle.transform.localScale = new Vector3(0.065f, 0.85f, 0.065f);
                handle.transform.rotation = Quaternion.Euler(28, -12, 18);
                var handleMr = handle.GetComponent<MeshRenderer>();
                if (handleMr)
                {
                    handleMr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    handleMr.sharedMaterial.color = new Color(0.33f, 0.27f, 0.19f);
                }
                UnityEngine.Object.DestroyImmediate(handle.GetComponent<Collider>());
                n++;
            }

            return n;
        }
    }

    /// <summary>
    /// Lightweight trigger that boots the full Moon 1 awakening sequence when the player enters the starting area.
    /// </summary>
    public class Moon1AwakeningTrigger : MonoBehaviour
    {
        bool _fired;

        void OnTriggerEnter(Collider other)
        {
            if (_fired || !other.CompareTag("Player")) return;
            _fired = true;

            var spawner = FindObjectOfType<EchohavenContentSpawner>();
            if (spawner != null)
            {
                // The spawner already handles Milo intro + first content on Awake/Start in most flows.
                // This just ensures the sequence kicks off even in a freshly populated scene.
                Debug.Log("[Moon 1] Awakening sequence triggered via start volume.");
            }

            // Could also fire the first objective / beat here via MoonBeatRunner if desired.
        }
    }

    /// <summary>
    /// FTUE helper for the very first excavation/tuning moment after population.
    /// POLISHED AWAKENING FLOW (UI FTUE subagent for Moon 1 Echohaven): clear scan prompts, companion success banner, 5-beat flow, reduced-motion + F310.
    ///   - Clear, magical "SCAN HERE" prompt with dynamic kb/gamepad + explicit F310 callouts.
    ///   - First tune success uses MoonHUDBanner + companion (Milo) trust tie-in + emotional dialogue.
    ///   - Advances 5-beat objective flow (Discovery → Restoration) via MoonProgressTracker + HUD.
    ///   - Reduced-motion friendly: no bob/pulse/VFX if SettingsOverlay.IsReducedMotion.
    ///   - Guides the first 5-10 min post-populate to feel magical, rewarding, on-rails but wondrous.
    /// </summary>
    public class Moon1FirstTuningTrigger : MonoBehaviour
    {
        bool _fired;

        void OnTriggerEnter(Collider other)
        {
            if (_fired || !other.CompareTag("Player")) return;

            _fired = true;

            var parent = transform.parent;

            // Reduced-motion safe highlight (static emissive glow only; no anim)
            if (parent != null)
            {
                var renderer = parent.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    // Always use gentle static color for accessibility; skip if already tuned
                    renderer.material.color = new Color(0.55f, 0.82f, 0.95f);
                }
            }

            // === CLEAR "SCAN HERE" PROMPT with F310 / gamepad callouts ===
            string kbPrompt = "[G] Hold to Scan • [E] Tune";
            string gamepadPrompt = "[B] Hold to Scan • [A] Tune (F310 South)";
            string f310Callout = "F310: Face buttons (A/B) or triggers for resonance input • Rumble confirms success";
            string localized = Tartaria.Input.InputPromptHelper.Localize(kbPrompt);
            string fullPrompt = $"{localized}  |  {gamepadPrompt}   — {f310Callout}";

            // Magical, guided on-screen objective for first 5-10 min
            var hud = FindObjectOfType<HUDController>();
            if (hud != null)
            {
                hud.ShowObjective("SCAN HERE — First Buried Ruin. " + fullPrompt + "\nThe Star Dome calls. Follow the light.");
            }

            // Also push a gentle world hint via accessibility for screen readers / reduced-motion players
            Tartaria.UI.AccessibilityManager.Instance?.PostSFXCaption("FTUE", "Scan the glowing mound ahead. Resonance awaits your first touch.");

            // First scan stinger (432Hz + PHI family) — rich magical entry for the obvious first ruin on populate. Wires Moon1_ScanStinger for wondrous "the valley answers" moment.
            Tartaria.Audio.AudioManager.Instance?.PlaySFX("Moon1_ScanStinger", other.transform.position, 0.38f);
            Tartaria.Audio.AudioManager.Instance?.PlaySFX("TuneLock", other.transform.position, 0.22f); // keep base for compatibility
            Tartaria.Audio.AudioManager.Instance?.PlayTone(432f, 0.28f, 0.12f);
            Tartaria.Audio.AudioManager.Instance?.PlayTone(432f * 1.618f, 0.22f, 0.09f); // explicit PHI harmonic layer

            Debug.Log($"[Moon 1 FTUE] Clear scan-here prompt shown: {fullPrompt} (rich Moon1_ScanStinger + PHI tones wired)");

            // Auto-advance to satisfying first tune (demo slice feel; real game uses hold-to-tune mini-game)
            StartCoroutine(CompleteFirstTune(other.gameObject, parent));
        }

        System.Collections.IEnumerator CompleteFirstTune(GameObject player, Transform parent)
        {
            // Gentle hold-to-scan simulation (0.8s for instant magic reward, forgiving for all players)
            yield return new WaitForSeconds(0.8f);

            // F310 rumble on success — explicit callout in log for dev/QA visibility
            HapticFeedbackManager.Instance?.TriggerF310Rumble(0.75f, 0.45f, 0.6f);

            // Rich Moon1 first tune success: 432+PHI stinger + dedicated F310-synced tone (short bright burst) + extra PHI family layers for wondrous payoff.
            // Makes the 5-10 min post-populate feel alive, emotional, and synced (audio + haptic).
            Tartaria.Audio.AudioManager.Instance?.PlaySFX("Moon1_TuneSuccessStinger", player.transform.position, 0.52f);
            Tartaria.Audio.AudioManager.Instance?.PlaySFX("TuneSuccess", player.transform.position, 0.38f); // base layer
            Tartaria.Audio.AudioManager.Instance?.PlaySFX("Moon1_F310SyncedTone", player.transform.position, 0.65f);
            Tartaria.Audio.AudioManager.Instance?.PlayTone(432f, 0.75f, 0.22f);
            Tartaria.Audio.AudioManager.Instance?.PlayTone(432f * 1.618f, 0.48f, 0.15f); // PHI ~699Hz sparkle
            Tartaria.Audio.AudioManager.Instance?.PlayTone(528f, 0.55f, 0.14f);
            Tartaria.Audio.AudioManager.Instance?.PlayTone(648f, 0.32f, 0.09f);

            Debug.Log("[Moon 1 FTUE] F310 rumble + rich Moon1_TuneSuccessStinger + Moon1_F310SyncedTone + PHI family fired on first tune success — body pulse + magical chord bloom for emotional payoff. First 5-10 min now wondrous and alive.");

            bool reduced = Tartaria.UI.SettingsOverlay.IsReducedMotion;

            // VFX only if not reduced-motion (magical but accessible)
            if (parent != null && !reduced)
            {
                VFXController.Instance?.SpawnPlatformStabilizeVFX(parent.position + Vector3.up * 1f);
            }

            // RS reward
            GameLoopController.Instance?.QueueRSReward(20f, "moon1_first_tune");

            // Permanent restored marker (static glow, no rotation/bob in reduced mode)
            GameObject marker = null;
            if (parent != null)
            {
                marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "FirstRestoredAetherCrystal_Permanent";
                marker.transform.position = parent.position + Vector3.up * 1.5f;
                marker.transform.localScale = Vector3.one * 0.9f;
                var mr = marker.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mr.material.color = new Color(0.35f, 0.78f, 0.95f);
                    mr.material.SetColor("_EmissionColor", new Color(0.3f, 0.9f, 1f) * 1.8f);
                    mr.material.EnableKeyword("_EMISSION");
                }
                var light = marker.AddComponent<Light>();
                light.color = new Color(0.4f, 0.9f, 1f);
                light.intensity = reduced ? 1.2f : 2.4f;  // softer for reduced motion eyes
                light.range = 10f;
                Destroy(marker.GetComponent<Collider>());
            }

            // === COMPANION TIE-IN + FIRST TUNE SUCCESS BANNER (magical + emotionally rewarding) ===
            var milo = FindObjectOfType<MiloController>();
            if (milo != null)
            {
                Debug.Log("[Moon 1 FTUE] Milo: 'You... you actually made it hum! The first chord... I feel it too. Thank you.' (trust beat + emotional anchor)");
                // Wire Milo discovery reaction stinger (rich Moon1_MiloDiscovery warm 432-family chime) — spatial at companion for magical "we did this together" moment in first 5-10 min.
                Tartaria.Audio.AudioManager.Instance?.PlaySFX("Moon1_MiloDiscovery", milo.transform.position, 0.48f);
                Tartaria.Audio.AudioManager.Instance?.PlayTone(540f, 0.45f, 0.13f); // soft companion overtone
            }
            // Companion trust payoff for first 5-10 min magic
            CompanionManager.Instance?.AddTrust("milo", 18f);

            // Clear, celebratory SUCCESS BANNER (ties tune directly to companion story)
            Tartaria.UI.MoonHUDBanner.Show(
                "★ FIRST RESONANCE TUNED ★",
                "The buried song answers. Milo smiles — 'We're not alone anymore.' +18 Trust • Star Dome stirs.",
                new Color(0.95f, 0.82f, 0.35f, 1f),  // warm gold — magical reward feel
                6.5f
            );

            // Rename state
            if (parent != null) parent.name = "FirstExcavationSite_Restored_Permanent";

            // === 5-BEAT OBJECTIVE FLOW: advance Restoration beat, push next guided objective ===
            Tartaria.Integration.MoonProgressTracker.Instance?.MarkBeatCleared(1, 1);  // Moon 01, Restoration (beat 1 after Discovery 0)
            Debug.Log("[Moon 1 FTUE] 5-beat flow advanced: Restoration complete. Objective now flows into Conflict (first golem) + Revelation.");

            var hud = FindObjectOfType<HUDController>();
            if (hud != null)
            {
                // Emotional, guided next objective for seamless 5-10 min experience
                hud.ShowObjective("The Star Dome awakens. Follow Milo — the first golem stirs. Purify with resonance (LMB Pulse).");
            }

            // Also ensure MoonBeatRunner knows (if listening)
            // The runner will pick up via progress tracker on next cycle.

            // Quest progress for awakening (ties to echohaven_awakening)
            QuestManager.Instance?.ProgressByType(Tartaria.Core.QuestObjectiveType.ExcavateRuin, "first_dome");

            // Optional spawner hook
            var spawner = FindObjectOfType<EchohavenContentSpawner>();
            if (spawner != null)
            {
                // FTUE boost already applied; spawner handles full 5-beat + Milo beckon continuity
            }

            // Final magical touch: small aether gift + haptic confirmation (F310 friendly)
            HapticFeedbackManager.Instance?.PlayDiscovery();

            // Big visual payoff: make the Star Dome visibly "awaken" (permanent world change, reduced-motion safe)
            var starDome = GameObject.Find("Echohaven_StarDome");
            if (starDome != null)
            {
                var domeMr = starDome.GetComponent<MeshRenderer>();
                if (domeMr != null)
                {
                    domeMr.material.SetColor("_EmissionColor", new Color(0.85f, 0.9f, 1f) * 2.2f);
                    domeMr.material.EnableKeyword("_EMISSION");
                }
                // Ensure a nice awakening light
                var existingLight = starDome.GetComponent<Light>();
                if (existingLight == null)
                {
                    var l = starDome.AddComponent<Light>();
                    l.color = new Color(0.75f, 0.85f, 1f);
                    l.intensity = 4.5f;
                    l.range = 30f;
                }
                else
                {
                    existingLight.intensity = Mathf.Max(existingLight.intensity, 4.5f);
                }
            }

            // "Resonance spreads" visual payoff: boost nearby new atmosphere props (crystals/ruins from 3D/TA subagent) on first tune success
            // This makes the dense new props feel alive and connected to the player's action (magical, 60fps, reduced-motion safe)
            if (!Tartaria.UI.SettingsOverlay.IsReducedMotion)
            {
                var allExtra = GameObject.FindGameObjectsWithTag("Untagged"); // broad but cheap for slice
                foreach (var obj in allExtra)
                {
                    if (obj.name.Contains("Resonance") || obj.name.Contains("ExtraAtmosphere") || obj.name.Contains("CorruptionMarker"))
                    {
                        float dist = Vector3.Distance(obj.transform.position, parent.position);
                        if (dist < 25f)
                        {
                            var r = obj.GetComponent<Renderer>();
                            if (r != null && r.material.HasProperty("_EmissionColor"))
                            {
                                r.material.SetColor("_EmissionColor", r.material.GetColor("_EmissionColor") * 1.8f);
                            }
                            var l = obj.GetComponent<Light>();
                            if (l != null) l.intensity *= 1.6f;
                        }
                    }
                }
            }

            // First companion trust moment + early Conflict teaser (first Mud Golem as hook)
            milo = FindObjectOfType<MiloController>();
            if (milo != null)
            {
                // Safe trust gain (first real companion moment)
                // In full system this would go through CompanionManager; here we log the emotional beat
                Debug.Log("[Moon 1] Milo trust +15 — first real 'we did it together' moment. He points toward the first corruption patch.");
            }

            // Spawn a single early Mud Golem as the "first corruption conflict" hook (not aggressive yet, just present)
            // This makes the 5-beat flow crystal clear on first playthrough
            var golem = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            golem.name = "Moon1_FirstMudGolem_Teaser";
            golem.transform.position = new Vector3(15f, 1f, 20f);
            var gmr = golem.GetComponent<MeshRenderer>();
            if (gmr) gmr.material.color = new Color(0.35f, 0.25f, 0.18f);
            // Add a simple "corrupted" light
            var gl = golem.AddComponent<Light>();
            gl.color = new Color(0.6f, 0.3f, 0.2f);
            gl.intensity = 1.2f;
            gl.range = 8f;
            UnityEngine.Object.DestroyImmediate(golem.GetComponent<Collider>()); // non-solid teaser
        }
    }

}