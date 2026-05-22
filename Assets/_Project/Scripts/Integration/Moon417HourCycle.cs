using UnityEngine;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4 specific 17-hour day cycle mechanics.
    /// Extends base DayNightCycleController with Moon 4 clock tower + NPC schedules.
    /// 
    /// 17-hour breakdown (per GDD §04 Architecture Guide):
    /// - Dawn (Golden Hour): 1 hour, +10% RS from rose windows
    /// - Day: 8 hours, standard operation
    /// - Dusk (Violet Hour): 1 hour, bell towers resonate strongest (+20% broadcast)
    /// - Night: 6 hours, spires spark, mercury orbs glow
    /// - 17th Hour: 1 hour, ALL architecture at peak, secret mechanisms activate
    /// </summary>
    public class Moon417HourCycleController : MonoBehaviour
    {
        public static Moon417HourCycleController Instance { get; private set; }

        [Header("17-Hour Configuration")]
        [SerializeField] float hourDuration = 60f; // 60 real seconds = 1 Tartarian hour
        [SerializeField] Vector3 clockTowerPosition = new Vector3(100f, 15f, 80f);

        [Header("NPC Schedules")]
        [SerializeField] bool npcSchedulesActive = true;

        GameObject _clockTower;
        float _currentHour = 6f; // Start at dawn (hour 6)
        string _currentPhase = "Dawn";

        // Events for systems that care about hour transitions
        public event System.Action<int> OnHourChanged;
        public event System.Action On17thHourStart;
        public event System.Action On17thHourEnd;

        public int CurrentHour => Mathf.FloorToInt(_currentHour);
        public string CurrentPhase => _currentPhase;
        public float HourProgress => _currentHour % 1f;

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
            SpawnClockTower();
            LoadState();
        }

        void Update()
        {
            // Advance time
            float hourIncrement = Time.deltaTime / hourDuration;
            float previousHour = _currentHour;
            _currentHour += hourIncrement;

            // Wrap at 17 hours
            if (_currentHour >= 17f)
            {
                _currentHour -= 17f;
                Debug.Log("[17HourCycle] New day begun (hour reset to 0).");
            }

            // Detect hour transitions
            int prevHourInt = Mathf.FloorToInt(previousHour);
            int currHourInt = Mathf.FloorToInt(_currentHour);
            if (prevHourInt != currHourInt)
            {
                OnHourTransition(currHourInt);
            }

            // Update phase
            UpdatePhase();

            // 17th hour detection
            if (currHourInt == 16 && prevHourInt != 16)
            {
                On17thHourStart?.Invoke();
                Debug.Log("[17HourCycle] THE 17TH HOUR BEGINS! All architecture at peak resonance!");
                HUDController.Instance?.ShowObjective("⚡ 17TH HOUR ACTIVE ⚡");
            }
            else if (currHourInt == 0 && prevHourInt == 16)
            {
                On17thHourEnd?.Invoke();
                Debug.Log("[17HourCycle] 17th hour complete. Dawn approaches...");
            }
        }

        void SpawnClockTower()
        {
            _clockTower = new GameObject("Moon4_ClockTower");
            _clockTower.transform.position = clockTowerPosition;

            // Multi-part clock tower structure
            // Base foundation
            GameObject towerBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            towerBase.name = "TowerBase";
            towerBase.transform.SetParent(_clockTower.transform);
            towerBase.transform.localPosition = Vector3.up * 2f;
            towerBase.transform.localScale = new Vector3(4f, 4f, 4f);

            // Lower tower section
            GameObject towerLower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            towerLower.name = "TowerLower";
            towerLower.transform.SetParent(_clockTower.transform);
            towerLower.transform.localPosition = Vector3.up * 10f;
            towerLower.transform.localScale = new Vector3(3.5f, 12f, 3.5f);

            // Upper tower section
            GameObject towerUpper = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            towerUpper.name = "TowerUpper";
            towerUpper.transform.SetParent(_clockTower.transform);
            towerUpper.transform.localPosition = Vector3.up * 26f;
            towerUpper.transform.localScale = new Vector3(3f, 8f, 3f);

            // Clock chamber platform
            GameObject clockChamber = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clockChamber.name = "ClockChamber";
            clockChamber.transform.SetParent(_clockTower.transform);
            clockChamber.transform.localPosition = Vector3.up * 35f;
            clockChamber.transform.localScale = new Vector3(5f, 2f, 5f);

            // Clock face (sphere on tower top)
            GameObject clockFace = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            clockFace.name = "ClockFace";
            clockFace.transform.SetParent(_clockTower.transform);
            clockFace.transform.localPosition = new Vector3(0f, 37f, 0f);
            clockFace.transform.localScale = new Vector3(4f, 4f, 0.5f);

            // Tower materials
            Renderer[] towerRenderers = _clockTower.GetComponentsInChildren<Renderer>();
            Color stoneColor = new Color(0.6f, 0.55f, 0.5f);
            foreach (Renderer rend in towerRenderers)
            {
                if (rend.gameObject.name == "ClockFace")
                {
                    rend.material.color = new Color(1f, 0.95f, 0.8f); // Golden face
                }
                else
                {
                    rend.material.color = stoneColor; // Stone
                }
            }

            // Clock light (pulses during 17th hour)
            Light clockLight = _clockTower.AddComponent<Light>();
            clockLight.type = LightType.Point;
            clockLight.color = new Color(1f, 0.9f, 0.6f);
            clockLight.range = 30f;
            clockLight.intensity = 2f;

            Debug.Log("[17HourCycle] Clock tower spawned at Moon 4 star fort.");
        }

        void OnHourTransition(int newHour)
        {
            Debug.Log($"[17HourCycle] Hour transition: {newHour} ({GetHourName(newHour)})");
            OnHourChanged?.Invoke(newHour);

            // NPC schedule updates
            if (npcSchedulesActive)
            {
                UpdateNPCSchedules(newHour);
            }

            SaveState();
        }

        void UpdatePhase()
        {
            int hour = CurrentHour;

            if (hour >= 6 && hour < 7)
                _currentPhase = "Dawn (Golden Hour)";
            else if (hour >= 7 && hour < 15)
                _currentPhase = "Day";
            else if (hour >= 15 && hour < 16)
                _currentPhase = "Dusk (Violet Hour)";
            else if (hour == 16)
                _currentPhase = "17th Hour (PEAK)";
            else
                _currentPhase = "Night";
        }

        string GetHourName(int hour)
        {
            if (hour == 6) return "Dawn (Golden Hour)";
            if (hour == 15) return "Dusk (Violet Hour)";
            if (hour == 16) return "17th Hour";
            if (hour >= 7 && hour < 15) return "Day";
            return "Night";
        }

        void UpdateNPCSchedules(int hour)
        {
            // Echo garrison NPCs have schedules based on hour
            // Dawn (6): patrol perimeter
            // Day (7-14): guard bastions
            // Dusk (15): return to fort center
            // Night (16-5): rest/inactive
            // 17th Hour (16): all NPCs visible + special dialogue

            if (hour == 6)
            {
                Debug.Log("[17HourCycle] NPC Schedule: Dawn patrol activated.");
            }
            else if (hour == 15)
            {
                Debug.Log("[17HourCycle] NPC Schedule: Dusk return to fort center.");
            }
            else if (hour == 16)
            {
                Debug.Log("[17HourCycle] NPC Schedule: 17th Hour — all Echo soldiers manifest!");
            }
        }

        public void SaveState()
        {
            if (SaveManager.Instance == null) return;

            SaveManager.Instance.SetMoonData(4, "current_hour", Mathf.FloorToInt(_currentHour));
        }

        public void LoadState()
        {
            if (SaveManager.Instance == null) return;

            int savedHour = SaveManager.Instance.GetMoonData(4, "current_hour", -1);
            if (savedHour >= 0 && savedHour < 17)
            {
                _currentHour = savedHour;
                Debug.Log($"[17HourCycle] Loaded state: hour {savedHour}");
            }
        }

        /// <summary>
        /// Check if currently in the 17th hour (peak resonance time).
        /// </summary>
        public bool IsIn17thHour() => CurrentHour == 16;

        /// <summary>
        /// Get current RS multiplier based on time of day.
        /// Dawn: +10%, Dusk: +20%, 17th Hour: +50%, Night: standard, Day: standard
        /// </summary>
        public float GetTimeOfDayRSMultiplier()
        {
            int hour = CurrentHour;
            if (hour == 6) return 1.1f;  // Dawn +10%
            if (hour == 15) return 1.2f; // Dusk +20%
            if (hour == 16) return 1.5f; // 17th Hour +50%
            return 1.0f;
        }
    }
}
