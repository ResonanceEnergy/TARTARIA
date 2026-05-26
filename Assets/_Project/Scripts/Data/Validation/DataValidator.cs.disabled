using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Data.Validation
{
    /// <summary>
    /// Central validation utility class with common validation rules.
    /// Provides reusable validation methods for all data types.
    /// </summary>
    public static class DataValidator
    {
        // ─── Common Validation Rules ───────────────────────────────────────

        /// <summary>
        /// Validates that a string ID is not null or empty.
        /// </summary>
        public static ValidationResult ValidateID(string id, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return ValidationResult.Error(
                    $"{fieldName} is null or empty",
                    "ID fields are required for data lookups",
                    $"Assign a unique identifier to {fieldName}"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that an ID follows naming conventions (lowercase, underscores).
        /// </summary>
        public static ValidationResult ValidateIDFormat(string id, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null; // Handled by ValidateID

            if (!System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-z0-9_]+$"))
            {
                return ValidationResult.Warning(
                    $"{fieldName} '{id}' does not follow naming convention",
                    "IDs should be lowercase with underscores only",
                    $"Rename to match pattern: 'item_name_here'"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that a display name is not empty.
        /// </summary>
        public static ValidationResult ValidateDisplayName(string displayName, string fieldName = "displayName")
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return ValidationResult.Warning(
                    $"{fieldName} is empty",
                    "Display names improve readability in UI",
                    $"Add a human-readable name"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that a numeric value is positive.
        /// </summary>
        public static ValidationResult ValidatePositive(float value, string fieldName)
        {
            if (value <= 0)
            {
                return ValidationResult.Error(
                    $"{fieldName} must be greater than 0 (current: {value})",
                    "Zero or negative values will cause runtime errors",
                    $"Set {fieldName} to a positive value"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that a numeric value is non-negative.
        /// </summary>
        public static ValidationResult ValidateNonNegative(float value, string fieldName)
        {
            if (value < 0)
            {
                return ValidationResult.Error(
                    $"{fieldName} cannot be negative (current: {value})",
                    "Negative values will cause runtime errors",
                    $"Set {fieldName} to 0 or higher"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that an asset reference is not null.
        /// </summary>
        public static ValidationResult ValidateAssetReference<T>(T asset, string fieldName) where T : Object
        {
            if (asset == null)
            {
                return ValidationResult.Error(
                    $"{fieldName} is null",
                    "Null asset references will cause NullReferenceException at runtime",
                    $"Assign a valid {typeof(T).Name} to {fieldName}"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that an optional asset reference is assigned (warning if null).
        /// </summary>
        public static ValidationResult ValidateOptionalAsset<T>(T asset, string fieldName) where T : Object
        {
            if (asset == null)
            {
                return ValidationResult.Warning(
                    $"{fieldName} is not assigned",
                    "This field is optional but recommended",
                    $"Consider assigning a {typeof(T).Name} to {fieldName}"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that an array is not null or empty.
        /// </summary>
        public static ValidationResult ValidateArrayNotEmpty<T>(T[] array, string fieldName)
        {
            if (array == null || array.Length == 0)
            {
                return ValidationResult.Error(
                    $"{fieldName} is empty",
                    "At least one entry is required",
                    $"Add entries to {fieldName}"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that a value is within a specified range.
        /// </summary>
        public static ValidationResult ValidateRange(float value, float min, float max, string fieldName)
        {
            if (value < min || value > max)
            {
                return ValidationResult.Error(
                    $"{fieldName} is out of range: {value} (expected {min}-{max})",
                    "Value must be within acceptable bounds",
                    $"Set {fieldName} between {min} and {max}"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that an enum value is defined.
        /// </summary>
        public static ValidationResult ValidateEnum<T>(T enumValue, string fieldName) where T : System.Enum
        {
            if (!System.Enum.IsDefined(typeof(T), enumValue))
            {
                return ValidationResult.Error(
                    $"{fieldName} has invalid enum value: {enumValue}",
                    "Undefined enum values cause unpredictable behavior",
                    $"Set {fieldName} to a valid {typeof(T).Name} value"
                );
            }
            return null;
        }

        // ─── Helper Methods ───────────────────────────────────────────────

        /// <summary>
        /// Adds a validation result to the list if it's not null.
        /// </summary>
        public static void AddIfNotNull(List<ValidationResult> results, ValidationResult result)
        {
            if (result != null)
                results.Add(result);
        }

        /// <summary>
        /// Checks if validation results contain any errors.
        /// </summary>
        public static bool HasErrors(List<ValidationResult> results)
        {
            foreach (var result in results)
            {
                if (result.Level == ValidationLevel.Error)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets count of errors in validation results.
        /// </summary>
        public static int GetErrorCount(List<ValidationResult> results)
        {
            int count = 0;
            foreach (var result in results)
            {
                if (result.Level == ValidationLevel.Error)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Gets count of warnings in validation results.
        /// </summary>
        public static int GetWarningCount(List<ValidationResult> results)
        {
            int count = 0;
            foreach (var result in results)
            {
                if (result.Level == ValidationLevel.Warning)
                    count++;
            }
            return count;
        }
    }
}
