using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Audio
{
    /// <summary>
    /// Catalog of named AudioCues. AudioManager.Play("ui_click") looks up here.
    /// Decouples gameplay code from specific clip assets.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Audio/Audio Cue Library", fileName = "AudioCueLibrary")]
    public class AudioCueLibrary : ScriptableObject
    {
        public AudioCue[] cues;

        Dictionary<string, AudioCue> _index;

        public AudioCue Find(string cueId)
        {
            if (_index == null) BuildIndex();
            return _index != null && _index.TryGetValue(cueId, out var cue) ? cue : null;
        }

        void BuildIndex()
        {
            _index = new Dictionary<string, AudioCue>();
            if (cues == null) return;
            foreach (var c in cues)
            {
                if (c == null || string.IsNullOrEmpty(c.cueId)) continue;
                _index[c.cueId] = c;
            }
        }

        void OnValidate() => _index = null;
    }
}
