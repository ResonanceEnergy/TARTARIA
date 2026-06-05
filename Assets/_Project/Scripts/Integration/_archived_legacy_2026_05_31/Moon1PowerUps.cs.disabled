using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Power-Ups — Temporary RS boosters and enhancement pickups
    /// Strategic placement to reward exploration and combat
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon1PowerUps : MonoBehaviour
    {
        [Header("Power-Up Prefabs")]
        [SerializeField] GameObject rsBoostPrefab;
        [SerializeField] GameObject combatBoostPrefab;
        [SerializeField] GameObject healingOrbPrefab;
        
        [Header("Spawn Configuration")]
        [SerializeField] int rsBoostCount = 8;
        [SerializeField] int combatBoostCount = 5;
        [SerializeField] int healingOrbCount = 10;
        
        [Header("Power-Up Stats")]
        [SerializeField] float rsBoostAmount = 10f;
        [SerializeField] float combatBoostDuration = 15f;
        [SerializeField] float combatBoostMultiplier = 1.5f;
        [SerializeField] float healingAmount = 25f;
        
        [Header("Respawn Settings")]
        [SerializeField] bool enableRespawn = true;
        [SerializeField] float respawnDelay = 60f;  // 60s respawn
        
        readonly List<PowerUpInstance> _activePowerUps = new();
        readonly Queue<PowerUpInstance> _respawnQueue = new();
        
        void Start()
        {
            SpawnInitialPowerUps();
            
            Debug.Log($"[Moon1PowerUps] ✅ Initialized - {rsBoostCount + combatBoostCount + healingOrbCount} power-ups placed");
        }
        
        void SpawnInitialPowerUps()
        {
            // RS Boost pickups (strategic locations)
            Vector3[] rsBoostLocations = new Vector3[]
            {
                new Vector3(15f, 2f, 10f),      // Near spawn
                new Vector3(-20f, 4f, 15f),     // West wing
                new Vector3(25f, 1f, -12f),     // East plaza
                new Vector3(-10f, 6f, 30f),     // North tower
                new Vector3(8f, 1f, -25f),      // South courtyard
                new Vector3(0f, 10f, 20f),      // Upper gallery
                new Vector3(-25f, 1f, -5f),     // West courtyard
                new Vector3(18f, 3f, 22f),      // East balcony
            };
            
            for (int i = 0; i < Mathf.Min(rsBoostCount, rsBoostLocations.Length); i++)
            {
                SpawnPowerUp(PowerUpType.RSBoost, rsBoostLocations[i]);
            }
            
            // Combat Boost pickups (near combat areas)
            Vector3[] combatBoostLocations = new Vector3[]
            {
                new Vector3(12f, 1f, 8f),       // First combat area
                new Vector3(-15f, 2f, -10f),    // Enemy spawn zone
                new Vector3(20f, 1f, 18f),      // Arena area
                new Vector3(-8f, 1f, -20f),     // Combat corridor
                new Vector3(5f, 4f, 25f),       // Upper combat zone
            };
            
            for (int i = 0; i < Mathf.Min(combatBoostCount, combatBoostLocations.Length); i++)
            {
                SpawnPowerUp(PowerUpType.CombatBoost, combatBoostLocations[i]);
            }
            
            // Healing Orbs (scattered throughout)
            for (int i = 0; i < healingOrbCount; i++)
            {
                Vector3 randomPos = GetRandomSpawnPosition();
                SpawnPowerUp(PowerUpType.HealingOrb, randomPos);
            }
        }
        
        void SpawnPowerUp(PowerUpType type, Vector3 position)
        {
            GameObject prefab = GetPrefabForType(type);
            if (prefab == null) return;
            
            GameObject powerUpObj = Instantiate(prefab, position, Quaternion.identity, transform);
            powerUpObj.name = $"PowerUp_{type}_{_activePowerUps.Count}";
            
            // Add pickup component
            PowerUpPickup pickup = powerUpObj.GetOrAddComponent<PowerUpPickup>();
            pickup.powerUpType = type;
            pickup.rsBoostAmount = rsBoostAmount;
            pickup.combatBoostDuration = combatBoostDuration;
            pickup.combatBoostMultiplier = combatBoostMultiplier;
            pickup.healingAmount = healingAmount;
            pickup.onPickedUp += () => OnPowerUpCollected(pickup, position);
            
            // Visual effects
            SetupPowerUpVisuals(powerUpObj, type);
            
            PowerUpInstance instance = new PowerUpInstance
            {
                powerUpType = type,
                gameObject = powerUpObj,
                spawnPosition = position,
                isActive = true
            };
            
            _activePowerUps.Add(instance);
        }
        
        GameObject GetPrefabForType(PowerUpType type)
        {
            return type switch
            {
                PowerUpType.RSBoost => rsBoostPrefab,
                PowerUpType.CombatBoost => combatBoostPrefab,
                PowerUpType.HealingOrb => healingOrbPrefab,
                _ => null
            };
        }
        
        void SetupPowerUpVisuals(GameObject powerUpObj, PowerUpType type)
        {
            // Color-coded glow
            Color glowColor = type switch
            {
                PowerUpType.RSBoost => new Color(0.3f, 0.8f, 1f),      // Cyan
                PowerUpType.CombatBoost => new Color(1f, 0.3f, 0.2f),  // Red
                PowerUpType.HealingOrb => new Color(0.3f, 1f, 0.4f),   // Green
                _ => Color.white
            };
            
            // Add point light
            Light light = powerUpObj.GetOrAddComponent<Light>();
            light.type = LightType.Point;
            light.color = glowColor;
            light.range = 5f;
            light.intensity = 0.8f;
            
            // Pulsing animation
            LeanTween.value(powerUpObj, 0.5f, 1f, 1f)
                .setEaseInOutSine()
                .setLoopPingPong()
                .setOnUpdate((float val) =>
                {
                    if (light != null)
                        light.intensity = val;
                });
            
            // Floating animation
            float startY = powerUpObj.transform.position.y;
            LeanTween.moveY(powerUpObj, startY + 0.5f, 1.5f)
                .setEaseInOutSine()
                .setLoopPingPong();
            
            // Rotation
            LeanTween.rotateY(powerUpObj, 360f, 3f)
                .setLoopClamp();
        }
        
        void OnPowerUpCollected(PowerUpPickup pickup, Vector3 originalPosition)
        {
            // Remove from active list
            _activePowerUps.RemoveAll(p => p.gameObject == pickup.gameObject);
            
            // Schedule respawn
            if (enableRespawn)
            {
                PowerUpInstance respawnInstance = new PowerUpInstance
                {
                    powerUpType = pickup.powerUpType,
                    spawnPosition = originalPosition,
                    respawnTime = Time.time + respawnDelay,
                    isActive = false
                };
                
                _respawnQueue.Enqueue(respawnInstance);
            }
        }
        
        void Update()
        {
            // Check respawn queue
            if (enableRespawn && _respawnQueue.Count > 0)
            {
                PowerUpInstance nextRespawn = _respawnQueue.Peek();
                
                if (Time.time >= nextRespawn.respawnTime)
                {
                    _respawnQueue.Dequeue();
                    SpawnPowerUp(nextRespawn.powerUpType, nextRespawn.spawnPosition);
                }
            }
        }
        
        Vector3 GetRandomSpawnPosition()
        {
            Vector2 randomCircle = Random.insideUnitCircle * 35f;
            Vector3 position = new Vector3(randomCircle.x, 2f, randomCircle.y);
            
            // Raycast to ground
            if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                return hit.point + Vector3.up * 1f;
            }
            
            return position;
        }
        
        void OnDestroy()
        {
            foreach (var powerUp in _activePowerUps)
            {
                if (powerUp.gameObject != null)
                    Destroy(powerUp.gameObject);
            }
        }
    }
    
    public enum PowerUpType
    {
        RSBoost,       // +10 RS instant
        CombatBoost,   // 1.5x damage for 15s
        HealingOrb     // +25 HP
    }
    
    public class PowerUpPickup : MonoBehaviour
    {
        public PowerUpType powerUpType;
        public float rsBoostAmount;
        public float combatBoostDuration;
        public float combatBoostMultiplier;
        public float healingAmount;
        public System.Action onPickedUp;
        
        float _pickupRadius = 2f;
        GameObject _player;
        bool _pickedUp;
        
        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
        }
        
        void Update()
        {
            if (_pickedUp || _player == null) return;
            
            float distance = Vector3.Distance(transform.position, _player.transform.position);
            if (distance <= _pickupRadius)
            {
                Pickup();
            }
        }
        
        void Pickup()
        {
            if (_pickedUp) return;
            
            _pickedUp = true;
            
            // Apply power-up effect
            switch (powerUpType)
            {
                case PowerUpType.RSBoost:
                    if (GameStateManager.Instance != null)
                        GameStateManager.Instance.AddResonancePoints(rsBoostAmount);
                    Debug.Log($"[PowerUp] +{rsBoostAmount} RS");
                    break;
                    
                case PowerUpType.CombatBoost:
                    if (PlayerStats.Instance != null)
                        PlayerStats.Instance.ApplyCombatBoost(combatBoostMultiplier, combatBoostDuration);
                    Debug.Log($"[PowerUp] Combat boost: {combatBoostMultiplier}x for {combatBoostDuration}s");
                    break;
                    
                case PowerUpType.HealingOrb:
                    if (PlayerStats.Instance != null)
                        PlayerStats.Instance.Heal(healingAmount);
                    Debug.Log($"[PowerUp] +{healingAmount} HP");
                    break;
            }
            
            // Notify
            onPickedUp?.Invoke();
            
            // VFX
            LeanTween.scale(gameObject, Vector3.zero, 0.3f).setEaseInBack().setOnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
    }
    
    public class PowerUpInstance
    {
        public PowerUpType powerUpType;
        public GameObject gameObject;
        public Vector3 spawnPosition;
        public float respawnTime;
        public bool isActive;
    }
}
