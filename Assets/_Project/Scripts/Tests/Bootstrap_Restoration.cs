#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Tests
{
    /// <summary>
    /// Bootstrap_Restoration --- isolated scene for tuning-pedestal /
    /// InteractableBuilding flow. Spawns fountain/dome/spire placeholder
    /// cylinders and lets Cowork stand on the pedestal triggers. Per
    /// HANDOFFS 2026-06-01 22:30 → QA Lead (test-scenes-mvp).
    ///
    /// Menu: <c>Tartaria → 9 QA → Open Test_Restoration</c>
    /// </summary>
    public static class Bootstrap_Restoration
    {
        static readonly string[] Buildings = { "fountain", "dome", "spire" };

        [MenuItem("Tartaria/9 QA/Open Test_Restoration", false, 93)]
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
            ground.name = "Plaza";
            ground.transform.localScale = new Vector3(8f, 1f, 8f);

            var interactableType = System.Type.GetType("Tartaria.Integration.InteractableBuilding, Tartaria.Integration");

            for (int i = 0; i < Buildings.Length; i++)
            {
                float ang = i * Mathf.PI * 2f / Buildings.Length;
                var bld = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bld.name = $"Building_{Buildings[i]}";
                bld.transform.position = new Vector3(Mathf.Cos(ang) * 10f, 1f, Mathf.Sin(ang) * 10f);
                bld.transform.localScale = new Vector3(3f, 3f, 3f);

                if (interactableType != null)
                {
                    var comp = bld.AddComponent(interactableType);
                    var setId = interactableType.GetMethod("SetBuildingId");
                    setId?.Invoke(comp, new object[] { Buildings[i] });
                }
                else
                {
                    Debug.LogWarning("[Bootstrap_Restoration] InteractableBuilding type not found --- attach + assign id manually.");
                }
            }

            var spawn = new GameObject("PlayerSpawnMarker");
            spawn.transform.position = new Vector3(0f, 2f, 0f);

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[Bootstrap_Restoration] Plaza ready with 3 placeholder buildings (fountain/dome/spire). Hit Play to test interaction prompts + tuning flow.");
        }
    }
}
#endif
