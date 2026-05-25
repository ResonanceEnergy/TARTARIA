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
                Debug.Log($"[AssetFramework] Mixer exists: {MIXER_PATH} — verifying snapshots.");
                EnsureSnapshotsOnExistingMixer();
                return;
            }

            // Use Unity's internal AudioMixerController. The static
            // CreateMixerControllerAtPath silently no-ops in batchmode on
            // Unity 6000.3, so we instantiate the SO directly and call
            // CreateDefaultAsset(path) which is what the menu item does.
            // NOTE: AudioMixerController lives in the UnityEditor assembly,
            // NOT in typeof(AudioMixer).Assembly (which is UnityEngine.AudioModule).
            System.Type ctrlType = null;
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                ctrlType = a.GetType("UnityEditor.Audio.AudioMixerController");
                if (ctrlType != null) break;
            }
            if (ctrlType == null)
            {
                Debug.LogWarning("[AssetFramework] AudioMixerController type not found - " +
                                 "create the mixer manually: Assets > Create > Audio Mixer at " + MIXER_PATH);
                return;
            }

            ScriptableObject ctrlInstance = null;
            try
            {
                ctrlInstance = ScriptableObject.CreateInstance(ctrlType);
                if (ctrlInstance == null)
                {
                    Debug.LogWarning("[AssetFramework] ScriptableObject.CreateInstance(AudioMixerController) returned null.");
                }
                else
                {
                    // Try CreateDefaultAsset(path) — the canonical Unity menu path.
                    var createDefault = ctrlType.GetMethod("CreateDefaultAsset",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null, new[] { typeof(string) }, null);
                    if (createDefault != null)
                    {
                        createDefault.Invoke(ctrlInstance, new object[] { MIXER_PATH });
                    }
                    else
                    {
                        // Fallback: direct AssetDatabase.CreateAsset.
                        AssetDatabase.CreateAsset(ctrlInstance, MIXER_PATH);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[AssetFramework] Direct mixer instantiation failed: {ex.GetBaseException().Message}. " +
                                 "Falling back to static factory.");
            }

            if (!File.Exists(MIXER_PATH))
            {
                // Last resort: original static factory path.
                var createMethod = ctrlType.GetMethod("CreateMixerControllerAtPath",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (createMethod != null)
                {
                    try { createMethod.Invoke(null, new object[] { MIXER_PATH }); }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[AssetFramework] CreateMixerControllerAtPath threw: {ex.GetBaseException().Message}.");
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            if (File.Exists(MIXER_PATH))
            {
                AssetDatabase.ImportAsset(MIXER_PATH, ImportAssetOptions.ForceSynchronousImport);
            }

            // Add child groups + expose volume parameters.
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MIXER_PATH);
            if (mixer == null)
            {
                Debug.LogWarning($"[AssetFramework] Mixer file at '{MIXER_PATH}' not found after creation " +
                                 $"(file exists on disk: {File.Exists(MIXER_PATH)}). " +
                                 $"Create manually via menu Assets > Create > Audio Mixer once; subsequent builds will populate groups + snapshots.");
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

        static void EnsureSnapshotsOnExistingMixer()
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MIXER_PATH);
            if (mixer == null) return;
            System.Type ctrlType = null;
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                ctrlType = a.GetType("UnityEditor.Audio.AudioMixerController");
                if (ctrlType != null) break;
            }
            if (ctrlType == null) return;
            EnsureSnapshot(mixer, ctrlType, "Exploration", makeCurrent: true);
            EnsureSnapshot(mixer, ctrlType, "Combat",      makeCurrent: false);
            EditorUtility.SetDirty(mixer);
            AssetDatabase.SaveAssets();
        }

        static void EnsureSnapshot(AudioMixer mixer, System.Type ctrlType, string name, bool makeCurrent)
        {
            if (mixer == null || ctrlType == null) return;
            if (mixer.FindSnapshot(name) != null) return;

            // Approach 1: try named factory methods on the controller (signatures
            // have shifted across Unity versions — enumerate all overloads).
            string[] candidates = { "CreateNewSnapshotFromCurrent", "CreateNewSnapshot", "AddSnapshot", "AddNewSnapShot" };
            System.Reflection.MethodInfo createSnap = null;
            object[] callArgs = null;
            foreach (var n in candidates)
            {
                foreach (var m in ctrlType.GetMethods(System.Reflection.BindingFlags.Public |
                                                       System.Reflection.BindingFlags.NonPublic |
                                                       System.Reflection.BindingFlags.Instance))
                {
                    if (m.Name != n) continue;
                    var p = m.GetParameters();
                    if (p.Length == 2 && p[0].ParameterType == typeof(string) && p[1].ParameterType == typeof(bool))
                    { createSnap = m; callArgs = new object[] { name, makeCurrent }; break; }
                    if (p.Length == 1 && p[0].ParameterType == typeof(string))
                    { createSnap = m; callArgs = new object[] { name }; break; }
                }
                if (createSnap != null) break;
            }
            if (createSnap != null)
            {
                try { createSnap.Invoke(mixer, callArgs); }
                catch (System.Exception ex)
                { Debug.LogWarning($"[AssetFramework] Named snapshot factory '{createSnap.Name}' threw: {ex.GetBaseException().Message}"); }
                if (mixer.FindSnapshot(name) != null)
                {
                    Debug.Log($"[AssetFramework] Snapshot '{name}' added via {createSnap.Name}.");
                    return;
                }
            }

            // Approach 2: fallback — instantiate AudioMixerSnapshotController directly,
            // attach as a sub-asset, and append to the controller's m_Snapshots array via
            // SerializedObject. This mirrors what AudioMixerController.AddNewSnapShot does
            // internally on Unity 6 and is stable across editor versions.
            System.Type snapType = null;
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                snapType = a.GetType("UnityEditor.Audio.AudioMixerSnapshotController");
                if (snapType != null) break;
            }
            if (snapType == null)
            {
                Debug.LogWarning($"[AssetFramework] AudioMixerSnapshotController type not found; cannot add snapshot '{name}'.");
                return;
            }

            try
            {
                // Constructor: AudioMixerSnapshotController(AudioMixer owner)
                object snapInstance = null;
                var ctorOwner = snapType.GetConstructor(new[] { typeof(AudioMixer) });
                if (ctorOwner != null) snapInstance = ctorOwner.Invoke(new object[] { mixer });
                else snapInstance = ScriptableObject.CreateInstance(snapType);
                if (snapInstance is UnityEngine.Object so)
                {
                    so.name = name;
                    AssetDatabase.AddObjectToAsset(so, mixer);
                    var sObj = new SerializedObject(mixer);
                    var arr = sObj.FindProperty("m_Snapshots");
                    if (arr != null && arr.isArray)
                    {
                        int idx = arr.arraySize;
                        arr.InsertArrayElementAtIndex(idx);
                        arr.GetArrayElementAtIndex(idx).objectReferenceValue = so;
                        if (makeCurrent)
                        {
                            var startProp = sObj.FindProperty("m_StartSnapshot");
                            if (startProp != null) startProp.objectReferenceValue = so;
                        }
                        sObj.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(mixer);
                        AssetDatabase.SaveAssets();
                        Debug.Log($"[AssetFramework] Snapshot '{name}' added via SerializedObject fallback.");
                        return;
                    }
                }
                Debug.LogWarning($"[AssetFramework] SerializedObject fallback could not append snapshot '{name}'.");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[AssetFramework] SerializedObject snapshot fallback threw: {ex.GetBaseException().Message}");
            }
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
            // Build ExposedAudioParameter struct via reflection (UnityEditor assembly).
            System.Type paramType = null;
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                paramType = a.GetType("UnityEditor.Audio.ExposedAudioParameter");
                if (paramType != null) break;
            }
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
