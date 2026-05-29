using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Integration;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// PlayerAbilities - Complete combat ability system.
    /// Harmonic Strike, Resonance Blast, Aether Shield, etc.
    /// </summary>
    public class PlayerAbilitiesComplete : MonoBehaviour
    {
        public static PlayerAbilitiesComplete Instance { get; private set; }

        [Header("Unlocked Abilities")]
        [SerializeField] private List<string> unlockedAbilities = new();

        [Header("Ability Stats")]
        [SerializeField] private float harmonicStrikeDamage = 25f;
        [SerializeField] private float resonanceBlastDamage = 50f;
        [SerializeField] private float aetherShieldDuration = 5f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            // Default ability
            UnlockAbility("BasicAttack");
        }

        void Update()
        {
            // Ability hotkeys
            if (Input.GetKeyDown(KeyCode.Q))
                UseAbility("HarmonicStrike");
            if (Input.GetKeyDown(KeyCode.E))
                UseAbility("ResonanceBlast");
            if (Input.GetKeyDown(KeyCode.R))
                UseAbility("AetherShield");
        }

        public void UnlockAbility(string abilityName)
        {
            if (!unlockedAbilities.Contains(abilityName))
            {
                unlockedAbilities.Add(abilityName);
                Debug.Log($"[PlayerAbilities] ✅ Unlocked: {abilityName}");
                HUDController.Instance?.ShowBanner("NEW ABILITY!", abilityName);
            }
        }

        public void UseAbility(string abilityName)
        {
            if (!unlockedAbilities.Contains(abilityName))
            {
                Debug.LogWarning($"[PlayerAbilities] Ability not unlocked: {abilityName}");
                return;
            }

            switch (abilityName)
            {
                case "HarmonicStrike":
                    HarmonicStrike();
                    break;
                case "ResonanceBlast":
                    ResonanceBlast();
                    break;
                case "AetherShield":
                    AetherShield();
                    break;
            }
        }

        void HarmonicStrike()
        {
            Debug.Log("[PlayerAbilities] Harmonic Strike!");
            // Find closest enemy
            var enemies = FindObjectsByType<MudGolemEnemy>(FindObjectsSortMode.None);
            if (enemies.Length > 0)
            {
                enemies[0].TakeDamage(harmonicStrikeDamage);
            }
            VFXWiringController.Instance?.SpawnVFX("HarmonicStrike", transform.position + transform.forward * 2f);
        }

        void ResonanceBlast()
        {
            Debug.Log("[PlayerAbilities] Resonance Blast!");
            // AOE damage
            Collider[] hits = Physics.OverlapSphere(transform.position, 5f);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<MudGolemEnemy>();
                if (enemy != null)
                    enemy.TakeDamage(resonanceBlastDamage);
            }
            VFXWiringController.Instance?.SpawnVFX("ResonanceBlast", transform.position);
        }

        void AetherShield()
        {
            Debug.Log("[PlayerAbilities] Aether Shield!");
            StartCoroutine(AetherShieldCoroutine());
        }

        System.Collections.IEnumerator AetherShieldCoroutine()
        {
            // Grant invulnerability
            var playerHealth = GetComponent<PlayerHealthController>();
            if (playerHealth != null)
                playerHealth.SetInvulnerable(true);

            VFXWiringController.Instance?.SpawnVFX("AetherShield", transform.position);
            yield return new WaitForSeconds(aetherShieldDuration);

            if (playerHealth != null)
                playerHealth.SetInvulnerable(false);
        }

        public bool HasAbility(string abilityName) => unlockedAbilities.Contains(abilityName);
    }
}
