using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Tartaria.Save;

namespace Tartaria.UI
{
    /// <summary>
    /// Sprint 6 Lane 3: Single save-slot card view.
    /// Renders: screenshot thumbnail, Moon name + ISO timestamp, play time HH:MM:SS,
    /// Resonance Shards (aetherShards), buildings restored count.
    /// Wires Load button to SaveManager.SwitchToSlot(int) (canonical — SaveManager has no LoadSlot;
    /// confirmed by grep at Assets/_Project/Scripts/Save/SaveManager.cs:595).
    /// Wires Delete button to SaveManager.DeleteSlot(int) (Assets/_Project/Scripts/Save/SaveManager.cs:693).
    ///
    /// IMPORTANT: this view is built at runtime by SaveSlotPanel — no scene wiring required.
    /// Thumbnails are cached as Texture2D and destroyed in OnDestroy.
    /// </summary>
    public class SaveSlotEntry : MonoBehaviour
    {
        // ── Cached UI refs (built at runtime via BuildVisuals) ───────────────
        RawImage _thumbnailImage;
        TMP_Text _titleText;
        TMP_Text _playTimeText;
        TMP_Text _shardsText;
        TMP_Text _buildingsText;
        TMP_Text _emptyLabel;
        Button _loadButton;
        Button _deleteButton;
        GameObject _populatedRoot;
        GameObject _emptyRoot;

        // ── State ───────────────────────────────────────────────────────────
        int _slotIndex = -1;
        Texture2D _ownedThumbnail; // owned by this entry, destroyed in OnDestroy

        // ── Callback wired by SaveSlotPanel ─────────────────────────────────
        Action<int> _onLoadRequested;
        Action<int> _onDeleteRequested;

        public int SlotIndex => _slotIndex;

        /// <summary>Build the runtime UI structure under this MonoBehaviour's transform.</summary>
        public void BuildVisuals(int slotIndex, Action<int> onLoad, Action<int> onDelete)
        {
            _slotIndex = slotIndex;
            _onLoadRequested = onLoad;
            _onDeleteRequested = onDelete;

            // Root layout: vertical layout group with background.
            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.13f, 0.18f, 0.95f);

            var rt = gameObject.GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(420f, 220f);

            var vlg = gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 6f;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // ── Populated root (shown when slot has data) ─────────────────
            _populatedRoot = new GameObject("Populated", typeof(RectTransform), typeof(VerticalLayoutGroup));
            _populatedRoot.transform.SetParent(transform, false);
            var pvlg = _populatedRoot.GetComponent<VerticalLayoutGroup>();
            pvlg.spacing = 4f;
            pvlg.childForceExpandWidth = true;
            pvlg.childForceExpandHeight = false;
            pvlg.childControlWidth = true;
            pvlg.childControlHeight = true;

            // Thumbnail
            var thumbGo = new GameObject("Thumbnail", typeof(RectTransform), typeof(RawImage), typeof(LayoutElement));
            thumbGo.transform.SetParent(_populatedRoot.transform, false);
            _thumbnailImage = thumbGo.GetComponent<RawImage>();
            _thumbnailImage.color = Color.white;
            var thumbLE = thumbGo.GetComponent<LayoutElement>();
            thumbLE.minHeight = 100f;
            thumbLE.preferredHeight = 100f;

            _titleText = CreateTextChild(_populatedRoot.transform, "Title", 14, FontStyles.Bold);
            _playTimeText = CreateTextChild(_populatedRoot.transform, "PlayTime", 12, FontStyles.Normal);
            _shardsText = CreateTextChild(_populatedRoot.transform, "Shards", 12, FontStyles.Normal);
            _buildingsText = CreateTextChild(_populatedRoot.transform, "Buildings", 12, FontStyles.Normal);

            // Button row
            var btnRow = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            btnRow.transform.SetParent(_populatedRoot.transform, false);
            var hlg = btnRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            btnRow.GetComponent<LayoutElement>().minHeight = 32f;

            _loadButton = CreateButtonChild(btnRow.transform, "LoadBtn", "LOAD", new Color(0.2f, 0.5f, 0.8f, 1f));
            _deleteButton = CreateButtonChild(btnRow.transform, "DeleteBtn", "DELETE", new Color(0.6f, 0.18f, 0.18f, 1f));
            _loadButton.onClick.AddListener(OnLoadClicked);
            _deleteButton.onClick.AddListener(OnDeleteClicked);

            // ── Empty root (shown when no save exists) ────────────────────
            _emptyRoot = new GameObject("Empty", typeof(RectTransform), typeof(VerticalLayoutGroup));
            _emptyRoot.transform.SetParent(transform, false);
            _emptyLabel = CreateTextChild(_emptyRoot.transform, "EmptyLabel", 14, FontStyles.Italic);
            _emptyLabel.alignment = TextAlignmentOptions.Center;
            _emptyLabel.text = $"-- Empty Slot {_slotIndex} --";
        }

        TMP_Text CreateTextChild(Transform parent, string name, float fontSize, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = Color.white;
            t.text = string.Empty;
            return t;
        }

        Button CreateButtonChild(Transform parent, string name, string label, Color bgColor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = bgColor;
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var labelText = labelGo.GetComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;
            labelText.fontSize = 14;
            labelText.fontStyle = FontStyles.Bold;
            return go.GetComponent<Button>();
        }

        /// <summary>
        /// Populate the card from SaveSlotInfo + extra fields. Caller (SaveSlotPanel) reads the
        /// canonical SaveManager.GetSaveInfo(slot) and an auxiliary metadata sidecar for shards / moon name / buildings.
        /// </summary>
        public void Populate(SaveSlotInfo info, SaveSlotMetadata extra, Texture2D thumbnail)
        {
            if (!info.exists)
            {
                _populatedRoot.SetActive(false);
                _emptyRoot.SetActive(true);
                _emptyLabel.text = $"-- Empty Slot {_slotIndex} --";
                ApplyThumbnail(null);
                return;
            }

            _populatedRoot.SetActive(true);
            _emptyRoot.SetActive(false);

            string moonName = extra != null && !string.IsNullOrEmpty(extra.moonName)
                ? extra.moonName
                : $"Moon {Mathf.Max(1, extra?.currentMoon ?? 1)}";

            string iso = string.IsNullOrEmpty(info.modifiedUtc) ? "?" : info.modifiedUtc;
            _titleText.text = $"Slot {info.slot} - {moonName}  [{iso}]";

            _playTimeText.text = $"Play Time: {FormatHMS(info.playTimeSeconds)}";

            int shards = extra?.resonanceShards ?? 0;
            _shardsText.text = $"Resonance Shards: {shards}";

            int buildings = extra?.buildingsRestored ?? 0;
            _buildingsText.text = $"Buildings Restored: {buildings}";

            ApplyThumbnail(thumbnail);
        }

        void ApplyThumbnail(Texture2D tex)
        {
            // Dispose previous owned thumbnail before swapping (Texture2D dispose pattern).
            if (_ownedThumbnail != null && _ownedThumbnail != tex)
            {
                Destroy(_ownedThumbnail);
                _ownedThumbnail = null;
            }
            _ownedThumbnail = tex;
            if (_thumbnailImage != null)
                _thumbnailImage.texture = tex;
        }

        void OnDestroy()
        {
            // Texture2D cleanup
            if (_ownedThumbnail != null)
            {
                Destroy(_ownedThumbnail);
                _ownedThumbnail = null;
            }
        }

        void OnLoadClicked()
        {
            if (_slotIndex < 0)
            {
                Debug.LogWarning($"[SaveSlotEntry] OnLoadClicked called with invalid slot index {_slotIndex}");
                return;
            }
            _onLoadRequested?.Invoke(_slotIndex);
        }

        void OnDeleteClicked()
        {
            if (_slotIndex < 0)
            {
                Debug.LogWarning($"[SaveSlotEntry] OnDeleteClicked called with invalid slot index {_slotIndex}");
                return;
            }
            _onDeleteRequested?.Invoke(_slotIndex);
        }

        static string FormatHMS(float totalSeconds)
        {
            if (totalSeconds < 0f) totalSeconds = 0f;
            var ts = TimeSpan.FromSeconds(totalSeconds);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }

    /// <summary>
    /// Extra per-slot metadata sidecar stored next to save file as JSON.
    /// Mirrors the fields SaveSlotInfo does NOT expose
    /// (SaveSlotInfo at Assets/_Project/Scripts/Save/SaveData.cs:930 only has
    /// slot / exists / createdUtc / modifiedUtc / playTimeSeconds / schemaVersion / gameVersion).
    /// Sidecar path: {persistentDataPath}/saves/slot{N}.meta.json
    /// </summary>
    [Serializable]
    public class SaveSlotMetadata
    {
        public int slot;
        public string moonName;
        public int currentMoon;
        public int resonanceShards;    // SaveData.economy.aetherShards (Assets/_Project/Scripts/Save/SaveData.cs:455)
        public int buildingsRestored;  // SaveData.header.buildingsRestored (Assets/_Project/Scripts/Save/SaveData.cs:238)
        public string capturedUtc;
    }
}