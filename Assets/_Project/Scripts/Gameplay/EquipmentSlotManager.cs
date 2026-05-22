using UnityEngine;
using System.Collections.Generic;
using Tartaria.Data;
using Tartaria.Save;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// EquipmentSlotManager — manages player equipment slots + stat bonuses.
    /// 6 slots: Weapon, Armor, Helmet, Gloves, Boots, Accessory.
    /// Each item provides stat bonuses (STR, AGI, VIT, etc) + special effects.
    /// Integrates with InventorySystem for item equip/unequip.
    /// 
    /// Equipment Stats:
    /// - Base stats: +STR, +AGI, +VIT, +RES, +ATT
    /// - Armor value: damage reduction
    /// - Special effects: +crit chance, +movement speed, +RS regen
    /// 
    /// Equipment System:
    /// - Equip item from inventory → apply bonuses
    /// - Unequip → remove bonuses, return to inventory
    /// - Display equipped items in Character UI
    /// - Visual updates (change player mesh/materials)
    /// 
    /// Save Integration:
    /// - Implements ISaveDataProvider (v17 modular extensibility)
    /// - Equipment state persists via itemID references
    /// - Auto-loads assets from Resources/Equipment/ on restore
    /// 
    /// Usage:
    /// - EquipmentSlotManager.Instance.EquipItem(EquipSlot.Weapon, itemData)
    /// - EquipmentSlotManager.Instance.UnequipSlot(EquipSlot.Weapon)
    /// - Subscribe to OnEquipmentChanged for UI refresh
    /// 
    /// GDD refs: §07 (Equipment System), §06 (Character Stats)
    /// </summary>
    public class EquipmentSlotManager : MonoBehaviour, ISaveDataProvider
    {
        public static EquipmentSlotManager Instance { get; private set; }

        [Header("Equipment Slots")]
        [SerializeField] EquipmentItemData weaponSlot;
        [SerializeField] EquipmentItemData armorSlot;
        [SerializeField] EquipmentItemData helmetSlot;
        [SerializeField] EquipmentItemData glovesSlot;
        [SerializeField] EquipmentItemData bootsSlot;
        [SerializeField] EquipmentItemData accessorySlot;

        public event System.Action<EquipSlot> OnEquipmentChanged;

        Dictionary<EquipSlot, EquipmentItemData> _equippedItems = new();

        // Cached total stats
        int _totalStrength = 0;
        int _totalAgility = 0;
        int _totalVitality = 0;
        int _totalResonance = 0;
        int _totalAttunement = 0;
        int _totalArmor = 0;

        public int TotalStrength => _totalStrength;
        public int TotalAgility => _totalAgility;
        public int TotalVitality => _totalVitality;
        public int TotalResonance => _totalResonance;
        public int TotalAttunement => _totalAttunement;
        public int TotalArmor => _totalArmor;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize slots
            _equippedItems[EquipSlot.Weapon] = weaponSlot;
            _equippedItems[EquipSlot.Armor] = armorSlot;
            _equippedItems[EquipSlot.Helmet] = helmetSlot;
            _equippedItems[EquipSlot.Gloves] = glovesSlot;
            _equippedItems[EquipSlot.Boots] = bootsSlot;
            _equippedItems[EquipSlot.Accessory] = accessorySlot;

            RecalculateStats();

            // Register with SaveManager (ISaveDataProvider pattern)
            SaveManager.Instance?.RegisterProvider(this);
        }

        void OnDestroy()
        {
            SaveManager.Instance?.UnregisterProvider(this);
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Equip item in slot.
        /// </summary>
        public bool EquipItem(EquipSlot slot, EquipmentItemData item)
        {
            if (item == null)
            {
                Debug.LogWarning("[EquipmentSlot] Cannot equip null item");
                return false;
            }

            // Check if slot matches item type
            if (item.slot != slot)
            {
                Debug.LogWarning($"[EquipmentSlot] Item '{item.itemName}' cannot be equipped in {slot} slot");
                return false;
            }

            // Unequip existing item if present
            if (_equippedItems[slot] != null)
            {
                UnequipSlot(slot);
            }

            // Equip new item
            _equippedItems[slot] = item;

            Debug.Log($"[EquipmentSlot] Equipped '{item.itemName}' in {slot} slot");

            RecalculateStats();
            OnEquipmentChanged?.Invoke(slot);

            // Update visual (change player mesh)
            // Note: Equipment visual system requires character model renderer integration

            return true;
        }

        /// <summary>
        /// Unequip item from slot.
        /// </summary>
        public bool UnequipSlot(EquipSlot slot)
        {
            var item = _equippedItems[slot];

            if (item == null)
            {
                Debug.LogWarning($"[EquipmentSlot] No item equipped in {slot} slot");
                return false;
            }

            Debug.Log($"[EquipmentSlot] Unequipped '{item.itemName}' from {slot} slot");

            _equippedItems[slot] = null;

            RecalculateStats();
            OnEquipmentChanged?.Invoke(slot);

            // Return item to inventory
            InventorySystem.Instance?.AddItem(item.itemID, 1);

            return true;
        }

        /// <summary>
        /// Get equipped item in slot.
        /// </summary>
        public EquipmentItemData GetEquippedItem(EquipSlot slot)
        {
            return _equippedItems.GetValueOrDefault(slot, null);
        }

        /// <summary>
        /// Recalculate total stats from all equipped items.
        /// </summary>
        void RecalculateStats()
        {
            _totalStrength = 0;
            _totalAgility = 0;
            _totalVitality = 0;
            _totalResonance = 0;
            _totalAttunement = 0;
            _totalArmor = 0;

            foreach (var item in _equippedItems.Values)
            {
                if (item == null) continue;

                _totalStrength += item.strengthBonus;
                _totalAgility += item.agilityBonus;
                _totalVitality += item.vitalityBonus;
                _totalResonance += item.resonanceBonus;
                _totalAttunement += item.attunementBonus;
                _totalArmor += item.armorValue;
            }

            Debug.Log($"[EquipmentSlot] Stats: STR {_totalStrength}, AGI {_totalAgility}, VIT {_totalVitality}, RES {_totalResonance}, ATT {_totalAttunement}, ARM {_totalArmor}");
        }

        /// <summary>
        /// Unequip all items.
        /// </summary>
        public void UnequipAll()
        {
            foreach (var slot in System.Enum.GetValues(typeof(EquipSlot)))
            {
                UnequipSlot((EquipSlot)slot);
            }

            Debug.Log("[EquipmentSlot] Unequipped all items");
        }

        // ═══════════════════════════════════════════════════════════════
        // ISaveDataProvider Implementation (v17 modular save pattern)
        // ═══════════════════════════════════════════════════════════════

        public string GetProviderKey() => "EquipmentSlotManager";

        public object GetSaveData()
        {
            return new EquipmentSaveData
            {
                weaponSlotItemID = _equippedItems[EquipSlot.Weapon]?.itemID,
                armorSlotItemID = _equippedItems[EquipSlot.Armor]?.itemID,
                helmetSlotItemID = _equippedItems[EquipSlot.Helmet]?.itemID,
                glovesSlotItemID = _equippedItems[EquipSlot.Gloves]?.itemID,
                bootsSlotItemID = _equippedItems[EquipSlot.Boots]?.itemID,
                accessorySlotItemID = _equippedItems[EquipSlot.Accessory]?.itemID
            };
        }

        public void RestoreSaveData(object data)
        {
            if (data is not EquipmentSaveData equipData)
            {
                Debug.LogWarning("[EquipmentSlot] Invalid save data type — expected EquipmentSaveData");
                return;
            }

            // Clear current equipment
            _equippedItems[EquipSlot.Weapon] = null;
            _equippedItems[EquipSlot.Armor] = null;
            _equippedItems[EquipSlot.Helmet] = null;
            _equippedItems[EquipSlot.Gloves] = null;
            _equippedItems[EquipSlot.Boots] = null;
            _equippedItems[EquipSlot.Accessory] = null;

            // Load equipment by itemID
            if (!string.IsNullOrEmpty(equipData.weaponSlotItemID))
                _equippedItems[EquipSlot.Weapon] = LoadEquipmentByID(equipData.weaponSlotItemID);

            if (!string.IsNullOrEmpty(equipData.armorSlotItemID))
                _equippedItems[EquipSlot.Armor] = LoadEquipmentByID(equipData.armorSlotItemID);

            if (!string.IsNullOrEmpty(equipData.helmetSlotItemID))
                _equippedItems[EquipSlot.Helmet] = LoadEquipmentByID(equipData.helmetSlotItemID);

            if (!string.IsNullOrEmpty(equipData.glovesSlotItemID))
                _equippedItems[EquipSlot.Gloves] = LoadEquipmentByID(equipData.glovesSlotItemID);

            if (!string.IsNullOrEmpty(equipData.bootsSlotItemID))
                _equippedItems[EquipSlot.Boots] = LoadEquipmentByID(equipData.bootsSlotItemID);

            if (!string.IsNullOrEmpty(equipData.accessorySlotItemID))
                _equippedItems[EquipSlot.Accessory] = LoadEquipmentByID(equipData.accessorySlotItemID);

            RecalculateStats();
            Debug.Log("[EquipmentSlot] Equipment state restored from save");
        }

        /// <summary>
        /// Load EquipmentItemData asset by itemID.
        /// Searches Resources/Equipment/ for matching asset.
        /// Falls back to Resources root if not found in Equipment folder.
        /// </summary>
        EquipmentItemData LoadEquipmentByID(string itemID)
        {
            if (string.IsNullOrEmpty(itemID))
                return null;

            // Try loading from Resources/Equipment/ first
            var item = Resources.Load<EquipmentItemData>($"Equipment/{itemID}");

            if (item == null)
            {
                // Fallback: search root Resources
                item = Resources.Load<EquipmentItemData>(itemID);
            }

            if (item == null)
            {
                Debug.LogWarning($"[EquipmentSlot] Failed to load equipment '{itemID}' from Resources");
            }

            return item;
        }

        /// <summary>
        /// Serializable equipment save data (ISaveDataProvider pattern).
        /// </summary>
        [System.Serializable]
        class EquipmentSaveData
        {
            public string weaponSlotItemID;
            public string armorSlotItemID;
            public string helmetSlotItemID;
            public string glovesSlotItemID;
            public string bootsSlotItemID;
            public string accessorySlotItemID;
        }
    }
}
