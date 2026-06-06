using System;
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
            try { GameEvents.RaiseHUDShowInteractionPrompt("Press [E] to tune (" + assignedVariant + ")"); }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(TuningPedestalLink)}] {nameof(OnTriggerEnter)} HUD interaction prompt raise failed: {ex.GetType().Name}: {ex.Message}\n  context: buildingId={buildingId} nodeIndex={nodeIndex} variant={assignedVariant} other={other?.name}\n{ex.StackTrace}");
                // Non-fatal: player still inside trigger, can still press E; only the on-screen 'Press E to tune' prompt is missed.
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") && other.GetComponentInParent<CharacterController>() == null) return;
            _playerInside = false;
            try { GameEvents.RaiseHUDHideInteractionPrompt(); }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(TuningPedestalLink)}] {nameof(OnTriggerExit)} HUD interaction prompt hide failed: {ex.GetType().Name}: {ex.Message}\n  context: buildingId={buildingId} nodeIndex={nodeIndex} other={other?.name}\n{ex.StackTrace}");
                // Non-fatal: prompt may remain stuck on-screen until next show/hide cycle; gameplay state is correct.
            }
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
            try { GameEvents.RaiseHUDShowObjective("Tuning " + buildingId + " node " + (nodeIndex + 1) + "/3"); }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(TuningPedestalLink)}] {nameof(StartTuning)} HUD objective raise failed: {ex.GetType().Name}: {ex.Message}\n  context: buildingId={buildingId} nodeIndex={nodeIndex} variant={assignedVariant} targetFreq={config.targetFrequency}\n{ex.StackTrace}");
                // Non-fatal: tuning mini-game still launches below; only the 'Tuning <id> node N/3' objective banner is missed.
            }
            Debug.Log("[TuningPedestalLink] Starting " + assignedVariant + " on " + buildingId + " node " + nodeIndex);

            // Find the InteractableBuilding and route through its tuning path
            var buildings = FindObjectsByType<InteractableBuilding>(FindObjectsSortMode.None);
            foreach (var b in buildings)
            {
                if (b.BuildingId == buildingId)
                {
                    DispatchToBuildingVariant(b, config);
                    return;
                }
            }
            // Fallback: spawn a standalone variant component anchored on this pedestal.
            DispatchSoloVariant(config);
            Invoke(nameof(ReleaseBusy), 30f);
        }

        /// <summary>
        /// Pick the right variant component on the target building and dispatch the config.
        /// Matches InteractableBuilding.DispatchTuningByVariant routing per docs/15 §9.
        /// </summary>
        void DispatchToBuildingVariant(InteractableBuilding b, TuningPuzzleConfig config)
        {
            switch (config.variant)
            {
                case TuningVariant.WaveformTrace:
                {
                    var v = b.GetComponentInChildren<TuningVariantB_Waveform>(true);
                    if (v == null) v = b.gameObject.AddComponent<TuningVariantB_Waveform>();
                    v.StartTuning(config);
                    return;
                }
                case TuningVariant.HarmonicPattern:
                {
                    var v = b.GetComponentInChildren<TuningVariantC_Pattern>(true);
                    if (v == null) v = b.gameObject.AddComponent<TuningVariantC_Pattern>();
                    v.StartTuning(config);
                    return;
                }
                default:
                {
                    var v = b.GetComponentInChildren<TuningMiniGame>(true);
                    if (v == null) v = b.gameObject.AddComponent<TuningMiniGame>();
                    v.StartTuning(config);
                    return;
                }
            }
        }

        /// <summary>
        /// Fallback when no InteractableBuilding matched buildingId — spawn standalone
        /// variant component on this pedestal so the player still gets a playable mini-game.
        /// </summary>
        void DispatchSoloVariant(TuningPuzzleConfig config)
        {
            switch (config.variant)
            {
                case TuningVariant.WaveformTrace:
                    gameObject.AddComponent<TuningVariantB_Waveform>().StartTuning(config);
                    break;
                case TuningVariant.HarmonicPattern:
                    gameObject.AddComponent<TuningVariantC_Pattern>().StartTuning(config);
                    break;
                default:
                    gameObject.AddComponent<TuningMiniGame>().StartTuning(config);
                    break;
            }
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
