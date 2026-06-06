using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Gameplay;

namespace Tartaria.UI
{
    /// <summary>
    /// Crafting Overlay — IMGUI panel toggled with C. Lists discovered recipes
    /// and provides a Craft button per row, gated by CraftingSystem.CanCraft.
    /// Self-bootstraps so it works in any scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class CraftingOverlay : MonoBehaviour
    {
        public static CraftingOverlay Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("CraftingOverlay");
            DontDestroyOnLoad(go);
            go.AddComponent<CraftingOverlay>();
        }

        bool _open;
        Vector2 _scroll;
        string _statusMessage;
        float _statusUntil;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            // C (keyboard) or North-button (gamepad Y/Triangle) toggles the overlay.
            bool keyboard = Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame;
            bool gamepad  = Gamepad.current  != null && Gamepad.current.buttonNorth.wasPressedThisFrame;
            if (keyboard || gamepad) _open = !_open;
        }

        void OnGUI()
        {
            if (!_open) return;
            var sys = CraftingSystem.Instance;
            if (sys == null) return;

            const float w = 460f;
            const float h = 360f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;
            GUI.Box(new Rect(x, y, w, h), "");

            var box = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperCenter, fontSize = 18, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(x, y + 6f, w, 30f), "CRAFTING  (K to close)", box);

            var list = sys.GetDiscoveredRecipes();
            var area = new Rect(x + 10f, y + 40f, w - 20f, h - 80f);
            GUILayout.BeginArea(area);
            _scroll = GUILayout.BeginScrollView(_scroll);

            if (list == null || list.Count == 0)
            {
                GUILayout.Label("No recipes discovered yet. Restore moons to unlock recipes.");
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var r = list[i];
                    if (r == null || string.IsNullOrEmpty(r.recipeId)) continue;
                    GUILayout.BeginHorizontal(GUI.skin.box);
                    string label = string.IsNullOrEmpty(r.displayName) ? r.recipeId : r.displayName;
                    GUILayout.Label($"<b>{label}</b>  [{r.requiredTier}]  →  {r.outputItemId} x{r.outputCount}",
                        new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleLeft });
                    GUI.enabled = sys.CanCraft(r.recipeId);
                    if (GUILayout.Button("Craft", GUILayout.Width(80f)))
                    {
                        bool ok = sys.Craft(r.recipeId);
                        ShowStatus(ok ? $"Crafted {r.outputItemId}" : $"Cannot craft {label}");
                    }
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            if (Time.unscaledTime < _statusUntil && !string.IsNullOrEmpty(_statusMessage))
            {
                GUI.Label(new Rect(x, y + h - 32f, w, 24f), _statusMessage,
                    new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic });
            }
        }

        void ShowStatus(string msg)
        {
            _statusMessage = msg;
            _statusUntil = Time.unscaledTime + 2.5f;
        }
    }
}
