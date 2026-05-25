using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Tartaria.Editor
{
    /// <summary>
    /// Drop-zone binder for designer-supplied ambient music tracks.
    /// Scans <c>Assets/_Project/Audio/Ambience/</c> for any .wav/.ogg/.mp3 the
    /// user has dropped (e.g. Sonniss GDC bundle ambient cuts) and attaches
    /// looping AudioSources under the existing <c>AudioAmbience</c> scene root.
    ///
    /// Idempotent: re-running won't duplicate sources for the same clip name.
    /// Routes to the "Ambience" mixer group if MasterMixer.mixer is present.
    /// </summary>
    public static class AmbienceAutoBinder
    {
        const string AmbienceDir = "Assets/_Project/Audio/Ambience";
        const string MixerPath   = "Assets/_Project/Audio/Mixers/MasterMixer.mixer";
        const float DefaultVolume = 0.22f;

        [MenuItem("TARTARIA/Audio/Bind Ambience Tracks")]
        public static void BindAll()
        {
            if (!AssetDatabase.IsValidFolder(AmbienceDir))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Audio", "Ambience");
                Debug.Log($"[AmbienceBinder] Created drop-zone folder: {AmbienceDir}");
            }

            var clips = AssetDatabase.FindAssets("t:AudioClip", new[] { AmbienceDir })
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Select(p => AssetDatabase.LoadAssetAtPath<AudioClip>(p))
                .Where(c => c != null)
                .ToArray();

            if (clips.Length == 0)
            {
                Debug.Log($"[AmbienceBinder] No clips in {AmbienceDir} — drop .wav/.ogg files there to auto-wire.");
                return;
            }

            var root = GameObject.Find("AudioAmbience");
            if (root == null)
            {
                root = new GameObject("AudioAmbience");
                Debug.Log("[AmbienceBinder] Created AudioAmbience root.");
            }

            // Find or create a "DesignerTracks" subgroup so we don't collide with
            // the procedural Wind/Choir loops attached by AudioFactory.
            var groupName = "DesignerTracks";
            var groupTf = root.transform.Find(groupName);
            GameObject group;
            if (groupTf == null)
            {
                group = new GameObject(groupName);
                group.transform.SetParent(root.transform, false);
            }
            else
            {
                group = groupTf.gameObject;
            }

            AudioMixerGroup mixerGroup = ResolveAmbienceMixerGroup();

            int added = 0, skipped = 0;
            foreach (var clip in clips)
            {
                var clipName = clip.name;
                // Skip if a child source for this clip already exists.
                if (group.GetComponentsInChildren<AudioSource>(true).Any(s => s.clip == clip))
                {
                    skipped++;
                    continue;
                }
                var go = new GameObject(clipName);
                go.transform.SetParent(group.transform, false);
                var src = go.AddComponent<AudioSource>();
                src.clip = clip;
                src.loop = true;
                src.volume = DefaultVolume;
                src.spatialBlend = 0f;
                src.playOnAwake = true;
                if (mixerGroup != null) src.outputAudioMixerGroup = mixerGroup;
                added++;
            }

            Debug.Log($"[AmbienceBinder] Bound {added} new ambient track(s), skipped {skipped} already wired.");
        }

        static AudioMixerGroup ResolveAmbienceMixerGroup()
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer == null) return null;
            var groups = mixer.FindMatchingGroups("Ambience");
            return (groups != null && groups.Length > 0) ? groups[0] : null;
        }
    }
}
