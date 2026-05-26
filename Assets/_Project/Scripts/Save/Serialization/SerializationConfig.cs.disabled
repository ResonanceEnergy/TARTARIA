using UnityEngine;

namespace Tartaria.Save.Serialization
{
    /// <summary>
    /// Serialization configuration for build-specific settings.
    /// Controls which serializer, compression, and encryption to use.
    /// 
    /// Recommended settings:
    ///   - Debug builds: JSON (human-readable, no encryption)
    ///   - Release builds: Binary + GZip + AES (fast, small, secure)
    ///   - Hybrid: Best of both (JSON metadata for tools, binary for data)
    /// </summary>
    [CreateAssetMenu(fileName = "SerializationConfig", menuPath = "TARTARIA/Save/Serialization Config")]
    public class SerializationConfig : ScriptableObject
    {
        public enum SerializerType
        {
            JSON,    // Human-readable, debug-friendly
            Binary,  // Fast, compact, production
            Hybrid   // JSON metadata + binary data
        }

        [Header("Serialization")]
        [Tooltip("Serializer to use (JSON for debug, Binary for release)")]
        public SerializerType serializerType = SerializerType.Binary;

        [Header("Compression")]
        [Tooltip("Enable compression (reduces file size 10x)")]
        public bool enableCompression = true;

        [Tooltip("Compression type (GZip = best ratio, Deflate = faster)")]
        public CompressionHelper.CompressionType compressionType = CompressionHelper.CompressionType.GZip;

        [Header("Encryption")]
        [Tooltip("Enable encryption (prevents save editing/cheating)")]
        public bool enableEncryption = true;

        [Header("Async I/O")]
        [Tooltip("Use async save/load (non-blocking, recommended for large saves)")]
        public bool useAsyncIO = true;

        [Header("Backward Compatibility")]
        [Tooltip("Support loading old JSON saves from v1.0 (before serialization optimization)")]
        public bool supportLegacyJsonSaves = true;

        /// <summary>
        /// Get serializer instance based on config.
        /// </summary>
        public IGameSerializer GetSerializer()
        {
            switch (serializerType)
            {
                case SerializerType.JSON:
                    return new JsonGameSerializer();
                case SerializerType.Binary:
                    return new BinaryGameSerializer();
                case SerializerType.Hybrid:
                    return new HybridGameSerializer();
                default:
                    Debug.LogWarning($"[SerializationConfig] Unknown serializer type: {serializerType}, falling back to JSON");
                    return new JsonGameSerializer();
            }
        }

        /// <summary>
        /// Create default config for debug builds.
        /// </summary>
        public static SerializationConfig CreateDebugConfig()
        {
            var config = CreateInstance<SerializationConfig>();
            config.serializerType = SerializerType.JSON;
            config.enableCompression = false;
            config.enableEncryption = false;
            config.useAsyncIO = false;
            config.supportLegacyJsonSaves = true;
            return config;
        }

        /// <summary>
        /// Create default config for release builds.
        /// </summary>
        public static SerializationConfig CreateReleaseConfig()
        {
            var config = CreateInstance<SerializationConfig>();
            config.serializerType = SerializerType.Binary;
            config.enableCompression = true;
            config.compressionType = CompressionHelper.CompressionType.GZip;
            config.enableEncryption = true;
            config.useAsyncIO = true;
            config.supportLegacyJsonSaves = true;
            return config;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("TARTARIA/Save/Create Debug Serialization Config")]
        static void CreateDebugConfigAsset()
        {
            var config = CreateDebugConfig();
            UnityEditor.AssetDatabase.CreateAsset(config, "Assets/_Project/Settings/SerializationConfig_Debug.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log("Created debug serialization config");
        }

        [UnityEditor.MenuItem("TARTARIA/Save/Create Release Serialization Config")]
        static void CreateReleaseConfigAsset()
        {
            var config = CreateReleaseConfig();
            UnityEditor.AssetDatabase.CreateAsset(config, "Assets/_Project/Settings/SerializationConfig_Release.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log("Created release serialization config");
        }
#endif
    }
}
