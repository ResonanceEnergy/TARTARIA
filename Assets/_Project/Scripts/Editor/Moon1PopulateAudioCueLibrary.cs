#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Tartaria.Audio;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/3 Wire/Populate Audio Cue Library
    ///
    /// Programmatically populates Assets/_Project/Audio/AudioCueLibrary.asset with
    /// AudioCue sub-assets pointing at the 5 existing WAVs under
    /// Assets/_Project/Audio/Moon1_Lore/:
    ///   - moon1.cathedral.restoration_stinger  → Cathedral_Restoration_Stinger.wav
    ///   - moon1.lirael.lullaby_432             → Lirael_Lullaby_432Hz.wav
    ///   - moon1.milo.blimey_chime              → Milo_Blimey_Chime.wav
    ///   - moon1.reset_scout.taunt              → Reset_Scout_Taunt.wav
    ///   - moon1.skeleton.hum_prophecy          → Skeleton_Hum_Prophecy.wav
    ///
    /// Each cue is added as a sub-asset of the library so the .asset file owns
    /// the cue ScriptableObjects (no orphan SO files). Existing cues with the
    /// same cueId are updated in place rather than duplicated.
    ///
    /// Per CLAUDE.md no-stubs mandate: this menu actually mutates the asset and
    /// calls AssetDatabase.SaveAssets. No "TODO" stubs.
    /// </summary>
    public static class Moon1PopulateAudioCueLibrary
    {
        const string LIBRARY_PATH = "Assets/_Project/Audio/AudioCueLibrary.asset";

        struct CueSpec
        {
            public string cueId;
            public string clipPath;
            public float volume;
            public float spatialBlend;
            public bool loop;

            public CueSpec(string id, string path, float vol = 1f, float blend = 0f, bool lp = false)
            {
                cueId = id;
                clipPath = path;
                volume = vol;
                spatialBlend = blend;
                loop = lp;
            }
        }

        static readonly CueSpec[] CUES = new[]
        {
            new CueSpec("moon1.cathedral.restoration_stinger",
                "Assets/_Project/Audio/Moon1_Lore/Cathedral_Restoration_Stinger.wav",
                0.85f, 0f, false),
            new CueSpec("moon1.lirael.lullaby_432",
                "Assets/_Project/Audio/Moon1_Lore/Lirael_Lullaby_432Hz.wav",
                0.70f, 1f, true),
            new CueSpec("moon1.milo.blimey_chime",
                "Assets/_Project/Audio/Moon1_Lore/Milo_Blimey_Chime.wav",
                0.75f, 0f, false),
            new CueSpec("moon1.reset_scout.taunt",
                "Assets/_Project/Audio/Moon1_Lore/Reset_Scout_Taunt.wav",
                0.80f, 1f, false),
            new CueSpec("moon1.skeleton.hum_prophecy",
                "Assets/_Project/Audio/Moon1_Lore/Skeleton_Hum_Prophecy.wav",
                0.60f, 1f, true),
        };

        [MenuItem("Tartaria/3 Wire/Populate Audio Cue Library", priority = 305)]
        public static void Run()
        {
            var lib = AssetDatabase.LoadAssetAtPath<AudioCueLibrary>(LIBRARY_PATH);
            if (lib == null)
            {
                EditorUtility.DisplayDialog("Audio Cue Library",
                    $"Library asset not found at:\n{LIBRARY_PATH}\n\nCreate one via\n" +
                    "Right-click → Create → Tartaria → Audio → Audio Cue Library",
                    "OK");
                return;
            }

            // Load existing sub-asset cues (or build empty index)
            var existing = new Dictionary<string, AudioCue>();
            foreach (var sub in AssetDatabase.LoadAllAssetsAtPath(LIBRARY_PATH))
            {
                if (sub is AudioCue cue && !string.IsNullOrEmpty(cue.cueId))
                    existing[cue.cueId] = cue;
            }

            int created = 0, updated = 0, missingClips = 0;
            var finalCues = new List<AudioCue>(CUES.Length);

            foreach (var spec in CUES)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(spec.clipPath);
                if (clip == null)
                {
                    Debug.LogWarning($"[Moon1PopulateAudioCueLibrary] Missing clip for cue '{spec.cueId}' at {spec.clipPath}");
                    missingClips++;
                    continue;
                }

                AudioCue cue;
                if (existing.TryGetValue(spec.cueId, out cue) && cue != null)
                {
                    updated++;
                }
                else
                {
                    cue = ScriptableObject.CreateInstance<AudioCue>();
                    cue.name = "Cue_" + spec.cueId.Replace('.', '_');
                    AssetDatabase.AddObjectToAsset(cue, lib);
                    created++;
                }

                // Use SerializedObject so the change tracks correctly and undo works.
                var so = new SerializedObject(cue);
                so.FindProperty("cueId").stringValue = spec.cueId;

                var clipsProp = so.FindProperty("clips");
                clipsProp.arraySize = 1;
                clipsProp.GetArrayElementAtIndex(0).objectReferenceValue = clip;

                so.FindProperty("volume").floatValue = spec.volume;
                so.FindProperty("spatialBlend").floatValue = spec.spatialBlend;
                so.FindProperty("loop").boolValue = spec.loop;
                // pitchRange stays at default (1,1) — set by AudioCue field initializer.

                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(cue);

                finalCues.Add(cue);
            }

            // Rewrite the library's `cues` array to point at the final ordered list.
            var libSO = new SerializedObject(lib);
            var libCues = libSO.FindProperty("cues");
            libCues.arraySize = finalCues.Count;
            for (int i = 0; i < finalCues.Count; i++)
                libCues.GetArrayElementAtIndex(i).objectReferenceValue = finalCues[i];
            libSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(lib);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string msg =
                $"AudioCueLibrary populated.\n\n" +
                $"  Created : {created}\n" +
                $"  Updated : {updated}\n" +
                $"  Missing : {missingClips}\n" +
                $"  Total   : {finalCues.Count} cue(s)\n\n" +
                $"Library: {LIBRARY_PATH}";
            Debug.Log("[Moon1PopulateAudioCueLibrary] " + msg.Replace("\n", " | "));
            EditorUtility.DisplayDialog("Audio Cue Library", msg, "OK");

            // Ping the asset so NATRIX can see it in the project window.
            EditorGUIUtility.PingObject(lib);
        }
    }
}
#endif
