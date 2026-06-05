using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Per-moon runtime bootstrapper. Lives at the root of every Moon 2–13 stub.
    /// On Start it:
    ///   • Applies fog / ambient from the MoonDefinition (overrides scene-baked).
    ///   • Ensures a PlayerSpawner exists (uses fallback player if no prefab).
    ///   • Logs an "Aether whisper" intro line + zone banner.
    ///   • (Optionally) auto-activates the moon's quest via QuestManager.
    /// Designed to be DROPPED IN by MoonBootstrapperAttacher with zero manual wiring.
    /// </summary>
    [DisallowMultipleComponent]
    public class MoonRuntimeBootstrapper : MonoBehaviour
    {
        [Header("Definition")]
        public MoonDefinition definition;

        [Header("Behavior")]
        public bool applyAtmosphere = true;
        public bool ensurePlayerSpawner = true;
        public bool autoActivateQuest = true;

        bool _booted;

        void Start()
        {
            if (_booted) return;
            _booted = true;
            if (definition == null)
            {
                Debug.LogWarning("[MoonBootstrap] No MoonDefinition assigned on " + name);
                return;
            }

            if (applyAtmosphere) ApplyAtmosphere();
            if (ensurePlayerSpawner) EnsurePlayerSpawner();
            LogBanner();
            if (autoActivateQuest) TryActivateQuest();
        }

        void ApplyAtmosphere()
        {
            RenderSettings.fog                = true;
            RenderSettings.fogMode            = FogMode.ExponentialSquared;
            RenderSettings.fogColor           = definition.fogColor;
            RenderSettings.fogDensity         = definition.fogDensity;
            RenderSettings.ambientMode        = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor    = definition.ambientHigh;
            RenderSettings.ambientEquatorColor = Color.Lerp(definition.ambientLow, definition.ambientHigh, 0.5f);
            RenderSettings.ambientGroundColor  = definition.ambientLow;
        }

        void EnsurePlayerSpawner()
        {
            var existing = FindFirstObjectByType<PlayerSpawner>();
            if (existing != null) return;

            // Move (or create) PlayerSpawn marker at the moon spawn position.
            var marker = GameObject.Find("PlayerSpawn");
            if (marker == null)
            {
                marker = new GameObject("PlayerSpawn");
                marker.tag = "Respawn";
            }
            marker.transform.position = definition.spawnPos;

            var spawnerGO = new GameObject("PlayerSpawner");
            spawnerGO.transform.position = definition.spawnPos;
            spawnerGO.AddComponent<PlayerSpawner>();
        }

        void LogBanner()
        {
            Debug.Log($"[Moon {definition.number:D2}] {definition.zoneName} — {definition.headline}");
            if (!string.IsNullOrEmpty(definition.aetherWhisper))
                Debug.Log($"[Moon {definition.number:D2}] Aether whisper: \"{definition.aetherWhisper}\"");
            Debug.Log($"[Moon {definition.number:D2}] Mechanic: {definition.mechanic} • Companion: {definition.companion}");
        }

        void TryActivateQuest()
        {
            if (string.IsNullOrEmpty(definition.questId)) return;
            var qm = FindFirstObjectByType<QuestManager>();
            if (qm == null) return;
            qm.ActivateQuest(definition.questId);
        }
    }
}
