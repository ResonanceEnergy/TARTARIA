using System;

namespace Tartaria.Security
{
    /// <summary>
    /// Save Encryption Helper — stub for save file encryption/decryption.
    /// TODO: Implement actual AES encryption when needed.
    /// </summary>
    public static class SaveEncryptionHelper
    {
        /// <summary>
        /// Check if save data is encrypted.
        /// Current stub: always returns false (no encryption).
        /// </summary>
        public static bool IsEncrypted(byte[] saveData)
        {
            // Stub: no encryption implemented yet
            return false;
        }

        /// <summary>
        /// Decrypt save data.
        /// Current stub: returns data unchanged.
        /// </summary>
        public static byte[] Decrypt(byte[] encryptedData)
        {
            // Stub: passthrough (no decryption)
            return encryptedData;
        }

        /// <summary>
        /// Encrypt save data.
        /// Current stub: returns data unchanged.
        /// </summary>
        public static byte[] Encrypt(byte[] plainData)
        {
            // Stub: passthrough (no encryption)
            return plainData;
        }
    }
}
