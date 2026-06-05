using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tool to wire all companion/enemy character prefabs to KayKit models.
    /// Replaces placeholder meshes with production-ready KayKit character models.
    /// </summary>
    public class KayKitCharacterWiringTool : EditorWindow
    {
        private struct CharacterMapping
        {
            public string characterName;
            public string kayKitPrefabPath;
            public float scale;

            public CharacterMapping(string name, string path, float scale = 1f)
            {
                this.characterName = name;
                this.kayKitPrefabPath = path;
                this.scale = scale;
            }
        }

        private static readonly CharacterMapping[] MAPPINGS = new[]
        {
            // Companions
            new CharacterMapping("Milo", "Assets/_Project/Prefabs/Characters/KayKit/Char_Ranger.prefab"),
            new CharacterMapping("Thorne", "Assets/_Project/Prefabs/Characters/KayKit/Char_Barbarian.prefab"),
            new CharacterMapping("Lirael", "Assets/_Project/Prefabs/Characters/KayKit/Char_Mage.prefab"),
            new CharacterMapping("Korath", "Assets/_Project/Prefabs/Characters/KayKit/Char_Barbarian.prefab", 2f),
            new CharacterMapping("Cassian", "Assets/_Project/Prefabs/Characters/KayKit/Char_Rogue_Hooded.prefab"),
            new CharacterMapping("Anastasia", "Assets/_Project/Prefabs/Characters/KayKit/Char_Mage.prefab"),
            
            // Enemies
            new CharacterMapping("MudGolem", "Assets/_Project/Prefabs/Characters/KayKit/Skeletons/Char_Skeleton_Warrior.prefab"),
            new CharacterMapping("ShadowStalker", "Assets/_Project/Prefabs/Characters/KayKit/Skeletons/Char_Skeleton_Rogue.prefab"),
            new CharacterMapping("CrystalSentry", "Assets/_Project/Prefabs/Characters/KayKit/Skeletons/Char_Skeleton_Minion.prefab"),
        };

        private Vector2 scrollPosition;
        private Dictionary<string, bool> results = new Dictionary<string, bool>();

        [MenuItem("Tartaria/3 Wire/Characters → KayKit Models", priority = 350)]
        public static void ShowWindow()
        {
            GetWindow<KayKitCharacterWiringTool>("KayKit Wiring");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("KayKit Character Wiring Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This tool wires all companion/enemy characters to KayKit models.\n\n" +
                "OPERATION:\n" +
                "• Replaces placeholder meshes with KayKit character models\n" +
                "• Preserves all existing scripts and components\n" +
                "• Adds Animator component with KayKit controller\n" +
                "• Maintains colliders and tags",
                MessageType.Info
            );

            EditorGUILayout.Space();

            if (GUILayout.Button("Wire All Characters", GUILayout.Height(40)))
            {
                WireAllCharacters();
            }

            EditorGUILayout.Space();

            // Display results
            if (results.Count > 0)
            {
                EditorGUILayout.LabelField("Results:", EditorStyles.boldLabel);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                foreach (var kvp in results)
                {
                    string icon = kvp.Value ? "✓" : "✗";
                    EditorGUILayout.LabelField($"{icon} {kvp.Key}");
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void WireAllCharacters()
        {
            results.Clear();
            int successCount = 0;
            int totalCount = MAPPINGS.Length;

            foreach (var mapping in MAPPINGS)
            {
                bool success = WireCharacter(mapping);
                results[mapping.characterName] = success;
                if (success) successCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string message = $"Wiring complete: {successCount}/{totalCount} characters processed successfully.";
            EditorUtility.DisplayDialog("KayKit Wiring", message, "OK");
            Debug.Log($"[KayKitWiring] {message}");
        }

        private bool WireCharacter(CharacterMapping mapping)
        {
            string characterPrefabPath = $"Assets/_Project/Prefabs/Characters/{mapping.characterName}.prefab";
            
            // Load character prefab
            GameObject characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(characterPrefabPath);
            if (characterPrefab == null)
            {
                Debug.LogWarning($"[KayKitWiring] Character prefab not found: {characterPrefabPath}");
                return false;
            }

            // Load KayKit model prefab
            GameObject kayKitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(mapping.kayKitPrefabPath);
            if (kayKitPrefab == null)
            {
                Debug.LogError($"[KayKitWiring] KayKit prefab not found: {mapping.kayKitPrefabPath}");
                return false;
            }

            // Open prefab for editing
            string prefabPath = AssetDatabase.GetAssetPath(characterPrefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            try
            {
                // Remove old visual children (meshes, primitives)
                List<Transform> toRemove = new List<Transform>();
                foreach (Transform child in prefabRoot.transform)
                {
                    // Keep script-bearing children, remove visual primitives
                    if (child.GetComponent<MeshFilter>() != null || child.GetComponent<MeshRenderer>() != null)
                    {
                        if (child.GetComponents<MonoBehaviour>().Length == 0)
                        {
                            toRemove.Add(child);
                        }
                    }
                }

                foreach (Transform child in toRemove)
                {
                    DestroyImmediate(child.gameObject);
                }

                // Instantiate KayKit model as child
                GameObject visualModel = (GameObject)PrefabUtility.InstantiatePrefab(kayKitPrefab, prefabRoot.transform);
                visualModel.name = "VisualModel";
                visualModel.transform.localPosition = Vector3.zero;
                visualModel.transform.localRotation = Quaternion.identity;
                visualModel.transform.localScale = Vector3.one * mapping.scale;

                // Add Animator to root if missing
                Animator animator = prefabRoot.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = prefabRoot.AddComponent<Animator>();
                }

                // Wire animator to KayKit controller
                string controllerPath = "Assets/_Project/Animations/KayKit/Controllers/AC_KayKit_Medium.controller";
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                }
                else
                {
                    Debug.LogWarning($"[KayKitWiring] Animator controller not found: {controllerPath}");
                }

                // Save modified prefab
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                Debug.Log($"[KayKitWiring] ✓ {mapping.characterName} → {mapping.kayKitPrefabPath}");

                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[KayKitWiring] Failed to wire {mapping.characterName}: {ex.Message}");
                return false;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        [MenuItem("Tartaria/3 Wire/Create Missing Character Prefabs", priority = 360)]
        public static void CreateMissingPrefabs()
        {
            int created = 0;

            foreach (var mapping in MAPPINGS)
            {
                string characterPrefabPath = $"Assets/_Project/Prefabs/Characters/{mapping.characterName}.prefab";
                
                if (AssetDatabase.LoadAssetAtPath<GameObject>(characterPrefabPath) == null)
                {
                    // Create new prefab
                    GameObject newCharacter = new GameObject(mapping.characterName);
                    
                    // Add basic components
                    var controller = newCharacter.AddComponent<CharacterController>();
                    controller.height = 2f;
                    controller.radius = 0.5f;

                    // Save as prefab
                    PrefabUtility.SaveAsPrefabAsset(newCharacter, characterPrefabPath);
                    DestroyImmediate(newCharacter);

                    Debug.Log($"[KayKitWiring] Created {characterPrefabPath}");
                    created++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Create Missing Prefabs", $"Created {created} new character prefabs.", "OK");
        }
    }
}
