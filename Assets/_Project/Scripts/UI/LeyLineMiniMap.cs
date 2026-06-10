using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Tartaria.UI
{
    /// <summary>
    /// R347 — Ley line minimap that renders inside an existing UI Canvas
    /// (the previously-empty WorldMapUI GameObject). Shows:
    ///   - Player position dot (animated heading)
    ///   - 3 hero buildings (Dome/Fountain/Spire) as labeled markers
    ///   - 10 LeyLine_* objects as cyan strokes
    ///   - Plaza ring boundary
    /// Toggled by RuntimeHUDBuilder's M key handler via Toggle().
    /// </summary>
    [DisallowMultipleComponent]
    public class LeyLineMiniMap : MonoBehaviour
    {
        public static LeyLineMiniMap Instance { get; private set; }

        RawImage _mapImage;
        Texture2D _mapTex;
        Transform _player;
        Transform[] _heroes;
        Transform[] _leyLines;
        const int MAP_SIZE = 256;
        const float WORLD_RADIUS = 80f; // 80m on each side
        bool _visible;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
            GatherWorldRefs();
            Refresh();
        }

        void BuildUI()
        {
            // Container fills this GameObject which is parented to HUD canvas
            var rt = GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();
            // Top-right corner under the existing compass minimap
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-16f, -270f);
            rt.sizeDelta = new Vector2(220f, 220f);

            // Background frame
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
            var bg = bgGO.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.65f);
            bg.raycastTarget = false;

            // Header
            var hdrGO = new GameObject("Header");
            hdrGO.transform.SetParent(transform, false);
            var hdrRT = hdrGO.AddComponent<RectTransform>();
            hdrRT.anchorMin = new Vector2(0f, 0.88f);
            hdrRT.anchorMax = new Vector2(1f, 1f);
            hdrRT.offsetMin = new Vector2(8f, 0f);
            hdrRT.offsetMax = new Vector2(-8f, -2f);
            var hdr = hdrGO.AddComponent<TextMeshProUGUI>();
            hdr.text = "LEY LINE MAP";
            hdr.fontSize = 14f;
            hdr.fontStyle = FontStyles.Bold;
            hdr.color = new Color(1f, 0.85f, 0.45f); // Aether-Gold
            hdr.alignment = TextAlignmentOptions.Left;
            hdr.raycastTarget = false;

            // Toggle hint
            var hintGO = new GameObject("Hint");
            hintGO.transform.SetParent(transform, false);
            var hintRT = hintGO.AddComponent<RectTransform>();
            hintRT.anchorMin = new Vector2(0f, 0f);
            hintRT.anchorMax = new Vector2(1f, 0.1f);
            hintRT.offsetMin = new Vector2(8f, 4f);
            hintRT.offsetMax = new Vector2(-8f, 0f);
            var hint = hintGO.AddComponent<TextMeshProUGUI>();
            hint.text = "[M] toggle";
            hint.fontSize = 10f;
            hint.color = new Color(0.7f, 0.7f, 0.7f);
            hint.alignment = TextAlignmentOptions.Right;
            hint.raycastTarget = false;

            // Map canvas
            var mapGO = new GameObject("MapImage");
            mapGO.transform.SetParent(transform, false);
            var mRT = mapGO.AddComponent<RectTransform>();
            mRT.anchorMin = new Vector2(0f, 0.1f);
            mRT.anchorMax = new Vector2(1f, 0.88f);
            mRT.offsetMin = new Vector2(8f, 4f);
            mRT.offsetMax = new Vector2(-8f, -4f);
            _mapImage = mapGO.AddComponent<RawImage>();
            _mapImage.raycastTarget = false;

            _mapTex = new Texture2D(MAP_SIZE, MAP_SIZE, TextureFormat.RGBA32, false);
            _mapTex.filterMode = FilterMode.Bilinear;
            _mapImage.texture = _mapTex;
        }

        void GatherWorldRefs()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null) _player = playerGO.transform;

            var heroNames = new[] { "Dome_ListenersHall", "Fountain_ThreadOfMemory", "Spire_FirstNote" };
            var heroList = new List<Transform>();
            foreach (var n in heroNames)
            {
                var go = GameObject.Find(n);
                if (go != null) heroList.Add(go.transform);
            }
            _heroes = heroList.ToArray();

            var leyList = new List<Transform>();
            for (int i = 0; i < 30; i++)
            {
                var go = GameObject.Find($"LeyLine_{i}");
                if (go != null) leyList.Add(go.transform);
            }
            _leyLines = leyList.ToArray();
        }

        public void Toggle()
        {
            _visible = !_visible;
            gameObject.SetActive(_visible);
        }

        void OnEnable() { Refresh(); }

        void Update()
        {
            // Repaint sparingly
            if (Time.frameCount % 30 == 0) Refresh();
        }

        Vector2 WorldToMap(Vector3 worldPos)
        {
            // Project world (x,z) to map (px,py). Plaza center (0,0,0). 80m radius.
            float nx = (worldPos.x + WORLD_RADIUS) / (WORLD_RADIUS * 2f);
            float nz = (worldPos.z + WORLD_RADIUS) / (WORLD_RADIUS * 2f);
            return new Vector2(Mathf.Clamp01(nx) * (MAP_SIZE - 1), Mathf.Clamp01(nz) * (MAP_SIZE - 1));
        }

        void Refresh()
        {
            if (_mapTex == null) return;

            // Clear
            var bg = new Color(0.05f, 0.07f, 0.10f, 0.95f);
            var pixels = new Color[MAP_SIZE * MAP_SIZE];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = bg;
            _mapTex.SetPixels(pixels);

            // Plaza ring outline
            var ringCol = new Color(0.5f, 0.5f, 0.55f, 1f);
            DrawCircle(MAP_SIZE / 2, MAP_SIZE / 2, MAP_SIZE / 2 - 2, ringCol);

            // LeyLines as cyan strokes
            var leyCol = new Color(0.55f, 0.85f, 1f, 1f);
            if (_leyLines != null)
            {
                foreach (var ley in _leyLines)
                {
                    if (ley == null) continue;
                    var p = WorldToMap(ley.position);
                    DrawDot((int)p.x, (int)p.y, 2, leyCol);
                }
            }

            // Hero buildings as gold squares
            var heroCol = new Color(1f, 0.85f, 0.45f, 1f);
            if (_heroes != null)
            {
                foreach (var h in _heroes)
                {
                    if (h == null) continue;
                    var p = WorldToMap(h.position);
                    DrawDot((int)p.x, (int)p.y, 4, heroCol);
                }
            }

            // Player marker — bright cyan
            if (_player != null)
            {
                var pp = WorldToMap(_player.position);
                DrawDot((int)pp.x, (int)pp.y, 3, new Color(1f, 1f, 1f, 1f));
                // Heading indicator
                var heading = _player.forward;
                var headPt = WorldToMap(_player.position + new Vector3(heading.x, 0, heading.z) * 8f);
                DrawLine((int)pp.x, (int)pp.y, (int)headPt.x, (int)headPt.y, new Color(1f, 0.85f, 0.45f, 1f));
            }

            _mapTex.Apply(false);
        }

        void DrawDot(int cx, int cy, int radius, Color col)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx * dx + dy * dy > radius * radius) continue;
                    int x = cx + dx, y = cy + dy;
                    if (x < 0 || x >= MAP_SIZE || y < 0 || y >= MAP_SIZE) continue;
                    _mapTex.SetPixel(x, y, col);
                }
            }
        }

        void DrawCircle(int cx, int cy, int radius, Color col)
        {
            for (int a = 0; a < 360; a += 1)
            {
                float r = a * Mathf.Deg2Rad;
                int x = cx + (int)(Mathf.Cos(r) * radius);
                int y = cy + (int)(Mathf.Sin(r) * radius);
                if (x < 0 || x >= MAP_SIZE || y < 0 || y >= MAP_SIZE) continue;
                _mapTex.SetPixel(x, y, col);
            }
        }

        void DrawLine(int x0, int y0, int x1, int y1, Color col)
        {
            int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            int max = Mathf.Max(dx, dy);
            while (max-- > 0)
            {
                if (x0 >= 0 && x0 < MAP_SIZE && y0 >= 0 && y0 < MAP_SIZE) _mapTex.SetPixel(x0, y0, col);
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }
    }
}
