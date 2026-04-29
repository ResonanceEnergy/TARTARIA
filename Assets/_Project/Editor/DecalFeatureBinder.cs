// DecalFeatureBinder.cs
// Adds the URP DecalRendererFeature to TartariaURP_Renderer if not already present.
// Idempotent. Runs as part of OneClickBuild Phase 13.
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Editor
{
    public static class DecalFeatureBinder
    {
        const string RendererPath = "Assets/_Project/Config/TartariaURP_Renderer.asset";

        [MenuItem("Tartaria/Setup/Add Decal Renderer Feature", false, 64)]
        public static void AddDecalFeature()
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (rendererData == null)
            {
                Debug.LogWarning($"[Tartaria][Decal] Renderer asset not found at {RendererPath}");
                return;
            }

            if (rendererData.rendererFeatures.Any(f => f is DecalRendererFeature))
            {
                Debug.Log("[Tartaria][Decal] DecalRendererFeature already present.");
                return;
            }

            var feature = ScriptableObject.CreateInstance<DecalRendererFeature>();
            feature.name = "Decal";
            feature.SetActive(true);

            // Persist as sub-asset of the renderer.
            AssetDatabase.AddObjectToAsset(feature, rendererData);
            rendererData.rendererFeatures.Add(feature);
            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(RendererPath, ImportAssetOptions.ForceUpdate);

            Debug.Log("[Tartaria][Decal] Added DecalRendererFeature to TartariaURP_Renderer.");
        }
    }
}
