using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Combat Hit Reactor — plays VFX + SFX on damage, triggers animation reactions.
    /// Attach to damageable entities (Player, enemies, destructibles).
    /// </summary>
    public class CombatHitReactor : MonoBehaviour
    {
        [Header("Visual Feedback")]
        [SerializeField] GameObject hitParticlePrefab;
        [SerializeField] float flashDuration = 0.15f;
        [SerializeField] Color flashColor = Color.red;

        [Header("Audio")]
        [SerializeField] string hitSFXName = "ImpactHit";

        Renderer[] _renderers;
        Color[] _originalColors;
        float _flashTimer;
        bool _isFlashing;

        void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _originalColors = new Color[_renderers.Length];

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].material != null)
                {
                    _originalColors[i] = _renderers[i].material.color;
                }
            }
        }

        void Update()
        {
            if (_isFlashing)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f)
                {
                    // Restore original colors
                    for (int i = 0; i < _renderers.Length; i++)
                    {
                        if (_renderers[i] != null && _renderers[i].material != null)
                        {
                            _renderers[i].material.color = _originalColors[i];
                        }
                    }
                    _isFlashing = false;
                }
            }
        }

        public void OnHit(Vector3 hitPoint, Vector3 hitNormal)
        {
            // VFX: spawn hit particles
            if (hitParticlePrefab != null)
            {
                var particles = Instantiate(hitParticlePrefab, hitPoint, Quaternion.LookRotation(hitNormal));
                Destroy(particles, 2f);
            }

            // SFX: play hit sound
            if (!string.IsNullOrEmpty(hitSFXName))
            {
                Audio.AudioManager.Instance?.PlaySFX(hitSFXName, hitPoint, 0.6f);
            }

            // Flash red
            FlashDamage();
        }

        void FlashDamage()
        {
            if (_renderers == null || _renderers.Length == 0) return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].material != null)
                {
                    _renderers[i].material.color = flashColor;
                }
            }

            _isFlashing = true;
            _flashTimer = flashDuration;
        }

        public void OnDeath()
        {
            // Spawn death VFX (can be customized)
            Debug.Log($"[CombatHitReactor] {gameObject.name} death VFX triggered");

            // Play death sound
            Audio.AudioManager.Instance?.PlaySFX2D("DeathSound");

            // Disable visual feedback (death anim will play)
            _isFlashing = false;
        }
    }
}
