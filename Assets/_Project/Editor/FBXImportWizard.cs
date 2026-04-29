using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Tartaria.Editor
{
    /// <summary>
    /// FBX Import Wizard — automates character mesh import pipeline.
    /// Scans Models/Characters/ for new FBX files, configures as Humanoid rig,
    /// creates Avatar, generates prefabs with Animator controller.
    /// 
    /// Usage: Unity menu → Tartaria → Import Character Meshes
    /// CLI Usage: Unity -batchmode -executeMethod Tartaria.Editor.FBXImportWizard.ImportAllCLI -quit
    /// </summary>
    public class FBXImportWizard : EditorWindow
    {
        private static readonly string CHARACTERS_DIR = "Assets/_Project/Models/Characters";
        private static readonly string PREFABS_DIR = "Assets/_Project/Prefabs/Characters";
        private static readonly string CONTROLLER_PATH = "Assets/_Project/Animations/PlayerLocomotion.controller";

        // Expected character mappings
        private static readonly Dictionary<string, string> CHARACTER_MAPPING = new Dictionary<string, string>
        {
            { "Player_Mesh", "Player" },
            { "Anastasia_Mesh", "Anastasia" },
            { "Milo_Mesh", "Milo" },
            { "MudGolem_Mesh", "MudGolem" }
        };

        [MenuItem("Tartaria/Import Character Meshes")]
        static void ShowWindow()
        {
            var window = GetWindow<FBXImportWizard>("FBX Import");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        /// <summary>
        /// CLI entry point: scan + import all FBX files (batch mode compatible).
        /// </summary>
        [MenuItem("Tartaria/CLI/Import Character Meshes")]
        public static void ImportAllCLI()
        {
            Debug.Log("[FBXWizard] CLI execution started...");
            
            EnsureDirectoriesExist();
            
            var fbxFiles = Directory.GetFiles(
                Path.Combine(Application.dataPath, "..", CHARACTERS_DIR),
                "*.fbx",
                SearchOption.TopDirectoryOnly
            );

            if (fbxFiles.Length == 0)
            {
                Debug.LogWarning("[FBXWizard] No FBX files found in " + CHARACTERS_DIR);
                Debug.LogWarning("[FBXWizard] Please download characters from Mixamo first.");
                return;
            }

            Debug.Log($"[FBXWizard] Found {fbxFiles.Length} FBX files to process...");

            int successCount = 0;
            foreach (var fbxPath in fbxFiles)
            {
                string assetPath = "Assets" + fbxPath.Replace(Application.dataPath, "").Replace('\\', '/');
                if (ProcessFBX(assetPath))
                {
                    successCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FBXWizard] CLI execution complete: {successCount}/{fbxFiles.Length} characters imported successfully.");
        }

        void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("FBX Import Wizard — P0 Character Meshes", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Automates character mesh import pipeline:\n\n" +
                "1. Scans Models/Characters/ for FBX files\n" +
                "2. Configures as Humanoid rig + creates Avatar\n" +
                "3. Generates prefabs with SkinnedMeshRenderer\n" +
                "4. Wires Animator controller (PlayerLocomotion)\n\n" +
                "Expected FBX files from Mixamo:\n" +
                "• Player_Mesh.fbx (Male Adventurer)\n" +
                "• Anastasia_Mesh.fbx (Female Queen)\n" +
                "• Milo_Mesh.fbx (Male Worker)\n" +
                "• MudGolem_Mesh.fbx (Creature Mutant)",
                MessageType.Info
            );

            GUILayout.Space(10);

            if (!Directory.Exists(Path.Combine(Application.dataPath, "..", CHARACTERS_DIR)))
            {
                EditorGUILayout.HelpBox(
                    $"Characters directory not found: {CHARACTERS_DIR}\n" +
                    "It will be created on import.",
                    MessageType.Warning
                );
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Scan for FBX Files", GUILayout.Height(40)))
            {
                ScanFBXFiles();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Import All Characters (Full Pipeline)", GUILayout.Height(60)))
            {
                ImportAllCLI();
                
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Import Complete", "Check Console for detailed results.", "OK");
                }
            }
        }

        void ScanFBXFiles()
        {
            EnsureDirectoriesExist();

            var fbxFiles = Directory.GetFiles(
                Path.Combine(Application.dataPath, "..", CHARACTERS_DIR),
                "*.fbx",
                SearchOption.TopDirectoryOnly
            );

            if (fbxFiles.Length == 0)
            {
                Debug.LogWarning($"[FBXWizard] No .fbx files in {CHARACTERS_DIR}. Download from mixamo.com.");
                
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "No FBX Files Found",
                        $"No .fbx files in {CHARACTERS_DIR}\n\n" +
                        "Please download characters from Mixamo:\n" +
                        "1. Visit mixamo.com\n" +
                        "2. Download: Adventurer, Queen, Worker, Mutant\n" +
                        "3. Save as: Player_Mesh.fbx, Anastasia_Mesh.fbx, Milo_Mesh.fbx, MudGolem_Mesh.fbx\n" +
                        $"4. Place in: {CHARACTERS_DIR}",
                        "OK"
                    );
                }
                return;
            }

            string report = $"Found {fbxFiles.Length} FBX file(s):\n\n";
            foreach (var path in fbxFiles)
            {
                string filename = Path.GetFileName(path);
                string assetPath = "Assets" + path.Replace(Application.dataPath, "").Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                
                if (importer != null)
                {
                    string status = importer.animationType == ModelImporterAnimationType.Human ? "✓ Humanoid" : "⚠ Needs config";
                    report += $"• {filename} — {status}\n";
                }
                else
                {
                    report += $"• {filename} — ⚠ Not imported yet\n";
                }
            }

            Debug.Log("[FBXWizard] " + report);
            
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("FBX Scan Results", report, "OK");
            }
        }

        static void EnsureDirectoriesExist()
        {
            string charDir = Path.Combine(Application.dataPath, "..", CHARACTERS_DIR);
            string prefabDir = Path.Combine(Application.dataPath, "..", PREFABS_DIR);

            if (!Directory.Exists(charDir))
            {
                Directory.CreateDirectory(charDir);
                Debug.Log($"[FBXWizard] Created: {CHARACTERS_DIR}");
            }

            if (!Directory.Exists(prefabDir))
            {
                Directory.CreateDirectory(prefabDir);
                Debug.Log($"[FBXWizard] Created: {PREFABS_DIR}");
            }
        }

        static bool ProcessFBX(string assetPath)
        {
            string filename = Path.GetFileNameWithoutExtension(assetPath);
            Debug.Log($"[FBXWizard] Processing: {filename}");

            try
            {
                // Step 1: Configure ModelImporter for Humanoid rig
                ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer == null)
                {
                    Debug.LogError($"[FBXWizard] Failed to get ModelImporter for: {assetPath}");
                    return false;
                }

                bool needsReimport = false;

                if (importer.animationType != ModelImporterAnimationType.Human)
                {
                    importer.animationType = ModelImporterAnimationType.Human;
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    needsReimport = true;
                }

                if (importer.materialImportMode != ModelImporterMaterialImportMode.ImportStandard)
                {
                    importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                    needsReimport = true;
                }

                if (importer.meshCompression != ModelImporterMeshCompression.Off)
                {
                    importer.meshCompression = ModelImporterMeshCompression.Off; // High quality
                    needsReimport = true;
                }

                if (needsReimport)
                {
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    Debug.Log($"[FBXWizard]  ✓ Configured as Humanoid: {filename}");
                }

                // Step 2: Load FBX GameObject
                GameObject fbxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (fbxPrefab == null)
                {
                    Debug.LogError($"[FBXWizard] Failed to load FBX GameObject: {assetPath}");
                    return false;
                }

                // Step 3: Create character prefab variant
                string prefabName = CHARACTER_MAPPING.ContainsKey(filename) 
                    ? CHARACTER_MAPPING[filename] 
                    : filename.Replace("_Mesh", "");
                
                string prefabPath = $"{PREFABS_DIR}/{prefabName}.prefab";

                // Instantiate FBX in scene temporarily
                GameObject instance = PrefabUtility.InstantiatePrefab(fbxPrefab) as GameObject;
                instance.name = prefabName;

                // Add Animator if missing
                Animator animator = instance.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = instance.AddComponent<Animator>();
                }

                // Wire controller
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CONTROLLER_PATH);
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                    animator.applyRootMotion = false; // CharacterController handles movement
                    Debug.Log($"[FBXWizard]  ✓ Wired Animator: PlayerLocomotion");
                }
                else
                {
                    Debug.LogWarning($"[FBXWizard] Controller not found: {CONTROLLER_PATH}");
                }

                // Ensure Avatar is assigned
                Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(assetPath);
                if (avatar != null)
                {
                    animator.avatar = avatar;
                }

                // Save as new prefab
                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                DestroyImmediate(instance);

                if (savedPrefab != null)
                {
                    Debug.Log($"[FBXWizard]  ✓ Created prefab: {prefabPath}");
                    return true;
                }
                else
                {
                    Debug.LogError($"[FBXWizard] Failed to save prefab: {prefabPath}");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FBXWizard] Exception processing {filename}: {e.Message}");
                return false;
            }
        }
    }
}
