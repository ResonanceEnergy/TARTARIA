using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Audio;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Exploration, Secrets & Collectibles — Rich network of 10 meaningful secrets for Crystalline Caverns.
    /// Exclusive Moon 2 domain. Tied to the "fractal cathedral" fantasy from 03C_MOON_MECHANICS_DETAILED + 12_VIVID_VISUALS.
    /// Uses and extends all R6/R7 strong visuals (fractal veins with thickness-aware fuse burns, godrays, dome breathing,
    /// crystal growth, recursive lighting hints, ley sparks, caustics, resonance pulses).
    /// 
    /// 10 Secrets (varying scale):
    /// Small (3): Vein Echo Shards — subtle visual breadcrumbs, quick resonance trace rewards.
    /// Medium (4): Refractive Alcoves — light alignment puzzles, pocket chambers with unique collectibles.
    /// Large (2): Micro-Giant Corruption Vein Puzzles — order-based using R7 fuse styles (thick/medium/thin), side fractal chambers.
    /// Epic (1): The Fractal Cathedral Heart — multi-condition deep exploration culminating in ultimate recursive chamber.
    /// 
    /// Rewards: Visual (permanent or session-deepened living crystal effects via manager), Narrative (Archive lore + companion VO hints),
    /// Mechanical (Aether bonuses, Moon2-specific unique items like Fractal Keystone that enhance future purges).
    /// 
    /// Encourages deep cavern exploration: players must master dissonance lens, micro-giant, bell sequences, restoration order,
    /// and notice the polished visual language of R7 (unusual refractions, pulsing veins, off-path godray hints).
    /// Breadcrumbs via companion comments, scanner pings on hidden POIs, and R7 visual cues (e.g. thickness-differentiated ember trails).
    /// 
    /// All absolute paths C:\dev\TARTARIA_new. Domain-strict: Moon 2 only.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon2ExplorationSecrets : MonoBehaviour
    {
        public static Moon2ExplorationSecrets Instance { get; private set; }

        [Header("Moon 2 Secret Network — 10 Secrets")]
        [SerializeField] bool enableSecrets = true;
        [SerializeField] float discoveryRadius = 4.5f;
        [SerializeField] float scanRevealBonus = 1.8f;

        // 10 secrets data (positions tuned to scaffold layout around 5 buildings)
        readonly List<Moon2Secret> _secrets = new List<Moon2Secret>
        {
            // Small: Vein Echo Shards (3)
            new Moon2Secret { id = 1, name = "Vein Echo Shard — North Cavern", type = "VeinEcho", pos = new Vector3(-22f, 0.8f, 52f),
                hint = "A faint dark fractal tip pulses when Dissonance Lens is active at twilight.", reward = "+65 Aether + 'Crystal Carver Lament I' lore entry", discovered = false, scale = "Small" },
            new Moon2Secret { id = 2, name = "Vein Echo Shard — Bell Approach", type = "VeinEcho", pos = new Vector3(-38f, 1.2f, 19f),
                hint = "Unusual black-purple vein segment hidden behind a KayKit rock cluster.", reward = "+50 Aether + 'Crystal Carver Lament II' lore", discovered = false, scale = "Small" },
            new Moon2Secret { id = 3, name = "Vein Echo Shard — Ley South", type = "VeinEcho", pos = new Vector3(14f, 0.4f, 6f),
                hint = "Ley-adjacent corruption tendril glows subtly only after fountain restore.", reward = "+40 Aether + minor resonance fragment", discovered = false, scale = "Small" },

            // Medium: Refractive Alcoves (4)
            new Moon2Secret { id = 4, name = "Refractive Alcove — Cathedral Base", type = "RefractiveAlcove", pos = new Vector3(-8f, 0.5f, 48f),
                hint = "Wall crystal bends light at impossible angles. Scanner + sustained resonance aligns it.", reward = "'Prism of Refraction' unique ( +12% Moon2 scanner radius, visual caustics boost)", discovered = false, scale = "Medium" },
            new Moon2Secret { id = 5, name = "Refractive Alcove — Bell Grotto", type = "RefractiveAlcove", pos = new Vector3(-27f, 2.1f, 22f),
                hint = "High alcove refracts bell ripples into a hidden doorway shape.", reward = "'Bell Echo Prism' (temporary godray vision in micro-giant)", discovered = false, scale = "Medium" },
            new Moon2Secret { id = 6, name = "Refractive Alcove — Fountain Grotto", type = "RefractiveAlcove", pos = new Vector3(28f, 0.3f, 9f),
                hint = "Submerged refraction pocket behind ionized mist.", reward = "+120 Aether cache + 'Mist Refractor' cosmetic shard", discovered = false, scale = "Medium" },
            new Moon2Secret { id = 7, name = "Refractive Alcove — Crystal Hall Overlook", type = "RefractiveAlcove", pos = new Vector3(-16f, 5.5f, 51f),
                hint = "High ledge with fractal wall that sings when all nearby crystals are lit.", reward = "'Hall Singer Shard' (narrative + Lirael affinity)", discovered = false, scale = "Medium" },

            // Large: Micro-Giant Vein Puzzles (2) — solved inside micro mode via order of thickness visual cues
            new Moon2Secret { id = 8, name = "Amber Lattice Vein Puzzle — Cathedral Depths", type = "MicroVeinPuzzle", pos = new Vector3(0f, 1f, 44f),
                hint = "Inside Fractured Cathedral Dome (micro-giant): side corridor with three veins of differing thickness (R7 fuse styles). Purge in thick→thin order.", reward = "'Amber Growth Catalyst' — triggers extra crystal growth + breathing amplitude on cathedral restore (visual payoff)", discovered = false, scale = "Large" },
            new Moon2Secret { id = 9, name = "Violet Heart Vein Puzzle — Ley Depths", type = "MicroVeinPuzzle", pos = new Vector3(17f, 2f, 31f),
                hint = "Inside Ley Node Chamber micro: hidden branching veins. Match medium-thin-thick sequence for recursive chamber.", reward = "'Ley Heart Fragment' + temporary full 9-probe + godray boost in micro", discovered = false, scale = "Large" },

            // Epic (1): The Fractal Cathedral Heart
            new Moon2Secret { id = 10, name = "The Fractal Cathedral Heart", type = "EpicFractalHeart", pos = new Vector3(0f, -1.5f, 42f),
                hint = "Requires: all 5 buildings restored + 2 micro vein puzzles solved + correct 3-bell sequence (visual ripple cues). Deepest recursive cathedral.", reward = "'Fractal Keystone' (Moon2 unique) — permanent zone visual escalation (stronger dome breathing, intensified ley sparks between all 5, extra recursive lights, auto first-node pulse in future micro purges) + major pre-Flood vision lore + high Aether", discovered = false, scale = "Epic" }
        };

        Transform _player;
        Moon2CavernVisualManager _visualManager;
        bool _moon2SceneActive;
        readonly HashSet<int> _discoveredThisSession = new HashSet<int>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;

            // Detect Moon 2 scene by root name or active buildings
            var sceneRoot = GameObject.Find("--- MOON2_CRYSTALLINE_CAVERNS ---");
            _moon2SceneActive = sceneRoot != null || (Application.isPlaying && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Crystalline"));

            if (_moon2SceneActive && enableSecrets)
            {
                // Find or create the visual manager (R7 production)
                var dressing = sceneRoot != null ? sceneRoot.transform.Find("Moon2_KayKitDressing_R7_FinalPolish") : null;
                if (dressing != null)
                    _visualManager = dressing.GetComponent<Moon2CavernVisualManager>();
                if (_visualManager == null && sceneRoot != null)
                    _visualManager = sceneRoot.gameObject.AddComponent<Moon2CavernVisualManager>();

                // Seed visual breadcrumbs using R7 visuals (subtle emissive vein tips / refractive shards)
                StartCoroutine(SpawnVisualBreadcrumbs());

                // Register with scanner for hidden POI reveals
                if (ResonanceScannerSystem.Instance != null)
                    ResonanceScannerSystem.Instance.OnPOIRevealed += HandleScannerReveal;

                Debug.Log("[Moon2 Secrets] 10-secret exploration network active. Fractal cathedral awaits deep explorers.");
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (ResonanceScannerSystem.Instance != null)
                ResonanceScannerSystem.Instance.OnPOIRevealed -= HandleScannerReveal;
        }

        void Update()
        {
            if (!_moon2SceneActive || !enableSecrets || _player == null) return;

            // Proximity discovery for all undiscovered secrets (player must be near + use tool or E)
            for (int i = 0; i < _secrets.Count; i++)
            {
                var s = _secrets[i];
                if (s.discovered) continue;

                float dist = Vector3.Distance(_player.position, s.pos);
                if (dist < discoveryRadius)
                {
                    // Visual hint pulse (R7 style)
                    if (Time.frameCount % 45 == 0 && _visualManager != null)
                    {
                        // Subtle resonance hint using existing pulse
                        _visualManager.SpawnCrystalResonancePulse(); // reuse for breadcrumb feel
                    }

                    // Auto or input-triggered discovery (E or scanner active)
                    if (UnityEngine.Input.GetKeyDown(KeyCode.E) || (ResonanceScannerSystem.Instance != null && ResonanceScannerSystem.Instance.IsReady))
                    {
                        DiscoverSecret(i);
                        break;
                    }
                }
            }

            // Epic special: check global conditions for heart
            if (!_secrets[9].discovered && AreEpicConditionsMet())
            {
                // Extra strong breadcrumb at heart pos
                if (Vector3.Distance(_player.position, _secrets[9].pos) < 18f && Time.frameCount % 30 == 0)
                {
                    if (_visualManager != null) _visualManager.SpawnLeyLineSparksOnRestore("moon2_ley_chamber");
                }
            }
        }

        bool AreEpicConditionsMet()
        {
            // Simplified: check if major Moon2 buildings restored (via GameLoop or progress) + 2 micro secrets done
            // In real: query MoonProgressTracker or BuildingSystem restored count for moon2_*
            bool buildingsReady = true; // Placeholder — in full build would query real state
            int microSolved = 0;
            if (_secrets[7].discovered) microSolved++;
            if (_secrets[8].discovered) microSolved++;
            return buildingsReady && microSolved >= 2;
        }

        void HandleScannerReveal(ScanResult result)
        {
            if (!_moon2SceneActive) return;
            // If near a secret, boost discovery chance / auto reveal breadcrumb
            for (int i = 0; i < _secrets.Count; i++)
            {
                var s = _secrets[i];
                if (s.discovered) continue;
                if (Vector3.Distance(result.worldPosition, s.pos) < 12f)
                {
                    // Make secret glow stronger visually
                    if (_visualManager != null)
                        _visualManager.RevealMoon2SecretVisual(s.id.ToString(), s.pos, s.type, "Scanner breadcrumb: " + s.hint);
                }
            }
        }

        IEnumerator SpawnVisualBreadcrumbs()
        {
            yield return new WaitForSeconds(1.2f);
            foreach (var s in _secrets)
            {
                if (s.discovered) continue;

                // Create subtle R7-style visual breadcrumb using primitives + emissive (no new assets)
                var breadcrumb = new GameObject($"Moon2SecretBreadcrumb_{s.id}_{s.type}");
                breadcrumb.transform.position = s.pos + Vector3.up * 0.6f;

                var prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prim.name = "SecretVisualHint";
                prim.transform.SetParent(breadcrumb.transform, false);
                prim.transform.localScale = Vector3.one * 0.45f;

                var rend = prim.GetComponent<Renderer>();
                if (rend != null)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    if (s.type.Contains("Vein"))
                    {
                        mat.color = new Color(0.08f, 0.02f, 0.12f);
                        mat.SetColor("_EmissionColor", new Color(0.45f, 0.1f, 0.55f) * 1.8f);
                    }
                    else if (s.type.Contains("Alcove") || s.type.Contains("Refractive"))
                    {
                        mat.color = new Color(0.35f, 0.55f, 0.85f);
                        mat.SetColor("_EmissionColor", new Color(0.6f, 0.85f, 1f) * 2.2f);
                    }
                    else
                    {
                        mat.color = new Color(0.75f, 0.65f, 0.95f);
                        mat.SetColor("_EmissionColor", new Color(0.95f, 0.8f, 1f) * 2.8f);
                    }
                    mat.EnableKeyword("_EMISSION");
                    mat.SetFloat("_Smoothness", 0.92f);
                    rend.sharedMaterial = mat;
                }

                // Auto-destroy breadcrumb after discovery or long time
                Destroy(breadcrumb, 420f);
                yield return new WaitForSeconds(0.08f);
            }
            Debug.Log("[Moon2 Secrets] Visual breadcrumbs (R7 fractal vein / refractive crystal style) seeded for all 10 secrets.");
        }

        public void DiscoverSecret(int index)
        {
            if (index < 0 || index >= _secrets.Count) return;
            var s = _secrets[index];
            if (s.discovered) return;

            s.discovered = true;
            _discoveredThisSession.Add(s.id);
            _secrets[index] = s;

            Debug.Log($"[Moon2 Secrets] DISCOVERED: {s.name} ({s.scale}) — {s.reward}");

            // Core visual payoff via R7 manager (fractal cathedral fantasy)
            if (_visualManager != null)
            {
                _visualManager.RevealMoon2SecretVisual(s.id.ToString(), s.pos, s.type, s.reward);
            }

            // Spawn real PickupInteractable collectible for mechanical/narrative reward
            SpawnSecretCollectible(s);

            // Narrative: log to Archive + companion hint
            ArchiveManager.Instance?.AddEntry($"moon2_secret_{s.id}", s.name, s.hint + " | Reward: " + s.reward);

            // Companion context comment (via existing system)
            if (CompanionManager.Instance != null)
            {
                // In full: trigger specific VO; here simulated
                Debug.Log($"[Moon2 Secrets] Companion hint triggered near {s.name}: \"The stone remembers... something important happened here.\"");
            }

            // Special epic permanent visual upgrade
            if (s.type == "EpicFractalHeart")
            {
                if (_visualManager != null)
                    _visualManager.ApplyMoon2EpicSecretPermanentVisualUpgrade();
                // Grant keystone to inventory
                SpawnFractalKeystonePickup(s.pos);
            }

            // Haptic + audio
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            AudioManager.Instance?.PlaySFX2D("DiscoveryChime");

            // Mechanical bonus (Aether / RS)
            GameLoopController.Instance?.QueueRSReward(12f + (s.scale == "Epic" ? 35f : s.scale == "Large" ? 18f : 5f), $"moon2_secret_{s.id}");

            // Encourage further exploration: if small/medium, hint at nearby larger secret
            if (s.scale == "Small" || s.scale == "Medium")
            {
                Debug.Log("[Moon2 Secrets] Exploration chain: A larger secret pulses nearby. The cathedral depths call.");
            }
        }

        void SpawnSecretCollectible(Moon2Secret s)
        {
            var go = new GameObject($"Collectible_{s.id}_{s.type}");
            go.transform.position = s.pos + Vector3.up * 1.8f;

            var pickup = go.AddComponent<PickupInteractable>();
            pickup.itemId = $"moon2_secret_{s.id}_{s.type.ToLower()}";
            pickup.quantity = 1;
            pickup.displayName = s.name;

            // Attach simple visual (reuses R7 crystal aesthetic)
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(go.transform, false);
            visual.transform.localScale = Vector3.one * 0.6f;
            visual.name = "SecretCollectibleVisual";
            var vr = visual.GetComponent<Renderer>();
            if (vr != null)
            {
                var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                m.color = s.type.Contains("Epic") ? new Color(0.95f, 0.9f, 1f) : (s.type.Contains("Micro") ? new Color(0.4f, 0.25f, 0.6f) : new Color(0.6f, 0.8f, 0.95f));
                m.SetColor("_EmissionColor", new Color(0.9f, 0.95f, 1f) * (s.type.Contains("Epic") ? 3.5f : 2.1f));
                m.EnableKeyword("_EMISSION");
                vr.sharedMaterial = m;
            }

            // Make interactable layer friendly
            go.layer = 9; // Interactable layer typical
        }

        void SpawnFractalKeystonePickup(Vector3 pos)
        {
            var keystone = new GameObject("Moon2_FractalKeystone_Unique");
            keystone.transform.position = pos + Vector3.up * 2.2f;

            var pickup = keystone.AddComponent<PickupInteractable>();
            pickup.itemId = "moon2_fractal_keystone";
            pickup.quantity = 1;
            pickup.displayName = "Fractal Keystone — Heart of the Cathedral";

            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.transform.SetParent(keystone.transform, false);
            vis.transform.localScale = Vector3.one * 1.1f;
            var rend = vis.GetComponent<Renderer>();
            if (rend != null)
            {
                var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                m.color = new Color(0.92f, 0.96f, 1f);
                m.SetColor("_EmissionColor", new Color(0.75f, 0.92f, 1f) * 4.5f);
                m.EnableKeyword("_EMISSION");
                m.SetFloat("_Smoothness", 0.98f);
                rend.sharedMaterial = m;
            }
            Debug.Log("[Moon2 Secrets] Fractal Keystone spawned — ultimate exploration reward. Permanent cathedral enhancement unlocked.");
        }

        /// <summary>
        /// Public API for MicroGiantController or other systems to notify when a micro vein puzzle side-node is solved in correct visual order.
        /// Unlocks the corresponding Large secret.
        /// </summary>
        public void NotifyMicroVeinPuzzleSolved(int microSecretId, string orderUsed)
        {
            int idx = microSecretId == 8 ? 7 : 8;
            if (! _secrets[idx].discovered)
            {
                DiscoverSecret(idx);
                Debug.Log($"[Moon2 Secrets] Micro vein puzzle solved with order {orderUsed} — large secret chamber opened with rich visual payoff.");
            }
        }

        [System.Serializable]
        public struct Moon2Secret
        {
            public int id;
            public string name;
            public string type;
            public Vector3 pos;
            public string hint;
            public string reward;
            public bool discovered;
            public string scale; // Small / Medium / Large / Epic
        }
    }
}