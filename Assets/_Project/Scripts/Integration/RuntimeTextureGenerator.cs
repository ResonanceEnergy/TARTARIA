using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Runtime Texture Generator — Creates procedural textures for buildings and terrain
    /// 
    /// ⚠️ DEPRECATED: This system is no longer needed.
    /// The project has high-quality PBR materials with 2K textures from AmbientCG/Polyhaven.
    /// Use Assets/_Project/Materials/PBR/ materials instead (Rocks023, PavingStones150, Marble006, etc.)
    /// 
    /// Kept for backward compatibility and as fallback if PBR materials are missing.
    /// </summary>
    public static class RuntimeTextureGenerator
    {
        /// <summary>
        /// Create a stone texture with noise variation
        /// </summary>
        public static Texture2D CreateStoneTexture(int size = 512)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseColor = new Color(0.75f, 0.72f, 0.65f);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                    float detail = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.2f;
                    float variation = noise * 0.3f + detail;
                    
                    Color pixelColor = baseColor * (0.7f + variation);
                    tex.SetPixel(x, y, pixelColor);
                }
            }
            
            tex.Apply();
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 8;
            return tex;
        }
        
        /// <summary>
        /// Create a mud texture with brown tones
        /// </summary>
        public static Texture2D CreateMudTexture(int size = 512)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseColor = new Color(0.35f, 0.25f, 0.15f);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                    float cracks = Mathf.PerlinNoise(x * 0.3f, y * 0.3f);
                    float variation = noise * 0.4f + cracks * 0.2f;
                    
                    Color pixelColor = baseColor * (0.6f + variation);
                    
                    // Add crack lines
                    if (cracks > 0.8f)
                        pixelColor *= 0.5f;
                    
                    tex.SetPixel(x, y, pixelColor);
                }
            }
            
            tex.Apply();
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 8;
            return tex;
        }
        
        /// <summary>
        /// Create a grass texture with green variation
        /// </summary>
        public static Texture2D CreateGrassTexture(int size = 512)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseColor = new Color(0.4f, 0.5f, 0.3f);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise1 = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    float noise2 = Mathf.PerlinNoise(x * 0.3f, y * 0.3f);
                    float variation = noise1 * 0.4f + noise2 * 0.3f;
                    
                    Color pixelColor = baseColor * (0.6f + variation);
                    
                    // Add occasional darker patches
                    if (noise2 < 0.3f)
                        pixelColor *= 0.8f;
                    
                    tex.SetPixel(x, y, pixelColor);
                }
            }
            
            tex.Apply();
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 8;
            return tex;
        }
        
        /// <summary>
        /// Create a gold ornament texture with metallic appearance
        /// </summary>
        public static Texture2D CreateGoldTexture(int size = 256)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseColor = new Color(1f, 0.85f, 0.3f);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.15f, y * 0.15f);
                    float shine = Mathf.PerlinNoise(x * 0.5f, y * 0.5f);
                    float variation = noise * 0.2f + shine * 0.3f;
                    
                    Color pixelColor = baseColor * (0.8f + variation);
                    tex.SetPixel(x, y, pixelColor);
                }
            }
            
            tex.Apply();
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 8;
            return tex;
        }
        
        /// <summary>
        /// Create a normal map from height data
        /// </summary>
        public static Texture2D CreateNormalMap(Texture2D heightMap)
        {
            int size = heightMap.width;
            Texture2D normalMap = new Texture2D(size, size, TextureFormat.RGBA32, true);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Sample neighboring pixels
                    float left = heightMap.GetPixel((x - 1 + size) % size, y).grayscale;
                    float right = heightMap.GetPixel((x + 1) % size, y).grayscale;
                    float down = heightMap.GetPixel(x, (y - 1 + size) % size).grayscale;
                    float up = heightMap.GetPixel(x, (y + 1) % size).grayscale;
                    
                    // Calculate normal
                    Vector3 normal = new Vector3(left - right, 2f, down - up);
                    normal.Normalize();
                    
                    // Convert to color (0-1 range)
                    Color normalColor = new Color(
                        normal.x * 0.5f + 0.5f,
                        normal.y * 0.5f + 0.5f,
                        normal.z * 0.5f + 0.5f,
                        1f
                    );
                    
                    normalMap.SetPixel(x, y, normalColor);
                }
            }
            
            normalMap.Apply();
            normalMap.filterMode = FilterMode.Trilinear;
            normalMap.anisoLevel = 8;
            return normalMap;
        }
        
        /// <summary>
        /// Create a water texture with flow animation data
        /// </summary>
        public static Texture2D CreateWaterTexture(int size = 512)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseColor = new Color(0.2f, 0.4f, 0.6f, 0.7f); // Semi-transparent blue
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Animated wave pattern
                    float wave1 = Mathf.Sin(x * 0.1f) * Mathf.Cos(y * 0.1f);
                    float wave2 = Mathf.Sin(x * 0.05f + y * 0.05f);
                    float waves = wave1 * 0.3f + wave2 * 0.2f;
                    
                    Color pixelColor = baseColor * (1f + waves);
                    tex.SetPixel(x, y, pixelColor);
                }
            }
            
            tex.Apply();
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 8;
            return tex;
        }
        
        /// <summary>
        /// Create material and assign generated textures
        /// </summary>
        public static Material CreateMaterialWithTextures(string materialName, MaterialType type)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = materialName;
            
            Texture2D albedo = null;
            float smoothness = 0.4f;
            float metallic = 0f;
            
            switch (type)
            {
                case MaterialType.Stone:
                    albedo = CreateStoneTexture();
                    smoothness = 0.3f;
                    break;
                    
                case MaterialType.Mud:
                    albedo = CreateMudTexture();
                    smoothness = 0.1f;
                    break;
                    
                case MaterialType.Grass:
                    albedo = CreateGrassTexture();
                    smoothness = 0.2f;
                    break;
                    
                case MaterialType.Gold:
                    albedo = CreateGoldTexture();
                    smoothness = 0.9f;
                    metallic = 0.95f;
                    break;
                    
                case MaterialType.Water:
                    albedo = CreateWaterTexture();
                    smoothness = 0.95f;
                    mat.SetFloat("_Surface", 1); // Transparent
                    mat.SetFloat("_Blend", 0); // Alpha blend
                    break;
            }
            
            if (albedo != null)
            {
                mat.SetTexture("_BaseMap", albedo);
                
                // Generate and assign normal map
                Texture2D normalMap = CreateNormalMap(albedo);
                mat.SetTexture("_BumpMap", normalMap);
                mat.EnableKeyword("_NORMALMAP");
            }
            
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            
            return mat;
        }
        
        public enum MaterialType
        {
            Stone,
            Mud,
            Grass,
            Gold,
            Water
        }
    }
}
