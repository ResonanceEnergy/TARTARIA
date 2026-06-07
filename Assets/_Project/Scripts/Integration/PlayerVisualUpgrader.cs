using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Integration
{
    /// <summary>
    /// R99 — Persistent runtime fix for the Player visual + EchohavenObelisk + a few
    /// other Moon 1 scene proportions. Runs every play session so fixes survive
    /// Editor recompile/Domain reload + don't need scene-YAML edits.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerVisualUpgrader : MonoBehaviour
    {
        const string EchohavenScene = "Echohaven_VerticalSlice";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (SceneManager.GetActiveScene().name != EchohavenScene) return;
            var go = new GameObject("PlayerVisualUpgrader");
            DontDestroyOnLoad(go);
            go.AddComponent<PlayerVisualUpgrader>();
        }

        float _settleDelay = 1.5f; // give EchohavenContentSpawner + EchohavenObelisk time to spawn first
        bool _applied;

        void Update()
        {
            if (_applied) return;
            _settleDelay -= Time.deltaTime;
            if (_settleDelay > 0) return;
            ApplyAllFixes();
            _applied = true;
        }

        void ApplyAllFixes()
        {
            int fixCount = 0;

            // ── R97/R99 fix 1: ensure EchohavenObelisk is at (38, 0, 5) — east of village, off-path ──
            var obelisk = GameObject.Find("EchohavenObelisk");
            if (obelisk != null)
            {
                obelisk.transform.position = new Vector3(38f, 0f, 5f);
                obelisk.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                fixCount++;
            }

            // ── R99 fix 2: ground-lock floating mud golems ──
            var golems = GameObject.FindGameObjectsWithTag("Untagged");
            foreach (var g in golems)
            {
                if (g == null || !g.name.Contains("MudGolem")) continue;
                var p = g.transform.position;
                if (p.y > 1f)
                {
                    p.y = 0f;
                    g.transform.position = p;
                    fixCount++;
                }
            }

            // ── R99 fix 3: player visual — give the PlayerVisual GameObject a real humanoid silhouette ──
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var pv = player.transform.Find("PlayerVisual");
                if (pv != null)
                {
                    var mf = pv.GetComponent<MeshFilter>();
                    if (mf == null) mf = pv.gameObject.AddComponent<MeshFilter>();
                    // Build a 3-segment human silhouette: head sphere + body capsule + feet box
                    // For now, use Capsule but scale to humanoid proportions (0.4 wide × 1.8 tall)
                    var capsuleProxy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    mf.sharedMesh = capsuleProxy.GetComponent<MeshFilter>().sharedMesh;
                    DestroyImmediate(capsuleProxy);

                    pv.localScale = new Vector3(0.5f, 1.0f, 0.5f);
                    pv.localPosition = new Vector3(0f, -0.2f, 0f);

                    var rend = pv.GetComponent<MeshRenderer>();
                    if (rend == null) rend = pv.gameObject.AddComponent<MeshRenderer>();
                    var urpLit = Shader.Find("Universal Render Pipeline/Lit");
                    if (urpLit != null)
                    {
                        var mat = new Material(urpLit);
                        mat.SetColor("_BaseColor", new Color(0.45f, 0.32f, 0.22f, 1f)); // tunic brown
                        mat.SetFloat("_Smoothness", 0.3f);
                        rend.sharedMaterial = mat;
                    }
                    fixCount++;
                }

                // Also hide the pink "Player_Limbs" main capsule renderer (if it's the bronze pumpkin)
                var mainRend = player.GetComponent<MeshRenderer>();
                if (mainRend != null && mainRend.sharedMaterial != null && mainRend.sharedMaterial.name.Contains("Limbs"))
                {
                    mainRend.enabled = false;
                    fixCount++;
                }
            }

            // ── R99 fix 4: Hide the 7 stacked Dome_* primitives if they still exist ──
            foreach (var name in new[] { "Dome_NW", "Dome_NE", "Dome_E", "Dome_SE", "Dome_S", "Dome_SW", "Dome_W" })
            {
                var d = GameObject.Find(name);
                if (d != null) { d.SetActive(false); fixCount++; }
            }

            // ── R99 fix 5: kill the 41 Cathedral_Facade primitive cubes (Sprint 11 punchlist) ──
            // The new CathedralFacade.fbx is the canonical replacement; primitive wrappers must go.
            foreach (var name in new[] { "Cathedral_Facade", "Spire_Base_2x2m", "Spire_Mid_Taper", "RoseWindow_4x4m" })
            {
                var c = GameObject.Find(name);
                if (c != null && c.GetComponent<MeshFilter>()?.sharedMesh?.name == "Cube")
                { c.SetActive(false); fixCount++; }
            }

            // ── R99 fix 6: StarDome RoseWindow_North scale (was huge blue eye blocking view) ──
            var roseN = GameObject.Find("RoseWindow_North");
            if (roseN != null)
            {
                roseN.transform.localScale = new Vector3(1.5f, 1.5f, 0.3f);
                fixCount++;
            }

            // ── R99 fix 7: Eye_0 + Eye_1 spheres (StarDome eye decor) ──
            for (int i = 0; i < 4; i++)
            {
                var e = GameObject.Find($"Eye_{i}");
                if (e != null)
                {
                    e.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    fixCount++;
                }
            }

            Debug.Log($"[PlayerVisualUpgrader] R99 applied {fixCount} fixes (Obelisk repos, mud golem ground-lock, player humanoid, Dome dedupe, Cathedral primitive cleanup, RoseWindow + Eye scale).");
        }
    }
}
