using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Day-3: Echohaven progression obelisk. Auto-spawns at the hub center.
    /// Walking up to it shows a list of cleared moons + remaining moons.
    /// Press [E] near it to open a quick-warp menu (uses MoonPortalSelector hotkeys F1..F12).
    /// </summary>
    [DisallowMultipleComponent]
    public class EchohavenObelisk : MonoBehaviour
    {
        const string EchohavenScene = "Echohaven_VerticalSlice";
        static readonly Vector3 SpawnOffset = new Vector3(8f, 0f, 8f);

        Transform _player;
        bool _playerNear;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            // Only spawn in Echohaven hub
            if (SceneManager.GetActiveScene().name != EchohavenScene) return;
            if (FindFirstObjectByType<EchohavenObelisk>() != null) return;
            SpawnAtPlayer();
        }

        static void SpawnAtPlayer()
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            Vector3 pos = playerGO != null
                ? playerGO.transform.position + SpawnOffset
                : SpawnOffset;
            pos.y = 0f;

            var go = new GameObject("EchohavenObelisk");
            go.transform.position = pos;

            // Multi-part obelisk structure (no single primitives)
            var smat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            smat.color = new Color(0.08f, 0.08f, 0.12f);

            // Base pedestal
            var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseObj.name = "Base";
            baseObj.transform.SetParent(go.transform, false);
            baseObj.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            baseObj.transform.localScale = new Vector3(1.2f, 0.6f, 1.2f);
            baseObj.GetComponent<MeshRenderer>().sharedMaterial = smat;

            // Lower shaft segment
            var shaftLower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shaftLower.name = "ShaftLower";
            shaftLower.transform.SetParent(go.transform, false);
            shaftLower.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            shaftLower.transform.localScale = new Vector3(0.8f, 2f, 0.8f);
            shaftLower.GetComponent<MeshRenderer>().sharedMaterial = smat;

            // Upper shaft segment
            var shaftUpper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shaftUpper.name = "ShaftUpper";
            shaftUpper.transform.SetParent(go.transform, false);
            shaftUpper.transform.localPosition = new Vector3(0f, 3.5f, 0f);
            shaftUpper.transform.localScale = new Vector3(0.7f, 3f, 0.7f);
            shaftUpper.GetComponent<MeshRenderer>().sharedMaterial = smat;

            // Crown ring (torus-like) - VFX replacement
            GameObject crownRingVFX = new GameObject("CrownRing_VFX");
            crownRingVFX.transform.SetParent(go.transform, false);
            crownRingVFX.transform.localPosition = new Vector3(0f, 5.2f, 0f);
            
            ParticleSystem psRing = crownRingVFX.AddComponent<ParticleSystem>();
            var mainRing = psRing.main;
            mainRing.startLifetime = 2.5f;
            mainRing.startSpeed = 0.2f;
            mainRing.startSize = 0.3f;
            mainRing.startColor = new Color(1f, 0.85f, 0.4f, 0.9f);
            mainRing.maxParticles = 100;
            mainRing.loop = true;
            
            var emissionRing = psRing.emission;
            emissionRing.rateOverTime = 40f;
            
            var shapeRing = psRing.shape;
            shapeRing.shapeType = ParticleSystemShapeType.Circle;
            shapeRing.radius = 1.1f;
            
            var rendererRing = crownRingVFX.GetComponent<ParticleSystemRenderer>();
            rendererRing.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            rendererRing.material.SetColor("_BaseColor", new Color(1f, 0.85f, 0.4f));
            rendererRing.material.EnableKeyword("_EMISSION");
            rendererRing.material.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.4f) * 2f);
            
            psRing.Play();

            // Crown orb (glowing) - VFX replacement
            GameObject crownOrbVFX = new GameObject("CrownOrb_VFX");
            crownOrbVFX.transform.SetParent(go.transform, false);
            crownOrbVFX.transform.localPosition = new Vector3(0f, 5.5f, 0f);
            
            ParticleSystem psOrb = crownOrbVFX.AddComponent<ParticleSystem>();
            var mainOrb = psOrb.main;
            mainOrb.startLifetime = 2.0f;
            mainOrb.startSpeed = 0.3f;
            mainOrb.startSize = 0.6f;
            mainOrb.startColor = new Color(1f, 0.85f, 0.4f, 1f);
            mainOrb.maxParticles = 80;
            mainOrb.loop = true;
            
            var emissionOrb = psOrb.emission;
            emissionOrb.rateOverTime = 30f;
            
            var shapeOrb = psOrb.shape;
            shapeOrb.shapeType = ParticleSystemShapeType.Sphere;
            shapeOrb.radius = 0.3f;
            
            var rendererOrb = crownOrbVFX.GetComponent<ParticleSystemRenderer>();
            rendererOrb.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            rendererOrb.material.SetColor("_BaseColor", new Color(1f, 0.85f, 0.4f));
            rendererOrb.material.EnableKeyword("_EMISSION");
            rendererOrb.material.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.4f) * 3f);
            
            psOrb.Play();

            var light = crownOrb.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = gold;
            light.intensity = 4f;
            light.range = 10f;

            // Proximity trigger
            var trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 4f;
            trigger.center = new Vector3(0f, 2f, 0f);

            go.AddComponent<EchohavenObelisk>();
            Debug.Log($"[EchohavenObelisk] Spawned at {pos}");
        }

        void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) _playerNear = true; }
        void OnTriggerExit(Collider other)  { if (other.CompareTag("Player")) _playerNear = false; }

        void OnGUI()
        {
            if (!_playerNear) return;
            const int W = 320, H = 420;
            int x = 12, y = Screen.height - H - 12;
            GUI.Box(new Rect(x, y, W, H), "OBELISK OF ECHOHAVEN");

            var tracker = MoonProgressTracker.Instance;
            int cleared = tracker != null ? tracker.ClearedCount : 0;
            var run = RunProgressTracker.Instance;

            var sb = new StringBuilder();
            sb.AppendLine($"<b>Cleared: {cleared}/{MoonProgressTracker.MoonCount}</b>");
            if (run != null)
            {
                sb.AppendLine($"<b>Total RS: {Mathf.RoundToInt(run.TotalRS)}</b>   <i>session #{run.SessionCount}</i>");
            }
            sb.AppendLine();
            string[] names =
            {
                "Echohaven","Crystalline Caverns","Windswept Highlands","Star Fort Bastion",
                "Sunken Colosseum","Living Library","Clockwork Citadel","Verdant Canopy",
                "Auroral Spire","Deep Forge","Tidal Archive","Celestial Observatory","Planetary Nexus"
            };
            for (int i = 0; i < names.Length; i++)
            {
                bool done = tracker != null && tracker.IsCleared(i + 1);
                string mark = done ? "<color=#7fff7f>[✓]</color>" : "<color=#888888>[ ]</color>";
                sb.AppendLine($"{mark} F{(i < 12 ? (i + 1).ToString() : "`")}  Moon {(i + 1):D2}  {names[i]}");
            }
            sb.AppendLine();
            sb.AppendLine("<i>Press F1..F12 / ` to warp.</i>");

            var style = new GUIStyle(GUI.skin.label) { fontSize = 11, richText = true, wordWrap = true };
            GUI.Label(new Rect(x + 10, y + 22, W - 20, H - 80), sb.ToString(), style);

            // Day-7: Continue / Reset buttons
            int by = y + H - 56;
            var btn = new GUIStyle(GUI.skin.button) { fontSize = 11 };
            if (run != null && !string.IsNullOrEmpty(run.LastScene) && run.LastScene != "Echohaven_VerticalSlice")
            {
                if (GUI.Button(new Rect(x + 10, by, W - 20, 22), $"Continue → {run.LastScene}", btn))
                {
                    try { SceneManager.LoadScene(run.LastScene); }
                    catch (System.Exception e) { Debug.LogWarning($"[Obelisk] Continue failed: {e.Message}"); }
                }
            }
            if (GUI.Button(new Rect(x + 10, by + 26, W - 20, 22), "Reset Progression (Run + Cleared)", btn))
            {
                run?.ResetRun();
                tracker?.ResetAll();
            }
        }
    }
}
