using System;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon1LiraelDay25Gate — subscribes to <see cref="GameEvents.OnDayChanged"/> and reveals Lirael
    /// on Moon 1, Day 25 per docs/03 Moon 1 NPC arrival schedule and the gate-note left by
    /// <c>Moon1BuildOutNPCs.PlaceOrUpdate("Lirael_AtFountain", ..., activeAtStart: false,
    /// gateNote: "Day >= 25 (TODO: hook GameEvents.OnDayChanged when it exists)")</c>.
    ///
    /// CONTRACT (Sprint 9 Lane 3):
    ///   - Trigger:   <c>GameEvents.OnDayChanged</c> fires with <c>dayIndex == 25</c>.
    ///   - Primary:   find <c>Lirael_AtFountain</c> (or any GameObject whose name starts with "Lirael")
    ///                via <c>FindFirstObjectByType&lt;Transform&gt;</c> walk (covers inactive objects).
    ///                If found and inactive, SetActive(true) + log + raise the HUD banner.
    ///   - Fallback:  if no Lirael GameObject is present, log a single warning naming the expected
    ///                identifier and raise the HUD banner so the player still gets the narrative beat.
    ///   - Idempotent: re-firing day 25 from the calendar (e.g., after a load) is a no-op once
    ///                 Lirael is already active.
    ///   - Cleanup:    unsubscribe on OnDestroy. No silent catches.
    ///
    /// Bootstrap via <see cref="RuntimeInitializeOnLoadMethod"/> AfterSceneLoad so the gate is
    /// guaranteed to exist regardless of scene authoring order.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1LiraelDay25Gate : MonoBehaviour
    {
        const int RevealDay = 25;
        const string PrimaryLiraelName = "Lirael_AtFountain";
        const string FallbackNameStart = "Lirael";

        static Moon1LiraelDay25Gate _instance;

        bool _hasFired;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;

            // Only spawn on the Echohaven / Moon 1 slice. Cheap heuristic: any TartarianCalendar in scene.
            var calendar = UnityEngine.Object.FindFirstObjectByType<TartarianCalendar>(FindObjectsInactive.Include);
            if (calendar == null)
            {
                // Not Moon 1 — silent skip. The gate is moon-1-specific narrative.
                return;
            }

            var go = new GameObject("Moon1LiraelDay25Gate");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1LiraelDay25Gate>();
            Debug.Log("[Moon1LiraelDay25Gate] Bootstrapped after scene load; awaiting OnDayChanged(25).");
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            GameEvents.OnDayChanged += HandleDayChanged;
            Debug.Log("[Moon1LiraelDay25Gate] Subscribed to GameEvents.OnDayChanged.");
        }

        void OnDestroy()
        {
            GameEvents.OnDayChanged -= HandleDayChanged;
            if (_instance == this) _instance = null;
        }

        void HandleDayChanged(int dayIndex)
        {
            try
            {
                if (_hasFired)
                {
                    return;
                }
                if (dayIndex < RevealDay)
                {
                    // Pre-gate days are quiet by design — verbose log would flood the console.
                    return;
                }

                _hasFired = true;
                Debug.Log($"[Moon1LiraelDay25Gate] OnDayChanged({dayIndex}) >= {RevealDay} — revealing Lirael.");

                var lirael = ResolveLiraelGameObject();
                if (lirael != null)
                {
                    if (!lirael.activeSelf)
                    {
                        lirael.SetActive(true);
                        Debug.Log($"[Moon1LiraelDay25Gate] Activated GameObject '{lirael.name}' at {lirael.transform.position}.");
                    }
                    else
                    {
                        Debug.Log($"[Moon1LiraelDay25Gate] GameObject '{lirael.name}' already active — no-op.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[Moon1LiraelDay25Gate] No GameObject named '{PrimaryLiraelName}' (or starting with '{FallbackNameStart}') found in scene. " +
                                     "Expected scene authoring: Echohaven_NPCs/Lirael_AtFountain (placed inactive by Tartaria/1 Build/Build Out Moon 1 NPCs). " +
                                     "Falling back to banner only.");
                }

                GameEvents.RaiseHUDShowBanner("Lirael appears", "An echo guardian wakes — find her at the Grotto.", 5f);
            }
            catch (Exception ex)
            {
                // No silent catches (per API_CONTRACT §7 NO-DEBT MANDATE).
                Debug.LogError($"[Moon1LiraelDay25Gate] HandleDayChanged threw: {ex.GetType().Name}: {ex.Message} (dayIndex={dayIndex})\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Resolve the Lirael GameObject. Prefers exact name "Lirael_AtFountain"; falls back to first
        /// object whose name starts with "Lirael" (case-insensitive). Walks inactive transforms because
        /// Lirael starts disabled per Moon1BuildOutNPCs spec.
        /// </summary>
        static GameObject ResolveLiraelGameObject()
        {
            try
            {
                var transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                Transform primary = null;
                Transform fallback = null;
                for (int i = 0; i < transforms.Length; i++)
                {
                    var t = transforms[i];
                    if (t == null) continue;
                    var name = t.name;
                    if (string.Equals(name, PrimaryLiraelName, StringComparison.OrdinalIgnoreCase))
                    {
                        primary = t;
                        break;
                    }
                    if (fallback == null && name != null && name.StartsWith(FallbackNameStart, StringComparison.OrdinalIgnoreCase))
                    {
                        fallback = t;
                    }
                }
                var hit = primary != null ? primary : fallback;
                return hit != null ? hit.gameObject : null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Moon1LiraelDay25Gate] ResolveLiraelGameObject threw: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
    }
}
