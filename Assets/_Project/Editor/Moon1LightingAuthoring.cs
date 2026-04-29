using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 1 (Echohaven) APV authoring helper:
    /// - Ensures URP/APV settings are enabled.
    /// - Creates a global APV volume and a StarDome local refinement volume.
    /// - Ensures a ProbeVolumeBakingSet with dawn/awakening scenarios exists.
    /// - Optionally kicks an async bake.
    /// </summary>
    public static class Moon1LightingAuthoring
    {
        const string EchohavenScenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        const string ApvConfigDir = "Assets/_Project/Config/APV";
        const string Moon1BakingSetPath = ApvConfigDir + "/Moon1_Echohaven_APV_BakingSet.asset";

        [MenuItem("Tartaria/Setup/Moon 1 APV + Scenarios", false, 60)]
        public static void SetupMoon1APV()
        {
            if (!File.Exists(EchohavenScenePath))
            {
                Debug.LogWarning($"[Tartaria] Moon 1 APV setup skipped: scene not found at {EchohavenScenePath}");
                return;
            }

            URPSetup.UpgradeURPQuality();
            EnsureDirectory("Assets/_Project/Config");
            EnsureDirectory(ApvConfigDir);

            var scene = EditorSceneManager.OpenScene(EchohavenScenePath, OpenSceneMode.Single);
            var sceneGuid = AssetDatabase.AssetPathToGUID(EchohavenScenePath);

            var globalVolume = EnsureGlobalProbeVolume();
            EnsureStarDomeLocalProbeVolume();
            EnsureScenarioController();

            var bakingSet = EnsureBakingSet(sceneGuid);
            EnsurePerSceneAPVData(scene, sceneGuid, bakingSet);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();

            Debug.Log("[Tartaria] Moon 1 APV setup complete. Scenarios ready: Dawn_PreAwakening, Dome_Awakening.");
        }

        [MenuItem("Tartaria/Setup/Moon 1 APV + Scenarios and Bake", false, 61)]
        public static void SetupMoon1APVAndBake()
        {
            SetupMoon1APV();

            // Run a blocking bake so this method is safe for batchmode automation.
            bool bakeOk = Lightmapping.Bake();
            Debug.Log(bakeOk
                ? "[Tartaria] Moon 1 GI/APV bake completed."
                : "[Tartaria] Moon 1 GI/APV bake did not complete successfully.");
        }

        static ProbeVolume EnsureGlobalProbeVolume()
        {
            var go = GameObject.Find("APV_Global_Echohaven");
            if (go == null)
                go = new GameObject("APV_Global_Echohaven");

            var pv = go.GetComponent<ProbeVolume>();
            if (pv == null)
                pv = go.AddComponent<ProbeVolume>();

            pv.mode = ProbeVolume.Mode.Global;
            pv.fillEmptySpaces = true;
            return pv;
        }

        static void EnsureScenarioController()
        {
            var go = GameObject.Find("APVScenarioController");
            if (go == null)
                go = new GameObject("APVScenarioController");

            if (go.GetComponent<Tartaria.Integration.APVScenarioController>() == null)
                go.AddComponent<Tartaria.Integration.APVScenarioController>();
        }

        static ProbeVolume EnsureStarDomeLocalProbeVolume()
        {
            var go = GameObject.Find("APV_Local_StarDome");
            if (go == null)
                go = new GameObject("APV_Local_StarDome");

            var pv = go.GetComponent<ProbeVolume>();
            if (pv == null)
                pv = go.AddComponent<ProbeVolume>();

            pv.mode = ProbeVolume.Mode.Local;
            pv.size = new Vector3(56f, 36f, 56f);

            // Align with the StarDome marker if present in Echohaven.
            var starDome = GameObject.Find("Echohaven_StarDome");
            if (starDome != null)
                go.transform.position = starDome.transform.position;
            else
                go.transform.position = new Vector3(30f, 10f, 20f);

            return pv;
        }

        static ProbeVolumeBakingSet EnsureBakingSet(string sceneGuid)
        {
            var set = AssetDatabase.LoadAssetAtPath<ProbeVolumeBakingSet>(Moon1BakingSetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<ProbeVolumeBakingSet>();
                set.name = "Moon1_Echohaven_APV_BakingSet";
                AssetDatabase.CreateAsset(set, Moon1BakingSetPath);
            }

            set.TryAddScene(sceneGuid);
            set.TryAddScenario("Dawn_PreAwakening");
            set.TryAddScenario("Dome_Awakening");

            EditorUtility.SetDirty(set);
            return set;
        }

        static void EnsurePerSceneAPVData(Scene scene, string sceneGuid, ProbeVolumeBakingSet bakingSet)
        {
            ProbeVolumePerSceneData perScene = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                perScene = root.GetComponentInChildren<ProbeVolumePerSceneData>(true);
                if (perScene != null)
                    break;
            }

            if (perScene == null)
            {
                var go = new GameObject("ProbeVolumePerSceneData");
                SceneManager.MoveGameObjectToScene(go, scene);
                perScene = go.AddComponent<ProbeVolumePerSceneData>();
            }

            var so = new SerializedObject(perScene);
            var sceneGuidProp = so.FindProperty("sceneGUID");
            var setProp = so.FindProperty("serializedBakingSet");
            if (sceneGuidProp != null)
                sceneGuidProp.stringValue = sceneGuid;
            if (setProp != null)
                setProp.objectReferenceValue = bakingSet;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(perScene);
        }

        static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            var folder = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folder))
                AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
