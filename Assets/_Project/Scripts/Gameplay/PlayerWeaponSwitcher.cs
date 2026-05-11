using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Weapon switching system: toggle between Melee and Bow.
    /// Q or D-pad-Up to swap. Enables/disables corresponding components.
    /// Auto-attached by CharacterPrefabFactory.
    /// </summary>
    public class PlayerWeaponSwitcher : MonoBehaviour
    {
        public enum WeaponType { Melee, Bow }

        [SerializeField] WeaponType currentWeapon = WeaponType.Melee;

        PlayerCombat _melee;
        PlayerRanged _ranged;

        public WeaponType CurrentWeapon => currentWeapon;
        public static event System.Action<WeaponType> OnWeaponChanged;

        void Awake()
        {
            _melee = GetComponent<PlayerCombat>();
            _ranged = GetComponent<PlayerRanged>();
        }

        void Start()
        {
            ApplyWeaponState();
        }

        void Update()
        {
            bool swap = false;
            var kb = Keyboard.current;
            if (kb != null && kb.qKey.wasPressedThisFrame) swap = true;
            var pad = Gamepad.current;
            if (pad != null && pad.dpad.up.wasPressedThisFrame) swap = true;

            if (swap)
                SwitchWeapon();
        }

        void SwitchWeapon()
        {
            currentWeapon = currentWeapon == WeaponType.Melee 
                ? WeaponType.Bow 
                : WeaponType.Melee;
            
            ApplyWeaponState();
            OnWeaponChanged?.Invoke(currentWeapon);
            Debug.Log($"[WeaponSwitcher] Switched to {currentWeapon}");
        }

        void ApplyWeaponState()
        {
            if (_melee != null) _melee.enabled = currentWeapon == WeaponType.Melee;
            if (_ranged != null) _ranged.enabled = currentWeapon == WeaponType.Bow;
        }
    }
}
