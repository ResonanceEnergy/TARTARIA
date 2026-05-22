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
                // Fallback: create final node visual
                _finalNode = new GameObject("FinalNode_13thMoon");
                _finalNode.transform.position = finalNodePoint;

                // Node chamber (massive sphere)
                var chamber = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                chamber.name = "NodeChamber";
                chamber.transform.SetParent(_finalNode.transform);
                chamber.transform.localScale = Vector3.one * 40f;
                chamber.transform.localPosition = Vector3.zero;

                // Core crystal (pulsing)
                var crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crystal.name = "CoreCrystal";
                crystal.transform.SetParent(_finalNode.transform);
                crystal.transform.localScale = Vector3.one * 15f;
                crystal.transform.localPosition = Vector3.zero;
                var renderer = crystal.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.9f, 0.7f, 1f, 1f);  // Violet cosmic energy
                }

                // Activation console
                var console = GameObject.CreatePrimitive(PrimitiveType.Cube);
                console.name = "ActivationConsole";
                console.transform.SetParent(_finalNode.transform);
                console.transform.localPosition = new Vector3(0f, -15f, 0f);
                console.transform.localScale = new Vector3(5f, 2f, 5f);

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

            // Portal visual (glowing ring)
            var portal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            portal.name = "Portal";
            portal.transform.SetParent(gate.transform);
            portal.transform.localScale = new Vector3(5f, 0.5f, 5f);
            portal.transform.localPosition = Vector3.zero;
            var renderer = portal.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            return gate;
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
                // Fallback: giant humanoid shape
                _zerethEcho = new GameObject("ZerethEcho_Tormented");
                _zerethEcho.transform.position = spawnPos;

                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body";
                body.transform.SetParent(_zerethEcho.transform);
                body.transform.localScale = new Vector3(5f, 10f, 5f);  // Giant scale
                body.transform.localPosition = Vector3.zero;
                var renderer = body.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.3f, 0.1f, 0.4f, 0.8f);  // Dark purple torment
                }
            }

            // Zereth dialogue (breaking, agonized)
            DialogueManager.Instance?.PlayContextDialogue("zereth_wanted_more");

            // Resonance dialogue combat (not physical — harmonic sequences vs dissonance)
            // TODO: Resonance combat system not fully implemented
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
                    {
                        renderer.material.color = new Color(1f, 0.9f, 0.5f, 0.8f);  // Golden peace
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

            Debug.Log("[Moon 13] Power without freedom. Safety without song.");
        }

        /// <summary>
        /// Final Node Console interactable — choose ending path
        /// </summary>
        public class FinalNodeConsole : MonoBehaviour, IInteractable
        {
            public Moon13ContentSpawner spawner;

            public string GetInteractPrompt()
            {
                if (spawner == null) return "";
                if (spawner.finalNodeActivated) return "";
                if (!spawner._zerethConfrontationComplete)
                    return "Complete Zereth confrontation first";
                return "Hold [E] to Activate Final Node — Choose Ending Path";
            }

            public void Interact(GameObject interactor)
            {
                if (spawner == null || spawner.finalNodeActivated) return;
                if (!spawner._zerethConfrontationComplete) return;

                Debug.Log("[FinalNodeConsole] Ending choice presented:");
                Debug.Log("  1. HARMONY: Forgive Zereth, merge timelines, Golden Age restored");
                Debug.Log("  2. ECHO: Preserve parallel timelines, switch between realities");
                Debug.Log("  3. RESET: Control grid distribution, maintain power structures");

                // TODO: Present choice UI to player
                // For now, default to Harmony (canon)
                spawner.ActivateFinalNode(EndingPath.Harmony);
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

                // TODO: Load echo realm scene/zone
                // For now, just log and play dialogue
            }
        }
    }
}
