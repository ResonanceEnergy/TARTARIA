using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Gameplay.Combat;

namespace Tartaria.AI
{
    /// <summary>
    /// Crystal Sentry AI — ranged turret enemy for Moons 5-8.
    /// Behavior: Stationary/slow, shoots frequency-attuned crystal projectiles, vulnerable during reload.
    /// Combat design: Teaches ranged threat prioritization, dodging telegraphed attacks.
    /// Difficulty: Medium-High (HP: 250, Damage: 35 ranged, Speed: Stationary)
    /// </summary>
    public class CrystalSentryAI : MonoBehaviour
    {
        [Header("Combat Stats")]
        [SerializeField] float maxHealth = 250f;
        [SerializeField] float projectileDamage = 35f;
        [SerializeField] float attackRange = 20f;
        [SerializeField] float attackCooldown = 3f;

        [Header("Projectile")]
        [SerializeField] float projectileSpeed = 15f;
        [SerializeField] float telegraphDuration = 0.8f;

        [Header("Vulnerability")]
        [SerializeField] float reloadDuration = 2f; // vulnerable window after shooting

        Transform _player;
        float _currentHealth;
        float _attackTimer;
        float _reloadTimer;
        bool _isReloading;
        Renderer _renderer;
        Color _originalColor;

        enum SentryState { Idle, Telegraphing, Firing, Reloading, Dead }
        SentryState _state = SentryState.Idle;

        void Awake()
        {
            _currentHealth = maxHealth;
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null)
                _originalColor = _renderer.material.color;
        }

        void Start()
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                _player = playerGO.transform;
        }

        void Update()
        {
            if (_state == SentryState.Dead || _player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            // Always face player
            Vector3 lookDir = (_player.position - transform.position);
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 3f);
            }

            _attackTimer -= Time.deltaTime;

            switch (_state)
            {
                case SentryState.Idle:
                    if (distanceToPlayer <= attackRange && _attackTimer <= 0f)
                    {
                        _state = SentryState.Telegraphing;
                        _attackTimer = telegraphDuration;
                        StartTelegraph();
                    }
                    break;

                case SentryState.Telegraphing:
                    // Visual telegraph before firing
                    if (_attackTimer <= 0f)
                    {
                        _state = SentryState.Firing;
                        FireProjectile();
                    }
                    break;

                case SentryState.Firing:
                    // Instant transition to reload
                    _state = SentryState.Reloading;
                    _reloadTimer = reloadDuration;
                    _isReloading = true;
                    UpdateReloadVisuals(true);
                    break;

                case SentryState.Reloading:
                    _reloadTimer -= Time.deltaTime;
                    if (_reloadTimer <= 0f)
                    {
                        _isReloading = false;
                        _state = SentryState.Idle;
                        _attackTimer = attackCooldown;
                        UpdateReloadVisuals(false);
                        Debug.Log("[CrystalSentry] Reload complete, ready to fire");
                    }
                    break;
            }
        }

        void StartTelegraph()
        {
            // Visual warning: glow bright
            if (_renderer != null)
            {
                { var __m = _renderer.material; if (__m.HasProperty("_BaseColor")) __m.SetColor("_BaseColor", Color.yellow); else __m.color = Color.yellow; }
            }
            VFXEventSystem.RequestVFX(VFXEffect.HarmonicCascade, transform.position + Vector3.up);
            Debug.Log("[CrystalSentry] Telegraphing attack...");
        }

        void FireProjectile()
        {
            if (_player == null) return;

            Vector3 direction = (_player.position - transform.position).normalized;

            // Create projectile
            var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "CrystalProjectile";
            projectile.transform.position = transform.position + Vector3.up + direction * 1.5f;
            projectile.transform.localScale = Vector3.one * 0.5f;

            // Cyan crystal color
            var projRenderer = projectile.GetComponent<Renderer>();
            if (projRenderer != null)
            {
                { var __m = projRenderer.material; if (__m.HasProperty("_BaseColor")) __m.SetColor("_BaseColor", new Color(0.2f, 0.8f, 1f)); else __m.color = new Color(0.2f, 0.8f, 1f); }
            }

            // Add projectile behavior
            var proj = projectile.AddComponent<CrystalProjectile>();
            proj.Initialize(direction, projectileSpeed, projectileDamage);

            VFXEventSystem.RequestVFX(VFXEffect.Spark, transform.position);
            Audio.AudioManager.Instance?.PlayTone(528f, 0.3f);

            // Reset visual
            if (_renderer != null)
            {
                { var __m = _renderer.material; if (__m.HasProperty("_BaseColor")) __m.SetColor("_BaseColor", _originalColor); else __m.color = _originalColor; }
            }

            Debug.Log("[CrystalSentry] Projectile fired!");
        }

        void UpdateReloadVisuals(bool reloading)
        {
            if (_renderer == null) return;

            // Dim color during reload (vulnerable)
            Color targetColor = reloading ? new Color(0.3f, 0.3f, 0.3f) : _originalColor;
            { var __m = _renderer.material; if (__m.HasProperty("_BaseColor")) __m.SetColor("_BaseColor", targetColor); else __m.color = targetColor; }
        }

        public void TakeDamage(float damage)
        {
            if (_state == SentryState.Dead) return;

            // Double damage during reload (vulnerable window)
            if (_isReloading)
            {
                damage *= 2f;
                Debug.Log("[CrystalSentry] Vulnerable! Double damage!");
            }

            _currentHealth -= damage;

            // Visual feedback
            if (_renderer != null)
            {
                { var __m = _renderer.material; if (__m.HasProperty("_BaseColor")) __m.SetColor("_BaseColor", Color.red); else __m.color = Color.red; }
            }
            Invoke(nameof(ResetColor), 0.15f);

            if (_currentHealth <= 0f)
            {
                Die();
            }

            Debug.Log($"[CrystalSentry] Took {damage} damage, HP: {_currentHealth}/{maxHealth}");
        }

        void ResetColor()
        {
            if (_renderer != null && _state != SentryState.Dead)
            {
                { var __m = _renderer.material; if (__m.HasProperty("_BaseColor")) __m.SetColor("_BaseColor", _isReloading ? new Color(0.3f, 0.3f, 0.3f) : _originalColor); else __m.color = _isReloading ? new Color(0.3f, 0.3f, 0.3f) : _originalColor; }
            }
        }

        void Die()
        {
            _state = SentryState.Dead;
            Debug.Log("[CrystalSentry] Defeated");

            VFXEventSystem.RequestVFX(VFXEffect.HarmonicCascade, transform.position);

            // Drop loot
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem("Crystal Fragment", 1);
            }

            Destroy(gameObject, 2f);
        }

        /// <summary>Procedurally build a Crystal Sentry at runtime.</summary>
        public static GameObject BuildProcedural(Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("CrystalSentry");
            root.transform.SetPositionAndRotation(position, rotation);

            // Crystal body (cube)
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);
            var mat = body.GetComponent<Renderer>().material;
            { if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(0.4f, 0.7f, 1f)); else mat.color = new Color(0.4f, 0.7f, 1f); } // cyan crystal

            // Add components
            root.AddComponent<CapsuleCollider>().radius = 0.8f;
            var ai = root.AddComponent<CrystalSentryAI>();

            return root;
        }
    }

    /// <summary>Crystal projectile behavior — flies forward, damages player on hit.</summary>
    public class CrystalProjectile : MonoBehaviour
    {
        Vector3 _direction;
        float _speed;
        float _damage;
        float _lifetime = 10f;

        public void Initialize(Vector3 direction, float speed, float damage)
        {
            _direction = direction;
            _speed = speed;
            _damage = damage;
        }

        void Update()
        {
            transform.position += _direction * _speed * Time.deltaTime;

            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // SendMessage pattern - AI↔Gameplay circular dep broken (Phase 16)
                other.SendMessage("TakeDamage", Mathf.RoundToInt(_damage), SendMessageOptions.DontRequireReceiver);
                // VFXController.Instance  // B1: Cross-assembly call commented (VFXController in Integration)?.PlayEffect(VFXEffect.Spark, transform.position);

                // Sprint 7 Lane 7: HitFeedback feedback (popup + hitstop + shake)
                try { HitFeedback.NotifyHit(other.transform.position, _damage, false); }
                catch (System.NullReferenceException) { Debug.LogWarning("[HitCallSite] HitFeedback not initialized at CrystalSentryAI.cs:CrystalProjectile.OnTriggerEnter"); }

                Destroy(gameObject);
            }
            else if (!other.isTrigger)
            {
                // Hit wall/obstacle
                Destroy(gameObject);
            }
        }
    }
}
