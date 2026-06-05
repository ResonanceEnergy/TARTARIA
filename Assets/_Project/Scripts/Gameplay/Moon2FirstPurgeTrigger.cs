using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Audio;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Moon 2 First Dissonance Vein Purge FTUE — the emotional anchor for the Moon 2 vertical slice.
    /// Lunar Moon: Shadow & Purge — immediate playable first 5 minutes.
    /// Polished HOLD TO PURGE + scan phase + F310 button callouts + reduced-motion safe visualizers (bar + conditional emissive).
    /// Success banners now explicitly call out Lirael trust gain. 5-beat objective flow for the first vein (guided 5-10min emotional clarity).
    /// Wires directly to Moon2ProgressionSystem (RegisterFirstPurge) + Moon2LunarContentSpawner (5-beat) + HUD polished context/hold.
    /// Persists across reloads. Makes the first purge unforgettable catharsis.
    /// </summary>
    public class Moon2FirstPurgeTrigger : MonoBehaviour
    {
        [Header("First Purge Vein")]
        [SerializeField] string veinId = "moon2_first_dissonance_vein";
        [SerializeField] float requiredPurgeProgress = 1.0f;
        [SerializeField] float purgeHoldTime = 2.2f;

        [Header("Permanent World Change")]
        [SerializeField] public GameObject purifiedCrystalMarker;
        [SerializeField] public GameObject dissonanceVeinVisual;
        [SerializeField] public LineRenderer leyThread;

        [Header("VFX & Audio")]
        [SerializeField] ParticleSystem purgeBurst;
        [SerializeField] ParticleSystem resonanceSpreadParticles;

        bool _purged;
        float _currentProgress;
        bool _playerInRange;
        bool _scanned; // scan phase before hold-to-purge (scan/tune/purge FTUE)
        AudioSource _veinHumSource; // for reactive audio during hold

        void Awake()
        {
            if (purifiedCrystalMarker) purifiedCrystalMarker.SetActive(false);
            if (leyThread) leyThread.enabled = false;

            // Check persisted purged state from Moon2 save block (via ProgressionSystem) for reloads
            if (ServiceLocator.Moon2Progression != null && ServiceLocator.Moon2Progression.IsSitePurged(veinId))
            {
                _purged = true;
                ApplyPurgedVisualsOnly();
                // Ensure permanent purified hum plays at low volume on return
                AudioManager.Instance?.PlaySFX("Moon2_PurifiedCrystalHum", transform.position, 0.28f);
                Debug.Log("[Moon2FirstPurgeTrigger] Restored purged state from Moon2SaveBlock (permanent purgedSites).");
            }
            else
            {
                // Start the initial dissonance vein hum (rich procedural from library) — only if not yet purged
                AudioManager.Instance?.PlaySFX("Moon2_FirstVeinDissonanceHum", transform.position, 0.38f);
            }
        }

        void ApplyPurgedVisualsOnly()
        {
            if (purifiedCrystalMarker) purifiedCrystalMarker.SetActive(true);
            if (dissonanceVeinVisual) dissonanceVeinVisual.SetActive(false);
            if (leyThread) leyThread.enabled = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) 
            {
                _playerInRange = true;
                if (!_purged)
                {
                    if (!_scanned)
                    {
                        ServiceLocator.HUD?.ShowContextPrompt("PRESS " + InputPromptHelper.PurgeHoldShort + " TO SCAN DISSONANCE VEIN");
                    }
                    else
                    {
                        ServiceLocator.HUD?.ShowContextPrompt(InputPromptHelper.GetMoon2PurgePrompt() + " VEIN");
                    }
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) _playerInRange = false;
            ServiceLocator.HUD?.HideContextPrompt();
            ServiceLocator.HUD?.HidePurgeHoldPrompt();
        }

        void Update()
        {
            if (!_playerInRange || _purged) return;

            bool actionPressed = false;
            bool holding = false;

            // Unity 6 Input System Package: legacy UnityEngine.Input.* throws InvalidOperationException
            // when the asset is configured for "Input System Package" mode (CLAUDE.md F310 section).
            // Gamepad south face button (A on F310 X-mode) replaces legacy "Fire1".
            var kb = Keyboard.current;
            var gamepad = Gamepad.current;
            actionPressed = (kb != null && kb.eKey.wasPressedThisFrame) || (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
            holding = (kb != null && kb.eKey.isPressed) || (gamepad != null && gamepad.buttonSouth.isPressed);

            // Handle scan phase transition (first press when not scanned)
            if (actionPressed && !_scanned)
            {
                _scanned = true;
                ServiceLocator.HUD?.ShowContextPrompt(InputPromptHelper.GetMoon2PurgePrompt() + " VEIN");
                ServiceLocator.HUD?.ShowObjective("1/5 — DISSONANCE VEIN SCANNED • NOW HOLD TO PURGE");
                AudioManager.Instance?.PlaySFX("Moon2_VeinScanTone", transform.position, 0.6f);
                HapticFeedbackManager.Instance?.TriggerF310Rumble(0.3f, 0.12f, 0.4f);
                return;
            }

            if (holding && _scanned)
            {
                _currentProgress += Time.deltaTime / purgeHoldTime;
                _currentProgress = Mathf.Clamp01(_currentProgress);

                // Live polished HUD hold visualizer (F310 callout + progress bar, reduced-motion safe)
                ServiceLocator.HUD?.ShowPurgeHoldPrompt(InputPromptHelper.GetMoon2PurgePrompt(), _currentProgress);

                // Reduced-motion safe emissive on vein (only pulse if motion allowed)
                bool reduced = UnityEngine.PlayerPrefs.GetInt("TARTARIA_ReducedMotion", 0) == 1;
                if (dissonanceVeinVisual != null)
                {
                    var rend = dissonanceVeinVisual.GetComponent<Renderer>();
                    if (rend != null && rend.material != null && rend.material.HasProperty("_EmissionColor"))
                    {
                        if (!reduced)
                        {
                            float pulse = 1.5f + Mathf.Sin(Time.time * 6f) * 0.8f * _currentProgress;
                            rend.material.SetColor("_EmissionColor", new Color(0.55f, 0.25f, 0.85f) * pulse);
                        }
                        else
                        {
                            // Static safe glow for reduced motion
                            rend.material.SetColor("_EmissionColor", new Color(0.55f, 0.25f, 0.85f) * (1.3f + _currentProgress * 0.6f));
                        }
                    }
                }

                if (_currentProgress > 0.1f)
                {
                    HapticFeedbackManager.Instance?.TriggerF310Rumble(0.4f + _currentProgress * 0.4f, 0.08f, 0.6f);
                }

                // Reactive audio: the dissonance hum rises in pitch and cleans up as purge progresses (beautiful tension release)
                if (_veinHumSource != null)
                {
                    float targetPitch = Mathf.Lerp(0.85f, 1.35f, _currentProgress); // from sick low to brighter
                    _veinHumSource.pitch = Mathf.Lerp(_veinHumSource.pitch, targetPitch, Time.deltaTime * 3f);
                    _veinHumSource.volume = Mathf.Lerp(0.38f, 0.22f, _currentProgress); // fades as it purifies
                }

                if (_currentProgress >= requiredPurgeProgress)
                {
                    CompleteFirstPurge();
                }
            }
            else if (_currentProgress > 0f)
            {
                _currentProgress = Mathf.Max(0f, _currentProgress - Time.deltaTime * 1.8f);
                ServiceLocator.HUD?.ShowPurgeHoldPrompt(InputPromptHelper.GetMoon2PurgePrompt(), _currentProgress); // show decaying progress

                // soften emissive back — reduced motion safe
                bool reduced = UnityEngine.PlayerPrefs.GetInt("TARTARIA_ReducedMotion", 0) == 1;
                if (dissonanceVeinVisual != null)
                {
                    var rend = dissonanceVeinVisual.GetComponent<Renderer>();
                    if (rend != null && rend.material != null && rend.material.HasProperty("_EmissionColor"))
                    {
                        float safe = reduced ? 1.4f : 1.8f;
                        rend.material.SetColor("_EmissionColor", new Color(0.4f, 0.2f, 0.7f) * safe);
                    }
                }
            }
            else
            {
                ServiceLocator.HUD?.HidePurgeHoldPrompt();
            }
        }

        void CompleteFirstPurge()
        {
            if (_purged) return;
            _purged = true;

            // Permanent world change — crystal lights up, ley thread appears
            if (purifiedCrystalMarker) purifiedCrystalMarker.SetActive(true);
            if (dissonanceVeinVisual) dissonanceVeinVisual.SetActive(false);
            if (leyThread)
            {
                leyThread.enabled = true;
                // simple static line for now; can be animated later
            }

            // Clean up the reactive dissonance hum source (it has been "purified")
            if (_veinHumSource != null)
            {
                _veinHumSource.Stop();
                Destroy(_veinHumSource, 0.5f);
            }

            // VFX (skip heavy if reduced motion for accessibility) — already safe
            bool reduced = UnityEngine.PlayerPrefs.GetInt("TARTARIA_ReducedMotion", 0) == 1;
            if (!reduced)
            {
                if (purgeBurst) purgeBurst.Play();
                if (resonanceSpreadParticles) resonanceSpreadParticles.Play();

                // Strong visual payoff for the first purge (aurora-style crystal bloom + ley connection, reduced-motion safe)
                ServiceLocator.VFX?.SpawnAuroraFountain(transform.position + Vector3.up * 1.4f, 3.2f);
                ServiceLocator.VFX?.TriggerOvertoneThread(transform.position, transform.position + new Vector3(0, 4.8f, 11f), 1.8f);
            }

            // Rich procedural audio for the cathartic first purge — emotional anchor of vertical slice
            AudioManager.Instance?.PlaySFX("Moon2_FirstPurgeStinger", transform.position, 0.85f);
            AudioManager.Instance?.PlaySFX("Moon2_PurgeSuccessF310Tone", transform.position, 0.9f);
            AudioManager.Instance?.PlaySFX("Moon2_LiraelFirstPurgeReaction", transform.position + Vector3.up * 2f, 0.7f);
            AudioManager.Instance?.PlaySFX("Moon2_PurifiedCrystalHum", purifiedCrystalMarker ? purifiedCrystalMarker.transform.position : transform.position, 0.42f);

            // F310 strong success rumble
            HapticFeedbackManager.Instance?.TriggerF310Rumble(0.9f, 0.35f, 0.8f);

            // RS reward + save (dirty for persistence)
            ServiceLocator.GameLoop?.QueueRSReward(18f, "First Dissonance Vein Purge");
            ServiceLocator.Save?.MarkDirty();

            // === WIRE TO MOON2 PROGRESSION: register as permanent purgedSite in moon2 SaveData block ===
            ServiceLocator.Moon2Progression?.RegisterFirstPurge(veinId);

            // Lirael reaction (Moon 2 companion) — rich emotional payoff (via Core service interface)
            ServiceLocator.Lirael?.ReactToFirstPurge();

            // Companion trust for the anchor moment
            ServiceLocator.Companion?.AddTrust("lirael", 15f);

            // === POLISHED SUCCESS BANNER WITH LIRAEL TRUST GAIN (emotional clarity) ===
            string trustLine = "Lirael Trust +15 — The caverns remember your hands.";
            ServiceLocator.HUD?.ShowBanner(
                "★ FIRST DISSONANCE VEIN PURGED ★",
                "Lirael: 'The song returns... the shadow lifts. Thank you.' " + trustLine,
                8f
            );
            ServiceLocator.HUD?.ShowBanner(
                "★ HOLD COMPLETE — DISSONANCE PURGED ★",
                "Lirael trust gained +15. The first vein sings again. 5-beat Lunar flow begins.",
                7.5f
            );

            // 5-beat objective flow for the first vein — makes first 5-10 min guided + emotionally clear
            ServiceLocator.HUD?.ShowObjective("2/5 — DISSONANCE PURGED • LIRAEL TRUST +15");
            // Subsequent beats fired via spawner + timed for guidance (Discovery→Restoration already marked below)
            StartCoroutine(AdvanceFirstVein5BeatObjectives());

            // 5-beat advancement (Discovery 0 + Restoration 1 for Moon 2)
            ServiceLocator.MoonProgress?.MarkBeatCleared(2, 0);
            ServiceLocator.MoonProgress?.MarkBeatCleared(2, 1);

            // Resonance spread to nearby crystals (permanent visual upgrade)
            SpreadResonanceToNearbyCrystals();

            // Teaser whisper + immediate light first dissonance conflict enemy (weak wraith that flees — gives the player their first real "conflict" taste right after the purge)
            SpawnFirstWraithTeaser();
            SpawnLightFirstConflictWraith();

            ServiceLocator.HUD?.ShowAchievementToast("DISSONANCE VEIN PURGED — THE CAVERNS BREATHE AGAIN • TRUST +15");
            ServiceLocator.HUD?.HideContextPrompt();
            ServiceLocator.HUD?.HidePurgeHoldPrompt();

            // Notify Moon2 spawner for full 5-beat narrative flow + returning player logic
            ServiceLocator.Moon2Progression?.OnFirstVeinPurgedEvent();

            Debug.Log("[Moon2] First Dissonance Vein Purge complete — EMOTIONAL ANCHOR for vertical slice. Permanent purgedSite saved. Lirael relief +15 trust + 5-beat objective flow + spawner + progression wired. F310 + reduced-motion safe. Wraith teaser (full spawn deferred to Conflict).");
        }

        System.Collections.IEnumerator AdvanceFirstVein5BeatObjectives()
        {
            // Guided 5-beat flow specifically for the first vein experience (first 5-10 minutes emotional hand-holding)
            yield return new WaitForSeconds(3.5f);
            ServiceLocator.HUD?.ShowObjective("3/5 — LEY THREAD AWAKENED • CRYSTAL HUMS");

            yield return new WaitForSeconds(4.2f);
            ServiceLocator.HUD?.ShowObjective("4/5 — FIRST SHADOW WITNESSED • WRAITH FLEES THE LIGHT");

            yield return new WaitForSeconds(5.0f);
            ServiceLocator.HUD?.ShowObjective("5/5 — THE CAVERNS REMEMBER • ENTER THE LUNAR PURGE");

            yield return new WaitForSeconds(3.0f);
            ServiceLocator.HUD?.ShowBanner("5-BEAT VEIN COMPLETE", "Discovery + Restoration anchored. Lirael walks with you now. The song grows stronger.", 6f);
        }

        void SpreadResonanceToNearbyCrystals()
        {
            var nearby = Physics.OverlapSphere(transform.position, 28f);
            foreach (var col in nearby)
            {
                if (col.CompareTag("ResonanceCrystal") || col.name.ToLower().Contains("crystal"))
                {
                    var renderer = col.GetComponent<Renderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        // Boost emissive for permanent "awakened" look
                        var mat = renderer.material;
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            mat.SetColor("_EmissionColor", new Color(0.6f, 0.95f, 1f) * 2.2f);
                        }
                    }
                }
            }
        }

        void SpawnFirstWraithTeaser()
        {
            // Lightweight audio + particle hint only — Conflict hook. Real wraith spawning lives in Moon2LunarContentSpawner.TriggerConflictBeat
            var teaserPos = transform.position + new Vector3(18f, 1f, -22f);
            var wraithHint = new GameObject("WraithTeaser_Hint");
            wraithHint.transform.position = teaserPos;
            var ps = wraithHint.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.2f, 0.15f, 0.35f, 0.7f);
            main.startSize = 0.6f;
            main.startLifetime = 1.8f;
            ps.Play();
            Destroy(wraithHint, 4.5f);

            // subtle audio cue — wraith teaser whisper (full embodied spawn on Conflict beat)
            AudioManager.Instance?.PlaySFX("Moon2_DissonanceWraithWhisper", teaserPos, 0.55f);
        }

        void SpawnLightFirstConflictWraith()
        {
            // Very light, non-lethal first "conflict" enemy right after the purge — runs away when approached.
            // Gives the player an immediate taste of the Shadow & Purge theme without punishing them.
            var wraithPos = transform.position + new Vector3(14f, 1.2f, -18f);
            var wraith = new GameObject("LightFirstWraith_ConflictHook");
            wraith.transform.position = wraithPos;

            // Simple visual (dark crystal wraith proxy)
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "WraithBody";
            body.transform.SetParent(wraith.transform);
            body.transform.localScale = new Vector3(0.8f, 1.4f, 0.8f);
            var rend = body.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                rend.material.color = new Color(0.2f, 0.12f, 0.32f);
                rend.material.SetColor("_EmissionColor", new Color(0.5f, 0.25f, 0.7f) * 0.8f);
            }

            // Flee behavior (simple timer + move away from player)
            var flee = wraith.AddComponent<Moon2LightFleeingWraith>();
            flee.lifetime = 9f;

            // Audio hint
            AudioManager.Instance?.PlaySFX("Moon2_DissonanceWraithWhisper", wraithPos, 0.7f);
        }
    }
}
