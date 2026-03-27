using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Creates placeholder AudioClip assets (1-sample silent clips) so
    /// AdaptiveMusicController and AudioManager SerializeFields can be assigned.
    /// Menu: Tartaria > Build Assets > Audio Stubs
    /// </summary>
    public static class AudioStubFactory
    {
        const string MusicPath = "Assets/_Project/Audio/Music";
        const string SFXPath = "Assets/_Project/Audio/SFX";

        [MenuItem("Tartaria/Build Assets/Audio Stubs", false, 22)]
        public static void BuildAudioStubs()
        {
            EnsureDirectories();

            // Music beds (AdaptiveMusicController)
            CreateSilentClip(MusicPath, "MusicBed_Exploration_Low", 5f);
            CreateSilentClip(MusicPath, "MusicBed_Exploration_High", 5f);
            CreateSilentClip(MusicPath, "MusicBed_Combat_Low", 5f);
            CreateSilentClip(MusicPath, "MusicBed_Combat_High", 5f);

            // Stingers
            CreateSilentClip(SFXPath, "Stinger_Discovery", 2f);
            CreateSilentClip(SFXPath, "Stinger_Restoration", 3f);
            CreateSilentClip(SFXPath, "Stinger_CombatStart", 1.5f);
            CreateSilentClip(SFXPath, "Stinger_ZoneShift", 2f);
            CreateSilentClip(SFXPath, "Stinger_AetherWake", 2f);

            // SFX
            CreateSilentClip(SFXPath, "SFX_ResonancePulse", 0.5f);
            CreateSilentClip(SFXPath, "SFX_HarmonicStrike", 0.5f);
            CreateSilentClip(SFXPath, "SFX_FrequencyShield", 0.8f);
            CreateSilentClip(SFXPath, "SFX_GolemSpawn", 1f);
            CreateSilentClip(SFXPath, "SFX_GolemDeath", 1f);
            CreateSilentClip(SFXPath, "SFX_TuningSuccess", 0.5f);
            CreateSilentClip(SFXPath, "SFX_TuningFail", 0.5f);
            CreateSilentClip(SFXPath, "SFX_BuildingEmerge", 3f);
            CreateSilentClip(SFXPath, "SFX_MoteCollect", 0.3f);
            CreateSilentClip(SFXPath, "SFX_Footstep", 0.2f);
            CreateSilentClip(SFXPath, "SFX_Anastasia_Whisper", 1f);
            CreateSilentClip(SFXPath, "SFX_Solidification", 4f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AudioStubs] All audio placeholder clips created.");
        }

        static void CreateSilentClip(string folder, string clipName, float durationSeconds)
        {
            string path = $"{folder}/{clipName}.asset";
            if (AssetDatabase.LoadAssetAtPath<AudioClip>(path) != null) return;

            int sampleRate = 44100;
            int samples = Mathf.Max(1, (int)(sampleRate * durationSeconds));
            var clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);

            // Fill with silence (all zeros)
            float[] data = new float[samples];
            clip.SetData(data, 0);

            AssetDatabase.CreateAsset(clip, path);
        }

        static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Audio"))
                AssetDatabase.CreateFolder("Assets/_Project", "Audio");
            if (!AssetDatabase.IsValidFolder(MusicPath))
                AssetDatabase.CreateFolder("Assets/_Project/Audio", "Music");
            if (!AssetDatabase.IsValidFolder(SFXPath))
                AssetDatabase.CreateFolder("Assets/_Project/Audio", "SFX");
        }
    }
}
