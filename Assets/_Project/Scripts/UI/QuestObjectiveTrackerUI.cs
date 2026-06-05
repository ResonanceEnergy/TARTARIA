// File: Assets/_Project/Scripts/UI/QuestObjectiveTrackerUI.cs
using UnityEngine;
using UnityEngine.UI;

namespace Tartaria.UI
{
    public class QuestObjectiveTrackerUI : MonoBehaviour
    {
        private static QuestObjectiveTrackerUI _instance;
        private Canvas _canvas;
        private Text _primaryText;
        private Text _sublineText;
        private Text _secondaryCountText;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("QuestObjectiveTrackerUI");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<QuestObjectiveTrackerUI>();
            _instance.BuildCanvas();
        }

        private void BuildCanvas()
        {
            _canvas = new Canvas();
            _canvas.sortingOrder = 200;
            _canvas.scaleMode = ScaleMode.ScaleWithScreenSize;
            _canvas.screenMatchMode = ScreenMatchMode.ScreenSize;
            _canvas.anchoredPosition = new Vector3(-20, -120);
            _canvas.sizeDelta = new Vector2(380, 90);

            var panel = new GameObject("Panel");
            panel.transform.SetParent(_canvas.transform, false);
            panel.AddComponent<RectTransform>();
            panel.GetComponent<RectTransform>().anchoredMin = new Vector2(1, 1);
            panel.GetComponent<RectTransform>().anchoredMax = new Vector2(1, 1);
            panel.GetComponent<RectTransform>().pivot = new Vector2(1, 1);

            var background = new GameObject("Background");
            background.transform.SetParent(panel.transform, false);
            background.AddComponent<SpriteRenderer>();
            background.GetComponent<SpriteRenderer>().sprite = GetWhite();

            _primaryText = new GameObject("PrimaryText");
            _primaryText.transform.SetParent(panel.transform, false);
            _primaryText.AddComponent<Text>();
            _primaryText.GetComponent<Text>().font = Resources.Load<Font>("LegacyRuntime.ttf");
            _primaryText.GetComponent<Text>().fontSize = 15;
            _primaryText.GetComponent<Text>().color = Color golden;

            _sublineText = new GameObject("SublineText");
            _sublineText.transform.SetParent(panel.transform, false);
            _sublineText.AddComponent<Text>();
            _sublineText.GetComponent<Text>().font = Resources.Load<Font>("LegacyRuntime.ttf");
            _sublineText.GetComponent<Text>().fontSize = 11;
            _sublineText.GetComponent<Text>().color = Color.gray;

            _secondaryCountText = new GameObject("SecondaryCountText");
            _secondaryCountText.transform.SetParent(panel.transform, false);
            _secondaryCountText.AddComponent<Text>();
            _secondaryCountText.GetComponent<Text>().font = Resources.Load<Font>("LegacyRuntime.ttf");
            _secondaryCountText.GetComponent<Text>().fontSize = 11;
            _secondaryCountText.GetComponent<Text>().color = Color.gray;

            _primaryText.text = "Restore the buried buildings";
            _sublineText.text = "Find and tune at least one hero structure";

            if (PlayerPrefs.GetInt("TARTARIA_Moon1Complete", 0) == 0)
            {
                SetPrimary("Restore the buried buildings");
                SetSubline("Find and tune at least one hero structure");
            }
            else
            {
                SetPrimary("Rest at the Inn");
                SetSubline("Find the warm-glowing platform east of spawn");
            }

            SubscribeToBuildingRestored();
        }

        private void SubscribeToBuildingRestored()
        {
            EventSystem.current.AddHandler<BuildingRestoredEvent>(OnBuildingRestored);
        }

        private void OnBuildingRestored(BuildingRestoredEvent e)
        {
            if (e.BuildingType == BuildingType.Restored)
            {
                UpdateProgress();
            }
        }

        private void UpdateProgress()
        {
            var restoredCount = PlayerPrefs.GetInt("TARTARIA_Restored_" + e.BuildingType.ToString(), 0);
            _primaryText.text = "Restore the buried buildings";
            _sublineText.text = $"Find and tune at least one hero structure ({restoredCount}/{3})";

            if (restoredCount >= 3)
            {
                SetPrimary("Rest at the Inn");
                SetSubline("Find the warm-glowing platform east of spawn");
            }
        }

        public static void SetPrimary(string title, string subline = "")
        {
            _primaryText.text = title;
            _sublineText.text = subline;
        }

        public static void AddSecondary(string id, string title)
        {
            // Implement secondary objective logic here
        }

        public static void RemoveSecondary(string id)
        {
            // Implement secondary objective removal logic here
        }

        public static void Clear()
        {
            _primaryText.text = "";
            _sublineText.text = "";
            _secondaryCountText.text = "";
        }
    }
}
