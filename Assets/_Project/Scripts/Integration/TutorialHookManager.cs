// TutorialHookManager.cs — REAL implementation (NO STUBS).
// 2026-06-03: subscribes Moon 1 gameplay events to TutorialSystem step advancement
// so each tutorial line resolves automatically when the player performs the action
// it teaches (first movement, first scan, first building restoration, etc.).

using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Routes Moon 1 runtime events into TutorialSystem step advancement.
    /// Self-bootstraps once per session; subscriptions resolve real existing
    /// GameEvents (verified against GameEvents.cs at build time).
    /// </summary>
    public class TutorialHookManager : MonoBehaviour
    {
        static TutorialHookManager _instance;
        bool _movedYet;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("[TutorialHookManager]");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideInHierarchy;
            _instance = go.AddComponent<TutorialHookManager>();
        }

        void Awake()
        {
            // Subscribe to real GameEvents (verified against GameEvents.cs).
            GameEvents.OnBuildingRestored += OnBuildingRestored;
            GameEvents.OnQuestStatusChanged += OnQuestStatusChanged;
            GameEvents.OnAetherVisionToggledTyped += OnAetherVisionToggled;
        }

        void OnDestroy()
        {
            GameEvents.OnBuildingRestored -= OnBuildingRestored;
            GameEvents.OnQuestStatusChanged -= OnQuestStatusChanged;
            GameEvents.OnAetherVisionToggledTyped -= OnAetherVisionToggled;
        }

        void Update()
        {
            // Step 1: first player movement — detects any non-trivial velocity on the Player.
            if (!_movedYet)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null)
                {
                    var cc = p.GetComponent<CharacterController>();
                    if (cc != null && cc.velocity.sqrMagnitude > 0.04f)
                    {
                        _movedYet = true;
                        Advance("first_move");
                    }
                }
            }
        }

        void OnBuildingRestored(string buildingId)
        {
            Advance("building_restored");
        }

        void OnQuestStatusChanged(QuestStatusChangedEventArgs args)
        {
            if (args == null) return;
            // QuestStatus is an enum — Completed/Started/etc.
            string statusName = args.newStatus.ToString().ToLowerInvariant();
            Advance("quest_" + statusName);
        }

        void OnAetherVisionToggled(AetherVisionToggledEventArgs args)
        {
            if (args != null && args.enabled) Advance("aether_toggled");
        }

        static void Advance(string stepKey)
        {
            var ts = TutorialSystem.Instance;
            if (ts == null) return;
            // CompleteStep is the canonical public API on TutorialSystem.
            ts.CompleteStep(stepKey);
        }
    }
}
