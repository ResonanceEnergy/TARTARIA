using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// TuningPedestalLink — runtime component placed on TuningPedestal_N GameObjects
    /// by the Moon1WireTuningPedestals Editor menu. When the player walks into the
    /// pedestal's trigger and presses E, dispatches the assigned tuning variant
    /// targeting the linked hero building.
    /// </summary>
    [DisallowMultipleComponent]
    public class TuningPedestalLink : MonoBehaviour
    {
        [Header("Pedestal Wiring (set by Moon1WireTuningPedestals)")]
        public string buildingId;
        public int nodeIndex;
        public TuningVariant assignedVariant;

        bool _playerInside;
        bool _busy;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && other.GetComponentInParent<CharacterController>() == null) return;
            _playerInside = true;
            try { GameEvents.RaiseHUDShowInteractionPrompt("Press [E] to tune (" + assignedVariant + ")"); } catch { }
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") && other.GetComponentInParent<CharacterController>() == null) return;
            _playerInside = false;
            try { GameEvents.RaiseHUDHideInteractionPrompt(); } catch { }
        }

        void Update()
        {
            if (!_playerInside || _busy) return;
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            var pad = UnityEngine.InputSystem.Gamepad.current;
            bool pressed = (kb != null && kb.eKey.wasPressedThisFrame) ||
                           (pad != null && pad.buttonSouth.wasPressedThisFrame);
            if (!pressed) return;
#else
            if (!UnityEngine.Input.GetKeyDown(KeyCode.E)) return;
#endif
            StartTuning();
        }

        void StartTuning()
        {
            _busy = true;
            var config = new TuningPuzzleConfig
            {
                variant = assignedVariant,
                targetFrequency = NextTargetFrequency(),
                timeLimitSeconds = assignedVariant == TuningVariant.FrequencySlider ? 15f
                                  : assignedVariant == TuningVariant.WaveformTrace ? 20f : 10f,
                tolerancePercent = assignedVariant == TuningVariant.FrequencySlider ? 0.08f
                                  : assignedVariant == TuningVariant.WaveformTrace ? 0.05f : 0.03f,
                difficultySpeed = 0.3f + nodeIndex * 0.15f
            };
            try { GameEvents.RaiseHUDShowObjective("Tuning " + buildingId + " node " + (nodeIndex + 1) + "/3"); } catch { }
            Debug.Log("[TuningPedestalLink] Starting " + assignedVariant + " on " + buildingId + " node " + nodeIndex);

            // Find the InteractableBuilding and route through its tuning path
            var buildings = FindObjectsByType<InteractableBuilding>(FindObjectsSortMode.None);
            foreach (var b in buildings)
            {
                if (b.BuildingId == buildingId)
                {
                    var mini = b.GetComponentInChildren<TuningMiniGame>(true);
                    if (mini == null) mini = b.gameObject.AddComponent<TuningMiniGame>();
                    mini.StartTuning(config);
                    return;
                }
            }
            // Fallback: spawn a standalone TuningMiniGame anchored on this pedestal
            var solo = gameObject.AddComponent<TuningMiniGame>();
            solo.StartTuning(config);
            Invoke(nameof(ReleaseBusy), 30f);
        }

        void ReleaseBusy() { _busy = false; }

        float NextTargetFrequency()
        {
            // Schumann-base + harmonic stack per docs/02 Aether bands
            switch (nodeIndex)
            {
                case 0: return 7.83f * 55f;   // ~430 Hz Telluric
                case 1: return 432f;          // Harmonic
                case 2: return 528f;          // Celestial
                default: return 432f;
            }
        }
    }
}
