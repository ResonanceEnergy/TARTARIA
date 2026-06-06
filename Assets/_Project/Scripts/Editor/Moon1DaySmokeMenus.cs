// Sprint 10 Lane 7 — Day-25 Lirael smoke-test menus.
// Edits the TartarianCalendar day counter from the Editor so we can verify
// the Lirael Day-25 gate (Sprint 9 Lane 3) without sitting through ~50 minutes
// of in-game time at the 120s/day rate baked into TartarianCalendar.cs.
//
// The canonical advance method on TartarianCalendar is `AdvanceDay()` (file:line
// Assets/_Project/Scripts/Core/TartarianCalendar.cs:83) but it is `private`.
// To avoid touching Core while Moon 1 is in flight (per CLAUDE.md), we invoke
// it via reflection from this Editor-only assembly. The public read API
// `CurrentMoonDay` (TartarianCalendar.cs:111) gives us the before/after values
// to log loudly.
//
// All three menus are gated on EditorApplication.isPlaying — advancing the
// in-game day during Edit mode would not propagate through `Start()` and the
// OnDayAdvanced subscribers (e.g. Moon1LiraelDay25Gate) would not fire.

using System.Reflection;
using UnityEditor;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Editor.Moon1
{
    /// <summary>
    /// Editor menus for smoke-testing the Day-25 Lirael gate without playing
    /// the game for 25 in-game days. Lives under <c>Tartaria/9 Debug</c>.
    /// </summary>
    public static class Moon1DaySmokeMenus
    {
        private const string MENU_ADVANCE       = "Tartaria/9 Debug/Advance Day";
        private const string MENU_JUMP_25       = "Tartaria/9 Debug/Jump to Day 25";
        private const string MENU_SET_PROMPT    = "Tartaria/9 Debug/Set Day to N (Prompt)";
        private const int    SAFETY_TICK_CAP    = 64; // hard upper bound on AdvanceDay() loops

        // ---------------- Advance Day ----------------

        [MenuItem(MENU_ADVANCE, priority = 9100)]
        public static void AdvanceDayOnce()
        {
            if (!TryGetCalendar(out var cal)) return;
            int before = cal.CurrentMoonDay;
            if (!InvokeAdvance(cal)) return;
            int after = cal.CurrentMoonDay;
            Debug.Log($"[Moon1DaySmoke] Day {before} -> {after}; OnDayChanged should fire next frame");
        }

        [MenuItem(MENU_ADVANCE, validate = true)]
        public static bool AdvanceDayOnce_Validate() => EditorApplication.isPlaying;

        // ---------------- Jump to Day 25 ----------------

        [MenuItem(MENU_JUMP_25, priority = 9101)]
        public static void JumpToDay25()
        {
            if (!TryGetCalendar(out var cal)) return;

            int target = 25;
            int safety = 0;
            int before = cal.CurrentMoonDay;

            while (cal.CurrentMoonDay != target && safety < SAFETY_TICK_CAP)
            {
                int prev = cal.CurrentMoonDay;
                if (!InvokeAdvance(cal)) return;
                int next = cal.CurrentMoonDay;
                Debug.Log($"[Moon1DaySmoke] Day {prev} -> {next}; OnDayChanged should fire next frame");
                safety++;

                // Calendar wraps at 28 -> 1; if we passed 25 going forward, bail loud.
                if (next == prev)
                {
                    Debug.LogError($"[Moon1DaySmoke] AdvanceDay() did not change CurrentMoonDay (still {next}). " +
                                   $"Aborting Jump to Day 25 — investigate TartarianCalendar.AdvanceDay() at line 83.");
                    return;
                }
            }

            if (cal.CurrentMoonDay != target)
            {
                Debug.LogError($"[Moon1DaySmoke] Hit safety cap ({SAFETY_TICK_CAP} ticks) without reaching Day {target}. " +
                               $"Started at Day {before}, currently Day {cal.CurrentMoonDay}.");
            }
            else
            {
                Debug.Log($"[Moon1DaySmoke] Reached Day {target} after {safety} tick(s). Lirael Day-25 gate should now have activated.");
            }
        }

        [MenuItem(MENU_JUMP_25, validate = true)]
        public static bool JumpToDay25_Validate() => EditorApplication.isPlaying;

        // ---------------- Set Day to N (Prompt) ----------------

        [MenuItem(MENU_SET_PROMPT, priority = 9102)]
        public static void SetDayToNPrompt()
        {
            if (!TryGetCalendar(out var cal)) return;

            // EditorUtility.DisplayDialog doesn't accept text input, so we use
            // a Selectable-style choice plus a fixed small list. For a true free
            // entry, we fall back to a prompt window via a simple input field.
            string entered = SimpleInputDialog.Show(
                title: "Set Day to N",
                message: $"Enter target day (1-28). Current: {cal.CurrentMoonDay}",
                defaultValue: cal.CurrentMoonDay.ToString());

            if (string.IsNullOrWhiteSpace(entered))
            {
                Debug.Log("[Moon1DaySmoke] Set Day to N cancelled.");
                return;
            }

            if (!int.TryParse(entered.Trim(), out int target))
            {
                Debug.LogError($"[Moon1DaySmoke] '{entered}' is not a valid integer. Aborting.");
                return;
            }

            target = Mathf.Clamp(target, 1, 28);
            int safety = 0;
            int before = cal.CurrentMoonDay;

            while (cal.CurrentMoonDay != target && safety < SAFETY_TICK_CAP)
            {
                int prev = cal.CurrentMoonDay;
                if (!InvokeAdvance(cal)) return;
                int next = cal.CurrentMoonDay;
                Debug.Log($"[Moon1DaySmoke] Day {prev} -> {next}; OnDayChanged should fire next frame");
                safety++;

                if (next == prev)
                {
                    Debug.LogError($"[Moon1DaySmoke] AdvanceDay() did not change CurrentMoonDay (still {next}). Aborting.");
                    return;
                }
            }

            if (cal.CurrentMoonDay != target)
                Debug.LogError($"[Moon1DaySmoke] Hit safety cap ({SAFETY_TICK_CAP}) targeting Day {target}. Now at Day {cal.CurrentMoonDay} (started at {before}).");
            else
                Debug.Log($"[Moon1DaySmoke] Reached Day {target} after {safety} tick(s).");
        }

        [MenuItem(MENU_SET_PROMPT, validate = true)]
        public static bool SetDayToNPrompt_Validate() => EditorApplication.isPlaying;

        // ---------------- helpers ----------------

        private static bool TryGetCalendar(out TartarianCalendar cal)
        {
            cal = TartarianCalendar.Instance;
            if (cal == null)
            {
                Debug.LogError("[Moon1DaySmoke] TartarianCalendar.Instance is NULL. " +
                               "Are you in Play mode AND has the Echohaven scene initialised? " +
                               "Aborting day-advance request.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// TartarianCalendar.AdvanceDay() is private (see TartarianCalendar.cs:83).
        /// Rather than mutate Core to add a public wrapper, the Editor tool calls
        /// the existing method via reflection. If the method name ever changes
        /// (e.g. to NextDay / Tick / IncrementDay) we log loud and bail.
        /// </summary>
        private static bool InvokeAdvance(TartarianCalendar cal)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var t = cal.GetType();
            var mi = t.GetMethod("AdvanceDay", flags, binder: null, types: System.Type.EmptyTypes, modifiers: null)
                  ?? t.GetMethod("Advance",    flags, binder: null, types: System.Type.EmptyTypes, modifiers: null)
                  ?? t.GetMethod("NextDay",    flags, binder: null, types: System.Type.EmptyTypes, modifiers: null)
                  ?? t.GetMethod("IncrementDay", flags, binder: null, types: System.Type.EmptyTypes, modifiers: null)
                  ?? t.GetMethod("Tick",       flags, binder: null, types: System.Type.EmptyTypes, modifiers: null);

            if (mi == null)
            {
                Debug.LogError("[Moon1DaySmoke] Could not find AdvanceDay / Advance / NextDay / IncrementDay / Tick on TartarianCalendar. " +
                               "Open Assets/_Project/Scripts/Core/TartarianCalendar.cs and grep for the canonical advance method.");
                return false;
            }

            try
            {
                mi.Invoke(cal, parameters: null);
                return true;
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                Debug.LogError($"[Moon1DaySmoke] {mi.Name}() threw: {tie.InnerException?.GetType().Name} :: {tie.InnerException?.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Minimal modal input dialog. <see cref="EditorUtility.DisplayDialog"/> does not
    /// accept free text, and we don't want to pull in IMGUI just for one prompt.
    /// </summary>
    internal class SimpleInputDialog : EditorWindow
    {
        private string _value = string.Empty;
        private string _message = string.Empty;
        private bool _confirmed;
        private bool _done;

        public static string Show(string title, string message, string defaultValue)
        {
            var w = CreateInstance<SimpleInputDialog>();
            w.titleContent = new GUIContent(title);
            w._message = message ?? string.Empty;
            w._value = defaultValue ?? string.Empty;
            w.minSize = new Vector2(360, 110);
            w.maxSize = new Vector2(360, 110);
            w.ShowModalUtility();
            return w._confirmed ? w._value : null;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(_message, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(4);

            GUI.SetNextControlName("Moon1DaySmoke_Input");
            _value = EditorGUILayout.TextField(_value);
            if (!_done)
            {
                EditorGUI.FocusTextInControl("Moon1DaySmoke_Input");
                _done = true;
            }

            EditorGUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Cancel", GUILayout.Height(24))) { _confirmed = false; Close(); }
                if (GUILayout.Button("OK",     GUILayout.Height(24))) { _confirmed = true;  Close(); }
            }

            var e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) { _confirmed = true; Close(); }
                else if (e.keyCode == KeyCode.Escape) { _confirmed = false; Close(); }
            }
        }
    }
}
