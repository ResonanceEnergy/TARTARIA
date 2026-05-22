using UnityEngine;
using System.Collections.Generic;

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
    /// Usage:
    /// - EquipmentSlotManager.Instance.EquipItem(EquipSlot.Weapon, itemData)
    /// - EquipmentSlotManager.Instance.UnequipSlot(EquipSlot.Weapon)
    /// - Subscribe to OnEquipmentChanged for UI refresh
    /// 
    /// GDD refs: §07 (Equipment System), §06 (Character Stats)
    /// </summary>
    public class EquipmentSlotManager : MonoBehaviour
    {
        public static EquipmentSlotManager Instance { get; private set; }

        [Header("Equipment Slots")]
        [SerializeField] EquipmentItem weaponSlot;
        [SerializeField] EquipmentItem armorSlot;
        [SerializeField] EquipmentItem helmetSlot;
        [SerializeField] EquipmentItem glovesSlot;
        [SerializeField] EquipmentItem bootsSlot;
        [SerializeField] EquipmentItem accessorySlot;

        public event System.Action<EquipSlot> OnEquipmentChanged;

        Dictionary<EquipSlot, EquipmentItem> _equippedItems = new();

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
        }

        /// <summary>
        /// Equip item in slot.
        /// </summary>
        public bool EquipItem(EquipSlot slot, EquipmentItem item)
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
        public EquipmentItem GetEquippedItem(EquipSlot slot)
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

        public enum EquipSlot : byte
        {
            Weapon = 0,
            Armor = 1,
            Helmet = 2,
            Gloves = 3,
            Boots = 4,
            Accessory = 5
        }
    }

    /// <summary>
    /// Equipment item data (ScriptableObject or class).
    /// </summary>
    [System.Serializable]
    public class EquipmentItem
    {
        public string itemID;
        public string itemName;
        public EquipmentSlotManager.EquipSlot slot;

        [Header("Stats")]
        public int strengthBonus;
        public int agilityBonus;
        public int vitalityBonus;
        public int resonanceBonus;
        public int attunementBonus;
        public int armorValue;

        [Header("Visual")]
        public GameObject meshPrefab;  // Visual representation
        public Sprite icon;
    }
}
