using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Tartaria.Save.Serialization
{
    /// <summary>
    /// Hybrid serializer: JSON metadata + binary data.
    /// Best of both worlds:
    ///   - JSON for header/metadata (version control friendly, human-readable)
    ///   - Binary for large data blocks (player stats, flags, arrays)
    /// 
    /// Format:
    ///   [4 bytes: JSON length]
    ///   [JSON metadata]
    ///   [Binary data]
    /// </summary>
    public class HybridGameSerializer : IGameSerializer
    {
        public string Name => "Hybrid";
        public bool IsHumanReadable => false; // Binary portion is not human-readable

        private readonly JsonGameSerializer _jsonSerializer = new();
        private readonly BinaryGameSerializer _binarySerializer = new();

        public byte[] Serialize<T>(T data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (!(data is SaveData saveData))
            {
                // Fallback to JSON for non-SaveData types
                return _jsonSerializer.Serialize(data);
            }

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                try
                {
                    // Create metadata-only version (small JSON for header)
                    var metadata = new SaveDataMetadata
                    {
                        version = saveData.version,
                        modifiedUtc = saveData.header.modifiedUtc,
                        playTimeSeconds = saveData.header.playTimeSeconds,
                        checksum = saveData.header.checksum,
                        currentMoonIndex = saveData.world.currentMoonIndex,
                        resonanceScore = saveData.world.resonanceScore,
                        buildingsRestored = saveData.world.buildingsRestored,
                        playerLevel = saveData.player.level,
                        playerPosition = saveData.player.position
                    };

                    // Serialize metadata as JSON
                    string metadataJson = JsonUtility.ToJson(metadata, prettyPrint: false);
                    byte[] metadataBytes = Encoding.UTF8.GetBytes(metadataJson);

                    // Write metadata length + metadata
                    writer.Write(metadataBytes.Length);
                    writer.Write(metadataBytes);

                    // Serialize full data as binary
                    byte[] binaryData = _binarySerializer.Serialize(saveData);
                    writer.Write(binaryData);

                    return stream.ToArray();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[HybridGameSerializer] Serialize failed: {e.Message}");
                    throw;
                }
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            if (data == null || data.Length < 4)
                throw new ArgumentException("Invalid hybrid data");

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                try
                {
                    // Read metadata
                    int metadataLength = reader.ReadInt32();
                    byte[] metadataBytes = reader.ReadBytes(metadataLength);
                    string metadataJson = Encoding.UTF8.GetString(metadataBytes);
                    var metadata = JsonUtility.FromJson<SaveDataMetadata>(metadataJson);

                    // Read binary data
                    byte[] binaryData = reader.ReadBytes((int)(stream.Length - stream.Position));
                    
                    if (typeof(T) == typeof(SaveData))
                    {
                        return _binarySerializer.Deserialize<T>(binaryData);
                    }
                    else
                    {
                        return _jsonSerializer.Deserialize<T>(metadataBytes);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[HybridGameSerializer] Deserialize failed: {e.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Lightweight metadata for quick save file inspection without full deserialization.
        /// </summary>
        [Serializable]
        public class SaveDataMetadata
        {
            public int version;
            public string modifiedUtc;
            public float playTimeSeconds;
            public string checksum;
            public int currentMoonIndex;
            public float resonanceScore;
            public int buildingsRestored;
            public int playerLevel;
            public Vector3 playerPosition;
        }
    }
}
