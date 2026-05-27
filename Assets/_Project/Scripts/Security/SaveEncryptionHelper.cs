using System;
using TartariaSave = Tartaria.Save;

namespace Tartaria.Security
{
    /// <summary>
    /// Save Encryption Helper — DEPRECATED STUB (redirect to Tartaria.Save.SaveEncryptionHelper)
    ///
    /// Agent 9: This stub redirects to the real implementation in Tartaria.Save namespace.
    /// The actual AES-256-CBC encryption + HMAC-SHA256 integrity validation is in:
    ///   Assets/_Project/Scripts/Save/SaveEncryptionHelper.cs
    ///
    /// This stub exists for backward compatibility only.
    /// </summary>
    [Obsolete("Use Tartaria.Save.SaveEncryptionHelper instead", false)]
    public static class SaveEncryptionHelper
    {
        /// <summary>
        /// Check if save data is encrypted.
        /// Redirects to Tartaria.Save.SaveEncryptionHelper.IsEncrypted()
        /// </summary>
        public static bool IsEncrypted(byte[] saveData)
        {
            return TartariaSave.SaveEncryptionHelper.IsEncrypted(saveData);
        }

        /// <summary>
        /// Decrypt save data.
        /// Redirects to Tartaria.Save.SaveEncryptionHelper.Decrypt()
        /// </summary>
        public static byte[] Decrypt(byte[] encryptedData)
        {
            return TartariaSave.SaveEncryptionHelper.Decrypt(encryptedData);
        }

        /// <summary>
        /// Encrypt save data.
        /// Redirects to Tartaria.Save.SaveEncryptionHelper.Encrypt()
        /// </summary>
        public static byte[] Encrypt(byte[] plainData)
        {
            return TartariaSave.SaveEncryptionHelper.Encrypt(plainData);
        }

        /// <summary>
        /// Validate data integrity using HMAC-SHA256.
        /// Redirects to Tartaria.Save.SaveEncryptionHelper.ValidateIntegrity()
        /// </summary>
        public static byte[] ValidateIntegrity(byte[] data)
        {
            return TartariaSave.SaveEncryptionHelper.ValidateIntegrity(data);
        }
    }
}
