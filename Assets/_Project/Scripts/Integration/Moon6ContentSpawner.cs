using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.Gameplay;
using Tartaria.Input;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 (Rhythmic Moon - "The Equality of Flow") content spawner.
    /// Sunken Cathedral Sanctum: Full pipe organ symphony conduction + multi-fountain networks + Lirael conducts choirs.
    /// Auto-unlocks when Moon 5 complete.
    /// 
    /// GDD §03: Moon 6 — Rhythmic Moon
    /// - Discovery (Days 1-5): Sunken cathedral organ plays broken melody, summons mud storms
    /// - Restoration (Days 6-12): Repair crystal pipes, fountain hydraulic bellows, conduct symphony
    /// - Conflict (Days 13-18): Dissonance cracks mid-performance, micro-golems spawn from pipes
    /// - Climax (Days 19-24): Cymatic Requiem, city-wide ionized mist rain, Lirael solo + choir
    /// - Revelation (Days 25-28): 9-band purity frozen note in pipes, Zereth's flawless calibration deepens mystery
    /// 
    /// Crossover seeds: Lirael conducts choirs (passive buff), organ prerequisite for Moon 12 bell sync, Zereth mystery
    /// </summary>
    public class Moon6ContentSpawner : MonoBehaviour
    {
        public static Moon6ContentSpawner Instance { get; private set; }

        [Header("Pipe Organ Configuration")]
        [SerializeField] int totalCrystalPipes = 12;
        int _pipesRepaired;

        [Header("Fountain Network")]
        [SerializeField] int totalFountains = 6;
        int _fountainsRestored;

        [Header("Spawn Configuration")]
        [SerializeField] Vector3 cathedralCenter = new Vector3(300f, -15f, 400f); // Deep underground
        [SerializeField] float fountainRadius = 25f;

        [Header("Audio")]
        [SerializeField] string brokenMelodyAudio = "Moon6_BrokenMelody";
        [SerializeField] string pipeRepairAudio = "Moon6_PipeRepair";
        [SerializeField] string cymaticRequiemAudio = "Moon6_CymaticRequiem";
        [SerializeField] string liraelChoirAudio = "Moon6_LiraelChoir";

        List<CrystalPipe> _activePipes = new List<CrystalPipe>();
        List<HydraulicFountain> _activeFountains = new List<HydraulicFountain>();
        GameObject _pipeOrganCore;
        bool _organRestored;
        bool _cymaticRequiemTriggered;
        bool _revelationUnlocked;

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
            // Check if Moon 5 complete → auto-unlock Moon 6
            if (SaveManager.Instance != null && SaveManager.Instance.GetMoonProgress(5) >= 100f)
            {
                UnlockMoon6();
            }
        }

        public void UnlockMoon6()
        {
            if (_pipesRepaired > 0) return; // Already spawned

            Debug.Log("[Moon6ContentSpawner] Moon 6 unlocked: Sunken Cathedral Sanctum discovered beneath White City.");
            SpawnMoon6Content();
            LoadState();
        }

        void SpawnMoon6Content()
        {
            // Discovery: massive pipe organ (broken, playing backwards melody)
            SpawnPipeOrgan();

            // Restoration: 12 crystal pipes to repair
            SpawnCrystalPipes();

            // Restoration: 6 hydraulic fountains feeding organ bellows
            SpawnHydraulicFountains();

            Debug.Log($"[Moon6ContentSpawner] Living Library pipe organ spawned: 12 pipes, 6 fountains.");
        }

        void SpawnPipeOrgan()
        {
            _pipeOrganCore = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _pipeOrganCore.name = "PipeOrgan_Core";
            _pipeOrganCore.transform.position = cathedralCenter;
            _pipeOrganCore.transform.localScale = new Vector3(10f, 12f, 5f); // Massive organ console

            // Placeholder visual: dark wood with brass accents
            Renderer rend = _pipeOrganCore.GetComponent<Renderer>();
            rend.material.color = new Color(0.25f, 0.18f, 0.12f); // Dark walnut

            // Broken melody plays (distorted harmony)
            AudioSource audioSrc = _pipeOrganCore.AddComponent<AudioSource>();
            audioSrc.loop = true;
            audioSrc.spatialBlend = 1f;
            audioSrc.maxDistance = 50f;
            // audioSrc.clip would load brokenMelodyAudio
            audioSrc.Play();

            Debug.Log("[Moon6ContentSpawner] Pipe organ core spawned. Broken melody playing.");
        }

        void SpawnCrystalPipes()
        {
            // 12 pipes arranged in arc behind organ
            for (int i = 0; i < totalCrystalPipes; i++)
            {
                float angle = (i - totalCrystalPipes / 2f) * 8f * Mathf.Deg2Rad; // Arc spread
                Vector3 pos = cathedralCenter + new Vector3(
                    Mathf.Sin(angle) * 8f,
                    6f,
                    -5f + Mathf.Cos(angle) * 2f
                );

                GameObject pipeObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pipeObj.name = $"CrystalPipe_{i}";
                pipeObj.transform.position = pos;
                pipeObj.transform.localScale = new Vector3(0.4f, 5f, 0.4f); // Tall pipe
                pipeObj.transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg); // Slight tilt

                // Placeholder visual: fractured crystal (dull until repaired)
                Renderer rend = pipeObj.GetComponent<Renderer>();
                rend.material.color = new Color(0.6f, 0.65f, 0.7f, 0.5f); // Dull translucent gray

                // CrystalPipe component: IInteractable repair mechanic
                CrystalPipe pipe = pipeObj.AddComponent<CrystalPipe>();
                pipe.pipeIndex = i;
                pipe.OnRepaired += OnPipeRepaired;

                _activePipes.Add(pipe);
            }

            Debug.Log($"[Moon6ContentSpawner] {totalCrystalPipes} crystal pipes generated.");
        }

        void SpawnHydraulicFountains()
        {
            // 6 fountains in hexagon around organ (feed hydraulic bellows)
            for (int i = 0; i < totalFountains; i++)
            {
                float angle = i * (360f / totalFountains) * Mathf.Deg2Rad;
                Vector3 pos = cathedralCenter + new Vector3(
                    Mathf.Cos(angle) * fountainRadius,
                    -2f,
                    Mathf.Sin(angle) * fountainRadius
                );

                GameObject fountainObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                fountainObj.name = $"HydraulicFountain_{i}";
                fountainObj.transform.position = pos;
                fountainObj.transform.localScale = new Vector3(2f, 1f, 2f); // Fountain basin

                // Placeholder visual: stone basin with dry cracked interior
                Renderer rend = fountainObj.GetComponent<Renderer>();
                rend.material.color = new Color(0.5f, 0.5f, 0.52f); // Gray stone

                // HydraulicFountain component: IInteractable restoration
                HydraulicFountain fountain = fountainObj.AddComponent<HydraulicFountain>();
                fountain.fountainIndex = i;
                fountain.OnRestored += OnFountainRestored;

                _activeFountains.Add(fountain);
            }

            Debug.Log($"[Moon6ContentSpawner] {totalFountains} hydraulic fountains generated.");
        }

        void OnPipeRepaired(CrystalPipe pipe)
        {
            _pipesRepaired++;
            Debug.Log($"[Moon6ContentSpawner] Crystal pipe {pipe.pipeIndex} repaired. Progress: {_pipesRepaired}/{totalCrystalPipes}");

            // Audio: pipe repair harmonic chime
            AudioManager.Instance?.PlaySFX3D(pipeRepairAudio, pipe.transform.position);

            CheckOrganRestoration();
            SaveState();
        }

        void OnFountainRestored(HydraulicFountain fountain)
        {
            _fountainsRestored++;
            Debug.Log($"[Moon6ContentSpawner] Hydraulic fountain {fountain.fountainIndex} restored. Progress: {_fountainsRestored}/{totalFountains}");

            CheckOrganRestoration();
            SaveState();
        }

        void CheckOrganRestoration()
        {
            if (_organRestored) return;
            if (_pipesRepaired < totalCrystalPipes || _fountainsRestored < totalFountains) return;

            _organRestored = true;
            Debug.Log("[Moon6ContentSpawner] Pipe organ fully restored! Ready for Cymatic Requiem.");

            // Organ plays correct melody now (broken melody stops)
            AudioSource organsrc = _pipeOrganCore?.GetComponent<AudioSource>();
            if (audioSrc != null)
            {
                audioSrc.Stop();
            }

            // Trigger climax after 3s
            Invoke(nameof(TriggerCymaticRequiem), 3f);
        }

        void TriggerCymaticRequiem()
        {
            if (_cymaticRequiemTriggered) return;
            _cymaticRequiemTriggered = true;

            Debug.Log("[Moon6ContentSpawner] CLIMAX: Cymatic Requiem! Lirael conducts, city-wide ionized mist rain falls!");

            // Climax VFX: ionized mist rain (cyan particles falling slowly)
            GameObject mistObj = new GameObject("IonizedMist_VFX");
            mistObj.transform.position = cathedralCenter + new Vector3(0f, 30f, 0f);

            ParticleSystem ps = mistObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 12f;
            main.startSpeed = 0.8f;
            main.startSize = 0.8f;
            main.loop = true;
            main.maxParticles = 5000;
            main.gravityModifier = 0.3f; // Slow fall

            var emission = ps.emission;
            emission.rateOverTime = 400f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(100f, 2f, 100f); // Wide rain field

            Renderer rend = ps.GetComponent<Renderer>();
            if (rend != null && rend.material != null)
            {
                rend.material.color = new Color(0.5f, 0.9f, 1f, 0.6f); // Cyan ionized mist
            }

            // Audio: Cymatic Requiem symphony (full organ + fountains)
            AudioManager.Instance?.PlaySFX3D(cymaticRequiemAudio, cathedralCenter);

            // Lirael conducts choir (adopted children from Moon 3 join)
            SpawnLiraelChoirScene();

            // Trigger revelation after 8s
            Invoke(nameof(TriggerRevelation), 8f);

            SaveState();
        }

        void SpawnLiraelChoirScene()
        {
            // Lirael NPC (transparent girl, now more solid from Moon 3 healing)
            GameObject liraelObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            liraelObj.name = "Lirael_Conducting";
            liraelObj.transform.position = cathedralCenter + new Vector3(0f, 1f, 8f);
            liraelObj.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f); // Child-sized

            // Placeholder visual: translucent blue-white (spectral girl, 60% solid now)
            Renderer liraelRend = liraelObj.GetComponent<Renderer>();
            liraelRend.material.color = new Color(0.7f, 0.85f, 1f, 0.6f); // More solid than Moon 3

            // Lirael sings (audio cue)
            AudioManager.Instance?.PlaySFX3D(liraelChoirAudio, liraelObj.transform.position);

            Debug.Log("[Moon6ContentSpawner] Lirael conducts children's choir. Cathedral heals.");
        }

        void TriggerRevelation()
        {
            if (_revelationUnlocked) return;
            _revelationUnlocked = true;

            Debug.Log("[Moon6ContentSpawner] REVELATION: 9-band purity frozen note discovered! Zereth's flawless calibration...");

            // Revelation: examine organ tuning records (IInteractable on organ core)
            OrganTuningRecords recordsInteract = _pipeOrganCore.AddComponent<OrganTuningRecords>();

            // Quest completion + Moon 7 unlock
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.CompleteQuest("moon6_cymatic_requiem");
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetMoonProgress(6, 100f);
                SaveManager.Instance.UnlockMoon(7);
                Debug.Log("[Moon6ContentSpawner] Moon 6 complete. Moon 7 (Giant Stasis Vault) unlocked.");
            }

            SaveState();
        }

        void SaveState()
        {
            if (SaveManager.Instance == null) return;

            SaveManager.Instance.SetMoonData(6, "pipesRepaired", _pipesRepaired);
            SaveManager.Instance.SetMoonData(6, "fountainsRestored", _fountainsRestored);
            SaveManager.Instance.SetMoonData(6, "organRestored", _organRestored ? 1 : 0);
            SaveManager.Instance.SetMoonData(6, "cymaticRequiemTriggered", _cymaticRequiemTriggered ? 1 : 0);
            SaveManager.Instance.SetMoonData(6, "revelationUnlocked", _revelationUnlocked ? 1 : 0);
        }

        void LoadState()
        {
            if (SaveManager.Instance == null) return;

            _pipesRepaired = SaveManager.Instance.GetMoonData(6, "pipesRepaired", 0);
            _fountainsRestored = SaveManager.Instance.GetMoonData(6, "fountainsRestored", 0);
            _organRestored = SaveManager.Instance.GetMoonData(6, "organRestored", 0) == 1;
            _cymaticRequiemTriggered = SaveManager.Instance.GetMoonData(6, "cymaticRequiemTriggered", 0) == 1;
            _revelationUnlocked = SaveManager.Instance.GetMoonData(6, "revelationUnlocked", 0) == 1;

            Debug.Log($"[Moon6ContentSpawner] State loaded: {_pipesRepaired}/{totalCrystalPipes} pipes, {_fountainsRestored}/{totalFountains} fountains.");
        }
    }

    /// <summary>
    /// Crystal pipe repair mechanics.
    /// IInteractable: player tunes pipe → harmonic note rings, pipe glows.
    /// </summary>
    public class CrystalPipe : MonoBehaviour, IInteractable
    {
        public int pipeIndex;
        public event System.Action<CrystalPipe> OnRepaired;

        bool _isRepaired;

        public string GetInteractPrompt() => _isRepaired ? "Pipe Restored" : "Repair Pipe (Hold E)";

        public void Interact(GameObject player)
        {
            if (_isRepaired) return;

            Debug.Log($"[CrystalPipe] Pipe {pipeIndex} repair begun (instant for beta).");
            StartRepair();
        }

        void StartRepair()
        {
            _isRepaired = true;

            // Visual: pipe turns clear brilliant crystal
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = new Color(0.85f, 0.95f, 1f, 0.9f); // Clear brilliant crystal
            }

            // Light: pipe glows cyan
            Light pipeLight = gameObject.AddComponent<Light>();
            pipeLight.type = LightType.Point;
            pipeLight.color = new Color(0.6f, 0.9f, 1f); // Cyan glow
            pipeLight.range = 6f;
            pipeLight.intensity = 1.5f;

            // Notify spawner
            OnRepaired?.Invoke(this);

            Debug.Log($"[CrystalPipe] Pipe {pipeIndex} repaired. Harmonic note resonates.");
        }
    }

    /// <summary>
    /// Hydraulic fountain restoration.
    /// IInteractable: player restores fountain → water flows, feeds organ bellows.
    /// </summary>
    public class HydraulicFountain : MonoBehaviour, IInteractable
    {
        public int fountainIndex;
        public event System.Action<HydraulicFountain> OnRestored;

        bool _isRestored;

        public string GetInteractPrompt() => _isRestored ? "Fountain Flowing" : "Restore Fountain (Hold E)";

        public void Interact(GameObject player)
        {
            if (_isRestored) return;

            Debug.Log($"[HydraulicFountain] Fountain {fountainIndex} restoration begun (instant for beta).");
            StartRestoration();
        }

        void StartRestoration()
        {
            _isRestored = true;

            // Visual: fountain fills with water (blue-white material)
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = new Color(0.5f, 0.7f, 0.9f); // Water blue
            }

            // Particle system: water spray upward (fountain active)
            GameObject sprayObj = new GameObject("FountainSpray_VFX");
            sprayObj.transform.SetParent(transform);
            sprayObj.transform.localPosition = Vector3.up;

            ParticleSystem ps = sprayObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 3f;
            main.startSize = 0.2f;
            main.loop = true;
            main.maxParticles = 200;

            var emission = ps.emission;
            emission.rateOverTime = 80f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;

            Renderer psRend = ps.GetComponent<Renderer>();
            if (psRend != null && psRend.material != null)
            {
                psRend.material.color = new Color(0.6f, 0.8f, 1f); // Water spray blue
            }

            // Notify spawner
            OnRestored?.Invoke(this);

            Debug.Log($"[HydraulicFountain] Fountain {fountainIndex} restored. Hydraulic bellows active.");
        }
    }

    /// <summary>
    /// Organ tuning records interaction (revelation trigger).
    /// Displays Zereth's flawless calibration data → deepens mystery.
    /// </summary>
    public class OrganTuningRecords : MonoBehaviour, IInteractable
    {
        bool _examined;

        public string GetInteractPrompt() => _examined ? "Records Examined" : "Examine Tuning Records (E)";

        public void Interact(GameObject player)
        {
            if (_examined) return;

            Debug.Log("[OrganTuningRecords] Last calibrated by Z. — Zereth. Calibration was FLAWLESS. If he was the villain...");

            // Dialogue: Zereth mystery deepens
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayDialogue("moon6_zereth_calibration_mystery");
            }

            _examined = true;
        }
    }
}
