using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Tartaria.Core
{
    /// <summary>
    /// Aether Fog Renderer Feature — injects volumetric Aether rendering into URP pipeline.
    /// Uses RenderGraph API (URP 17+). Reads _AetherVolume global texture set by
    /// AetherFieldRenderBridge, raymarches against scene depth, blends additively.
    /// 
    /// Injection point: AfterRenderingTransparents (before post-processing).
    /// </summary>
    public class AetherFogRendererFeature : ScriptableRendererFeature
    {
        class AetherFogPass : ScriptableRenderPass
        {
            const string kPassName = "AetherFog";
            Material _material;

            public AetherFogPass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            }

            public void Setup()
            {
                var shader = Shader.Find("Hidden/Tartaria/AetherFog");
                if (shader == null)
                {
                    Debug.LogError("[AetherFogPass] Shader 'Hidden/Tartaria/AetherFog' not found!");
                    return;
                }
                if (_material == null)
                    _material = CoreUtils.CreateEngineMaterial(shader);
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (_material == null) return;

                var resourceData = frameData.Get<UniversalResourceData>();
                var cameraData = frameData.Get<UniversalCameraData>();
                
                if (resourceData == null || cameraData == null)
                    return;

                // Skip for scene view unless visualization is explicitly enabled
                if (cameraData.cameraType == CameraType.SceneView)
                    return;

                // RenderGraph pass builder
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(kPassName, out var passData))
                {
                    // Read camera color and depth
                    passData.cameraColor = resourceData.activeColorTexture;
                    passData.cameraDepth = resourceData.activeDepthTexture;
                    passData.material = _material;

                    builder.UseTexture(passData.cameraColor, AccessFlags.ReadWrite);
                    builder.UseTexture(passData.cameraDepth, AccessFlags.Read);

                    builder.SetRenderFunc<PassData>(static (PassData data, RasterGraphContext context) =>
                    {
                        if (data.material == null) return;

                        // Blit using the AetherFog shader
                        Blitter.BlitTexture(context.cmd, data.cameraColor, new Vector4(1f, 1f, 0f, 0f), data.material, 0);
                    });
                }
            }

            public void Dispose()
            {
                CoreUtils.Destroy(_material);
            }

            class PassData
            {
                public TextureHandle cameraColor;
                public TextureHandle cameraDepth;
                public Material material;
            }
        }

        AetherFogPass _pass;

        public override void Create()
        {
            _pass = new AetherFogPass();
            _pass.Setup();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_pass == null) return;
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            _pass?.Dispose();
        }
    }
}
