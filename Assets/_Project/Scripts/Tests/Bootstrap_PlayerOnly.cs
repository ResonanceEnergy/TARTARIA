#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Tests
{
    /// <summary>
    /// Bootstrap_PlayerOnly --- isolated micro-scene that builds: directional
    /// light + ground plane + spawn marker. Validates locomotion + camera in
    /// the absence of every other moon system. Per HANDOFFS 2026-06-01 22:30
    /// → QA Lead (test-scenes-mvp).
    ///
    /// Menu: <c>Tartaria → 9 QA → Open Test_PlayerOnly</c>
    ///
    /// Why programmatic vs checked-in .unity:
    /// .unity files are binary YAML with internal GUIDs; generating them
    /// agent-side risks drift. Bootstrap creates a fresh untitled scene and
    /// populates it deterministically — Cowork can Save-As if they want it
    /// checked in.
    /// </summary>
    public static class Bootstrap_PlayerOnly
    {
        [MenuItem("Tartaria/9 QA/Open Test_PlayerOnly", false, 91)]
        public static void Open()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var light = new GameObject("Sun");
            var l = light.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(45f, 30f, 0f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(10f, 1f, 10f);

            var spawn = new GameObject("PlayerSpawnMarker");
            spawn.transform.position = new Vector3(0f, 2f, 15f);

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[Bootstrap_PlayerOnly] Test scene ready. Hit Play --- Moon1PlayerSetup auto-bootstrap will spawn the player at marker.");
        }
    }
}
#endif
