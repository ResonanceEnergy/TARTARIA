using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets; // R6 Moon streaming

namespace Tartaria.Core
{
    /// <summary>
    /// Game Bootstrap — initializes the ECS world, creates singleton entities,
    /// and sets up the Aether field configuration.
    /// Attached to the Boot scene's bootstrap GameObject.
    /// 
    /// R6: + MemoryWatchdog init, dynamic resolution lerp, Moon2/3 Addressables streaming hooks,
    /// hardened tier + guard integration.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        [Header("Aether Field Configuration")]
        [SerializeField, Min(1)] int aetherGridX = 64;
        [SerializeField, Min(1)] int aetherGridY = 64;
        [SerializeField, Min(1)] int aetherGridZ = 32;
        [SerializeField, Min(0.1f)] float aetherCellSize = 2.0f;
        [SerializeField, Range(0f, 1f)] float aetherDissipation = 0.05f;
        [SerializeField, Min(0f)] float aetherAdvectionSpeed = 1.0f;

        [Header("Performance (Round 4-6)")]
        [SerializeField] PerformanceProfile performanceProfile;
        [SerializeField] bool autoDetectHardwareTier = true;

        // R6 dynamic resolution state
        float _targetRenderScale = 1.0f;
        float _currentRenderScale = 1.0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("GameBootstrap");
            DontDestroyOnLoad(go);
            go.AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            // R6: MemoryWatchdog for profiling + leak hunting on Moon2/3 dense
            if (FindAnyObjectByType<MemoryWatchdog>() == null)
            {
                var mw = new GameObject("MemoryWatchdog_R6").AddComponent<MemoryWatchdog>();
                DontDestroyOnLoad(mw.gameObject);
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            if (!InitializeECSWorld())
            {
                Debug.LogError("[Tartaria] ECS world initialization failed — cannot proceed.");
                return;
            }

            // Phase 3 Round 4-6: Full hardware tier profiles + auto fallback + memory watchdog
            InitializePerformanceTiers();

            // R6: Dynamic resolution start + Moon streaming
            _currentRenderScale = performanceProfile != null ? performanceProfile.renderScale : 1f;
            _targetRenderScale = _currentRenderScale;
            _ = AddressableAssetLoader.InitializeAsync();

            // Day-13: gate behind main menu "Start" / "Continue" click.
            bool autoStart = PlayerPrefs.GetInt("TARTARIA_SkipMainMenu", 0) == 1;
            if (autoStart || !MainMenuActive)
            {
                TriggerSceneLoad();
            }
        }

        // Set true by MainMenuOverlay before scene-load to keep GameBootstrap waiting.
        public static bool MainMenuActive;

        public void TriggerSceneLoad()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            var sceneLoader = FindAnyObjectByType<SceneLoader>();
            if (sceneLoader != null)
            {
                sceneLoader.LoadGameplayScenes();
            }
            else
            {
                Debug.LogWarning("[Tartaria] No SceneLoader found — falling back to direct state transition.");
                GameStateManager.Instance.TransitionTo(GameState.Loading);
                GameStateManager.Instance.TransitionTo(GameState.Exploration);
            }
        }

        /// <summary>Day-13: invoked by MainMenuOverlay when player clicks Start / Continue.</summary>
        public static void BeginGameplay()
        {
            MainMenuActive = false;
            Instance?.TriggerSceneLoad();
        }

        bool InitializeECSWorld()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                Debug.LogError("[Tartaria] DefaultGameObjectInjectionWorld is null — Entities package may not be initialized.");
                return false;
            }

            if (aetherGridX <= 0 || aetherGridY <= 0 || aetherGridZ <= 0)
            {
                Debug.LogError($"[Tartaria] Invalid Aether grid dimensions: {aetherGridX}x{aetherGridY}x{aetherGridZ}");
                return false;
            }

            var em = world.EntityManager;

            var configQuery = em.CreateEntityQuery(typeof(AetherFieldConfig));
            if (configQuery.CalculateEntityCount() > 0)
            {
                configQuery.Dispose();
                Debug.Log("[Tartaria] ECS world already initialized — skipping duplicate creation.");
                return true;
            }
            configQuery.Dispose();

            var configEntity = em.CreateEntity();
            em.AddComponentData(configEntity, new AetherFieldConfig
            {
                GridSizeX = aetherGridX,
                GridSizeY = aetherGridY,
                GridSizeZ = aetherGridZ,
                CellSize = aetherCellSize,
                DissipationRate = aetherDissipation,
                AdvectionSpeed = aetherAdvectionSpeed
            });

            var playerEntity = em.CreateEntity();
            em.AddComponentData(playerEntity, new PlayerTag());
            em.AddComponentData(playerEntity, new LocalTransform
            {
                Position = new float3(0f, 1f, -20f),
                Rotation = quaternion.identity,
                Scale = 1f
            });

            Debug.Log("[Tartaria] ECS world initialized. Aether field configured. (R6 MemoryWatchdog + dyn res active)");
            return true;
        }

        // ─── Round 4-6 Performance: Hardware Tier + Persistence + Fallback + Dyn Res ──────
        void InitializePerformanceTiers()
        {
            if (performanceProfile == null)
            {
                performanceProfile = Resources.Load<PerformanceProfile>("PerformanceProfile");
                if (performanceProfile == null)
                {
                    performanceProfile = ScriptableObject.CreateInstance<PerformanceProfile>();
                    Debug.LogWarning("[GameBootstrap] No PerformanceProfile asset — using runtime default for tiers.");
                }
            }

            var tier = performanceProfile.tier;
            if (autoDetectHardwareTier)
            {
                tier = DetectHardwareTier();
                PlayerPrefs.SetInt("TARTARIA_LastHardwareTier", (int)tier);
            }
            else
            {
                int saved = PlayerPrefs.GetInt("TARTARIA_ActivePerfTier", (int)tier);
                tier = (PerformanceProfile.HardwareTier)Mathf.Clamp(saved, 0, 3);
            }

            performanceProfile.ApplyTierDefaults(tier);

            int qLevel = Mathf.Clamp((int)tier, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(qLevel, true);

            Debug.Log($"[GameBootstrap] Perf tier initialized: {performanceProfile.GetTierSummary()} (R6 dyn res + watchdog ready)");
        }

        static PerformanceProfile.HardwareTier DetectHardwareTier()
        {
            string gpu = SystemInfo.graphicsDeviceName.ToLowerInvariant();
            int cores = SystemInfo.processorCount;
            int memMB = SystemInfo.systemMemorySize;
            int gfxMem = SystemInfo.graphicsMemorySize;

            if (gfxMem < 2048 || memMB < 8192 || gpu.Contains("intel") || (gpu.Contains("gtx 10") && !gpu.Contains("70")))
                return PerformanceProfile.HardwareTier.Low;
            if (gfxMem < 4096 || memMB < 12288 || cores < 6)
                return PerformanceProfile.HardwareTier.Medium;
            if (gfxMem < 8192 || memMB < 16384)
                return PerformanceProfile.HardwareTier.High;
            return PerformanceProfile.HardwareTier.Ultra;
        }

        public static void TriggerAutoQualityFallback(string reason)
        {
            var inst = Instance;
            if (inst == null || inst.performanceProfile == null || !inst.performanceProfile.autoFallbackEnabled) return;

            var p = inst.performanceProfile;
            int current = (int)p.tier;
            if (current > 0)
            {
                var next = (PerformanceProfile.HardwareTier)(current - 1);
                p.fallbackCount++;
                p.ApplyTierDefaults(next);
                PlayerPrefs.SetInt("TARTARIA_ActivePerfTier", (int)next);
                PlayerPrefs.SetInt("TARTARIA_FallbackCount", p.fallbackCount);
                PlayerPrefs.Save();

                int q = Mathf.Clamp((int)next, 0, QualitySettings.names.Length - 1);
                QualitySettings.SetQualityLevel(q, true);

                Debug.LogWarning($"[GameBootstrap] AUTO-FALLBACK to {next} — {reason}. Fallbacks: {p.fallbackCount}");
                GameEvents.FirePerformanceFallback(next.ToString(), reason);
            }
        }

        // ─── Round 5-6: Dynamic tier switching + hardened Dynamic Resolution ─────────
        public static void ApplyRuntimePerformanceTier(PerformanceProfile.HardwareTier newTier)
        {
            var inst = Instance;
            if (inst == null || inst.performanceProfile == null) return;

            var p = inst.performanceProfile;
            p.ApplyTierDefaults(newTier);
            PlayerPrefs.SetInt("TARTARIA_ActivePerfTier", (int)newTier);
            PlayerPrefs.Save();

            int q = Mathf.Clamp((int)newTier, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(q, true);
            QualitySettings.lodBias = p.lodBias;
            QualitySettings.shadowDistance = p.shadowDistance;

            inst._targetRenderScale = p.renderScale; // R6 dyn res target

            TryReconfigureAetherGridFromProfile(p);

            GameEvents.FirePerformanceFallback(newTier.ToString(), "Runtime manual tier switch (R6 + dyn res)");

            Debug.Log($"[GameBootstrap] RUNTIME TIER SWITCH → {newTier} (dyn res active, no restart)");
        }

        void Update()
        {
            // R6: Lightweight dynamic resolution lerp for low-end (tied to guard avg)
            if (performanceProfile != null && performanceProfile.tier <= PerformanceProfile.HardwareTier.Medium)
            {
                var guard = PerformanceGuard.Instance;
                if (guard != null)
                {
                    float avg = guard.AverageFrameTimeMs;
                    float targetFps = performanceProfile.targetFrameRate;
                    float frameBudget = 1000f / targetFps;

                    if (avg > frameBudget * 1.15f)
                        _targetRenderScale = Mathf.Max(0.55f, _targetRenderScale - 0.015f);
                    else if (avg < frameBudget * 0.85f)
                        _targetRenderScale = Mathf.Min(performanceProfile.renderScale, _targetRenderScale + 0.01f);
                }

                _currentRenderScale = Mathf.Lerp(_currentRenderScale, _targetRenderScale, Time.unscaledDeltaTime * 1.8f);
                // Apply to URP if available (safe)
                var urp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
                if (urp != null) urp.renderScale = _currentRenderScale;
            }
        }

        static void TryReconfigureAetherGridFromProfile(PerformanceProfile p)
        {
            try
            {
                var world = Unity.Entities.World.DefaultGameObjectInjectionWorld;
                if (world == null) return;
                var em = world.EntityManager;
                var query = em.CreateEntityQuery(typeof(AetherFieldConfig));
                if (query.CalculateEntityCount() > 0)
                {
                    var e = query.GetSingletonEntity();
                    var cfg = em.GetComponentData<AetherFieldConfig>(e);
                    cfg.GridSizeX = p.aetherGridX;
                    cfg.GridSizeY = p.aetherGridY;
                    cfg.GridSizeZ = p.aetherGridZ;
                    em.SetComponentData(e, cfg);
                    Debug.Log($"[GameBootstrap] Aether grid dynamically updated to {p.aetherGridX}x{p.aetherGridY}x{p.aetherGridZ} for tier");
                }
                query.Dispose();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameBootstrap] Aether grid partial update skipped (safe): {ex.Message} — full effect on zone reload");
            }
        }

        /// <summary>
        /// Exposes active profile for other systems (Round 5 singleton locator pattern).
        /// </summary>
        public static PerformanceProfile GetActivePerformanceProfile() => Instance?.performanceProfile;

        /// <summary>
        /// M2: Used by Pause Menu "Save & Quit to Menu".
        /// </summary>
        public static void LoadMainMenu()
        {
            Time.timeScale = 1f;
            MainMenuActive = true;
            var loader = FindAnyObjectByType<SceneLoader>();
            if (loader != null)
                loader.LoadMainMenuScene();
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("Boot");
        }
    }
}
