using System.Collections.Generic;
using Tartaria.Core;
using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Memory Echo Vision system - displays temporal visions of past events.
    /// Used in Moon 11 to show the aquifer's history and Golden Age water rituals.
    /// Triggers ambient dialogue and visual ghosting of past NPCs/structures.
    /// </summary>
    public class MemoryEchoSystem : MonoBehaviour
    {
        [Header("Echo Configuration")]
        [SerializeField] float visionDuration = 15f;
        [SerializeField] float echoTriggerRadius = 10f;
        [SerializeField] Color echoTint = new(0.6f, 0.8f, 1f, 0.5f); // Ethereal blue

        [Header("Vision Locations")]
        [SerializeField] Vector3[] echoPointLocations; // Where echoes can be triggered
        [SerializeField] string[] echoDialogueIds;     // Corresponding dialogue for each echo

        readonly List<GameObject> _activeEchoes = new();
        readonly HashSet<int> _triggeredEchoes = new();
        bool _systemActive;

        public bool AllEchoesViewed => _triggeredEchoes.Count >= (echoPointLocations?.Length ?? 0);
        public int EchoesViewed => _triggeredEchoes.Count;
        public int TotalEchoes => echoPointLocations?.Length ?? 0;

        void Start()
        {
            if (echoPointLocations != null && echoPointLocations.Length > 0)
            {
                SpawnEchoTriggers();
            }
        }

        /// <summary>
        /// Activate the memory echo system
        /// </summary>
        public void ActivateSystem()
        {
            if (_systemActive) return;

            _systemActive = true;
            Debug.Log("[MemoryEcho] System activated — temporal visions now accessible");

            // Spawn trigger zones at each echo point
            SpawnEchoTriggers();

            // Ambient audio: whispers of the past
            Audio.AudioManager.Instance?.PlayLoopingSFX("EchoWhispers", transform.position, 0.3f);
        }

        void SpawnEchoTriggers()
        {
            if (echoPointLocations == null) return;

            for (int i = 0; i < echoPointLocations.Length; i++)
            {
                var trigger = new GameObject($"MemoryEchoTrigger_{i}");
                trigger.transform.position = echoPointLocations[i];
                trigger.transform.SetParent(transform);

                // Visual marker (ghostly sphere)
                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "EchoMarker";
                marker.transform.SetParent(trigger.transform);
                marker.transform.localScale = Vector3.one * 2f;
                marker.transform.localPosition = Vector3.zero;

                var renderer = marker.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = echoTint;
                }

                // Trigger zone
                var triggerZone = marker.AddComponent<SphereCollider>();
                triggerZone.isTrigger = true;
                triggerZone.radius = echoTriggerRadius / 2f;

                // Interactable component
                var interactable = marker.AddComponent<MemoryEchoInteractable>();
                interactable.system = this;
                interactable.echoIndex = i;

                _activeEchoes.Add(trigger);
            }

            Debug.Log($"[MemoryEcho] Spawned {echoPointLocations.Length} echo trigger points");
        }

        /// <summary>
        /// Trigger a memory echo vision at the specified index
        /// </summary>
        public void TriggerEcho(int echoIndex)
        {
            if (echoIndex < 0 || echoIndex >= (echoPointLocations?.Length ?? 0))
            {
                Debug.LogWarning($"[MemoryEcho] Invalid echo index {echoIndex}");
                return;
            }

            if (_triggeredEchoes.Contains(echoIndex))
            {
                Debug.Log($"[MemoryEcho] Echo {echoIndex} already viewed");
                return;
            }

            _triggeredEchoes.Add(echoIndex);

            Debug.Log($"[MemoryEcho] Triggering memory echo {echoIndex + 1}/{TotalEchoes}");

            // Start vision sequence
            StartCoroutine(PlayEchoVision(echoIndex));

            // Quest progress
            QuestManager.Instance?.ProgressObjective("moon11_memory_echoes", 0, 1);

            // Check if all echoes viewed
            if (AllEchoesViewed)
            {
                OnAllEchoesViewed();
            }
        }

        System.Collections.IEnumerator PlayEchoVision(int echoIndex)
        {
            Vector3 position = echoPointLocations[echoIndex];

            Debug.Log($"[MemoryEcho] Playing vision {echoIndex} — {visionDuration}s temporal playback");

            // Visual: ghostly figures/structures appear
            var visionObj = new GameObject($"EchoVision_{echoIndex}");
            visionObj.transform.position = position;

            // Create ghostly NPCs or structures (simple visual representation)
            for (int i = 0; i < 3; i++)
            {
                var ghost = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                ghost.name = $"Ghost_{i}";
                ghost.transform.SetParent(visionObj.transform);
                ghost.transform.localPosition = new Vector3(i * 3f - 3f, 0, 0);
                ghost.transform.localScale = new Vector3(1f, 2f, 1f);

                var renderer = ghost.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = echoTint;
                }

                // Animate: fade in
                StartCoroutine(FadeGhost(renderer, 0f, 0.5f, 1f));
            }

            // VFX: temporal distortion
            var particles = visionObj.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = echoTint;
            main.startSize = 5f;
            main.startLifetime = visionDuration;
            main.maxParticles = 1000;

            // Play dialogue if available
            if (echoDialogueIds != null && echoIndex < echoDialogueIds.Length)
            {
                DialogueManager.Instance?.PlayContextDialogue(echoDialogueIds[echoIndex]);
            }

            // Play echo SFX
            Audio.AudioManager.Instance?.PlaySFX3D("MemoryEchoVision", position, 0.7f);

            // Wait for vision duration
            yield return new UnityEngine.WaitForSeconds(visionDuration);

            // Fade out and destroy
            foreach (Transform child in visionObj.transform)
            {
                var renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    StartCoroutine(FadeGhost(renderer, 0.5f, 0f, 1f));
                }
            }

            yield return new UnityEngine.WaitForSeconds(1f);
            Destroy(visionObj);

            Debug.Log($"[MemoryEcho] Vision {echoIndex} complete");
        }

        System.Collections.IEnumerator FadeGhost(Renderer renderer, float from, float to, float duration)
        {
            if (renderer == null) yield break;

            float elapsed = 0f;
            Color color = renderer.material.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, elapsed / duration);
                color.a = alpha;
                renderer.material.color = color;
                yield return null;
            }

            color.a = to;
            renderer.material.color = color;
        }

        void OnAllEchoesViewed()
        {
            Debug.Log("[MemoryEcho] All memory echoes viewed — ancient history revealed");

            // Dialogue: synthesis
            DialogueManager.Instance?.PlayContextDialogue("lirael_echoes_complete");

            // Quest complete
            QuestManager.Instance?.CompleteQuest("moon11_memory_echoes_complete");

            // Achievement
            AchievementSystem.Instance?.Unlock("memory_archivist");

            Debug.Log("[MemoryEcho] The aquifer remembers — water is the oldest keeper of memory");
        }

        /// <summary>
        /// Interactable component for memory echo trigger points
        /// </summary>
        public class MemoryEchoInteractable : MonoBehaviour, Input.IInteractable
        {
            public MemoryEchoSystem system;
            public int echoIndex;

            public string GetInteractPrompt()
            {
                if (system == null) return "";
                if (system._triggeredEchoes.Contains(echoIndex))
                    return "Echo already viewed";
                return $"Hold [E] to Experience Memory Echo ({echoIndex + 1}/{system.TotalEchoes})";
            }

            public void Interact(GameObject interactor)
            {
                if (system == null) return;
                system.TriggerEcho(echoIndex);
            }
        }
    }
}
