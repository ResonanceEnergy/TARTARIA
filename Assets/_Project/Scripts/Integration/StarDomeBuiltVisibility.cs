// Hammer Lane 4 — Phase 6.3 — Sprint 11 L8 50ff78ea
// Subscribes to GameEvents.OnBuildingRestored and enables the authored "built variant"
// prefab when the StarDome ("dome") restoration fires. Replaces the 30+ runtime
// Instantiate calls in BuildingSpawner.CreateModularDungeonStarDome with a single
// SetActive(true) on the pre-composed Echohaven_StarDome_Built.prefab.
//
// API_CONTRACT — grep-verified: Tartaria.Core.GameEvents.OnBuildingRestored is declared
// at GameEvents.cs:56 as `public static event Action<string> OnBuildingRestored`
// (payload = buildingId string), and is fired via FireBuildingRestored(GameEvents.cs:59).

using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    [DisallowMultipleComponent]
    public class StarDomeBuiltVisibility : MonoBehaviour
    {
        GameObject _builtVariant;
        string _buildingId = "dome";
        bool _subscribed;

        public void Configure(GameObject builtVariant, string buildingId)
        {
            _builtVariant = builtVariant;
            _buildingId = buildingId;
        }

        void OnEnable()
        {
            if (_subscribed) return;
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
            _subscribed = true;
        }

        void OnDisable()
        {
            if (!_subscribed) return;
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
            _subscribed = false;
        }

        void HandleBuildingRestored(string buildingId)
        {
            if (buildingId != _buildingId) return;
            if (_builtVariant == null)
            {
                Debug.LogWarning("[StarDomeBuiltVisibility] OnBuildingRestored fired but built variant reference is null.");
                return;
            }
            if (_builtVariant.activeSelf)
            {
                // Already swapped (e.g., on save load). Idempotent.
                return;
            }
            _builtVariant.SetActive(true);
            Debug.Log($"[StarDomeBuiltVisibility] OnBuildingRestored(\"{buildingId}\") — built variant swapped in.");
        }
    }
}
