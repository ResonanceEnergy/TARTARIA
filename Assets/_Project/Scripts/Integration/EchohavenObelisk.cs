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

            // Tall obsidian shaft with glowing top
            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shaft.name = "Shaft";
            shaft.transform.SetParent(go.transform, false);
            shaft.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            shaft.transform.localScale = new Vector3(0.7f, 5f, 0.7f);
            var smat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            smat.color = new Color(0.08f, 0.08f, 0.12f);
            shaft.GetComponent<MeshRenderer>().sharedMaterial = smat;

            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Crown";
            crown.transform.SetParent(go.transform, false);
            crown.transform.localPosition = new Vector3(0f, 5.4f, 0f);
            crown.transform.localScale = Vector3.one * 0.9f;
            Object.Destroy(crown.GetComponent<SphereCollider>());
            var cmat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            var gold = new Color(1f, 0.85f, 0.4f);
            cmat.color = gold;
            cmat.EnableKeyword("_EMISSION");
            cmat.SetColor("_EmissionColor", gold * 3f);
            crown.GetComponent<MeshRenderer>().sharedMaterial = cmat;

            var light = crown.AddComponent<Light>();
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
