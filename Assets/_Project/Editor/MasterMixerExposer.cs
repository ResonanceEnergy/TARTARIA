using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Tartaria.EditorTools
{
    /// <summary>
    /// Exposes Master/Music/SFX/UI/Ambience volume parameters on MasterMixer.mixer
    /// so SettingsOverlay can drive them via mixer.SetFloat("MasterVol", dB).
    ///
    /// Edits the mixer's serialized m_ExposedParameters list directly (no public API).
    /// Idempotent.
    /// </summary>
    public static class MasterMixerExposer
    {
        const string MixerPath = "Assets/_Project/Audio/Mixers/MasterMixer.mixer";

        // Group display name → exposed parameter name we want to add.
        static readonly (string group, string param)[] Wanted =
        {
            ("Master",   "MasterVol"),
            ("Music",    "MusicVol"),
            ("SFX",      "SFXVol"),
            ("UI",       "UIVol"),
            ("Ambience", "AmbienceVol"),
        };

        [MenuItem("Tartaria/Fix/Expose Master Mixer Parameters")]
        public static void Run()
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer == null)
            {
                Debug.LogWarning($"[MasterMixerExposer] Mixer not found at {MixerPath}");
                return;
            }

            var so = new SerializedObject(mixer);
            var exposedProp = so.FindProperty("m_ExposedParameters");
            if (exposedProp == null)
            {
                Debug.LogError("[MasterMixerExposer] m_ExposedParameters not found on mixer.");
                return;
            }

            // Find each named group in m_MasterGroup tree and capture its m_GroupID, then
            // add an entry to m_ExposedParameters with parameter = "Volume" guid + a name.
            // The mixer's attenuation parameter GUID is on the group's m_Effects[0] (Attenuation).
            // We use SerializedObject reflection to walk the tree.

            var groupIds = CollectAttenuationGuids(so);
            int added = 0;

            foreach (var (groupName, paramName) in Wanted)
            {
                if (!groupIds.TryGetValue(groupName, out var guid))
                {
                    Debug.LogWarning($"[MasterMixerExposer] Group '{groupName}' not found.");
                    continue;
                }

                if (HasExposedParameter(exposedProp, paramName)) continue;

                exposedProp.arraySize++;
                var entry = exposedProp.GetArrayElementAtIndex(exposedProp.arraySize - 1);
                entry.FindPropertyRelative("name").stringValue = paramName;
                var guidProp = entry.FindPropertyRelative("guid");
                guidProp.FindPropertyRelative("data1").uintValue = guid.data1;
                guidProp.FindPropertyRelative("data2").uintValue = guid.data2;
                guidProp.FindPropertyRelative("data3").uintValue = guid.data3;
                guidProp.FindPropertyRelative("data4").uintValue = guid.data4;
                added++;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mixer);
            AssetDatabase.SaveAssetIfDirty(mixer);

            // Ensure a Resources locator exists so runtime code can find the mixer.
            EnsureLocator(mixer);

            Debug.Log($"[MasterMixerExposer] Exposed {added} new mixer parameters (already-exposed entries left intact).");
        }

        static void EnsureLocator(AudioMixer mixer)
        {
            const string ResDir  = "Assets/_Project/Resources";
            const string LocPath = "Assets/_Project/Resources/MasterMixerLocator.asset";

            if (!AssetDatabase.IsValidFolder(ResDir))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");

            var locator = AssetDatabase.LoadAssetAtPath<Tartaria.Audio.MasterMixerLocator>(LocPath);
            if (locator == null)
            {
                locator = ScriptableObject.CreateInstance<Tartaria.Audio.MasterMixerLocator>();
                AssetDatabase.CreateAsset(locator, LocPath);
            }
            if (locator.mixer != mixer)
            {
                locator.mixer = mixer;
                EditorUtility.SetDirty(locator);
                AssetDatabase.SaveAssetIfDirty(locator);
            }
        }

        struct Guid128
        {
            public uint data1, data2, data3, data4;
        }

        static bool HasExposedParameter(SerializedProperty exposed, string name)
        {
            for (int i = 0; i < exposed.arraySize; i++)
            {
                var n = exposed.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
                if (n == name) return true;
            }
            return false;
        }

        // Walk m_MasterGroup recursively. For each group, find its first Attenuation effect's
        // first parameter GUID — that's "Volume" — and map groupName → guid.
        static System.Collections.Generic.Dictionary<string, Guid128> CollectAttenuationGuids(SerializedObject mixerSO)
        {
            var dict = new System.Collections.Generic.Dictionary<string, Guid128>();
            var master = mixerSO.FindProperty("m_MasterGroup");
            if (master != null) Walk(master, dict);
            return dict;
        }

        static void Walk(SerializedProperty group, System.Collections.Generic.Dictionary<string, Guid128> dict)
        {
            if (group == null || group.objectReferenceValue == null) return;
            var groupSO = new SerializedObject(group.objectReferenceValue);

            string name = groupSO.FindProperty("m_Name")?.stringValue ?? "(unnamed)";

            var effects = groupSO.FindProperty("m_Effects");
            if (effects != null)
            {
                for (int i = 0; i < effects.arraySize; i++)
                {
                    var fxRef = effects.GetArrayElementAtIndex(i);
                    if (fxRef.objectReferenceValue == null) continue;
                    var fxSO = new SerializedObject(fxRef.objectReferenceValue);
                    string fxName = fxSO.FindProperty("m_EffectName")?.stringValue;
                    if (fxName != "Attenuation") continue;

                    var paramsProp = fxSO.FindProperty("m_Parameters");
                    if (paramsProp == null || paramsProp.arraySize == 0) continue;

                    var firstParam = paramsProp.GetArrayElementAtIndex(0);
                    var guidProp = firstParam.FindPropertyRelative("ParameterID");
                    if (guidProp == null) continue;

                    var g = new Guid128
                    {
                        data1 = guidProp.FindPropertyRelative("data1").uintValue,
                        data2 = guidProp.FindPropertyRelative("data2").uintValue,
                        data3 = guidProp.FindPropertyRelative("data3").uintValue,
                        data4 = guidProp.FindPropertyRelative("data4").uintValue,
                    };
                    if (!dict.ContainsKey(name)) dict.Add(name, g);
                    break;
                }
            }

            var children = groupSO.FindProperty("m_Children");
            if (children != null)
            {
                for (int i = 0; i < children.arraySize; i++)
                    Walk(children.GetArrayElementAtIndex(i), dict);
            }
        }
    }
}
