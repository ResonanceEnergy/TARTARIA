using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// Batch runner for KayKit character wiring.
    /// Invoked from command line via Unity -executeMethod.
    /// </summary>
    public static class KayKitWiringBatch
    {
        public static void RunWiring()
        {
            Debug.Log("[KayKitWiring] Starting batch character wiring...");

            // First, create missing prefabs
            CreateMissingCharacterPrefabs();

            // Then wire all characters to KayKit models
            WireAllCharacters();

            Debug.Log("[KayKitWiring] Batch wiring complete!");
        }

        private static void CreateMissingCharacterPrefabs()
        {
            string[] characterNames = new[]
            {
                "Milo", "Thorne", "Lirael", "Korath", "Cassian", "Anastasia",
                "MudGolem", "ShadowStalker", "CrystalSentry"
            };

            int created = 0;

            foreach (string name in characterNames)
            {
                string prefabPath = $"Assets/_Project/Prefabs/Characters/{name}.prefab";
                
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
                {
                    GameObject newCharacter = new GameObject(name);
                    
                    var controller = newCharacter.AddComponent<CharacterController>();
                    controller.height = 2f;
                    controller.radius = 0.5f;
                    controller.center = new Vector3(0, 1, 0);

                    PrefabUtility.SaveAsPrefabAsset(newCharacter, prefabPath);
                    Object.DestroyImmediate(newCharacter);

                    Debug.Log($"[KayKitWiring] Created {prefabPath}");
                    created++;
                }
            }

            if (created > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[KayKitWiring] Created {created} new prefabs");
            }
        }

        private static void WireAllCharacters()
        {
            var mappings = new[]
            {
                ("Milo", "Assets/_Project/Prefabs/Characters/KayKit/Char_Ranger.prefab", 1f),
                ("Thorne", "Assets/_Project/Prefabs/Characters/KayKit/Char_Barbarian.prefab", 1f),
                ("Lirael", "Assets/_Project/Prefabs/Characters/KayKit/Char_Mage.prefab", 1f),
                ("Korath", "Assets/_Project/Prefabs/Characters/KayKit/Char_Barbarian.prefab", 2f),
                ("Cassian", "Assets/_Project/Prefabs/Characters/KayKit/Char_Rogue_Hooded.prefab", 1f),
                ("Anastasia", "Assets/_Project/Prefabs/Characters/KayKit/Char_Mage.prefab", 1f),
                ("MudGolem", "Assets/_Project/Prefabs/Characters/KayKit/Skeletons/Char_Skeleton_Warrior.prefab", 1f),
                ("ShadowStalker", "Assets/_Project/Prefabs/Characters/KayKit/Skeletons/Char_Skeleton_Rogue.prefab", 1f),
                ("CrystalSentry", "Assets/_Project/Prefabs/Characters/KayKit/Skeletons/Char_Skeleton_Minion.prefab", 1f),
            };

            int successCount = 0;

            foreach (var (name, kayKitPath, scale) in mappings)
            {
                if (WireCharacter(name, kayKitPath, scale))
                {
                    successCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[KayKitWiring] Wiring complete: {successCount}/{mappings.Length} characters wired successfully");
        }

        private static bool WireCharacter(string characterName, string kayKitPrefabPath, float scale)
        {
            string characterPrefabPath = $"Assets/_Project/Prefabs/Characters/{characterName}.prefab";
            
            GameObject characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(characterPrefabPath);
            if (characterPrefab == null)
            {
                Debug.LogWarning($"[KayKitWiring] Character prefab not found: {characterPrefabPath}");
                return false;
            }

            GameObject kayKitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(kayKitPrefabPath);
            if (kayKitPrefab == null)
            {
                Debug.LogError($"[KayKitWiring] KayKit prefab not found: {kayKitPrefabPath}");
                return false;
            }

            string prefabPath = AssetDatabase.GetAssetPath(characterPrefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            try
            {
                // Remove old visual children
                var toRemove = new System.Collections.Generic.List<Transform>();
                foreach (Transform child in prefabRoot.transform)
                {
                    if (child.name == "VisualModel" || 
                        (child.GetComponent<MeshFilter>() != null && child.GetComponents<MonoBehaviour>().Length == 0))
                    {
                        toRemove.Add(child);
                    }
                }

                foreach (Transform child in toRemove)
                {
                    Object.DestroyImmediate(child.gameObject);
                }

                // Instantiate KayKit model
                GameObject visualModel = (GameObject)PrefabUtility.InstantiatePrefab(kayKitPrefab, prefabRoot.transform);
                visualModel.name = "VisualModel";
                visualModel.transform.localPosition = Vector3.zero;
                visualModel.transform.localRotation = Quaternion.identity;
                visualModel.transform.localScale = Vector3.one * scale;

                // Add/update Animator
                Animator animator = prefabRoot.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = prefabRoot.AddComponent<Animator>();
                }

                string controllerPath = "Assets/_Project/Animations/KayKit/Controllers/AC_KayKit_Medium.controller";
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                    animator.applyRootMotion = false;
                    animator.updateMode = AnimatorUpdateMode.Normal;
                }
                else
                {
                    Debug.LogWarning($"[KayKitWiring] Controller not found: {controllerPath}");
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                Debug.Log($"[KayKitWiring] ✓ {characterName} → {kayKitPrefabPath}");

                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[KayKitWiring] Failed to wire {characterName}: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }
}
