using UnityEngine;
using UnityEngine.Audio;

namespace Tartaria.Audio
{
    /// <summary>
    /// One named audio cue. Multiple clips = automatic random pick (anti-repetition).
    /// Targets a specific AudioMixerGroup so the player's volume sliders work.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Audio/Audio Cue", fileName = "Cue_New")]
    public class AudioCue : ScriptableObject
    {
        public string cueId = "ui_click";

        [Tooltip("One or more variants — a random one is picked on Play to avoid repetition.")]
        public AudioClip[] clips;

        public AudioMixerGroup mixerGroup;

        [Range(0f, 1f)] public float volume = 1f;
        public Vector2 pitchRange = new(1f, 1f);

        [Tooltip("0 = 2D / UI, 1 = full 3D positional.")]
        [Range(0f, 1f)] public float spatialBlend = 0f;

        [Tooltip("Loop on play (set false for one-shots).")]
        public bool loop = false;

        public AudioClip PickClip()
        {
            if (clips == null || clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }

        public float PickPitch()
        {
            return Random.Range(pitchRange.x, pitchRange.y);
        }
    }
}
