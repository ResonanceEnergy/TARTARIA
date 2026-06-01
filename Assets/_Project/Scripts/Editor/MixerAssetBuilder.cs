// MixerAssetBuilder.cs
// Editor-only tool: programmatically creates EchohavenMaster.mixer with
//   - groups: Music, SFX, Ambient, UI (children of the default Master group)
//   - snapshots: Normal, Paused (Paused ducks Music by -20 dB)
//
// AudioMixer .mixer assets are binary YAML; Unity exposes no public API
// to author them at edit time. We use reflection over the INTERNAL
// UnityEditor.Audio.AudioMixerController. If any reflection step fails
// (Unity version drift), we fall back to an EditorUtility.DisplayDialog
// instructing Cowork to author the asset by hand.
//
// Owner: agent/audio/mixer-snapshot-system
// Menu: Tartaria/5 Audio/Create EchohavenMaster Mixer  (priority 51)
// Output path (per spec): Assets/_Project/Audio/Mixers/EchohavenMaster.mixer
//
// NOTE: For MixerSnapshotController.LoadMixer() (runtime) to resolve the
// asset via Resources.Load("Audio/Mixers/EchohavenMaster"), this builder
// also writes a MasterMixerLocator-style shim into Resources --- OR Cowork
// can move/copy the .mixer into a Resources/ subfolder. The dialog at the
// end of the build documents this.

#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Tartaria.Editor.Audio
{
    public static class MixerAssetBuilder
    {
        const string OutputDir = "Assets/_Project/Audio/Mixers";
        const string OutputPath = "Assets/_Project/Audio/Mixers/EchohavenMaster.mixer";
        const string ResourcesMirrorDir = "Assets/_Project/Resources/Audio/Mixers";
        const string ResourcesMirrorPath = "Assets/_Project/Resources/Audio/Mixers/EchohavenMaster.mixer";

        static readonly string[] GroupNames = { "Music", "SFX", "Ambient", "UI" };
        const string NormalSnapshot = "Normal";
        const string PausedSnapshot = "Paused";
        const float PausedMusicDuckDb = -20f;

        [MenuItem("Tartaria/5 Audio/Create EchohavenMaster Mixer", false, 51)]
        public static void CreateEchohavenMaster()
        {
            EnsureDir(OutputDir);

            string status;
            bool ok = TryBuildMixer(out status);

            if (ok)
            {
                MirrorIntoResources();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog(
                    "EchohavenMaster Mixer",
                    "Created Assets/_Project/Audio/Mixers/EchohavenMaster.mixer.\n\n" +
                    "Groups: Music, SFX, Ambient, UI\n" +
                    "Snapshots: Normal, Paused (Music duck = " + PausedMusicDuckDb + " dB)\n\n" +
                    "A mirror was also written to:\n" + ResourcesMirrorPath + "\n" +
                    "so MixerSnapshotController.LoadMixer() can find it at runtime via " +
                    "Resources.Load(\"Audio/Mixers/EchohavenMaster\").\n\n" +
                    "NOTE: " + status,
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "EchohavenMaster Mixer --- Manual Author Required",
                    "Automatic mixer authoring failed:\n\n" + status + "\n\n" +
                    "Cowork: create the asset manually --\n" +
                    "  1. Right-click Assets/_Project/Audio/Mixers/ -> Create -> Audio Mixer\n" +
                    "  2. Name it: EchohavenMaster\n" +
                    "  3. Add child groups under Master: Music, SFX, Ambient, UI\n" +
                    "  4. Add snapshot: Paused (duplicate Normal; set Music group volume to " +
                    PausedMusicDuckDb + " dB)\n" +
                    "  5. Copy the asset to: " + ResourcesMirrorPath + "\n" +
                    "     (so the runtime Resources.Load can find it)\n\n" +
                    "Then re-run this menu item to verify, or just move on --- " +
                    "MixerSnapshotController.cs null-guards a missing mixer.",
                    "OK");
            }
        }

        static void EnsureDir(string dir)
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        // === Build via reflection over UnityEditor.Audio.AudioMixerController ===

        static bool TryBuildMixer(out string status)
        {
            status = "";
            try
            {
                Assembly editorAsm = typeof(EditorWindow).Assembly; // UnityEditor.dll / UnityEditor.CoreModule
                Type ctrlType = editorAsm.GetType("UnityEditor.Audio.AudioMixerController");
                if (ctrlType == null)
                {
                    // Try alternate assembly probe
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        ctrlType = asm.GetType("UnityEditor.Audio.AudioMixerController");
                        if (ctrlType != null) break;
                    }
                }
                if (ctrlType == null)
                {
                    status = "UnityEditor.Audio.AudioMixerController type not found via reflection.";
                    return false;
                }

                MethodInfo createAtPath = ctrlType.GetMethod(
                    "CreateMixerControllerAtPath",
                    BindingFlags.Public | BindingFlags.Static,
                    null, new[] { typeof(string) }, null);
                if (createAtPath == null)
                {
                    status = "CreateMixerControllerAtPath(string) not found on AudioMixerController.";
                    return false;
                }

                // Delete existing so we get a clean asset
                if (File.Exists(OutputPath))
                {
                    AssetDatabase.DeleteAsset(OutputPath);
                }

                object mixerCtrl = createAtPath.Invoke(null, new object[] { OutputPath });
                if (mixerCtrl == null)
                {
                    status = "CreateMixerControllerAtPath returned null.";
                    return false;
                }

                // Resolve master group (parent of any new groups we add)
                object masterGroup = GetMasterGroup(ctrlType, mixerCtrl);
                if (masterGroup == null)
                {
                    status = "Created mixer, but could not resolve Master group via reflection. Groups not added.";
                    return false;
                }

                // Add Music / SFX / Ambient / UI as children of Master
                MethodInfo createNewGroup = ctrlType.GetMethod(
                    "CreateNewGroup",
                    BindingFlags.Public | BindingFlags.Instance,
                    null, new[] { typeof(string), typeof(bool) }, null);
                MethodInfo addChildToParent = ctrlType.GetMethod(
                    "AddChildToParent",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo addGroupToCurrentView = ctrlType.GetMethod(
                    "AddGroupToCurrentView",
                    BindingFlags.Public | BindingFlags.Instance);

                if (createNewGroup == null || addChildToParent == null)
                {
                    status = "CreateNewGroup or AddChildToParent reflection missing --- groups not authored.";
                    return false;
                }

                object musicGroup = null;
                foreach (var name in GroupNames)
                {
                    object g = createNewGroup.Invoke(mixerCtrl, new object[] { name, false });
                    if (g == null) continue;
                    addChildToParent.Invoke(mixerCtrl, new object[] { g, masterGroup });
                    if (addGroupToCurrentView != null)
                    {
                        try { addGroupToCurrentView.Invoke(mixerCtrl, new object[] { g }); }
                        catch { /* non-fatal */ }
                    }
                    if (name == "Music") musicGroup = g;
                }

                // Snapshots: Normal already exists (rename default); add Paused
                bool snapshotsOk = TryCreateSnapshots(ctrlType, mixerCtrl, musicGroup, out string snapStatus);
                if (!snapshotsOk)
                {
                    status = "Mixer + groups created. Snapshot authoring partial: " + snapStatus;
                    // partial success --- still write the asset so Cowork can finish
                    AssetDatabase.SaveAssets();
                    return false;
                }

                AssetDatabase.SaveAssets();
                status = "Reflection authoring succeeded.";
                return true;
            }
            catch (Exception ex)
            {
                status = "Exception during reflection authoring: " + ex.GetType().Name + ": " + ex.Message;
                return false;
            }
        }

        static object GetMasterGroup(Type ctrlType, object mixerCtrl)
        {
            try
            {
                PropertyInfo masterProp = ctrlType.GetProperty("masterGroup",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (masterProp != null) return masterProp.GetValue(mixerCtrl);

                FieldInfo masterField = ctrlType.GetField("m_MasterGroup",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (masterField != null) return masterField.GetValue(mixerCtrl);
            }
            catch
            {
            }
            return null;
        }

        static bool TryCreateSnapshots(Type ctrlType, object mixerCtrl, object musicGroup, out string status)
        {
            status = "";
            try
            {
                // Try to rename the default snapshot to "Normal"
                PropertyInfo snapshotsProp = ctrlType.GetProperty("snapshots",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (snapshotsProp != null)
                {
                    Array existing = snapshotsProp.GetValue(mixerCtrl) as Array;
                    if (existing != null && existing.Length > 0)
                    {
                        object defaultSnap = existing.GetValue(0);
                        if (defaultSnap is UnityEngine.Object uo) uo.name = NormalSnapshot;
                    }
                }

                MethodInfo createSnap = ctrlType.GetMethod("CreateNewSnapshot",
                    BindingFlags.Public | BindingFlags.Instance,
                    null, new[] { typeof(string), typeof(bool) }, null);
                if (createSnap == null)
                {
                    status = "CreateNewSnapshot(string,bool) not found.";
                    return false;
                }
                object pausedSnap = createSnap.Invoke(mixerCtrl, new object[] { PausedSnapshot, false });
                if (pausedSnap == null)
                {
                    status = "CreateNewSnapshot returned null.";
                    return false;
                }

                // Try to set Music group volume to -20 dB in the Paused snapshot.
                // This is the BRITTLEST step --- if it fails, the snapshot will simply
                // mirror Normal (still useful for testing transitions; Cowork sets the duck).
                if (musicGroup != null)
                {
                    if (!TrySetGroupVolumeInSnapshot(mixerCtrl, ctrlType, musicGroup, pausedSnap, PausedMusicDuckDb, out string volStatus))
                    {
                        status = "Snapshots created. Music duck (" + PausedMusicDuckDb + " dB in Paused) skipped: " + volStatus;
                        return true; // still consider snapshots authored
                    }
                }

                status = "Snapshots authored cleanly.";
                return true;
            }
            catch (Exception ex)
            {
                status = "Exception: " + ex.Message;
                return false;
            }
        }

        static bool TrySetGroupVolumeInSnapshot(object mixerCtrl, Type ctrlType, object group, object snapshot,
            float db, out string status)
        {
            status = "";
            try
            {
                // AudioMixerGroupController exposes `GetGUIDForVolume()` returning a GUID;
                // AudioMixerController has `SetValueForVolume(snapshot, guid, value)` (internal).
                Type groupType = group.GetType();
                MethodInfo getGuidForVolume = groupType.GetMethod("GetGUIDForVolume",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (getGuidForVolume == null)
                {
                    status = "GetGUIDForVolume not found on group type.";
                    return false;
                }
                object guid = getGuidForVolume.Invoke(group, null);

                MethodInfo setValue = ctrlType.GetMethod("SetValueForVolume",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (setValue == null)
                {
                    // alternate naming
                    setValue = ctrlType.GetMethod("SetValueForVol",
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                }
                if (setValue == null)
                {
                    status = "SetValueForVolume not found on AudioMixerController.";
                    return false;
                }

                // Common signature: SetValueForVolume(AudioMixerSnapshotController snapshot, GUID guid, float val)
                var pars = setValue.GetParameters();
                object[] args;
                if (pars.Length == 3)
                {
                    args = new object[] { snapshot, guid, db };
                }
                else
                {
                    status = "SetValueForVolume signature unexpected (params=" + pars.Length + ").";
                    return false;
                }
                setValue.Invoke(mixerCtrl, args);
                status = "set " + db + " dB";
                return true;
            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }
        }

        static void MirrorIntoResources()
        {
            try
            {
                if (!File.Exists(OutputPath)) return;
                EnsureDir(ResourcesMirrorDir);
                if (File.Exists(ResourcesMirrorPath))
                {
                    AssetDatabase.DeleteAsset(ResourcesMirrorPath);
                }
                AssetDatabase.CopyAsset(OutputPath, ResourcesMirrorPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[MixerAssetBuilder] Resources mirror copy failed: " + ex.Message);
            }
        }
    }
}
#endif
