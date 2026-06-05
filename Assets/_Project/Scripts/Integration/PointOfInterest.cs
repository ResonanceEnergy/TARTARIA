using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// PointOfInterest — discovery trigger placed at Moon 1 / future-Moon vista or lore
    /// anchors (Overlook, Root Chamber, Carved Stone, Mud Pools, future POIs).
    ///
    /// On the first time the Player enters the trigger volume this component:
    ///   1. Fires <see cref="GameEvents.OnPOIDiscovered"/> (poiId, rsReward).
    ///   2. Awards RS via <see cref="GameEvents.FireRSChange"/> so the economy
    ///      counter ticks even when no HUD is bound.
    ///   3. Surfaces the discovery banner + subtitle through
    ///      <see cref="GameEvents.OnHUDShowBanner"/> / <see cref="GameEvents.OnHUDShowSubtitle"/>
    ///      and an interaction prompt fallback.
    ///   4. Plays a one-shot discovery chime (auto-loaded Resources clip if available,
    ///      otherwise a procedural bell via <see cref="AudioSource.PlayClipAtPoint"/>).
    ///
    /// Idempotent — re-entering the volume does nothing. Designed to be attached at
    /// edit time by <c>Moon1BuildOutEnvironment</c> or hand-placed in scenes.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class PointOfInterest : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string poiId = "POI_Unnamed";
        [SerializeField] private int rsReward = 5;

        [Header("Discovery Presentation")]
        [Tooltip("Banner title shown when the POI is discovered.")]
        [SerializeField] private string discoveryTitle = "Discovered";
        [Tooltip("Short narrative or lore line shown beneath the title.")]
        [SerializeField, TextArea(2, 4)] private string discoveryDialogue = "A new place revealed.";

        [Header("Audio")]
        [Tooltip("Optional override clip. If null, falls back to Resources/Audio/SFX/discovery_chime.")]
        [SerializeField] private AudioClip discoveryChime;
        [SerializeField, Range(0f, 1f)] private float chimeVolume = 0.8f;

        [Header("Tag Filter")]
        [Tooltip("Only objects with this tag count as the player. Empty = any rigidbody/CharacterController.")]
        [SerializeField] private string playerTag = "Player";

        private bool _alreadyDiscovered;

        public string PoiId => poiId;
        public int RsReward => rsReward;
        public bool AlreadyDiscovered => _alreadyDiscovered;

        void Reset()
        {
            // Force a sensible default collider config when component is added in Editor.
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        void Awake()
        {
            // Belt-and-suspenders: make sure whatever collider we have is a trigger.
            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
            }
        }

        public void Configure(string id, int reward, string title, string dialogue)
        {
            poiId = id;
            rsReward = reward;
            discoveryTitle = title;
            discoveryDialogue = dialogue;
        }

        void OnTriggerEnter(Collider other)
        {
            if (_alreadyDiscovered) return;
            if (!PassesPlayerFilter(other)) return;
            FireDiscovery();
        }

        bool PassesPlayerFilter(Collider other)
        {
            if (other == null) return false;
            if (!string.IsNullOrEmpty(playerTag))
            {
                // Compare against tag — guard for "Player" missing in TagManager (default tag is always present).
                try
                {
                    if (other.CompareTag(playerTag)) return true;
                }
                catch (UnityException)
                {
                    // playerTag undefined in TagManager — fall through to physics-based heuristic.
                }
            }
            // Heuristic fallback — accept any rigidbody or character controller (the player rig usually has one of these).
            if (other.attachedRigidbody != null) return true;
            if (other.GetComponentInParent<CharacterController>() != null) return true;
            return false;
        }

        void FireDiscovery()
        {
            _alreadyDiscovered = true;

            // Primary signal — typed event for any subscriber (Quest, HUD, analytics).
            GameEvents.FirePOIDiscovered(poiId, rsReward, transform.position);

            // Economy — award RS so the run-meta tracks discovery rewards.
            if (rsReward != 0) GameEvents.FireRSChange(rsReward);

            // HUD — banner + subtitle + interaction-prompt fallback so SOMETHING surfaces
            // regardless of which HUD binder is active.
            string subtitle = $"+{rsReward} RS — {discoveryDialogue}";
            GameEvents.RaiseHUDShowBanner(discoveryTitle, subtitle, 4.5f);
            GameEvents.RaiseHUDShowSubtitle(subtitle, 4.5f);
            GameEvents.RaiseHUDShowInteractionPrompt($"{discoveryTitle}: {discoveryDialogue}");

            // Audio — discovery chime.
            PlayDiscoveryChime();

            Debug.Log($"[PointOfInterest] Discovered '{poiId}' (+{rsReward} RS) at {transform.position}");
        }

        void PlayDiscoveryChime()
        {
            AudioClip clip = discoveryChime;
            if (clip == null)
            {
                // Lazy-load a shared bell if the project ships one — null-safe if it doesn't.
                clip = Resources.Load<AudioClip>("Audio/SFX/discovery_chime");
            }
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, chimeVolume);
                return;
            }
            // Final fallback — emit a one-shot sine-wave ping so playtesters get *some* feedback.
            EmitProceduralBell();
        }

        void EmitProceduralBell()
        {
            // 0.6s 880 Hz sine with exponential decay → "discovery ping".
            int sampleRate = 44100;
            int length = (int)(sampleRate * 0.6f);
            float[] samples = new float[length];
            const float freq = 880f;
            const float twoPi = 2f * Mathf.PI;
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float env = Mathf.Exp(-4f * t); // fast decay
                samples[i] = Mathf.Sin(twoPi * freq * t) * env * 0.5f;
            }
            var clip = AudioClip.Create($"POI_Bell_{poiId}", length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            AudioSource.PlayClipAtPoint(clip, transform.position, chimeVolume);
        }
    }
}
