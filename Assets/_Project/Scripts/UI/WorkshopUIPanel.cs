using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// Workshop UI Panel -- displays building upgrade tiers, requirements,
    /// and allows the player to upgrade buildings.
    ///
    /// Data-driven: receives display data via SetBuildings() / RefreshBuilding().
    /// Fires OnUpgradeRequested event -- Integration layer wires to WorkshopSystem.
    /// </summary>
    public class WorkshopUIPanel : MonoBehaviour
    {
        public static WorkshopUIPanel Instance { get; private set; }

        [Header("Panel Root")]
        [SerializeField] GameObject panelRoot;

        [Header("Building List")]
        [SerializeField] RectTransform buildingListContainer;
        [SerializeField] GameObject buildingEntryPrefab;

        [Header("Detail View")]
        [SerializeField] TMPro.TextMeshProUGUI buildingNameText;
        [SerializeField] TMPro.TextMeshProUGUI currentTierText;
        [SerializeField] TMPro.TextMeshProUGUI nextTierText;
        [SerializeField] TMPro.TextMeshProUGUI rsRequirementText;
        [SerializeField] TMPro.TextMeshProUGUI outputMultiplierText;
        [SerializeField] TMPro.TextMeshProUGUI descriptionText;
        [SerializeField] Image tierProgressBar;
        [SerializeField] Button upgradeButton;
        [SerializeField] TMPro.TextMeshProUGUI upgradeButtonText;

        [Header("Colors")]
        [SerializeField] Color canUpgradeColor = new(0.2f, 0.8f, 0.3f);
        [SerializeField] Color cannotUpgradeColor = new(0.5f, 0.5f, 0.5f);
        [SerializeField] Color maxTierColor = new(0.9f, 0.85f, 0.3f);

        /// <summary>Fired when player clicks Upgrade. Payload = buildingId.</summary>
        public event Action<string> OnUpgradeRequested;

        readonly List<BuildingDisplayData> _buildings = new();
        readonly Dictionary<string, GameObject> _entryObjects = new();
        readonly List<GameObject> _entryPool = new();
        string _selectedBuildingId;
        bool _isOpen;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(HandleUpgradeClick);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Public API (called from Integration layer) ──────

        /// <summary>
        /// Replace the full building list. Called when panel opens or zone changes.
        /// </summary>
        public void SetBuildings(List<BuildingDisplayData> buildings)
        {
            _buildings.Clear();
            _buildings.AddRange(buildings);
            RebuildList();

            if (_buildings.Count > 0)
                SelectBuilding(_buildings[0].buildingId);
        }

        /// <summary>
        /// Refresh a single building's data (after upgrade).
        /// </summary>
        public void RefreshBuilding(BuildingDisplayData updated)
        {
            for (int i = 0; i < _buildings.Count; i++)
            {
                if (_buildings[i].buildingId == updated.buildingId)
                {
                    _buildings[i] = updated;
                    break;
                }
            }

            if (_selectedBuildingId == updated.buildingId)
                ShowDetail(updated);
        }

        public void Open()
        {
            _isOpen = true;
            if (panelRoot != null) panelRoot.SetActive(true);
        }

        public void Close()
        {
            _isOpen = false;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void Toggle()
        {
            if (_isOpen) Close(); else Open();
        }

        public bool IsOpen => _isOpen;

        // ─── Internal ────────────────────────────────

        void RebuildList()
        {
            // Deactivate all pooled entries
            foreach (var kvp in _entryObjects)
            {
                if (kvp.Value != null) kvp.Value.SetActive(false);
            }
            _entryObjects.Clear();

            if (buildingListContainer == null || buildingEntryPrefab == null) return;

            int poolIdx = 0;
            foreach (var data in _buildings)
            {
                GameObject entry;
                if (poolIdx < _entryPool.Count && _entryPool[poolIdx] != null)
                {
                    entry = _entryPool[poolIdx];
                    entry.SetActive(true);
                }
                else
                {
                    entry = Instantiate(buildingEntryPrefab, buildingListContainer);
                    if (poolIdx < _entryPool.Count)
                        _entryPool[poolIdx] = entry;
                    else
                        _entryPool.Add(entry);
                }
                poolIdx++;

                UpdateEntryVisuals(entry, data);
                _entryObjects[data.buildingId] = entry;
            }
        }

        void UpdateEntryVisuals(GameObject entry, BuildingDisplayData data)
        {
            entry.SetActive(true);

            var label = entry.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (label != null)
                label.text = $"{data.buildingName}  [Tier {data.currentTier}]";

            var img = entry.GetComponent<Image>();
            if (img != null)
                img.color = data.isMaxTier ? maxTierColor : (data.canUpgrade ? canUpgradeColor : cannotUpgradeColor);

            var btn = entry.GetComponent<Button>();
            if (btn == null) btn = entry.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            string capturedId = data.buildingId;
            btn.onClick.AddListener(() => SelectBuilding(capturedId));
        }

        void SelectBuilding(string buildingId)
        {
            _selectedBuildingId = buildingId;
            foreach (var data in _buildings)
            {
                if (data.buildingId == buildingId)
                {
                    ShowDetail(data);
                    return;
                }
            }
        }

        void ShowDetail(BuildingDisplayData data)
        {
            if (buildingNameText != null)
                buildingNameText.text = data.buildingName;

            if (currentTierText != null)
                currentTierText.text = $"Current: Tier {data.currentTier} -- {data.currentTierName}";

            if (nextTierText != null)
            {
                nextTierText.text = data.isMaxTier
                    ? "PERFECTED -- Maximum tier reached"
                    : $"Next: Tier {data.currentTier + 1} -- {data.nextTierName}";
            }

            if (rsRequirementText != null)
            {
                rsRequirementText.text = data.isMaxTier
                    ? ""
                    : $"Requires RS: {data.rsRequired:F0}  (Current: {data.currentRS:F0})";
            }

            if (outputMultiplierText != null)
                outputMultiplierText.text = $"Output: x{data.outputMultiplier:F2}";

            if (descriptionText != null)
                descriptionText.text = data.nextTierDescription;

            if (tierProgressBar != null)
                tierProgressBar.fillAmount = data.maxTier > 0
                    ? data.currentTier / (float)data.maxTier
                    : 1f;

            // Upgrade button state
            if (upgradeButton != null)
            {
                upgradeButton.interactable = data.canUpgrade;
                var colors = upgradeButton.colors;
                colors.normalColor = data.canUpgrade ? canUpgradeColor : cannotUpgradeColor;
                upgradeButton.colors = colors;
            }

            if (upgradeButtonText != null)
            {
                upgradeButtonText.text = data.isMaxTier ? "MAX TIER" : "UPGRADE";
            }
        }

        void HandleUpgradeClick()
        {
            if (!string.IsNullOrEmpty(_selectedBuildingId))
                OnUpgradeRequested?.Invoke(_selectedBuildingId);
        }
    }

    /// <summary>
    /// Data struct passed from Integration to Workshop UI. No dependency on WorkshopSystem.
    /// </summary>
    [Serializable]
    public struct BuildingDisplayData
    {
        public string buildingId;
        public string buildingName;
        public int currentTier;
        public int maxTier;
        public string currentTierName;
        public string nextTierName;
        public string nextTierDescription;
        public float rsRequired;
        public float currentRS;
        public float outputMultiplier;
        public bool canUpgrade;
        public bool isMaxTier;
    }
}
