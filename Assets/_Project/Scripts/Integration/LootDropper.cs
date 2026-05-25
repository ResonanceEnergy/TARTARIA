using UnityEngine;
using Tartaria.Gameplay;
using Tartaria.Core; // AGENT 28: Added for VFXPoolManager and MaterialPropertyBlockHelper
using Tartaria.Core.Enums; // AGENT 5: Added for ItemRarity enum

namespace Tartaria.Integration
{
    /// <summary>
    /// Day-11: spawns runtime loot pickups when enemies die.
    /// No prefab required — builds a glowing cube + PickupInteractable on the fly.
    /// Items rotate through a small drop table by deterministic count.
    /// Optional VFX: Set ShardCollectVFX prefab for vacuum effect on spawn.
    /// 
    /// AGENT 5 ENHANCEMENT: Tiered loot system for endgame (level 40-50).
    /// - SpawnTieredLoot() for level-appropriate drops (Epic/Legendary/Ascendant)
    /// - Boss-specific loot tables
    /// - Rarity-based color coding
    /// </summary>
    public static class LootDropper
    {
        static int _dropCount;

        /// <summary>
        /// Optional ShardCollect VFX prefab for loot spawn effect.
        /// Assign via LootDropper.ShardCollectVFX = yourPrefab in scene setup.
        /// </summary>
        public static GameObject ShardCollectVFX { get; set; }

        struct Drop { public string id; public string display; public Color color; public ItemRarity rarity; }
        
        // Basic loot table (pre-endgame)
        static readonly Drop[] Table =
        {
            new() { id = "aether_shard",     display = "Aether Shard",     color = new Color(0.45f, 0.85f, 1.0f), rarity = ItemRarity.Common },
            new() { id = "golem_core",       display = "Golem Core",       color = new Color(0.95f, 0.6f, 0.25f), rarity = ItemRarity.Uncommon },
            new() { id = "resonance_crystal",display = "Resonance Crystal",color = new Color(0.85f, 0.4f, 1.0f), rarity = ItemRarity.Rare },
        };

        // AGENT 5: Endgame loot tables (level 40-50)
        static readonly Drop[] EndgameTable =
        {
            // Epic (Moon 7-10): Purple glow
            new() { id = "harmonic_essence", display = "Harmonic Essence", color = new Color(0.7f, 0.3f, 1f), rarity = ItemRarity.Epic },
            new() { id = "void_shard",       display = "Void Shard",       color = new Color(0.5f, 0.1f, 0.8f), rarity = ItemRarity.Epic },
            
            // Legendary (Moon 10-12): Gold glow
            new() { id = "legendary_crystal",display = "Legendary Crystal",color = new Color(1f, 0.7f, 0.2f), rarity = ItemRarity.Legendary },
            new() { id = "aquifer_essence",  display = "Aquifer Essence",  color = new Color(0.3f, 0.9f, 1f), rarity = ItemRarity.Legendary },
            
            // Ascendant (Moon 13 only): Cyan/white glow
            new() { id = "ascendant_core",   display = "Ascendant Core",   color = new Color(0.9f, 1f, 1f), rarity = ItemRarity.Ascendant },
            new() { id = "true_history_relic",display = "True History Relic",color = new Color(1f, 0.95f, 0.8f), rarity = ItemRarity.Ascendant },
        };

        /// <summary>
        /// AGENT 5: Spawn tiered loot based on player level and boss type.
        /// Level 40-49: Epic (5%) + Legendary (1%)
        /// Level 50+: Epic (10%) + Legendary (3%) + Ascendant (1%)
        /// </summary>
        public static void SpawnTieredLoot(Vector3 position, int playerLevel, BossType bossType = BossType.CorruptionTitan)
        {
            ItemRarity rarity = ItemRarity.Common;
            
            if (playerLevel >= 50)
            {
                // Moon 13 + Post-game: Ascendant drops enabled
                float roll = Random.value;
                if (roll < 0.01f)       rarity = ItemRarity.Ascendant;  // 1% chance
                else if (roll < 0.04f)  rarity = ItemRarity.Legendary;  // 3% chance
                else if (roll < 0.14f)  rarity = ItemRarity.Epic;       // 10% chance
                else                    rarity = ItemRarity.Rare;        // 86% fallback
            }
            else if (playerLevel >= 40)
            {
                // Moons 10-12: Legendary + Epic enabled
                float roll = Random.value;
                if (roll < 0.01f)       rarity = ItemRarity.Legendary;  // 1% chance
                else if (roll < 0.06f)  rarity = ItemRarity.Epic;       // 5% chance
                else                    rarity = ItemRarity.Rare;        // 94% fallback
            }
            else
            {
                // Pre-endgame: Use basic loot table
                Spawn(position);
                return;
            }
            
            SpawnItemByRarity(position, rarity, bossType);
        }

        /// <summary>
        /// AGENT 5: Spawn item of specific rarity with boss-themed drops.
        /// </summary>
        static void SpawnItemByRarity(Vector3 position, ItemRarity rarity, BossType bossType)
        {
            Drop pick;
            
            // Select item based on rarity
            if (rarity == ItemRarity.Epic)
            {
                // Epic: Harmonic Essence or Void Shard
                pick = EndgameTable[Random.Range(0, 2)];
            }
            else if (rarity == ItemRarity.Legendary)
            {
                // Legendary: Boss-specific themed drops
                if (bossType == BossType.VoidArchitect)  // Aquifer Guardian
                    pick = EndgameTable[3];  // Aquifer Essence
                else
                    pick = EndgameTable[2];  // Legendary Crystal
            }
            else if (rarity == ItemRarity.Ascendant)
            {
                // Ascendant: True History Relic or Ascendant Core
                pick = EndgameTable[Random.Range(4, 6)];
            }
            else
            {
                // Fallback to basic table
                pick = Table[Random.Range(0, Table.Length)];
            }
            
            SpawnLootInternal(position, pick);
        }

        /// <summary>
        /// Original spawn logic (pre-endgame or basic loot).
        /// </summary>
        public static void Spawn(Vector3 position)
        {
            var pick = Table[_dropCount++ % Table.Length];
            SpawnLootInternal(position, pick);
        }

        /// <summary>
        /// AGENT 5: Internal loot spawn logic (shared by all spawn methods).
        /// </summary>
        static void SpawnLootInternal(Vector3 position, Drop pick)
        {
            // Build loot cube from components (no CreatePrimitive per primitive elimination mandate).
            var go = new GameObject($"Loot_{pick.id}");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.35f;
            
            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            go.AddComponent<MeshRenderer>();
            go.AddComponent<BoxCollider>();
            // Make collider a trigger so the player can walk through; PickupInteractable uses E.
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            var rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                var mat = new Material(sh) { color = pick.color };
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", pick.color * 2.5f);
                }
                rend.material = mat;
            }

            // Layer: try to put on Interactable layer (9) if it exists, otherwise leave Default.
            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0) go.layer = layer;

            // Wobble + slow spin so loot reads at a glance.
            go.AddComponent<LootHover>();

            var p = go.AddComponent<PickupInteractable>();
            // Configure via reflection (private serialized fields).
            var t = typeof(PickupInteractable);
            var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            t.GetField("itemId",      bf)?.SetValue(p, pick.id);
            t.GetField("displayName", bf)?.SetValue(p, pick.display);
            t.GetField("quantity",    bf)?.SetValue(p, 1);

            // AGENT 28: Spawn ShardCollect VFX using object pooling (eliminates GC allocations)
            if (ShardCollectVFX != null)
            {
                var poolManager = VFXPoolManager.Instance;
                if (poolManager != null)
                {
                    ParticleSystem ps = poolManager.SpawnParticle(ShardCollectVFX, position, Quaternion.identity, autoReturnDelay: 2f);
                    
                    // Match VFX color to loot rarity using MaterialPropertyBlock (preserves GPU instancing)
                    if (ps != null)
                    {
                        var renderer = ps.GetComponent<ParticleSystemRenderer>();
                        if (renderer != null)
                        {
                            MaterialPropertyBlockHelper.SetColor(renderer, pick.color);
                        }
                    }
                }
            }

            // Auto-cleanup if never picked up.
            Object.Destroy(go, 60f);
        }
    }

    /// <summary>Visual flair for loot cubes — float and spin until collected.</summary>
    public class LootHover : MonoBehaviour
    {
        Vector3 _origin;
        float _phase;
        
        // AGENT 6: Cache transform to eliminate GetComponent overhead
        Transform _cachedTransform;

        void Start()
        {
            _cachedTransform = transform;
            _origin = _cachedTransform.position;
            _phase = Random.value * 6.28f;
        }

        void Update()
        {
            // AGENT 6: Optimized hover animation - cached transform, reduced trig calls
            float time = Time.time;
            _cachedTransform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
            _cachedTransform.position = _origin + Vector3.up * (0.25f + 0.1f * Mathf.Sin(time * 2.5f + _phase));
        }
    }
}
