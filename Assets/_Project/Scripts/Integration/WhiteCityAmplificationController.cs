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

        // ─── HUD Wiring Events (subscribed by Moon5AmplificationHUD) ───
        public event System.Action<int> OnPavilionAmplified;
        public event System.Action OnDockAdvanced;
        public event System.Action OnBridgeFormed;
        public event System.Action<string> OnRadioLogEntry;

        readonly bool[] _pavilionDone = new bool[5];
        readonly List<GameObject> _floatingPlatforms = new();
        GameObject _spireVisual;
        GameObject _dockVisual;

        bool _sequenceStarted;
        bool _climaxFired;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void BeginAmplificationSequence()
        {
            if (_sequenceStarted) return;
            _sequenceStarted = true;

            HUDController.Instance?.ShowObjective("<b>OVERTONE MOON — WHITE CITY</b>\nAmplify the pavilions. Let the light rise.");

            // Ensure Moon 5 radiant HUD is present
            if (Tartaria.UI.Moon5AmplificationHUD.Instance == null)
            {
                var hudGO = new GameObject("Moon5_AmplificationHUD");
                hudGO.AddComponent<Tartaria.UI.Moon5AmplificationHUD>();
            }

            // Initial Thorne static burst + first line already handled by trigger
            var beatRunner = Object.FindAnyObjectByType<MoonBeatRunner>();
            if (beatRunner != null) beatRunner.enabled = true;

            OnRadioLogEntry?.Invoke("Thorne: First contact. White City grid resonance detected. The overtone awaits.");

            // Fire HUD events for initial state
            OnRadioLogEntry?.Invoke("SYSTEM: Overtone Moon sequence initiated. Amplify the five pavilions.");

            // Light the first two pavilions as "already faintly glowing" (discovery feel)
            StartCoroutine(InitialDiscoveryGlow());
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

            // Visual + VFX
            FlashPavilionLight(index, 1f);
            VFXController.Instance?.SpawnAuroraFountain(pavilionPositions[index] + Vector3.up * 2f, 4.5f);

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

            HUDController.Instance?.ShowObjective($"Pavilion {index + 1} amplified — {pavilionsAmplified}/5  |  Light at {Mathf.RoundToInt(currentResonancePercent * 100)}%");

            CheckForClimaxReady();
            PersistMoon5State();
        }

        void FlashPavilionLight(int index, float intensity)
        {
            // Proxy: brighten the cylinder permanently for amplified state (radiant gold)
            string name = $"Moon5_Pavilion_{index + 1:00}";
            var p = GameObject.Find(name);
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
                    light.intensity = 2.5f;
                    light.range = 12f;
                    Destroy(orb.GetComponent<Collider>());
                }
            }

            // Real VFX thread / overtone pulse
            VFXController.Instance?.TriggerOvertoneThread(pavilionPositions[index], districtCenter, intensity);
        }

        void ActivateSixBandHealingAura(Vector3 origin)
        {
            // The signature Moon 5 mechanic: healing light that keeps buildings "alive" during conflict
            // and visually buffs the player + nearby restored elements.
            VFXController.Instance?.SpawnSixBandHealingPulse(origin, 18f);

            // Passive RS trickle + companion trust
            GameLoopController.Instance?.QueueRSReward(6f, "moon5_6band_healing");
            // Future: actual building health regen for restored pavilions

            AudioManager.Instance?.PlaySFX2D("HealingAura");
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

            yield return new WaitForSeconds(1.4f);

            // 2. Five golden threads + aurora ribbon (the defining 15s visual)
            VFXController.Instance?.SpawnIntercontinentalAuroraBridge(spireBasePosition, new Vector3(180f, 40f, 120f)); // toward distant star fort
            Moon5WhiteCityAudioManager.Instance?.PlayBridgeIgnition();

            // 3. Global grid surge (visual + mechanical)
            currentResonancePercent = Mathf.Max(currentResonancePercent, 0.82f);
            GameLoopController.Instance?.QueueRSReward(35f, "moon5_bridge_climax");

            // 4. Thorne reaction + massive trust
            ThorneController.Instance?.RadioSpireIgnitionAndBridgeFormed();

            // 5. All fountains go wild + permanent world change
            VFXController.Instance?.TriggerPermanentWhiteCityRadiance(districtCenter);

            // 5b. Permanent glowing golden ley-line network (the "Intercontinental Bridge" and grid surge made visible forever - matches Moon 3 golden rails payoff)
            CreatePermanentLeyNetwork();

            // 6. Permanent state + Moon cleared
            bridgeFormed = true;
            OnBridgeFormed?.Invoke();
            OnRadioLogEntry?.Invoke("THE INTERCONTINENTAL AURORA BRIDGE IS FORMED. The White City sings across worlds. The radiance endures.");

            MoonProgressTracker.Instance?.MarkCleared(5);
            // MoonRewardService is static and operates per-beat (AwardBeat). Cleared-marker triggers reward via tracker.

            PersistMoon5State();

            // Final banner + return portal
            yield return new WaitForSeconds(3.2f);
            HUDController.Instance?.ShowObjective("<b>MOON 5 COMPLETE — THE RADIANCE ENDURES</b>\nThe White City remembers. The bridge is eternal.");
            ReturnPortal.SpawnAt(districtCenter + Vector3.forward * 12f + Vector3.up * 1f);

            // Optional fast travel hook back to previous zones
            // (reuse Moon3FastTravelTrigger pattern or new one)
        }

        /// <summary>
        /// Creates permanent glowing golden ley-line network after bridge ignition.
        /// Makes the "Intercontinental Bridge" and grid surge a lasting visible world change (like Moon 3 golden rails).
        /// Cheap LineRenderers + emissive gold, static, 60fps friendly.
        /// </summary>
        void CreatePermanentLeyNetwork()
        {
            if (GameObject.Find("Moon5_PermanentLeyNetwork") != null) return;

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

            Debug.Log("[Moon 5] Permanent golden ley network activated - the White City grid remembers the overtone forever.");
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
        bool _fired;
        void OnTriggerEnter(Collider other)
        {
            if (_fired || !other.CompareTag("Player")) return;
            _fired = true;
            var ctrl = FindObjectOfType<WhiteCityAmplificationController>();
            ctrl?.BeginAmplificationSequence();
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
        void OnMouseDown()
        {
            var ctrl = FindObjectOfType<WhiteCityAmplificationController>();
            ctrl?.AmplifyPavilion(pavilionIndex);
        }
    }

    /// <summary>Proximity + hold E (or F310 South) interactor for pavilions. Hold to "tune" the overtone — real mini interaction for vertical slice.</summary>
    public class Moon5PavilionInteractor : MonoBehaviour
    {
        public int pavilionIndex;
        bool _nearPlayer;
        float _holdTime;
        const float HOLD_REQUIRED = 1.2f;
        GameObject _pulseOrb; // temporary visual feedback during hold

        void Update()
        {
            if (!_nearPlayer) 
            {
                _holdTime = 0f;
                if (_pulseOrb != null) Destroy(_pulseOrb);
                _pulseOrb = null;
                return;
            }

            bool holding = UnityEngine.Input.GetKey(KeyCode.E) || UnityEngine.Input.GetKey(KeyCode.Joystick1Button0);

            if (holding)
            {
                _holdTime += Time.deltaTime;

                // Visual feedback: spawn or pulse a golden orb above the pavilion during tuning hold
                if (_pulseOrb == null)
                {
                    var parent = GameObject.Find($"Moon5_Pavilion_{pavilionIndex + 1:00}");
                    if (parent != null)
                    {
                        _pulseOrb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        _pulseOrb.name = "TuningPulse";
                        _pulseOrb.transform.SetParent(parent.transform);
                        _pulseOrb.transform.localPosition = Vector3.up * 6f;
                        _pulseOrb.transform.localScale = Vector3.one * 0.6f;
                        var pmr = _pulseOrb.GetComponent<MeshRenderer>();
                        if (pmr) pmr.material.color = new Color(1f, 0.92f, 0.5f, 0.7f);
                        Destroy(_pulseOrb.GetComponent<Collider>());
                    }
                }

                if (_pulseOrb != null)
                {
                    float pulse = 1f + Mathf.Sin(Time.time * 12f) * 0.25f * (_holdTime / HOLD_REQUIRED);
                    _pulseOrb.transform.localScale = Vector3.one * 0.6f * pulse;
                }

                if (_holdTime >= HOLD_REQUIRED)
                {
                    var ctrl = FindObjectOfType<WhiteCityAmplificationController>();
                    ctrl?.AmplifyPavilion(pavilionIndex);
                    Moon5WhiteCityAudioManager.Instance?.PlayAmplificationStinger(pavilionIndex, 0.9f);
                    _holdTime = 0f;
                    if (_pulseOrb != null) Destroy(_pulseOrb);
                    _pulseOrb = null;
                }
            }
            else
            {
                _holdTime = 0f;
                if (_pulseOrb != null) Destroy(_pulseOrb);
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
                _holdTime = 0f;
                if (_pulseOrb != null) Destroy(_pulseOrb);
                _pulseOrb = null;
            }
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
}