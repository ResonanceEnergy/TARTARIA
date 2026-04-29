using System.IO;
using System.Reflection;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.VFX;

namespace Tartaria.Editor
{
    /// <summary>
    /// Wires Moon 1 dome-awakening VFX Graph into the scene VFXController.
    /// Expects a VFX Graph at Assets/_Project/VFX/Graphs/DomeAwakeningBurst.vfx.
    /// </summary>
    public static class Moon1VFXGraphSetup
    {
        const string EchohavenScenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        const string GraphDir = "Assets/_Project/VFX/Graphs";
        const string DomeBurstGraphPath = GraphDir + "/DomeAwakeningBurst.vfx";

        [MenuItem("Tartaria/Setup/Moon 1 Dome VFX Graph Wiring", false, 62)]
        public static void WireMoon1DomeVFXGraph()
        {
            EnsureDirectory("Assets/_Project/VFX");
            EnsureDirectory(GraphDir);
            EnsureDomeBurstGraphAsset();

            if (!File.Exists(EchohavenScenePath))
            {
                Debug.LogWarning($"[Tartaria] Moon 1 VFX setup skipped: scene not found at {EchohavenScenePath}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(EchohavenScenePath, OpenSceneMode.Single);
            var controller = UnityEngine.Object.FindFirstObjectByType<Tartaria.Integration.VFXController>();
            if (controller == null)
            {
                var managers = GameObject.Find("--- GAME MANAGERS ---");
                var go = new GameObject("VFXController");
                if (managers != null)
                    go.transform.SetParent(managers.transform);
                controller = go.AddComponent<Tartaria.Integration.VFXController>();
            }

            var graph = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(DomeBurstGraphPath);
            var so = new SerializedObject(controller);
            var graphProp = so.FindProperty("domeAwakeningBurstGraph");
            var poolProp = so.FindProperty("domeAwakeningBurstPoolSize");

            if (graphProp != null)
                graphProp.objectReferenceValue = graph;
            if (poolProp != null)
                poolProp.intValue = 2;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();

            if (graph == null)
            {
                Debug.LogWarning("[Tartaria] DomeAwakeningBurst.vfx is still missing after auto-create attempt.");
            }
            else
            {
                Debug.Log("[Tartaria] Moon 1 dome-awakening VFX Graph wired into VFXController.");
            }
        }

        static void EnsureDomeBurstGraphAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(DomeBurstGraphPath) != null)
                return;

            var utilType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType("UnityEditor.VisualEffectAssetEditorUtility", false))
                .FirstOrDefault(t => t != null);
            try
            {
                if (utilType != null)
                {
                    var createMethod = utilType.GetMethod("CreateNewAsset", BindingFlags.Public | BindingFlags.Static);
                    if (createMethod != null)
                    {
                        createMethod.Invoke(null, new object[] { DomeBurstGraphPath });
                        AssetDatabase.ImportAsset(DomeBurstGraphPath);
                        AssetDatabase.SaveAssets();
                        Debug.Log("[Tartaria] Auto-created VFX Graph asset via utility: Assets/_Project/VFX/Graphs/DomeAwakeningBurst.vfx");
                        return;
                    }
                }

                const string emptyVfxYaml =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &114350483966674976
MonoBehaviour:
  m_Script: {fileID: 11500000, guid: 7d4c867f6b72b714dbb5fd1780afe208, type: 3}
--- !u!2058629511 &1
VisualEffectResource:
  m_Graph: {fileID: 114350483966674976}
";

                File.WriteAllText(DomeBurstGraphPath, emptyVfxYaml);
                AssetDatabase.ImportAsset(DomeBurstGraphPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[Tartaria] Auto-created VFX Graph asset via YAML fallback: Assets/_Project/VFX/Graphs/DomeAwakeningBurst.vfx");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Tartaria] Failed to auto-create DomeAwakeningBurst.vfx: {ex.Message}");
            }
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
