using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Tartaria.Save
{
    /// <summary>
    /// Save Encryption Helper — AES-256 encryption for save files.
    ///
    /// SECURITY IMPLEMENTATION (Agent 8):
    ///   - AES-256-CBC encryption with random IV per save
    ///   - Key derived from Unity Application.identifier + salt
    ///   - Prevents save editing/cheating
    ///   - Backward compatible: detects unencrypted saves and migrates
    ///
    /// Agent 9 added encryption TODOs but no implementation — this fills the gap.
    /// </summary>
    public static class SaveEncryptionHelper
    {
        // SECURITY: Change this salt for your game (hardcode or generate at build time)
        const string SALT = "TARTARIA_SAVE_ENCRYPTION_V1_2026";

        // SECURITY: Encryption key derived from app identifier + salt
        // This makes saves specific to this game instance
        static byte[] DeriveKey()
        {
            string source = Application.identifier + SALT;
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(source));
        }

        /// <summary>
        /// Encrypts save data bytes using AES-256-CBC.
        /// Returns: [IV (16 bytes)] + [Encrypted Data]
        /// </summary>
        public static byte[] Encrypt(byte[] plaintext)
        {
            if (plaintext == null || plaintext.Length == 0)
            {
                Debug.LogWarning("[SaveEncryption] Empty plaintext, returning as-is");
                return plaintext;
            }

            try
            {
                using var aes = Aes.Create();
                aes.Key = DeriveKey();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV(); // Random IV per save

                using var encryptor = aes.CreateEncryptor();
                using var ms = new MemoryStream();

                // Write IV first (needed for decryption)
                ms.Write(aes.IV, 0, aes.IV.Length);

                // Write encrypted data
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(plaintext, 0, plaintext.Length);
                    cs.FlushFinalBlock();
                }

                byte[] result = ms.ToArray();
                Debug.Log($"[SaveEncryption] Encrypted {plaintext.Length} bytes -> {result.Length} bytes");
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveEncryption] Encryption failed: {e.Message}");
                return plaintext; // Fallback: return unencrypted
            }
        }

        /// <summary>
        /// Decrypts save data bytes using AES-256-CBC.
        /// Expects: [IV (16 bytes)] + [Encrypted Data]
        /// </summary>
        public static byte[] Decrypt(byte[] ciphertext)
        {
            if (ciphertext == null || ciphertext.Length == 0)
            {
                Debug.LogWarning("[SaveEncryption] Empty ciphertext, returning as-is");
                return ciphertext;
            }

            // Check if data is actually encrypted (has IV prefix)
            if (ciphertext.Length < 16)
            {
                Debug.LogWarning("[SaveEncryption] Data too short to be encrypted, assuming plaintext");
                return ciphertext; // Backward compat: unencrypted save
            }

            try
            {
                using var aes = Aes.Create();
                aes.Key = DeriveKey();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Extract IV from first 16 bytes
                byte[] iv = new byte[16];
                Array.Copy(ciphertext, 0, iv, 0, 16);
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(ciphertext, 16, ciphertext.Length - 16);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var output = new MemoryStream();

                cs.CopyTo(output);
                byte[] result = output.ToArray();

                Debug.Log($"[SaveEncryption] Decrypted {ciphertext.Length} bytes -> {result.Length} bytes");
                return result;
            }
            catch (CryptographicException)
            {
                // Decryption failed — likely an unencrypted save (backward compat)
                Debug.LogWarning("[SaveEncryption] Decryption failed, assuming plaintext (backward compat)");
                return ciphertext;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveEncryption] Decryption error: {e.Message}");
                return ciphertext; // Fallback: return as-is
            }
        }

        /// <summary>
        /// Detects if data is encrypted (has valid IV + encrypted payload).
        /// Used for backward compatibility.
        /// </summary>
        public static bool IsEncrypted(byte[] data)
        {
            if (data == null || data.Length < 16)
                return false;

            // Heuristic: encrypted data won't have readable JSON markers
            try
            {
                string text = Encoding.UTF8.GetString(data, 0, Math.Min(50, data.Length));
                return !text.Contains("{") && !text.Contains("\"version\"");
            }
            catch
            {
                return true; // Binary data, likely encrypted
            }
        }

        /// <summary>
        /// Validates data integrity using HMAC-SHA256.
        /// Returns: [HMAC (32 bytes)] + [Data]
        ///
        /// Agent 9: HMAC-SHA256 integrity check to detect tampering.
        /// Call this AFTER encryption to protect both data and ciphertext.
        /// </summary>
        public static byte[] ComputeIntegrityTag(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                Debug.LogWarning("[SaveEncryption] Empty data for HMAC, returning as-is");
                return data;
            }

            try
            {
                using var hmac = new HMACSHA256(DeriveKey());
                byte[] hash = hmac.ComputeHash(data);

                // Prepend HMAC to data: [HMAC][DATA]
                byte[] result = new byte[hash.Length + data.Length];
                Array.Copy(hash, 0, result, 0, hash.Length);
                Array.Copy(data, 0, result, hash.Length, data.Length);

                Debug.Log($"[SaveEncryption] Added HMAC tag ({hash.Length} bytes) to {data.Length} bytes");
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveEncryption] HMAC computation failed: {e.Message}");
                return data; // Fallback
            }
        }

        /// <summary>
        /// Validates HMAC-SHA256 integrity tag and strips it.
        /// Returns: data without HMAC (or null if validation fails)
        ///
        /// Agent 9: Detects tampering before decryption.
        /// Returns null if HMAC validation fails (tampered save).
        /// </summary>
        public static byte[] ValidateIntegrity(byte[] dataWithHmac)
        {
            if (dataWithHmac == null || dataWithHmac.Length < 32)
            {
                Debug.LogWarning("[SaveEncryption] Data too short for HMAC validation");
                return dataWithHmac; // Backward compat: no HMAC tag
            }

            try
            {
                // Extract HMAC and data
                byte[] receivedHmac = new byte[32];
                Array.Copy(dataWithHmac, 0, receivedHmac, 0, 32);

                byte[] data = new byte[dataWithHmac.Length - 32];
                Array.Copy(dataWithHmac, 32, data, 0, data.Length);

                // Recompute HMAC
                using var hmac = new HMACSHA256(DeriveKey());
                byte[] computedHmac = hmac.ComputeHash(data);

                // Constant-time comparison (prevent timing attacks)
                bool valid = receivedHmac.Length == computedHmac.Length;
                for (int i = 0; i < receivedHmac.Length && i < computedHmac.Length; i++)
                {
                    valid &= receivedHmac[i] == computedHmac[i];
                }

                if (valid)
                {
                    Debug.Log($"[SaveEncryption] HMAC validation passed ({data.Length} bytes)");
                    return data;
                }
                else
                {
                    Debug.LogError("[SaveEncryption] HMAC validation FAILED — save file tampered!");
                    return null; // TAMPERED
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveEncryption] HMAC validation error: {e.Message}");
                return dataWithHmac; // Fallback: assume unprotected save
            }
        }

        /// <summary>
        /// Full encryption pipeline: Encrypt → HMAC tag.
        /// Use this for saving data with both confidentiality and integrity.
        /// </summary>
        public static byte[] EncryptAndProtect(byte[] plaintext)
        {
            byte[] encrypted = Encrypt(plaintext);
            return ComputeIntegrityTag(encrypted);
        }

        /// <summary>
        /// Full decryption pipeline: Validate HMAC → Decrypt.
        /// Returns null if integrity check fails (tampered save).
        /// </summary>
        public static byte[] ValidateAndDecrypt(byte[] protectedData)
        {
            byte[] validated = ValidateIntegrity(protectedData);
            if (validated == null)
            {
                Debug.LogError("[SaveEncryption] Cannot decrypt — integrity validation failed");
                return null; // TAMPERED
            }
            return Decrypt(validated);
        }
    }
}
