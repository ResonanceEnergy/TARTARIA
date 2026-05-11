using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Game Bootstrap — initializes the ECS world, creates singleton entities,
    /// and sets up the Aether field configuration.
    /// Attached to the Boot scene's bootstrap GameObject.
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

            // Note: InventorySystem is Lazy singleton - auto-initializes on first access
            // No need to initialize here (avoids assembly circular dependency)
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

            // Day-13: gate behind main menu "Start" / "Continue" click.
            // PlayerPrefs flag bypasses menu for dev convenience.
            bool autoStart = PlayerPrefs.GetInt("TARTARIA_SkipMainMenu", 0) == 1;
            if (autoStart || !MainMenuActive)
            {
                TriggerSceneLoad();
            }
            // else: MainMenuOverlay will call BeginGameplay() below when player clicks Start.
        }

        // Set true by MainMenuOverlay before scene-load to keep GameBootstrap waiting.
        public static bool MainMenuActive;

        public void TriggerSceneLoad()
        {
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

            // Guard: skip if AetherFieldConfig singleton already exists (re-enter Play mode)
            var configQuery = em.CreateEntityQuery(typeof(AetherFieldConfig));
            if (configQuery.CalculateEntityCount() > 0)
            {
                configQuery.Dispose();
                Debug.Log("[Tartaria] ECS world already initialized — skipping duplicate creation.");
                return true;
            }
            configQuery.Dispose();

            // Create Aether field configuration singleton
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

            // Create player tag singleton (required by DiscoverySystem, AI systems)
            var playerEntity = em.CreateEntity();
            em.AddComponentData(playerEntity, new PlayerTag());
            em.AddComponentData(playerEntity, new LocalTransform
            {
                Position = new float3(0f, 1f, -20f),
                Rotation = quaternion.identity,
                Scale = 1f
            });

            Debug.Log("[Tartaria] ECS world initialized. Aether field configured.");
            return true;
        }
    }
}
