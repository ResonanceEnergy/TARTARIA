#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Generates a stub .unity scene for every Moon zone defined in
    /// <see cref="ZoneDefinitionFactory"/> that does not already have one
    /// on disk. Each stub includes:
    ///   • Ground plane (50 × 50, single material)
    ///   • Directional sunlight tinted to the zone's high-fog color
    ///   • Ambient + fog tuned from the zone definition
    ///   • PlayerSpawn marker at the zone's spawn position
    ///   • Skybox set from the procedural sky color (no asset deps)
    ///
    /// All stubs are placed in <c>Assets/_Project/Scenes/Moons/</c>.
    /// Build Settings is then rewritten to:
    ///   Boot(0) → Echohaven(1) → all 12 other moons (2..13) → UI_Overlay(14)
    ///
    /// Moon 1 Dev: Echohaven_VerticalSlice can be explicitly set as first scene
    /// for clean direct-launch Development Builds of the vertical slice.
    /// </summary>
    public static class MoonScenesFactory
    {
        const string MoonsDir          = "Assets/_Project/Scenes/Moons";
        const string EchohavenScene    = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        const string BootScene         = "Assets/_Project/Scenes/Boot.unity";
        const string UIOverlayScene    = "Assets/_Project/Scenes/UI_Overlay.unity";

        public struct MoonInfo
        {
            public int      number;
            public string   sceneName;
            public string   zoneName;
            public Vector3  spawnPos;
            public Color    fogLow;
            public Color    fogHigh;
            public float    fogDensity;
            public Color    ambientLow;
            public Color    ambientHigh;
        }

        // Mirrors ZoneDefinitionFactory data. Moon 1 is the existing
        // Echohaven scene, so it is excluded from stub generation.
        public static readonly MoonInfo[] Moons =
        {
            new(){ number=1,  sceneName="Echohaven_VerticalSlice", zoneName="Echohaven",
                   spawnPos=new(0,1,0),
                   fogLow=new(.35f,.28f,.20f), fogHigh=new(.80f,.75f,.50f), fogDensity=.035f,
                   ambientLow=new(.15f,.12f,.10f), ambientHigh=new(.60f,.55f,.40f) },
            new(){ number=2,  sceneName="CrystallineCaverns",     zoneName="Crystalline Caverns",
                   spawnPos=new(0,1,0),
                   fogLow=new(.15f,.18f,.25f), fogHigh=new(.40f,.55f,.70f), fogDensity=.020f,
                   ambientLow=new(.10f,.12f,.18f), ambientHigh=new(.40f,.50f,.65f) },
            new(){ number=3,  sceneName="WindsweptHighlands",     zoneName="Windswept Highlands",
                   spawnPos=new(0,1,0),
                   fogLow=new(.40f,.42f,.45f), fogHigh=new(.70f,.75f,.80f), fogDensity=.015f,
                   ambientLow=new(.20f,.20f,.22f), ambientHigh=new(.65f,.65f,.70f) },
            new(){ number=4,  sceneName="StarFortBastion",        zoneName="Star Fort Bastion",
                   spawnPos=new(0,1,0),
                   fogLow=new(.25f,.22f,.18f), fogHigh=new(.65f,.60f,.45f), fogDensity=.025f,
                   ambientLow=new(.15f,.13f,.10f), ambientHigh=new(.55f,.50f,.38f) },
            new(){ number=5,  sceneName="SunkenColosseum",        zoneName="Sunken Colosseum",
                   spawnPos=new(0,1,0),
                   fogLow=new(.20f,.25f,.30f), fogHigh=new(.50f,.60f,.70f), fogDensity=.030f,
                   ambientLow=new(.12f,.15f,.20f), ambientHigh=new(.45f,.55f,.65f) },
            new(){ number=6,  sceneName="LivingLibrary",          zoneName="Living Library",
                   spawnPos=new(0,1,0),
                   fogLow=new(.18f,.15f,.12f), fogHigh=new(.55f,.48f,.35f), fogDensity=.020f,
                   ambientLow=new(.12f,.10f,.08f), ambientHigh=new(.50f,.42f,.30f) },
            new(){ number=7,  sceneName="ClockworkCitadel",       zoneName="Clockwork Citadel",
                   spawnPos=new(0,1,0),
                   fogLow=new(.22f,.20f,.18f), fogHigh=new(.60f,.55f,.45f), fogDensity=.018f,
                   ambientLow=new(.15f,.13f,.11f), ambientHigh=new(.50f,.45f,.35f) },
            new(){ number=8,  sceneName="VerdantCanopy",          zoneName="Verdant Canopy",
                   spawnPos=new(0,1,0),
                   fogLow=new(.12f,.22f,.12f), fogHigh=new(.35f,.65f,.35f), fogDensity=.040f,
                   ambientLow=new(.08f,.15f,.08f), ambientHigh=new(.30f,.55f,.30f) },
            new(){ number=9,  sceneName="AuroralSpire",           zoneName="Auroral Spire",
                   spawnPos=new(0,1,0),
                   fogLow=new(.15f,.12f,.20f), fogHigh=new(.50f,.45f,.70f), fogDensity=.012f,
                   ambientLow=new(.10f,.08f,.15f), ambientHigh=new(.45f,.40f,.65f) },
            new(){ number=10, sceneName="DeepForge",              zoneName="Deep Forge",
                   spawnPos=new(0,1,0),
                   fogLow=new(.30f,.15f,.08f), fogHigh=new(.70f,.40f,.20f), fogDensity=.050f,
                   ambientLow=new(.20f,.10f,.05f), ambientHigh=new(.60f,.35f,.15f) },
            new(){ number=11, sceneName="TidalArchive",           zoneName="Tidal Archive",
                   spawnPos=new(0,1,0),
                   fogLow=new(.18f,.22f,.28f), fogHigh=new(.45f,.55f,.65f), fogDensity=.028f,
                   ambientLow=new(.12f,.15f,.20f), ambientHigh=new(.40f,.50f,.60f) },
            new(){ number=12, sceneName="CelestialObservatory",   zoneName="Celestial Observatory",
                   spawnPos=new(0,1,0),
                   fogLow=new(.08f,.08f,.15f), fogHigh=new(.20f,.20f,.40f), fogDensity=.008f,
                   ambientLow=new(.05f,.05f,.10f), ambientHigh=new(.20f,.20f,.35f) },
            new(){ number=13, sceneName="PlanetaryNexus",         zoneName="Planetary Nexus",
                   spawnPos=new(0,1,0),
                   fogLow=new(.15f,.12f,.08f), fogHigh=new(.90f,.85f,.60f), fogDensity=.010f,
                   ambientLow=new(.10f,.08f,.05f), ambientHigh=new(.85f,.80f,.55f) },
        };

        public static string ScenePathFor(MoonInfo moon)
        {
            return moon.number == 1
                ? EchohavenScene
                : $"{MoonsDir}/{moon.sceneName}.unity";
        }

        [MenuItem("TARTARIA/Integration/Create Missing Moon Scenes (2-13)")]
        public static void CreateAllMenu() => CreateAll();

        public static void CreateAll()
        {
            EnsureFolder(MoonsDir);

            int created = 0;
            foreach (var m in Moons)
            {
                if (m.number == 1) continue; // Echohaven already exists
                string path = ScenePathFor(m);
                if (File.Exists(path)) continue;
                CreateStub(m, path);
                created++;
            }

            UpdateBuildSettings();
            Debug.Log($"[MoonScenes] Created {created} stub moon scenes; Build Settings rebuilt.");
        }

        static void CreateStub(MoonInfo moon, string path)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Ambient + fog ──
            RenderSettings.ambientMode      = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor    = moon.ambientHigh;
            RenderSettings.ambientEquatorColor = Color.Lerp(moon.ambientLow, moon.ambientHigh, .5f);
            RenderSettings.ambientGroundColor  = moon.ambientLow;
            RenderSettings.fog            = true;
            RenderSettings.fogMode        = FogMode.ExponentialSquared;
            RenderSettings.fogColor       = moon.fogHigh;
            RenderSettings.fogDensity     = moon.fogDensity;

            // ── Ground plane ──
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = $"Moon{moon.number:D2}_Ground";
            ground.transform.localScale = new Vector3(10, 1, 10); // 100×100 m
            var groundRend = ground.GetComponent<Renderer>();
            var groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            groundMat.color = Color.Lerp(moon.ambientLow, moon.fogHigh, .35f);
            groundRend.sharedMaterial = groundMat;

            // ── Directional sun ──
            var sun = new GameObject("Sun");
            var l = sun.AddComponent<Light>();
            l.type      = LightType.Directional;
            l.color     = Color.Lerp(moon.fogHigh, Color.white, .25f);
            l.intensity = 1.1f;
            l.shadows   = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45f, 30f + moon.number * 6f, 0f);

            // ── Player spawn marker ──
            var spawn = new GameObject("PlayerSpawn");
            spawn.transform.position = moon.spawnPos;
            spawn.tag = "Respawn";

            // ── Camera ──
            var camGO = new GameObject("MainCamera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.backgroundColor = moon.fogHigh;
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = moon.spawnPos + new Vector3(0, 4, -8);
            camGO.transform.rotation = Quaternion.Euler(20f, 0f, 0f);

            // ── Zone label ──
            var label = new GameObject($"--- {moon.zoneName} (Moon {moon.number}) ---");
            label.transform.SetSiblingIndex(0);

            EditorSceneManager.MarkSceneDirty(scene);
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            EditorSceneManager.SaveScene(scene, path);
        }

        public static void UpdateBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene>();
            TryAdd(list, BootScene);
            TryAdd(list, EchohavenScene);
            foreach (var m in Moons)
            {
                if (m.number == 1) continue;
                TryAdd(list, ScenePathFor(m));
            }
            TryAdd(list, UIOverlayScene);
            EditorBuildSettings.scenes = list.ToArray();

            Debug.Log($"[MoonScenes] BuildSettings now has {list.Count} scenes "
                      + $"(Boot + Echohaven + 12 moons + UI_Overlay).");
        }

        /// <summary>
        /// Configures EditorBuildSettings for a clean Moon 1 Development Build:
        /// Echohaven_VerticalSlice as the very first scene (index 0).
        /// This allows the built Development player to launch *directly* into
        /// the Echohaven vertical slice gameplay without Boot overhead.
        /// RuntimeInitialize bootstraps (GameBootstrap, SceneLoader, etc.) auto-create.
        /// UI_Overlay follows for HUD. Other moons omitted for focused Moon 1 dev iteration.
        /// </summary>
        [MenuItem("Tartaria/Configure Moon 1 Dev Build Settings (Echohaven first)")]
        public static void ConfigureMoon1DevBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene>();
            TryAdd(list, EchohavenScene);
            TryAdd(list, UIOverlayScene);
            // Note: Boot intentionally not first (and can be omitted) so Echohaven starts the player.
            // Full pipeline still available via normal UpdateBuildSettings.
            EditorBuildSettings.scenes = list.ToArray();

            Debug.Log("[MoonScenes] Moon 1 Dev Build Settings configured: Echohaven_VerticalSlice as FIRST scene for clean direct-launch Development Build.");
        }

        static void TryAdd(List<EditorBuildSettingsScene> list, string path)
        {
            if (File.Exists(path) && AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
                list.Add(new EditorBuildSettingsScene(path, true));
            else
                Debug.LogWarning($"[MoonScenes] Build Settings — skipping missing scene: {path}");
        }

        static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
#endif
