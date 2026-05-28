using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon Scene Builder — Automated scene creation for Moon 1-13
    /// Creates scenes with proper lighting, terrain, and Volume Profile setup
    /// Usage: Tools → TARTARIA → Build Moon Scene
    /// </summary>
    public class MoonSceneBuilder : EditorWindow
    {
        private int moonNumber = 1;
        private string moonName = "MagneticMoon";
        private Color ambientColor = new Color(1f, 0.8f, 0.6f); // Golden hour
        private float terrainSize = 500f;

        [MenuItem("Tools/TARTARIA/Build Moon Scene")]
        static void ShowWindow()
        {
            var window = GetWindow<MoonSceneBuilder>("Moon Scene Builder");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("TARTARIA Moon Scene Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            moonNumber = EditorGUILayout.IntSlider("Moon Number", moonNumber, 1, 13);
            moonName = EditorGUILayout.TextField("Moon Name", moonName);
            terrainSize = EditorGUILayout.FloatField("Terrain Size (m)", terrainSize);
            ambientColor = EditorGUILayout.ColorField("Ambient Color", ambientColor);

            GUILayout.Space(20);

            if (GUILayout.Button("Create Moon Scene", GUILayout.Height(40)))
            {
                CreateMoonScene();
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                $"Will create: Assets/_Project/Scenes/Moon{moonNumber}_{moonName}.unity\n\n" +
                "Includes:\n" +
                "• Terrain ({terrainSize}m x {terrainSize}m)\n" +
                "• Directional Light (sun)\n" +
                "• Volume Profile (URP)\n" +
                "• Reflection Probe\n" +
                "• Moon{moonNumber} GameObject (scene root)",
                MessageType.Info
            );
        }

        void CreateMoonScene()
        {
            string scenePath = $"Assets/_Project/Scenes/Moon{moonNumber}_{moonName}.unity";
            
            // Check if scene already exists
            if (File.Exists(scenePath))
            {
                if (!EditorUtility.DisplayDialog(
                    "Scene Exists",
                    $"Moon{moonNumber}_{moonName}.unity already exists. Overwrite?",
                    "Yes", "Cancel"))
                {
                    return;
                }
            }

            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Create scene root GameObject
            GameObject moonRoot = new GameObject($"Moon{moonNumber}_{moonName}");
            moonRoot.transform.position = Vector3.zero;

            // Create Directional Light (Sun)
            GameObject sunObj = new GameObject("Directional Light (Sun)");
            Light sunLight = sunObj.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = ambientColor;
            sunLight.intensity = 1.0f;
            sunLight.shadows = LightShadows.Soft;
            sunObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            sunObj.transform.SetParent(moonRoot.transform);

            // Create Terrain
            TerrainData terrainData = new TerrainData();
            terrainData.size = new Vector3(terrainSize, 100f, terrainSize);
            terrainData.SetHeights(0, 0, new float[513, 513]); // Flat terrain initially

            GameObject terrainObj = Terrain.CreateTerrainGameObject(terrainData);
            terrainObj.name = "Terrain";
            terrainObj.transform.position = Vector3.zero;
            terrainObj.transform.SetParent(moonRoot.transform);

            // Create Reflection Probe
            GameObject probeObj = new GameObject("Reflection Probe");
            ReflectionProbe probe = probeObj.AddComponent<ReflectionProbe>();
            probe.size = new Vector3(terrainSize, 200f, terrainSize);
            probe.center = new Vector3(terrainSize / 2, 50f, terrainSize / 2);
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
            probeObj.transform.SetParent(moonRoot.transform);

            // Create Volume (for URP post-processing)
            GameObject volumeObj = new GameObject("Global Volume");
            Volume volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0;
            // Note: VolumeProfile asset must be created separately and assigned
            volumeObj.transform.SetParent(moonRoot.transform);

            // Create Camera placeholder
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.transform.position = new Vector3(terrainSize / 2, 50f, terrainSize / 2 - 100f);
            cam.transform.LookAt(new Vector3(terrainSize / 2, 0f, terrainSize / 2));
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(moonRoot.transform);

            // Set ambient lighting
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = 0.3f;

            // Save scene
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"[MoonSceneBuilder] Created scene: {scenePath}");

            // Select moon root in hierarchy
            Selection.activeGameObject = moonRoot;

            EditorUtility.DisplayDialog(
                "Moon Scene Created!",
                $"Successfully created Moon{moonNumber}_{moonName}.unity\n\n" +
                "Next steps:\n" +
                "1. Assign VolumeProfile to Global Volume\n" +
                "2. Add terrain textures and sculpt mud/excavation areas\n" +
                "3. Place modular cathedral prefab from Assets/_Project/Prefabs/Moon{moonNumber}/\n" +
                "4. Configure lighting and skybox\n\n" +
                "Scene saved to: {scenePath}",
                "OK"
            );
        }
    }
}
