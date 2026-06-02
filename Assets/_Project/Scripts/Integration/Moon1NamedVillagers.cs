using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Sprint 9 Lane 8 — Named villagers (Audit v2 fix-it #2.5).
    /// Spawns 5 named, dialogue-bearing villagers in the Echohaven village to
    /// complement the 4 anonymous ambient villagers from <see cref="Moon1VillagerAmbient"/>.
    ///
    /// Each villager occupies a canonical narrative location:
    ///   - Bram the Smith        @ (15, 0, 5)    — forge / smithy
    ///   - Marisol the Weaver    @ (-12, 0, 8)   — weaver shop
    ///   - Old Tobias            @ (4, 0, -6)    — bench beside the well
    ///   - Wren the Apprentice   @ (-3, 0, 12)   — village green, near Milo
    ///   - Father Caelum         @ (0, 0, 22)    — cathedral steps (entrance plaza)
    ///
    /// Real model loads when available; URP-safe capsule fallback otherwise per CLAUDE.md.
    /// All dialogue is generic peasant-village vernacular — no Romanov framing per the
    /// political-risk callouts in CLAUDE.md.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Moon1NamedVillagers : MonoBehaviour
    {
        private const string SceneName = "Echohaven_VerticalSlice";
        private const string RootObjectName = "Moon1_NamedVillagers_Root";

        private static Moon1NamedVillagers _instance;

        /// <summary>Canonical villager record. Public to allow tooling introspection.</summary>
        public struct NamedVillagerSpec
        {
            public string Name;
            public string Role;
            public Vector3 Position;
            public Color TintFallback;     // Used only if a real prefab fails to load.
            public string IdleAnimation;   // Animator state name, optional.
            public string Greeting;        // Banner line (Yarn node is preferred if registered).
        }

        // Canonical roster for this PR. Coordinates picked so the five villagers
        // occupy distinct, distant spots across the Echohaven village footprint.
        public static readonly NamedVillagerSpec[] Roster =
        {
            new NamedVillagerSpec
            {
                Name           = "Bram the Smith",
                Role           = "Smith",
                Position       = new Vector3(15f, 0f, 5f),
                TintFallback   = new Color(0.55f, 0.32f, 0.20f), // forge-leather brown
                IdleAnimation  = "Hammer",
                Greeting       = "The bellows wake slow these days — the air's not Mended.",
            },
            new NamedVillagerSpec
            {
                Name           = "Marisol the Weaver",
                Role           = "Weaver",
                Position       = new Vector3(-12f, 0f, 8f),
                TintFallback   = new Color(0.42f, 0.30f, 0.55f), // dyed-wool violet
                IdleAnimation  = "Weave",
                Greeting       = "I'd weave longer if the light held — bring back the dome and I'll teach you the cymatic warp.",
            },
            new NamedVillagerSpec
            {
                Name           = "Old Tobias",
                Role           = "Elder",
                Position       = new Vector3(4f, 0f, -6f),
                TintFallback   = new Color(0.65f, 0.62f, 0.55f), // homespun grey
                IdleAnimation  = "SitPipe",
                Greeting       = "Heard the bells once, when I was small. They'll ring again.",
            },
            new NamedVillagerSpec
            {
                Name           = "Wren the Apprentice",
                Role           = "Apprentice",
                Position       = new Vector3(-3f, 0f, 12f),
                TintFallback   = new Color(0.85f, 0.72f, 0.38f), // sun-bright tunic
                IdleAnimation  = "KickStones",
                Greeting       = "Are you the one Milo said would come? Are you?",
            },
            new NamedVillagerSpec
            {
                Name           = "Father Caelum",
                Role           = "Priest",
                Position       = new Vector3(0f, 0f, 22f),
                TintFallback   = new Color(0.30f, 0.30f, 0.36f), // priestly slate
                IdleAnimation  = "Pray",
                Greeting       = "Pilgrim. The litanies remember. Light the braziers and they will sing.",
            },
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (active.name != SceneName)
            {
                return;
            }
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("Moon1NamedVillagers");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1NamedVillagers>();
        }

        private void Start()
        {
            var root = new GameObject(RootObjectName);
            root.transform.SetParent(transform, worldPositionStays: false);

            int spawned = 0;
            for (int i = 0; i < Roster.Length; i++)
            {
                if (SpawnVillager(root.transform, Roster[i]))
                {
                    spawned++;
                }
            }
            Debug.Log("[Moon1NamedVillagers] Spawned " + spawned + " / " + Roster.Length + " named villagers.");
        }

        private bool SpawnVillager(Transform parent, NamedVillagerSpec spec)
        {
            GameObject instance = TryLoadCharacterPrefab(spec, parent);

            if (instance == null)
            {
                instance = BuildFallbackVisual(spec, parent);
            }
            if (instance == null)
            {
                Debug.LogWarning("[Moon1NamedVillagers] Failed to create villager '" + spec.Name + "'.");
                return false;
            }

            instance.name = "NamedVillager_" + spec.Name.Replace(" ", "_");
            instance.transform.position = spec.Position;

            // Interaction trigger — 2m radius sphere per the lane spec.
            var sphere = instance.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 2f;
            sphere.center = new Vector3(0f, 1f, 0f);

            // Hook up the interaction component (it owns the prompt + dialogue raising).
            var interaction = instance.AddComponent<NamedVillagerInteraction>();
            interaction.Configure(spec.Name, spec.Greeting);

            // Best-effort idle animation hint — if the loaded prefab has an Animator
            // with the requested state, play it. Otherwise, silently leave default.
            var anim = instance.GetComponentInChildren<Animator>();
            if (anim != null && anim.runtimeAnimatorController != null && !string.IsNullOrEmpty(spec.IdleAnimation))
            {
                if (anim.HasState(0, Animator.StringToHash(spec.IdleAnimation)))
                {
                    anim.Play(spec.IdleAnimation, 0, 0f);
                }
            }
            return true;
        }

        private GameObject TryLoadCharacterPrefab(NamedVillagerSpec spec, Transform parent)
        {
            // Try a few known character prefab paths in priority order.
            // KayKit Char_* pool gives us variety until the Blender pipeline produces
            // bespoke models (see CLAUDE.md art pipeline section).
            string[] candidates;
            switch (spec.Role)
            {
                case "Smith":
                    candidates = new[]
                    {
                        "Assets/_Project/Prefabs/Characters/KayKit/Char_Knight.prefab",
                        "Assets/_Project/Prefabs/Characters/KayKit/Char_Rogue.prefab",
                    };
                    break;
                case "Weaver":
                    candidates = new[]
                    {
                        "Assets/_Project/Prefabs/Characters/KayKit/Char_Mage.prefab",
                        "Assets/_Project/Prefabs/Characters/KayKit/Char_Rogue.prefab",
                    };
                    break;
                case "Elder":
                    candidates = new[]
                    {
                        "Assets/_Project/Prefabs/Characters/KayKit/Char_Mage.prefab",
                        "Assets/_Project/Prefabs/Characters/KayKit/Char_Knight.prefab",
                    };
                    break;
                case "Apprentice":
                    candidates = new[]
                    {
                        "Assets/_Project/Prefabs/Characters/KayKit/Char_Ranger.prefab",
                        "Assets/_Project/Prefabs/Characters/KayKit/Char_Rogue.prefab",
                    };
                    break;
                case "Priest":
                    candidates = new[]
                    {
                        "Assets/_Project/Prefabs/Characters/KayKit/Char_Mage.prefab",
                        "Assets/_Project/Prefabs/Characters/KayKit/Char_Knight.prefab",
                    };
                    break;
                default:
                    candidates = new[]
                    {
                        "Assets/_Project/Prefabs/Characters/KayKit/Char_Rogue.prefab",
                    };
                    break;
            }

            foreach (var path in candidates)
            {
                GameObject prefab = null;
#if UNITY_EDITOR
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#endif
                if (prefab == null)
                {
                    var leaf = System.IO.Path.GetFileNameWithoutExtension(path);
                    prefab = Resources.Load<GameObject>("Characters/KayKit/" + leaf);
                }
                if (prefab != null)
                {
                    return Object.Instantiate(prefab, spec.Position, Quaternion.identity, parent);
                }
            }
            return null;
        }

        private GameObject BuildFallbackVisual(NamedVillagerSpec spec, Transform parent)
        {
            // URP-safe capsule with tinted _BaseColor per CLAUDE.md mandate.
            var root = new GameObject("NamedVillagerFallback_" + spec.Role);
            root.transform.SetParent(parent, worldPositionStays: false);
            root.transform.position = spec.Position;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); // URP-safe
            body.transform.SetParent(root.transform, worldPositionStays: false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.9f, 0.45f);

            // The capsule's auto CapsuleCollider gets in the way of our SphereCollider trigger.
            var existingCollider = body.GetComponent<Collider>();
            if (existingCollider != null)
            {
                Object.Destroy(existingCollider);
            }

            var renderer = body.GetComponent<Renderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    // URP missing? Fall back to a builtin that still renders.
                    shader = Shader.Find("Standard");
                }
                if (shader != null)
                {
                    var mat = new Material(shader);
                    if (mat.HasProperty("_BaseColor"))
                    {
                        mat.SetColor("_BaseColor", spec.TintFallback);
                    }
                    else
                    {
                        mat.color = spec.TintFallback;
                    }
                    renderer.sharedMaterial = mat;
                }
            }

            // Tiny "head" so the fallback reads as a person rather than a pill.
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
            head.transform.SetParent(root.transform, worldPositionStays: false);
            head.transform.localPosition = new Vector3(0f, 1.85f, 0f);
            head.transform.localScale = new Vector3(0.32f, 0.32f, 0.32f);
            var headCol = head.GetComponent<Collider>();
            if (headCol != null)
            {
                Object.Destroy(headCol);
            }
            var headRenderer = head.GetComponent<Renderer>();
            if (headRenderer != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }
                if (shader != null)
                {
                    var headMat = new Material(shader);
                    var skin = new Color(0.86f, 0.72f, 0.60f);
                    if (headMat.HasProperty("_BaseColor"))
                    {
                        headMat.SetColor("_BaseColor", skin);
                    }
                    else
                    {
                        headMat.color = skin;
                    }
                    headRenderer.sharedMaterial = headMat;
                }
            }
            return root;
        }
    }
}
