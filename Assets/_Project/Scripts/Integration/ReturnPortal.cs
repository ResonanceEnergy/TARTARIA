using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Day-3: spawned at the center of a moon when its mechanic is cleared.
    /// Walking into it warps the player back to Echohaven (the hub).
    /// </summary>
    [DisallowMultipleComponent]
    public class ReturnPortal : MonoBehaviour
    {
        const string EchohavenScene = "Echohaven_VerticalSlice";

        public static ReturnPortal SpawnAt(Vector3 pos)
        {
            // Single portal per scene
            var existing = FindFirstObjectByType<ReturnPortal>();
            if (existing != null) return existing;

            var go = new GameObject("ReturnPortal_Echohaven");
            go.transform.position = pos;

            // Visual: tall glowing cyan cylinder
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "PortalPillar";
            pillar.transform.SetParent(go.transform, false);
            pillar.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            pillar.transform.localScale = new Vector3(0.6f, 2f, 0.6f);
            Object.Destroy(pillar.GetComponent<CapsuleCollider>());
            var renderer = pillar.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            var cyan = new Color(0.25f, 0.85f, 1f);
            mat.color = cyan;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", cyan * 4f);
            renderer.sharedMaterial = mat;

            var light = pillar.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = cyan;
            light.intensity = 6f;
            light.range = 12f;

            // Trigger zone
            var trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 2.5f;
            trigger.center = new Vector3(0f, 1.5f, 0f);

            var portal = go.AddComponent<ReturnPortal>();
            HUDController.Instance?.ShowObjective("<b>↪ Return Portal active</b>  Walk into the cyan beam to leave.");
            return portal;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Debug.Log("[ReturnPortal] Player entered — warping to Echohaven.");
            HUDController.Instance?.ShowObjective("<b>↪ Returning to Echohaven...</b>");
            try { SceneManager.LoadScene(EchohavenScene); }
            catch (System.Exception e) { Debug.LogWarning($"[ReturnPortal] Load failed: {e.Message}"); }
        }
    }
}
