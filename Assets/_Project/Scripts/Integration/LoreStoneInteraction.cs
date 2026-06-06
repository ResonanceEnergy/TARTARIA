using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// LoreStoneInteraction — small interactable cube that grants +1 RS + fires
    /// a HUD lore banner once when the player presses E (or gamepad A) inside its
    /// trigger. Phase 2 prop content per docs/15 §7 "Carved Stone" pattern but
    /// applied to multiple smaller stones around the village.
    ///
    /// Wiring is done by Moon1BuildOutProps.SpawnLoreStones() via Init().
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class LoreStoneInteraction : MonoBehaviour
    {
        [SerializeField] private string stoneId = "lore_stone_default";
        [SerializeField] private string dialogueKey = "lore_default";
        [SerializeField] private float rsReward = 1f;
        [SerializeField] private float floatAmplitude = 0.15f;
        [SerializeField] private float floatSpeed = 1.2f;

        private bool _playerInRange;
        private bool _consumed;
        private Vector3 _basePos;

        public void Init(string id, string key)
        {
            stoneId = id;
            dialogueKey = key;
        }

        void Awake()
        {
            _basePos = transform.position;
            var col = GetComponent<BoxCollider>();
            col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            if (!_consumed)
                ServiceLocator.HUD?.ShowInteractionPrompt($"[E / A]  Read lore stone");
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            ServiceLocator.HUD?.HideContextPrompt();
        }

        void Update()
        {
            // Gentle hover animation so the stone reads as "special"
            if (!_consumed)
            {
                transform.position = _basePos + new Vector3(0f, Mathf.Sin(Time.time * floatSpeed) * floatAmplitude, 0f);
            }

            if (_playerInRange && !_consumed)
            {
                bool kbPress = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
                bool gpPress = Gamepad.current  != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
                if (kbPress || gpPress)
                {
                    Consume();
                }
            }
        }

        void Consume()
        {
            _consumed = true;
            GameLoopController.Instance?.AwardRS(rsReward, $"Lore stone: {stoneId}");
            ServiceLocator.HUD?.HideContextPrompt();
            ServiceLocator.HUD?.ShowBanner("Lore unearthed", LookupLoreText(dialogueKey), 4f);
            // Route the same key to the dialogue runner so the yarn node fires
            Moon1DialogueBindings.PlayLoreContext(dialogueKey);
            // Dim the stone visually to indicate it's been read
            var rend = GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial != null && rend.sharedMaterial.HasProperty("_EmissionColor"))
            {
                rend.sharedMaterial.SetColor("_EmissionColor", Color.black);
            }
            Debug.Log($"[LoreStone] {stoneId} consumed, +{rsReward} RS, dialogue={dialogueKey}");
        }

        static string LookupLoreText(string key)
        {
            // Placeholder lore copy — replace with localization keys once dialogue
            // tables exist. Per docs/01 lore bible thematic register: Tartarians spoke
            // of frequency and harmony; their ruins remember.
            switch (key)
            {
                case "lore_listener_hall":  return "\"Here we gathered to listen to the Earth herself sing.\"";
                case "lore_first_note":     return "\"This spire holds the first note. Strike it and the others answer.\"";
                case "lore_thread_memory":  return "\"The water remembers names the wind forgot.\"";
                case "lore_old_well":       return "\"Drop a stone. Listen. The well replies in 432 Hz.\"";
                case "lore_root_chamber":   return "\"Aether wells up where the roots go deepest.\"";
                default:                    return "Tartarian script, weathered beyond reading.";
            }
        }
    }
}
