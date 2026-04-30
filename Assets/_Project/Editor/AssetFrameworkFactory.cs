using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using Tartaria.Audio;
using Tartaria.Core.Data;

namespace Tartaria.Editor
{
    /// <summary>
    /// Phase 9k factory: bootstraps the asset-framework SOs that decouple
    /// content from code — AudioMixer, AudioCueLibrary, default CharacterVisualProfile,
    /// MaterialVariantSets. Idempotent: safe to re-run on every build.
    /// </summary>
    public static class AssetFrameworkFactory
    {
        const string MIXER_PATH         = "Assets/_Project/Audio/Mixers/MasterMixer.mixer";
        const string CUE_LIB_PATH       = "Assets/_Project/Audio/AudioCueLibrary.asset";
        const string PROFILE_PATH       = "Assets/_Project/Config/Profile_Elara_Capsule.asset";
        const string MV_STONE_PATH      = "Assets/_Project/Config/MV_Stone.asset";

        [MenuItem("TARTARIA/Asset Framework/Bootstrap All")]
        public static void BootstrapAll()
        {
            EnsureFolders();
            CreateMixerIfMissing();
            CreateCueLibraryIfMissing();
            CreateDefaultCharacterProfileIfMissing();
            CreateDefaultMaterialVariantsIfMissing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AssetFramework] Bootstrap complete.");
        }

        static void EnsureFolders()
        {
            CreateFolder("Assets/_Project/Audio/Mixers");
            CreateFolder("Assets/_Project/Audio/Music");
            CreateFolder("Assets/_Project/Audio/SFX");
            CreateFolder("Assets/_Project/Audio/Ambience");
            CreateFolder("Assets/_Project/Config");
        }

        static void CreateFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)!.Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) CreateFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        static void CreateMixerIfMissing()
        {
            if (File.Exists(MIXER_PATH))
            {
                Debug.Log($"[AssetFramework] Mixer exists: {MIXER_PATH}");
                return;
            }

            // Use Unity's internal AudioMixerController.CreateMixerControllerAtPath via reflection.
            // This produces a valid mixer asset with a default Master group.
            var asm = typeof(AudioMixer).Assembly;
            var ctrlType = asm.GetType("UnityEditor.Audio.AudioMixerController");
            if (ctrlType == null)
            {
                Debug.LogWarning("[AssetFramework] AudioMixerController type not found - " +
                                 "create the mixer manually: Assets > Create > Audio Mixer at " + MIXER_PATH);
                return;
            }
            var createMethod = ctrlType.GetMethod("CreateMixerControllerAtPath",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (createMethod == null)
            {
                Debug.LogWarning("[AssetFramework] CreateMixerControllerAtPath method not found.");
                return;
            }

            createMethod.Invoke(null, new object[] { MIXER_PATH });
            AssetDatabase.ImportAsset(MIXER_PATH, ImportAssetOptions.ForceSynchronousImport);

            // Add child groups + expose volume parameters.
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MIXER_PATH);
            if (mixer == null)
            {
                Debug.LogWarning("[AssetFramework] Mixer created but failed to load.");
                return;
            }

            var masterGroup = mixer.FindMatchingGroups("Master")[0];
            string[] childNames = { "Music", "SFX", "UI", "Ambience", "Voice" };

            var addChildMethod = ctrlType.GetMethod("CreateNewGroup",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                null, new[] { typeof(string), typeof(bool) }, null);
            var addToParent = ctrlType.GetMethod("AddChildToParent",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var addExposed = ctrlType.GetMethod("AddExposedParameter",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            // Expose master volume.
            TryExposeVolume(mixer, masterGroup, "MasterVolume", addExposed);

            foreach (var name in childNames)
            {
                if (mixer.FindMatchingGroups(name).Length > 0) continue;
                if (addChildMethod == null || addToParent == null) continue;
                var newGroup = addChildMethod.Invoke(mixer, new object[] { name, false });
                addToParent.Invoke(mixer, new[] { newGroup, masterGroup });
                if (newGroup is AudioMixerGroup g)
                    TryExposeVolume(mixer, g, name + "Volume", addExposed);
            }

            // Snapshots: Exploration (default) + Combat. dB values left at defaults;
            // designer tunes per-group ducking in the Audio Mixer window. The runtime
            // (AudioManager) selects which snapshot is active based on GameState.
            EnsureSnapshot(mixer, ctrlType, "Exploration", makeCurrent: true);
            EnsureSnapshot(mixer, ctrlType, "Combat",      makeCurrent: false);

            EditorUtility.SetDirty(mixer);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AssetFramework] Mixer created with 6 groups + 2 snapshots: {MIXER_PATH}");
        }

        /// <summary>
        /// Adds a snapshot to the mixer if missing, via internal
        /// AudioMixerController.CreateNewSnapshotFromCurrent reflection.
        /// </summary>
        static void EnsureSnapshot(AudioMixer mixer, System.Type ctrlType, string name, bool makeCurrent)
        {
            if (mixer == null || ctrlType == null) return;
            if (mixer.FindSnapshot(name) != null) return;

            var createSnap = ctrlType.GetMethod("CreateNewSnapshotFromCurrent",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                null, new[] { typeof(string), typeof(bool) }, null);
            if (createSnap == null)
            {
                Debug.LogWarning($"[AssetFramework] CreateNewSnapshotFromCurrent not found - " +
                                 $"add snapshot '{name}' manually in the mixer window.");
                return;
            }

            createSnap.Invoke(mixer, new object[] { name, makeCurrent });
        }

        static void TryExposeVolume(AudioMixer mixer, AudioMixerGroup group, string exposedName,
                                    System.Reflection.MethodInfo addExposedMethod)
        {
            if (mixer == null || group == null || addExposedMethod == null) return;
            // Find the volume GUID on the group via reflection.
            var grpType = group.GetType();
            var volProp = grpType.GetField("m_Volume",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (volProp == null) return;
            var volGuid = volProp.GetValue(group);
            // Build ExposedAudioParameter struct via reflection.
            var asm = typeof(AudioMixer).Assembly;
            var paramType = asm.GetType("UnityEditor.Audio.ExposedAudioParameter");
            if (paramType == null) return;
            var paramObj = System.Activator.CreateInstance(paramType);
            paramType.GetField("name").SetValue(paramObj, exposedName);
            paramType.GetField("guid").SetValue(paramObj, volGuid);
            try { addExposedMethod.Invoke(mixer, new[] { paramObj }); }
            catch { /* parameter might already exist */ }
        }

        static void CreateCueLibraryIfMissing()
        {
            if (File.Exists(CUE_LIB_PATH)) return;
            var lib = ScriptableObject.CreateInstance<AudioCueLibrary>();
            lib.cues = new AudioCue[0];
            AssetDatabase.CreateAsset(lib, CUE_LIB_PATH);
            Debug.Log($"[AssetFramework] AudioCueLibrary created: {CUE_LIB_PATH}");
        }

        static void CreateDefaultCharacterProfileIfMissing()
        {
            if (File.Exists(PROFILE_PATH)) return;
            var profile = ScriptableObject.CreateInstance<CharacterVisualProfile>();
            profile.characterId = "elara_capsule";
            profile.displayName = "Elara Voss (Capsule)";
            // Mesh + animator deliberately null — uses Player.prefab as-is (procedural capsule).
            AssetDatabase.CreateAsset(profile, PROFILE_PATH);
            Debug.Log($"[AssetFramework] Default CharacterVisualProfile created: {PROFILE_PATH}");
        }

        static void CreateDefaultMaterialVariantsIfMissing()
        {
            if (File.Exists(MV_STONE_PATH)) return;
            var mv = ScriptableObject.CreateInstance<MaterialVariantSet>();
            mv.variantId = "stone";
            AssetDatabase.CreateAsset(mv, MV_STONE_PATH);
            Debug.Log($"[AssetFramework] MaterialVariantSet created: {MV_STONE_PATH}");
        }
    }
}
