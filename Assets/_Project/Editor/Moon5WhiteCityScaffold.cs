using UnityEngine;
using UnityEditor;
using Tartaria.Core;
using Tartaria.Integration;
using System.Collections.Generic;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 5 Scaffolding: Overtone Moon — White City Echo District (1893 World's Fair pavilions).
    /// "The Radiance of Empowerment" — 5 pavilions, floating platforms, airship dock, 6-band healing, spire bridge climax.
    ///
    /// One-click population for immediate vertical slice play:
    ///   - 5 Beaux-Arts style pavilions (platforms + tuning nodes)
    ///   - Grand ionized fountains with aurora potential
    ///   - Central spire base for fragment placement (Intercontinental Bridge climax)
    ///   - Airship dock foundation + construction stages
    ///   - Floating platform proxies that rise on amplification success
    ///   - WhiteCityAmplificationController wired + start trigger volume
    ///   - Thorne radio contact volume
    ///
    /// Menu: Tartaria > Populate Moon 5 (White City Echo District)
    /// Open a clean scene (or WhiteCityEchoDistrict.unity) then run.
    /// Mirrors exact Moon 3 scaffold success pattern for instant playable slice.
    /// </summary>
    public static class Moon5WhiteCityScaffold
    {
        const string Moon5SceneHint = "WhiteCityEchoDistrict";

        [MenuItem("Tartaria/Build Assets/Moon 5 — Populate White City (Overtone)", false, 40)]
        public static void PopulateMoon5WhiteCity()
        {
            int created = 0;

            // Core controller
            created += SetupWhiteCityAmplificationController();

            // Static world anchors (pavilions, fountains, spire, dock)
            created += PlacePavilionsAndFountains();
            created += PlaceCentralSpireAndLeyAnchor();
            created += PlaceAirshipDockFoundation();
            created += PlaceFloatingPlatformProxies();

            // Atmosphere polish: 8 cheap glowing resonance lanterns + floating crystals (magical 1893 World's Fair night)
            created += PlaceResonanceLanternsAndCrystals();

            // Start trigger + Thorne radio volume
            created += CreateStartVolumeAndThorneRadio();

            // Lighting / post hint (gold warm overtone bias)
            created += ApplyOvertoneAtmosphere();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Tartaria] Moon 5 White City Echo District populated. {created} elements placed. Open scene and walk the start volume to begin the Overtone slice.");
        }

        static int SetupWhiteCityAmplificationController()
        {
            var existing = Object.FindObjectOfType<WhiteCityAmplificationController>();
            if (existing != null) return 0;

            var go = new GameObject("Moon5_WhiteCityAmplificationController");
            var ctrl = go.AddComponent<WhiteCityAmplificationController>();

            // Center of the district (matches MoonDefinition spawn + scaffold layout)
            ctrl.districtCenter = new Vector3(28f, 1.5f, 4f);

            // Audio heart for the district (amplification harmonics, aurora fountains, Thorne radio, bridge motif)
            var audioGO = new GameObject("Moon5_WhiteCityAudioManager");
            audioGO.transform.SetParent(go.transform);
            audioGO.AddComponent<Tartaria.Audio.Moon5WhiteCityAudioManager>();

            // 5 pavilion world positions (Beaux-Arts crescent layout)
            ctrl.pavilionPositions = new[]
            {
                new Vector3(12f, 0.8f, -14f),   // Pavilion 1 - Grand Hall (west)
                new Vector3(38f, 0.8f, -16f),   // Pavilion 2 - Gallery of Light
                new Vector3(52f, 0.8f, 2f),     // Pavilion 3 - Fountain Court
                new Vector3(42f, 0.8f, 22f),    // Pavilion 4 - Resonance Dome
                new Vector3(14f, 0.8f, 18f)     // Pavilion 5 - East Promenade
            };

            // Spire placement target (climax)
            ctrl.spireBasePosition = new Vector3(28f, 0.5f, 3f);

            // Airship dock
            ctrl.dockPosition = new Vector3(68f, 1.2f, 28f);

            // Reference the existing MoonDefinition asset for beat tracking
            var moonDef = AssetDatabase.LoadAssetAtPath<MoonDefinition>("Assets/_Project/Config/Moons/Moon05_WhiteCityEcho.asset");
            if (moonDef != null)
            {
                // MoonMechanicActivator will pick this up if present on same root or via bootstrap
            }

            return 1;
        }

        static int PlacePavilionsAndFountains()
        {
            int n = 0;
            Vector3[] pos = {
                new Vector3(12f, 0.8f, -14f),
                new Vector3(38f, 0.8f, -16f),
                new Vector3(52f, 0.8f, 2f),
                new Vector3(42f, 0.8f, 22f),
                new Vector3(14f, 0.8f, 18f)
            };

            for (int i = 0; i < pos.Length; i++)
            {
                string name = $"Moon5_Pavilion_{i+1:00}";
                if (GameObject.Find(name) != null) continue;

                // Main body - taller for Beaux-Arts grandeur
                var p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                p.name = name;
                p.transform.position = pos[i];
                p.transform.localScale = new Vector3(11f, 4.5f, 11f);

                // Cream stone + copper overtone tint (visual proxy until real KayKit/Beaux-Arts prefabs)
                var mr = p.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mr.sharedMaterial.color = (i % 2 == 0) ? new Color(0.95f, 0.90f, 0.82f) : new Color(0.88f, 0.85f, 0.78f);
                }

                // Fluted columns around the pavilion (classic 1893 World's Fair look)
                int columns = 10;
                float radius = 6.2f;
                for (int c = 0; c < columns; c++)
                {
                    float angle = (c / (float)columns) * Mathf.PI * 2f;
                    var colGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    colGO.name = $"{name}_Column_{c}";
                    colGO.transform.position = pos[i] + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                    colGO.transform.localScale = new Vector3(0.8f, 4.8f, 0.8f);
                    var cmr = colGO.GetComponent<MeshRenderer>();
                    if (cmr) cmr.sharedMaterial = mr.sharedMaterial;
                    UnityEngine.Object.DestroyImmediate(colGO.GetComponent<Collider>()); // no physics needed for proxy
                }

                // Massive dome on top (Beaux-Arts signature)
                var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dome.name = $"{name}_Dome";
                dome.transform.position = pos[i] + Vector3.up * 5.8f;
                dome.transform.localScale = new Vector3(10f, 5f, 10f);
                var dmr = dome.GetComponent<MeshRenderer>();
                if (dmr)
                {
                    dmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    dmr.sharedMaterial.color = new Color(0.92f, 0.88f, 0.78f);
                }
                UnityEngine.Object.DestroyImmediate(dome.GetComponent<Collider>());

                // Main body collider — keep solid for nice "walk on the pavilion base" proxy feel
                var col = p.GetComponent<Collider>();
                col.isTrigger = false; // solid platform

                // Full interaction: Click OR walk close + E (F310 South button supported)
                var amp = p.AddComponent<Tartaria.Integration.Moon5PavilionClickAmplifier>();
                amp.pavilionIndex = i;

                var inter = p.AddComponent<Tartaria.Integration.Moon5PavilionInteractor>();
                inter.pavilionIndex = i;

                // Dedicated proximity trigger sphere for clean E/hold interaction (doesn't interfere with walking)
                var prox = p.AddComponent<SphereCollider>();
                prox.isTrigger = true;
                prox.radius = 9f;

                n++;
            }

            // Grand ionized fountain basins (3 spectacular ones)
            for (int f = 0; f < 3; f++)
            {
                var fgo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                fgo.name = $"Moon5_AuroraFountain_{f+1}";
                fgo.transform.position = new Vector3(22 + f * 9f, 0.3f, -2 + f * 1.5f);
                fgo.transform.localScale = new Vector3(7f, 0.6f, 7f);
                var mr = fgo.GetComponent<MeshRenderer>();
                if (mr) mr.sharedMaterial.color = new Color(0.6f, 0.85f, 0.95f);
                n++;
            }

            return n;
        }

        static int PlaceCentralSpireAndLeyAnchor()
        {
            if (GameObject.Find("Moon5_SpiresAnchor") != null) return 0;

            var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spire.name = "Moon5_SpiresAnchor";
            spire.transform.position = new Vector3(28f, 6f, 3f);
            spire.transform.localScale = new Vector3(2.8f, 14f, 2.8f);

            var mr = spire.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mr.sharedMaterial.color = new Color(0.98f, 0.92f, 0.65f); // warm gold
            }

            // Ley line visual anchors (simple spheres for now — LeyLineVisualizer will take over)
            for (int i = 0; i < 5; i++)
            {
                var ley = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ley.name = $"Moon5_LeyNode_{i}";
                ley.transform.position = new Vector3(28f + Mathf.Cos(i * 1.256f) * 38f, 1.5f, 3f + Mathf.Sin(i * 1.256f) * 28f);
                ley.transform.localScale = Vector3.one * 1.8f;
            }

            // Spire placement trigger volume (player approaches when pavilions done → triggers the aurora bridge climax)
            var spireVol = new GameObject("Moon5_SpirePlacement_Volume");
            spireVol.transform.position = new Vector3(28f, 2f, 3f);
            var spCol = spireVol.AddComponent<SphereCollider>();
            spCol.isTrigger = true;
            spCol.radius = 5.5f;
            spireVol.AddComponent<Tartaria.Integration.Moon5SpirePlacementTrigger>();

            return 7;
        }

        static int PlaceAirshipDockFoundation()
        {
            if (GameObject.Find("Moon5_AirshipDock") != null) return 0;

            var dock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dock.name = "Moon5_AirshipDock";
            dock.transform.position = new Vector3(68f, 1.8f, 28f);
            dock.transform.localScale = new Vector3(22f, 1.2f, 38f);

            var mr = dock.GetComponent<MeshRenderer>();
            if (mr) mr.sharedMaterial.color = new Color(0.75f, 0.72f, 0.65f);

            // Landing pad markers
            for (int m = 0; m < 4; m++)
            {
                var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pad.name = $"Moon5_DockPad_{m}";
                pad.transform.position = dock.transform.position + new Vector3(-7 + m * 5f, 1.1f, -12 + (m % 2) * 24f);
                pad.transform.localScale = new Vector3(3.5f, 0.3f, 3.5f);
            }

            return 5;
        }

        static int PlaceFloatingPlatformProxies()
        {
            int n = 0;
            // 6 floating platforms that rise dramatically when pavilions are amplified
            Vector3[] fp = {
                new Vector3(8f, 4f, -22f), new Vector3(48f, 5f, -20f),
                new Vector3(58f, 6f, 12f), new Vector3(18f, 7f, 26f),
                new Vector3(30f, 9f, -8f), new Vector3(35f, 8f, 32f)
            };

            for (int i = 0; i < fp.Length; i++)
            {
                var plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plat.name = $"Moon5_FloatingPlatform_{i+1}";
                plat.transform.position = fp[i];
                plat.transform.localScale = new Vector3(6f, 0.8f, 6f);

                var mr = plat.GetComponent<MeshRenderer>();
                if (mr) mr.sharedMaterial.color = new Color(0.85f, 0.92f, 0.98f, 0.6f);

                n++;
            }
            return n;
        }

        static int PlaceResonanceLanternsAndCrystals()
        {
            if (GameObject.Find("Moon5_ResonanceLantern_01") != null) return 0;

            int n = 0;
            var decorRoot = new GameObject("Moon5_AtmosphereDecor");
            decorRoot.isStatic = true;

            // 8 cheap glowing resonance lantern posts + floating crystals scattered around district,
            // fountains, dock and spire. Warm gold lanterns (classic 1893 fair) + cool resonance cyan crystals.
            // Pure primitives + emissive + point lights. Instant magical atmosphere on first Populate. 60fps.
            Vector3[] positions = {
                new Vector3(22f, 1.2f, -8f),    // near fountains west
                new Vector3(35f, 1.2f, -9f),
                new Vector3(26f, 0.9f, 12f),    // path to pavilion 5
                new Vector3(48f, 1.1f, 8f),     // near pavilion 3/4
                new Vector3(28f, 2.5f, -2f),    // spire base west float crystal
                new Vector3(30f, 1.8f, 8f),     // spire east
                new Vector3(60f, 1.5f, 18f),    // dock approach
                new Vector3(75f, 2.2f, 35f)     // dock far edge, floating
            };

            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 basePos = positions[i];
                bool isLantern = (i % 2 == 0);

                if (isLantern)
                {
                    // Bronze post + glowing lantern head
                    var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    post.name = $"Moon5_ResonanceLantern_{i+1:00}_Post";
                    post.transform.position = basePos;
                    post.transform.localScale = new Vector3(0.28f, 3.2f, 0.28f);
                    var pmr = post.GetComponent<MeshRenderer>();
                    if (pmr)
                    {
                        pmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        pmr.sharedMaterial.color = new Color(0.35f, 0.32f, 0.28f);
                    }
                    UnityEngine.Object.DestroyImmediate(post.GetComponent<Collider>());
                    post.transform.SetParent(decorRoot.transform);
                    n++;

                    var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    head.name = $"Moon5_ResonanceLantern_{i+1:00}";
                    head.transform.position = basePos + Vector3.up * 3.35f;
                    head.transform.localScale = Vector3.one * 0.65f;
                    var hmr = head.GetComponent<MeshRenderer>();
                    if (hmr)
                    {
                        hmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        hmr.sharedMaterial.color = new Color(0.98f, 0.92f, 0.65f);
                        hmr.sharedMaterial.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.4f) * 2.8f);
                        hmr.sharedMaterial.EnableKeyword("_EMISSION");
                    }
                    UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());
                    var lgt = head.AddComponent<Light>();
                    lgt.type = LightType.Point;
                    lgt.color = new Color(1f, 0.9f, 0.6f);
                    lgt.intensity = 2.4f;
                    lgt.range = 14f;
                    lgt.shadows = LightShadows.None;
                    head.transform.SetParent(decorRoot.transform);
                    n++;
                }
                else
                {
                    // Floating resonance crystal (hovering, no post)
                    var cry = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    cry.name = $"Moon5_FloatingCrystal_{i+1:00}";
                    cry.transform.position = basePos + Vector3.up * 2.8f;
                    cry.transform.localScale = Vector3.one * 0.55f;
                    var cmr = cry.GetComponent<MeshRenderer>();
                    if (cmr)
                    {
                        cmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        cmr.sharedMaterial.color = new Color(0.7f, 0.95f, 0.98f);
                        cmr.sharedMaterial.SetColor("_EmissionColor", new Color(0.6f, 0.95f, 1f) * 3.2f);
                        cmr.sharedMaterial.EnableKeyword("_EMISSION");
                    }
                    UnityEngine.Object.DestroyImmediate(cry.GetComponent<Collider>());
                    var clgt = cry.AddComponent<Light>();
                    clgt.type = LightType.Point;
                    clgt.color = new Color(0.6f, 0.95f, 1f);
                    clgt.intensity = 1.9f;
                    clgt.range = 11f;
                    clgt.shadows = LightShadows.None;
                    cry.transform.SetParent(decorRoot.transform);
                    n++;
                }
            }

            return n;
        }

        static int CreateStartVolumeAndThorneRadio()
        {
            int n = 0;

            // Start trigger (player walks in = boot amplification sequence + first Thorne contact)
            if (GameObject.Find("Moon5_StartAmplification_Volume") == null)
            {
                var vol = new GameObject("Moon5_StartAmplification_Volume");
                vol.transform.position = new Vector3(25f, 2f, -12f);
                var col = vol.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 7f;
                vol.AddComponent<Moon5StartTrigger>();
                n++;
            }

            // Thorne radio static volume (distant captain voice)
            if (GameObject.Find("Moon5_ThorneRadio_Volume") == null)
            {
                var radio = new GameObject("Moon5_ThorneRadio_Volume");
                radio.transform.position = new Vector3(30f, 3f, 0f);
                var rcol = radio.AddComponent<SphereCollider>();
                rcol.isTrigger = true;
                rcol.radius = 9f;
                radio.AddComponent<Moon5ThorneRadioTrigger>();
                n++;
            }

            return n;
        }

        static int ApplyOvertoneAtmosphere()
        {
            // Soft warm golden bias + reduced fog for the "radiance" feel
            var cam = UnityEngine.Camera.main;
            if (cam != null)
            {
                var pp = cam.GetComponent<TartariaPostProcessing>();
                if (pp != null) { /* already handles global */ }
            }
            return 1;
        }
    }

    // Triggers are defined in WhiteCityAmplificationController.cs (runtime assembly)
}
