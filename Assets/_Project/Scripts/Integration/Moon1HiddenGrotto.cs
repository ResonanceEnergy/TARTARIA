using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Hidden Grotto trigger / reveal.
    ///
    /// Behaviour:
    ///   1. Bootstrapped as a singleton GameObject after scene load.
    ///   2. Subscribes to GameEvents.OnBuildingRestored (Action&lt;string&gt;).
    ///   3. When the Crystal Spire restores, flips the "entrance revealed" flag and logs the position.
    ///   4. Once revealed, polls every FixedUpdate for a Player-tagged actor within 2 m of grottoEntrancePosition.
    ///   5. On first entry, fires GameEvents.RaiseHUDShowBanner(title, subtitle, duration).
    ///
    /// Owned by Level Design (see docs/design/EchohavenGrotto_design.md).
    /// Does NOT spawn cavern geometry — that's a later Moon1Grotto builder pass.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1HiddenGrotto : MonoBehaviour
    {
        // ---- canonical building id for the Crystal Spire ---------------
        // BuildingRestorationCeremony / CathedralRestorationSystem fire OnBuildingRestored
        // with the registered building id. Moon 1's spire is canonically "CrystalSpire".
        // If the Moon 1 building registry changes the id, update this constant.
        private const string SpireBuildingId = "CrystalSpire";

        // ---- inspector-tweakable trigger geometry ----------------------
        [Header("Grotto trigger geometry")]
        [Tooltip("World position the player must approach to open the grotto. Defaults to a position behind the Crystal Spire in Echohaven.")]
        [SerializeField] private Vector3 grottoEntrancePosition = new Vector3(-18.0f, 0.5f, 24.0f);

        [Tooltip("Player must be within this radius (metres) of grottoEntrancePosition to trigger OpenGrotto().")]
        [SerializeField] private float triggerRadius = 2.0f;

        [Header("Banner copy")]
        [SerializeField] private string bannerTitle = "Hidden Grotto Revealed";
        [SerializeField] private string bannerSubtitle = "Step inside to discover the heart of resonance.";
        [SerializeField] private float bannerDuration = 5.0f;

        // ---- runtime state ---------------------------------------------
        private static Moon1HiddenGrotto _instance;
        private bool _spireRestored;
        private bool _grottoOpened;
        private float _lastGateLogTime;       // throttle gate logging to once per ~3 s
        private const float GateLogIntervalSec = 3.0f;

        // ---- bootstrap --------------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null)
            {
                Debug.Log($"[Moon1HiddenGrotto] Bootstrap skipped — instance already exists on '{_instance.gameObject.name}'.");
                return;
            }

            // Prefer an authored instance in the scene if one exists (so Moon 1 level dressing can
            // override grottoEntrancePosition without code edits). Unity 6: FindFirstObjectByType.
            var existing = FindFirstObjectByType<Moon1HiddenGrotto>(FindObjectsInactive.Include);
            if (existing != null)
            {
                _instance = existing;
                Debug.Log($"[Moon1HiddenGrotto] Bootstrap adopted scene-authored instance on '{existing.gameObject.name}' at {existing.transform.position}.");
            }
            else
            {
                var go = new GameObject("Moon1HiddenGrotto");
                _instance = go.AddComponent<Moon1HiddenGrotto>();
                DontDestroyOnLoad(go);
                Debug.Log($"[Moon1HiddenGrotto] Bootstrap created runtime singleton (no scene instance found). Default entrance={_instance.grottoEntrancePosition}.");
            }

            _instance.ValidateConfiguration();
        }

        private void ValidateConfiguration()
        {
            if (grottoEntrancePosition == Vector3.zero)
            {
                Debug.LogError($"[Moon1HiddenGrotto] grottoEntrancePosition is Vector3.zero on '{gameObject.name}'. " +
                               $"This is almost certainly a missing SerializeField override. Trigger will not arm at the world origin. " +
                               $"Set a real position in the inspector or update the default in Moon1HiddenGrotto.cs.");
            }

            if (triggerRadius <= 0f)
            {
                Debug.LogError($"[Moon1HiddenGrotto] triggerRadius={triggerRadius} on '{gameObject.name}' is non-positive. Trigger cannot fire. Resetting to 2.0f.");
                triggerRadius = 2.0f;
            }
        }

        // ---- event subscription ----------------------------------------
        private void OnEnable()
        {
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
            Debug.Log($"[Moon1HiddenGrotto] Subscribed to GameEvents.OnBuildingRestored. Watching for buildingId='{SpireBuildingId}'.");
        }

        private void OnDisable()
        {
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
        }

        private void HandleBuildingRestored(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                Debug.LogWarning($"[Moon1HiddenGrotto] OnBuildingRestored fired with null/empty buildingId — ignoring. Caller should pass a real id.");
                return;
            }

            if (!string.Equals(buildingId, SpireBuildingId, System.StringComparison.Ordinal))
            {
                // Not the spire — quiet path, but log at info level so we can audit the building id stream.
                Debug.Log($"[Moon1HiddenGrotto] OnBuildingRestored buildingId='{buildingId}' is not the Crystal Spire ('{SpireBuildingId}') — gate not opened.");
                return;
            }

            if (_spireRestored)
            {
                Debug.Log($"[Moon1HiddenGrotto] Spire restoration fired twice for '{buildingId}' — already revealed, ignoring duplicate.");
                return;
            }

            _spireRestored = true;
            Debug.Log($"[Moon1HiddenGrotto] Spire restored — grotto entrance now revealed at {grottoEntrancePosition}");
        }

        // ---- proximity check -------------------------------------------
        private void FixedUpdate()
        {
            if (_grottoOpened) return;

            if (!_spireRestored)
            {
                // Spire-not-yet-restored gate. Log once per interval with the current player position
                // (if we can find one) so we can audit why a player who thinks they're "at the grotto"
                // is not getting the banner.
                MaybeLogGate("spire-not-yet-restored", null);
                return;
            }

            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo == null)
            {
                MaybeLogGate("player-tagged-actor-not-found", null);
                return;
            }

            var playerPos = playerGo.transform.position;
            var distance = Vector3.Distance(playerPos, grottoEntrancePosition);
            if (distance > triggerRadius)
            {
                MaybeLogGate("player-not-in-range", $"playerPos={playerPos} entrance={grottoEntrancePosition} distance={distance:F2}m radius={triggerRadius:F2}m");
                return;
            }

            // All gates passed.
            OpenGrotto();
        }

        private void MaybeLogGate(string gateName, string detail)
        {
            float now = Time.unscaledTime;
            if (now - _lastGateLogTime < GateLogIntervalSec) return;
            _lastGateLogTime = now;

            if (string.IsNullOrEmpty(detail))
            {
                Debug.Log($"[Moon1HiddenGrotto] Gate '{gateName}' blocking trigger. spireRestored={_spireRestored} grottoOpened={_grottoOpened} entrance={grottoEntrancePosition}");
            }
            else
            {
                Debug.Log($"[Moon1HiddenGrotto] Gate '{gateName}' blocking trigger. {detail}");
            }
        }

        // ---- the payoff ------------------------------------------------
        public void OpenGrotto()
        {
            if (_grottoOpened)
            {
                Debug.Log($"[Moon1HiddenGrotto] OpenGrotto() called but _grottoOpened=true — ignoring re-entry.");
                return;
            }

            _grottoOpened = true;

            Debug.Log($"[Moon1HiddenGrotto] OpenGrotto() — firing HUD banner. title='{bannerTitle}' subtitle='{bannerSubtitle}' duration={bannerDuration}");

            // Signature verified against GameEvents.cs:623 — RaiseHUDShowBanner(string title, string subtitle, float duration = 5f).
            GameEvents.RaiseHUDShowBanner(bannerTitle, bannerSubtitle, bannerDuration);

            // Reward bundle (Skeleton Key #2 + Lorebook entry + +5% Telluric regen) is granted by
            // sibling systems that listen for the banner or for a follow-up event. This script's
            // contract is the reveal + banner only; design doc §7 spells out the reward chain.
        }

        // ---- editor visualisation --------------------------------------
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _spireRestored ? new Color(0.2f, 0.85f, 1.0f, 0.6f) : new Color(0.7f, 0.7f, 0.7f, 0.3f);
            Gizmos.DrawWireSphere(grottoEntrancePosition, triggerRadius);
        }
#endif
    }
}
