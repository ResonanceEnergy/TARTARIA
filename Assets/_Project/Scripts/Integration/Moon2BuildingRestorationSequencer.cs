using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Building Restoration Sequencer — orchestrates the 5 key building transformations.
    /// 
    /// Per GDD §03 Moon 2 + Moon2ProgressionSystem:
    /// 1. Cathedral Dome (Discovery/Restoration beat) → M2_CathedralBreath
    /// 2. Bell Tower (Restoration beat) → M2_BellCleansing
    /// 3. Fountain (Conflict beat) → M2_FountainSpring
    /// 4. Crystal Hall (Climax beat) → M2_CrystalLens
    /// 5. Ley Chamber (Revelation beat) → M2_LeyBond
    /// 
    /// Each restoration:
    /// - Triggers mud-to-crystal visual transformation (VFXController + Moon2LunarVisualsManager)
    /// - Grants permanent blessing (Moon2ProgressionSystem)
    /// - Plays atmospheric audio bloom (Moon2AtmosphereAudioManager)
    /// - Awards RS + haptic feedback
    /// - Persists to save system
    /// 
    /// Absolute path: C:\\dev\\TARTARIA_new
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon2BuildingRestorationSequencer : MonoBehaviour
    {
        public static Moon2BuildingRestorationSequencer Instance { get; private set; }

        [Header("Building References")]
        [SerializeField] InteractableBuilding cathedralDome;
        [SerializeField] InteractableBuilding bellTower;
        [SerializeField] InteractableBuilding fountain;
        [SerializeField] InteractableBuilding crystalHall;
        [SerializeField] InteractableBuilding leyChamber;

        [Header("Restoration State")]
        readonly HashSet<string> _restoredBuildings = new HashSet<string>();
        bool _allBuildingsRestored;

        public bool IsCathedralRestored => _restoredBuildings.Contains("moon2_cathedral_dome");
        public bool IsBellTowerRestored => _restoredBuildings.Contains("moon2_bell_tower");
        public bool IsFountainRestored => _restoredBuildings.Contains("moon2_fountain");
        public bool IsCrystalHallRestored => _restoredBuildings.Contains("moon2_crystal_hall");
        public bool IsLeyChamberRestored => _restoredBuildings.Contains("moon2_ley_chamber");
        public int RestoredCount => _restoredBuildings.Count;

        public event System.Action<string> OnBuildingRestored;
        public event System.Action OnAllBuildingsRestored;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Subscribe to GameEvents for building restoration
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                GameEvents.OnBuildingRestored -= HandleBuildingRestored;
            }
        }

        /// <summary>
        /// Discovers and auto-wires the 5 key buildings in the Moon 2 scene.
        /// Called by Moon2ContentSpawner or scene setup.
        /// </summary>
        public void DiscoverAndWireBuildings()
        {
            // Find buildings by name/tag in scene
            var allBuildings = FindObjectsOfType<InteractableBuilding>();

            foreach (var building in allBuildings)
            {
                string buildingName = building.gameObject.name.ToLowerInvariant();

                if (buildingName.Contains("cathedral") || buildingName.Contains("dome"))
                {
                    cathedralDome = building;
                    Debug.Log($"[Moon2 Restoration] Wired Cathedral Dome: {building.gameObject.name}");
                }
                else if (buildingName.Contains("bell") || buildingName.Contains("tower"))
                {
                    bellTower = building;
                    Debug.Log($"[Moon2 Restoration] Wired Bell Tower: {building.gameObject.name}");
                }
                else if (buildingName.Contains("fountain"))
                {
                    fountain = building;
                    Debug.Log($"[Moon2 Restoration] Wired Fountain: {building.gameObject.name}");
                }
                else if (buildingName.Contains("crystal") && buildingName.Contains("hall"))
                {
                    crystalHall = building;
                    Debug.Log($"[Moon2 Restoration] Wired Crystal Hall: {building.gameObject.name}");
                }
                else if (buildingName.Contains("ley") || buildingName.Contains("chamber"))
                {
                    leyChamber = building;
                    Debug.Log($"[Moon2 Restoration] Wired Ley Chamber: {building.gameObject.name}");
                }
            }

            Debug.Log($"[Moon2 Restoration] Discovered {RestoredCount}/5 buildings");
        }

        void HandleBuildingRestored(string buildingId)
        {
            if (!buildingId.Contains("moon2_")) return;  // Not a Moon 2 building

            if (_restoredBuildings.Contains(buildingId)) return;  // Already restored

            _restoredBuildings.Add(buildingId);
            Debug.Log($"[Moon2 Restoration] Building restored: {buildingId} ({RestoredCount}/5)");

            // Trigger specific restoration sequence based on building
            RestoreBuilding(buildingId);

            // Fire event
            OnBuildingRestored?.Invoke(buildingId);

            // Check if all 5 complete
            if (RestoredCount >= 5 && !_allBuildingsRestored)
            {
                CompleteAllRestorations();
            }
        }

        void RestoreBuilding(string buildingId)
        {
            string buildingKey = buildingId.ToLowerInvariant();
            Vector3 buildingCenter = GetBuildingCenter(buildingId);

            // Mud-to-crystal transformation VFX (Moon 2 version of mud-to-restored)
            if (VFXController.Instance != null)
            {
                VFXController.Instance.PlayMudToRestoredCathedralTransformation(buildingCenter, 1.0f);
            }

            // Lunar shadow purge visuals (golden burn)
            if (Moon2LunarVisualsManager.Instance != null)
            {
                Moon2LunarVisualsManager.Instance.PlayLunarShadowPurgeCathedralTransformation(buildingCenter, 1.0f);
            }

            // Atmospheric audio bloom
            if (Moon2AtmosphereAudioManager.Instance != null)
            {
                // Handled by AtmosphereAudioManager subscribed to GameEvents.OnBuildingRestored
            }

            // Award RS
            AetherResonanceSystem.Instance?.AddResonance(15f, $"Restored {buildingId}");

            // Haptic feedback
            HapticFeedbackManager.Instance?.PlayDiscovery();

            // Grant blessing via progression system (auto-triggered by specific methods)
            if (buildingKey.Contains("cathedral") || buildingKey.Contains("dome"))
            {
                Moon2ProgressionSystem.Instance?.OnCathedralDomePurged();
                ShowRestorationBanner("Cathedral Dome Restored", "The fractal architecture breathes. Micro-giant access granted.");
            }
            else if (buildingKey.Contains("bell") || buildingKey.Contains("tower"))
            {
                Moon2ProgressionSystem.Instance?.OnBellTowerPurged();
                ShowRestorationBanner("Bell Tower Restored", "Scalar waves pulse across the sky. The caverns answer.");
            }
            else if (buildingKey.Contains("fountain"))
            {
                Moon2ProgressionSystem.Instance?.OnFountainPurged();
                ShowRestorationBanner("Fountain Restored", "Pure water springs from crystal veins. The thirst ends.");
            }
            else if (buildingKey.Contains("crystal") && buildingKey.Contains("hall"))
            {
                Moon2ProgressionSystem.Instance?.OnCrystalHallPurged();
                ShowRestorationBanner("Crystal Hall Restored", "The fractal core burns golden. All light remembers you.");
            }
            else if (buildingKey.Contains("ley") || buildingKey.Contains("chamber"))
            {
                Moon2ProgressionSystem.Instance?.OnLeyChamberPurged();
                ShowRestorationBanner("Ley Chamber Restored", "The leyline bond solidifies. Moon 1's song echoes here.");
            }

            // Audio: restoration harmonic
            AudioManager.Instance?.PlaySFX2D("Moon2_RestoreHarmonic");
        }

        Vector3 GetBuildingCenter(string buildingId)
        {
            // Try to find actual building position
            InteractableBuilding building = null;

            string key = buildingId.ToLowerInvariant();
            if (key.Contains("cathedral") || key.Contains("dome")) building = cathedralDome;
            else if (key.Contains("bell")) building = bellTower;
            else if (key.Contains("fountain")) building = fountain;
            else if (key.Contains("crystal")) building = crystalHall;
            else if (key.Contains("ley")) building = leyChamber;

            if (building != null)
            {
                return building.transform.position;
            }

            // Fallback: use Moon2ContentSpawner position
            if (Moon2ContentSpawner.Instance != null)
            {
                return Moon2ContentSpawner.Instance.transform.position;
            }

            return Vector3.zero;
        }

        void ShowRestorationBanner(string title, string description)
        {
            if (HUDController.Instance != null)
            {
                HUDController.Instance.ShowBanner(title, description, 5f);
            }
        }

        void CompleteAllRestorations()
        {
            _allBuildingsRestored = true;

            Debug.Log("[Moon2 Restoration] ★ ALL 5 BUILDINGS RESTORED ★ True Lunar Purifier unlocked!");

            // Grant capstone blessing
            Moon2ProgressionSystem.Instance?.GrantCapstoneIfAllPurged();

            // Massive RS reward
            AetherResonanceSystem.Instance?.AddResonance(100f, "Moon 2 Complete — All Buildings Restored");

            // Celebration VFX (golden dome over entire cathedral zone)
            if (VFXController.Instance != null)
            {
                VFXController.Instance.PlayAetherPulse(transform.position, 15f);
            }

            // Celebration audio
            AudioManager.Instance?.PlaySFX2D("Moon2_AllRestored");

            // Haptic crescendo
            HapticFeedbackManager.Instance?.PlayDiscovery();
            HapticFeedbackManager.Instance?.PlayDiscovery();

            // HUD banner
            ShowRestorationBanner(
                "Moon 2 Complete — Challenge of Shadows Conquered",
                "Five buildings sing. The fractal corruption breaks. Cassian watches, silent. The caverns remember your name.",
                8f
            );

            // Achievement
            AchievementSystem.Instance?.Unlock("moon2_complete");

            // Fire completion event
            OnAllBuildingsRestored?.Invoke();

            // Unlock Moon 3
            SaveManager.Instance?.SetMoonProgress(2, 100f);
        }

        void ShowRestorationBanner(string title, string description, float duration)
        {
            if (HUDController.Instance != null)
            {
                HUDController.Instance.ShowBanner(title, description, duration);
            }
        }

        /// <summary>
        /// Save/load support: restore building restoration state.
        /// </summary>
        public void LoadState(HashSet<string> restoredIds)
        {
            _restoredBuildings.Clear();
            foreach (var id in restoredIds)
            {
                _restoredBuildings.Add(id);
            }

            _allBuildingsRestored = RestoredCount >= 5;

            Debug.Log($"[Moon2 Restoration] Loaded state: {RestoredCount}/5 buildings restored");
        }

        public HashSet<string> SaveState()
        {
            return new HashSet<string>(_restoredBuildings);
        }
    }
}
