using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Sprint 9 Lane 8 — Named villager interaction component.
    ///
    /// Attached by <see cref="Moon1NamedVillagers"/> alongside a SphereCollider (isTrigger).
    /// Behaviour contract:
    ///   - OnTriggerEnter (player): raise <see cref="GameEvents.RaiseHUDShowInteractionPrompt"/>.
    ///   - While player inside: poll for E (Keyboard.current.eKey) and raise
    ///     <see cref="GameEvents.RaiseHUDShowDialogue"/> (speaker = villager name, message = greeting).
    ///     The Sprint 7 Lane 6 <see cref="YarnTutorialBinding"/> routes that event to a
    ///     Yarn node if a mapping exists; otherwise the HUD shows a banner.
    ///   - OnTriggerExit: raise <see cref="GameEvents.RaiseHUDHideInteractionPrompt"/>.
    ///   - 5s cooldown between dialogue triggers to prevent E-mashing spam.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    [DisallowMultipleComponent]
    public sealed class NamedVillagerInteraction : MonoBehaviour
    {
        [SerializeField] private string _villagerName = "Villager";
        [SerializeField, TextArea] private string _greetingLine = "...";
        [SerializeField] private float _interactionCooldownSeconds = 5f;

        private bool _playerInRange;
        private float _lastInteractionTime = -999f;

        /// <summary>Configures the villager identity. Called by the spawner immediately after AddComponent.</summary>
        public void Configure(string villagerName, string greetingLine)
        {
            if (!string.IsNullOrWhiteSpace(villagerName))
            {
                _villagerName = villagerName;
            }
            if (!string.IsNullOrWhiteSpace(greetingLine))
            {
                _greetingLine = greetingLine;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other))
            {
                return;
            }
            _playerInRange = true;
            GameEvents.RaiseHUDShowInteractionPrompt("Press E to talk to " + _villagerName);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsPlayer(other))
            {
                return;
            }
            _playerInRange = false;
            GameEvents.RaiseHUDHideInteractionPrompt();
        }

        private void Update()
        {
            if (!_playerInRange)
            {
                return;
            }
            if (Keyboard.current == null)
            {
                return;
            }
            if (!Keyboard.current.eKey.wasPressedThisFrame)
            {
                return;
            }
            float now = Time.time;
            if (now - _lastInteractionTime < _interactionCooldownSeconds)
            {
                return;
            }
            _lastInteractionTime = now;
            GameEvents.RaiseHUDShowDialogue(_villagerName, _greetingLine);
        }

        private static bool IsPlayer(Collider other)
        {
            if (other == null)
            {
                return false;
            }
            if (other.CompareTag("Player"))
            {
                return true;
            }
            // Fall back to checking for a CharacterController on the collider hierarchy.
            return other.GetComponentInParent<CharacterController>() != null;
        }
    }
}
