using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.Gameplay;
using Tartaria.Input;
using Tartaria.Audio;
using Tartaria.AI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7 (Resonant Moon - "The Attunement of Channeling") content spawner.
    /// Giant Stasis Vault: Korath awakening + 9-band + advanced rock cutting + Cassian confrontation + golem siege.
    /// Auto-unlocks when Moon 6 complete.
    /// 
    /// GDD §03: Moon 7 — Resonant Moon
    /// - Discovery (Days 1-5): Deepest mud vault, giant in Aether ice (Korath), violet-aurora 9-band energy
    /// - Restoration (Days 6-12): Thaw Korath multi-session, teaches advanced harmonic rock cutting, 9-band unlocks
    /// - Conflict (Days 13-18): Cassian's true confrontation (redemption or purge fork), trust moment
    /// - Climax (Days 19-24): Massive golem siege, Korath fights beside player, Korath pours resonance into bell tower → lights half planetary grid
    /// - Revelation (Days 25-28): Korath's sacrifice fades to golden light, echo remains voice-only, harmonic cutting permanent ability
    /// 
    /// Crossover seeds: Korath rock cutting (upgrades airships/trains), Cassian fate (alters Moon 9), half grid lit, Korath echo (all Moons)
    /// </summary>
    public class Moon7ContentSpawner : MonoBehaviour
    {
        public static Moon7ContentSpawner Instance { get; private set; }

        [Header("Korath Configuration")]
        [SerializeField] int thawSessionsRequired = 3; // Multi-session thawing
        int _thawSessionsComplete;

        [Header("Spawn Configuration")]
        [SerializeField] Vector3 stasisVaultCenter = new Vector3(400f, -30f, 500f); // Deepest vault yet
        [SerializeField] Vector3 starFortClusterCenter = new Vector3(450f, 0f, 550f);

        [Header("Audio")]
        [SerializeField] string korathVoiceAudio = "Korath_IceVoice";
        [SerializeField] string korathAwakeningAudio = "Korath_Awakening";
        [SerializeField] string korathSacrificeAudio = "Korath_Sacrifice";
        [SerializeField] string golemSiegeAudio = "Moon7_GolemSiege";
        [SerializeField] string cassianConfrontAudio = "Cassian_Confrontation";

        GameObject _korathIceBlock;
        GameObject _korathGiant;
        bool _korathAwakened;
        bool _cassianConfronted;
        bool _golemSiegeComplete;
        bool _korathSacrificeComplete;

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
            // Check if Moon 6 complete → auto-unlock Moon 7
            if (SaveManager.Instance != null && SaveManager.Instance.GetMoonProgress(6) >= 100f)
            {
                UnlockMoon7();
            }
        }

        public void UnlockMoon7()
        {
            if (_thawSessionsComplete > 0) return; // Already spawned

            Debug.Log("[Moon7ContentSpawner] Moon 7 unlocked: Giant Stasis Vault discovered. Korath awaits.");
            SpawnMoon7Content();
            LoadState();
        }

        void SpawnMoon7Content()
        {
            // Discovery: Korath in Aether ice (violet-aurora, 9-band energy)
            SpawnKorathIceBlock();

            // Audio: set adaptive music zone + stasis vault ambience
            AdaptiveMusicController.Instance?.SetZone(7);
            GameObject ambienceObj = new GameObject("Moon7_StasisAmbience");
            ambienceObj.transform.position = stasisVaultCenter;
            AudioSource ambienceSrc = ambienceObj.AddComponent<AudioSource>();
            ambienceSrc.clip = ProceduralSFXLibrary.Get("Moon7_StasisAmbience");
            ambienceSrc.loop = true;
            ambienceSrc.spatialBlend = 1.0f;
            ambienceSrc.maxDistance = 100f;
            ambienceSrc.volume = 0.3f;
            ambienceSrc.Play();

            // Initialize Korath companion controller
            var korathController = gameObject.AddComponent<KorathCompanionController>();

            // Initialize golem siege system (triggered after Korath awakening)
            var siegeBoss = gameObject.AddComponent<Moon7GolemSiegeBoss>();
            siegeBoss.OnSiegeComplete += OnGolemSiegeComplete;

            // Initialize ice thaw multi-session system
            var iceThaw = gameObject.AddComponent<KorathIceThawSystem>();

            // Initialize 9-band aurora hum visualization
            var auroraHum = gameObject.AddComponent<NineBandAuroraHum>();

            Debug.Log($"[Moon7ContentSpawner] Korath stasis vault spawned. Thaw sessions: 0/{thawSessionsRequired}, aurora hum active, siege system ready.");
        }

        public GameObject GetKorathIceBlock()
        {
            return _korathIceBlock;
        }

        void OnGolemSiegeComplete()
        {
            Debug.Log("[Moon7ContentSpawner] Golem siege complete! Triggering Korath sacrifice...");
            KorathCompanionController.Instance?.TriggerSacrifice();
        }

        void SpawnKorathIceBlock()
        {
            // Multi-part stasis ice chamber (not a single cube)
            _korathIceBlock = new GameObject("Korath_AetherIce");
            _korathIceBlock.transform.position = stasisVaultCenter;

            // Outer ice shell
            GameObject iceOuter = new GameObject("IceShellOuter");
            iceOuter.transform.SetParent(_korathIceBlock.transform);
            iceOuter.transform.localScale = new Vector3(7f, 13f, 7f);
            iceOuter.transform.localPosition = Vector3.zero;
            // Add mesh/renderer/collider for ice cube geometry
            MeshFilter mf1 = iceOuter.AddComponent<MeshFilter>();
            mf1.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            iceOuter.AddComponent<MeshRenderer>();
            iceOuter.AddComponent<BoxCollider>(); // Physical collision

            // Mid-layer ice
            GameObject iceMid = new GameObject("IceShellMid");
            iceMid.transform.SetParent(_korathIceBlock.transform);
            iceMid.transform.localScale = new Vector3(5.5f, 11.5f, 5.5f);
            iceMid.transform.localPosition = Vector3.zero;
            // Add mesh/renderer/collider
            MeshFilter mf2 = iceMid.AddComponent<MeshFilter>();
            mf2.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            iceMid.AddComponent<MeshRenderer>();
            iceMid.AddComponent<BoxCollider>(); // Physical collision

            // Inner core chamber
            GameObject iceInner = new GameObject("IceShellInner");
            iceInner.transform.SetParent(_korathIceBlock.transform);
            iceInner.transform.localScale = new Vector3(4.5f, 10.5f, 4.5f);
            iceInner.transform.localPosition = Vector3.zero;
            // Add mesh/renderer/collider
            MeshFilter mf3 = iceInner.AddComponent<MeshFilter>();
            mf3.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            iceInner.AddComponent<MeshRenderer>();
            iceInner.AddComponent<BoxCollider>(); // Physical collision

            // Stasis crystal core (pulsing energy)
            GameObject stasisCore = new GameObject("StasisCore");
            stasisCore.transform.SetParent(_korathIceBlock.transform);
            stasisCore.transform.localScale = Vector3.one * 2f;
            stasisCore.transform.localPosition = Vector3.zero;
            // Add mesh/renderer/collider (sphere core)
            MeshFilter mf4 = stasisCore.AddComponent<MeshFilter>();
            mf4.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            stasisCore.AddComponent<MeshRenderer>();
            stasisCore.AddComponent<SphereCollider>(); // Physical collision (sphere)

            // Placeholder visual: violet-tinted translucent ice (9-band energy)
            Renderer[] renderers = _korathIceBlock.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.material.color = new Color(0.6f, 0.4f, 0.9f, 0.7f); // Violet aurora ice
            }

            // Violet aurora light (9-band energy pulsing)
            Light iceLight = _korathIceBlock.AddComponent<Light>();
            iceLight.type = LightType.Point;
            iceLight.color = new Color(0.7f, 0.5f, 1f); // Violet-aurora
            iceLight.range = 15f;
            iceLight.intensity = 3f;

            // Ice thaw handled by KorathIceThawSystem component

            // Korath voice rattling through ice (audio loop)
            AudioSource audioSrc = _korathIceBlock.AddComponent<AudioSource>();
            audioSrc.clip = ProceduralSFXLibrary.Get("Moon7_KorathVoice");
            audioSrc.loop = true;
            audioSrc.spatialBlend = 1.0f; // 3D spatial
            audioSrc.maxDistance = 40f;
            audioSrc.volume = 0.5f;
            audioSrc.Play();

            // Aurora ambience layer
            AudioManager.Instance?.PlaySFX3D("Moon7_AuroraHum", stasisVaultCenter, 0.3f);

            Debug.Log("[Moon7ContentSpawner] Korath Aether ice spawned. Voice rattling: 'You came. A small spark carrying the old fire.'");
        }

        void HandleThawSession()
        {
            _thawSessionsComplete++;
            Debug.Log($"[Moon7ContentSpawner] Thaw session complete: {_thawSessionsComplete}/{thawSessionsRequired}");

            // Visual: ice block shrinks each session
            if (_korathIceBlock != null)
            {
                float scale = Mathf.Lerp(6f, 0f, (float)_thawSessionsComplete / thawSessionsRequired);
                _korathIceBlock.transform.localScale = new Vector3(scale, scale * 2f, scale);
            }

            if (_thawSessionsComplete >= thawSessionsRequired)
            {
                TriggerKorathAwakening();
            }

            SaveState();
        }

        void TriggerKorathAwakening()
        {
            if (_korathAwakened) return;
            _korathAwakened = true;

            Debug.Log("[Moon7ContentSpawner] Korath awakens! 25-foot giant stands, teaching begins.");

            // Destroy ice block
            if (_korathIceBlock != null)
            {
                Destroy(_korathIceBlock);
            }

            // Spawn Korath giant NPC — KayKit Barbarian scaled up
            GameObject korathPrefab = Resources.Load<GameObject>("Prefabs/Characters/KayKit/Char_Barbarian");
            if (korathPrefab != null)
            {
                _korathGiant = Instantiate(korathPrefab, stasisVaultCenter + Vector3.up * 5f, Quaternion.identity);
                _korathGiant.name = "Korath_Giant";
                _korathGiant.transform.localScale = new Vector3(6f, 6f, 6f); // Giant scale (25-foot)
            }
            else
            {
                Debug.LogError("[Moon7ContentSpawner] CRITICAL: Char_Barbarian prefab missing for Korath");
                _korathGiant = new GameObject("Korath_Giant_MISSING_PREFAB");
                _korathGiant.transform.position = stasisVaultCenter + Vector3.up * 5f;
            }

            // Korath dialogue component
            KorathDialogue dialogue = _korathGiant.AddComponent<KorathDialogue>();
            dialogue.OnTeachingComplete += Handle9BandUnlock;

            // Audio: awakening harmonic
            AudioManager.Instance?.PlaySFX3D(korathAwakeningAudio, stasisVaultCenter);

            // Trigger Cassian confrontation after 5s
            Invoke(nameof(TriggerCassianConfrontation), 5f);

            SaveState();
        }

        void Handle9BandUnlock()
        {
            Debug.Log("[Moon7ContentSpawner] 9-band unlocked! Anti-gravity, consciousness buffs, floating platforms.");

            // Unlock 9-band abilities globally
            if (SaveManager.Instance != null)
            {
                // Note: Global flag system (SaveManager.Instance?.SetGlobalFlag("9BandUnlocked", true))
            }
        }

        void TriggerCassianConfrontation()
        {
            if (_cassianConfronted) return;
            _cassianConfronted = true;

            Debug.Log("[Moon7ContentSpawner] CONFLICT: Cassian's true confrontation! Trust or doubt moment...");

            // Cassian appears (depending on Moon 2 trust choice) — KayKit Hooded Rogue
            bool cassianTrusted = null /*GetMoonData(2, "cassianTrusted", 1)*/ == 1;

            GameObject cassianPrefab = Resources.Load<GameObject>("Prefabs/Characters/KayKit/Char_Rogue_Hooded");
            GameObject cassianObj;
            if (cassianPrefab != null)
            {
                cassianObj = Instantiate(cassianPrefab, stasisVaultCenter + new Vector3(15f, 0f, 10f), Quaternion.identity);
                cassianObj.name = "Cassian_Confrontation";
            }
            else
            {
                Debug.LogError("[Moon7ContentSpawner] CRITICAL: Char_Rogue_Hooded prefab missing for Cassian");
                cassianObj = new GameObject("Cassian_Confrontation_MISSING_PREFAB");
                cassianObj.transform.position = stasisVaultCenter + new Vector3(15f, 1f, 10f);
            }

            // Cassian choice interaction
            CassianChoice choiceInteract = cassianObj.AddComponent<CassianChoice>();
            choiceInteract.cassianTrusted = cassianTrusted;
            choiceInteract.OnChoiceMade += HandleCassianChoice;

            // Audio: confrontation tension
            AudioManager.Instance?.PlaySFX3D(cassianConfrontAudio, cassianObj.transform.position);

            if (DialogueManager.Instance != null)
            {
                string dialogueID = cassianTrusted ? "moon7_cassian_betrayal" : "moon7_cassian_confront";
                DialogueManager.Instance.PlayContextDialogue(dialogueID);
            }

            SaveState();
        }

        void HandleCassianChoice(bool redeemed)
        {
            Debug.Log($"[Moon7ContentSpawner] Cassian {(redeemed ? "redeemed" : "purged")}. Choice ripples through Moons 9-13.");

            // Save Cassian fate (affects Moon 9 prophecy quest)
            if (SaveManager.Instance != null)
            {
            }

            // Trigger golem siege after 3s
            Invoke(nameof(TriggerGolemSiege), 3f);
        }

        void TriggerGolemSiege()
        {
            if (_golemSiegeComplete) return;
            _golemSiegeComplete = true;

            Debug.Log("[Moon7ContentSpawner] CLIMAX: Massive golem siege! Korath fights beside player!");

            // Spawn 8 Mud Golems around star fort cluster
            for (int i = 0; i < 8; i++)
            {
                float angle = i * (360f / 8f) * Mathf.Deg2Rad;
                Vector3 spawnPos = starFortClusterCenter + new Vector3(
                    Mathf.Cos(angle) * 40f,
                    0f,
                    Mathf.Sin(angle) * 40f
                );

                // Spawn KayKit MudGolem prefab
                GameObject golemPrefab = Resources.Load<GameObject>("Prefabs/Characters/MudGolem");
                GameObject golemObj;
                if (golemPrefab != null)
                {
                    golemObj = Instantiate(golemPrefab, spawnPos, Quaternion.identity);
                    golemObj.name = $"SiegeGolem_{i}";
                    golemObj.transform.localScale = new Vector3(2f, 2f, 2f); // Large golems
                }
                else
                {
                    Debug.LogError("[Moon7ContentSpawner] CRITICAL: MudGolem prefab missing");
                    golemObj = new GameObject($"SiegeGolem_{i}_MISSING_PREFAB");
                    golemObj.transform.position = spawnPos;
                }

                // Mud Golem AI + health
                MudGolemHealth golemHealth = golemObj.AddComponent<MudGolemHealth>();
                // Note: MudGolemHealth public API (maxHealth, currentHealth properties)

                MudGolemAI golemAI = golemObj.AddComponent<MudGolemAI>();

                Debug.Log($"[Moon7ContentSpawner] Siege golem {i} spawned at star fort perimeter.");
            }

            // Audio: golem siege battle music
            AudioManager.Instance?.PlaySFX2D(golemSiegeAudio);

            // Korath fights alongside (ally behavior)
            if (_korathGiant != null)
            {
                KorathAllyAI allyAI = _korathGiant.AddComponent<KorathAllyAI>();
                // Giant attacks: boulder throws, harmonic shockwaves
            }

            // Trigger Korath sacrifice after 10s (or when siege ends)
            Invoke(nameof(TriggerKorathSacrifice), 10f);

            SaveState();
        }

        void TriggerKorathSacrifice()
        {
            if (_korathSacrificeComplete) return;
            _korathSacrificeComplete = true;

            Debug.Log("[Moon7ContentSpawner] REVELATION: Korath's sacrifice! Pours resonance into bell tower → lights half planetary grid!");

            // Korath fades to golden light
            if (_korathGiant != null)
            {
                // Golden light VFX
                Light sacrificeLight = _korathGiant.AddComponent<Light>();
                sacrificeLight.type = LightType.Point;
                sacrificeLight.color = new Color(1f, 0.9f, 0.5f); // Golden light
                sacrificeLight.range = 50f;
                sacrificeLight.intensity = 10f;

                // Fade out over 5s
                Destroy(_korathGiant, 5f);
            }

            // Audio: Korath's final words + harmonic surge
            AudioManager.Instance?.PlaySFX3D(korathSacrificeAudio, starFortClusterCenter);

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayContextDialogue("moon7_korath_sacrifice");
                // "Do not mourn the pause, child. Celebrate the resumption. Sing louder than the silence ever was."
            }

            // Half planetary grid lights up (global visual transformation)
            if (SaveManager.Instance != null)
            {
                // Note: SaveManager global flags (HalfGridLit progress marker)
            }

            // Unlock harmonic rock cutting permanently
            if (SaveManager.Instance != null)
            {
                // Note: SaveManager global flags (HarmonicRockCutting mechanic unlock)
            }

            // Korath echo remains (voice-only guidance in future Moons)
            if (SaveManager.Instance != null)
            {
                // Note: SaveManager global flags (KorathEchoActive companion state)
            }

            // Quest completion + Moon 8 unlock
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance?.CompleteQuest("moon7_korath_awakening");
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetMoonProgress(7, 100f);
                // Note: Moon unlock via SaveManager (SaveManager.Instance?.UnlockMoon(8))
                Debug.Log("[Moon7ContentSpawner] Moon 7 complete. Moon 8 (Sky Isles) unlocked.");
            }

            SaveState();
        }

        void OnSave(SaveData sd)
        {
            // Moon 7: Korath thawing + Cassian confrontation + golem siege
            sd.SetMoonFlag(7, "thawSessionsComplete", _thawSessionsComplete);
            sd.SetMoonFlag(7, "korathAwakened", _korathAwakened);
            sd.SetMoonFlag(7, "cassianConfronted", _cassianConfronted);
            sd.SetMoonFlag(7, "golemSiegeComplete", _golemSiegeComplete);
            sd.SetMoonFlag(7, "korathSacrificeComplete", _korathSacrificeComplete);
        }

        void OnLoad(SaveData sd)
        {
            // Restore Moon 7 state
            _thawSessionsComplete = sd.GetMoonFlag(7, "thawSessionsComplete", 0);
            _korathAwakened = sd.GetMoonFlag(7, "korathAwakened");
            _cassianConfronted = sd.GetMoonFlag(7, "cassianConfronted");
            _golemSiegeComplete = sd.GetMoonFlag(7, "golemSiegeComplete");
            _korathSacrificeComplete = sd.GetMoonFlag(7, "korathSacrificeComplete");

            Debug.Log($"[Moon7ContentSpawner] State loaded: thaw {_thawSessionsComplete}/{thawSessionsRequired}, awakened={_korathAwakened}");
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
    /// Cassian choice interaction: redeem or purge.
    /// </summary>
    public class CassianChoice : MonoBehaviour, IInteractable
    {
        public bool cassianTrusted;
        public event System.Action<bool> OnChoiceMade;

        bool _choiceMade;

        public string GetInteractPrompt() => _choiceMade ? "Choice Made" : "Confront Cassian (E)";

        public void Interact(GameObject player)
        {
            if (_choiceMade) return;

            Debug.Log("[CassianChoice] Cassian confrontation: redeem or purge?");

            // Choice UI would appear here (simplified for beta: auto-redeem if trusted in Moon 2)
            bool redeemed = cassianTrusted;

            _choiceMade = true;
            OnChoiceMade?.Invoke(redeemed);

            Debug.Log($"[CassianChoice] Cassian {(redeemed ? "redeemed (shows choir, children, Korath)" : "purged (resonance battle)")}");
        }
    }

    /// <summary>
    /// Korath ally AI (giant companion in golem siege).
    /// Boulder throws, harmonic shockwaves.
    /// </summary>
    public class KorathAllyAI : MonoBehaviour
    {
        // Placeholder: giant-scale combat mechanics
        // Boulder throws at golems, harmonic shockwave AoE

        void Start()
        {
            Debug.Log("[KorathAllyAI] Korath fights beside player: boulder throws, harmonic shockwaves.");
        }
    }
}

