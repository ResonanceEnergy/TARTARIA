using Unity.Entities;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Debug Overlay — F1 toggles an on-screen debug panel showing:
    ///   - FPS counter
    ///   - Current RS (from ECS)
    ///   - Game State
    ///   - Aether charge
    ///   - Active entity count
    ///   - Zone info (buildings restored)
    ///   - Save status
    ///   - Player position
    ///
    /// Uses IMGUI for zero-dependency rendering (no Canvas/TMP required).
    /// Editor and Development builds only.
    /// </summary>
    public class DebugOverlay : MonoBehaviour
    {
        [SerializeField] bool showOnStart;
        [SerializeField] KeyCode toggleKey = KeyCode.F1;

        bool _visible;
        float _fps;
        float _fpsTimer;
        int _frameCount;

        // ECS cache
        World _world;
        EntityManager _em;
        Entity _rsEntity;
        bool _ecsReady;

        // Player cache
        Transform _cachedPlayer;

        void Start()
        {
            _visible = showOnStart;
        }

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(toggleKey))
                _visible = !_visible;

            // FPS calculation
            _frameCount++;
            _fpsTimer += Time.unscaledDeltaTime;
            if (_fpsTimer >= 0.5f)
            {
                _fps = _frameCount / _fpsTimer;
                _frameCount = 0;
                _fpsTimer = 0f;
            }

            // Lazy ECS init
            if (!_ecsReady)
            {
                _world = World.DefaultGameObjectInjectionWorld;
                if (_world != null)
                {
                    _em = _world.EntityManager;
                    var query = _em.CreateEntityQuery(typeof(ResonanceScore));
                    if (query.CalculateEntityCount() > 0)
                    {
                        _rsEntity = query.GetSingletonEntity();
                        _ecsReady = true;
                    }
                }
            }
        }

        void OnGUI()
        {
            if (!_visible) return;

            // Style
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                alignment = TextAnchor.UpperLeft,
                richText = true
            };
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                richText = true
            };

            float w = 320f;
            float h = 340f;
            float x = Screen.width - w - 10f;
            float y = 10f;

            GUI.Box(new Rect(x, y, w, h), "<b>TARTARIA DEBUG [F1]</b>", style);

            float lineHeight = 20f;
            float cx = x + 10f;
            float cy = y + 28f;

            // FPS
            Color fpsColor = _fps >= 60 ? Color.green : _fps >= 30 ? Color.yellow : Color.red;
            DrawLabel(labelStyle, cx, cy, $"FPS: <color=#{ColorUtility.ToHtmlStringRGB(fpsColor)}>{_fps:F0}</color>");
            cy += lineHeight;

            // Game State
            var gs = GameStateManager.Instance;
            DrawLabel(labelStyle, cx, cy, $"State: <color=yellow>{gs.CurrentState}</color> (prev: {gs.PreviousState})");
            cy += lineHeight;

            // RS
            float rs = 0f;
            if (_ecsReady && _em.Exists(_rsEntity))
            {
                var rsData = _em.GetComponentData<ResonanceScore>(_rsEntity);
                rs = rsData.CurrentRS;
                DrawLabel(labelStyle, cx, cy, $"RS: <color=#F4C542>{rs:F1}</color>/100  Threshold: {rsData.ThresholdReached}");
                cy += lineHeight;
                DrawLabel(labelStyle, cx, cy, $"Global RS: {rsData.GlobalRS:F1}  Best Zone: {rsData.HighestZoneRS:F1}");
            }
            else
            {
                DrawLabel(labelStyle, cx, cy, "RS: <color=red>ECS not ready</color>");
                cy += lineHeight;
                DrawLabel(labelStyle, cx, cy, "");
            }
            cy += lineHeight;

            // Entity count
            if (_world != null)
            {
                int entityCount = _world.IsCreated ? _em.UniversalQuery.CalculateEntityCount() : 0;
                DrawLabel(labelStyle, cx, cy, $"Entities: {entityCount}");
            }
            cy += lineHeight;

            // Player position
            if (_cachedPlayer == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) _cachedPlayer = playerObj.transform;
            }
            if (_cachedPlayer != null)
            {
                var pos = _cachedPlayer.position;
                DrawLabel(labelStyle, cx, cy, $"Player: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
            }
            cy += lineHeight;

            // Zone info
            var zone = ZoneController.Instance;
            if (zone != null)
            {
                DrawLabel(labelStyle, cx, cy, $"Zone: {zone.ZoneName}  Buildings: {zone.GetRestoredBuildingCount()}/{zone.GetTotalBuildingCount()}");
            }
            cy += lineHeight;

            // Combat
            var combat = CombatBridge.Instance;
            DrawLabel(labelStyle, cx, cy, $"Combat: {(combat != null ? "Ready" : "N/A")}  " +
                $"InCombat: {(gs.CurrentState == GameState.Combat)}");
            cy += lineHeight;

            // Save
            var save = SaveManager.Instance;
            if (save?.CurrentSave != null)
            {
                float playTime = save.CurrentSave.header.playTimeSeconds;
                int mins = (int)(playTime / 60);
                int secs = (int)(playTime % 60);
                DrawLabel(labelStyle, cx, cy, $"Save: v{save.CurrentSave.header.gameVersion}  Play: {mins}m {secs}s");
            }
            cy += lineHeight;

            // Memory
            long mem = System.GC.GetTotalMemory(false) / (1024 * 1024);
            DrawLabel(labelStyle, cx, cy, $"Memory: {mem} MB (managed)");
            cy += lineHeight;

            // Controls hint
            DrawLabel(labelStyle, cx, cy, "<color=#888>WASD=Move  Tab=Aether  Click=Interact  Esc=Pause</color>");
        }

        void DrawLabel(GUIStyle style, float x, float y, string text)
        {
            GUI.Label(new Rect(x, y, 300f, 20f), text, style);
        }
    }
}
