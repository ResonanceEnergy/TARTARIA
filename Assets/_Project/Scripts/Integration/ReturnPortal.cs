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

            // Visual: tall glowing cyan cylinder (VFX replacement)
            GameObject pillarVFX = new GameObject("PortalPillar_VFX");
            pillarVFX.transform.SetParent(go.transform, false);
            pillarVFX.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            
            ParticleSystem psPillar = pillarVFX.AddComponent<ParticleSystem>();
            var mainPillar = psPillar.main;
            mainPillar.startLifetime = 3.0f;
            mainPillar.startSpeed = 0.4f;
            mainPillar.startSize = 0.6f;
            mainPillar.startColor = new Color(0.25f, 0.85f, 1f, 0.9f);
            mainPillar.maxParticles = 200;
            mainPillar.loop = true;
            mainPillar.simulationSpace = ParticleSystemSimulationSpace.Local;
            
            var emissionPillar = psPillar.emission;
            emissionPillar.rateOverTime = 70f;
            
            var shapePillar = psPillar.shape;
            shapePillar.shapeType = ParticleSystemShapeType.Cone;
            shapePillar.angle = 3f;
            shapePillar.radius = 0.3f;
            shapePillar.length = 4f;
            
            var rendererPillar = pillarVFX.GetComponent<ParticleSystemRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            var cyan = new Color(0.25f, 0.85f, 1f);
            mat.SetColor("_BaseColor", cyan);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", cyan * 4f);
            rendererPillar.material = mat;
            
            psPillar.Play();

            var light = pillarVFX.AddComponent<Light>();
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
