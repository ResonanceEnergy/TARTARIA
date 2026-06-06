using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Tartaria.Save;

namespace Tartaria.Tests
{
    /// <summary>
    /// SaveEncryptionHelper Tests — validates AES-256-CBC + HMAC-SHA256 implementation.
    ///
    /// Agent 9: Encryption validation suite for mission completion.
    /// </summary>
    [TestFixture]
    public class SaveEncryptionTests
    {
        [Test]
        public void TestEncryptDecryptRoundTrip()
        {
            // Arrange
            string originalText = "This is test save data with unicode: φ ψ ω";
            byte[] originalData = Encoding.UTF8.GetBytes(originalText);

            // Act
            byte[] encrypted = SaveEncryptionHelper.Encrypt(originalData);
            byte[] decrypted = SaveEncryptionHelper.Decrypt(encrypted);
            string decryptedText = Encoding.UTF8.GetString(decrypted);

            // Assert
            Assert.AreNotEqual(originalData, encrypted, "Encrypted data should differ from original");
            Assert.AreEqual(originalText, decryptedText, "Decrypted text should match original");
            Debug.Log($"[Test] Round-trip SUCCESS: {originalData.Length}B → {encrypted.Length}B → {decrypted.Length}B");
        }

        [Test]
        public void TestEncryptionProducesDifferentOutputs()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("Same plaintext");

            // Act
            byte[] encrypted1 = SaveEncryptionHelper.Encrypt(data);
            byte[] encrypted2 = SaveEncryptionHelper.Encrypt(data);

            // Assert
            Assert.AreNotEqual(encrypted1, encrypted2, "Each encryption should use unique IV");
            Debug.Log($"[Test] IV randomness PASS: outputs differ despite same input");
        }

        [Test]
        public void TestIsEncryptedDetection()
        {
            // Arrange
            byte[] plaintext = Encoding.UTF8.GetBytes("{\"version\":1,\"data\":\"test\"}");
            byte[] encrypted = SaveEncryptionHelper.Encrypt(plaintext);

            // Act
            bool plainDetected = SaveEncryptionHelper.IsEncrypted(plaintext);
            bool encryptedDetected = SaveEncryptionHelper.IsEncrypted(encrypted);

            // Assert
            Assert.IsFalse(plainDetected, "JSON plaintext should not be detected as encrypted");
            Assert.IsTrue(encryptedDetected, "Encrypted data should be detected");
            Debug.Log($"[Test] IsEncrypted detection PASS: plaintext={plainDetected}, encrypted={encryptedDetected}");
        }

        [Test]
        public void TestHMACIntegrityValidation()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("Critical save data");

            // Act
            byte[] tagged = SaveEncryptionHelper.ComputeIntegrityTag(data);
            byte[] validated = SaveEncryptionHelper.ValidateIntegrity(tagged);

            // Assert
            Assert.IsNotNull(validated, "Validation should succeed for valid HMAC");
            Assert.AreEqual(data.Length, validated.Length, "Validated data should match original length");
            CollectionAssert.AreEqual(data, validated, "Validated data should match original");
            Debug.Log($"[Test] HMAC validation PASS: {data.Length}B → {tagged.Length}B (with tag) → {validated.Length}B (validated)");
        }

        [Test]
        public void TestHMACDetectsTampering()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("Original data");
            byte[] tagged = SaveEncryptionHelper.ComputeIntegrityTag(data);

            // Act: Tamper with data (flip a bit in the data section, not HMAC)
            byte[] tampered = new byte[tagged.Length];
            Array.Copy(tagged, tampered, tagged.Length);
            tampered[40] ^= 0xFF; // Flip bits in data section

            byte[] validated = SaveEncryptionHelper.ValidateIntegrity(tampered);

            // Assert
            Assert.IsNull(validated, "Validation should fail for tampered data");
            Debug.Log($"[Test] Tampering detection PASS: validation correctly rejected tampered data");
        }

        [Test]
        public void TestFullEncryptAndProtectPipeline()
        {
            // Arrange
            string originalText = "Full pipeline test: encryption + HMAC protection";
            byte[] originalData = Encoding.UTF8.GetBytes(originalText);

            // Act
            byte[] protectedData = SaveEncryptionHelper.EncryptAndProtect(originalData);
            byte[] recovered = SaveEncryptionHelper.ValidateAndDecrypt(protectedData);
            string recoveredText = Encoding.UTF8.GetString(recovered);

            // Assert
            Assert.AreEqual(originalText, recoveredText, "Full pipeline should preserve data");
            Debug.Log($"[Test] Full pipeline PASS: {originalData.Length}B → {protectedData.Length}B → {recovered.Length}B");
        }

        [Test]
        public void TestFullPipelineDetectsTampering()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("Protected data");
            byte[] protectedData = SaveEncryptionHelper.EncryptAndProtect(data);

            // Act: Tamper with protected data
            byte[] tampered = new byte[protectedData.Length];
            Array.Copy(protectedData, tampered, protectedData.Length);
            tampered[50] ^= 0x01; // Flip a bit

            byte[] recovered = SaveEncryptionHelper.ValidateAndDecrypt(tampered);

            // Assert
            Assert.IsNull(recovered, "Full pipeline should reject tampered data");
            Debug.Log($"[Test] Full pipeline tampering detection PASS");
        }

        [Test]
        public void TestBackwardCompatibilityWithUnencryptedSaves()
        {
            // Arrange
            byte[] plaintext = Encoding.UTF8.GetBytes("{\"version\":1}");

            // Act
            byte[] decrypted = SaveEncryptionHelper.Decrypt(plaintext);
            string decryptedText = Encoding.UTF8.GetString(decrypted);

            // Assert
            Assert.AreEqual("{\"version\":1}", decryptedText, "Unencrypted saves should pass through");
            Debug.Log($"[Test] Backward compatibility PASS: unencrypted save handled gracefully");
        }

        [Test]
        public void TestEmptyDataHandling()
        {
            // Arrange
            byte[] empty = new byte[0];

            // Act
            byte[] encrypted = SaveEncryptionHelper.Encrypt(empty);
            byte[] decrypted = SaveEncryptionHelper.Decrypt(encrypted);

            // Assert
            Assert.AreEqual(0, decrypted.Length, "Empty data should remain empty");
            Debug.Log($"[Test] Empty data handling PASS");
        }

        [Test]
        public void TestNullDataHandling()
        {
            // Act
            byte[] encrypted = SaveEncryptionHelper.Encrypt(null);
            byte[] decrypted = SaveEncryptionHelper.Decrypt(null);
            byte[] validated = SaveEncryptionHelper.ValidateIntegrity(null);

            // Assert
            Assert.IsNull(encrypted, "Null input should return null");
            Assert.IsNull(decrypted, "Null input should return null");
            Assert.IsNull(validated, "Null input should return null");
            Debug.Log($"[Test] Null handling PASS: no crashes on null input");
        }

        [Test]
        public void TestLargeDataEncryption()
        {
            // Arrange: 1MB of data
            byte[] largeData = new byte[1024 * 1024];
            new System.Random().NextBytes(largeData);

            // Act
            var startTime = DateTime.Now;
            byte[] encrypted = SaveEncryptionHelper.Encrypt(largeData);
            byte[] decrypted = SaveEncryptionHelper.Decrypt(encrypted);
            var duration = (DateTime.Now - startTime).TotalMilliseconds;

            // Assert
            Assert.AreEqual(largeData.Length, decrypted.Length, "Large data should decrypt to same size");
            CollectionAssert.AreEqual(largeData, decrypted, "Large data should decrypt correctly");
            Debug.Log($"[Test] Large data (1MB) PASS: encrypted+decrypted in {duration:F1}ms");
        }
    }
}
