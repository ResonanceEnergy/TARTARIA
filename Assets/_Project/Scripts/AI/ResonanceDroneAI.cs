using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.AI
{
    /// <summary>
    /// Resonance Drone AI — flying support enemy for Moons 8-11.
    /// Behavior: Flies above combat, buffs nearby enemies, weakens player with dissonance beams.
    /// Combat design: Priority target, teaches threat assessment and focus fire.
    /// Difficulty: Medium (HP: 150, Damage: 15 DoT, Speed: Fast flying)
    /// </summary>
    public class ResonanceDroneAI : MonoBehaviour
    {
        [Header("Combat Stats")]
        [SerializeField] float maxHealth = 150f;
        [SerializeField] float beamDamagePerSecond = 15f;
        [SerializeField] float beamRange = 15f;

        [Header("Flight")]
        [SerializeField] float flyHeight = 6f;
        [SerializeField] float flySpeed = 4f;
        [SerializeField] float orbitRadius = 8f;

        [Header("Support Mechanics")]
        [SerializeField] float buffRadius = 10f;
        [SerializeField] float buffStrength = 0.3f; // 30% damage boost to nearby enemies

        Transform _player;
        float _currentHealth;
        float _orbitAngle;
        bool _isBeaming;
        LineRenderer _beamLine;
        Renderer _renderer;
        Color _originalColor;

        List<EnemyAIController> _nearbyEnemies = new List<EnemyAIController>();

        enum DroneState { Orbiting, Beaming, Dead }
        DroneState _state = DroneState.Orbiting;

        void Awake()
        {
            _currentHealth = maxHealth;
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null)
                _originalColor = _renderer.material.color;

            // Setup beam line renderer
            _beamLine = gameObject.AddComponent<LineRenderer>();
            _beamLine.startWidth = 0.1f;
            _beamLine.endWidth = 0.1f;
            _beamLine.material = new Material(Shader.Find("Sprites/Default"));
            _beamLine.startColor = new Color(1f, 0.3f, 0.3f);
            _beamLine.endColor = new Color(1f, 0.3f, 0.3f);
            _beamLine.enabled = false;
        }

        void Start()
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                _player = playerGO.transform;

            _orbitAngle = Random.Range(0f, 360f);
            InvokeRepeating(nameof(UpdateNearbyEnemies), 0f, 1f);
        }

        void Update()
        {
            if (_state == DroneState.Dead || _player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            switch (_state)
            {
                case DroneState.Orbiting:
                    // Orbit around player at altitude
                    _orbitAngle += flySpeed * 20f * Time.deltaTime;
                    float rad = _orbitAngle * Mathf.Deg2Rad;
                    Vector3 targetPos = _player.position + new Vector3(
                        Mathf.Cos(rad) * orbitRadius,
                        flyHeight,
                        Mathf.Sin(rad) * orbitRadius
                    );

                    transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * flySpeed);

                    // Always face player
                    Vector3 lookDir = (_player.position - transform.position);
                    if (lookDir.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 4f);
                    }

                    // Start beaming if in range
                    if (distanceToPlayer <= beamRange)
                    {
                        _state = DroneState.Beaming;
                        _isBeaming = true;
                        _beamLine.enabled = true;
                        Debug.Log("[ResonanceDrone] Dissonance beam activated");
                    }
                    break;

                case DroneState.Beaming:
                    // Continue orbiting while beaming
                    _orbitAngle += flySpeed * 15f * Time.deltaTime;
                    rad = _orbitAngle * Mathf.Deg2Rad;
                    targetPos = _player.position + new Vector3(
                        Mathf.Cos(rad) * orbitRadius,
                        flyHeight,
                        Mathf.Sin(rad) * orbitRadius
                    );

                    transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * flySpeed);

                    // Update beam
                    if (distanceToPlayer <= beamRange)
                    {
                        UpdateBeam();
                        DamagePlayerWithBeam();
                    }
                    else
                    {
                        _state = DroneState.Orbiting;
                        _isBeaming = false;
                        _beamLine.enabled = false;
                    }
                    break;
            }

            // Buff nearby enemies
            ApplyBuffsToNearbyEnemies();
        }

        void UpdateBeam()
        {
            _beamLine.SetPosition(0, transform.position);
            _beamLine.SetPosition(1, _player.position + Vector3.up);
        }

        void DamagePlayerWithBeam()
        {
            var playerHealth = _player?.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                int damage = Mathf.RoundToInt(beamDamagePerSecond * Time.deltaTime);
                if (damage > 0)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }

        void UpdateNearbyEnemies()
        {
            _nearbyEnemies.Clear();
            var colliders = Physics.OverlapSphere(transform.position, buffRadius);
            foreach (var col in colliders)
            {
                var enemy = col.GetComponent<EnemyAIController>();
                if (enemy != null && enemy != this)
                {
                    _nearbyEnemies.Add(enemy);
                }
            }
        }

        void ApplyBuffsToNearbyEnemies()
        {
            // Visual indicator for buffed enemies (they glow red)
            foreach (var enemy in _nearbyEnemies)
            {
                if (enemy == null) continue;
                
                // Note: In full implementation, this would modify enemy damage
                // For now, just visual feedback
                var rend = enemy.GetComponentInChildren<Renderer>();
                if (rend != null && Time.frameCount % 60 == 0)
                {
                    VFXEventSystem.RequestVFX(VFXEffect.Spark, enemy.transform.position);
                }
            }
        }

        public void TakeDamage(float damage)
        {
            if (_state == DroneState.Dead) return;

            _currentHealth -= damage;

            // Visual feedback
            if (_renderer != null)
            {
                _renderer.material.color = Color.red;
            }
            Invoke(nameof(ResetColor), 0.15f);

            if (_currentHealth <= 0f)
            {
                Die();
            }

            Debug.Log($"[ResonanceDrone] Took {damage} damage, HP: {_currentHealth}/{maxHealth}");
        }

        void ResetColor()
        {
            if (_renderer != null && _state != DroneState.Dead)
            {
                _renderer.material.color = _originalColor;
            }
        }

        void Die()
        {
            _state = DroneState.Dead;
            _beamLine.enabled = false;
            Debug.Log("[ResonanceDrone] Defeated");
            
            VFXEventSystem.RequestVFX(VFXEffect.HarmonicCascade, transform.position);
            
            // Drop loot
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem("Resonance Core", 1);
            }

            Destroy(gameObject, 1f);
        }

        void OnDestroy()
        {
            CancelInvoke();
        }

        /// <summary>Procedurally build a Resonance Drone at runtime.</summary>
        public static GameObject BuildProcedural(Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("ResonanceDrone");
            root.transform.SetPositionAndRotation(position, rotation);

            // Core body (sphere)
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "Core";
            core.transform.SetParent(root.transform, false);
            core.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            var coreMat = core.GetComponent<Renderer>().material;
            coreMat.color = new Color(0.9f, 0.4f, 0.4f); // red glow

            // Wings/Rotors (cylinders)
            for (int i = 0; i < 4; i++)
            {
                var wing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wing.name = $"Wing{i}";
                wing.transform.SetParent(root.transform, false);
                float angle = i * 90f;
                float rad = angle * Mathf.Deg2Rad;
                wing.transform.localPosition = new Vector3(Mathf.Cos(rad) * 0.6f, 0f, Mathf.Sin(rad) * 0.6f);
                wing.transform.localScale = new Vector3(0.1f, 0.4f, 0.1f);
                wing.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
                Destroy(wing.GetComponent<Collider>());
            }

            // Add components
            root.AddComponent<SphereCollider>().radius = 0.5f;
            var ai = root.AddComponent<ResonanceDroneAI>();

            return root;
        }
    }
}
