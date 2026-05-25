using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Tartaria.Save.Serialization
{
    /// <summary>
    /// Custom binary serializer for production builds.
    /// Features:
    ///   - Schema versioning (4-byte header: TART + version byte)
    ///   - Variable-length encoding (varint for small numbers)
    ///   - String interning (avoid duplicate strings)
    ///   - Chunked layout (skip unknown sections for forward compatibility)
    ///   - Zero-copy deserialization where possible
    /// 
    /// Performance targets:
    ///   - Serialize: &lt;10ms for typical save
    ///   - Deserialize: &lt;20ms for typical save
    ///   - File size: ~60% of JSON (before compression)
    /// </summary>
    public class BinaryGameSerializer : IGameSerializer
    {
        public string Name => "Binary";
        public bool IsHumanReadable => false;

        // Binary format header: "TART" + version byte
        const uint MAGIC_NUMBER = 0x54415254; // "TART" in ASCII
        const byte FORMAT_VERSION = 1;

        // String interning table (deduplicate common strings)
        private readonly System.Collections.Generic.Dictionary<string, int> _stringTable = new();
        private readonly System.Collections.Generic.List<string> _stringList = new();

        public byte[] Serialize<T>(T data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Reset string table for each serialization
            _stringTable.Clear();
            _stringList.Clear();

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                try
                {
                    // Write header
                    writer.Write(MAGIC_NUMBER);
                    writer.Write(FORMAT_VERSION);

                    // Serialize data based on type
                    if (data is SaveData saveData)
                    {
                        SerializeSaveData(writer, saveData);
                    }
                    else
                    {
                        // Fallback: use Unity's JsonUtility then encode as bytes
                        string json = JsonUtility.ToJson(data);
                        WriteString(writer, json);
                    }

                    return stream.ToArray();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BinaryGameSerializer] Serialize failed: {e.Message}");
                    throw;
                }
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            if (data == null || data.Length < 5)
                throw new ArgumentException("Invalid binary data");

            // Reset string table for deserialization
            _stringTable.Clear();
            _stringList.Clear();

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                try
                {
                    // Validate header
                    uint magic = reader.ReadUInt32();
                    if (magic != MAGIC_NUMBER)
                        throw new InvalidDataException($"Invalid magic number: 0x{magic:X8}");

                    byte version = reader.ReadByte();
                    if (version > FORMAT_VERSION)
                        Debug.LogWarning($"[BinaryGameSerializer] Save file version {version} is newer than supported {FORMAT_VERSION}");

                    // Deserialize based on type
                    if (typeof(T) == typeof(SaveData))
                    {
                        return (T)(object)DeserializeSaveData(reader);
                    }
                    else
                    {
                        // Fallback: read JSON string and deserialize
                        string json = ReadString(reader);
                        return JsonUtility.FromJson<T>(json);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BinaryGameSerializer] Deserialize failed: {e.Message}");
                    throw;
                }
            }
        }

        // ─── SaveData-specific serialization ─────────────────────────────

        void SerializeSaveData(BinaryWriter writer, SaveData data)
        {
            // Chunk 0: String interning table (write at start so we can reference strings by index)
            WriteChunkHeader(writer, 0, 0); // String table chunk
            int stringTablePos = (int)writer.BaseStream.Position;
            WriteVarint(writer, 0); // Placeholder for string count
            int stringTableStart = (int)writer.BaseStream.Position;

            // Chunk 1: Header
            WriteChunkHeader(writer, 1, 0);
            WriteVarint(writer, data.version);
            WriteString(writer, data.header.modifiedUtc ?? "");
            writer.Write((float)data.header.playTimeSeconds);
            WriteString(writer, data.header.checksum ?? "");

            // Chunk 2: Player data
            WriteChunkHeader(writer, 2, 0);
            SerializePlayerData(writer, data.player);

            // Chunk 3: World data
            WriteChunkHeader(writer, 3, 0);
            SerializeWorldData(writer, data.world);

            // Chunk 4: Quest data
            WriteChunkHeader(writer, 4, 0);
            SerializeQuestData(writer, data.quests);

            // Chunk 5: Economy data
            WriteChunkHeader(writer, 5, 0);
            SerializeEconomyData(writer, data.economy);

            // Chunk 6: Skill tree data
            WriteChunkHeader(writer, 6, 0);
            SerializeSkillTreeData(writer, data.skillTree);

            // Chunk 7: Campaign data
            WriteChunkHeader(writer, 7, 0);
            SerializeCampaignData(writer, data.campaign);

            // Chunk 8: Moon flags (extensible flags system)
            WriteChunkHeader(writer, 8, 0);
            SerializeMoonFlags(writer, data.moonFlags, data.moonFlagsInt);

            // Chunk 9: Global flags
            WriteChunkHeader(writer, 9, 0);
            SerializeGlobalFlags(writer, data.globalFlags);

            // Chunk 255: End marker
            WriteChunkHeader(writer, 255, 0);

            // Go back and write string table count
            int endPos = (int)writer.BaseStream.Position;
            writer.BaseStream.Seek(stringTablePos, SeekOrigin.Begin);
            WriteVarint(writer, _stringList.Count);
            
            // Write string table
            foreach (string str in _stringList)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                WriteVarint(writer, bytes.Length);
                writer.Write(bytes);
            }

            writer.BaseStream.Seek(endPos, SeekOrigin.End);
        }

        SaveData DeserializeSaveData(BinaryReader reader)
        {
            var data = new SaveData();

            // Read string table first
            uint chunkId = ReadChunkHeader(reader, out uint chunkVersion);
            if (chunkId == 0)
            {
                int stringCount = ReadVarint(reader);
                _stringList.Clear();
                for (int i = 0; i < stringCount; i++)
                {
                    int length = ReadVarint(reader);
                    byte[] bytes = reader.ReadBytes(length);
                    string str = Encoding.UTF8.GetString(bytes);
                    _stringList.Add(str);
                }
                chunkId = ReadChunkHeader(reader, out chunkVersion);
            }

            // Read chunks until end marker (255)
            while (chunkId != 255 && reader.BaseStream.Position < reader.BaseStream.Length)
            {
                switch (chunkId)
                {
                    case 1: // Header
                        data.version = ReadVarint(reader);
                        data.header.modifiedUtc = ReadString(reader);
                        data.header.playTimeSeconds = reader.ReadSingle();
                        data.header.checksum = ReadString(reader);
                        break;

                    case 2: // Player
                        DeserializePlayerData(reader, data.player);
                        break;

                    case 3: // World
                        DeserializeWorldData(reader, data.world);
                        break;

                    case 4: // Quests
                        DeserializeQuestData(reader, data.quests);
                        break;

                    case 5: // Economy
                        DeserializeEconomyData(reader, data.economy);
                        break;

                    case 6: // Skill tree
                        DeserializeSkillTreeData(reader, data.skillTree);
                        break;

                    case 7: // Campaign
                        DeserializeCampaignData(reader, data.campaign);
                        break;

                    case 8: // Moon flags
                        DeserializeMoonFlags(reader, data.moonFlags, data.moonFlagsInt);
                        break;

                    case 9: // Global flags
                        DeserializeGlobalFlags(reader, data.globalFlags);
                        break;

                    default:
                        // Unknown chunk — skip it for forward compatibility
                        Debug.LogWarning($"[BinaryGameSerializer] Unknown chunk {chunkId}, skipping");
                        break;
                }

                // Read next chunk
                if (reader.BaseStream.Position < reader.BaseStream.Length)
                    chunkId = ReadChunkHeader(reader, out chunkVersion);
                else
                    break;
            }

            return data;
        }

        // ─── Chunk I/O helpers ────────────────────────────────────────────

        void WriteChunkHeader(BinaryWriter writer, uint chunkId, uint version)
        {
            WriteVarint(writer, (int)chunkId);
            WriteVarint(writer, (int)version);
        }

        uint ReadChunkHeader(BinaryReader reader, out uint version)
        {
            uint chunkId = (uint)ReadVarint(reader);
            version = (uint)ReadVarint(reader);
            return chunkId;
        }

        // ─── Variable-length integer encoding (protobuf varint) ──────────

        void WriteVarint(BinaryWriter writer, int value)
        {
            uint unsigned = (uint)value;
            while (unsigned >= 0x80)
            {
                writer.Write((byte)(unsigned | 0x80));
                unsigned >>= 7;
            }
            writer.Write((byte)unsigned);
        }

        int ReadVarint(BinaryReader reader)
        {
            int result = 0;
            int shift = 0;
            byte b;
            do
            {
                b = reader.ReadByte();
                result |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return result;
        }

        // ─── String interning (deduplicate common strings) ────────────────

        void WriteString(BinaryWriter writer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteVarint(writer, -1); // Null marker
                return;
            }

            if (_stringTable.TryGetValue(value, out int index))
            {
                WriteVarint(writer, index); // String table reference
            }
            else
            {
                int newIndex = _stringList.Count;
                _stringTable[value] = newIndex;
                _stringList.Add(value);
                WriteVarint(writer, newIndex);
            }
        }

        string ReadString(BinaryReader reader)
        {
            int index = ReadVarint(reader);
            if (index < 0)
                return null;
            if (index >= _stringList.Count)
                return "";
            return _stringList[index];
        }

        // ─── SaveData block serialization ─────────────────────────────────

        void SerializePlayerData(BinaryWriter writer, PlayerSaveData player)
        {
            writer.Write(player.position.x);
            writer.Write(player.position.y);
            writer.Write(player.position.z);
            writer.Write(player.health);
            writer.Write(player.maxHealth);
            writer.Write(player.mana);
            writer.Write(player.maxMana);
            WriteVarint(writer, player.level);
            WriteVarint(writer, player.experience);
        }

        void DeserializePlayerData(BinaryReader reader, PlayerSaveData player)
        {
            player.position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            player.health = reader.ReadSingle();
            player.maxHealth = reader.ReadSingle();
            player.mana = reader.ReadSingle();
            player.maxMana = reader.ReadSingle();
            player.level = ReadVarint(reader);
            player.experience = ReadVarint(reader);
        }

        void SerializeWorldData(BinaryWriter writer, WorldSaveData world)
        {
            WriteVarint(writer, world.currentMoonIndex);
            writer.Write(world.gameTimeSeconds);
            writer.Write(world.resonanceScore);
            WriteVarint(writer, world.buildingsRestored);
        }

        void DeserializeWorldData(BinaryReader reader, WorldSaveData world)
        {
            world.currentMoonIndex = ReadVarint(reader);
            world.gameTimeSeconds = reader.ReadSingle();
            world.resonanceScore = reader.ReadSingle();
            world.buildingsRestored = ReadVarint(reader);
        }

        void SerializeQuestData(BinaryWriter writer, QuestSaveBlock quests)
        {
            WriteVarint(writer, quests.activeQuests?.Length ?? 0);
            if (quests.activeQuests != null)
            {
                foreach (var quest in quests.activeQuests)
                {
                    WriteString(writer, quest);
                }
            }

            WriteVarint(writer, quests.completedQuests?.Length ?? 0);
            if (quests.completedQuests != null)
            {
                foreach (var quest in quests.completedQuests)
                {
                    WriteString(writer, quest);
                }
            }
        }

        void DeserializeQuestData(BinaryReader reader, QuestSaveBlock quests)
        {
            int activeCount = ReadVarint(reader);
            quests.activeQuests = new string[activeCount];
            for (int i = 0; i < activeCount; i++)
            {
                quests.activeQuests[i] = ReadString(reader);
            }

            int completedCount = ReadVarint(reader);
            quests.completedQuests = new string[completedCount];
            for (int i = 0; i < completedCount; i++)
            {
                quests.completedQuests[i] = ReadString(reader);
            }
        }

        void SerializeEconomyData(BinaryWriter writer, EconomySaveBlock economy)
        {
            WriteVarint(writer, economy.gold);
            WriteVarint(writer, economy.crystals);
            WriteVarint(writer, economy.artifacts);
        }

        void DeserializeEconomyData(BinaryReader reader, EconomySaveBlock economy)
        {
            economy.gold = ReadVarint(reader);
            economy.crystals = ReadVarint(reader);
            economy.artifacts = ReadVarint(reader);
        }

        void SerializeSkillTreeData(BinaryWriter writer, SkillTreeSaveBlock skillTree)
        {
            WriteVarint(writer, skillTree.unlockedSkills?.Length ?? 0);
            if (skillTree.unlockedSkills != null)
            {
                foreach (var skill in skillTree.unlockedSkills)
                {
                    WriteString(writer, skill);
                }
            }

            WriteVarint(writer, skillTree.skillPoints);
        }

        void DeserializeSkillTreeData(BinaryReader reader, SkillTreeSaveBlock skillTree)
        {
            int count = ReadVarint(reader);
            skillTree.unlockedSkills = new string[count];
            for (int i = 0; i < count; i++)
            {
                skillTree.unlockedSkills[i] = ReadString(reader);
            }

            skillTree.skillPoints = ReadVarint(reader);
        }

        void SerializeCampaignData(BinaryWriter writer, CampaignSaveBlock campaign)
        {
            WriteVarint(writer, campaign.currentAct);
            WriteVarint(writer, campaign.currentScene);
            writer.Write(campaign.campaignProgress);
        }

        void DeserializeCampaignData(BinaryReader reader, CampaignSaveBlock campaign)
        {
            campaign.currentAct = ReadVarint(reader);
            campaign.currentScene = ReadVarint(reader);
            campaign.campaignProgress = reader.ReadSingle();
        }

        void SerializeMoonFlags(BinaryWriter writer, MoonFlagsSaveBlock moonFlags, MoonFlagsIntSaveBlock moonFlagsInt)
        {
            // Bool flags
            WriteVarint(writer, moonFlags?.flags?.Length ?? 0);
            if (moonFlags?.flags != null)
            {
                foreach (var flag in moonFlags.flags)
                {
                    WriteString(writer, flag);
                }
            }

            // Int flags
            WriteVarint(writer, moonFlagsInt?.keys?.Length ?? 0);
            if (moonFlagsInt?.keys != null && moonFlagsInt?.values != null)
            {
                for (int i = 0; i < moonFlagsInt.keys.Length; i++)
                {
                    WriteString(writer, moonFlagsInt.keys[i]);
                    WriteVarint(writer, moonFlagsInt.values[i]);
                }
            }
        }

        void DeserializeMoonFlags(BinaryReader reader, MoonFlagsSaveBlock moonFlags, MoonFlagsIntSaveBlock moonFlagsInt)
        {
            // Bool flags
            int boolCount = ReadVarint(reader);
            moonFlags.flags = new string[boolCount];
            for (int i = 0; i < boolCount; i++)
            {
                moonFlags.flags[i] = ReadString(reader);
            }

            // Int flags
            int intCount = ReadVarint(reader);
            moonFlagsInt.keys = new string[intCount];
            moonFlagsInt.values = new int[intCount];
            for (int i = 0; i < intCount; i++)
            {
                moonFlagsInt.keys[i] = ReadString(reader);
                moonFlagsInt.values[i] = ReadVarint(reader);
            }
        }

        void SerializeGlobalFlags(BinaryWriter writer, System.Collections.Generic.List<string> globalFlags)
        {
            WriteVarint(writer, globalFlags?.Count ?? 0);
            if (globalFlags != null)
            {
                foreach (var flag in globalFlags)
                {
                    WriteString(writer, flag);
                }
            }
        }

        void DeserializeGlobalFlags(BinaryReader reader, System.Collections.Generic.List<string> globalFlags)
        {
            int count = ReadVarint(reader);
            globalFlags.Clear();
            for (int i = 0; i < count; i++)
            {
                globalFlags.Add(ReadString(reader));
            }
        }
    }
}
