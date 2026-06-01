using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon1AudioOrchestra — listens for hero-building restoration events and
    /// plays a per-building stinger plus a capstone layer when the moon completes.
    ///
    /// Auto-bootstraps via <see cref="RuntimeInitializeOnLoadMethodAttribute"/>;
    /// no scene wiring required. Looks up clips by name under Resources/Audio/Stingers/
    /// to stay decoupled from the Audio asmdef while keeping this file inside
    /// Tartaria.Integration (where the other Moon1* listeners live).
    ///
    /// Per HANDOFFS 2026-06-01 22:30 → Audio Engineer (restoration-stinger-chain).
    /// Wires to <see cref="Tartaria.Core.GameEvents.OnBuildingRestored"/> (stingers)
    /// and <see cref="Tartaria.Core.GameEvents.OnMoonCompleted"/> (capstone).
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class Moon1AudioOrchestra : MonoBehaviour
    {
        const string ResourcePathPrefix = "Audio/Stingers/";
        const string CapstoneClipName = "EchohavenAwakened";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            // Singleton — survives scene reload, idempotent.
            if (FindObjectOfType<Moon1AudioOrchestra>() != null) return;
            var go = new GameObject(nameof(Moon1AudioOrchestra));
            DontDestroyOnLoad(go);
            go.AddComponent<Moon1AudioOrchestra>();
        }

        AudioSource _source;

        void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f; // 2D — restoration stinger is non-positional.
            _source.volume = 0.85f;

            Tartaria.Core.GameEvents.OnBuildingRestored += HandleBuildingRestored;
            Tartaria.Core.GameEvents.OnMoonCompleted += HandleMoonCompleted;
            Debug.Log("[Moon1AudioOrchestra] Bootstrapped + subscribed to OnBuildingRestored / OnMoonCompleted.");
        }

        void OnDestroy()
        {
            Tartaria.Core.GameEvents.OnBuildingRestored -= HandleBuildingRestored;
            Tartaria.Core.GameEvents.OnMoonCompleted -= HandleMoonCompleted;
        }

        void HandleBuildingRestored(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                Debug.LogWarning("[Moon1AudioOrchestra] OnBuildingRestored fired with empty buildingId — skipping stinger.");
                return;
            }

            // Map buildingId substrings → clip suffix. Forgiving match so
            // "Echohaven_StarDome_Hero" still resolves to StingerDome.
            string clipName = ResolveStingerForBuilding(buildingId);
            var clip = Resources.Load<AudioClip>(ResourcePathPrefix + clipName);
            if (clip == null)
            {
                Debug.LogWarning($"[Moon1AudioOrchestra] No clip at Resources/{ResourcePathPrefix}{clipName} for buildingId '{buildingId}'. " +
                                 "Generate placeholders via Tartaria/3 Tier/Tier 3 Procedural Audio.");
                return;
            }
            _source.PlayOneShot(clip);
            Debug.Log($"[Moon1AudioOrchestra] Stinger fired: {clipName} for '{buildingId}'.");
        }

        static string ResolveStingerForBuilding(string buildingId)
        {
            string id = buildingId.ToLowerInvariant();
            if (id.Contains("fountain")) return "StingerFountain";
            if (id.Contains("dome")) return "StingerDome";
            if (id.Contains("spire")) return "StingerSpire";
            return "StingerGeneric";
        }

        void HandleMoonCompleted(Tartaria.Core.MoonCompletedEventArgs args)
        {
            if (args == null || args.moonIndex != 1) return;
            var capstone = Resources.Load<AudioClip>(ResourcePathPrefix + CapstoneClipName);
            if (capstone == null)
            {
                Debug.LogWarning($"[Moon1AudioOrchestra] Capstone clip missing at Resources/{ResourcePathPrefix}{CapstoneClipName}. " +
                                 "Skipping layer.");
                return;
            }
            _source.PlayOneShot(capstone, 1.0f);
            Debug.Log($"[Moon1AudioOrchestra] Capstone layered: {CapstoneClipName} (Moon {args.moonIndex} '{args.moonName}').");
        }
    }
}
