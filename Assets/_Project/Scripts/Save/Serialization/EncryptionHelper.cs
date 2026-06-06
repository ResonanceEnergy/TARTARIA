using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Tartaria.Save.Serialization
{
    /// <summary>
    /// Encryption layer for save file security.
    /// Features:
    ///   - AES-256 encryption
    ///   - Key derivation from device ID + salt (PBKDF2)
    ///   - Integrity check (HMAC-SHA256)
    ///   - Prevents save file editing / cheating
    /// 
    /// Format:
    ///   [16 bytes: Salt]
    ///   [16 bytes: IV]
    ///   [32 bytes: HMAC]
    ///   [N bytes: Encrypted data]
    /// </summary>
    public static class EncryptionHelper
    {
        // Salt for key derivation (change this for your game)
        const string GAME_SALT = "TARTARIA_SAVE_ENCRYPTION_v1";
        const int KEY_SIZE = 256;
        const int ITERATIONS = 10000; // PBKDF2 iterations

        /// <summary>
        /// Encrypt data using AES-256 with device-specific key.
        /// </summary>
        public static byte[] Encrypt(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Cannot encrypt null or empty data");

            try
            {
                // Generate encryption key from device ID
                byte[] key = DeriveKey();

                using (var aes = Aes.Create())
                {
                    aes.KeySize = KEY_SIZE;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.GenerateIV();

                    byte[] iv = aes.IV;

                    // Encrypt data
                    byte[] encrypted;
                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(data, 0, data.Length);
                        }
                        encrypted = ms.ToArray();
                    }

                    // Compute HMAC for integrity
                    byte[] hmac = ComputeHMAC(encrypted, key);

                    // Build final format: [Salt][IV][HMAC][Encrypted Data]
                    byte[] salt = Encoding.UTF8.GetBytes(GAME_SALT);
                    using (var output = new MemoryStream())
                    {
                        output.Write(salt, 0, Math.Min(salt.Length, 16));
                        if (salt.Length < 16)
                            output.Write(new byte[16 - salt.Length], 0, 16 - salt.Length);
                        
                        output.Write(iv, 0, iv.Length);
                        output.Write(hmac, 0, hmac.Length);
                        output.Write(encrypted, 0, encrypted.Length);
                        return output.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[EncryptionHelper] Encryption failed: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Decrypt data using AES-256 with device-specific key.
        /// </summary>
        public static byte[] Decrypt(byte[] data)
        {
            if (data == null || data.Length < 64)
                throw new ArgumentException("Invalid encrypted data");

            try
            {
                // Parse format: [Salt][IV][HMAC][Encrypted Data]
                byte[] salt = new byte[16];
                byte[] iv = new byte[16];
                byte[] hmac = new byte[32];
                
                Array.Copy(data, 0, salt, 0, 16);
                Array.Copy(data, 16, iv, 0, 16);
                Array.Copy(data, 32, hmac, 0, 32);
                
                byte[] encrypted = new byte[data.Length - 64];
                Array.Copy(data, 64, encrypted, 0, encrypted.Length);

                // Generate decryption key
                byte[] key = DeriveKey();

                // Verify HMAC
                byte[] computedHmac = ComputeHMAC(encrypted, key);
                if (!HmacEquals(hmac, computedHmac))
                {
                    throw new CryptographicException("Save file integrity check failed - file may be corrupted or tampered");
                }

                // Decrypt data
                using (var aes = Aes.Create())
                {
                    aes.KeySize = KEY_SIZE;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(encrypted))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var output = new MemoryStream())
                    {
                        cs.CopyTo(output);
                        return output.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[EncryptionHelper] Decryption failed: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if data is encrypted (has valid header).
        /// </summary>
        public static bool IsEncrypted(byte[] data)
        {
            if (data == null || data.Length < 64)
                return false;

            // Check if first 16 bytes match salt prefix
            byte[] salt = Encoding.UTF8.GetBytes(GAME_SALT);
            int checkLen = Math.Min(salt.Length, 16);
            for (int i = 0; i < checkLen; i++)
            {
                if (data[i] != salt[i])
                    return false;
            }

            return true;
        }

        // ─── Key derivation ───────────────────────────────────────────────

        static byte[] DeriveKey()
        {
            // Use device ID as passphrase (unique per device)
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(deviceId))
                deviceId = "TARTARIA_FALLBACK_KEY"; // Fallback for editor

            byte[] passphrase = Encoding.UTF8.GetBytes(deviceId);
            byte[] salt = Encoding.UTF8.GetBytes(GAME_SALT);

            using (var pbkdf2 = new Rfc2898DeriveBytes(passphrase, salt, ITERATIONS))
            {
                return pbkdf2.GetBytes(KEY_SIZE / 8);
            }
        }

        // ─── HMAC integrity check ─────────────────────────────────────────

        static byte[] ComputeHMAC(byte[] data, byte[] key)
        {
            using (var hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(data);
            }
        }

        static bool HmacEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}
