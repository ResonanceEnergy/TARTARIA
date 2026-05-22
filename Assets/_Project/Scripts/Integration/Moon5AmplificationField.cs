using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5 pavilion amplification field mechanics.
    /// When player enters amplified zone, grant temporary buffs:
    /// - +20% RS generation
    /// - +15% movement speed
    /// - +10% damage resistance
    /// - 6-band healing aura (slow HP regen)
    /// </summary>
    public class PavilionAmplificationField : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] float fieldRadius = 15f;
        [SerializeField] float rsBonus = 0.2f;           // +20% RS generation
        [SerializeField] float speedBonus = 0.15f;       // +15% movement speed
        [SerializeField] float resistanceBonus = 0.1f;   // +10% damage resistance
        [SerializeField] float healingRate = 2f;         // 2 HP per second

        [Header("State")]
        [SerializeField] bool isAmplified = false;

        GameObject _player;
        PlayerAbilities _playerAbilities;
        PlayerHealth _playerHealth;
        bool _playerInField = false;

        Light _amplificationLight;
        ParticleSystem _auraParticles;

        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player != null)
            {
                _playerAbilities = _player.GetComponent<PlayerAbilities>();
                _playerHealth = _player.GetComponent<PlayerHealth>();
            }

            CreateFieldVisuals();
        }

        void Update()
        {
            if (!isAmplified || _player == null) return;

            // Check if player in field range
            float distToPlayer = Vector3.Distance(transform.position, _player.transform.position);
            bool nowInField = distToPlayer <= fieldRadius;

            if (nowInField && !_playerInField)
            {
                OnPlayerEnterField();
            }
            else if (!nowInField && _playerInField)
            {
                OnPlayerExitField();
            }

            // Healing while in field
            if (_playerInField && _playerHealth != null)
            {
                _playerHealth.Heal(healingRate * Time.deltaTime);
            }
        }

        void CreateFieldVisuals()
        {
            // Golden light (inactive until amplified)
            _amplificationLight = gameObject.AddComponent<Light>();
            _amplificationLight.type = LightType.Point;
            _amplificationLight.color = new Color(1f, 0.9f, 0.5f);
            _amplificationLight.range = fieldRadius * 1.5f;
            _amplificationLight.intensity = isAmplified ? 4f : 0f;

            // Golden particle aura
            GameObject particleObj = new GameObject("AmplificationAura");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;

            _auraParticles = particleObj.AddComponent<ParticleSystem>();
            var main = _auraParticles.main;
            main.startLifetime = 3f;
            main.startSpeed = 1f;
            main.startSize = 0.5f;
            main.startColor = new Color(1f, 0.9f, 0.5f, 0.6f);
            main.maxParticles = 100;
            main.loop = true;

            var emission = _auraParticles.emission;
            emission.rateOverTime = isAmplified ? 20f : 0f;

            var shape = _auraParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = fieldRadius;
        }

        public void Amplify()
        {
            if (isAmplified) return;

            isAmplified = true;

            // Enable visuals
            if (_amplificationLight != null)
                _amplificationLight.intensity = 4f;

            if (_auraParticles != null)
            {
                var emission = _auraParticles.emission;
                emission.rateOverTime = 20f;
            }

            Debug.Log($"[PavilionAmplification] Pavilion amplified! Field active (radius: {fieldRadius}m)");
            Audio.AudioManager.Instance?.PlaySFX2D("PavilionAmplify");

            HUDController.Instance?.ShowObjective("Amplification field active! +20% RS, +15% speed, +10% resistance");
        }

        void OnPlayerEnterField()
        {
            _playerInField = true;

            Debug.Log("[PavilionAmplification] Player entered amplification field — buffs applied");

            // Apply buffs
            if (_playerAbilities != null)
            {
                _playerAbilities.AddRSMultiplier("pavilion_amp", rsBonus);
                _playerAbilities.AddSpeedMultiplier("pavilion_amp", speedBonus);
                _playerAbilities.AddResistanceMultiplier("pavilion_amp", resistanceBonus);
            }

            HUDController.Instance?.ShowObjective("⚡ AMPLIFICATION FIELD ⚡");
            Audio.AudioManager.Instance?.PlaySFX2D("AmplificationEnter");
        }

        void OnPlayerExitField()
        {
            _playerInField = false;

            Debug.Log("[PavilionAmplification] Player exited amplification field — buffs removed");

            // Remove buffs
            if (_playerAbilities != null)
            {
                _playerAbilities.RemoveRSMultiplier("pavilion_amp");
                _playerAbilities.RemoveSpeedMultiplier("pavilion_amp");
                _playerAbilities.RemoveResistanceMultiplier("pavilion_amp");
            }
        }
    }

    /// <summary>
    /// White City pavilion structure with restoration + amplification.
    /// </summary>
    public class WhiteCityPavilion : MonoBehaviour, IInteractable
    {
        public int pavilionIndex;
        public event System.Action<WhiteCityPavilion> OnRestored;

        [SerializeField] bool isRestored = false;
        [SerializeField] float restorationProgress = 0f;
        const float RESTORATION_DURATION = 4f;

        PavilionAmplificationField _amplificationField;

        void Start()
        {
            // Add amplification field component
            _amplificationField = gameObject.AddComponent<PavilionAmplificationField>();
        }

        public string GetInteractPrompt()
        {
            if (isRestored) return $"Pavilion {pavilionIndex + 1} Restored ✓";
            if (restorationProgress > 0f) return $"Restoring... {restorationProgress / RESTORATION_DURATION:P0}";
            return $"[E] Restore Pavilion {pavilionIndex + 1}";
        }

        public void Interact(GameObject player)
        {
            if (isRestored) return;

            StartCoroutine(RestorePavilion());
        }

        System.Collections.IEnumerator RestorePavilion()
        {
            Debug.Log($"[Pavilion {pavilionIndex}] Restoring pavilion with golden-ratio template...");
            HUDController.Instance?.ShowObjective($"Restoring pavilion {pavilionIndex + 1}...");

            // Restoration progress
            while (restorationProgress < RESTORATION_DURATION)
            {
                restorationProgress += Time.deltaTime;
                yield return null;
            }

            isRestored = true;

            Debug.Log($"[Pavilion {pavilionIndex}] Restoration complete! Amplification field activating...");

            // Activate amplification field
            _amplificationField?.Amplify();

            // Notify spawner
            OnRestored?.Invoke(this);

            // Update HUD
            HUDController.Instance?.ShowObjective($"⚡ Pavilion {pavilionIndex + 1} Restored! Amplification field active ⚡");

            // Quest progress
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.Interact, $"restore_pavilion_{pavilionIndex}");
        }

        public void MarkRestored()
        {
            isRestored = true;
            restorationProgress = RESTORATION_DURATION;
            _amplificationField?.Amplify();
        }
    }
}
