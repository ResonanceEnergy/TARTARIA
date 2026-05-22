using UnityEngine;
using System.Collections;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Input;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5 floating platform progression mechanics.
    /// 5 golden-ratio positioned platforms rise from White City ruins.
    /// Player must restore each platform and navigate between them.
    /// Final platform connects to central spire.
    /// </summary>
    public class FloatingPlatformProgression : MonoBehaviour
    {
        public static FloatingPlatformProgression Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] int totalPlatforms = 5;
        [SerializeField] Vector3 whiteCityCenter = new Vector3(200f, 0f, 300f);
        [SerializeField] float platformRadius = 40f;
        [SerializeField] float platformHeight = 15f;

        int _platformsActivated = 0;
        readonly System.Collections.Generic.List<FloatingPlatform> _platforms = new();

        public float Progress => _platformsActivated / (float)totalPlatforms;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void InitializePlatforms()
        {
            const float PHI = 1.618033988749f;

            for (int i = 0; i < totalPlatforms; i++)
            {
                // Golden ratio spiral positioning
                float angle = i * PHI * Mathf.PI * 2f;
                float radius = platformRadius * Mathf.Pow(PHI, i / (float)totalPlatforms);
                
                Vector3 pos = whiteCityCenter + new Vector3(
                    Mathf.Cos(angle) * radius,
                    platformHeight + (i * 5f), // Ascending height
                    Mathf.Sin(angle) * radius
                );

                GameObject platformObj = new GameObject($"FloatingPlatform_{i}");
                platformObj.transform.position = pos;

                FloatingPlatform platform = platformObj.AddComponent<FloatingPlatform>();
                platform.platformIndex = i;
                platform.OnActivated += OnPlatformActivated;

                _platforms.Add(platform);

                // Visual
                CreatePlatformVisual(platformObj, false);
            }

            Debug.Log($"[FloatingPlatforms] Spawned {totalPlatforms} platforms in golden-ratio spiral.");
        }

        void CreatePlatformVisual(GameObject platformObj, bool activated)
        {
            // Platform: circular disc
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.transform.SetParent(platformObj.transform);
            disc.transform.localPosition = Vector3.zero;
            disc.transform.localScale = new Vector3(5f, 0.3f, 5f);

            Renderer rend = disc.GetComponent<Renderer>();
            rend.material.color = activated 
                ? new Color(1f, 0.9f, 0.5f)  // Golden (active)
                : new Color(0.5f, 0.5f, 0.5f, 0.3f); // Gray translucent (inactive)

            // Active platforms have upward beam
            if (activated)
            {
                Light light = platformObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.9f, 0.5f);
                light.range = 15f;
                light.intensity = 3f;
            }
        }

        void OnPlatformActivated(FloatingPlatform platform)
        {
            _platformsActivated++;

            Debug.Log($"[FloatingPlatforms] Platform {platform.platformIndex} activated! ({_platformsActivated}/{totalPlatforms})");

            Audio.AudioManager.Instance?.PlaySFX2D("PlatformActivate");

            // Update visual
            foreach (Transform child in platform.transform)
            {
                Destroy(child.gameObject);
            }
            CreatePlatformVisual(platform.gameObject, true);

            // Check completion
            if (_platformsActivated >= totalPlatforms)
            {
                OnAllPlatformsActivated();
            }
        }

        void OnAllPlatformsActivated()
        {
            Debug.Log("[FloatingPlatforms] ALL PLATFORMS ACTIVE! Bridge to central spire complete.");
            
            HUDController.Instance?.ShowObjective("Floating platform bridge complete! Central spire accessible.");

            // Quest progress
            QuestManager.Instance?.CompleteQuest("moon5_floating_platforms");
        }
    }

    /// <summary>
    /// Individual floating platform with restoration mechanic.
    /// </summary>
    public class FloatingPlatform : MonoBehaviour, IInteractable
    {
        public int platformIndex;
        public event System.Action<FloatingPlatform> OnActivated;

        bool _isActivated = false;
        bool _isRestoring = false;

        public string GetInteractPrompt()
        {
            if (_isActivated) return "Platform Active ✓";
            if (_isRestoring) return "Restoring...";
            return "[E] Restore Platform";
        }

        public void Interact(GameObject player)
        {
            if (_isActivated || _isRestoring) return;

            StartCoroutine(RestorePlatform());
        }

        IEnumerator RestorePlatform()
        {
            _isRestoring = true;

            Debug.Log($"[FloatingPlatform {platformIndex}] Restoring platform...");
            HUDController.Instance?.ShowObjective($"Restoring platform {platformIndex + 1}...");

            // Restoration VFX: golden energy fills platform
            yield return new WaitForSeconds(2f);

            _isActivated = true;
            _isRestoring = false;

            Debug.Log($"[FloatingPlatform {platformIndex}] Platform restored!");
            OnActivated?.Invoke(this);
        }
    }

    /// <summary>
    /// Captain Thorne NPC controller for Moon 5.
    /// Airship captain who arrives via radio signal, provides aerial transport.
    /// </summary>
    public class CaptainThorneNPC : MonoBehaviour, IInteractable
    {
        public static CaptainThorneNPC Instance { get; private set; }

        [Header("Dialogue")]
        [SerializeField] bool hasIntroduced = false;
        [SerializeField] int dialogueIndex = 0;

        readonly string[] _dialogueLines = {
            "About time someone lit a signal. I've been circling for two centuries.",
            "The White City was the crown jewel. Five pavilions, all golden-ratio precise.",
            "Your restoration work... it's crude, but it's WORKING. The auras are back.",
            "Need a lift? My airship can reach any spire in the grid. For a price.",
            "The Dissonant One's agents... they're getting nervous. You're lighting up the grid too fast."
        };

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public string GetInteractPrompt() => "[E] Talk to Captain Thorne";

        public void Interact(GameObject player)
        {
            string line = _dialogueLines[dialogueIndex % _dialogueLines.Length];
            
            Debug.Log($"[Thorne] {line}");
            HUDController.Instance?.ShowDialogue("Captain Thorne", line);
            
            DialogueManager.Instance?.PlayContextDialogue($"thorne_line_{dialogueIndex}");
            Audio.AudioManager.Instance?.PlaySFX2D("Thorne_Voice");

            dialogueIndex++;

            if (!hasIntroduced && dialogueIndex == 1)
            {
                hasIntroduced = true;
                QuestManager.Instance?.CompleteQuest("moon5_thorne_introduction");
            }
        }

        /// <summary>
        /// Offer airship transport to target location.
        /// </summary>
        public void OfferTransport(Vector3 destination)
        {
            Debug.Log($"[Thorne] Airship transport to {destination} (feature pending full implementation)");
            HUDController.Instance?.ShowObjective("Thorne's airship ready for transport.");
        }
    }
}
