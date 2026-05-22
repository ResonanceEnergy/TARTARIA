using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Integration;
using Tartaria.Audio;
using Tartaria.UI;
using Tartaria.Input;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5 Core Gameplay Driver — Overtone Moon: Amplification & Harmony.
    /// White City Echo District vertical slice.
    ///
    /// Mechanics delivered:
    /// - 5 pavilion amplification (tune/restore proxy → 6-band healing aura at 80% RS)
    /// - Floating platforms rise dramatically on success (visual + gameplay platforms)
    /// - Airship dock construction in 4 visual stages (player "empowers" via resonance)
    /// - Demolition crew defense waves during Conflict beat
    /// - Spire fragment placement climax → Intercontinental Aurora Bridge (permanent world change + grid surge)
    /// - Thorne radio captain integration (trust + voice lines on key moments)
    /// - Full 5-beat MoonBeatRunner compatibility + MoonProgressTracker + Save
    ///
    /// One editor menu populates everything. Walk the start volume → full emotional 10-15min slice.
    /// Mirrors RailEscortController intensity and completeness for Moon 3.
    /// </summary>
    public class WhiteCityAmplificationController : MonoBehaviour
    {
        public static WhiteCityAmplificationController Instance { get; private set; }

        [Header("District Layout (set by scaffold)")]
        public Vector3 districtCenter = new Vector3(28f, 1.5f, 4f);
        public Vector3[] pavilionPositions = new Vector3[5];
        public Vector3 spireBasePosition = new Vector3(28f, 0.5f, 3f);
        public Vector3 dockPosition = new Vector3(68f, 1.2f, 28f);

        [Header("State")]
        public int pavilionsAmplified;
        public int dockStage;
        public int FloatingPlatformsRaised;
        public bool spirePlaced;
        public bool bridgeFormed;
        public float currentResonancePercent = 0.55f; // starts near unlock

        // Central visualizer orbs near spire (5 small glowing indicators that light as pavilions are amplified — strong "UI visualizer" without Canvas/asmdef issues)
        private GameObject[] _pavilionStatusOrbs = new GameObject[5];

        // ─── PERF: Cached references + lazy resolution ───
        MoonBeatRunner _beatRunner;
        GameObject _permanentLeyNetwork;
        float _sceneObjectRetryTimer;

        // ─── HUD Wiring Events (subscribed by Moon5AmplificationHUD) ───
        public event System.Action<int> OnPavilionAmplified;
        public event System.Action OnDockAdvanced;
        public event System.Action OnBridgeFormed;
        public event System.Action<string> OnRadioLogEntry;

        // Fast-travel hook (bridge victory) — mirrors Moon 3 Continental Rail unlock for world map / return polish
        public static bool Moon5IntercontinentalFastTravelUnlocked { get; private set; }

        readonly bool[] _pavilionDone = new bool[5];
        readonly List<GameObject> _floatingPlatforms = new();
        List<Light> _amplifiedOrbLights = new List<Light>();
        GameObject _spireVisual;
        GameObject _dockVisual;

        bool _sequenceStarted;
        bool _climaxFired;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Moon5IntercontinentalFastTravelUnlocked = false;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void BeginAmplificationSequence()
        {
            if (_sequenceStarted) return;
            _sequenceStarted = true;

            RestoreMoon5StateFromSave(); // ensure SaveData moon5 block + permanent leys/orbs/dock visuals on re-entry or cleared play

            if (bridgeFormed)
            {
                // Cleared visit: permanent golden world state already applied (leys, bright orbs, max hum, full dock/platforms)
                HUDController.Instance?.ShowObjective("<b>WHITE CITY — THE RADIANCE ENDURES</b>\nIntercontinental Aurora Bridge active. Fast travel open. The grid sings forever.");
                Moon5WhiteCityAudioManager.Instance?.SetResonanceLevel(1.0f);
                StartCoroutine(AmplifiedOrbPulseLoop());
                return; // no discovery glow or "amplify" flow for already-complete emotional payoff state
            }

            HUDController.Instance?.ShowObjective("<b>OVERTONE MOON — WHITE CITY</b>\nAmplify the pavilions. Let the light rise.");

            // Ensure Moon 5 radiant HUD is present
            if (Tartaria.UI.Moon5AmplificationHUD.Instance == null)
            {
                var hudGO = new GameObject("Moon5_AmplificationHUD");
                hudGO.AddComponent<Tartaria.UI.Moon5AmplificationHUD>();
            }

            // Initial Thorne static burst + first line already handled by trigger
            if (_beatRunner == null) _beatRunner = Object.FindAnyObjectByType<MoonBeatRunner>();
            if (_beatRunner != null) _beatRunner.enabled = true;

            OnRadioLogEntry?.Invoke("Thorne: First contact. White City grid resonance detected. The overtone awaits.");

            // Fire HUD events for initial state
            OnRadioLogEntry?.Invoke("SYSTEM: Overtone Moon sequence initiated. Amplify the five pavilions.");

            // AUDIO: start living overtone drone (resonance-reactive hum) + Thorne radio static burst for immediate atmosphere
            Moon5WhiteCityAudioManager.Instance?.StartOvertoneDrone(currentResonancePercent);
            Moon5WhiteCityAudioManager.Instance?.PlayThorneRadioStatic(districtCenter + Vector3.up * 4f);

            // Create central 5-pavilion status orbs near spire (beautiful in-world visualizer showing overall progress)
            CreatePavilionStatusOrbs();

            // Light the first two pavilions as "already faintly glowing" (discovery feel)
            StartCoroutine(InitialDiscoveryGlow());

            // Start gentle ongoing orb pulse loop (reacts to resonance on every amplify)
            StartCoroutine(AmplifiedOrbPulseLoop());
        }

        IEnumerator InitialDiscoveryGlow()
        {
            yield return new WaitForSeconds(1.8f);
            if (!_pavilionDone[0]) FlashPavilionLight(0, 0.35f);
            yield return new WaitForSeconds(1.1f);
            if (!_pavilionDone[1]) FlashPavilionLight(1, 0.35f);
        }

        // Called by tuning success / proxy interaction / MoonMechanicActivator hook
        public void AmplifyPavilion(int index)
        {
            if (index < 0 || index >= 5 || _pavilionDone[index]) return;

            _pavilionDone[index] = true;
            pavilionsAmplified++;

            // Progress the overtone resonance (key for 6-band healing at 80%)
            currentResonancePercent = Mathf.Clamp01(0.55f + (pavilionsAmplified / 5f) * 0.45f);

            // Orbs immediately react to new overall resonance (pulse + intensity scale)
            UpdateAmplifiedOrbs();

            // AUDIO: dynamic volume of district hum reacts to new resonance (city comes alive)
            Moon5WhiteCityAudioManager.Instance?.SetResonanceLevel(currentResonancePercent);

            // Visual + VFX
            FlashPavilionLight(index, 1f);
            VFXController.Instance?.SpawnAuroraFountain(pavilionPositions[index] + Vector3.up * 2f, 4.5f);

            // Always play fountain whoosh audio for every amplify (aurora fountains feel alive)
            Moon5WhiteCityAudioManager.Instance?.PlayAuroraFountainBurst(pavilionPositions[index] + Vector3.up * 1.5f);

            // 6-band healing aura (the key new mechanic — works at 80% instead of 100%)
            if (currentResonancePercent >= 0.78f || pavilionsAmplified >= 2)
            {
                ActivateSixBandHealingAura(pavilionPositions[index]);
                Moon5WhiteCityAudioManager.Instance?.PlayHealingAuraTone(pavilionPositions[index]);
            }

            Moon5WhiteCityAudioManager.Instance?.PlayAmplificationStinger(index, 0.8f);

            // Raise nearby floating platforms
            RaiseFloatingPlatforms(Mathf.Min(2, pavilionsAmplified));

            // Dock progress every 2 pavilions
            if (pavilionsAmplified % 2 == 0 && dockStage < 4)
            {
                AdvanceDockStage();
            }

            // Thorne radio reaction
            ThorneController.Instance?.RadioPavilionAmplified(index, currentResonancePercent);

            // HUD wiring
            OnPavilionAmplified?.Invoke(index);
            OnRadioLogEntry?.Invoke($"Pavilion {index + 1} amplified — the overtone holds. Grid at {Mathf.RoundToInt(currentResonancePercent * 100)}%.");

            // Haptic on perfect amplification
            HapticFeedbackManager.Instance?.TriggerF310Rumble(0.75f, 0.35f, 0.6f);

            // Drive the living district hum (overtone breathes with the grid)
            Moon5WhiteCityAudioManager.Instance?.SetResonanceLevel(currentResonancePercent);

            // All existing amplified orbs brighten together as the overtone grows (dynamic living district visualizer)
            RefreshAllAmplifiedOrbs();

            // Light the corresponding central status orb (central progress visual)
            if (_pavilionStatusOrbs[index] != null)
            {
                var orb = _pavilionStatusOrbs[index];
                var mr = orb.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.material.color = new Color(1f, 0.92f, 0.5f);
                    mr.material.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.5f) * 1.8f);
                    mr.material.EnableKeyword("_EMISSION");
                }
                var light = orb.GetComponent<Light>();
                if (light) light.intensity = 2.2f + currentResonancePercent * 1.5f;
            }

            // Make all lit central status orbs also breathe with overall resonance (consistent with amplified orbs)
            UpdateStatusOrbsWithResonance();

            HUDController.Instance?.ShowObjective($"Pavilion {index + 1} amplified — {pavilionsAmplified}/5  |  Light at {Mathf.RoundToInt(currentResonancePercent * 100)}%");

            // Extra objective banners for emotional flow (if thin spots)
            if (pavilionsAmplified == 3)
            {
                HUDController.Instance?.ShowObjective("<b>THREE PAVILIONS SING</b>\nThe 6-band healing aura spreads. The overtone strengthens across the district.");
                OnRadioLogEntry?.Invoke("Thorne: Three nodes live — the city begins to remember its own voice.");
            }
            else if (pavilionsAmplified == 1)
            {
                OnRadioLogEntry?.Invoke("Thorne: First pavilion holds. Resonance climbing — keep the frequency pure.");
            }

            CheckForClimaxReady();
            PersistMoon5State();
        }

        void FlashPavilionLight(int index, float intensity)
        {
            // Proxy: brighten the cylinder permanently for amplified state (radiant gold)
            string name = $"Moon5_Pavilion_{index + 1:00}";
            GameObject p = null;
            // Use cached ref if exists, otherwise find
            if (index >= 0 && index < 5 && _pavilionStatusOrbs[index] != null)
                p = _pavilionStatusOrbs[index].transform.parent?.gameObject;
            if (p == null) p = GameObject.Find(name);
            if (p != null)
            {
                var mr = p.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.material.color = new Color(1f, 0.95f, 0.55f); // permanent empowered gold
                    mr.material.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.4f) * 1.5f);
                    mr.material.EnableKeyword("_EMISSION");
                }

                // Add a permanent glowing orb child as visual "amplified" indicator (UI visualizer proxy)
                if (p.transform.Find("AmplifiedOrb") == null)
                {
                    var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    orb.name = "AmplifiedOrb";
                    orb.transform.SetParent(p.transform);
                    orb.transform.localPosition = Vector3.up * 4.5f;
                    orb.transform.localScale = Vector3.one * 1.2f;
                    var omr = orb.GetComponent<MeshRenderer>();
                    if (omr) omr.material.color = new Color(1f, 0.92f, 0.5f);
                    var light = orb.AddComponent<Light>();
                    light.color = new Color(1f, 0.9f, 0.5f);
                    light.intensity = 1.8f + currentResonancePercent * 2.2f; // grows stronger as the grid empowers
                    light.range = 10f + currentResonancePercent * 6f;
                    Destroy(orb.GetComponent<Collider>());
                    _amplifiedOrbLights.Add(light); // track for dynamic resonance-based pulsing
                }
            }

            // Real VFX thread / overtone pulse
            VFXController.Instance?.TriggerOvertoneThread(pavilionPositions[index], districtCenter, intensity);
        }

        /// <summary>
        /// Live update for all existing AmplifiedOrbs so the entire district visibly brightens as resonance climbs.
        /// This is the key "living world" visual payoff for the Overtone Moon.
        /// </summary>
        void RefreshAllAmplifiedOrbs()
        {
            for (int i = 0; i < 5; i++)
            {
                if (!_pavilionDone[i]) continue;

                string name = $"Moon5_Pavilion_{i + 1:00}";
                GameObject p = null;
                if (i >= 0 && i < 5 && _pavilionStatusOrbs[i] != null)
                    p = _pavilionStatusOrbs[i].transform.parent?.gameObject;
                if (p == null) p = GameObject.Find(name);
                if (p == null) continue;

                var orb = p.transform.Find("AmplifiedOrb");
                if (orb == null) continue;

                var light = orb.GetComponent<Light>();
                if (light != null)
                {
                    light.intensity = 1.8f + currentResonancePercent * 2.2f;
                    light.range = 10f + currentResonancePercent * 6f;
                }

                // Subtle scale pulse for "singing" feel (cheap)
                float scale = 1.1f + (currentResonancePercent * 0.25f);
                orb.localScale = Vector3.one * scale;
            }
        }

        /// <summary>
        /// Final dramatic surge when the Intercontinental Bridge forms — all orbs and status orbs brighten + pulse for the big emotional payoff.
        /// </summary>
        void SurgeStatusOrbsAndAmplifiedOrbs()
        {
            // Status orbs near spire
            for (int i = 0; i < 5; i++)
            {
                if (_pavilionStatusOrbs[i] == null) continue;
                var orb = _pavilionStatusOrbs[i];
                var light = orb.GetComponent<Light>();
                if (light) light.intensity = 4.5f + currentResonancePercent * 2f;

                var mr = orb.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.material.color = new Color(1f, 0.95f, 0.6f);
                    mr.material.SetColor("_EmissionColor", new Color(1f, 0.92f, 0.6f) * 3f);
                }

                // Reduced-motion safe surge: bright static for accessibility, no pop/pulse (PulseScale already guards but initial scale must too)
                bool reduced = Tartaria.UI.SettingsOverlay.IsReducedMotion;
                if (!reduced)
                {
                    orb.transform.localScale = Vector3.one * 0.7f;
                    StartCoroutine(PulseScale(orb.transform, 0.7f, 1.1f, 1.8f));
                }
                else
                {
                    orb.transform.localScale = Vector3.one * 1.05f; // keep prominent bright size, no motion
                }
            }

            // Also surge any AmplifiedOrbs one last time
            RefreshAllAmplifiedOrbs();
        }

        System.Collections.IEnumerator PulseScale(Transform t, float from, float to, float duration)
        {
            if (Tartaria.UI.SettingsOverlay.IsReducedMotion) yield break;
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float s = Mathf.Lerp(from, to, Mathf.Sin((time / duration) * Mathf.PI));
                t.localScale = Vector3.one * s;
                yield return null;
            }
            t.localScale = Vector3.one * to;
        }

        /// <summary>
        /// Makes the central status orbs also dynamically respond to overall resonance (breathing intensity like the per-pavilion orbs).
        /// Called on every amplify for consistent "living grid" feel.
        /// </summary>
        void UpdateStatusOrbsWithResonance()
        {
            for (int i = 0; i < 5; i++)
            {
                var orb = _pavilionStatusOrbs[i];
                if (orb == null) continue;

                // Only affect orbs that have already been lit (their pavilion is done)
                var light = orb.GetComponent<Light>();
                if (light != null && light.intensity > 0.5f) // already lit
                {
                    // Base from when it was lit + extra from current overall resonance
                    float baseIntensity = 2.2f + currentResonancePercent * 1.5f;
                    light.intensity = baseIntensity * (0.92f + Mathf.Sin(Time.time * 1.6f) * 0.08f); // gentle breath
                }
            }
        }

        /// <summary>
        /// Creates 5 small glowing status orbs in a nice arc near the spire.
        /// They light up gold one-by-one as pavilions are amplified — excellent central visualizer for the "Radiance of Empowerment".
        /// </summary>
        void CreatePavilionStatusOrbs()
        {
            if (_pavilionStatusOrbs[0] != null) return;

            Vector3 center = spireBasePosition + Vector3.up * 3f;
            float radius = 4.5f;

            for (int i = 0; i < 5; i++)
            {
                float angle = (i - 2) * 0.35f; // nice spread
                Vector3 pos = center + new Vector3(Mathf.Sin(angle) * radius, (i % 2 == 0 ? 0.5f : -0.3f), Mathf.Cos(angle) * radius * 0.6f);

                var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.name = $"PavilionStatusOrb_{i}";
                orb.transform.position = pos;
                orb.transform.localScale = Vector3.one * 0.45f;

                var mr = orb.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mr.material.color = new Color(0.4f, 0.38f, 0.32f); // dim start
                    mr.material.SetColor("_EmissionColor", Color.black);
                }

                var light = orb.AddComponent<Light>();
                light.color = new Color(1f, 0.9f, 0.5f);
                light.intensity = 0f; // starts off
                light.range = 3f;

                Destroy(orb.GetComponent<Collider>());
                _pavilionStatusOrbs[i] = orb;
            }
        }

        void ActivateSixBandHealingAura(Vector3 origin)
        {
            // The signature Moon 5 mechanic: healing light that keeps buildings "alive" during conflict
            // and visually buffs the player + nearby restored elements.
            VFXController.Instance?.SpawnSixBandHealingPulse(origin, 18f);

            // Passive RS trickle + companion trust
            GameLoopController.Instance?.QueueRSReward(6f, "moon5_6band_healing");
            // Future: actual building health regen for restored pavilions

            // Use Moon5 manager (procedural 6-band healing tones + fallback)
            Moon5WhiteCityAudioManager.Instance?.PlayHealingAuraTone(origin);
        }

        void RaiseFloatingPlatforms(int count)
        {
            // Collect proxies if not already
            if (_floatingPlatforms.Count == 0)
            {
                for (int i = 1; i <= 6; i++)
                {
                    var plat = GameObject.Find($"Moon5_FloatingPlatform_{i}");
                    if (plat != null) _floatingPlatforms.Add(plat);
                }
            }

            int raised = 0;
            foreach (var plat in _floatingPlatforms)
            {
                if (raised >= count) break;
                if (plat.transform.position.y > 18f) continue; // already high

                StartCoroutine(RisePlatform(plat));
                FloatingPlatformsRaised = Mathf.Min(6, FloatingPlatformsRaised + 1);
                raised++;
            }
        }

        IEnumerator RisePlatform(GameObject plat)
        {
            float targetY = plat.transform.position.y + 14f + Random.Range(-1f, 3f);
            Vector3 start = plat.transform.position;
            float t = 0f;
            float dur = 2.8f;

            while (t < dur)
            {
                t += Time.deltaTime;
                float y = Mathf.Lerp(start.y, targetY, Mathf.SmoothStep(0, 1, t / dur));
                plat.transform.position = new Vector3(start.x, y, start.z);
                // Subtle rotation for "levitating sacred geometry"
                plat.transform.Rotate(Vector3.up, 18f * Time.deltaTime, Space.World);
                yield return null;
            }

            VFXController.Instance?.SpawnPlatformStabilizeVFX(plat.transform.position);
            HapticFeedbackManager.Instance?.TriggerF310Rumble(0.4f, 0.22f, 0.3f);
        }

        void AdvanceDockStage()
        {
            dockStage++;
            string dockName = "Moon5_AirshipDock";
            var dock = GameObject.Find(dockName);
            if (dock != null)
            {
                // Grow the dock + add visual modules
                dock.transform.localScale = new Vector3(22f + dockStage * 1.8f, 1.2f + dockStage * 0.3f, 38f);
            }

            // Spawn construction VFX / golden welding sparks
            VFXController.Instance?.SpawnDockConstruction(dockPosition + Vector3.up * 3f, dockStage);

            ThorneController.Instance?.RadioDockProgress(dockStage);

            OnDockAdvanced?.Invoke();
            OnRadioLogEntry?.Invoke($"Airship dock advancing to stage {dockStage}. Thorne's signal strengthens — the fleet approaches.");

            if (dockStage >= 3)
            {
                HUDController.Instance?.ShowObjective("AIRSHIP DOCK — 80%  |  Thorne's signal is strengthening.");
            }

            PersistMoon5State();
        }

        void CheckForClimaxReady()
        {
            if (pavilionsAmplified >= 5 && !spirePlaced && !_climaxFired)
            {
                _climaxFired = true;
                HUDController.Instance?.ShowObjective("<b>CLIMAX — THE SPIRE FRAGMENT</b>\nPlace the Moon 1 spire piece at the central anchor. The world will sing.");
                OnRadioLogEntry?.Invoke("Thorne: All five! The grid is one voice now. Lock the fragment — birth the bridge!");
            }
        }

        // Called by player interaction with spire base (or auto in climax beat)
        public void PlaceSpireFragment()
        {
            if (spirePlaced) return;
            spirePlaced = true;

            // Spectacular ignition sequence (matches vivid visuals doc exactly)
            if (_spireVisual == null)
            {
                _spireVisual = GameObject.Find("Moon5_SpiresAnchor");
            }

            StartCoroutine(ExecuteIntercontinentalBridgeClimax());
        }

        IEnumerator ExecuteIntercontinentalBridgeClimax()
        {
            HUDController.Instance?.ShowObjective("THE INTERCONTINENTAL AURORA BRIDGE");

            // 1. Spire base ignition
            VFXController.Instance?.IgniteSpireBridge(spireBasePosition, 6.5f);

            // Visual "Moon 1 Spire Fragment" placement — a glowing crystal flies into the spire (dramatic lore payoff)
            StartCoroutine(PlaceSpireFragmentVisual());

            yield return new WaitForSeconds(1.4f);

            // 2. Five golden threads + aurora ribbon (the defining 15s visual)
            VFXController.Instance?.SpawnIntercontinentalAuroraBridge(spireBasePosition, new Vector3(180f, 40f, 120f)); // toward distant star fort
            Moon5WhiteCityAudioManager.Instance?.PlayBridgeIgnition();

            // 3. Global grid surge (visual + mechanical)
            currentResonancePercent = Mathf.Max(currentResonancePercent, 0.82f);
            Moon5WhiteCityAudioManager.Instance?.SetResonanceLevel(currentResonancePercent);
            GameLoopController.Instance?.QueueRSReward(35f, "moon5_bridge_climax");

            // Max out the living district hum for the bridge moment
            Moon5WhiteCityAudioManager.Instance?.SetResonanceLevel(1.0f);

            // Final brightening of all orbs as the overtone peaks (the whole district sings)
            RefreshAllAmplifiedOrbs();

            // Dramatic final surge on central status orbs + all AmplifiedOrbs for the bridge moment
            SurgeStatusOrbsAndAmplifiedOrbs();

            // 4. Thorne reaction + massive trust
            ThorneController.Instance?.RadioSpireIgnitionAndBridgeFormed();

            // 5. All fountains go wild + permanent world change
            VFXController.Instance?.TriggerPermanentWhiteCityRadiance(districtCenter);

            // 5b. Permanent glowing golden ley-line network (the "Intercontinental Bridge" and grid surge made visible forever - matches Moon 3 golden rails payoff)
            CreatePermanentLeyNetwork();

            // 6. Permanent state + Moon cleared
            bridgeFormed = true;
            Moon5IntercontinentalFastTravelUnlocked = true;
            OnBridgeFormed?.Invoke();
            OnRadioLogEntry?.Invoke("THE INTERCONTINENTAL AURORA BRIDGE IS FORMED. The White City sings across worlds. The radiance endures. Fast travel link established.");

            MoonProgressTracker.Instance?.MarkCleared(5);
            // MoonRewardService is static and operates per-beat (AwardBeat). Cleared-marker triggers reward via tracker.

            PersistMoon5State();

            // Final banner + return portal
            yield return new WaitForSeconds(3.2f);
            HUDController.Instance?.ShowObjective("<b>MOON 5 COMPLETE — THE RADIANCE ENDURES</b>\nThe White City remembers. The bridge is eternal.");
            ReturnPortal.SpawnAt(districtCenter + Vector3.forward * 12f + Vector3.up * 1f);

            // Return portal + fast-travel hook polish (Moon 5 bridge victory, mirrors Moon 3 rail emotional closure + permanent link)
            HUDController.Instance?.ShowObjective("<b>↪ GOLDEN RETURN PORTAL + INTERCONTINENTAL FAST TRAVEL</b>\nThe Aurora Bridge endures. The White City is forever connected.");
            VFXController.Instance?.SpawnPlatformStabilizeVFX(districtCenter + Vector3.forward * 12f + Vector3.up * 2f);
            // Hook ready: Moon5IntercontinentalFastTravelUnlocked is now true for map / future triggers
        }

        /// <summary>
        /// Creates permanent glowing golden ley-line network after bridge ignition.
        /// Makes the "Intercontinental Bridge" and grid surge a lasting visible world change (like Moon 3 golden rails).
        /// Cheap LineRenderers + emissive gold, static, 60fps friendly.
        /// </summary>
        void CreatePermanentLeyNetwork()
        {
            if (_permanentLeyNetwork == null) _permanentLeyNetwork = GameObject.Find("Moon5_PermanentLeyNetwork");
            if (_permanentLeyNetwork != null) return;

            var root = new GameObject("Moon5_PermanentLeyNetwork");
            root.transform.position = districtCenter;
            root.isStatic = true;

            // Connect each pavilion to the central spire with glowing golden lines
            Vector3 spirePos = spireBasePosition + Vector3.up * 8f; // high on the spire

            for (int i = 0; i < 5; i++)
            {
                Vector3 p = pavilionPositions[i] + Vector3.up * 4f; // from the amplified orb height

                var lineGO = new GameObject($"LeyLine_Pav{i+1}_Spire");
                lineGO.transform.SetParent(root.transform);
                var lr = lineGO.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.SetPositions(new[] { p, spirePos });
                lr.startWidth = 0.25f;
                lr.endWidth = 0.15f;
                lr.useWorldSpace = true;

                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.98f, 0.88f, 0.45f);
                mat.SetColor("_EmissionColor", new Color(1f, 0.92f, 0.5f) * 2.2f);
                mat.EnableKeyword("_EMISSION");
                lr.material = mat;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lineGO.isStatic = true;
            }

            // Add cross connections between pavilions for network feel
            for (int i = 0; i < 5; i++)
            {
                Vector3 p1 = pavilionPositions[i] + Vector3.up * 3.5f;
                Vector3 p2 = pavilionPositions[(i + 1) % 5] + Vector3.up * 3.5f;

                var cross = new GameObject($"LeyCross_{i}");
                cross.transform.SetParent(root.transform);
                var lr2 = cross.AddComponent<LineRenderer>();
                lr2.positionCount = 2;
                lr2.SetPositions(new[] { p1, p2 });
                lr2.startWidth = 0.12f;
                lr2.endWidth = 0.12f;
                lr2.material = root.transform.GetChild(0).GetComponent<LineRenderer>().material; // reuse
                cross.isStatic = true;
            }

            // Add subtle pulsing lights along the network for "alive" radiance (cheap)
            for (int l = 0; l < 5; l++)
            {
                var lightGO = new GameObject($"LeyLight_{l}");
                lightGO.transform.SetParent(root.transform);
                lightGO.transform.position = Vector3.Lerp(pavilionPositions[l] + Vector3.up * 4f, spirePos, 0.5f);
                var light = lightGO.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.9f, 0.5f);
                light.intensity = 1.8f;
                light.range = 15f;
                light.shadows = LightShadows.None;
                lightGO.isStatic = true;
            }

            // Task polish: attach cheap ongoing subtle pulse coroutine behavior to root (widths + lights breathe gently)
            root.AddComponent<Moon5LeyNetworkPulsar>();

            // Quick victory marker polish: extra golden crystals as permanent "ley network complete" focal points
            // (scattered along radials + central heart above spire) — makes climax feel like lasting world change
            AddVictoryLeyMarkers(root);

            Debug.Log("[Moon 5] Permanent golden ley network activated - the White City grid remembers the overtone forever. (with live pulse + victory crystals)");
        }

        void AddVictoryLeyMarkers(GameObject root)
        {
            if (root == null) return;

            Color gold = new Color(0.98f, 0.9f, 0.55f);

            // 5 radial mid-point victory crystals (permanent markers of the completed bridge)
            for (int i = 0; i < 5; i++)
            {
                Vector3 mid = Vector3.Lerp(pavilionPositions[i] + Vector3.up * 4f, spireBasePosition + Vector3.up * 8f, 0.48f);
                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = $"Moon5_VictoryLeyCrystal_{i+1}";
                marker.transform.SetParent(root.transform);
                marker.transform.position = mid + Vector3.up * 0.6f + UnityEngine.Random.insideUnitSphere * 0.35f;
                marker.transform.localScale = Vector3.one * 0.38f;
                var mmr = marker.GetComponent<MeshRenderer>();
                if (mmr)
                {
                    mmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mmr.sharedMaterial.color = gold;
                    mmr.sharedMaterial.SetColor("_EmissionColor", new Color(1f, 0.92f, 0.5f) * 3.5f);
                    mmr.sharedMaterial.EnableKeyword("_EMISSION");
                }
                Destroy(marker.GetComponent<Collider>());
                var mlight = marker.AddComponent<Light>();
                mlight.type = LightType.Point;
                mlight.color = gold;
                mlight.intensity = 0.9f;
                mlight.range = 7f;
                mlight.shadows = LightShadows.None;
                marker.isStatic = true;
            }

            // Central Ley Heart — strong focal victory marker high on the spire (the "radiance endures")
            var heart = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            heart.name = "Moon5_LeyHeart_Victory";
            heart.transform.SetParent(root.transform);
            heart.transform.position = spireBasePosition + Vector3.up * 14f;
            heart.transform.localScale = Vector3.one * 0.9f;
            var hmr = heart.GetComponent<MeshRenderer>();
            if (hmr)
            {
                hmr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                hmr.sharedMaterial.color = new Color(1f, 0.95f, 0.6f);
                hmr.sharedMaterial.SetColor("_EmissionColor", Color.yellow * 4.2f);
                hmr.sharedMaterial.EnableKeyword("_EMISSION");
            }
            Destroy(heart.GetComponent<Collider>());
            var hl = heart.AddComponent<Light>();
            hl.color = new Color(1f, 0.92f, 0.5f);
            hl.intensity = 2.8f;
            hl.range = 22f;
            hl.shadows = LightShadows.None;
            heart.isStatic = true;
        }

        /// <summary>
        /// Spawns and animates the Moon 1 spire fragment flying into the anchor during climax.
        /// Gives strong visual "you are completing the bridge" moment.
        /// </summary>
        IEnumerator PlaceSpireFragmentVisual()
        {
            // Spawn a beautiful glowing fragment crystal near the spire base (VFX replacement)
            GameObject fragVFX = new GameObject("Moon1_SpineFragment_Visual_VFX");
            fragVFX.transform.position = spireBasePosition + new Vector3(0, 2f, -4f); // slightly offset as if player brought it
            
            ParticleSystem psFrag = fragVFX.AddComponent<ParticleSystem>();
            var mainFrag = psFrag.main;
            mainFrag.startLifetime = 2.0f;
            mainFrag.startSpeed = 0.3f;
            mainFrag.startSize = 0.7f;
            mainFrag.startColor = new Color(1f, 0.95f, 0.6f, 1f);
            mainFrag.maxParticles = 100;
            mainFrag.loop = true;
            
            var emissionFrag = psFrag.emission;
            emissionFrag.rateOverTime = 50f;
            
            var shapeFrag = psFrag.shape;
            shapeFrag.shapeType = ParticleSystemShapeType.Sphere;
            shapeFrag.radius = 0.35f;
            
            var rendererFrag = fragVFX.GetComponent<ParticleSystemRenderer>();
            rendererFrag.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            rendererFrag.material.SetColor("_BaseColor", new Color(1f, 0.95f, 0.6f));
            rendererFrag.material.EnableKeyword("_EMISSION");
            rendererFrag.material.SetColor("_EmissionColor", Color.yellow * 2.5f);
            
            psFrag.Play();

            // Add a light for radiance
            var light = fragVFX.AddComponent<Light>();
            light.color = new Color(1f, 0.92f, 0.5f);
            light.intensity = 3f;
            light.range = 8f;

            // Lerp the fragment up into the spire over 2.5 seconds
            Vector3 startPos = fragVFX.transform.position;
            Vector3 endPos = spireBasePosition + Vector3.up * 7f;

            float t = 0f;
            float dur = 2.5f;

            while (t < dur)
            {
                t += Time.deltaTime;
                float u = Mathf.SmoothStep(0, 1, t / dur);
                fragVFX.transform.position = Vector3.Lerp(startPos, endPos, u);
                fragVFX.transform.Rotate(Vector3.up, 180f * Time.deltaTime, Space.World); // spinning as it locks in
                yield return null;
            }

            // Absorption burst
            VFXController.Instance?.SpawnPlatformStabilizeVFX(endPos);
            VFXController.Instance?.IgniteSpireBridge(endPos, 1.5f); // extra ignition flash

            Destroy(fragVFX, 0.8f);
        }

        void PersistMoon5State()
        {
            var save = Save.SaveManager.Instance?.CurrentSave;
            if (save == null) return;

            // Ensure Moon5 block exists
            if (save.moon5 == null) save.moon5 = new Moon5State();
            save.moon5.pavilionsAmplified = pavilionsAmplified;
            save.moon5.dockStage = dockStage;
            save.moon5.spirePlaced = spirePlaced;
            save.moon5.bridgeFormed = bridgeFormed;
            save.moon5.whiteCityRadianceActive = bridgeFormed;

            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>
        /// Restores Moon5 save block state + applies permanent visuals (leys, orbs, amplified pavilions, dock, max resonance).
        /// Makes re-entry / cleared playthroughs rock-solid with the emotional "radiance endures" world change.
        /// Lightweight: uses existing visual helpers; only full bridge state + partial count for slice playability.
        /// </summary>
        void RestoreMoon5StateFromSave()
        {
            var save = Save.SaveManager.Instance?.CurrentSave;
            if (save == null || save.moon5 == null) return;

            var s = save.moon5;

            if (s.bridgeFormed)
            {
                // Full victory state — permanent golden ley + bright orbs + max drone (emotional payoff preserved)
                CreatePavilionStatusOrbs(); // ensure status orbs exist for lighting even on early restore call
                pavilionsAmplified = 5;
                dockStage = 4;
                spirePlaced = true;
                bridgeFormed = true;
                Moon5IntercontinentalFastTravelUnlocked = true;
                currentResonancePercent = 1f;

                for (int i = 0; i < 5; i++) _pavilionDone[i] = true;

                // Apply all pavilion gold visuals + orbs (no side FX, no audio/haptic/Thorne)
                for (int i = 0; i < 5; i++)
                {
                    ApplyPavilionAmplifiedVisualOnly(i);
                    if (_pavilionStatusOrbs[i] != null)
                    {
                        var orb = _pavilionStatusOrbs[i];
                        var l = orb.GetComponent<Light>(); if (l) l.intensity = 4.8f;
                        var mr = orb.GetComponent<MeshRenderer>();
                        if (mr) { mr.material.color = new Color(1f, 0.95f, 0.6f); mr.material.SetColor("_EmissionColor", new Color(1f, 0.92f, 0.6f) * 3.5f); }
                        orb.transform.localScale = Vector3.one * 1.05f;
                    }
                }

                RefreshAllAmplifiedOrbs();
                
                if (_permanentLeyNetwork == null) _permanentLeyNetwork = GameObject.Find("Moon5_PermanentLeyNetwork");
                if (_permanentLeyNetwork != null)
                    ApplyDockVisualOnly();

                if (GameObject.Find("Moon5_PermanentLeyNetwork") == null)
                    CreatePermanentLeyNetwork();

                Moon5WhiteCityAudioManager.Instance?.StartOvertoneDrone(1.0f); // ensure living hum at max for cleared re-entry
                Moon5WhiteCityAudioManager.Instance?.SetResonanceLevel(1.0f);

                HUDController.Instance?.ShowObjective("<b>THE RADIANCE ENDURES</b>\nWhite City grid permanent. Bridge & fast travel active.");
                return;
            }

            if (s.pavilionsAmplified > 0)
            {
                CreatePavilionStatusOrbs(); // ensure for status orb lighting on restore
                pavilionsAmplified = Mathf.Clamp(s.pavilionsAmplified, 0, 5);
                dockStage = Mathf.Clamp(s.dockStage, 0, 4);
                spirePlaced = s.spirePlaced;
                bridgeFormed = s.bridgeFormed;
                currentResonancePercent = Mathf.Clamp01(0.55f + (pavilionsAmplified / 5f) * 0.45f);

                for (int i = 0; i < pavilionsAmplified && i < 5; i++)
                {
                    _pavilionDone[i] = true;
                    ApplyPavilionAmplifiedVisualOnly(i);
                }

                // Light status orbs for restored count
                for (int i = 0; i < pavilionsAmplified && i < 5; i++)
                {
                    if (_pavilionStatusOrbs[i] != null)
                    {
                        var orb = _pavilionStatusOrbs[i];
                        var l = orb.GetComponent<Light>(); if (l) l.intensity = 2.0f + currentResonancePercent * 1.2f;
                        var mr = orb.GetComponent<MeshRenderer>();
                        if (mr) { mr.material.color = new Color(1f, 0.92f, 0.5f); mr.material.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.5f) * 1.8f); }
                    }
                }

                RefreshAllAmplifiedOrbs();
                if (pavilionsAmplified >= 2) RaiseFloatingPlatforms(Mathf.Min(2, pavilionsAmplified));
                if (dockStage > 0) ApplyDockVisualOnly();

                Moon5WhiteCityAudioManager.Instance?.SetResonanceLevel(currentResonancePercent);
            }
        }

        // Helper: apply gold material + AmplifiedOrb to a pavilion proxy WITHOUT events, VFX, audio, haptics or HUD (for save restore)
        void ApplyPavilionAmplifiedVisualOnly(int index)
        {
            string name = $"Moon5_Pavilion_{index + 1:00}";
            var p = GameObject.Find(name);
            if (p == null) return;

            var mr = p.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material.color = new Color(1f, 0.95f, 0.55f);
                mr.material.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.4f) * 1.5f);
                mr.material.EnableKeyword("_EMISSION");
            }

            if (p.transform.Find("AmplifiedOrb") == null)
            {
                var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.name = "AmplifiedOrb";
                orb.transform.SetParent(p.transform);
                orb.transform.localPosition = Vector3.up * 4.5f;
                orb.transform.localScale = Vector3.one * (1.1f + currentResonancePercent * 0.2f);
                var omr = orb.GetComponent<MeshRenderer>();
                if (omr) omr.material.color = new Color(1f, 0.92f, 0.5f);
                var light = orb.AddComponent<Light>();
                light.color = new Color(1f, 0.9f, 0.5f);
                light.intensity = 1.8f + currentResonancePercent * 2.2f;
                light.range = 10f + currentResonancePercent * 6f;
                Destroy(orb.GetComponent<Collider>());
                _amplifiedOrbLights.Add(light);
            }
        }

        void ApplyDockVisualOnly()
        {
            string dockName = "Moon5_AirshipDock";
            var dock = GameObject.Find(dockName);
            if (dock != null)
            {
                dock.transform.localScale = new Vector3(22f + dockStage * 1.8f, 1.2f + dockStage * 0.3f, 38f);
            }
        }

        // ─── Amplified Orb Resonance Polish (task) ───
        void UpdateAmplifiedOrbs()
        {
            if (_amplifiedOrbLights == null || _amplifiedOrbLights.Count == 0) return;

            bool reduced = Tartaria.UI.SettingsOverlay.IsReducedMotion;
            float t = Time.time;
            float pulse = reduced ? 1f : (1f + Mathf.Sin(t * 1.65f) * 0.085f); // gentle organic breathing, reduced-motion safe
            float resonanceBoost = 0.85f + currentResonancePercent * 0.42f;   // orbs scale brighter as overall grid resonance rises

            for (int i = _amplifiedOrbLights.Count - 1; i >= 0; i--)
            {
                var light = _amplifiedOrbLights[i];
                if (light == null)
                {
                    _amplifiedOrbLights.RemoveAt(i);
                    continue;
                }
                light.intensity = (2.1f + currentResonancePercent * 1.6f) * resonanceBoost * pulse;
                light.range = 9.5f + currentResonancePercent * 7.5f + (pulse - 1f) * 1.8f;
            }
        }

        IEnumerator AmplifiedOrbPulseLoop()
        {
            while (true)
            {
                UpdateAmplifiedOrbs();
                yield return new WaitForSeconds(0.09f); // cheap update, ~11x/sec — plenty for slow elegant pulse, 60fps friendly
            }
        }

        // Public hook for MoonMechanicActivator / external tuning systems
        public void OnPavilionTunedSuccess(int index) => AmplifyPavilion(index);

        /// <summary>Query for HUD / UI. Safe per-pavilion state.</summary>
        public bool IsPavilionDone(int index)
        {
            if (index < 0 || index >= 5) return false;
            return _pavilionDone[index];
        }

        // Called from spire interactable
        public void OnSpireFragmentPlaced() => PlaceSpireFragment();

        void Update()
        {
            // Playtest controls for fast vertical slice validation (keyboard + gamepad friendly)
            if (UnityEngine.Input.GetKeyDown(KeyCode.T))
            {
                // Amplify next unfinished pavilion
                for (int i = 0; i < 5; i++)
                {
                    if (!_pavilionDone[i])
                    {
                        AmplifyPavilion(i);
                        break;
                    }
                }
            }

            // Playtest: K = force spire climax when ready (full emotional payoff)
            if (UnityEngine.Input.GetKeyDown(KeyCode.K) && pavilionsAmplified >= 5 && !spirePlaced)
                PlaceSpireFragment();
        }
    }

    // Runtime trigger components (added by scaffold). Keep tiny and self-contained.
    public class Moon5StartTrigger : MonoBehaviour
    {
        WhiteCityAmplificationController _cachedCtrl;
        bool _fired;

        void OnTriggerEnter(Collider other)
        {
            if (_fired || !other.CompareTag("Player")) return;
            _fired = true;
            if (_cachedCtrl == null) _cachedCtrl = FindObjectOfType<WhiteCityAmplificationController>();
            _cachedCtrl?.BeginAmplificationSequence();
            ThorneController.Instance?.RadioFirstContact();
        }
    }

    public class Moon5ThorneRadioTrigger : MonoBehaviour
    {
        bool _heard;
        void OnTriggerEnter(Collider other)
        {
            if (_heard || !other.CompareTag("Player")) return;
            _heard = true;
            ThorneController.Instance?.RadioFirstContact();
        }
    }

    /// <summary>Playtest helper: click a pavilion proxy to fire amplification (demo until real tuning nodes wired).</summary>
    public class Moon5PavilionClickAmplifier : MonoBehaviour
    {
        public int pavilionIndex;
        WhiteCityAmplificationController _cachedCtrl;

        void OnMouseDown()
        {
            if (_cachedCtrl == null) _cachedCtrl = FindObjectOfType<WhiteCityAmplificationController>();
            _cachedCtrl?.AmplifyPavilion(pavilionIndex);
        }
    }

    /// <summary>Proximity + hold E (or F310 South) interactor for pavilions.
    /// POLISHED TUNING MOMENT: Hold 1.5s for real frequency match — pulsing orb + rising-pitch overtone harmonics via Moon5Audio.
    /// Success = amplify + stinger. Reduced-motion aware. Uses existing VFX/audio paths. Empowering "the city responds" feel.
    /// </summary>
    public class Moon5PavilionInteractor : MonoBehaviour
    {
        public int pavilionIndex;
        WhiteCityAmplificationController _cachedCtrl;
        GameObject _cachedPavilion;
        bool _nearPlayer;
        float _holdTime;
        const float HOLD_REQUIRED = 1.5f;
        GameObject _pulseOrb; // temporary visual feedback during hold (preview before permanent AmplifiedOrb)

        void Update()
        {
            if (_cachedCtrl == null) _cachedCtrl = FindObjectOfType<WhiteCityAmplificationController>();
            if (!_nearPlayer) return;
            
            var ctrl = _cachedCtrl;
            if (ctrl != null && ctrl.IsPavilionDone(pavilionIndex))
            {
                CleanupPulseOrb();
                return;
            }

            bool holding = UnityEngine.Input.GetKey(KeyCode.E) || UnityEngine.Input.GetKey(KeyCode.Joystick1Button0);

            if (holding)
            {
                if (_holdTime <= 0f)
                {
                    // Fresh hold start: kick off live tuning audio (rising harmonics for frequency match)
                    Vector3 pos = transform.position;
                    Moon5WhiteCityAudioManager.Instance?.StartPavilionTuning(pavilionIndex, pos);
                    HUDController.Instance?.ShowObjective($"PAVILION {pavilionIndex + 1} — MATCHING OVERTONE FREQUENCY (HOLD E)");
                }

                _holdTime += Time.deltaTime;

                // Create / update pulsing preview orb (frequency lock visual)
                EnsurePulseOrb();

                float norm = Mathf.Clamp01(_holdTime / HOLD_REQUIRED);
                if (_pulseOrb != null)
                {
                    bool reduced = Tartaria.UI.SettingsOverlay.IsReducedMotion;

                    float baseS = 0.65f + norm * 1.45f;
                    float pulse = reduced ? 1f : (0.92f + 0.16f * Mathf.Sin(Time.time * 10.5f + norm * 5f));
                    _pulseOrb.transform.localScale = Vector3.one * baseS * pulse;

                    var light = _pulseOrb.GetComponent<Light>();
                    if (light != null)
                    {
                        light.intensity = reduced ? (1.1f + norm * 3.1f) : (0.9f + norm * 4.4f + Mathf.Sin(Time.time * 13f) * 0.7f);
                        light.color = Color.Lerp(new Color(0.7f, 0.85f, 1f), new Color(1f, 0.95f, 0.5f), norm);
                    }

                    var mr = _pulseOrb.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        mr.material.color = Color.Lerp(new Color(0.82f, 0.9f, 1f, 0.85f), new Color(1f, 0.96f, 0.6f, 1f), norm);
                    }
                }

                // Drive audio rising pitch + dynamic volume (the core "tuning" sensation)
                float resonanceProxy = 0.55f + norm * 0.42f;
                Moon5WhiteCityAudioManager.Instance?.UpdateTuningProgress(pavilionIndex, norm, resonanceProxy);

                if (_holdTime >= HOLD_REQUIRED)
                {
                    if (_cachedCtrl != null)
                        _cachedCtrl.AmplifyPavilion(pavilionIndex);
                    else
                        Moon5WhiteCityAudioManager.Instance?.PlayAmplificationStinger(pavilionIndex, 0.95f);

                    // Success audio already triggered by controller amplify path; stop live tone cleanly
                    Moon5WhiteCityAudioManager.Instance?.StopPavilionTuning(pavilionIndex, false);

                    _holdTime = 0f;
                    CleanupPulseOrb();
                }
            }
            else
            {
                CancelHoldVisualsAndAudio();
            }
        }

        void EnsurePulseOrb()
        {
            if (_pulseOrb != null) return;

            if (_cachedPavilion == null) _cachedPavilion = GameObject.Find($"Moon5_Pavilion_{pavilionIndex + 1:00}");
            var parent = _cachedPavilion ?? gameObject;
            
            // Try loading ScanPulse prefab first
            GameObject pulsePrefab = Resources.Load<GameObject>("Prefabs/VFX/ScanPulse");
            
            if (pulsePrefab != null)
            {
                _pulseOrb = Instantiate(pulsePrefab);
                _pulseOrb.name = "TuningPulse_Preview";
                _pulseOrb.transform.SetParent(parent.transform);
                _pulseOrb.transform.localPosition = Vector3.up * 5.2f;
                
                ParticleSystem ps = _pulseOrb.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }
            else
            {
                // Fallback: create runtime pulse particle system
                _pulseOrb = new GameObject("TuningPulse_Preview_VFX");
                _pulseOrb.transform.SetParent(parent.transform);
                _pulseOrb.transform.localPosition = Vector3.up * 5.2f;
                
                ParticleSystem ps = _pulseOrb.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 2.0f;
                main.startSpeed = 1.0f;
                main.startSize = 0.7f;
                main.startColor = new Color(0.8f, 0.9f, 1f, 0.8f);
                main.maxParticles = 100;
                main.loop = true;
                
                var emission = ps.emission;
                emission.rateOverTime = 30f;
                
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.35f;
                
                var renderer = _pulseOrb.GetComponent<ParticleSystemRenderer>();
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                renderer.material.SetColor("_BaseColor", new Color(0.5f, 0.7f, 1f));
                
                ps.Play();
                
                Debug.LogWarning("[WhiteCityAmplification] ScanPulse prefab missing - using runtime ParticleSystem");
            }

            var light = _pulseOrb.AddComponent<Light>();
            light.color = new Color(0.7f, 0.85f, 1f);
            light.intensity = 1.6f;
            light.range = 9f;
            light.shadows = LightShadows.None;
        }

        void CancelHoldVisualsAndAudio()
        {
            if (_holdTime > 0.02f)
            {
                // Cancelled mid-tune: stop rising tone, play soft cue
                Moon5WhiteCityAudioManager.Instance?.StopPavilionTuning(pavilionIndex, true);
                AudioManager.Instance?.PlaySFX2D("TuneFail", 0.18f);
            }
            _holdTime = 0f;
            CleanupPulseOrb();
        }

        void CleanupPulseOrb()
        {
            if (_pulseOrb != null)
            {
                Destroy(_pulseOrb);
                _pulseOrb = null;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) _nearPlayer = true;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) 
            {
                _nearPlayer = false;
                CancelHoldVisualsAndAudio();
            }
        }

        void OnDisable()
        {
            CancelHoldVisualsAndAudio();
        }
    }

    /// <summary>Simple trigger at the spire base. When player enters + all pavilions amplified, prompt + allow placement (or auto for climax).</summary>
    public class Moon5SpirePlacementTrigger : MonoBehaviour
    {
        bool _prompted;
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            var ctrl = FindObjectOfType<WhiteCityAmplificationController>();
            if (ctrl == null || ctrl.spirePlaced || ctrl.pavilionsAmplified < 5) return;

            if (!_prompted)
            {
                _prompted = true;
                HUDController.Instance?.ShowObjective("<b>THE SPIRE FRAGMENT</b>\nPress [E] or [K] to lock the Moon 1 fragment and ignite the Intercontinental Bridge");
            }

            // Auto-place on entry for smooth vertical slice flow (or player can press K)
            // For now auto after short delay to feel intentional
            ctrl.OnSpireFragmentPlaced();
        }
    }

    /// <summary>
    /// Cheap self-contained subtle pulse animator for the permanent ley network root.
    /// Gently modulates LineRenderer widths (breathing ribbons) and child point light intensities.
    /// 100% reduced-motion friendly (disables when setting on). Pure Update, tiny footprint, 60fps.
    /// Attached to Moon5_PermanentLeyNetwork at climax for "the grid lives on" victory atmosphere.
    /// </summary>
    public class Moon5LeyNetworkPulsar : MonoBehaviour
    {
        LineRenderer[] _lines;
        Light[] _lights;
        bool _reduced;
        float _pulsePhase;

        void Awake()
        {
            _reduced = Tartaria.UI.SettingsOverlay.IsReducedMotion;
            _lines = GetComponentsInChildren<LineRenderer>(true);
            _lights = GetComponentsInChildren<Light>(true);

            if (_reduced)
            {
                // Lock beautiful static radiance, no animation cost or motion
                enabled = false;
            }
        }

        void Update()
        {
            if (_reduced || _lines == null) return;

            float t = Time.time * 0.68f + _pulsePhase;
            float wPulse = 1f + Mathf.Sin(t) * 0.042f;           // ±4.2% ultra-subtle ribbon breathing (elegant, not flashy)
            float lPulse = 1f + Mathf.Sin(t * 0.93f + 1.7f) * 0.095f; // phase offset lights, ±9.5% gentle glow swell

            foreach (var lr in _lines)
            {
                if (lr == null) continue;
                // Radials are wider (0.25 start), crosses narrower (0.12)
                float baseW = (lr.startWidth > 0.18f) ? 0.25f : 0.12f;
                lr.startWidth = baseW * wPulse;
                lr.endWidth = baseW * 0.62f * wPulse;
            }

            foreach (var lt in _lights)
            {
                if (lt == null) continue;
                lt.intensity = 1.72f * lPulse; // fixed elegant base * gentle pulse (ley lights breathe softly forever)
            }
        }
    }
}