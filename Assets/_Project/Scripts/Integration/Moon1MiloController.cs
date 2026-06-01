using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Milo onboarding trigger.
    ///
    /// Auto-bootstraps after scene load. On trigger (~0.5s after Bootstrap, the
    /// proxy for "player spawned" since <see cref="GameEvents"/> currently has
    /// no <c>OnPlayerSpawned</c> event — only the stub <c>FirePlayerSpawned</c>
    /// method that doesn't invoke a real event), Milo greets the player and
    /// points them at the first hero building (the Fountain).
    ///
    /// Mirrors the pattern in <see cref="AnastasiaController"/> (singleton +
    /// dialogue-then-HUD-fallback). Replace the timer proxy with a real event
    /// subscribe when GameEvents grows an actual <c>OnPlayerSpawned</c>
    /// Action&lt;Vector3&gt;.
    /// </summary>
    [DefaultExecutionOrder(-40)]
    [DisallowMultipleComponent]
    public class Moon1MiloController : MonoBehaviour
    {
        public static Moon1MiloController Instance { get; private set; }

        // Timer proxy — see class summary
        const float kSpawnProxyDelay = 0.5f;
        const string kDialogueContext = "milo_onboarding";
        const string kBannerTitle = "Milo";
        const string kBannerSubtitle = "Welcome to Echohaven. Three buildings await your touch.";
        const float kBannerDuration = 6f;

        bool _triggered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBootstrap()
        {
            // Echohaven-only: don't spawn in menu / boot scenes
            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(active)) return;
            if (active.IndexOf("Echohaven", System.StringComparison.OrdinalIgnoreCase) < 0) return;

            if (Instance != null) return;

            var go = new GameObject("Moon1MiloController");
            go.AddComponent<Moon1MiloController>();
            DontDestroyOnLoad(go);
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Treat Bootstrap + kSpawnProxyDelay as the "player spawned" signal
            // until GameEvents grows a real OnPlayerSpawned event subscribers
            // can hook.
            StartCoroutine(SpawnProxyCoroutine());
        }

        IEnumerator SpawnProxyCoroutine()
        {
            yield return new WaitForSecondsRealtime(kSpawnProxyDelay);
            TriggerOnboarding();
        }

        void TriggerOnboarding()
        {
            if (_triggered) return;
            _triggered = true;

            bool dialoguePlayed = false;
            try
            {
                var dm = DialogueManager.Instance;
                if (dm != null)
                {
                    dm.PlayContextDialogue(kDialogueContext);
                    dialoguePlayed = true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Moon1MiloController] DialogueManager threw, falling back to HUD banner: {ex.Message}");
            }

            if (!dialoguePlayed)
            {
                try
                {
                    GameEvents.RaiseHUDShowBanner(kBannerTitle, kBannerSubtitle, kBannerDuration);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Moon1MiloController] HUD banner fallback also failed: {ex}");
                }
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
