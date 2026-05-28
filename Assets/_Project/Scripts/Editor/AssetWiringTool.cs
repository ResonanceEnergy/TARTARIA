using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace Tartaria.Editor
{
    /// <summary>
    /// Automated asset wiring tool - connects existing assets to gameplay systems
    /// Menu: Tools → TARTARIA → Wire Assets Automatically
    /// </summary>
    public class AssetWiringTool : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool wiringComplete = false;
        private List<string> logMessages = new List<string>();

        [MenuItem("Tools/TARTARIA/Wire Assets Automatically")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetWiringTool>("Asset Wiring Tool");
            window.minSize = new Vector2(600, 400);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("TARTARIA Asset Wiring Tool", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Automatically connects existing assets to gameplay systems", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool will:\n" +
                "1. Wire door sounds to InteractableObjects\n" +
                "2. Assign Hovl VFX to PowerUpPickups\n" +
                "3. Connect character prefabs to EnemySpawners\n" +
                "4. Assign UI sounds to NPCDialogue systems\n" +
                "5. Wire discovery VFX to EnvironmentalSecrets",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Wire All Assets (All Moon Scenes)", GUILayout.Height(40)))
            {
                WireAllAssets();
            }

            EditorGUILayout.Space(10);

            if (logMessages.Count > 0)
            {
                EditorGUILayout.LabelField("Wiring Log:", EditorStyles.boldLabel);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250));
                foreach (var message in logMessages)
                {
                    EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);
                }
                EditorGUILayout.EndScrollView();
            }

            if (wiringComplete)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("✅ Wiring complete! Check the log above for details.", MessageType.Info);
            }
        }

        private void WireAllAssets()
        {
            logMessages.Clear();
            wiringComplete = false;

            Log("=== STARTING ASSET WIRING ===");
            Log($"Time: {System.DateTime.Now:HH:mm:ss}");
            Log("");

            // Load all Moon scenes (Moon3 through Moon13)
            for (int moonNum = 3; moonNum <= 13; moonNum++)
            {
                string scenePath = $"Assets/_Project/Scenes/Moons/Moon{moonNum}_*.unity";
                string[] sceneGuids = AssetDatabase.FindAssets($"Moon{moonNum}_", new[] { "Assets/_Project/Scenes/Moons" });
                
                if (sceneGuids.Length > 0)
                {
                    string sceneAssetPath = AssetDatabase.GUIDToAssetPath(sceneGuids[0]);
                    Log($"Processing scene: {sceneAssetPath}");
                    
                    var scene = EditorSceneManager.OpenScene(sceneAssetPath, OpenSceneMode.Single);
                    
                    WireInteractiveObjects(moonNum);
                    WirePowerUps(moonNum);
                    WireEnemySpawners(moonNum);
                    WireNPCDialogues(moonNum);
                    WireEnvironmentalSecrets(moonNum);
                    
                    EditorSceneManager.SaveScene(scene);
                    Log($"✅ Moon{moonNum} complete!");
                    Log("");
                }
            }

            wiringComplete = true;
            Log("=== WIRING COMPLETE ===");
            Log($"Total operations: {logMessages.Count}");
            EditorUtility.DisplayDialog("Success", "Asset wiring complete! Check the log for details.", "OK");
        }

        #region Interactive Objects
        private void WireInteractiveObjects(int moonNum)
        {
            Log($"[Moon{moonNum}] Wiring Interactive Objects...");

            // Load audio clips
            var unlockSound = LoadAsset<AudioClip>("Unlock 1", "Assets/Door, Cabinet and Locker Sound Pack (Free)");
            var openSound = LoadAsset<AudioClip>("Open Door 7", "Assets/Door, Cabinet and Locker Sound Pack (Free)");
            var closeSound = LoadAsset<AudioClip>("Creaking Door Close 2", "Assets/Door, Cabinet and Locker Sound Pack (Free)");
            var leverSound = LoadAsset<AudioClip>("Swinging Metal Door Clang Shut 1", "Assets/Door, Cabinet and Locker Sound Pack (Free)");
            var breakSound = LoadAsset<AudioClip>("Close Cabinet Cupboard 1", "Assets/Door, Cabinet and Locker Sound Pack (Free)");

            // Load VFX
            var explosionVFX = LoadAsset<GameObject>("SmallExplosionEffect", "Assets/EffectExamples");

            // Find all InteractableObject components
            var interactables = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb.GetType().Name == "InteractableObject")
                .ToArray();

            int wired = 0;
            foreach (var interactable in interactables)
            {
                var type = interactable.GetType();
                
                // Wire audio based on object name
                if (interactable.name.Contains("Door"))
                {
                    SetField(interactable, "unlockSound", unlockSound);
                    SetField(interactable, "openSound", openSound);
                    SetField(interactable, "closeSound", closeSound);
                    wired++;
                }
                else if (interactable.name.Contains("Lever") || interactable.name.Contains("Pressure"))
                {
                    SetField(interactable, "activateSound", leverSound);
                    wired++;
                }
                else if (interactable.name.Contains("Breakable"))
                {
                    SetField(interactable, "breakSound", breakSound);
                    SetField(interactable, "breakEffect", explosionVFX);
                    wired++;
                }

                EditorUtility.SetDirty(interactable);
            }

            Log($"  → Wired {wired} interactive objects");
        }
        #endregion

        #region Power-Ups
        private void WirePowerUps(int moonNum)
        {
            Log($"[Moon{moonNum}] Wiring Power-Ups...");

            // Load Hovl Studio VFX
            var crystalIdle = LoadAsset<GameObject>("Crystals crossfade", "Assets/Hovl Studio");
            var buffEffect = LoadAsset<GameObject>("Buff", "Assets/Hovl Studio");
            var collectBurst = LoadAsset<GameObject>("ShardCollect", "Assets/_Project/Prefabs/VFX");

            // Load audio from Casual Game Sounds
            var collectSound = LoadAsset<AudioClip>("", "Assets/Casual Game Sounds U6"); // First suitable sound

            // Find all PowerUpPickup components
            var powerups = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb.GetType().Name == "PowerUpPickup")
                .ToArray();

            int wired = 0;
            foreach (var powerup in powerups)
            {
                SetField(powerup, "idleEffect", crystalIdle);
                SetField(powerup, "collectEffect", collectBurst);
                SetField(powerup, "activationEffect", buffEffect);
                SetField(powerup, "collectSound", collectSound);
                
                EditorUtility.SetDirty(powerup);
                wired++;
            }

            Log($"  → Wired {wired} power-ups");
        }
        #endregion

        #region Enemy Spawners
        private void WireEnemySpawners(int moonNum)
        {
            Log($"[Moon{moonNum}] Wiring Enemy Spawners...");

            // Load character prefabs
            var skeletonWarrior = LoadAsset<GameObject>("Char_Skeleton_Warrior", "Assets/_Project/Prefabs/Characters");
            var skeletonMage = LoadAsset<GameObject>("Char_Skeleton_Mage", "Assets/_Project/Prefabs/Characters");
            var mudGolem = LoadAsset<GameObject>("MudGolem", "Assets/_Project/Prefabs/Characters");

            // Load Hovl Studio VFX
            var spawnPortal = LoadAsset<GameObject>("Plexus AoE", "Assets/Hovl Studio");
            var spawnBurst = LoadAsset<GameObject>("Ground AOE explosion", "Assets/Hovl Studio");

            // Find all EnemySpawner components
            var spawners = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb.GetType().Name == "EnemySpawner")
                .ToArray();

            int wired = 0;
            foreach (var spawner in spawners)
            {
                // Assign enemy prefab based on tier
                if (spawner.name.Contains("Basic"))
                {
                    SetField(spawner, "enemyPrefab", skeletonWarrior);
                }
                else if (spawner.name.Contains("Elite"))
                {
                    SetField(spawner, "enemyPrefab", skeletonMage);
                }
                else if (spawner.name.Contains("Boss"))
                {
                    SetField(spawner, "enemyPrefab", mudGolem);
                }

                // Wire VFX
                SetField(spawner, "spawnPortalEffect", spawnPortal);
                SetField(spawner, "spawnBurstEffect", spawnBurst);

                EditorUtility.SetDirty(spawner);
                wired++;
            }

            Log($"  → Wired {wired} enemy spawners");
        }
        #endregion

        #region NPC Dialogues
        private void WireNPCDialogues(int moonNum)
        {
            Log($"[Moon{moonNum}] Wiring NPC Dialogues...");

            // Load character prefabs
            var anastasia = LoadAsset<GameObject>("Anastasia", "Assets/_Project/Prefabs/Characters");
            var milo = LoadAsset<GameObject>("Milo", "Assets/_Project/Prefabs/Characters");
            var knight = LoadAsset<GameObject>("Char_Knight", "Assets/_Project/Prefabs/Characters");
            var mage = LoadAsset<GameObject>("Char_Mage", "Assets/_Project/Prefabs/Characters");

            // Load UI sounds from Casual Game Sounds
            var talkSound = LoadAsset<AudioClip>("", "Assets/Casual Game Sounds U6");

            // Find all NPCDialogue components
            var npcs = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb.GetType().Name == "NPCDialogue")
                .ToArray();

            int wired = 0;
            foreach (var npc in npcs)
            {
                // Assign character model based on NPC type
                if (npc.name.Contains("QuestGiver"))
                {
                    SetField(npc, "characterModel", anastasia);
                }
                else if (npc.name.Contains("Merchant"))
                {
                    SetField(npc, "characterModel", milo);
                }
                else if (npc.name.Contains("Helper"))
                {
                    SetField(npc, "characterModel", knight);
                }
                else if (npc.name.Contains("Lore"))
                {
                    SetField(npc, "characterModel", mage);
                }

                // Wire audio
                SetField(npc, "dialogueBlipSound", talkSound);

                EditorUtility.SetDirty(npc);
                wired++;
            }

            Log($"  → Wired {wired} NPCs");
        }
        #endregion

        #region Environmental Secrets
        private void WireEnvironmentalSecrets(int moonNum)
        {
            Log($"[Moon{moonNum}] Wiring Environmental Secrets...");

            // Load custom VFX
            var scanPulse = LoadAsset<GameObject>("ScanPulse", "Assets/_Project/Prefabs/VFX");
            var crystalAttack = LoadAsset<GameObject>("Crystals front attack", "Assets/Hovl Studio");

            // Load discovery sound from Casual Game Sounds
            var discoverySound = LoadAsset<AudioClip>("", "Assets/Casual Game Sounds U6");

            // Find all EnvironmentalSecret components
            var secrets = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb.GetType().Name == "EnvironmentalSecret")
                .ToArray();

            int wired = 0;
            foreach (var secret in secrets)
            {
                SetField(secret, "revealEffect", scanPulse);
                SetField(secret, "discoveryEffect", crystalAttack);
                SetField(secret, "discoverySound", discoverySound);

                EditorUtility.SetDirty(secret);
                wired++;
            }

            Log($"  → Wired {wired} secrets");
        }
        #endregion

        #region Helper Methods
        private T LoadAsset<T>(string assetName, string searchFolder) where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets($"{assetName} t:{typeof(T).Name}", new[] { searchFolder });
            
            if (guids.Length == 0)
            {
                // Try without name filter if nothing found
                guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { searchFolder });
                if (guids.Length > 0)
                {
                    return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private void SetField(object obj, string fieldName, object value)
        {
            if (obj == null || value == null) return;

            var type = obj.GetType();
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                // Try as property
                var property = type.GetProperty(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(obj, value);
                }
            }
        }

        private void Log(string message)
        {
            logMessages.Add(message);
            Debug.Log($"[AssetWiring] {message}");
        }
        #endregion
    }
}
