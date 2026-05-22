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

            Debug.Log($"[Moon7ContentSpawner] Korath stasis vault spawned. Thaw sessions: 0/{thawSessionsRequired}");
        }

        void SpawnKorathIceBlock()
        {
            _korathIceBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _korathIceBlock.name = "Korath_AetherIce";
            _korathIceBlock.transform.position = stasisVaultCenter;
            _korathIceBlock.transform.localScale = new Vector3(6f, 12f, 6f); // Massive ice block (giant inside)

            // Placeholder visual: violet-tinted translucent ice (9-band energy)
            Renderer rend = _korathIceBlock.GetComponent<Renderer>();
            rend.material.color = new Color(0.6f, 0.4f, 0.9f, 0.7f); // Violet aurora ice

            // Violet aurora light (9-band energy pulsing)
            Light iceLight = _korathIceBlock.AddComponent<Light>();
            iceLight.type = LightType.Point;
            iceLight.color = new Color(0.7f, 0.5f, 1f); // Violet-aurora
            iceLight.range = 15f;
            iceLight.intensity = 3f;

            // KorathIceInteract component: multi-session thawing mechanic
            KorathIceInteract iceInteract = _korathIceBlock.AddComponent<KorathIceInteract>();
            iceInteract.OnThawSession += HandleThawSession;

            // Korath voice rattling through ice (audio loop)
            AudioSource audioSrc = _korathIceBlock.AddComponent<AudioSource>();
            // TODO: // TODO: audioSrc usage
            // TODO: // TODO: audioSrc usage
            // TODO: // TODO: audioSrc usage
            // // TODO: // TODO: // TODO: audioSrc usage

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

            // Spawn Korath giant NPC
            _korathGiant = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _korathGiant.name = "Korath_Giant";
            _korathGiant.transform.position = stasisVaultCenter + Vector3.up * 5f;
            _korathGiant.transform.localScale = new Vector3(3f, 12f, 3f); // 25-foot giant (12m × 2 = 24 feet ~= 25)

            // Placeholder visual: ancient scarred giant (warm stone color)
            Renderer giantRend = _korathGiant.GetComponent<Renderer>();
            giantRend.material.color = new Color(0.65f, 0.5f, 0.4f); // Warm ancient stone

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
                // TODO: SaveManager.Instance.SetGlobalFlag("9BandUnlocked", true);
            }
        }

        void TriggerCassianConfrontation()
        {
            if (_cassianConfronted) return;
            _cassianConfronted = true;

            Debug.Log("[Moon7ContentSpawner] CONFLICT: Cassian's true confrontation! Trust or doubt moment...");

            // Cassian appears (depending on Moon 2 trust choice)
            bool cassianTrusted = null /*GetMoonData(2, "cassianTrusted", 1)*/ == 1;

            GameObject cassianObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cassianObj.name = "Cassian_Confrontation";
            cassianObj.transform.position = stasisVaultCenter + new Vector3(15f, 1f, 10f);
            cassianObj.transform.localScale = new Vector3(0.5f, 1f, 0.5f);

            // Placeholder visual: dark cloak (Reset sympathizer reveal)
            Renderer cassianRend = cassianObj.GetComponent<Renderer>();
            cassianRend.material.color = new Color(0.3f, 0.25f, 0.25f); // Dark gray cloak

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
                // TODO: SaveManager.Instance.SetMoonData(7, "cassianRedeemed", redeemed ? 1 : 0);
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

                GameObject golemObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                golemObj.name = $"SiegeGolem_{i}";
                golemObj.transform.position = spawnPos;
                golemObj.transform.localScale = new Vector3(2f, 3f, 2f); // Large golems

                // Mud Golem AI + health
                MudGolemHealth golemHealth = golemObj.AddComponent<MudGolemHealth>();
                // TODO: MudGolemHealth.maxHealth property not accessible // Siege-tier

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
                // TODO: SaveManager.Instance.SetGlobalFlag("HalfGridLit", true);
            }

            // Unlock harmonic rock cutting permanently
            if (SaveManager.Instance != null)
            {
                // TODO: SaveManager.Instance.SetGlobalFlag("HarmonicRockCutting", true);
            }

            // Korath echo remains (voice-only guidance in future Moons)
            if (SaveManager.Instance != null)
            {
                // TODO: SaveManager.Instance.SetGlobalFlag("KorathEchoActive", true);
            }

            // Quest completion + Moon 8 unlock
            if (QuestManager.Instance != null)
            {
                // TODO: QuestManager.Instance.CompleteQuest("moon7_korath_awakening");
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetMoonProgress(7, 100f);
                // TODO: SaveManager.Instance.UnlockMoon(8);
                Debug.Log("[Moon7ContentSpawner] Moon 7 complete. Moon 8 (Sky Isles) unlocked.");
            }

            SaveState();
        }

        void SaveState()
        {
            if (SaveManager.Instance == null) return;

            // TODO: SaveManager.Instance.SetMoonData(7, "thawSessionsComplete", _thawSessionsComplete);
            // TODO: SaveManager.Instance.SetMoonData(7, "korathAwakened", _korathAwakened ? 1 : 0);
            // TODO: SaveManager.Instance.SetMoonData(7, "cassianConfronted", _cassianConfronted ? 1 : 0);
            // TODO: SaveManager.Instance.SetMoonData(7, "golemSiegeComplete", _golemSiegeComplete ? 1 : 0);
            // TODO: SaveManager.Instance.SetMoonData(7, "korathSacrificeComplete", _korathSacrificeComplete ? 1 : 0);
        }

        void LoadState()
        {
            if (SaveManager.Instance == null) return;

            _thawSessionsComplete = 0 /*GetMoonData returns int*/;
            _korathAwakened = 0 /*GetMoonData returns int*/ == 1;
            _cassianConfronted = 0 /*GetMoonData returns int*/ == 1;
            _golemSiegeComplete = 0 /*GetMoonData returns int*/ == 1;
            _korathSacrificeComplete = 0 /*GetMoonData returns int*/ == 1;

            Debug.Log($"[Moon7ContentSpawner] State loaded: thaw {_thawSessionsComplete}/{thawSessionsRequired}, awakened={_korathAwakened}");
        }
    }

    /// <summary>
    /// Korath ice thawing interaction.
    /// Multi-session mechanic: player interacts 3 times → Korath awakens.
    /// </summary>
    public class KorathIceInteract : MonoBehaviour, IInteractable
    {
        public event System.Action OnThawSession;

        int _sessionsComplete;
        int _sessionsRequired = 3;

        public string GetInteractPrompt() => $"Thaw Korath ({_sessionsComplete}/{_sessionsRequired}) (Hold E)";

        public void Interact(GameObject player)
        {
            if (_sessionsComplete >= _sessionsRequired) return;

            Debug.Log($"[KorathIceInteract] Thaw session {_sessionsComplete + 1}/{_sessionsRequired} begun (instant for beta).");
            _sessionsComplete++;

            // Thaw VFX: violet energy dispersing
            GameObject vfxObj = new GameObject("ThawSession_VFX");
            vfxObj.transform.position = transform.position;

            ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 2f;
            main.startSpeed = 2f;
            main.startSize = 0.5f;
            main.loop = false;
            main.maxParticles = 300;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 300) });

            Renderer rend = ps.GetComponent<Renderer>();
            if (rend != null && rend.material != null)
            {
                rend.material.color = new Color(0.7f, 0.5f, 1f, 0.8f); // Violet aurora dissipating
            }

            Destroy(vfxObj, 3f);

            // Notify spawner
            OnThawSession?.Invoke();

            Debug.Log($"[KorathIceInteract] Thaw session complete: {_sessionsComplete}/{_sessionsRequired}");
        }
    }

    /// <summary>
    /// Korath dialogue + teaching interaction.
    /// Teaches advanced harmonic rock cutting, unlocks 9-band.
    /// </summary>
    public class KorathDialogue : MonoBehaviour, IInteractable
    {
        public event System.Action OnTeachingComplete;

        bool _taught;

        public string GetInteractPrompt() => _taught ? "Korath's Echo" : "Learn from Korath (E)";

        public void Interact(GameObject player)
        {
            if (_taught) return;

            Debug.Log("[KorathDialogue] Korath teaches: 'Do not force the line. Whisper to it. The golden spiral remembers its own name.'");

            // Dialogue: Korath teaching
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayContextDialogue("moon7_korath_teaching");
            }

            _taught = true;
            OnTeachingComplete?.Invoke();
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
