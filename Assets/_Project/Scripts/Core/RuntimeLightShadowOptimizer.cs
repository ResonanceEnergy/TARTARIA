using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Core
{
    /// <summary>
    /// R34: Runtime light shadow optimizer. Many RIOLM spawners (Moon1EnvironmentDetail,
    /// AmbientZoneController, etc.) add Point/Spot lights with Soft shadows by default.
    /// Soft point/spot shadows are 2-4× more expensive than Hard, and most cosmetic lights
    /// don't need the soft penumbra. Sweep after scene load, downgrade Soft to Hard, and
    /// disable shadows entirely on small (range &lt; 6m) cosmetic point lights.
    /// </summary>
    public static class RuntimeLightShadowOptimizer  // public so it can be called manually if needed
    {
        static bool _initialized;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BootstrapBefore() => Initialize();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void BootstrapAfter() { Initialize(); Apply(SceneManager.GetActiveScene()); SpawnDriver(); }
        static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        static void SpawnDriver()
        {
            // Avoid spawning duplicate drivers
            if (Object.FindFirstObjectByType<DelayedDriver>() != null) return;
            var driver = new GameObject("RuntimeLightShadowOptimizer_Driver");
            Object.DontDestroyOnLoad(driver);
            driver.AddComponent<DelayedDriver>();
        }
        /// <summary>External call: force-run the sweep on the active scene now. Useful for screenshots/manual perf tests.</summary>
        public static void Sweep() => Apply(SceneManager.GetActiveScene());

        // MonoBehaviour driver because the static class can't use coroutines/Update.
        class DelayedDriver : MonoBehaviour
        {
            float _t;
            int _sweeps;
            void Update()
            {
                _t += Time.unscaledDeltaTime;
                if (_sweeps == 0 && _t > 1.0f) { Apply(SceneManager.GetActiveScene()); _sweeps = 1; }
                else if (_sweeps == 1 && _t > 3.0f) { Apply(SceneManager.GetActiveScene()); _sweeps = 2; Destroy(gameObject); }
            }
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Apply(scene);

        static void Apply(Scene scene)
        {
            // R34 v3 fix: don't filter by scene. RIOLM spawners (Moon1EnvironmentDetail etc.) use DontDestroyOnLoad,
            // putting their lights in the special "DontDestroyOnLoad" scene rather than the active scene.
            // We want to optimize ALL lights, not just ones in the named active scene.
            var lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int downgraded = 0, killed = 0;
            foreach (var l in lights)
            {
                if (l == null) continue;
                // Cosmetic small lights: no shadows needed
                if ((l.type == LightType.Point || l.type == LightType.Spot) && l.range < 6f && l.shadows != LightShadows.None)
                {
                    l.shadows = LightShadows.None;
                    killed++;
                    continue;
                }
                // Soft Point/Spot -> Hard (preserve directional Soft shadows for hero look)
                if ((l.type == LightType.Point || l.type == LightType.Spot) && l.shadows == LightShadows.Soft)
                {
                    l.shadows = LightShadows.Hard;
                    downgraded++;
                }
            }
            if (downgraded + killed > 0)
                Debug.Log($"[RuntimeLightShadowOptimizer] '{scene.name}': downgraded {downgraded} soft->hard, killed shadows on {killed} small lights.");
        }
    }
}
