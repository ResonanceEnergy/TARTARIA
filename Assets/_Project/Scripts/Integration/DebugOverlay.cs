using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
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
        EntityQuery _rsQuery;
        bool _ecsReady;

        // Player cache
        Transform _cachedPlayer;

        // GUI cache
        GUIStyle _boxStyle;
        GUIStyle _labelStyle;

        // String cache (rebuilt when values change)
        string _fpsString = "";
        float _lastFpsDisplay;
        string _stateString = "";
        GameState _lastState;
        GameState _lastPrevState;
        string _memString = "";
        long _lastMemMB;

        void Start()
        {
            _visible = showOnStart;

            // Cache player transform at startup
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) _cachedPlayer = playerObj.transform;
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
                _visible = !_visible;
            // Legacy Input fallback (configurable toggleKey)
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
                    _rsQuery = _em.CreateEntityQuery(typeof(ResonanceScore));
                }
                if (_rsQuery.IsValid && _rsQuery.CalculateEntityCount() > 0)
                {
                    _rsEntity = _rsQuery.GetSingletonEntity();
                    _ecsReady = true;
                }
            }
        }

        void OnGUI()
        {
            if (!_visible) return;

            // Style (cached)
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 14,
                    alignment = TextAnchor.UpperLeft,
                    richText = true
                };
            }
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    richText = true
                };
            }

            float w = 320f;
            float h = 340f;
            float x = Screen.width - w - 10f;
            float y = 10f;

            GUI.Box(new Rect(x, y, w, h), "<b>TARTARIA DEBUG [F1]</b>", _boxStyle);

            float lineHeight = 20f;
            float cx = x + 10f;
            float cy = y + 28f;

            // FPS (cached — only reformats every 0.5s when _fps changes)
            if (_fps != _lastFpsDisplay)
            {
                _lastFpsDisplay = _fps;
                Color fpsColor = _fps >= 60 ? Color.green : _fps >= 30 ? Color.yellow : Color.red;
                _fpsString = $"FPS: <color=#{ColorUtility.ToHtmlStringRGB(fpsColor)}>{_fps:F0}</color>";
            }
            DrawLabel(cx, cy, _fpsString);
            cy += lineHeight;

            // Game State (cached)
            var gs = GameStateManager.Instance;
            if (gs.CurrentState != _lastState || gs.PreviousState != _lastPrevState)
            {
                _lastState = gs.CurrentState;
                _lastPrevState = gs.PreviousState;
                _stateString = $"State: <color=yellow>{gs.CurrentState}</color> (prev: {gs.PreviousState})";
            }
            DrawLabel(cx, cy, _stateString);
            cy += lineHeight;

            // RS
            float rs = 0f;
            if (_ecsReady && _em.Exists(_rsEntity))
            {
                var rsData = _em.GetComponentData<ResonanceScore>(_rsEntity);
                rs = rsData.CurrentRS;
                DrawLabel(cx, cy, $"RS: <color=#F4C542>{rs:F1}</color>/100  Threshold: {rsData.ThresholdReached}");
                cy += lineHeight;
                DrawLabel(cx, cy, $"Global RS: {rsData.GlobalRS:F1}  Best Zone: {rsData.HighestZoneRS:F1}");
            }
            else
            {
                DrawLabel(cx, cy, "RS: <color=red>ECS not ready</color>");
                cy += lineHeight;
                DrawLabel(cx, cy, "");
            }
            cy += lineHeight;

            // Entity count
            if (_world != null)
            {
                int entityCount = _world.IsCreated ? _em.UniversalQuery.CalculateEntityCount() : 0;
                DrawLabel(cx, cy, $"Entities: {entityCount}");
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
                DrawLabel(cx, cy, $"Player: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
            }
            cy += lineHeight;

            // Zone info
            var zone = ZoneController.Instance;
            if (zone != null)
            {
                DrawLabel(cx, cy, $"Zone: {zone.ZoneName}  Buildings: {zone.GetRestoredBuildingCount()}/{zone.GetTotalBuildingCount()}");
            }
            cy += lineHeight;

            // Combat
            var combat = CombatBridge.Instance;
            DrawLabel(cx, cy, $"Combat: {(combat != null ? "Ready" : "N/A")}  " +
                $"InCombat: {(gs.CurrentState == GameState.Combat)}");
            cy += lineHeight;

            // Save
            var save = SaveManager.Instance;
            if (save?.CurrentSave != null)
            {
                float playTime = save.CurrentSave.header.playTimeSeconds;
                int mins = (int)(playTime / 60);
                int secs = (int)(playTime % 60);
                DrawLabel(cx, cy, $"Save: v{save.CurrentSave.header.gameVersion}  Play: {mins}m {secs}s");
            }
            cy += lineHeight;

            // Memory (cached)
            long mem = System.GC.GetTotalMemory(false) / (1024 * 1024);
            if (mem != _lastMemMB)
            {
                _lastMemMB = mem;
                _memString = $"Memory: {mem} MB (managed)";
            }
            DrawLabel(cx, cy, _memString);
            cy += lineHeight;

            // Controls hint
            DrawLabel(cx, cy, "<color=#888>WASD=Move  Tab=Aether  Click=Interact  Esc=Pause</color>");
        }

        void DrawLabel(float x, float y, string text)
        {
            GUI.Label(new Rect(x, y, 300f, 20f), text, _labelStyle);
        }
    }
}
