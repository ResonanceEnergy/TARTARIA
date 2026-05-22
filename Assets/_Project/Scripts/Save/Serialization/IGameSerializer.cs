using System;

namespace Tartaria.Save.Serialization
{
    /// <summary>
    /// Generic game serialization interface.
    /// Supports JSON (human-readable debug), Binary (fast production), and Hybrid (JSON metadata + binary data).
    /// </summary>
    public interface IGameSerializer
    {
        /// <summary>Serialize object to byte array.</summary>
        byte[] Serialize<T>(T data);

        /// <summary>Deserialize byte array to object.</summary>
        T Deserialize<T>(byte[] data);

        /// <summary>Serializer name for logging/debugging.</summary>
        string Name { get; }

        /// <summary>Whether this serializer produces human-readable output.</summary>
        bool IsHumanReadable { get; }
    }
}
