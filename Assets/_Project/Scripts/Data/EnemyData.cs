using UnityEngine;
using System;
using System.Collections.Generic;

namespace Tartaria.Data
{
    /// <summary>
    /// Enemy Data — ScriptableObject definition for enemy types.
    /// Stores all enemy metadata: stats, behavior, loot, visuals.
    ///
    /// Create assets via: Assets → Create → Tartaria → Enemy Data
    /// Place in: Assets/_Project/Resources/Enemies/
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Tartaria/Enemy Data", order = 105)]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier (e.g., 'golem_worker', 'echo_phantom')")]
        public string enemyID;

        [Tooltip("Display name shown in UI")]
        public string displayName;

        [TextArea(3, 6)]
        [Tooltip("Lore description")]
        public string description;

        [Header("Visuals")]
        [Tooltip("Enemy prefab for spawning")]
        public GameObject prefab;

        [Tooltip("Icon for UI display (bestiary, death notifications)")]
        public Sprite icon;

        [Header("Stats")]
        [Tooltip("Maximum health points")]
        [Range(10f, 10000f)]
        public float maxHealth = 100f;

        [Tooltip("Movement speed (m/s)")]
        [Range(0f, 20f)]
        public float moveSpeed = 3.5f;

        [Tooltip("Attack damage per hit")]
        [Range(1f, 500f)]
        public float attackDamage = 10f;

        [Tooltip("Attack range (meters)")]
        [Range(1f, 50f)]
        public float attackRange = 5f;

        [Tooltip("Attack cooldown (seconds)")]
        [Range(0.1f, 10f)]
        public float attackCooldown = 2f;

        [Tooltip("Detection range (meters)")]
        [Range(1f, 100f)]
        public float detectionRange = 15f;

        [Header("Combat Behavior")]
        [Tooltip("Enemy AI archetype")]
        public EnemyArchetype archetype = EnemyArchetype.Melee;

        [Tooltip("Special abilities (e.g., 'Shield Bash', 'Teleport', 'Summon Minions')")]
        public List<string> specialAbilities = new();

        [Tooltip("Damage resistances (% reduction)")]
        public DamageResistances resistances = new();

        [Header("Loot & Rewards")]
        [Tooltip("RS awarded on defeat")]
        [Range(0f, 1000f)]
        public float rsReward = 10f;

        [Tooltip("XP awarded on defeat")]
        [Range(0, 10000)]
        public int xpReward = 50;

        [Tooltip("Loot table: item IDs with drop chances (0-1)")]
        public List<LootDrop> lootTable = new();

        [Header("Spawn Settings")]
        [Tooltip("Moon IDs where this enemy can spawn (e.g., 1, 2, 3)")]
        public List<int> spawnMoons = new();

        [Tooltip("Minimum player level to encounter this enemy")]
        [Range(1, 50)]
        public int minPlayerLevel = 1;

        [Header("Audio")]
        [Tooltip("Attack sound effect")]
        public AudioClip attackSound;

        [Tooltip("Death sound effect")]
        public AudioClip deathSound;

        [Tooltip("Idle/ambient sound effect")]
        public AudioClip ambientSound;

        /// <summary>
        /// Validates enemy data on asset creation/edit.
        /// </summary>
        void OnValidate()
        {
            // Ensure enemyID is lowercase with underscores
            if (!string.IsNullOrWhiteSpace(enemyID))
            {
                enemyID = enemyID.ToLower().Replace(" ", "_");
            }

            // Ensure at least one spawn moon
            if (spawnMoons.Count == 0)
            {
                Debug.LogWarning($"[EnemyData] {displayName} has no spawn moons defined", this);
            }

            // Validate loot drop chances
            foreach (var loot in lootTable)
            {
                if (loot.dropChance < 0f || loot.dropChance > 1f)
                {
                    Debug.LogWarning($"[EnemyData] {displayName} loot '{loot.itemID}' has invalid drop chance {loot.dropChance}", this);
                }
            }
        }

        /// <summary>
        /// Get formatted stat summary for tooltips.
        /// </summary>
        public string GetStatSummary()
        {
            return $"HP: {maxHealth:F0} | ATK: {attackDamage:F0} | SPD: {moveSpeed:F1} m/s\n" +
                   $"Range: {attackRange:F1}m | Cooldown: {attackCooldown:F1}s";
        }
    }

    [Serializable]
    public struct LootDrop
    {
        [Tooltip("Item ID to drop")]
        public string itemID;

        [Tooltip("Drop chance (0 = never, 1 = always)")]
        [Range(0f, 1f)]
        public float dropChance;

        [Tooltip("Min quantity if dropped")]
        public int minQuantity;

        [Tooltip("Max quantity if dropped")]
        public int maxQuantity;
    }

    [Serializable]
    public struct DamageResistances
    {
        [Range(-100f, 100f)]
        [Tooltip("Physical damage resistance (%)")]
        public float physical;

        [Range(-100f, 100f)]
        [Tooltip("Resonance damage resistance (%)")]
        public float resonance;

        [Range(-100f, 100f)]
        [Tooltip("Environmental damage resistance (%)")]
        public float environmental;
    }

    public enum EnemyArchetype
    {
        Melee,          // Close-range attacker
        Ranged,         // Long-range attacker
        Tank,           // High HP, slow, heavy damage
        Swarm,          // Weak individually, dangerous in groups
        Elite,          // Balanced stats, special abilities
        Boss,           // High HP, phases, special mechanics
        Support,        // Buffs allies, debuffs player
        Caster          // Magic attacks, vulnerable to interrupts
    }

    public enum ItemCategory
    {
        Material,
        Consumable,
        KeyItem,
        Currency,
        QuestItem,
        Misc
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic,         // Red/Crimson
        Ascendant       // Cyan/White (Moon 13 endgame)
    }

    // EquipSlot enum defined in EquipmentItemData.cs (Phase 8)
    // Duplicate definition removed to prevent CS0101 conflict
    // Use: Tartaria.Data.EquipSlot
}
