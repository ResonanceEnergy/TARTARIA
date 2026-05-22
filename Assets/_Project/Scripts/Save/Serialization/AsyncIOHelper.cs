using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Tartaria.Save.Serialization
{
    /// <summary>
    /// Async I/O wrapper for non-blocking save/load operations.
    /// Offloads serialization and file I/O to background threads to avoid frame drops.
    /// 
    /// Usage:
    ///   await AsyncIOHelper.SaveAsync(serializer, data, path, compress: true, encrypt: true);
    ///   var data = await AsyncIOHelper.LoadAsync&lt;SaveData&gt;(serializer, path, decompress: true, decrypt: true);
    /// </summary>
    public static class AsyncIOHelper
    {
        /// <summary>
        /// Save data asynchronously (non-blocking).
        /// Progress callbacks for UI feedback.
        /// </summary>
        public static async Task SaveAsync<T>(
            IGameSerializer serializer,
            T data,
            string path,
            bool compress = false,
            bool encrypt = false,
            IProgress<float> progress = null)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty");

            try
            {
                progress?.Report(0f);

                // Serialize on background thread
                byte[] serialized = await Task.Run(() => serializer.Serialize(data));
                progress?.Report(0.3f);

                // Compress if requested
                if (compress)
                {
                    serialized = await Task.Run(() => CompressionHelper.Compress(serialized));
                    progress?.Report(0.6f);
                }

                // Encrypt if requested
                if (encrypt)
                {
                    serialized = await Task.Run(() => EncryptionHelper.Encrypt(serialized));
                    progress?.Report(0.8f);
                }

                // Write to disk (background thread)
                await Task.Run(() =>
                {
                    // Safe double-write: temp file → move to target
                    string tempPath = path + ".tmp";
                    string backupPath = path + ".backup";

                    // Write to temp
                    File.WriteAllBytes(tempPath, serialized);

                    // Backup existing file
                    if (File.Exists(path))
                        File.Copy(path, backupPath, overwrite: true);

                    // Move temp to target
                    if (File.Exists(path))
                        File.Delete(path);
                    File.Move(tempPath, path);
                });

                progress?.Report(1f);
                Debug.Log($"[AsyncIOHelper] Save completed: {path} ({serialized.Length / 1024f:F1} KB)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AsyncIOHelper] SaveAsync failed: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load data asynchronously (non-blocking).
        /// Progress callbacks for UI feedback.
        /// </summary>
        public static async Task<T> LoadAsync<T>(
            IGameSerializer serializer,
            string path,
            bool decompress = false,
            bool decrypt = false,
            IProgress<float> progress = null)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty");
            if (!File.Exists(path))
                throw new FileNotFoundException($"Save file not found: {path}");

            try
            {
                progress?.Report(0f);

                // Read from disk (background thread)
                byte[] data = await Task.Run(() => File.ReadAllBytes(path));
                progress?.Report(0.2f);

                // Decrypt if requested
                if (decrypt && EncryptionHelper.IsEncrypted(data))
                {
                    data = await Task.Run(() => EncryptionHelper.Decrypt(data));
                    progress?.Report(0.4f);
                }

                // Decompress if requested
                if (decompress)
                {
                    data = await Task.Run(() => CompressionHelper.Decompress(data));
                    progress?.Report(0.6f);
                }

                // Deserialize on background thread
                T result = await Task.Run(() => serializer.Deserialize<T>(data));
                progress?.Report(1f);

                Debug.Log($"[AsyncIOHelper] Load completed: {path} ({data.Length / 1024f:F1} KB)");
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AsyncIOHelper] LoadAsync failed: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Synchronous save wrapper (for compatibility with existing SaveManager).
        /// </summary>
        public static void SaveSync<T>(
            IGameSerializer serializer,
            T data,
            string path,
            bool compress = false,
            bool encrypt = false)
        {
            // Run async method synchronously
            SaveAsync(serializer, data, path, compress, encrypt).Wait();
        }

        /// <summary>
        /// Synchronous load wrapper (for compatibility with existing SaveManager).
        /// </summary>
        public static T LoadSync<T>(
            IGameSerializer serializer,
            string path,
            bool decompress = false,
            bool decrypt = false)
        {
            // Run async method synchronously
            return LoadAsync<T>(serializer, path, decompress, decrypt).Result;
        }
    }
}
