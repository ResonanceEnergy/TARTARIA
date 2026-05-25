using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Centralized game balance configuration for economy, crafting, and loot systems.
    /// All numeric tuning values should be referenced from this singleton instead of hardcoded.
    ///
    /// Design per Agent-13 refactor (eliminates magic numbers across economy/crafting/loot).
    /// </summary>
    [DisallowMultipleComponent]
    public class GameBalanceConfig : MonoBehaviour
    {
        public static GameBalanceConfig Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("GameBalanceConfig");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<GameBalanceConfig>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Economy System ───

        [Header("Economy - Income")]
        [Tooltip("Seconds between passive building income ticks")]
        public float incomeTickInterval = 10f;

        [Tooltip("RS 0-100 maps to 1.0-2.0× income, this is the divisor")]
        public float rsMultiplierScaling = 100f;

        [Tooltip("Minimum moon multiplier clamp")]
        public float minMoonMultiplier = 0.1f;

        [Tooltip("Building level income scaling factor (diminishing returns)")]
        public float buildingLevelScaling = 0.5f;

        [Header("Economy - Building Restoration")]
        public int restoreBuildingTier1 = 50;
        public int restoreBuildingTier2 = 150;
        public int restoreBuildingTier3 = 400;

        [Header("Economy - Workshop Upgrades")]
        public int workshopUpgrade1 = 100;
        public int workshopUpgrade2 = 250;
        public int workshopUpgrade3 = 500;

        [Header("Economy - Skill Unlocks")]
        public int skillTier1 = 75;
        public int skillTier2 = 200;
        public int skillTier3 = 350;
        public int skillTier4 = 500;
        public int skillTier5 = 750;

        [Header("Economy - Consumable Costs")]
        public int repairKitCost = 30;
        public int aetherPotionCost = 50;
        public int rsBoosterCost = 80;

        // ─── Crafting System ───

        [Header("Crafting - Consumable Effects")]
        [Tooltip("HP restored by repair kit")]
        public float repairKitHealAmount = 30f;

        [Tooltip("Aether charge restored by aether potion")]
        public float aetherPotionChargeAmount = 50f;

        [Tooltip("Duration in seconds for resonance amplifier buff")]
        public float resonanceAmplifierDuration = 60f;

        // ─── Loot Dropper ───

        [Header("Loot - Visual")]
        [Tooltip("Scale of loot pickup cubes")]
        public float lootCubeScale = 0.35f;

        [Tooltip("Emission color brightness multiplier")]
        public float lootEmissionMultiplier = 2.5f;

        [Tooltip("Loot cube rotation speed (degrees per second)")]
        public float lootRotationSpeed = 90f;

        [Tooltip("Base hover height offset")]
        public float lootHoverOffset = 0.25f;

        [Tooltip("Hover sine wave amplitude")]
        public float lootHoverAmplitude = 0.1f;

        [Tooltip("Hover sine wave frequency")]
        public float lootHoverFrequency = 2.5f;

        [Header("Loot - Lifecycle")]
        [Tooltip("Seconds before uncollected loot despawns")]
        public float lootDespawnTime = 60f;

        [Tooltip("Seconds before loot spawn VFX is destroyed")]
        public float lootVFXDuration = 2f;

        // ─── UI Systems (Agent 14) ───

        [Header("UI - Timing")]
        [Tooltip("Fade duration for UI elements (notifications, overlays)")]
        public float uiFadeDuration = 0.3f;

        [Tooltip("Slide-in animation duration for notifications")]
        public float uiSlideInDuration = 0.3f;

        [Tooltip("How long to show accessibility hints before hiding")]
        public float accessibilityHintDuration = 4.5f;

        [Tooltip("Giant ready flash cycle duration")]
        public float giantReadyFlashDuration = 1.2f;

        [Header("UI - Damage Numbers")]
        [Tooltip("Duration of damage number rise animation")]
        public float damageNumberAnimationDuration = 0.8f;

        [Tooltip("Font size for critical hit numbers")]
        public float damageCritFontSize = 4.5f;

        [Tooltip("Font size multiplier for heal numbers")]
        public float healFontSizeMultiplier = 1.2f;

        [Header("UI - Notification Colors")]
        public Color notificationInfoColor = new Color(0.3f, 0.7f, 1f);
        public Color notificationSuccessColor = new Color(0.3f, 1f, 0.4f);
        public Color notificationWarningColor = new Color(1f, 0.9f, 0.3f);
        public Color notificationErrorColor = new Color(1f, 0.3f, 0.3f);

        [Header("UI - HUD Colors")]
        public Color bossHealthColor = new Color(0.8f, 0.15f, 0.1f);
        public Color bossHealthLowColor = new Color(0.9f, 0.3f, 0.05f);
        public Color wheelBaseColor = new Color(0.4f, 0.7f, 0.9f);
        public Color meterReadyColor = new Color(1f, 0.85f, 0.2f);
        public Color abilityCooldownGrayColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);

        [Header("UI - Damage Number Colors")]
        public Color damageHealColor = new Color(0.3f, 1f, 0.3f, 1f);

        [Header("UI - Thresholds")]
        [Tooltip("Readiness threshold for giant mode and abilities (0.99 = 99%)")]
        [Range(0f, 1f)]
        public float readyThreshold = 0.99f;

        [Tooltip("Boss health percentage threshold for low color (0.3 = 30%)")]
        [Range(0f, 1f)]
        public float bossHealthLowThreshold = 0.3f;

        [Header("UI - Notification System")]
        [Tooltip("Maximum visible toast notifications")]
        public int maxVisibleToasts = 5;

        [Tooltip("Duration to show toast notifications")]
        public float toastDuration = 3f;

        // ─── Combat & Enemy System ───

        [Header("Enemy - Mud Golem Stats")]
        [Tooltip("Mud Golem maximum health")]
        public int enemyMudGolemMaxHealth = 50;

        [Tooltip("Mud Golem melee attack damage")]
        public int enemyMudGolemMeleeDamage = 10;

        [Header("Enemy - Mud Golem Behavior")]
        [Tooltip("Patrol radius around spawn point")]
        public float enemyPatrolRadius = 20f;

        [Tooltip("Chase range to pursue player")]
        public float enemyChaseRange = 15f;

        [Tooltip("Attack range for melee")]
        public float enemyAttackRange = 3f;

        [Tooltip("Cooldown between attacks in seconds")]
        public float enemyAttackCooldown = 1.5f;

        [Tooltip("Wait time at patrol waypoint")]
        public float enemyPatrolWaitTime = 5f;

        [Header("Enemy - Mud Golem Perception")]
        [Tooltip("Visual perception range")]
        public float enemySightRange = 18f;

        [Tooltip("Field of view angle in degrees")]
        public float enemySightFOV = 90f;

        [Tooltip("How long to search after losing sight of player")]
        public float enemyLostTargetSearchDuration = 8f;

        [Header("Enemy - Mud Golem Combat")]
        [Tooltip("Attack telegraph/wind-up duration")]
        public float enemyTelegraphDuration = 0.5f;

        [Tooltip("Knockback lerp speed multiplier")]
        public float enemyKnockbackLerpSpeed = 5f;

        [Header("Enemy - Mud Golem Movement")]
        [Tooltip("Base movement speed (patrol)")]
        public float enemyMoveSpeed = 3f;

        [Tooltip("Chase movement speed")]
        public float enemyChaseSpeed = 5f;

        [Header("Enemy - Procedural Build Stats")]
        [Tooltip("NavMeshAgent speed for procedural enemies")]
        public float enemyNavAgentSpeed = 3.5f;

        [Tooltip("NavMeshAgent angular speed")]
        public float enemyNavAgentAngularSpeed = 240f;

        [Tooltip("NavMeshAgent acceleration")]
        public float enemyNavAgentAcceleration = 10f;

        [Tooltip("NavMeshAgent stopping distance")]
        public float enemyNavAgentStoppingDistance = 2.5f;

        [Tooltip("Rigidbody mass for procedural enemies")]
        public float enemyRigidbodyMass = 80f;

        [Header("Enemy - Spawner System")]
        [Tooltip("Wait time between waves")]
        public float spawnerTimeBetweenWaves = 10f;

        [Tooltip("Polling interval for wave clear check")]
        public float spawnerWaveClearCheckInterval = 0.5f;
    }
}
