using System.Collections;
using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon1AnastasiaController — fires the Anastasia reveal beat ~8.7s after
    /// <see cref="Tartaria.Core.GameEvents.OnMoonCompleted"/> for moonIndex 1
    /// (~2 s after the Moon1WinScreen card finishes its 1.2 + 6 + 1.5 sequence).
    ///
    /// Per HANDOFFS 2026-06-01 22:30 → Narrative Designer (anastasia-reveal-yarn).
    ///
    /// Trigger path:
    ///   - Primary: <c>DialogueManager.Instance.PlayContextDialogue("anastasia_reveal")</c>
    ///     — DialogueManager picks up <c>Dialogue/Echohaven/anastasia_reveal.yarn</c>
    ///     via its existing context resolver.
    ///   - Fallback (DialogueManager null or context not registered):
    ///     <c>GameEvents.RaiseHUDShowBanner("Anastasia", "<teaser>", 6f)</c>.
    ///
    /// Auto-bootstraps via <see cref="RuntimeInitializeOnLoadMethodAttribute"/>; no scene wiring.
    /// </summary>
    [DefaultExecutionOrder(-40)]
    public class Moon1AnastasiaController : MonoBehaviour
    {
        const float PostWinScreenDelaySeconds = 8.7f;
        const string DialogueContext = "anastasia_reveal";
        const string FallbackTitle = "Anastasia";
        const string FallbackSubtitle = "I was hoping you would come, traveler. My name is Anastasia. Lirael is my daughter.";
        const float FallbackDuration = 6f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Object.FindFirstObjectByType<Moon1AnastasiaController>(FindObjectsInactive.Include) != null) return;
            var go = new GameObject(nameof(Moon1AnastasiaController));
            DontDestroyOnLoad(go);
            go.AddComponent<Moon1AnastasiaController>();
        }

        void Awake()
        {
            Tartaria.Core.GameEvents.OnMoonCompleted += HandleMoonCompleted;
            Debug.Log("[Moon1AnastasiaController] Bootstrapped + subscribed to OnMoonCompleted.");
        }

        void OnDestroy()
        {
            Tartaria.Core.GameEvents.OnMoonCompleted -= HandleMoonCompleted;
        }

        void HandleMoonCompleted(Tartaria.Core.MoonCompletedEventArgs args)
        {
            if (args == null || args.moonIndex != 1) return;
            StartCoroutine(DelayedReveal());
        }

        IEnumerator DelayedReveal()
        {
            // Realtime — the win screen runs on Time.unscaledDeltaTime so we
            // align with wall-clock, not Time.timeScale.
            yield return new WaitForSecondsRealtime(PostWinScreenDelaySeconds);

            bool firedDialogue = false;
            var dm = DialogueManager.Instance;
            if (dm != null)
            {
                try
                {
                    dm.PlayContextDialogue(DialogueContext);
                    firedDialogue = true;
                    Debug.Log($"[Moon1AnastasiaController] Played dialogue context '{DialogueContext}'.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[Moon1AnastasiaController] DialogueManager.PlayContextDialogue threw: {ex.Message} — falling back to HUD banner.");
                }
            }

            if (!firedDialogue)
            {
                Tartaria.Core.GameEvents.RaiseHUDShowBanner(FallbackTitle, FallbackSubtitle, FallbackDuration);
                Debug.Log("[Moon1AnastasiaController] Fired fallback HUD banner.");
            }
        }
    }
}
