#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Tests
{
    /// <summary>
    /// Bootstrap_AICombat --- isolated arena for MudGolem aggro / damage /
    /// death validation. Spawns 3 golem placeholders (cube primitives tagged
    /// with MudGolemHealth) around a center pad. Per HANDOFFS 2026-06-01 22:30
    /// → QA Lead (test-scenes-mvp).
    ///
    /// Menu: <c>Tartaria → 9 QA → Open Test_AICombat</c>
    ///
    /// MudGolemHealth resolves its own renderer/material via the shared
    /// MaterialBank, so primitives are sufficient targets.
    /// </summary>
    public static class Bootstrap_AICombat
    {
        [MenuItem("Tartaria/9 QA/Open Test_AICombat", false, 92)]
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
            ground.name = "Arena";
            ground.transform.localScale = new Vector3(5f, 1f, 5f);

            var center = new GameObject("CombatCenter");
            center.transform.position = Vector3.zero;

            for (int i = 0; i < 3; i++)
            {
                float ang = i * Mathf.PI * 2f / 3f;
                var golem = GameObject.CreatePrimitive(PrimitiveType.Cube);
                golem.name = $"MudGolem_{i}";
                golem.transform.position = new Vector3(Mathf.Cos(ang) * 6f, 1f, Mathf.Sin(ang) * 6f);
                // MudGolemHealth lives in Tartaria.AI --- attach by type name via reflection so
                // this bootstrap stays decoupled from that asmdef reference.
                var t = System.Type.GetType("Tartaria.AI.MudGolemHealth, Tartaria.AI");
                if (t != null) golem.AddComponent(t);
                else Debug.LogWarning("[Bootstrap_AICombat] Tartaria.AI.MudGolemHealth not found --- attach manually.");
            }

            var spawn = new GameObject("PlayerSpawnMarker");
            spawn.transform.position = new Vector3(0f, 2f, 0f);

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[Bootstrap_AICombat] Arena ready. Hit Play to engage 3 mud golems.");
        }
    }
}
#endif
