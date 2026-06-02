#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor-only QA bypass for the E-key tuning chain. While WASD movement is being
    /// repaired on a separate lane, this menu lets QA left-click any InteractableBuilding
    /// in the Game view to fire one tuning-node completion (accuracy=1f) — three clicks
    /// completes a building and triggers BeginEmergence via the canonical pipeline.
    ///
    /// Rules honored (per CLAUDE.md no-debt mandate):
    /// - Does NOT introduce a runtime override driver. All work happens in the editor
    ///   assembly under #if UNITY_EDITOR.
    /// - Does NOT bypass the canonical pipeline. It invokes InteractableBuilding's own
    ///   private OnTuningComplete(float) method by reflection — the exact callback the
    ///   real TuningMiniGame fires. State machine, save-dirty, audio cues, BeginEmergence,
    ///   GameEvents.RaiseBuildingRestored all run through the real code paths.
    /// - No silent fails / silent fallbacks. Missed raycasts log warnings with cursor
    ///   world position; missing reflection targets log errors naming the symbol searched.
    ///
    /// Menu: Tartaria → 9 QA → Click-To-Tune (Game-view raycast)
    /// Toggle the menu item to enable/disable. While enabled, left-click in the Game
    /// view (Play mode required) is intercepted via EditorApplication.update polling
    /// Mouse.current. Click-through resumes when the toggle is off.
    /// </summary>
    public static class Moon1ClickToTune
    {
        const string MenuPath = "Tartaria/9 QA/Click-To-Tune (Game-view raycast)";
        const string PrefsKey = "Tartaria.Moon1ClickToTune.Enabled";

        const string TargetTypeName = "Tartaria.Integration.InteractableBuilding";
        const string TargetMethodName = "OnTuningComplete";
        const string TargetNodesField = "_nodesCompleted";

        static bool _enabled;
        static bool _hookInstalled;
        static bool _lastMouseDown;

        [InitializeOnLoadMethod]
        static void Init()
        {
            _enabled = EditorPrefs.GetBool(PrefsKey, false);
            EditorApplication.delayCall += () => Menu.SetChecked(MenuPath, _enabled);
            if (_enabled) InstallHook();
        }

        [MenuItem(MenuPath, priority = 920)]
        static void Toggle()
        {
            _enabled = !_enabled;
            EditorPrefs.SetBool(PrefsKey, _enabled);
            Menu.SetChecked(MenuPath, _enabled);

            if (_enabled)
            {
                InstallHook();
                Debug.Log("[Moon1ClickToTune] ENABLED. Left-click an InteractableBuilding in the Game view (Play mode) to tune one node. 3 clicks per building.");
            }
            else
            {
                UninstallHook();
                Debug.Log("[Moon1ClickToTune] DISABLED. Game-view clicks pass through normally.");
            }
        }

        [MenuItem(MenuPath, validate = true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, _enabled);
            return true;
        }

        static void InstallHook()
        {
            if (_hookInstalled) return;
            EditorApplication.update += PollMouse;
            _hookInstalled = true;
            _lastMouseDown = false;
        }

        static void UninstallHook()
        {
            if (!_hookInstalled) return;
            EditorApplication.update -= PollMouse;
            _hookInstalled = false;
        }

        static void PollMouse()
        {
            if (!_enabled) return;
            if (!Application.isPlaying) return;

            var mouse = Mouse.current;
            if (mouse == null)
            {
                // Fail loud per rule 4 — don't swallow silently.
                if (!_lastMouseDown)
                {
                    Debug.LogWarning("[Moon1ClickToTune] Mouse.current is null — Input System has no mouse device. Cannot intercept Game-view clicks.");
                    _lastMouseDown = true; // suppress repeat spam until pointer reappears
                }
                return;
            }

            bool down = mouse.leftButton.isPressed;
            // Detect rising edge: was up, now down.
            if (down && !_lastMouseDown)
            {
                Vector2 screenPos = mouse.position.ReadValue();
                if (IsOverGameView(screenPos))
                {
                    TryTuneAtScreen(screenPos);
                }
            }
            _lastMouseDown = down;
        }

        /// <summary>
        /// Mouse.current.position is in OS/Game-view coordinates already in Play mode,
        /// but we sanity-check the GameView window has focus to avoid intercepting
        /// clicks aimed at Scene view / inspector while Play is running.
        /// </summary>
        static bool IsOverGameView(Vector2 screenPos)
        {
            var focused = EditorWindow.focusedWindow;
            if (focused == null) return false;
            string n = focused.GetType().Name;
            // GameView, PlayModeView — be permissive but exclude SceneView/Inspector.
            return n == "GameView" || n == "PlayModeView";
        }

        static void TryTuneAtScreen(Vector2 screenPos)
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null)
            {
                Debug.LogWarning($"[Moon1ClickToTune] Camera.main is null — no MainCamera tag in the active scene. Cursor was at screen={screenPos}.");
                return;
            }

            // Game view bottom-left origin matches ScreenPointToRay's expectation.
            Ray ray = cam.ScreenPointToRay(screenPos);

            if (!Physics.Raycast(ray, out RaycastHit hit, 500f, ~0, QueryTriggerInteraction.Collide))
            {
                Vector3 missPoint = ray.origin + ray.direction * 25f;
                Debug.LogWarning($"[Moon1ClickToTune] Raycast missed. screen={screenPos} rayOrigin={ray.origin} rayDir={ray.direction} cursorWorld(approx 25m)={missPoint}");
                return;
            }

            var building = hit.collider.GetComponentInParent<InteractableBuilding>();
            if (building == null)
            {
                Debug.LogWarning($"[Moon1ClickToTune] Hit '{hit.collider.gameObject.name}' at world={hit.point} but no InteractableBuilding on it or its parents.");
                return;
            }

            InvokeOnTuningComplete(building, 1f);
        }

        static void InvokeOnTuningComplete(InteractableBuilding building, float accuracy)
        {
            var t = building.GetType();
            if (t.FullName != TargetTypeName)
            {
                Debug.LogError($"[Moon1ClickToTune] Unexpected type '{t.FullName}' — expected '{TargetTypeName}'. Reflection target mismatch.");
                return;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            var method = t.GetMethod(TargetMethodName, flags, null, new[] { typeof(float) }, null);
            if (method == null)
            {
                Debug.LogError($"[Moon1ClickToTune] Reflection target not found: {TargetTypeName}.{TargetMethodName}(float). InteractableBuilding API may have changed — update Moon1ClickToTune.cs.");
                return;
            }

            var nodesField = t.GetField(TargetNodesField, flags);
            if (nodesField == null)
            {
                Debug.LogError($"[Moon1ClickToTune] Reflection field not found: {TargetTypeName}.{TargetNodesField}. InteractableBuilding API may have changed — update Moon1ClickToTune.cs.");
                return;
            }

            int before = (int)nodesField.GetValue(building);
            if (before >= 3)
            {
                Debug.Log($"[Moon1ClickToTune] '{building.gameObject.name}' already at 3/3 nodes (state={building.State}). No-op.");
                return;
            }

            try
            {
                method.Invoke(building, new object[] { accuracy });
            }
            catch (TargetInvocationException tie)
            {
                Debug.LogError($"[Moon1ClickToTune] OnTuningComplete threw on '{building.gameObject.name}': {tie.InnerException}");
                return;
            }

            int after = (int)nodesField.GetValue(building);
            int remaining = Mathf.Max(0, 3 - after);

            Debug.Log($"[Moon1ClickToTune] Tuned node on '{building.gameObject.name}' — nodesCompleted now {after}/3 (state={building.State}). {remaining} click(s) remaining for this building.");

            if (after >= 3)
            {
                Debug.Log($"[Moon1ClickToTune] '{building.gameObject.name}' fully tuned — BeginEmergence should have fired. Watch for restore_success SFX and EchohavenProgressionSystem notification.");
            }
        }
    }
}
#endif
