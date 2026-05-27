using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Tartaria.Core.Validation;
using static Tartaria.Core.Validation.ValidationResult;

namespace Tartaria.Editor.Tests
{
    /// <summary>
    /// Manual test runner for DataValidator production implementation (Phase 10).
    /// Run via Window > TARTARIA > Test DataValidator
    /// </summary>
    public static class DataValidatorTests
    {
        [MenuItem("Window/TARTARIA/Test DataValidator")]
        public static void RunTests()
        {
            Debug.Log("=== DataValidator Test Suite Started ===");

            int passCount = 0;
            int failCount = 0;

            // Test 1: ValidateNonEmpty
            Debug.Log("\n--- Testing ValidateNonEmpty ---");
            passCount += TestValidateNonEmpty_Pass();
            failCount += TestValidateNonEmpty_Fail();

            // Test 2: ValidateNonNegative (int)
            Debug.Log("\n--- Testing ValidateNonNegative (int) ---");
            passCount += TestValidateNonNegativeInt_Pass();
            failCount += TestValidateNonNegativeInt_Fail();

            // Test 3: ValidateNonNegative (float)
            Debug.Log("\n--- Testing ValidateNonNegative (float) ---");
            passCount += TestValidateNonNegativeFloat_Pass();
            failCount += TestValidateNonNegativeFloat_Fail();

            // Test 4: ValidateRange (int)
            Debug.Log("\n--- Testing ValidateRange (int) ---");
            passCount += TestValidateRangeInt_Pass();
            failCount += TestValidateRangeInt_Fail();

            // Test 5: ValidateRange (float)
            Debug.Log("\n--- Testing ValidateRange (float) ---");
            passCount += TestValidateRangeFloat_Pass();
            failCount += TestValidateRangeFloat_Fail();

            // Test 6: ValidateUnique
            Debug.Log("\n--- Testing ValidateUnique ---");
            passCount += TestValidateUnique_Pass();
            failCount += TestValidateUnique_Fail();

            // Test 7: ValidateEnumDefined
            Debug.Log("\n--- Testing ValidateEnumDefined ---");
            passCount += TestValidateEnumDefined_Pass();
            failCount += TestValidateEnumDefined_Fail();

            // Test 8: ValidateAssetReference
            Debug.Log("\n--- Testing ValidateAssetReference ---");
            passCount += TestValidateAssetReference_Pass();
            failCount += TestValidateAssetReference_Fail();

            // Summary
            Debug.Log($"\n=== DataValidator Test Suite Complete ===");
            Debug.Log($"PASSED: {passCount} | FAILED: {failCount}");

            if (failCount == 0)
            {
                Debug.Log("<color=green>✓ ALL TESTS PASSED - DataValidator is GREEN</color>");
            }
            else
            {
                Debug.LogError($"✗ {failCount} TESTS FAILED - Review errors above");
            }
        }

        // ============================================================
        // ValidateNonEmpty Tests
        // ============================================================

        private static int TestValidateNonEmpty_Pass()
        {
            var result = DataValidator.ValidateNonEmpty("valid_string", "testField");
            if (result == null)
            {
                Debug.Log("✓ ValidateNonEmpty: Valid string passes");
                return 1;
            }
            Debug.LogError("✗ ValidateNonEmpty: Valid string should pass (returned error)");
            return 0;
        }

        private static int TestValidateNonEmpty_Fail()
        {
            var result1 = DataValidator.ValidateNonEmpty(null, "testField");
            var result2 = DataValidator.ValidateNonEmpty("", "testField");
            var result3 = DataValidator.ValidateNonEmpty("   ", "testField");

            if (result1 != null && result2 != null && result3 != null)
            {
                Debug.Log("✓ ValidateNonEmpty: Null/empty/whitespace fails as expected");
                return 1;
            }
            Debug.LogError("✗ ValidateNonEmpty: Should reject null/empty/whitespace");
            return 0;
        }

        // ============================================================
        // ValidateNonNegative Tests
        // ============================================================

        private static int TestValidateNonNegativeInt_Pass()
        {
            var result1 = DataValidator.ValidateNonNegative(0, "testField");
            var result2 = DataValidator.ValidateNonNegative(10, "testField");

            if (result1 == null && result2 == null)
            {
                Debug.Log("✓ ValidateNonNegative(int): Zero and positive pass");
                return 1;
            }
            Debug.LogError("✗ ValidateNonNegative(int): Should accept zero and positive");
            return 0;
        }

        private static int TestValidateNonNegativeInt_Fail()
        {
            var result = DataValidator.ValidateNonNegative(-5, "testField");

            if (result != null && result.Level == Severity.Error)
            {
                Debug.Log("✓ ValidateNonNegative(int): Negative fails as expected");
                return 1;
            }
            Debug.LogError("✗ ValidateNonNegative(int): Should reject negative values");
            return 0;
        }

        private static int TestValidateNonNegativeFloat_Pass()
        {
            var result1 = DataValidator.ValidateNonNegative(0f, "testField");
            var result2 = DataValidator.ValidateNonNegative(10.5f, "testField");

            if (result1 == null && result2 == null)
            {
                Debug.Log("✓ ValidateNonNegative(float): Zero and positive pass");
                return 1;
            }
            Debug.LogError("✗ ValidateNonNegative(float): Should accept zero and positive");
            return 0;
        }

        private static int TestValidateNonNegativeFloat_Fail()
        {
            var result = DataValidator.ValidateNonNegative(-5.5f, "testField");

            if (result != null && result.Level == Severity.Error)
            {
                Debug.Log("✓ ValidateNonNegative(float): Negative fails as expected");
                return 1;
            }
            Debug.LogError("✗ ValidateNonNegative(float): Should reject negative values");
            return 0;
        }

        // ============================================================
        // ValidateRange Tests
        // ============================================================

        private static int TestValidateRangeInt_Pass()
        {
            var result1 = DataValidator.ValidateRange(5, 1, 10, "testField");
            var result2 = DataValidator.ValidateRange(1, 1, 10, "testField"); // min edge
            var result3 = DataValidator.ValidateRange(10, 1, 10, "testField"); // max edge

            if (result1 == null && result2 == null && result3 == null)
            {
                Debug.Log("✓ ValidateRange(int): Values within range pass");
                return 1;
            }
            Debug.LogError("✗ ValidateRange(int): Should accept values within range");
            return 0;
        }

        private static int TestValidateRangeInt_Fail()
        {
            var result1 = DataValidator.ValidateRange(0, 1, 10, "testField");
            var result2 = DataValidator.ValidateRange(11, 1, 10, "testField");

            if (result1 != null && result2 != null)
            {
                Debug.Log("✓ ValidateRange(int): Out-of-range values fail as expected");
                return 1;
            }
            Debug.LogError("✗ ValidateRange(int): Should reject out-of-range values");
            return 0;
        }

        private static int TestValidateRangeFloat_Pass()
        {
            var result = DataValidator.ValidateRange(5.5f, 1.0f, 10.0f, "testField");

            if (result == null)
            {
                Debug.Log("✓ ValidateRange(float): Values within range pass");
                return 1;
            }
            Debug.LogError("✗ ValidateRange(float): Should accept values within range");
            return 0;
        }

        private static int TestValidateRangeFloat_Fail()
        {
            var result1 = DataValidator.ValidateRange(0.5f, 1.0f, 10.0f, "testField");
            var result2 = DataValidator.ValidateRange(10.5f, 1.0f, 10.0f, "testField");

            if (result1 != null && result2 != null)
            {
                Debug.Log("✓ ValidateRange(float): Out-of-range values fail as expected");
                return 1;
            }
            Debug.LogError("✗ ValidateRange(float): Should reject out-of-range values");
            return 0;
        }

        // ============================================================
        // ValidateUnique Tests
        // ============================================================

        private static int TestValidateUnique_Pass()
        {
            var collection = new List<string> { "a", "b", "c" };
            var result = DataValidator.ValidateUnique(collection, "testField");

            if (result == null)
            {
                Debug.Log("✓ ValidateUnique: Unique collection passes");
                return 1;
            }
            Debug.LogError("✗ ValidateUnique: Should accept unique collections");
            return 0;
        }

        private static int TestValidateUnique_Fail()
        {
            var collection = new List<string> { "a", "b", "a", "c" };
            var result = DataValidator.ValidateUnique(collection, "testField");

            if (result != null && result.Message.Contains("duplicate"))
            {
                Debug.Log("✓ ValidateUnique: Duplicate collection fails as expected");
                return 1;
            }
            Debug.LogError("✗ ValidateUnique: Should reject collections with duplicates");
            return 0;
        }

        // ============================================================
        // ValidateEnumDefined Tests
        // ============================================================

        private enum TestEnum { Value1, Value2, Value3 }

        private static int TestValidateEnumDefined_Pass()
        {
            var result = DataValidator.ValidateEnumDefined(TestEnum.Value1, "testField");

            if (result == null)
            {
                Debug.Log("✓ ValidateEnumDefined: Defined enum value passes");
                return 1;
            }
            Debug.LogError("✗ ValidateEnumDefined: Should accept defined enum values");
            return 0;
        }

        private static int TestValidateEnumDefined_Fail()
        {
            var invalidEnum = (TestEnum)999;
            var result = DataValidator.ValidateEnumDefined(invalidEnum, "testField");

            if (result != null)
            {
                Debug.Log("✓ ValidateEnumDefined: Undefined enum value fails as expected");
                return 1;
            }
            Debug.LogError("✗ ValidateEnumDefined: Should reject undefined enum values");
            return 0;
        }

        // ============================================================
        // ValidateAssetReference Tests
        // ============================================================

        private static int TestValidateAssetReference_Pass()
        {
            // Create a temporary ScriptableObject for testing
            var testAsset = ScriptableObject.CreateInstance<ScriptableObject>();
            var result = DataValidator.ValidateAssetReference(testAsset, "testField");
            Object.DestroyImmediate(testAsset);

            if (result == null)
            {
                Debug.Log("✓ ValidateAssetReference: Valid asset reference passes");
                return 1;
            }
            Debug.LogError("✗ ValidateAssetReference: Should accept valid asset references");
            return 0;
        }

        private static int TestValidateAssetReference_Fail()
        {
            var result = DataValidator.ValidateAssetReference(null, "testField");

            if (result != null && result.Level == Severity.Error)
            {
                Debug.Log("✓ ValidateAssetReference: Null reference fails as expected");
                return 1;
            }
            Debug.LogError("✗ ValidateAssetReference: Should reject null asset references");
            return 0;
        }
    }
}
