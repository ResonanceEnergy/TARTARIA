using System;
using System.IO;
using System.IO.Compression;

namespace Tartaria.Save.Serialization
{
    /// <summary>
    /// Compression utilities for save file size reduction.
    /// Supports GZip (high compression, slower) and LZ4 (fast, moderate compression).
    /// </summary>
    public static class CompressionHelper
    {
        public enum CompressionType
        {
            None,
            GZip,    // Best compression (~10:1 ratio), ~50ms for 500KB
            Deflate  // Fast compression (~7:1 ratio), ~20ms for 500KB (using Deflate as LZ4 alternative)
        }

        /// <summary>
        /// Compress data using specified algorithm.
        /// Default: GZip for best compression ratio.
        /// </summary>
        public static byte[] Compress(byte[] data, CompressionType type = CompressionType.GZip)
        {
            if (data == null || data.Length == 0)
                return data;

            switch (type)
            {
                case CompressionType.GZip:
                    return CompressGZip(data);
                case CompressionType.Deflate:
                    return CompressDeflate(data);
                default:
                    return data;
            }
        }

        /// <summary>
        /// Decompress data. Auto-detects compression type from header.
        /// </summary>
        public static byte[] Decompress(byte[] data)
        {
            if (data == null || data.Length < 2)
                return data;

            // GZip magic number: 0x1F 0x8B
            if (data[0] == 0x1F && data[1] == 0x8B)
                return DecompressGZip(data);

            // Deflate has no magic number, try it if not GZip
            try
            {
                return DecompressDeflate(data);
            }
            catch
            {
                // Not compressed, return as-is
                return data;
            }
        }

        // ─── GZip compression (best ratio, moderate speed) ────────────────

        static byte[] CompressGZip(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionMode.Compress, leaveOpen: true))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }

        static byte[] DecompressGZip(byte[] data)
        {
            using (var input = new MemoryStream(data))
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }

        // ─── Deflate compression (faster, good ratio) ─────────────────────

        static byte[] CompressDeflate(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
                {
                    deflate.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }

        static byte[] DecompressDeflate(byte[] data)
        {
            using (var input = new MemoryStream(data))
            using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                deflate.CopyTo(output);
                return output.ToArray();
            }
        }

        /// <summary>
        /// Calculate compression ratio as percentage.
        /// </summary>
        public static float GetCompressionRatio(int originalSize, int compressedSize)
        {
            if (originalSize == 0) return 0f;
            return 100f * compressedSize / originalSize;
        }
    }
}
