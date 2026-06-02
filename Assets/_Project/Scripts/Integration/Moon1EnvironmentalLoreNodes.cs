// Moon1EnvironmentalLoreNodes.cs
// Owner lane: NARRATIVE DESIGNER (environmental).
// Purpose: bootstraps 8 environmental story-beat readers throughout Echohaven.
// Each readable via Interact key (E / GP:A) when player is within proximity, or
// auto-discovered on close proximity entry. Discoveries fan out to the Lorebook UI
// (LorebookPanel owned by sibling UI lane) via reflection so this file does not
// require a hard reference. If LorebookPanel is absent at runtime, falls back to
// GameEvents.RaiseHUDShowBanner with a loud warning per CLAUDE.md no-debt rule 4.
//
// Conforms to API_CONTRACT.md:
//  - Namespace Tartaria.Integration (not a banned suffix).
//  - GameEvents.RaiseHUDShowBanner verified at GameEvents.cs:623.
//  - No deprecated Unity 6 APIs used (no FindObjectOfType, no LightmapEditorSettings).
//  - No silent catches; no empty bodies; no TODOs.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Runtime registrar for Moon 1 environmental lore nodes (Echohaven).
    /// Singleton, auto-bootstrapped after scene load. Owns the catalog of 8
    /// scripted lore beats and the per-node proximity/interact handlers.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1EnvironmentalLoreNodes : MonoBehaviour
    {
        // -------------------------------------------------------------------
        // Catalog: 8 environmental story beats. Keep ids in sync with yarn
        // node titles under Assets/_Project/Dialogue/Lore/Moon1/Environmental/.
        // -------------------------------------------------------------------
        [Serializable]
        public class LoreEntry
        {
            public string id;             // matches GameObject name + yarn node title
            public string title;          // shown in Lorebook header
            public string body;           // shown in Lorebook body (snippet)
            public string yarnNodeName;   // for the dialogue runner to play full prose
            public Vector3 fallbackPosition; // used only if no scene GameObject is found

            public LoreEntry(string id, string title, string body, string yarn, Vector3 fallback)
            {
                this.id = id;
                this.title = title;
                this.body = body;
                this.yarnNodeName = yarn;
                this.fallbackPosition = fallback;
            }
        }

        static readonly LoreEntry[] Catalog = new LoreEntry[]
        {
            new LoreEntry(
                "lore_inn_painting",
                "Painting: Echohaven Before the Silence",
                "An amber-varnished view of pre-mud Echohaven — three white bridges, copper domes, a sky like struck flint.",
                "lore_inn_painting",
                new Vector3(-12f, 1.6f, 4f)),
            new LoreEntry(
                "lore_stardome_inscription",
                "Star Dome: Prophecy of the Doorway",
                "A Tartarian spiral inscription names the thirteenth moon, the seventeenth hour, and a doorway that listens.",
                "lore_stardome_inscription",
                new Vector3(0f, 0.2f, 32f)),
            new LoreEntry(
                "lore_anastasia_rocker_journal",
                "Anastasia's Journal: First Sighting",
                "A schoolroom-neat hand recounts the day Lirael first came out of the mud, forty-two years ago.",
                "lore_anastasia_rocker_journal",
                new Vector3(-22f, 1.0f, -6f)),
            new LoreEntry(
                "lore_resetscout_badge",
                "Bureau Badge: Recent Fight",
                "A smashed brass badge of the Bureau of Continuance, bent and bloodied, lying at the lip of the mud pool.",
                "lore_resetscout_badge",
                new Vector3(18f, 0.3f, -14f)),
            new LoreEntry(
                "lore_milo_child_drawing",
                "Milo's Wall: The Sun Comes Back",
                "A child's drawing of Anastasia, Lirael, and a thirteen-wedged sun, tacked to Milo's lean-to with a rusted nail.",
                "lore_milo_child_drawing",
                new Vector3(-30f, 2.0f, 12f)),
            new LoreEntry(
                "lore_cathedral_tallymarks",
                "Cathedral Basement: Hour Count",
                "Sixteen full clusters of tally marks. The seventeenth is incomplete. A candle waits, wick fresh.",
                "lore_cathedral_tallymarks",
                new Vector3(2f, -3.0f, 18f)),
            new LoreEntry(
                "lore_skeleton_key_fragments",
                "Skeleton Key: One Moon, One Piece",
                "Thirteen sheared segments of a colossal key, each glyphed with a moon phase. Only Moon One fits cleanly.",
                "lore_skeleton_key_fragments",
                new Vector3(0f, 0.5f, 22f)),
            new LoreEntry(
                "lore_pipe_organ_glyph",
                "Pipe Organ Glyph: Resonance Instruction",
                "Gilded Tartarian script: Earth at 7, heart at 432, crown at 528. The third phrase is sooted out save 'doorway.'",
                "lore_pipe_organ_glyph",
                new Vector3(6f, 4.0f, 16f)),
        };

        // -------------------------------------------------------------------
        // Singleton bootstrap
        // -------------------------------------------------------------------
        public static Moon1EnvironmentalLoreNodes Instance { get; private set; }

        const float InteractRadius = 1.5f;     // proximity auto-discovery + interact range
        const string LoreNodeTag = "LoreNode"; // scene GameObjects must share this tag
        const KeyCode InteractKey = KeyCode.E;

        // discovered ids stay discovered for the session — don't re-banner the same beat
        readonly HashSet<string> _discovered = new HashSet<string>();

        // resolved transforms (or null if missing — handled via fallback world position)
        readonly Dictionary<string, Transform> _resolvedTransforms = new Dictionary<string, Transform>();

        // cached player transform (refreshed on demand if null)
        Transform _playerT;

        // cached LorebookPanel reflection target (one-time)
        bool _lorebookProbed;
        Type _lorebookType;
        object _lorebookInstance;
        MethodInfo _lorebookAddEntry;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBootstrap()
        {
            if (Instance != null) return;
            var host = new GameObject("[Moon1EnvironmentalLoreNodes]");
            DontDestroyOnLoad(host);
            Instance = host.AddComponent<Moon1EnvironmentalLoreNodes>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Moon1EnvironmentalLoreNodes] Duplicate singleton on '{name}' — destroying duplicate.");
                Destroy(this);
                return;
            }
            Instance = this;
            ResolveSceneNodes();
        }

        // -------------------------------------------------------------------
        // Scene node resolution — find tagged GameObjects whose name matches a
        // catalog id. Missing nodes are warned-on per CLAUDE.md rule 4, never
        // thrown. The Anastasia tag pattern (UnityException catch) applies here
        // because the "LoreNode" tag may not be defined in TagManager yet.
        // -------------------------------------------------------------------
        void ResolveSceneNodes()
        {
            GameObject[] hits;
            try
            {
                hits = GameObject.FindGameObjectsWithTag(LoreNodeTag);
            }
            catch (UnityException ex)
            {
                Debug.LogWarning(
                    $"[Moon1EnvironmentalLoreNodes] Tag '{LoreNodeTag}' is not defined in TagManager " +
                    $"(scene '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}'). " +
                    $"All 8 environmental lore nodes will use catalog fallback positions instead. " +
                    $"Add the tag via Project Settings -> Tags and Layers. ({ex.Message})");
                return;
            }

            // Build a name -> transform lookup of every tagged hit.
            var byName = new Dictionary<string, Transform>(StringComparer.Ordinal);
            foreach (var go in hits)
            {
                if (go == null) continue;
                byName[go.name] = go.transform;
            }

            int matched = 0;
            foreach (var entry in Catalog)
            {
                if (byName.TryGetValue(entry.id, out var t))
                {
                    _resolvedTransforms[entry.id] = t;
                    matched++;
                }
                else
                {
                    Debug.LogWarning(
                        $"[Moon1EnvironmentalLoreNodes] Lore node '{entry.id}' not present in scene " +
                        $"'{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}' " +
                        $"(tag '{LoreNodeTag}' expected; using fallback world pos {entry.fallbackPosition}).");
                }
            }

            Debug.Log($"[Moon1EnvironmentalLoreNodes] Resolved {matched}/{Catalog.Length} environmental lore nodes from scene tags.");
        }

        // -------------------------------------------------------------------
        // Per-frame proximity + interact polling. Cheap: at most 8 distance
        // checks per frame, only while player exists. No allocations.
        // -------------------------------------------------------------------
        void Update()
        {
            if (_playerT == null)
            {
                var pgo = GameObject.FindGameObjectWithTag("Player");
                if (pgo != null) _playerT = pgo.transform;
                else return; // no player yet — try again next frame
            }

            bool interactPressed = false;
            // Keyboard E or gamepad south button. Use Input System where available,
            // fall back to legacy Input only if Input System Package is not active.
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame) interactPressed = true;
            var gp = UnityEngine.InputSystem.Gamepad.current;
            if (!interactPressed && gp != null && gp.buttonSouth.wasPressedThisFrame) interactPressed = true;

            Vector3 pp = _playerT.position;
            foreach (var entry in Catalog)
            {
                if (_discovered.Contains(entry.id)) continue;

                Vector3 nodePos;
                if (_resolvedTransforms.TryGetValue(entry.id, out var t) && t != null)
                    nodePos = t.position;
                else
                    nodePos = entry.fallbackPosition;

                float d = Vector3.Distance(pp, nodePos);
                if (d > InteractRadius) continue;

                // Within range: auto-discover on proximity OR explicit interact key.
                // Auto-discover makes these readable even if input is contested.
                if (interactPressed || d <= InteractRadius * 0.66f)
                {
                    DiscoverEntry(entry);
                }
            }
        }

        // -------------------------------------------------------------------
        // Discovery dispatch — adds to session set, calls LorebookPanel if
        // present (via reflection so we don't take a hard UI reference), else
        // falls back to a HUD banner per CLAUDE.md rule 4.
        // -------------------------------------------------------------------
        void DiscoverEntry(LoreEntry entry)
        {
            if (!_discovered.Add(entry.id)) return;

            float t = UnityEngine.Time.time;
            Debug.Log($"[Moon1EnvironmentalLoreNodes] Discovered lore beat '{entry.id}' (yarn='{entry.yarnNodeName}') at t={t:F2}.");

            bool routed = TryRouteToLorebook(entry, t);
            if (!routed)
            {
                // Fallback path. Loud warning so we know the UI didn't pick up.
                Debug.LogWarning(
                    $"[Moon1EnvironmentalLoreNodes] LorebookPanel not resolvable via reflection for id '{entry.id}'. " +
                    $"Falling back to GameEvents.RaiseHUDShowBanner. Sibling UI lane should ship Tartaria.UI.LorebookPanel " +
                    $"with a static Instance and AddEntry(id,title,body,time) signature.");
                GameEvents.RaiseHUDShowBanner(entry.title, entry.body, 6f);
            }
        }

        // -------------------------------------------------------------------
        // Reflection-based bridge into LorebookPanel.
        //
        // Sibling UI lane is expected to expose:
        //   public class LorebookPanel : MonoBehaviour {
        //       public static LorebookPanel Instance;
        //       public void AddEntry(string id, string title, string body, float discoveredAt);
        //   }
        //
        // We probe candidate namespaces, cache the MethodInfo on first hit, and
        // invoke. Anything missing causes the caller to fall back to a banner.
        // -------------------------------------------------------------------
        bool TryRouteToLorebook(LoreEntry entry, float discoveredAt)
        {
            if (!_lorebookProbed)
            {
                _lorebookProbed = true;
                _lorebookType = ResolveLorebookType();
                if (_lorebookType != null)
                {
                    // Method lookup: AddEntry(string,string,string,float) preferred,
                    // (string,string,string) acceptable.
                    _lorebookAddEntry = _lorebookType.GetMethod(
                        "AddEntry",
                        BindingFlags.Public | BindingFlags.Instance,
                        binder: null,
                        types: new[] { typeof(string), typeof(string), typeof(string), typeof(float) },
                        modifiers: null);
                    if (_lorebookAddEntry == null)
                    {
                        _lorebookAddEntry = _lorebookType.GetMethod(
                            "AddEntry",
                            BindingFlags.Public | BindingFlags.Instance,
                            binder: null,
                            types: new[] { typeof(string), typeof(string), typeof(string) },
                            modifiers: null);
                    }

                    var instProp = _lorebookType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    if (instProp != null) _lorebookInstance = instProp.GetValue(null);
                    if (_lorebookInstance == null)
                    {
                        var instField = _lorebookType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                        if (instField != null) _lorebookInstance = instField.GetValue(null);
                    }
                }
            }

            // Instance may have been spawned after our first probe — re-fetch each call if needed.
            if (_lorebookInstance == null && _lorebookType != null)
            {
                var instProp = _lorebookType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instProp != null) _lorebookInstance = instProp.GetValue(null);
                if (_lorebookInstance == null)
                {
                    var instField = _lorebookType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                    if (instField != null) _lorebookInstance = instField.GetValue(null);
                }
            }

            if (_lorebookType == null || _lorebookAddEntry == null || _lorebookInstance == null)
                return false;

            try
            {
                var pars = _lorebookAddEntry.GetParameters();
                object[] args = pars.Length == 4
                    ? new object[] { entry.id, entry.title, entry.body, discoveredAt }
                    : new object[] { entry.id, entry.title, entry.body };
                _lorebookAddEntry.Invoke(_lorebookInstance, args);
                return true;
            }
            catch (TargetInvocationException tie)
            {
                Debug.LogError(
                    $"[Moon1EnvironmentalLoreNodes] LorebookPanel.AddEntry threw for id '{entry.id}': " +
                    $"{tie.InnerException?.GetType().Name}: {tie.InnerException?.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[Moon1EnvironmentalLoreNodes] Reflection invoke failed for id '{entry.id}' " +
                    $"({ex.GetType().Name}: {ex.Message}). Falling back to banner.");
                return false;
            }
        }

        static Type ResolveLorebookType()
        {
            // Probe common namespaces the sibling lane might land it in.
            string[] candidates =
            {
                "Tartaria.UI.LorebookPanel",
                "Tartaria.Integration.LorebookPanel",
                "Tartaria.Core.LorebookPanel",
                "LorebookPanel"
            };
            foreach (var fqn in candidates)
            {
                var t = Type.GetType(fqn);
                if (t != null) return t;
            }
            // Scan loaded assemblies as last resort.
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type found;
                try { found = asm.GetType("Tartaria.UI.LorebookPanel"); }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Moon1EnvironmentalLoreNodes] Assembly probe failed on '{asm.GetName().Name}': {ex.GetType().Name}: {ex.Message}");
                    continue;
                }
                if (found != null) return found;
            }
            return null;
        }

        // -------------------------------------------------------------------
        // External API — let other systems trigger a beat by id (e.g. a Yarn
        // command, a quest progression hook, a debug menu).
        // -------------------------------------------------------------------
        public bool TryDiscover(string id)
        {
            foreach (var entry in Catalog)
            {
                if (entry.id == id)
                {
                    DiscoverEntry(entry);
                    return true;
                }
            }
            Debug.LogWarning($"[Moon1EnvironmentalLoreNodes] TryDiscover called with unknown id '{id}'. " +
                             $"Catalog ids: {string.Join(", ", CatalogIds())}");
            return false;
        }

        public IReadOnlyCollection<string> DiscoveredIds() => _discovered;

        public static IEnumerable<string> CatalogIds()
        {
            foreach (var e in Catalog) yield return e.id;
        }
    }
}
