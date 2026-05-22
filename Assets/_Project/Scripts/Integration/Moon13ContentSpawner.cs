using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13: COSMIC MOON — "The Presence of Enduring"
    /// Final node activation + Echo realm travel + Zereth confrontation + 3-way ending choice.
    /// Global grid 95% → 100%. The 13th Moon rises, the 17th Hour approaches. Cosmic alignment.
    /// Discovery: Final node beneath New Chicago, Echo realms shimmer into existence.
    /// </summary>
    public class Moon13ContentSpawner : MonoBehaviour
    {
        [Header("Moon 13 State")]
        [SerializeField] bool moon13Unlocked;
        [SerializeField] bool finalNodeActivated;
        [SerializeField] EndingPath chosenPath = EndingPath.None;

        [Header("Final Node")]
        [SerializeField] Vector3 finalNodePoint = new(0f, -50f, 0f);  // Deepest mud layer
        [SerializeField] GameObject finalNodePrefab;

        [Header("Echo Realms")]
        [SerializeField] Vector3[] echoRealmGatePoints;  // 3 realm portals
        bool _goldenAgeRealmVisited;
        bool _dissonantRealmVisited;
        bool _floodMomentRealmVisited;

        [Header("Zereth Confrontation")]
        [SerializeField] GameObject zerethEchoPrefab;
        bool _zerethConfrontationComplete;
        ZerethResonanceDialogue _zerethResonanceSystem;

        [Header("Companion Farewells")]
        CompanionFarewellSystem _farewellSystem;
        bool _farewellsComplete;

        readonly List<GameObject> _echoRealms = new();
        GameObject _finalNode;
        GameObject _zerethEcho;
        bool _contentSpawned;

        public bool IsMoon13Active => moon13Unlocked && !finalNodeActivated;
        public bool AllRealmsVisited => _goldenAgeRealmVisited && _dissonantRealmVisited && _floodMomentRealmVisited;

        public enum EndingPath
        {
            None,
            Harmony,  // Forgive Zereth, merge timelines, Golden Age restored
            Echo,     // Parallel timelines preserved, switch between realities
            Reset     // Side with Cabal philosophy, controlled grid distribution
        }

        void Awake()
        {
            // Check save state
            moon13Unlocked = SaveManager.Instance?.GetMoonProgress(13) > 0f;

            // Wire save/load events
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave += OnSave;
                SaveManager.Instance.OnAfterLoad += OnLoad;
            }
        }

        void OnDestroy()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave -= OnSave;
                SaveManager.Instance.OnAfterLoad -= OnLoad;
            }
        }

        void OnSave(SaveData sd)
        {
            // P0 CRITICAL: Ending choice MUST persist
            sd.SetMoonFlag(13, "finalNodeActivated", finalNodeActivated);
            sd.SetMoonFlag(13, "chosenPath", (int)chosenPath);
            sd.SetMoonFlag(13, "goldenAgeRealmVisited", _goldenAgeRealmVisited);
            sd.SetMoonFlag(13, "dissonantRealmVisited", _dissonantRealmVisited);
            sd.SetMoonFlag(13, "floodMomentRealmVisited", _floodMomentRealmVisited);
            sd.SetMoonFlag(13, "zerethConfrontationComplete", _zerethConfrontationComplete);
            sd.SetMoonFlag(13, "farewellsComplete", _farewellsComplete);

            // Save Zereth resonance phase
            if (_zerethResonanceSystem != null)
            {
                sd.SetMoonFlag(13, "zerethResonancePhase", _zerethResonanceSystem.GetCurrentPhase());
            }

            // Save farewell state
            if (_farewellSystem != null)
            {
                var farewellState = _farewellSystem.GetFarewellState();
                for (int i = 0; i < farewellState.Length; i++)
                {
                    sd.SetMoonFlag(13, $"farewell_{i}", farewellState[i]);
                }
            }
        }

        void OnLoad(SaveData sd)
        {
            // Restore critical ending choice state
            finalNodeActivated = sd.GetMoonFlag(13, "finalNodeActivated");
            chosenPath = (EndingPath)sd.GetMoonFlag(13, "chosenPath", 0);
            _goldenAgeRealmVisited = sd.GetMoonFlag(13, "goldenAgeRealmVisited");
            _dissonantRealmVisited = sd.GetMoonFlag(13, "dissonantRealmVisited");
            _floodMomentRealmVisited = sd.GetMoonFlag(13, "floodMomentRealmVisited");
            _zerethConfrontationComplete = sd.GetMoonFlag(13, "zerethConfrontationComplete");
            _farewellsComplete = sd.GetMoonFlag(13, "farewellsComplete");

            // Restore Zereth resonance phase
            if (_zerethResonanceSystem != null)
            {
                int resonancePhase = sd.GetMoonFlag(13, "zerethResonancePhase", 0);
                _zerethResonanceSystem.SetCurrentPhase(resonancePhase);
            }

            // Restore farewell state
            if (_farewellSystem != null)
            {
                var farewellState = new bool[4];
                for (int i = 0; i < 4; i++)
                {
                    farewellState[i] = sd.GetMoonFlag(13, $"farewell_{i}");
                }
                _farewellSystem.RestoreFarewellState(farewellState);
            }

            Debug.Log($"[Moon 13] State loaded: choice={chosenPath}, realms={AllRealmsVisited}, finalNode={finalNodeActivated}, farewells={_farewellsComplete}");
        }

        void Start()
        {
            if (moon13Unlocked && !_contentSpawned)
            {
                SpawnMoon13Content();
            }
        }

        public void UnlockMoon13()
        {
            if (moon13Unlocked) return;

            moon13Unlocked = true;
            SaveManager.Instance?.SetMoonProgress(13, 5f);
            Debug.Log("[Moon 13] COSMIC MOON unlocked — The 13th Moon rises. The 17th Hour approaches. The sky trembles.");

            SpawnMoon13Content();
        }

        void SpawnMoon13Content()
        {
            _contentSpawned = true;

            Debug.Log("[Moon 13] Spawning Final Node + Echo Realm content");

            // Final node beneath New Chicago (deepest mud)
            SpawnFinalNode();

            // 3 Echo realm portals
            SpawnEchoRealmGates();

            // Companion farewell system (activated after Zereth confrontation)
            SpawnCompanionFarewellSystem();

            // Ambient audio: Aether tremor
            var aetherAmbience = Audio.AudioManager.Instance?.PlayLoopingSFX("AetherTremor", finalNodePoint, 0.5f);
            if (aetherAmbience != null)
            {
                Debug.Log("[Moon 13] Aether tremor ambient active — reality thinning");
            }

            // Quest activation
            QuestManager.Instance?.ActivateQuest("moon13_final_node_discovery");

            // Zereth dialogue (clearer now)
            DialogueManager.Instance?.PlayContextDialogue("zereth_you_deserve_truth");
        }

        void SpawnFinalNode()
        {
            if (finalNodePrefab != null)
            {
                _finalNode = Instantiate(finalNodePrefab, finalNodePoint, Quaternion.identity);
                _finalNode.name = "FinalNode_13thMoon";
            }
            else
            {
                // Multi-part citadel final node (no single primitives)
                _finalNode = new GameObject("FinalNode_13thMoon");
                _finalNode.transform.position = finalNodePoint;

                // Multi-layer convergence chamber (no primitives)
                // Outer dome shell - large rock formation
                GameObject outerRockPrefab = Resources.Load<GameObject>("Prefabs/Props/KayKit/Rock_Large_01");
                GameObject domeOuter;
                if (outerRockPrefab != null)
                {
                    domeOuter = Instantiate(outerRockPrefab);
                    domeOuter.name = "DomeOuter";
                    domeOuter.transform.SetParent(_finalNode.transform);
                    domeOuter.transform.localScale = Vector3.one * 21f;  // Half of 42f sphere scale for visual parity
                    domeOuter.transform.localPosition = Vector3.zero;
                    Debug.Log("[Moon13] Outer dome spawned using Rock_Large_01 prefab");
                }
                else
                {
                    Debug.LogWarning("[Moon13] Rock_Large_01 prefab not found - creating fallback outer dome");
                    domeOuter = new GameObject("DomeOuter_FALLBACK");
                    domeOuter.transform.SetParent(_finalNode.transform);
                    domeOuter.transform.localScale = Vector3.one * 42f;
                    domeOuter.transform.localPosition = Vector3.zero;
                }

                // Mid-layer chamber - medium rock
                GameObject midRockPrefab = Resources.Load<GameObject>("Prefabs/Props/KayKit/Rock_Medium_01");
                GameObject chamberMid;
                if (midRockPrefab != null)
                {
                    chamberMid = Instantiate(midRockPrefab);
                    chamberMid.name = "ChamberMid";
                    chamberMid.transform.SetParent(_finalNode.transform);
                    chamberMid.transform.localScale = Vector3.one * 14f;  // Half of 28f sphere scale
                    chamberMid.transform.localPosition = Vector3.zero;
                    Debug.Log("[Moon13] Mid chamber spawned using Rock_Medium_01 prefab");
                }
                else
                {
                    Debug.LogWarning("[Moon13] Rock_Medium_01 prefab not found - creating fallback mid chamber");
                    chamberMid = new GameObject("ChamberMid_FALLBACK");
                    chamberMid.transform.SetParent(_finalNode.transform);
                    chamberMid.transform.localScale = Vector3.one * 28f;
                    chamberMid.transform.localPosition = Vector3.zero;
                }

                // Inner sanctum - small rock
                GameObject innerRockPrefab = Resources.Load<GameObject>("Prefabs/Props/KayKit/Rock_Small_01");
                GameObject chamberInner;
                if (innerRockPrefab != null)
                {
                    chamberInner = Instantiate(innerRockPrefab);
                    chamberInner.name = "ChamberInner";
                    chamberInner.transform.SetParent(_finalNode.transform);
                    chamberInner.transform.localScale = Vector3.one * 9f;  // Half of 18f sphere scale
                    chamberInner.transform.localPosition = Vector3.zero;
                    Debug.Log("[Moon13] Inner sanctum spawned using Rock_Small_01 prefab");
                }
                else
                {
                    Debug.LogWarning("[Moon13] Rock_Small_01 prefab not found - creating fallback inner chamber");
                    chamberInner = new GameObject("ChamberInner_FALLBACK");
                    chamberInner.transform.SetParent(_finalNode.transform);
                    chamberInner.transform.localScale = Vector3.one * 18f;
                    chamberInner.transform.localPosition = Vector3.zero;
                }

                // Core crystal - ParticleSystem energy orb (no primitive)
                var crystalObj = new GameObject("CoreCrystalEnergy");
                crystalObj.transform.SetParent(_finalNode.transform);
                crystalObj.transform.localPosition = Vector3.zero;
                
                var crystalPS = crystalObj.AddComponent<ParticleSystem>();
                var crystalMain = crystalPS.main;
                crystalMain.startColor = new ParticleSystem.MinMaxGradient(new Color(0.9f, 0.7f, 1f, 1f));  // Violet cosmic energy
                crystalMain.startSize = 0.8f;
                crystalMain.startLifetime = 3.0f;
                crystalMain.startSpeed = 0.3f;
                crystalMain.loop = true;
                crystalMain.maxParticles = 200;
                
                var crystalShape = crystalPS.shape;
                crystalShape.shapeType = ParticleSystemShapeType.Sphere;
                crystalShape.radius = 4f;  // 8f sphere scale → 4f radius
                
                var crystalEmission = crystalPS.emission;
                crystalEmission.rateOverTime = 80;
                
                var crystalRenderer = crystalPS.GetComponent<ParticleSystemRenderer>();
                if (crystalRenderer != null)
                {
                    crystalRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                    crystalRenderer.material.SetColor("_BaseColor", new Color(0.9f, 0.7f, 1f, 1f));
                    crystalRenderer.material.SetColor("_EmissionColor", new Color(0.9f, 0.7f, 1f, 1f) * 3f);
                }
                Debug.Log("[Moon13] Core crystal spawned as ParticleSystem energy orb");

                // Crystal spire (vertical pillar) - use Props pillar
                GameObject spirePrefab = Resources.Load<GameObject>("Prefabs/Props/KayKit/Pillar_Stone_Large");
                GameObject spire;
                if (spirePrefab != null)
                {
                    spire = Instantiate(spirePrefab);
                    spire.name = "CrystalSpire";
                    spire.transform.SetParent(_finalNode.transform);
                    spire.transform.localScale = new Vector3(1f, 10f, 1f);  // Tall vertical element
                    spire.transform.localPosition = Vector3.zero;
                    Debug.Log("[Moon13] Crystal spire spawned using Pillar_Stone_Large prefab");
                }
                else
                {
                    Debug.LogWarning("[Moon13] Pillar_Stone_Large prefab not found - creating fallback spire");
                    spire = new GameObject("CrystalSpire_FALLBACK");
                    spire.transform.SetParent(_finalNode.transform);
                    spire.transform.localScale = new Vector3(2f, 20f, 2f);
                    spire.transform.localPosition = Vector3.zero;
                }

                // Activation console (crate as terminal)
                GameObject consolePrefab = Resources.Load<GameObject>("Prefabs/Props/KayKit/Wood_Crate_Medium");
                GameObject console;
                if (consolePrefab != null)
                {
                    console = Instantiate(consolePrefab);
                    console.name = "ActivationConsole";
                    console.transform.SetParent(_finalNode.transform);
                    console.transform.localPosition = new Vector3(0f, -15f, 0f);
                    console.transform.localScale = new Vector3(2.5f, 1f, 2.5f);
                    Debug.Log("[Moon13] Console spawned using Wood_Crate_Medium prefab");
                }
                else
                {
                    Debug.LogWarning("[Moon13] Wood_Crate_Medium prefab not found - creating fallback console");
                    console = new GameObject("ActivationConsole_FALLBACK");
                    console.transform.SetParent(_finalNode.transform);
                    console.transform.localPosition = new Vector3(0f, -15f, 0f);
                    console.transform.localScale = new Vector3(5f, 2f, 5f);
                }

                var interactable = console.AddComponent<FinalNodeConsole>();
                interactable.spawner = this;
            }

            Debug.Log("[Moon 13] Final node spawned — 5% remaining to 100% grid completion");
        }

        void SpawnEchoRealmGates()
        {
            if (echoRealmGatePoints == null || echoRealmGatePoints.Length < 3)
            {
                Debug.LogWarning("[Moon 13] Not enough echo realm gate points (need 3)");
                return;
            }

            // Gate 1: Golden Age realm
            var gate1 = CreateEchoGate(echoRealmGatePoints[0], "EchoGate_GoldenAge", Color.yellow);
            var interactable1 = gate1.AddComponent<EchoRealmGate>();
            interactable1.spawner = this;
            interactable1.realmType = EchoRealmGate.RealmType.GoldenAge;
            _echoRealms.Add(gate1);

            // Gate 2: Dissonant timeline realm
            var gate2 = CreateEchoGate(echoRealmGatePoints[1], "EchoGate_Dissonant", Color.black);
            var interactable2 = gate2.AddComponent<EchoRealmGate>();
            interactable2.spawner = this;
            interactable2.realmType = EchoRealmGate.RealmType.Dissonant;
            _echoRealms.Add(gate2);

            // Gate 3: Flood moment realm
            var gate3 = CreateEchoGate(echoRealmGatePoints[2], "EchoGate_FloodMoment", Color.red);
            var interactable3 = gate3.AddComponent<EchoRealmGate>();
            interactable3.spawner = this;
            interactable3.realmType = EchoRealmGate.RealmType.FloodMoment;
            _echoRealms.Add(gate3);

            Debug.Log("[Moon 13] 3 Echo realm gates spawned — parallel timelines accessible");
        }

        GameObject CreateEchoGate(Vector3 position, string name, Color color)
        {
            var gate = new GameObject(name);
            gate.transform.position = position;

            // Multi-part portal structure
            // Outer ring (frame) - use Props/Dungeon/Stone_Floor_Tile as circular base
            GameObject ringPrefab = Resources.Load<GameObject>("Prefabs/Props/KayKit/Stone_Floor_Tile");
            GameObject outerRing;
            if (ringPrefab != null)
            {
                outerRing = Instantiate(ringPrefab);
                outerRing.name = "OuterRing";
                outerRing.transform.SetParent(gate.transform);
                outerRing.transform.localScale = new Vector3(3f, 0.15f, 3f);
                outerRing.transform.localPosition = Vector3.zero;
                outerRing.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                Debug.Log($"[Moon13] Echo gate ring spawned using Stone_Floor_Tile prefab");
            }
            else
            {
                Debug.LogWarning("[Moon13] Stone_Floor_Tile prefab not found - creating fallback ring");
                outerRing = new GameObject("OuterRing_FALLBACK");
                outerRing.transform.SetParent(gate.transform);
                outerRing.transform.localPosition = Vector3.zero;
            }

            // Inner portal energy field - ParticleSystem instead of primitive
            var portalObj = new GameObject("PortalEnergyField");
            portalObj.transform.SetParent(gate.transform);
            portalObj.transform.localPosition = Vector3.zero;
            
            var ps = portalObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color);
            main.startSize = 0.5f;
            main.startLifetime = 2.0f;
            main.startSpeed = 0.5f;
            main.loop = true;
            main.maxParticles = 150;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 4.5f;
            shape.radiusThickness = 0.8f;
            
            var emission = ps.emission;
            emission.rateOverTime = 60;
            
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                renderer.material.SetColor("_BaseColor", color);
                renderer.material.SetColor("_EmissionColor", color * 2f);
            }

            // Support pillars (4 cardinal points) - use Props pillars
            GameObject pillarPrefab = Resources.Load<GameObject>("Prefabs/Props/KayKit/Pillar_Stone_Small");
            for (int p = 0; p < 4; p++)
            {
                float angle = p * 90f * Mathf.Deg2Rad;
                GameObject pillar;
                
                if (pillarPrefab != null)
                {
                    pillar = Instantiate(pillarPrefab);
                    pillar.name = $"Pillar_{p}";
                    pillar.transform.SetParent(gate.transform);
                    pillar.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * 5f,
                        -4f,
                        Mathf.Sin(angle) * 5f
                    );
                    pillar.transform.localScale = new Vector3(1f, 2f, 1f);
                }
                else
                {
                    Debug.LogWarning($"[Moon13] Pillar_Stone_Small prefab not found - creating fallback pillar {p}");
                    pillar = new GameObject($"Pillar_{p}_FALLBACK");
                    pillar.transform.SetParent(gate.transform);
                    pillar.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * 5f,
                        -4f,
                        Mathf.Sin(angle) * 5f
                    );
                }
            }

            return gate;
        }

        void SpawnCompanionFarewellSystem()
        {
            var farewellObj = new GameObject("CompanionFarewellSystem");
            farewellObj.transform.position = finalNodePoint + Vector3.up * 100f; // Viewing platform
            farewellObj.transform.SetParent(transform);

            _farewellSystem = farewellObj.AddComponent<CompanionFarewellSystem>();

            Debug.Log("[Moon 13] Companion farewell system ready — will activate after Zereth confrontation");
        }

        public void VisitEchoRealm(EchoRealmGate.RealmType realm)
        {
            switch (realm)
            {
                case EchoRealmGate.RealmType.GoldenAge:
                    if (!_goldenAgeRealmVisited)
                    {
                        _goldenAgeRealmVisited = true;
                        Debug.Log("[Moon 13] Golden Age realm visited — empire at full glory witnessed");
                        DialogueManager.Instance?.PlayContextDialogue("echo_realm_golden_age");
                        QuestManager.Instance?.ProgressObjective("moon13_echo_realms", 0, 1);
                    }
                    break;

                case EchoRealmGate.RealmType.Dissonant:
                    if (!_dissonantRealmVisited)
                    {
                        _dissonantRealmVisited = true;
                        Debug.Log("[Moon 13] Dissonant timeline visited — what if Zereth won");
                        DialogueManager.Instance?.PlayContextDialogue("echo_realm_dissonant");
                        QuestManager.Instance?.ProgressObjective("moon13_echo_realms", 0, 1);
                    }
                    break;

                case EchoRealmGate.RealmType.FloodMoment:
                    if (!_floodMomentRealmVisited)
                    {
                        _floodMomentRealmVisited = true;
                        Debug.Log("[Moon 13] Flood moment witnessed — trigger room, 3 figures revealed");
                        Debug.Log("  - Zereth (giant) experimenting with 9-band transcendence");
                        Debug.Log("  - 2 Parasite Cabal humans infiltrated, reversed polarity");
                        Debug.Log("  - Zereth was victim, not villain — first casualty of his own stolen tech");
                        DialogueManager.Instance?.PlayContextDialogue("echo_realm_flood_moment");
                        QuestManager.Instance?.ProgressObjective("moon13_echo_realms", 0, 1);
                    }
                    break;
            }

            // Check if all realms visited → unlock Zereth confrontation
            if (AllRealmsVisited && !_zerethConfrontationComplete)
            {
                TriggerZerethConfrontation();
            }
        }

        void TriggerZerethConfrontation()
        {
            Debug.Log("[Moon 13] All Echo realms explored — Zereth's corrupted echo manifests");

            // Spawn Zereth echo at final node
            Vector3 spawnPos = finalNodePoint + Vector3.up * 10f;
            if (zerethEchoPrefab != null)
            {
                _zerethEcho = Instantiate(zerethEchoPrefab, spawnPos, Quaternion.identity);
                _zerethEcho.name = "ZerethEcho_Tormented";
            }
            else
            {
                // Fallback: KayKit Barbarian scaled to giant size for final boss
                GameObject zerethPrefab = Resources.Load<GameObject>("Prefabs/Characters/KayKit/Char_Barbarian");
                if (zerethPrefab != null)
                {
                    _zerethEcho = Instantiate(zerethPrefab, spawnPos, Quaternion.identity);
                    _zerethEcho.name = "ZerethEcho_Tormented";
                    _zerethEcho.transform.localScale = new Vector3(8f, 8f, 8f); // Giant final boss scale
                }
                else
                {
                    Debug.LogError("[Moon13ContentSpawner] CRITICAL: Char_Barbarian prefab missing for Zereth Echo");
                    _zerethEcho = new GameObject("ZerethEcho_Tormented_MISSING_PREFAB");
                    _zerethEcho.transform.position = spawnPos;
                }
            }

            // Zereth dialogue (breaking, agonized)
            DialogueManager.Instance?.PlayContextDialogue("zereth_wanted_more");

            // Resonance dialogue combat (not physical — harmonic sequences vs dissonance)
            // Note: Resonance combat system (frequency tuning vs. corruption entities)
            Debug.Log("[Moon 13] Resonance dialogue initiated — match pain with harmony");

            // Quest update
            QuestManager.Instance?.ActivateQuest("moon13_zereth_resonance_dialogue");

            // Lirael joins (fully solid now)
            DialogueManager.Instance?.PlayContextDialogue("lirael_we_hear_you_now");
        }

        public void CompleteZerethConfrontation()
        {
            if (_zerethConfrontationComplete) return;

            _zerethConfrontationComplete = true;

            Debug.Log("[Moon 13] Zereth resonance dialogue complete — echo calmed, ready for final choice");

            // Zereth echo visual change: purple → golden
            if (_zerethEcho != null)
            {
                var body = _zerethEcho.transform.Find("Body");
                if (body != null)
                {
                    var renderer = body.GetComponent<Renderer>();
                    if (renderer != null)

            // Trigger companion farewells before final choice
            TriggerCompanionFarewells();
        }

        void TriggerCompanionFarewells()
        {
            if (_farewellsComplete || _farewellSystem == null) return;

            Debug.Log("[Moon 13] Triggering companion farewell sequence — emotional payoff before final choice");

            // Dialogue: transition to farewells
            DialogueManager.Instance?.PlayContextDialogue("moon13_time_for_farewells");

            GameEvents.RaiseHUDShowBanner(
                "A Moment of Peace",
                "Your companions wish to speak with you before the final choice.",
                5f
            );

            // Start farewell sequence
            _farewellSystem.BeginFarewells();

            // Wait for farewells to complete before allowing final node activation
            StartCoroutine(WaitForFarewells());
        }

        System.Collections.IEnumerator WaitForFarewells()
        {
            // Poll until farewells complete (~2 minutes)
            while (_farewellSystem != null && !_farewellSystem.AllFarewellsComplete)
            {
                yield return new UnityEngine.WaitForSeconds(1f);
            }

            _farewellsComplete = true;
            Debug.Log("[Moon 13] Companion farewells complete — player ready for final choice");

            GameEvents.RaiseHUDShowBanner(
                "The Final Node Awaits",
                "All companions have spoken. The choice is yours."
            );
                    {
                        var rend = GetComponent<Renderer>();
                        if (rend != null) rend.material.color = new Color(1f, 0.9f, 0.5f, 0.8f);  // Golden peace
                    }
                }
            }

            // Quest complete
            QuestManager.Instance?.CompleteQuest("moon13_zereth_resonance_complete");

            // Final node activation now available
            Debug.Log("[Moon 13] Final node ready for activation during 13th Moon, 17th Hour alignment");
        }

        public void ActivateFinalNode(EndingPath path)
        {
            if (finalNodeActivated) return;
            if (!_zerethConfrontationComplete)
            {
                Debug.LogWarning("[Moon 13] Cannot activate final node — complete Zereth confrontation first");
                return;
            }

            finalNodeActivated = true;
            chosenPath = path;

            Debug.Log($"[Moon 13] Final node activated — {path} Path chosen");

            // All companions present, ley lines lit, bells ringing, organs thundering
            Debug.Log("[Moon 13] ALL companions present. Every ley line lit. Every bell ringing. Every fountain spraying.");

            // The Choice outcomes
            switch (path)
            {
                case EndingPath.Harmony:
                    ExecuteHarmonyEnding();
                    break;

                case EndingPath.Echo:
                    ExecuteEchoEnding();
                    break;

                case EndingPath.Reset:
                    ExecuteResetEnding();
                    break;
            }

            // Global grid 100%
            SaveManager.Instance?.SetMoonProgress(13, 100f);

            // Quest complete
            QuestManager.Instance?.CompleteQuest("moon13_cosmic_alignment_complete");
        }

        void ExecuteHarmonyEnding()
        {
            Debug.Log("[Moon 13] HARMONY PATH — Forgive Zereth, merge timelines, Golden Age restored");

            // Mud Flood reverses in real time
            Debug.Log("[Moon 13] Mud recedes globally. Sunken windows rise. Buildings emerge in full glory.");

            // VFX: global golden wave
            var wave = new GameObject("HarmonyWave_Global");
            wave.transform.position = finalNodePoint;
            var particles = wave.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(1f, 0.9f, 0.4f, 1f);  // Golden
            main.startSize = 500f;
            main.startLifetime = 30f;
            main.maxParticles = 100000;

            // Giants walk again
            Debug.Log("[Moon 13] Giants walk among humans again. Airships fill sky. Bells ring perpetual harmony.");

            // Final scene dialogue
            DialogueManager.Instance?.PlayContextDialogue("lirael_lullaby_finale");
            DialogueManager.Instance?.PlayContextDialogue("milo_remembering_more");
            DialogueManager.Instance?.PlayContextDialogue("thorne_skys_ours_again");
            DialogueManager.Instance?.PlayContextDialogue("korath_song_resumes");
            DialogueManager.Instance?.PlayContextDialogue("zereth_at_last");

            // Achievement
            AchievementSystem.Instance?.Unlock("harmony_ending_golden_age");

            // Complete ending quest to trigger end card
            QuestManager.Instance?.CompleteQuest(EndCardController.HarmonyEndingQuestId);

            Debug.Log("[Moon 13] The Aether never left. It was waiting for someone to listen.");
        }

        void ExecuteEchoEnding()
        {
            Debug.Log("[Moon 13] ECHO PATH — Parallel timelines preserved, reality switching enabled");

            // Both timelines as parallel layers
            Debug.Log("[Moon 13] Players can switch between Golden Age and post-Flood realities in post-game");

            // Zereth finds peace in between-space
            Debug.Log("[Moon 13] Zereth becomes guardian of the threshold — neither past nor present, but eternal now");

            // VFX: shimmering aurora between worlds
            var aurora = new GameObject("EchoAurora_Threshold");
            aurora.transform.position = finalNodePoint + Vector3.up * 100f;
            var particles = aurora.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(0.6f, 0.8f, 1f, 0.7f);  // Aurora blue
            main.startSize = 300f;
            main.startLifetime = 60f;
            main.maxParticles = 50000;

            // Dialogue
            DialogueManager.Instance?.PlayContextDialogue("echo_ending_threshold");

            // Achievement
            AchievementSystem.Instance?.Unlock("echo_ending_parallel_worlds");

            // Complete ending quest to trigger end card
            QuestManager.Instance?.CompleteQuest(EndCardController.EchoEndingQuestId);

            Debug.Log("[Moon 13] Two worlds, one heart. Walk between them freely.");
        }

        void ExecuteResetEnding()
        {
            Debug.Log("[Moon 13] RESET PATH — Controlled grid distribution, bittersweet power");

            // Grid active but distribution controlled
            Debug.Log("[Moon 13] Immense power, but the wonder dims. Sky never fully clears.");

            // VFX: muted golden light (not full brilliance)
            var light = new GameObject("ResetLight_Controlled");
            light.transform.position = finalNodePoint + Vector3.up * 50f;
            var particles = light.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = new Color(0.7f, 0.7f, 0.5f, 0.6f);  // Muted gold
            main.startSize = 200f;
            main.startLifetime = 20f;
            main.maxParticles = 20000;

            // Dialogue: companions conflicted
            DialogueManager.Instance?.PlayContextDialogue("reset_ending_control");

            // Achievement
            AchievementSystem.Instance?.Unlock("reset_ending_controlled_power");

            // Complete ending quest to trigger end card
            QuestManager.Instance?.CompleteQuest(EndCardController.ResetEndingQuestId);

            Debug.Log("[Moon 13] Power without freedom. Safety without song.");
        }

        /// <summary>
        /// Callback when player makes final ending choice
        /// </summary>
        public void OnFinalChoiceMade(int choiceIndex)
        {
            Debug.Log($"[Moon13] Player selected ending choice: {choiceIndex}");

            EndingPath path = choiceIndex switch
            {
                0 => EndingPath.Harmony,
                1 => EndingPath.Echo,
                2 => EndingPath.Reset,
                _ => EndingPath.Harmony
            };

            ActivateFinalNode(path);
        }

        /// <summary>
        /// Final Node Console interactable — choose ending path
        /// </summary>
        public class FinalNodeConsole : MonoBehaviour, IInteractable
        {
            public Moon13ContentSpawner spawner;

            public string GetInteractPrompt()
            {
                // Check if farewells are complete before allowing final choice
                if (!spawner._farewellsComplete)
                {
                    Debug.Log("[FinalNodeConsole] Farewells not complete — wait for companions to speak");
                    GameEvents.RaiseHUDShowObjective("Complete farewell conversations first");
                    return "Complete farewell conversations first";
                }

                if (spawner == null) return "";
                if (spawner.finalNodeActivated) return "Final node already activated";
                if (!spawner._zerethConfrontationComplete)
                    return "Complete Zereth confrontation first";
                return "Hold [E] to Activate Final Node — Choose Ending Path";
            }

            public void Interact(GameObject interactor)
            {
                if (spawner == null || spawner.finalNodeActivated) return;
                if (!spawner._zerethConfrontationComplete) return;

                Debug.Log("[FinalNodeConsole] Presenting final ending choice...");

                // Present final choice UI to player (Harmony/Echo/Reset endings)
                string title = "The 13th Moon Rises";
                string description = "Choose the fate of Tartaria and all who dwell within it.\nThis choice cannot be undone.";
                string[] choices = new string[]
                {
                    "HARMONY - Forgive Zereth, restore the Golden Age",
                    "ECHO - Preserve both timelines, walk between worlds",
                    "RESET - Control the grid, maintain order and power"
                };

                ChoiceDialogUI.Instance?.ShowChoices(choices, spawner.OnFinalChoiceMade, title, description);
            }
        }

        /// <summary>
        /// Echo Realm Gate interactable — visit parallel timeline instances
        /// </summary>
        public class EchoRealmGate : MonoBehaviour, IInteractable
        {
            public Moon13ContentSpawner spawner;
            public RealmType realmType;

            public enum RealmType
            {
                GoldenAge,      // Empire at full glory
                Dissonant,      // What if Zereth won
                FloodMoment     // Trigger room truth
            }

            public string GetInteractPrompt()
            {
                string realmName = realmType switch
                {
                    RealmType.GoldenAge => "Golden Age Realm",
                    RealmType.Dissonant => "Dissonant Timeline Realm",
                    RealmType.FloodMoment => "Flood Moment Realm",
                    _ => "Echo Realm"
                };

                return $"Hold [E] to Enter {realmName}";
            }

            public void Interact(GameObject interactor)
            {
                if (spawner == null) return;

                Debug.Log($"[EchoRealmGate] Entering {realmType} realm — timeline shimmer active");

                // Visit realm via spawner
                spawner.VisitEchoRealm(realmType);

                // Load echo realm scene (additive scene load or zone transition)
                Debug.Log("[Moon13] Loading Echo Realm zone...");
                // For now, just log and play dialogue
            }
        }
    }
}
