using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace Tartaria.Editor
{
    /// <summary>
    /// Custom Shader Material Builder + Scene Applicator.
    /// Creates materials from the 4 custom Tartaria shaders and applies them to scene objects.
    /// 
    /// Usage: Unity menu → Tartaria → Apply Custom Shaders
    /// CLI Usage: Unity -batchmode -executeMethod Tartaria.Editor.CustomShaderApplicator.ApplyAllCLI -quit
    /// </summary>
    public class CustomShaderApplicator : EditorWindow
    {
        /// <summary>
        /// CLI entry point: create materials + apply to scene (batch mode compatible).
        /// </summary>
        [MenuItem("Tartaria/CLI/Apply Custom Shaders")]
        public static void ApplyAllCLI()
        {
            Debug.Log("[CustomShaders] CLI execution started...");
            Debug.Log("[CustomShaders] CLI execution started...");
            CreateAllMaterialsStatic();
            ApplyMaterialsToSceneStatic();
            Debug.Log("[CustomShaders] CLI execution complete.");
        }
        [MenuItem("Tartaria/Apply Custom Shaders")]
        static void ShowWindow()
        {
            var window = GetWindow<CustomShaderApplicator>("Custom Shaders");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Custom Shader Material Builder", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Creates materials from 4 custom URP shaders:\n" +
                "• AetherVein (pulsing blue-white glow)\n" +
                "• Corruption (iridescent fractal dissolve)\n" +
                "• Restoration (mud → clean transition)\n" +
                "• SpectralGhost (transparent shimmer)", 
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("1. Create All Materials", GUILayout.Height(40)))
            {
                CreateAllMaterialsStatic();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("2. Apply to Scene Objects", GUILayout.Height(40)))
            {
                ApplyMaterialsToSceneStatic();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Full Automated Setup (Create + Apply)", GUILayout.Height(50)))
            {
                CreateAllMaterialsStatic();
                ApplyMaterialsToSceneStatic();
            }
        }

        public static void CreateAllMaterialsStatic()
        {
            Debug.Log("[CustomShaders] Creating materials...");

            CreateMaterial("AetherVein", "Tartaria/AetherVein", mat =>
            {
                mat.SetColor("_BaseColor", new Color(0.2f, 0.4f, 0.8f, 1f));
                mat.SetColor("_EmissionColor", new Color(0.5f, 0.8f, 1.0f, 1f));
                mat.SetFloat("_EmissionIntensity", 2.5f);
                mat.SetFloat("_PulseSpeed", 1.2f);
                mat.SetFloat("_PulseAmplitude", 0.4f);
                mat.SetFloat("_WaveSpeed", 0.6f);
                mat.SetFloat("_WaveAmplitude", 0.08f);
                mat.SetFloat("_Metallic", 0.3f);
                mat.SetFloat("_Smoothness", 0.7f);
            });

            CreateMaterial("Corruption", "Tartaria/Corruption", mat =>
            {
                mat.SetColor("_BaseColor", new Color(0.4f, 0.2f, 0.4f, 1f));
                mat.SetColor("_CorruptionColor", new Color(0.15f, 0.0f, 0.2f, 1f));
                mat.SetColor("_IridescentTint", new Color(0.5f, 0.2f, 0.8f, 1f));
                mat.SetFloat("_CorruptionAmount", 0.7f);
                mat.SetFloat("_FractalScale", 12.0f);
                mat.SetFloat("_FractalSpeed", 0.4f);
                mat.SetFloat("_DissolveEdgeWidth", 0.15f);
                mat.SetColor("_DissolveEdgeColor", new Color(1f, 0.3f, 0f, 1f));
                mat.SetFloat("_Metallic", 0.6f);
                mat.SetFloat("_Smoothness", 0.8f);
            });

            CreateMaterial("Restoration", "Tartaria/Restoration", mat =>
            {
                mat.SetColor("_MudColor", new Color(0.35f, 0.25f, 0.15f, 1f));
                mat.SetColor("_CleanColor", new Color(0.9f, 0.85f, 0.7f, 1f));
                mat.SetFloat("_RestorationProgress", 0.0f); // Will be animated
                mat.SetFloat("_TransitionSharpness", 8.0f);
                mat.SetColor("_GlowColor", new Color(1f, 0.8f, 0.3f, 1f));
                mat.SetFloat("_GlowIntensity", 2.5f);
                mat.SetFloat("_Metallic", 0.4f);
                mat.SetFloat("_Smoothness", 0.75f);
                
                // Set textures to existing mud/stone textures
                var mudTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Materials/Tex_MudNoise.png");
                var stoneTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Materials/Tex_StoneNoise.png");
                if (mudTex) mat.SetTexture("_MudTexture", mudTex);
                if (stoneTex) mat.SetTexture("_CleanTexture", stoneTex);
            });

            CreateMaterial("SpectralGhost", "Tartaria/SpectralGhost", mat =>
            {
                mat.SetColor("_BaseColor", new Color(0.7f, 0.9f, 1.0f, 1f));
                mat.SetColor("_RimColor", new Color(0.8f, 1.0f, 1.0f, 1f));
                mat.SetFloat("_RimPower", 3.5f);
                mat.SetFloat("_RimIntensity", 2.0f);
                mat.SetFloat("_Transparency", 0.5f);
                mat.SetFloat("_ShimmerSpeed", 1.8f);
                mat.SetFloat("_ShimmerScale", 25.0f);
                mat.SetFloat("_ShimmerIntensity", 0.4f);
                mat.SetFloat("_DistortionStrength", 0.03f);
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[CustomShaders] ✓ All 4 materials created in Assets/_Project/Materials/");
            
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Success", "Created 4 custom shader materials:\n• AetherVein\n• Corruption\n• Restoration\n• SpectralGhost", "OK");
            }
        }

        static void CreateMaterial(string name, string shaderPath, System.Action<Material> configure)
        {
            string matPath = $"Assets/_Project/Materials/M_{name}.mat";

            Shader shader = Shader.Find(shaderPath);
            if (shader == null)
            {
                Debug.LogError($"[CustomShaders] Shader not found: {shaderPath}");
                return;
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            configure?.Invoke(mat);

            EditorUtility.SetDirty(mat);
            Debug.Log($"[CustomShaders] Created/Updated: {matPath}");
        }

        public static void ApplyMaterialsToSceneStatic()
        {
            Debug.Log("[CustomShaders] Applying materials to scene...");

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("[CustomShaders] No active scene. Open Echohaven_VerticalSlice.unity first.");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Error", "No active scene. Open Echohaven_VerticalSlice.unity first.", "OK");
                }
                return;
            }

            // Load materials
            Material aetherVein = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_AetherVein.mat");
            Material corruption = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_Corruption.mat");
            Material restoration = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_Restoration.mat");
            Material spectralGhost = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_SpectralGhost.mat");

            if (!aetherVein || !corruption || !restoration || !spectralGhost)
            {
                Debug.LogError("[CustomShaders] Materials not found. Run CreateAllMaterialsStatic() first.");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Error", "Materials not found. Run 'Create All Materials' first.", "OK");
                }
                return;
            }

            int aetherCount = 0, corruptionCount = 0, restorationCount = 0, ghostCount = 0;

            // Find all renderers in scene
            var allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);

            foreach (var renderer in allRenderers)
            {
                GameObject go = renderer.gameObject;
                string name = go.name.ToLower();

                // Apply AetherVein to Aether veins, crystals, orbs AND detail geometry
                if (name.Contains("aether") || name.Contains("vein") || name.Contains("energy") || 
                    name.Contains("crystal") || name.Contains("orb") || 
                    name.Contains("orbhalo") || name.Contains("halo"))  // Added detail geo names
                {
                    ApplyMaterial(renderer, aetherVein);
                    aetherCount++;
                }

                // Apply Corruption to corruption nodes + golem spawn rings
                if (name.Contains("corruption") || name.Contains("corrupt") || name.Contains("fractal") ||
                    name.Contains("spawnring") || name.Contains("golem"))  // Added golem names
                {
                    ApplyMaterial(renderer, corruption);
                    corruptionCount++;
                }

                // Apply Restoration to ALL buildings (mud→clean shader)
                if (name.Contains("dome") || name.Contains("spire") || name.Contains("fountain") ||
                    name.Contains("building") || name.Contains("echohaven") || name.Contains("body"))  // Added body (building main mesh)
                {
                    ApplyMaterial(renderer, restoration);
                    restorationCount++;
                }

                // Apply SpectralGhost to Anastasia prefab
                if (name.Contains("anastasia") || name.Contains("ghost") || name.Contains("spectral"))
                {
                    ApplyMaterial(renderer, spectralGhost);
                    ghostCount++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);

            string report = $"Applied custom materials:\n" +
                           $"• AetherVein: {aetherCount} objects\n" +
                           $"• Corruption: {corruptionCount} objects\n" +
                           $"• Restoration: {restorationCount} objects\n" +
                           $"• SpectralGhost: {ghostCount} objects";

            Debug.Log($"[CustomShaders] {report}");
            
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Success", report, "OK");
            }
        }

        static void ApplyMaterial(Renderer renderer, Material material)
        {
            var materials = renderer.sharedMaterials.ToList();
            
            // If renderer has no materials, add one
            if (materials.Count == 0)
            {
                materials.Add(material);
            }
            else
            {
                // Replace first material
                materials[0] = material;
            }

            renderer.sharedMaterials = materials.ToArray();
            EditorUtility.SetDirty(renderer);
        }
    }
}
