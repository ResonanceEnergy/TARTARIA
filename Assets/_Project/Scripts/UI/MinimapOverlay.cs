using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.UI
{
    /// <summary>
    /// Top-right minimap + objective marker overlay (IMGUI).
    /// • Renders nearby enemies / pickups / companions as colored dots
    ///   on a circular radar around the player.
    /// • Draws the active objective text + an off-screen waypoint arrow if
    ///   `SetWaypoint(Vector3)` has been called.
    /// </summary>
    [DisallowMultipleComponent]
    public class MinimapOverlay : MonoBehaviour
    {
        static MinimapOverlay _instance;
        public static MinimapOverlay Instance => _instance;

        const float Range = 35f;
        const int Size = 160;

        Vector3? _waypoint;
        string _waypointLabel;
        Texture2D _ringTex;
        Texture2D _pixel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("MinimapOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<MinimapOverlay>();
        }

        public static void SetWaypoint(Vector3 worldPos, string label = "")
        {
            if (_instance == null) Bootstrap();
            _instance!._waypoint = worldPos;
            _instance._waypointLabel = label;
        }

        public static void ClearWaypoint()
        {
            if (_instance != null) { _instance._waypoint = null; _instance._waypointLabel = null; }
        }

        void Awake()
        {
            _pixel = new Texture2D(1, 1);
            _pixel.SetPixel(0, 0, Color.white); _pixel.Apply();
        }

        Transform FindPlayer()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            return p != null ? p.transform : null;
        }

        void OnGUI()
        {
            var player = FindPlayer();
            if (player == null) return;

            int x = Screen.width - Size - 24;
            int y = 24;
            var rect = new Rect(x, y, Size, Size);

            // Backing
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            // Border ring
            GUI.color = new Color(1f, 0.92f, 0.5f, 0.8f);
            DrawRing(x + Size / 2, y + Size / 2, Size / 2 - 2, 2);

            GUI.color = prev;

            // Player dot
            var center = new Vector2(x + Size / 2f, y + Size / 2f);
            DrawDot(center, 4f, new Color(1f, 0.95f, 0.6f));

            // Forward arrow (3px line in front of player dot)
            var forward = new Vector2(player.forward.x, player.forward.z);
            forward.Normalize();
            DrawDot(center + forward * 8f, 2.5f, new Color(1f, 0.95f, 0.6f, 0.9f));

            // Plot world objects within Range.
            PlotByTag("Enemy",        new Color(1f, 0.35f, 0.35f), player, center);
            PlotByName("Loot_",       new Color(0.4f, 1f, 0.6f),  player, center);
            PlotByName("Companion_",  new Color(0.5f, 0.85f, 1f), player, center);
            PlotByName("ReturnPortal",new Color(1f, 0.7f, 1f),    player, center, 1, sizeMul: 1.6f);
            PlotByName("Boss",        new Color(1f, 0.2f, 0.6f),  player, center, 1, sizeMul: 2f);

            // Waypoint
            if (_waypoint.HasValue)
            {
                Vector2 wp = WorldToMinimap(_waypoint.Value, player, center, out bool inside);
                var wcol = inside ? new Color(0.4f, 1f, 1f) : new Color(1f, 0.85f, 0.2f);
                DrawDot(wp, inside ? 4f : 6f, wcol);
                if (!string.IsNullOrEmpty(_waypointLabel))
                {
                    var s = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleCenter, normal = { textColor = wcol } };
                    GUI.Label(new Rect(x, y + Size + 2, Size, 16), _waypointLabel, s);
                }
            }

            // Compass label
            var nstyle = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.UpperCenter, normal = { textColor = new Color(1f, 0.92f, 0.5f) } };
            GUI.Label(new Rect(x, y - 2, Size, 14), "N", nstyle);
        }

        void PlotByTag(string tag, Color col, Transform player, Vector2 center, int max = 24, float sizeMul = 1f)
        {
            GameObject[] arr;
            try { arr = GameObject.FindGameObjectsWithTag(tag); }
            catch { return; }
            int n = 0;
            foreach (var go in arr)
            {
                if (n++ >= max) break;
                if (go == null) continue;
                var p = WorldToMinimap(go.transform.position, player, center, out bool inside);
                if (!inside) continue;
                DrawDot(p, 3f * sizeMul, col);
            }
        }

        void PlotByName(string startsWith, Color col, Transform player, Vector2 center, int max = 24, float sizeMul = 1f)
        {
            int n = 0;
            // Limit the brute-force scan to nearby objects via OverlapSphere.
            var hits = Physics.OverlapSphere(player.position, Range, ~0, QueryTriggerInteraction.Collide);
            foreach (var h in hits)
            {
                if (n >= max) break;
                if (h == null || h.transform == null) continue;
                var name = h.transform.root.name;
                if (string.IsNullOrEmpty(name) || !name.StartsWith(startsWith)) continue;
                var p = WorldToMinimap(h.transform.position, player, center, out bool inside);
                if (!inside) continue;
                DrawDot(p, 3f * sizeMul, col);
                n++;
            }
        }

        Vector2 WorldToMinimap(Vector3 worldPos, Transform player, Vector2 center, out bool inside)
        {
            Vector3 d = worldPos - player.position;
            // Top-down: x→x, z→y; rotate by -player yaw so up is forward.
            float yaw = -player.eulerAngles.y * Mathf.Deg2Rad;
            float cs = Mathf.Cos(yaw), sn = Mathf.Sin(yaw);
            Vector2 r = new Vector2(d.x * cs - d.z * sn, d.x * sn + d.z * cs);
            float scale = (Size / 2f - 6f) / Range;
            Vector2 pix = center + new Vector2(r.x, -r.y) * scale;
            inside = r.sqrMagnitude <= Range * Range;
            // Clamp to ring edge for off-screen markers
            if (!inside)
            {
                Vector2 fromCenter = pix - center;
                fromCenter = fromCenter.normalized * (Size / 2f - 6f);
                pix = center + fromCenter;
            }
            return pix;
        }

        void DrawDot(Vector2 p, float size, Color c)
        {
            var prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(new Rect(p.x - size, p.y - size, size * 2f, size * 2f), _pixel);
            GUI.color = prev;
        }

        void DrawRing(int cx, int cy, int radius, int thickness)
        {
            const int Segments = 36;
            for (int i = 0; i < Segments; i++)
            {
                float a = i / (float)Segments * Mathf.PI * 2f;
                float px = cx + Mathf.Cos(a) * radius;
                float py = cy + Mathf.Sin(a) * radius;
                GUI.DrawTexture(new Rect(px - thickness, py - thickness, thickness * 2, thickness * 2), _pixel);
            }
        }
    }
}
