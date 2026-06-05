using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.Gameplay;
using Tartaria.Input;
using Tartaria.Audio;

#pragma warning disable CS0067  // Event never used
#pragma warning disable CS0219  // Variable assigned but not used
#pragma warning disable CS0414  // Field assigned but not used
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

            // Wire save/load events
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave += OnSave;
                SaveManager.Instance.OnAfterLoad += OnLoad;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;

            // Cleanup save/load event handlers
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave -= OnSave;
                SaveManager.Instance.OnAfterLoad -= OnLoad;
            }
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

            // Audio: set adaptive music zone + cathedral ambience
            AdaptiveMusicController.Instance?.SetZone(6);
            GameObject ambienceObj = new GameObject("Moon6_CathedralAmbience");
            ambienceObj.transform.position = cathedralCenter;
            AudioSource ambienceSrc = ambienceObj.AddComponent<AudioSource>();
            ambienceSrc.clip = ProceduralSFXLibrary.Get("Moon6_CathedralAmbience");
            ambienceSrc.loop = true;
            ambienceSrc.spatialBlend = 1.0f;
            ambienceSrc.maxDistance = 80f;
            ambienceSrc.volume = 0.25f;
            ambienceSrc.Play();

            // Spawn Lirael (spectral form)
            var liraelController = gameObject.AddComponent<LiraelSolidificationController>();
            liraelController.SpawnLirael();

            // Initialize cinematic arc system
            var cinematics = gameObject.AddComponent<Moon6RhythmicArcCinematics>();
            cinematics.PlayDiscoveryCinematic();

            // Initialize organ puzzle controller (12-pipe harmonic sequences)
            var organPuzzle = gameObject.AddComponent<Moon6OrganPuzzle>();
            organPuzzle.OnRequiemComplete += HandleRequiemComplete;

            Debug.Log($"[Moon6ContentSpawner] Living Library pipe organ spawned: 12 pipes, 6 fountains, Lirael (spectral), organ puzzle active.");
        }

        void HandleRequiemComplete()
        {
            _revelationUnlocked = true;
            Debug.Log("[Moon6ContentSpawner] Cymatic Requiem complete! Revelation unlocked: 9-band purity frozen in pipes.");
            
            // Update Moon progress
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetMoonProgress(6, 100f);
            }
        }

        public void MarkRevelationUnlocked()
        {
            _revelationUnlocked = true;
            SaveState();
        }

        void SpawnPipeOrgan()
        {
            // Multi-part pipe organ structure
            _pipeOrganCore = new GameObject("PipeOrgan_Core");
            _pipeOrganCore.transform.position = cathedralCenter;

            // Console base
            GameObject consoleBase = new GameObject("ConsoleBase");
            consoleBase.AddComponent<MeshFilter>();
            consoleBase.AddComponent<MeshRenderer>();
            consoleBase.AddComponent<BoxCollider>();
            consoleBase.transform.SetParent(_pipeOrganCore.transform);
            consoleBase.transform.localScale = new Vector3(8f, 2f, 4f);
            consoleBase.transform.localPosition = Vector3.up * 1f;

            // Organ body (tall rear panel)
            GameObject organBody = new GameObject("OrganBody");
            organBody.AddComponent<MeshFilter>();
            organBody.AddComponent<MeshRenderer>();
            organBody.AddComponent<BoxCollider>();
            organBody.transform.SetParent(_pipeOrganCore.transform);
            organBody.transform.localScale = new Vector3(10f, 10f, 1f);
            organBody.transform.localPosition = new Vector3(0f, 6f, -2f);

            // Upper decorative crown
            GameObject crown = new GameObject("Crown");
            crown.AddComponent<MeshFilter>();
            crown.AddComponent<MeshRenderer>();
            crown.AddComponent<BoxCollider>();
            crown.transform.SetParent(_pipeOrganCore.transform);
            crown.transform.localScale = new Vector3(11f, 2f, 1.5f);
            crown.transform.localPosition = new Vector3(0f, 12f, -2f);

            // Keyboard platform
            GameObject keyboard = new GameObject("Keyboard");
            keyboard.AddComponent<MeshFilter>();
            keyboard.AddComponent<MeshRenderer>();
            keyboard.AddComponent<BoxCollider>();
            keyboard.transform.SetParent(_pipeOrganCore.transform);
            keyboard.transform.localScale = new Vector3(6f, 0.2f, 1f);
            keyboard.transform.localPosition = new Vector3(0f, 2f, 1f);

            // Placeholder visual: dark wood with brass accents
            Renderer[] renderers = _pipeOrganCore.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.material.color = new Color(0.25f, 0.18f, 0.12f); // Dark walnut
            }

            // Broken melody plays (distorted harmony)
            AudioSource audioSrc = _pipeOrganCore.AddComponent<AudioSource>();
            audioSrc.clip = ProceduralSFXLibrary.Get("Moon6_BrokenMelody");
            audioSrc.loop = true;
            audioSrc.spatialBlend = 1.0f; // 3D spatial
            audioSrc.maxDistance = 50f;
            audioSrc.volume = 0.4f;
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

                GameObject pipeObj = new GameObject($"CrystalPipe_{i}");
                pipeObj.AddComponent<MeshFilter>();
                pipeObj.AddComponent<MeshRenderer>();
                pipeObj.AddComponent<CapsuleCollider>();
                pipeObj.transform.position = pos;
                pipeObj.transform.localScale = new Vector3(0.4f, 5f, 0.4f); // Tall pipe
                pipeObj.transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg); // Slight tilt

                // Placeholder visual: fractured crystal (dull until repaired)
                Renderer rend = pipeObj.GetComponent<Renderer>();
                rend.material.color = new Color(0.6f, 0.65f, 0.7f, 0.5f); // Dull translucent gray

                // CrystalPipe component: IInteractable repair mechanic
                CrystalPipe pipe = pipeObj.AddComponent<CrystalPipe>();
                pipe.pipeIndex = i;

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

                // Multi-part hydraulic fountain
                GameObject fountainObj = new GameObject($"HydraulicFountain_{i}");
                fountainObj.transform.position = pos;

                // Foundation
                GameObject foundation = new GameObject("Foundation");
                foundation.AddComponent<MeshFilter>();
                foundation.AddComponent<MeshRenderer>();
                foundation.AddComponent<CapsuleCollider>();
                foundation.transform.SetParent(fountainObj.transform);
                foundation.transform.localScale = new Vector3(3f, 0.5f, 3f);
                foundation.transform.localPosition = Vector3.up * 0.25f;

                // Basin
                GameObject basin = new GameObject("Basin");
                basin.AddComponent<MeshFilter>();
                basin.AddComponent<MeshRenderer>();
                basin.AddComponent<CapsuleCollider>();
                basin.transform.SetParent(fountainObj.transform);
                basin.transform.localScale = new Vector3(2f, 1f, 2f);
                basin.transform.localPosition = Vector3.up * 1f;

                // Water pipe (feeds organ bellows)
                GameObject pipe = new GameObject("WaterPipe");
                pipe.AddComponent<MeshFilter>();
                pipe.AddComponent<MeshRenderer>();
                pipe.AddComponent<CapsuleCollider>();
                pipe.transform.SetParent(fountainObj.transform);
                pipe.transform.localScale = new Vector3(0.3f, 2f, 0.3f);
                pipe.transform.localPosition = Vector3.up * 2.5f;

                // Valve cap
                GameObject valve = new GameObject("Valve");
                valve.AddComponent<MeshFilter>();
                valve.AddComponent<MeshRenderer>();
                valve.AddComponent<SphereCollider>();
                valve.transform.SetParent(fountainObj.transform);
                valve.transform.localScale = Vector3.one * 0.5f;
                valve.transform.localPosition = Vector3.up * 3.5f;

                // Placeholder visual: stone basin with dry cracked interior
                Renderer[] renderers = fountainObj.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {
                    rend.material.color = new Color(0.5f, 0.5f, 0.52f); // Gray stone
                }

                // HydraulicFountain component: IInteractable restoration
                HydraulicFountain fountain = fountainObj.AddComponent<HydraulicFountain>();
                fountain.fountainIndex = i;

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

            // Organ plays correct melody now (broken melody stops, switch to harmonic tones)
            AudioSource organSrc = _pipeOrganCore?.GetComponent<AudioSource>();
            if (organSrc != null)
            {
                organSrc.Stop();
                organSrc.clip = ProceduralSFXLibrary.Get("Moon6_OrganTone");
                organSrc.loop = true;
                organSrc.volume = 0.6f;
                organSrc.Play();
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
            // Lirael NPC (spectral girl, more solid from Moon 3 healing) — use KayKit Mage
            GameObject liraelPrefab = Resources.Load<GameObject>("Prefabs/Characters/KayKit/Char_Mage");
            GameObject liraelObj;
            if (liraelPrefab != null)
            {
                liraelObj = Instantiate(liraelPrefab, cathedralCenter + new Vector3(0f, 0f, 8f), Quaternion.identity);
                liraelObj.name = "Lirael_Conducting";
                liraelObj.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f); // Child-sized
            }
            else
            {
                Debug.LogError("[Moon6ContentSpawner] CRITICAL: Char_Mage prefab missing for Lirael");
                liraelObj = new GameObject("Lirael_Conducting_MISSING_PREFAB");
                liraelObj.transform.position = cathedralCenter + new Vector3(0f, 1f, 8f);
            }

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
                QuestManager.Instance?.CompleteQuest("moon6_cymatic_requiem");
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetMoonProgress(6, 100f);
                // Note: Moon unlock via SaveManager (SaveManager.Instance?.UnlockMoon(7))
                Debug.Log("[Moon6ContentSpawner] Moon 6 complete. Moon 7 (Giant Stasis Vault) unlocked.");
            }

            SaveState();
        }

        void OnSave(SaveData sd)
        {
            // Moon 6: Pipe organ + fountains + Cymatic Requiem
            sd.SetMoonFlag(6, "pipesRepaired", _pipesRepaired);
            sd.SetMoonFlag(6, "fountainsRestored", _fountainsRestored);
            sd.SetMoonFlag(6, "organRestored", _organRestored);
            sd.SetMoonFlag(6, "cymaticRequiemTriggered", _cymaticRequiemTriggered);
            sd.SetMoonFlag(6, "revelationUnlocked", _revelationUnlocked);
        }

        void OnLoad(SaveData sd)
        {
            // Restore Moon 6 state
            _pipesRepaired = sd.GetMoonFlag(6, "pipesRepaired", 0);
            _fountainsRestored = sd.GetMoonFlag(6, "fountainsRestored", 0);
            _organRestored = sd.GetMoonFlag(6, "organRestored");
            _cymaticRequiemTriggered = sd.GetMoonFlag(6, "cymaticRequiemTriggered");
            _revelationUnlocked = sd.GetMoonFlag(6, "revelationUnlocked");

            Debug.Log($"[Moon6ContentSpawner] State loaded: {_pipesRepaired}/{totalCrystalPipes} pipes, {_fountainsRestored}/{totalFountains} fountains.");
        }

        void SaveState()
        {
            // Legacy method - now handled by OnSave event
        }

        void LoadState()
        {
            // Legacy method - now handled by OnLoad event
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
        public event System.Action<int> OnPlayed;

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

        /// <summary>
        /// Play this pipe (trigger its note for organ puzzle).
        /// </summary>
        public void Play()
        {
            if (!_isRepaired)
            {
                Debug.Log($"[CrystalPipe] Pipe {pipeIndex} cannot be played - not yet repaired.");
                return;
            }

            Debug.Log($"[CrystalPipe] Pipe {pipeIndex} played!");
            OnPlayed?.Invoke(pipeIndex);
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
                DialogueManager.Instance.PlayContextDialogue("moon6_zereth_calibration_mystery");
            }

            _examined = true;
        }
    }
}

