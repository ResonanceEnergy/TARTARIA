using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Audio
{
    /// <summary>
    /// VO Placeholder Library — maps dialogue line IDs to placeholder AudioClips.
    /// Lazy-loads clips from Resources/VO/Placeholder/ as needed.
    /// Used by DialogueManager to play procedural beep tones until real VO is recorded.
    /// </summary>
    [CreateAssetMenu(fileName = "VOPlaceholderLibrary", menuName = "Tartaria/Audio/VO Placeholder Library")]
    public class VOPlaceholderLibrary : ScriptableObject
    {
        static VOPlaceholderLibrary s_instance;
        static readonly Dictionary<string, AudioClip> s_clipCache = new();
        static readonly AudioClip[] s_voClips = new AudioClip[12];
        static bool s_initialized;

        /// <summary>
        /// Play a VO placeholder for the given line ID if available.
        /// Returns true if a clip was played, false if no mapping exists.
        /// </summary>
        public static bool PlayLineIfAvailable(string lineId)
        {
            if (string.IsNullOrEmpty(lineId)) return false;

            EnsureInitialized();

            // Check if we have a cached clip for this line
            if (s_clipCache.TryGetValue(lineId, out var clip) && clip != null)
            {
                AudioManager.Instance?.PlayVoiceLine(lineId, clip);
                return true;
            }

            // Map line ID to one of 12 placeholder tones (hash-based distribution)
            int voIndex = Mathf.Abs(lineId.GetHashCode()) % 12;
            AudioClip voClip = s_voClips[voIndex];

            if (voClip == null)
            {
                // Lazy-load the VO clip from Resources
                voClip = Resources.Load<AudioClip>($"VO/Placeholder/vo_{voIndex:D2}");
                s_voClips[voIndex] = voClip;
            }

            if (voClip == null)
            {
                // Placeholder not found — return false to signal text-only mode
                return false;
            }

            s_clipCache[lineId] = voClip;
            AudioManager.Instance?.PlayVoiceLine(lineId, voClip);
            return true;
        }

        static void EnsureInitialized()
        {
            if (s_initialized) return;
            s_initialized = true;

            // Pre-load all 12 VO clips on first use
            for (int i = 0; i < 12; i++)
            {
                s_voClips[i] = Resources.Load<AudioClip>($"VO/Placeholder/vo_{i:D2}");
            }

            Debug.Log("[VOPlaceholderLibrary] Initialized with 12 VO placeholder clips.");
        }
    }

    /// <summary>
    /// Extension for AudioManager to play VO clips. If AudioManager doesn't have
    /// a PlayVoiceLine(string, AudioClip) overload, this method no-ops gracefully.
    /// </summary>
    public static class AudioManagerVOExtension
    {
        public static void PlayVoiceLine(this AudioManager manager, string lineId, AudioClip clip)
        {
            if (manager == null || clip == null) return;

            // Try to find an AudioSource on the AudioManager or create a transient one
            var source = manager.GetComponent<AudioSource>();
            if (source == null)
            {
                var go = new GameObject("VO_Transient");
                go.transform.SetParent(manager.transform);
                source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f; // 2D
                Object.Destroy(go, clip.length + 0.5f); // Auto-cleanup
            }

            source.PlayOneShot(clip, 0.7f);
            Debug.Log($"[VOPlaceholder] Playing VO for line '{lineId}' (clip: {clip.name})");
        }
    }
}
