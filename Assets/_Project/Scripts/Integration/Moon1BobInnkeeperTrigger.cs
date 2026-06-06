using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Integration
{
    // Moon1BobInnkeeperTrigger -- proximity dialogue for Bob the Innkeeper.
    // Bootstraps after scene load, finds the placed Bob_AtInn / BobInnkeeper
    // GameObject (Moon1NewAssetsPlacer.cs:46, Moon1BuildOutNPCs.cs:74,
    // Moon1WireSpawnerPrefabs.cs:47), and fires a dialogue beat when the
    // player approaches and presses Interact. Routes through
    // GameEvents.RaiseHUDShowDialogue("Bob", ...) which YarnTutorialBinding
    // resolves to bob_first_meet via the speaker fallback map.
    [DefaultExecutionOrder(-30)]
    public sealed class Moon1BobInnkeeperTrigger : MonoBehaviour
    {
        private const float InteractRadius = 3.0f;
        private const string Speaker = "Bob";
        private const string GreetingLine = "Heyup. Mind the lintel.";

        private static Moon1BobInnkeeperTrigger _instance;
        private static readonly string[] BobObjectNames =
        {
            "Bob_AtInn",
            "BobInnkeeper",
        };

        private Transform _bobTransform;
        private Transform _playerTransform;
        private bool _greeted;
        private bool _playerInRange;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject(nameof(Moon1BobInnkeeperTrigger));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1BobInnkeeperTrigger>();
        }

        private void Awake() { ResolveBob(); }

        private void Update()
        {
            if (_bobTransform == null)
            {
                ResolveBob();
                if (_bobTransform == null) return;
            }
            if (_playerTransform == null)
            {
                var playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO == null) return;
                _playerTransform = playerGO.transform;
            }

            float sqr = (_playerTransform.position - _bobTransform.position).sqrMagnitude;
            bool inRange = sqr <= InteractRadius * InteractRadius;

            if (inRange && !_playerInRange)
            {
                _playerInRange = true;
                ServiceLocator.HUD?.ShowInteractionPrompt("[E / A]   Speak with Bob the Innkeeper");
            }
            else if (!inRange && _playerInRange)
            {
                _playerInRange = false;
                ServiceLocator.HUD?.HideContextPrompt();
            }

            if (_playerInRange && InteractPressed())
            {
                FireDialogue();
            }
        }

        private static bool InteractPressed()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame) return true;
            var gp = Gamepad.current;
            if (gp != null && gp.buttonSouth.wasPressedThisFrame) return true;
            return false;
        }

        private void FireDialogue()
        {
            try
            {
                GameEvents.RaiseHUDShowDialogue(Speaker, _greeted ? string.Empty : GreetingLine);
                _greeted = true;
                ServiceLocator.HUD?.HideContextPrompt();
                Debug.Log("[Moon1BobInnkeeperTrigger] Fired dialogue for Bob.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[Moon1BobInnkeeperTrigger] RaiseHUDShowDialogue threw " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private void ResolveBob()
        {
            for (int i = 0; i < BobObjectNames.Length; i++)
            {
                var go = GameObject.Find(BobObjectNames[i]);
                if (go != null) { _bobTransform = go.transform; return; }
            }
        }
    }
}
