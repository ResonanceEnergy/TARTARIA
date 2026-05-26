using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.Gameplay;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// Day-10: Lightweight IMGUI inventory + quest log overlays.
    /// I = Inventory, J = Quest Log. Single self-bootstrap singleton.
    /// Reads from InventorySystem and QuestProviderLocator.
    /// </summary>
    [DisallowMultipleComponent]
    public class InventoryQuestOverlay : MonoBehaviour
    {
        static InventoryQuestOverlay _instance;

        enum View { Hidden, Inventory, Quests }
        View _view = View.Hidden;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("InventoryQuestOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<InventoryQuestOverlay>();
        }

        void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;
            if (kb.iKey.wasPressedThisFrame)
                _view = _view == View.Inventory ? View.Hidden : View.Inventory;
            else if (kb.jKey.wasPressedThisFrame)
                _view = _view == View.Quests ? View.Hidden : View.Quests;
        }

        void OnGUI()
        {
            if (_view == View.Hidden) return;

            const int W = 360, H = 420;
            int x = Screen.width - W - 24;
            int y = 24;

            GUI.Box(new Rect(x, y, W, H), "");
            string title = _view == View.Inventory ? "<b>INVENTORY</b>  (I)" : "<b>QUEST LOG</b>  (J)";
            var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, richText = true, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
            GUI.Label(new Rect(x, y + 6, W, 24), title, titleStyle);

            var bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, richText = true, wordWrap = true, normal = { textColor = new Color(0.92f, 0.92f, 0.92f) } };
            string body = _view == View.Inventory ? BuildInventoryText() : BuildQuestText();
            GUI.Label(new Rect(x + 12, y + 36, W - 24, H - 50), body, bodyStyle);
        }

        string BuildInventoryText()
        {
            var inv = InventorySystem.Instance;
            if (inv == null) return "<i>InventorySystem not ready.</i>";
            var sb = new StringBuilder();
            int total = 0;
            foreach (var kv in inv.GetAllItems())
            {
                sb.AppendLine($"• <b>{kv.Key}</b>  ×{kv.Value}");
                total += kv.Value;
            }
            if (total == 0) sb.AppendLine("<i>Empty. Defeat enemies and explore to find items.</i>");
            sb.AppendLine();
            sb.AppendLine("<color=#888888>Press I to close.</color>");
            return sb.ToString();
        }

        string BuildQuestText()
        {
            // QuestProviderLocator disabled (Phase 35 - Integration assembly)
            var sb = new StringBuilder();
            sb.AppendLine($"<b>Scene:</b> {SceneManager.GetActiveScene().name}");
            sb.AppendLine();
            sb.AppendLine("<i>QuestProvider not ready.</i>");

            // Moon clear progression — fetched via reflection to avoid asmdef cycle UI ↔ Integration.
            int? clearedCount = TryGetMoonClearedCount();
            if (clearedCount.HasValue)
            {
                sb.AppendLine();
                sb.AppendLine($"<b>Moons cleared:</b> {clearedCount.Value} / 13");
            }

            sb.AppendLine();
            sb.AppendLine("<color=#888888>Press J to close.</color>");
            return sb.ToString();
        }

        static int? TryGetMoonClearedCount()
        {
            var t = System.Type.GetType("Tartaria.Integration.MoonProgressTracker, Tartaria.Integration");
            if (t == null) return null;
            var instProp = t.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var inst = instProp?.GetValue(null);
            if (inst == null) return null;
            var countProp = t.GetProperty("ClearedCount");
            return countProp?.GetValue(inst) as int?;
        }
    }
}
