using System;
using System.Text;
using UnityEngine;

namespace Tartaria.Save.Serialization
{
    /// <summary>
    /// JSON serializer using Unity's JsonUtility.
    /// Human-readable, good for debug builds and version control diffs.
    /// Performance: ~150ms for 500KB save, high GC allocations.
    /// </summary>
    public class JsonGameSerializer : IGameSerializer
    {
        public string Name => "JSON";
        public bool IsHumanReadable => true;

        public byte[] Serialize<T>(T data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            try
            {
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                return Encoding.UTF8.GetBytes(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonGameSerializer] Serialize failed: {e.Message}");
                throw;
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Cannot deserialize null or empty data");

            try
            {
                string json = Encoding.UTF8.GetString(data);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonGameSerializer] Deserialize failed: {e.Message}");
                throw;
            }
        }
    }
}
