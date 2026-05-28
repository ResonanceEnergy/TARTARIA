using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Tartaria.Editor
{
    /// <summary>
    /// PHASE 5: Populates Moon 1 scene with ALL NPCs, objects, collectibles
    /// Executes after CathedralAssembler builds structure
    /// Menu: Tools → TARTARIA → Populate Moon 1 Complete
    /// </summary>
    public class Moon1ScenePopulator : EditorWindow
    {
        [MenuItem("Tools/TARTARIA/Populate Moon 1 Complete")]
        static void PopulateScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Cannot Run", "Stop Play Mode first.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                "Populate Moon 1 - 100% ECHOHAVEN",
                "This creates the COMPLETE Moon 1 experience:\n\n" +
                "CHARACTERS:\n" +
                "• Milo (companion, near south entrance)\n" +
                "• Lirael (Echo child, at rose window)\n" +
                "• 3 Reset Scouts (enemies, patrol zones)\n" +
                "• Giant Skeleton (giant-mode trigger)\n\n" +
                "INTERACTIVE OBJECTS:\n" +
                "• Pipe Organ (432 Hz tuning puzzle)\n" +
                "• 3 Water Fountains (NE/NW/S)\n" +
                "• Bell Tower controller\n" +
                "• Excavation pit zone\n\n" +
                "COLLECTIBLES:\n" +
                "• Giant Skeleton Key #1\n" +
                "• Spire Fragment (Moon 5 crossover)\n" +
                "• Airship Component (Moon 8)\n" +
                "• Prophecy Fragment\n\n" +
                "Continue?",
                "Yes - Build 100%!", "Cancel"))
            {
                return;
            }

            Debug.Log("[Moon1Populator] Starting COMPLETE SCENE POPULATION...");

            // Find or create root
            GameObject cathedralRoot = GameObject.Find("MagneticCathedral_Complete");
            if (!cathedralRoot)
            {
                Debug.LogWarning("[Moon1Populator] Cathedral not found. Run Cathedral Assembler first!");
                if (!EditorUtility.DisplayDialog(
                    "Cathedral Not Found",
                    "Cathedral structure not assembled. Run '"'"'Assemble Complete Cathedral'"'"' first?",
                    "Run Now", "Cancel"))
                {
                    return;
                }
                
                CathedralAssembler.AssembleCathedral();
                cathedralRoot = GameObject.Find("MagneticCathedral_Complete");
            }

            Vector3 cathedralPos = cathedralRoot.transform.position;

            // === CHARACTERS ===
            GameObject npcsRoot = new GameObject("NPCs");
            npcsRoot.transform.SetParent(cathedralRoot.transform);

            // Milo - near south entrance
            GameObject milo = CreateCharacter("Milo", cathedralPos + new Vector3(3f, 0f, -12f), npcsRoot.transform);
            var miloController = milo.AddComponent(System.Type.GetType("Tartaria.Gameplay.MiloController, Tartaria.Gameplay"));
            milo.tag = "NPC";
            Debug.Log("✅ Milo placed (South entrance, companion)");

            // Lirael - at rose window
            GameObject lirael = CreateCharacter("Lirael_Echo", cathedralPos + new Vector3(0f, 2f, 6f), npcsRoot.transform);
            var liraelEcho = lirael.AddComponent(System.Type.GetType("Tartaria.Gameplay.LiraelEcho, Tartaria.Gameplay"));
            lirael.tag = "NPC";
            Debug.Log("✅ Lirael placed (Rose window, 432 Hz lullaby)");

            // Reset Scouts (3 enemies)
            for (int i = 0; i < 3; i++)
            {
                Vector3[] spawnPos = {
                    cathedralPos + new Vector3(-10f, 0f, 10f),   // NW patrol
                    cathedralPos + new Vector3(10f, 0f, 10f),    // NE patrol
                    cathedralPos + new Vector3(0f, 0f, 15f)      // North patrol
                };
                
                GameObject scout = CreateCharacter($"ResetScout_{i + 1}", spawnPos[i], npcsRoot.transform);
                var scoutAI = scout.AddComponent(System.Type.GetType("Tartaria.Gameplay.ResetScout, Tartaria.Gameplay"));
                scout.tag = "Enemy";
                scout.AddComponent<CapsuleCollider>().height = 2f;
                
                // Add clipboard prop (visual only for now)
                GameObject clipboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                clipboard.name = "Clipboard";
                clipboard.transform.SetParent(scout.transform);
                clipboard.transform.localPosition = new Vector3(0.3f, 1f, 0.3f);
                clipboard.transform.localScale = new Vector3(0.2f, 0.3f, 0.02f);
            }
            Debug.Log("✅ 3 Reset Scouts placed (Victorian enemies with clipboards)");

            // Giant Skeleton
            GameObject skeleton = CreateCharacter("GiantSkeleton", cathedralPos + new Vector3(-6f, -1f, -6f), npcsRoot.transform);
            skeleton.transform.localScale = new Vector3(4.572f, 4.572f, 4.572f); // 15 feet
            skeleton.AddComponent<BoxCollider>().isTrigger = true;
            skeleton.tag = "Interactable";
            Debug.Log("✅ Giant Skeleton placed (15ft scale, giant-mode trigger)");

            // === INTERACTIVE OBJECTS ===
            GameObject interactivesRoot = new GameObject("InteractiveObjects");
            interactivesRoot.transform.SetParent(cathedralRoot.transform);

            // Pipe Organ (center nave)
            GameObject organ = GameObject.CreatePrimitive(PrimitiveType.Cube);
            organ.name = "PipeOrgan_432Hz";
            organ.transform.SetParent(interactivesRoot.transform);
            organ.transform.position = cathedralPos + new Vector3(0f, 0f, -4f);
            organ.transform.localScale = new Vector3(3f, 4f, 1f);
            var organScript = organ.AddComponent(System.Type.GetType("Tartaria.Gameplay.PipeOrgan432Hz, Tartaria.Gameplay"));
            organ.tag = "Interactable";
            Debug.Log("✅ Pipe Organ placed (432 Hz tuning puzzle)");

            // Water Fountains (3)
            Vector3[] fountainPositions = {
                cathedralPos + new Vector3(6f, 0f, 6f),   // NE
                cathedralPos + new Vector3(-6f, 0f, 6f),  // NW
                cathedralPos + new Vector3(0f, 0f, -6f)   // South
            };
            
            for (int i = 0; i < 3; i++)
            {
                GameObject fountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                fountain.name = $"PureWaterFountain_{i + 1}";
                fountain.transform.SetParent(interactivesRoot.transform);
                fountain.transform.position = fountainPositions[i];
                fountain.transform.localScale = new Vector3(2f, 1f, 2f);
                var fountainScript = fountain.AddComponent(System.Type.GetType("Tartaria.Gameplay.PureWaterFountain, Tartaria.Gameplay"));
                fountain.AddComponent<SphereCollider>().radius = 5f;
                fountain.AddComponent<SphereCollider>().isTrigger = true;
            }
            Debug.Log("✅ 3 Water Fountains placed (ionized mist, golem repel)");

            // Excavation Zone (surrounding pit)
            GameObject excavation = new GameObject("ExcavationZone");
            excavation.transform.SetParent(interactivesRoot.transform);
            excavation.transform.position = cathedralPos + new Vector3(0f, -5f, 0f);
            var excScript = excavation.AddComponent(System.Type.GetType("Tartaria.Gameplay.ExcavationZone, Tartaria.Gameplay"));
            excavation.AddComponent<BoxCollider>().size = new Vector3(50f, 10f, 50f);
            excavation.AddComponent<BoxCollider>().isTrigger = true;
            Debug.Log("✅ Excavation Zone created (50m pit, swipe minigame)");

            // === COLLECTIBLES ===
            GameObject collectiblesRoot = new GameObject("Collectibles");
            collectiblesRoot.transform.SetParent(cathedralRoot.transform);

            // Giant Skeleton Key #1 (on skeleton)
            CreateCollectible("GiantSkeletonKey_1", cathedralPos + new Vector3(-6f, 2f, -6f), 
                "GiantSkeletonKey", collectiblesRoot.transform, Color.yellow);

            // Spire Fragment (top of dome)
            CreateCollectible("SpireFragment_Moon5", cathedralPos + new Vector3(0f, 12f, 0f), 
                "SpireFragment", collectiblesRoot.transform, Color.cyan);

            // Airship Component (hidden in excavation)
            CreateCollectible("AirshipComponent_Moon8", cathedralPos + new Vector3(8f, -2f, 8f), 
                "AirshipComponent", collectiblesRoot.transform, new Color(1f, 0.5f, 0f));

            // Prophecy Fragment (behind rose window)
            CreateCollectible("ProphecyFragment_Dissonant", cathedralPos + new Vector3(0f, 4f, 7f), 
                "ProphecyFragment", collectiblesRoot.transform, Color.magenta);

            Debug.Log("✅ 4 Secret collectibles placed (keys, crossover items)");

            Debug.Log("[Moon1Populator] ✅ 100% ECHOHAVEN SCENE COMPLETE!");
            Debug.Log("Characters: 4 (Milo, Lirael, 3 Reset Scouts, Giant Skeleton)");
            Debug.Log("Interactive Objects: 5 (Organ, 3 Fountains, Excavation)");
            Debug.Log("Collectibles: 4 (Key, Spire, Airship, Prophecy)");

            Selection.activeGameObject = cathedralRoot;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog(
                "Moon 1 Complete - 100%!",
                "✅ ECHOHAVEN FULLY POPULATED!\n\n" +
                "✓ 4 Characters (Milo, Lirael, 3 enemies, skeleton)\n" +
                "✓ 5 Interactive objects (organ, fountains, excavation)\n" +
                "✓ 4 Secret collectibles (crossover items)\n" +
                "✓ Complete cathedral structure\n\n" +
                "Ready to PLAY! Hit Ctrl+P in Unity to test!",
                "LFG!"
            );
        }

        static GameObject CreateCharacter(string name, Vector3 position, Transform parent)
        {
            GameObject character = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            character.name = name;
            character.transform.position = position;
            character.transform.SetParent(parent);
            character.AddComponent<CharacterController>();
            return character;
        }

        static void CreateCollectible(string name, Vector3 position, string type, Transform parent, Color color)
        {
            GameObject collectible = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            collectible.name = name;
            collectible.transform.position = position;
            collectible.transform.localScale = Vector3.one * 0.5f;
            collectible.transform.SetParent(parent);
            
            var collScript = collectible.AddComponent(System.Type.GetType("Tartaria.Gameplay.Collectible, Tartaria.Gameplay"));
            collectible.AddComponent<SphereCollider>().isTrigger = true;
            
            // Set color
            var renderer = collectible.GetComponent<Renderer>();
            if (renderer && renderer.material)
            {
                renderer.material.color = color;
                renderer.material.SetColor("_EmissionColor", color * 2f);
                renderer.material.EnableKeyword("_EMISSION");
            }
        }
    }
}