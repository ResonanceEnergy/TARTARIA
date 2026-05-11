#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Drops an EchohavenCombatArena MonoBehaviour into Echohaven_VerticalSlice
    /// so the player gets immediate combat the moment the scene loads.
    /// Idempotent — skips if already present.
    /// </summary>
    public static class EchohavenCombatArenaAttacher
    {
        const string ScenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";

        [MenuItem("TARTARIA/Integration/Attach Echohaven Combat Arena")]
        public static void AttachMenu() => Attach();

        public static void Attach()
        {
            if (!System.IO.File.Exists(ScenePath))
            {
                Debug.LogWarning($"[CombatArenaAttacher] Scene not found: {ScenePath}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var existing = Object.FindFirstObjectByType<EchohavenCombatArena>();
            if (existing == null)
            {
                var go = new GameObject("EchohavenCombatArena");
                go.AddComponent<EchohavenCombatArena>();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, ScenePath);
                Debug.Log("[CombatArenaAttacher] Combat arena attached to Echohaven.");
            }
            else
            {
                Debug.Log("[CombatArenaAttacher] Combat arena already present.");
            }
        }
    }
}
#endif
