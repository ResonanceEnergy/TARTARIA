using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.Gameplay;
using Tartaria.Input;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5 (Overtone Moon - "The Radiance of Empowerment") content spawner.
    /// White City Echo District: 5 pavilions restoration + 6-band introduction + airship dock + Captain Thorne.
    /// Auto-unlocks when Moon 4 complete.
    /// 
    /// GDD §03: Moon 5 — Overtone Moon
    /// - Discovery (Days 1-5): Buried White City pavilions glow, Thorne radio signal arrives
    /// - Restoration (Days 6-12): Restore 5 pavilions with golden-ratio templates, 6-band healing auras
    /// - Conflict (Days 13-18): Reset demolition crews attack, floating platforms defend
    /// - Climax (Days 19-24): Ionized fountain aurora holograms replay pre-flood festivals
    /// - Revelation (Days 25-28): Spire fragment completes central spire → multi-zone ley-line bridge, Thorne incoming
    /// 
    /// Crossover seeds: Airship dock (blooms Moon 8), Fair Circuit live-ops zone, 6-band healing (available all zones)
    /// </summary>
    public class Moon5ContentSpawner : MonoBehaviour
    {
        public static Moon5ContentSpawner Instance { get; private set; }

        [Header("White City Pavilions")]
        [SerializeField] int totalPavilions = 5;
        int _pavilionsRestored;

        [Header("Spawn Configuration")]
        [SerializeField] Vector3 whiteCityCenter = new Vector3(200f, 0f, 300f);
        [SerializeField] float pavilionRadius = 60f;

        [Header("Audio")]
        [SerializeField] string thorneCrackleAudio = "Thorne_RadioCrackle";
        [SerializeField] string pavilionRestoreAudio = "Moon5_PavilionRestore";
        [SerializeField] string auroraHologramAudio = "Moon5_AuroraHologram";
        [SerializeField] string centralSpireCompleteAudio = "Moon5_CentralSpireComplete";

        List<WhiteCityPavilion> _activePavilions = new List<WhiteCityPavilion>();
        GameObject _thorneCommunicator;
        bool _thorneIntroduced;
        bool _auroraHologramTriggered;
        bool _centralSpireComplete;

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
            // Check if Moon 4 complete → auto-unlock Moon 5
            if (SaveManager.Instance != null && SaveManager.Instance.GetMoonProgress(4) >= 100f)
            {
                UnlockMoon5();
            }
        }

        public void UnlockMoon5()
        {
            if (_pavilionsRestored > 0) return; // Already spawned

            Debug.Log("[Moon5ContentSpawner] Moon 5 unlocked: White City Echo District rises from the mud.");
            SpawnMoon5Content();
            LoadState();
        }

        void SpawnMoon5Content()
        {
            // Discovery: Captain Thorne radio communicator (crackling signal)
            SpawnThorneCommunicator();

            // Restoration: 5 pavilions (golden-ratio Beaux-Arts structures)
            SpawnPavilions();

            // Initialize floating platform progression
            var platforms = gameObject.AddComponent<FloatingPlatformProgression>();
            platforms.InitializePlatforms();

            // Initialize companion combat abilities (Thorne airship support)
            var companionCombat = gameObject.AddComponent<CompanionCombatAbilities>();

            // Spawn Captain Thorne NPC (after radio introduction)
            SpawnThorneNPC();

            Debug.Log($"[Moon5ContentSpawner] White City pavilions spawned: 5 pavilions, floating platforms, Thorne communicator active.");
        }

        void SpawnThorneNPC()
        {
            GameObject thorneObj = new GameObject("CaptainThorne_NPC");
            thorneObj.transform.position = whiteCityCenter + new Vector3(10f, 0f, 0f);

            var thorne = thorneObj.AddComponent<CaptainThorneNPC>();

            // Visual: humanoid capsule (airship captain)
            var filter = thorneObj.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
            
            var renderer = thorneObj.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.4f, 0.3f, 0.2f); // Leather jacket brown

            thorneObj.transform.localScale = new Vector3(0.8f, 2f, 0.8f);

            Debug.Log("[Moon5ContentSpawner] Captain Thorne NPC spawned.");
        }

        void SpawnThorneCommunicator()
        {
            _thorneCommunicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _thorneCommunicator.name = "Thorne_Radio_Communicator";
            _thorneCommunicator.transform.position = whiteCityCenter + new Vector3(0f, 2f, 0f);
            _thorneCommunicator.transform.localScale = new Vector3(0.6f, 0.8f, 0.3f); // Radio box shape

            // Placeholder visual: dark gray with pulsing light (radio signal)
            Renderer rend = _thorneCommunicator.GetComponent<Renderer>();
            rend.material.color = new Color(0.2f, 0.2f, 0.25f); // Dark gray metal

            // Pulsing light (radio signal active)
            Light radioLight = _thorneCommunicator.AddComponent<Light>();
            radioLight.type = LightType.Point;
            radioLight.color = Color.yellow; // Radio signal glow
            radioLight.range = 5f;
            radioLight.intensity = 1.5f;

            // IInteractable: introduces Thorne via radio dialogue
            ThorneRadioInteract radioInteract = _thorneCommunicator.AddComponent<ThorneRadioInteract>();
            radioInteract.OnThorneIntroduced += HandleThorneIntroduced;

            Debug.Log("[Moon5ContentSpawner] Thorne radio communicator spawned at White City center.");
        }

        void SpawnPavilions()
        {
            // 5 pavilions in pentagon formation around White City center
            for (int i = 0; i < totalPavilions; i++)
            {
                float angle = i * (360f / totalPavilions) * Mathf.Deg2Rad;
                Vector3 pos = whiteCityCenter + new Vector3(
                    Mathf.Cos(angle) * pavilionRadius,
                    0f,
                    Mathf.Sin(angle) * pavilionRadius
                );

                GameObject pavilionObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pavilionObj.name = $"WhiteCity_Pavilion_{i}";
                pavilionObj.transform.position = pos;
                pavilionObj.transform.localScale = new Vector3(8f, 6f, 8f); // Large Beaux-Arts pavilion

                // Placeholder visual: white marble (Beaux-Arts style)
                Renderer rend = pavilionObj.GetComponent<Renderer>();
                rend.material.color = new Color(0.95f, 0.95f, 0.98f); // White marble

                // Warm light inside (healing aura placeholder)
                Light healingLight = pavilionObj.AddComponent<Light>();
                healingLight.type = LightType.Point;
                healingLight.color = new Color(1f, 0.85f, 0.6f); // Warm golden healing glow
                healingLight.range = 12f;
                healingLight.intensity = 0f; // Off until restored

                // WhiteCityPavilion component: IInteractable restoration mechanic
                WhiteCityPavilion pavilion = pavilionObj.AddComponent<WhiteCityPavilion>();
                pavilion.pavilionIndex = i;
                pavilion.OnRestored += OnPavilionRestored;

                // Add amplification field component (buffs player in radius)
                var amplificationField = pavilionObj.AddComponent<PavilionAmplificationField>();

                _activePavilions.Add(pavilion);
            }

            Debug.Log($"[Moon5ContentSpawner] {totalPavilions} White City pavilions generated.");
        }

        void OnPavilionRestored(WhiteCityPavilion pavilion)
        {
            _pavilionsRestored++;
            Debug.Log($"[Moon5ContentSpawner] Pavilion {pavilion.pavilionIndex} restored. Progress: {_pavilionsRestored}/{totalPavilions}");

            // Audio: pavilion restoration chime
            AudioManager.Instance?.PlaySFX2D(pavilionRestoreAudio);

            // 6-band healing aura now radiates from pavilion
            Light healingLight = pavilion.GetComponent<Light>();
            if (healingLight != null)
            {
                healingLight.intensity = 2f; // Warm golden glow active
            }

            // Check climax trigger: all 5 pavilions restored
            if (_pavilionsRestored >= totalPavilions)
            {
                TriggerAuroraHologram();
            }

            SaveState();
        }

        void HandleThorneIntroduced()
        {
            _thorneIntroduced = true;
            Debug.Log("[Moon5ContentSpawner] Thorne introduced via radio: 'About time someone lit a signal. Coming in.'");
            SaveState();
        }

        void TriggerAuroraHologram()
        {
            if (_auroraHologramTriggered) return;
            _auroraHologramTriggered = true;

            Debug.Log("[Moon5ContentSpawner] CLIMAX: Ionized fountain aurora holograms replay pre-flood festivals!");

            // Climax VFX: aurora hologram above White City center
            GameObject auroraObj = new GameObject("Aurora_Hologram_VFX");
            auroraObj.transform.position = whiteCityCenter + new Vector3(0f, 15f, 0f);

            // Particle system: aurora-colored hologram (green/blue/purple shimmer)
            ParticleSystem ps = auroraObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 8f;
            main.startSpeed = 0.5f;
            main.startSize = 3f;
            main.loop = true;
            main.maxParticles = 2000;

            var emission = ps.emission;
            emission.rateOverTime = 250f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 20f;

            // Aurora gradient: green → blue → purple
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.3f, 1f, 0.5f), 0f), // Green
                    new GradientColorKey(new Color(0.4f, 0.6f, 1f), 0.5f), // Blue
                    new GradientColorKey(new Color(0.8f, 0.4f, 1f), 1f) // Purple
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.6f, 0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0.3f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

            // Audio: aurora hologram harmonic
            AudioManager.Instance?.PlaySFX3D(auroraHologramAudio, whiteCityCenter);

            // Trigger revelation after 5s
            Invoke(nameof(TriggerRevelation), 5f);

            SaveState();
        }

        void TriggerRevelation()
        {
            if (_centralSpireComplete) return;
            _centralSpireComplete = true;

            Debug.Log("[Moon5ContentSpawner] REVELATION: Spire fragment from Moon 1 completes White City central spire!");

            // Spawn central spire at White City center (bridges zones)
            GameObject spireObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spireObj.name = "WhiteCity_CentralSpire";
            spireObj.transform.position = whiteCityCenter + new Vector3(0f, 12f, 0f);
            spireObj.transform.localScale = new Vector3(1.5f, 12f, 1.5f); // Tall spire

            // Golden material (ley-line bridge active)
            Renderer spireRend = spireObj.GetComponent<Renderer>();
            spireRend.material.color = new Color(1f, 0.9f, 0.4f); // Golden glow

            // Spire light (multi-zone ley-line corridor)
            Light spireLight = spireObj.AddComponent<Light>();
            spireLight.type = LightType.Point;
            spireLight.color = new Color(1f, 0.85f, 0.3f); // Golden light
            spireLight.range = 30f;
            spireLight.intensity = 4f;

            // Audio: central spire completion harmonic
            AudioManager.Instance?.PlaySFX3D(centralSpireCompleteAudio, whiteCityCenter);

            // Thorne's signal strengthens (cockpit dialogue callback)
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayContextDialogue("moon5_thorne_incoming");
            }

            // Quest completion + Moon 6 unlock
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance?.CompleteQuest("moon5_white_city_restoration");
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetMoonProgress(5, 100f);
                // Note: Moon unlock via SaveManager (SaveManager.Instance?.UnlockMoon(6))
                Debug.Log("[Moon5ContentSpawner] Moon 5 complete. Moon 6 (Living Library) unlocked.");
            }

            SaveState();
        }

        void OnSave(SaveData sd)
        {
            // Moon 5: White City pavilions + Thorne intro
            sd.SetMoonFlag(5, "pavilionsRestored", _pavilionsRestored);
            sd.SetMoonFlag(5, "thorneIntroduced", _thorneIntroduced);
            sd.SetMoonFlag(5, "auroraHologramTriggered", _auroraHologramTriggered);
            sd.SetMoonFlag(5, "centralSpireComplete", _centralSpireComplete);
        }

        void OnLoad(SaveData sd)
        {
            // Restore Moon 5 state
            _pavilionsRestored = sd.GetMoonFlag(5, "pavilionsRestored", 0);
            _thorneIntroduced = sd.GetMoonFlag(5, "thorneIntroduced");
            _auroraHologramTriggered = sd.GetMoonFlag(5, "auroraHologramTriggered");
            _centralSpireComplete = sd.GetMoonFlag(5, "centralSpireComplete");

            Debug.Log($"[Moon5ContentSpawner] State loaded: {_pavilionsRestored}/{totalPavilions} pavilions restored.");
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
    /// White City pavilion restoration mechanics.
    /// IInteractable: player tunes pavilion → 6-band healing aura activates.
    /// </summary>
    public class WhiteCityPavilion : MonoBehaviour, IInteractable
    {
        public int pavilionIndex;
        public event System.Action<WhiteCityPavilion> OnRestored;

        bool _isRestored;

        public string GetInteractPrompt() => _isRestored ? "Pavilion Restored" : "Restore Pavilion (Hold E)";

        public void Interact(GameObject player)
        {
            if (_isRestored) return;

            Debug.Log($"[WhiteCityPavilion] Pavilion {pavilionIndex} restoration begun (instant for beta).");
            StartRestoration();
        }

        void StartRestoration()
        {
            _isRestored = true;

            // Restoration VFX: golden shimmer particles
            GameObject vfxObj = new GameObject("PavilionRestore_VFX");
            vfxObj.transform.position = transform.position + Vector3.up * 3f;

            ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 2f;
            main.startSpeed = 1.5f;
            main.startSize = 0.4f;
            main.loop = false;
            main.maxParticles = 300;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 300)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(8f, 6f, 8f);

            Renderer rend = ps.GetComponent<Renderer>();
            if (rend != null && rend.material != null)
            {
                rend.material.color = new Color(1f, 0.9f, 0.4f); // Golden shimmer
            }

            Destroy(vfxObj, 3f);

            // Notify spawner
            OnRestored?.Invoke(this);

            Debug.Log($"[WhiteCityPavilion] Pavilion {pavilionIndex} restored. 6-band healing aura active.");
        }
    }

    /// <summary>
    /// Thorne radio communicator interaction.
    /// First approach: plays Thorne's crackling radio introduction dialogue.
    /// </summary>
    public class ThorneRadioInteract : MonoBehaviour, IInteractable
    {
        public event System.Action OnThorneIntroduced;

        bool _thorneIntroduced;

        public string GetInteractPrompt() => _thorneIntroduced ? "Radio Signal Active" : "Listen to Radio (E)";

        public void Interact(GameObject player)
        {
            if (_thorneIntroduced) return;

            Debug.Log("[ThorneRadioInteract] Thorne radio intro: 'About time someone lit a signal. I've been circling for two centuries.'");

            // Play Thorne intro dialogue
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayContextDialogue("moon5_thorne_radio_intro");
            }

            // Audio: radio crackle SFX
            AudioManager.Instance?.PlaySFX3D("Thorne_RadioCrackle", transform.position);

            _thorneIntroduced = true;
            OnThorneIntroduced?.Invoke();
        }
    }
}

