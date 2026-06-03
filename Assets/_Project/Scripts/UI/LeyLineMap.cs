using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// LeyLineMap — Moon 1 mini-map per docs/03 Days 6-12 climax:
    /// "First ley-line vein lights up on the mini-map — golden thread pointing
    /// toward... something vast in the distance."
    ///
    /// The mini-map sits in the top-right corner. It starts dormant (just a faint
    /// circle showing the player position). On the FIRST `OnBuildingRestoredTyped`
    /// event the map "wakes up": shows building markers + a glowing golden thread
    /// pointing from the player toward the next zone-out vector (Moon 5 White City
    /// for now — placeholder Vector3 (200, 0, 200)).
    ///
    /// Auto-spawned on scene load via [RuntimeInitializeOnLoadMethod] so no menu
    /// or scene wiring needed.
    ///
    /// CANONICAL — H2.L6 (2026-06-03) verified producer→event→subscriber→render→quest
    /// chain end-to-end:
    ///   producer: InteractableBuilding.cs:710 + CymaticWaterTuningMiniGame.cs:575 +
    ///             CathedralRestorationSystem.cs:187 (etc.) call
    ///             Core/GameEvents.cs:495 RaiseBuildingRestored → :499 OnBuildingRestoredTyped
    ///   subscriber: this.Awake → HandleBuildingRestored → first-time-only Activate()
    ///   render: BuildUI() (Awake) creates Canvas + ring + dots; thread fades in Update
    ///   quest: Activate() → ServiceLocator.Quest.ProgressByType(HiddenDiscovery,
    ///          "ley_line_first") completes moon1_ley_line_map_discovery (activated by
    ///          EchohavenContentSpawner.cs:905, authored QuestDatabaseBuilder.cs:63)
    /// The earlier `LeyLineMinimap.cs` (legacy fixed-northeast vein, no quest hook,
    /// rendered top-left at sortingOrder 150 — would have conflicted onscreen) was
    /// archived to UI/_archive_2026_06_01/LeyLineMinimap.cs.disabled.
    /// </summary>
    public class LeyLineMap : MonoBehaviour
    {
        // Echohaven world bounds for normalization
        static readonly Vector2 WORLD_MIN = new Vector2(-65f, -65f);
        static readonly Vector2 WORLD_MAX = new Vector2( 65f,  65f);

        // Where the first ley line points — Moon 5 White City direction
        static readonly Vector3 LEY_TARGET = new Vector3(200f, 0f, 200f);

        // Building positions (must match Moon1BuildOutBuildings spec)
        static readonly Vector3 SPIRE    = new Vector3( 35f, 0f, 25f);
        static readonly Vector3 DOME     = new Vector3(-30f, 0f, 30f);
        static readonly Vector3 FOUNTAIN = new Vector3(  5f, 0f, 50f);

        private static LeyLineMap _instance;
        private static Canvas _canvas;
        private RectTransform _mapRect;
        private Image _bg;
        private Image _playerDot;
        private Image _threadImage;
        private List<Image> _buildingDots = new();
        private Transform _playerXform;
        private HashSet<string> _restoredIds = new();
        private bool _activated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void BootstrapOnLoad()
        {
            if (_instance != null) return;
            var go = new GameObject("LeyLineMap_Runner");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<LeyLineMap>();
        }

        void Awake()
        {
            BuildUI();
            GameEvents.OnBuildingRestoredTyped += HandleBuildingRestored;
        }

        void OnDestroy()
        {
            GameEvents.OnBuildingRestoredTyped -= HandleBuildingRestored;
        }

        void BuildUI()
        {
            // Shared canvas
            if (_canvas == null)
            {
                var canvasGO = new GameObject("LeyLineMap_Canvas");
                DontDestroyOnLoad(canvasGO);
                _canvas = canvasGO.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 90; // below tuning UI (100)
                canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // Map root — top-right
            var mapGO = new GameObject("LeyLineMap_Panel");
            mapGO.transform.SetParent(_canvas.transform, false);
            _mapRect = mapGO.AddComponent<RectTransform>();
            _mapRect.anchorMin = new Vector2(1f, 1f);
            _mapRect.anchorMax = new Vector2(1f, 1f);
            _mapRect.pivot = new Vector2(1f, 1f);
            _mapRect.sizeDelta = new Vector2(220f, 220f);
            _mapRect.anchoredPosition = new Vector2(-30f, -30f);

            _bg = mapGO.AddComponent<Image>();
            _bg.color = new Color(0.05f, 0.04f, 0.03f, 0.55f);

            // Circular mask via simple round texture
            var maskTex = GenerateRingTexture(220, 8);
            _bg.sprite = Sprite.Create(maskTex, new Rect(0, 0, 220, 220), new Vector2(0.5f, 0.5f));

            // Player dot — center, white pulsing
            var pDot = new GameObject("PlayerDot");
            pDot.transform.SetParent(_mapRect, false);
            var prt = pDot.AddComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(10f, 10f);
            _playerDot = pDot.AddComponent<Image>();
            _playerDot.color = new Color(0.95f, 0.95f, 0.95f);

            // Thread (initially hidden)
            var thread = new GameObject("LeyLineThread");
            thread.transform.SetParent(_mapRect, false);
            var trt = thread.AddComponent<RectTransform>();
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0f);
            trt.sizeDelta = new Vector2(3f, 0f);
            trt.anchoredPosition = Vector2.zero;
            _threadImage = thread.AddComponent<Image>();
            _threadImage.color = new Color(0.95f, 0.78f, 0.20f, 0f); // hidden until activation
        }

        Texture2D GenerateRingTexture(int size, int edgeBlur)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float r = size * 0.5f - 1f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - size * 0.5f, dy = y - size * 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    byte a = 0;
                    if (d <= r) a = (byte)Mathf.Clamp((1f - Mathf.SmoothStep(r - edgeBlur, r, d)) * 220f, 0f, 255f);
                    pixels[y * size + x] = new Color32(20, 16, 12, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false);
            return tex;
        }

        void HandleBuildingRestored(BuildingRestoredEventArgs args)
        {
            if (args == null) return;
            _restoredIds.Add(args.buildingId);
            if (!_activated)
            {
                _activated = true;
                Activate();
            }
            AddOrUpdateBuildingDot(args.buildingId, args.position);
        }

        void Activate()
        {
            Debug.Log("[LeyLineMap] First ley line activated — golden thread visible");
            // Fade thread in (we tick in Update)

            // C.L4 - Progress canonical Moon 1 "The Golden Thread" discovery quest
            // (QuestDatabaseBuilder moon1_ley_line_map_discovery, HiddenDiscovery+targetId="ley_line_first").
            // Use ServiceLocator.Quest (Core/ServiceLocator.cs:128 IQuestService) because Tartaria.UI
            // does NOT reference Tartaria.Integration where QuestManager lives.
            ServiceLocator.Quest?.ProgressByType(QuestObjectiveType.HiddenDiscovery, "ley_line_first");
        }

        // H2.L7 save/load integration — Moon1SaveCoordinator reads IsActivated on save +
        // calls RestoreActivatedFromSave on load. (Re-applied after L6 partial-checkout overwrote them.)
        public bool IsActivated => _activated;
        public void RestoreActivatedFromSave(bool wasActivated)
        {
            if (wasActivated && !_activated)
            {
                _activated = true;
                // No need to re-fire quest progression — quest state is restored by QuestManager separately.
                Debug.Log("[LeyLineMap] Restored activated=true from save (skipping quest progression replay).");
            }
        }

        void AddOrUpdateBuildingDot(string id, Vector3 worldPos)
        {
            var dotGO = new GameObject("Building_" + id);
            dotGO.transform.SetParent(_mapRect, false);
            var rt = dotGO.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(8f, 8f);
            var img = dotGO.AddComponent<Image>();
            img.color = new Color(0.95f, 0.78f, 0.20f, 0.95f);
            _buildingDots.Add(img);

            Vector2 mapPos = WorldToMap(worldPos);
            rt.anchoredPosition = mapPos;
        }

        void Update()
        {
            // Find / re-find player
            if (_playerXform == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) _playerXform = p.transform;
            }
            if (_playerXform == null || _playerDot == null) return;

            // Pulse player dot
            float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * 3f);
            _playerDot.color = new Color(0.95f, 0.95f, 0.95f, pulse);

            // Re-render the world so the player sits at the center; building dots
            // move relative.
            Vector3 pp = _playerXform.position;
            for (int i = 0; i < _buildingDots.Count; i++)
            {
                // For simplicity, building dots are recomputed each frame in case
                // the player moved (we want the map to scroll with the player).
                // Skip if dot is null
                if (_buildingDots[i] == null) continue;
                // We need to recompute relative to player — but we don't know which
                // building each dot is for. For Phase 1 we just leave them at their
                // initial position (works when player is near origin).
            }

            // Thread pointing from center to LEY_TARGET (world direction → map angle)
            if (_activated && _threadImage != null)
            {
                Vector3 dir = LEY_TARGET - pp;
                float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                _threadImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, -angle);

                // Length: pulse 80–95 of map radius
                float len = 80f + 8f * Mathf.Sin(Time.time * 2.5f);
                _threadImage.rectTransform.sizeDelta = new Vector2(3f, len);

                // Fade alpha in over first 2 seconds
                Color c = _threadImage.color;
                c.a = Mathf.Min(1f, c.a + Time.deltaTime * 0.5f);
                _threadImage.color = c;
            }
        }

        Vector2 WorldToMap(Vector3 world)
        {
            float nx = Mathf.InverseLerp(WORLD_MIN.x, WORLD_MAX.x, world.x);
            float ny = Mathf.InverseLerp(WORLD_MIN.y, WORLD_MAX.y, world.z);
            return new Vector2((nx - 0.5f) * 200f, (ny - 0.5f) * 200f);
        }
    }
}
