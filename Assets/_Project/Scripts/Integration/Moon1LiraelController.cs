using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration {
    [DisallowMultipleComponent]
    public class Moon1LiraelController : MonoBehaviour {
        public static Moon1LiraelController Instance { get; private set; }
        [SerializeField] float postWinDelaySeconds = 12f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap() {
            if (Instance != null) return;
            var go = new GameObject("Moon1LiraelController");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<Moon1LiraelController>();
        }

        void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GameEvents.OnMoonCompleted += HandleMoonCompleted;
        }

        void OnDestroy() {
            if (Instance == this) Instance = null;
            GameEvents.OnMoonCompleted -= HandleMoonCompleted;
        }

        void HandleMoonCompleted(MoonCompletedEventArgs args) {
            if (args == null || args.moonIndex != 1) return;
            StartCoroutine(WaitAndSpeak());
        }

        IEnumerator WaitAndSpeak() {
            yield return new WaitForSecondsRealtime(postWinDelaySeconds);
            // Try DialogueManager Yarn first, fall back to HUD banner
            var dmType = System.Type.GetType("Tartaria.Dialogue.DialogueManager") ?? System.Type.GetType("DialogueManager");
            bool playedYarn = false;
            if (dmType != null) {
                var prop = dmType.GetProperty("Instance");
                var dm = prop?.GetValue(null);
                var method = dmType.GetMethod("PlayYarn") ?? dmType.GetMethod("PlayContextDialogue");
                if (dm != null && method != null) {
                    try { method.Invoke(dm, new object[] { "lirael_17th_hour" }); playedYarn = true; }
                    catch (System.Exception ex) { Debug.LogWarning($"[LiraelController] Yarn invoke failed: {ex.Message}"); }
                }
            }
            if (!playedYarn) {
                GameEvents.RaiseHUDShowBanner("Lirael", "Twelve more wait. Some won't want to wake. Find me again under the next moon.", 12f);
            }
            Debug.Log($"[Moon1LiraelController] Lirael spoke (yarnPath={playedYarn}).");
        }
    }
}
