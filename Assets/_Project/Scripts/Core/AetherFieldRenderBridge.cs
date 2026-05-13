using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Tartaria.Core;

namespace Tartaria.Core
{
    /// <summary>
    /// Aether Field Render Bridge — copies voxel density data from ECS simulation
    /// into a RenderTexture3D for GPU sampling in shaders.
    /// 
    /// Grid: 64×64×32 voxels. Each voxel stores 4 channels:
    /// - R: 3Hz band density
    /// - G: 6Hz band density
    /// - B: 9Hz band density
    /// - A: Total Aether intensity
    /// 
    /// Singleton pattern: AetherFieldRenderBridge.Current
    /// </summary>
    [DefaultExecutionOrder(-60)] // After world init, before GameLoopController
    public class AetherFieldRenderBridge : MonoBehaviour
    {
        const int GridX = 64;
        const int GridY = 64;
        const int GridZ = 32;

        public static AetherFieldRenderBridge Instance { get; private set; }
        public static RenderTexture Current => Instance?._volumeTexture;

        RenderTexture _volumeTexture;
        Texture3D _readableTexture;
        Color[] _pixelBuffer;
        EntityQuery _aetherNodeQuery;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[AetherFieldRenderBridge]");
            go.AddComponent<AetherFieldRenderBridge>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create 3D texture (RGBAHalf for HDR density values)
            _volumeTexture = new RenderTexture(GridX, GridY, 0, RenderTextureFormat.ARGBHalf)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
                volumeDepth = GridZ,
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear
            };
            _volumeTexture.Create();

            // Create readable copy for CPU updates
            _readableTexture = new Texture3D(GridX, GridY, GridZ, TextureFormat.RGBAHalf, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear
            };

            _pixelBuffer = new Color[GridX * GridY * GridZ];

            Debug.Log($"[AetherFieldRenderBridge] Created {GridX}×{GridY}×{GridZ} 3D texture for Aether visualization.");
        }

        void OnDestroy()
        {
            if (_volumeTexture != null)
            {
                _volumeTexture.Release();
                Destroy(_volumeTexture);
            }
            if (_readableTexture != null)
            {
                Destroy(_readableTexture);
            }
            if (Instance == this)
                Instance = null;
        }

        void LateUpdate()
        {
            UpdateTextureFromECS();
        }

        void UpdateTextureFromECS()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
                return;

            var em = world.EntityManager;
            if (_aetherNodeQuery == default)
            {
                _aetherNodeQuery = em.CreateEntityQuery(
                    ComponentType.ReadOnly<AetherNode>(),
                    ComponentType.ReadOnly<LocalTransform>()
                );
            }

            // Simple approach: query all AetherNode entities and map to 3D grid
            // Real production would use ComputeShader for better perf
            var nodes = _aetherNodeQuery.ToComponentDataArray<AetherNode>(Allocator.Temp);
            var transforms = _aetherNodeQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            // Clear buffer
            for (int i = 0; i < _pixelBuffer.Length; i++)
                _pixelBuffer[i] = Color.clear;

            // Map each node to grid cell (simplified spatial hashing)
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var pos = transforms[i].Position;

                // Remap world position to grid coordinates (assume 500m zone centered at origin)
                int x = Mathf.Clamp((int)((pos.x + 250f) / 500f * GridX), 0, GridX - 1);
                int y = Mathf.Clamp((int)((pos.y + 250f) / 500f * GridY), 0, GridY - 1);
                int z = Mathf.Clamp((int)((pos.z + 250f) / 500f * GridZ), 0, GridZ - 1);

                int idx = x + y * GridX + z * GridX * GridY;

                // Pack band densities into RGBA (Telluric=R, Harmonic=G, Celestial=B, total=A)
                float bandDensity = node.Intensity * node.Coherence;
                switch (node.Band)
                {
                    case HarmonicBand.Telluric:
                        _pixelBuffer[idx].r = Mathf.Max(_pixelBuffer[idx].r, bandDensity);
                        break;
                    case HarmonicBand.Harmonic:
                        _pixelBuffer[idx].g = Mathf.Max(_pixelBuffer[idx].g, bandDensity);
                        break;
                    case HarmonicBand.Celestial:
                        _pixelBuffer[idx].b = Mathf.Max(_pixelBuffer[idx].b, bandDensity);
                        break;
                }
                _pixelBuffer[idx].a = Mathf.Max(_pixelBuffer[idx].a, node.Intensity);
            }

            nodes.Dispose();
            transforms.Dispose();

            // Upload to GPU
            _readableTexture.SetPixels(_pixelBuffer);
            _readableTexture.Apply(false);

            Graphics.CopyTexture(_readableTexture, _volumeTexture);
        }
    }
}
