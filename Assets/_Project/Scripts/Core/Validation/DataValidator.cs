using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Tartaria.Core.Validation
{
    /// <summary>
    /// Data validation helper methods.
    /// Production implementation (Phase 10) — full validation enabled.
    /// </summary>
    public static class DataValidator
    {
        public static void AddIfNotNull(List<ValidationResult> results, ValidationResult result)
        {
            if (result != null)
            {
                results.Add(result);
            }
        }

        // ============================================================
        // String Validation
        // ============================================================

        /// <summary>
        /// Validates that a string is not null, empty, or whitespace.
        /// </summary>
        public static ValidationResult ValidateNonEmpty(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return ValidationResult.Error(
                    $"{fieldName} is null, empty, or whitespace",
                    "Non-empty strings are required for data integrity",
                    $"Assign a valid string value to {fieldName}"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that an ID is not null or empty (basic check).
        /// </summary>
        public static ValidationResult ValidateID(string id, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return ValidationResult.Error(
                    $"{fieldName} is null or empty",
                    "IDs must be non-empty for data identification",
                    $"Assign a unique ID string to {fieldName}"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that an ID follows a standard format (lowercase, alphanumeric, underscores).
        /// </summary>
        public static ValidationResult ValidateIDFormat(string id, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null; // ValidateID handles null/empty
            }

            // Standard ID format: lowercase letters, numbers, underscores only
            if (!Regex.IsMatch(id, @"^[a-z0-9_]+$"))
            {
                return ValidationResult.Warning(
                    $"{fieldName} '{id}' does not follow standard format",
                    "IDs should use lowercase letters, numbers, and underscores only",
                    $"Rename to match pattern: [a-z0-9_]+ (e.g., 'health_potion' not 'Health-Potion')"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that a display name is not null or empty.
        /// </summary>
        public static ValidationResult ValidateDisplayName(string name, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return ValidationResult.Error(
                    $"{fieldName} is null or empty",
                    "Display names are required for UI presentation",
                    $"Assign a player-facing name to {fieldName}"
                );
            }
            return null;
        }

        // ============================================================
        // Numeric Validation
        // ============================================================

        /// <summary>
        /// Validates that an integer value is non-negative (>= 0).
        /// </summary>
        public static ValidationResult ValidateNonNegative(int value, string fieldName)
        {
            if (value < 0)
            {
                return ValidationResult.Error(
                    $"{fieldName} is negative: {value}",
                    "Negative values are not allowed for this field",
                    $"Set {fieldName} to a value >= 0"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that a float value is non-negative (>= 0).
        /// </summary>
        public static ValidationResult ValidateNonNegative(float value, string fieldName)
        {
            if (value < 0f)
            {
                return ValidationResult.Error(
                    $"{fieldName} is negative: {value}",
                    "Negative values are not allowed for this field",
                    $"Set {fieldName} to a value >= 0"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that an integer value falls within a specified range [min, max].
        /// </summary>
        public static ValidationResult ValidateRange(int value, int min, int max, string fieldName)
        {
            if (value < min || value > max)
            {
                return ValidationResult.Error(
                    $"{fieldName} is out of range: {value} (expected {min}-{max})",
                    $"Value must be between {min} and {max} inclusive",
                    $"Set {fieldName} to a value within [{min}, {max}]"
                );
            }
            return null;
        }

        /// <summary>
        /// Validates that a float value falls within a specified range [min, max].
        /// </summary>
        public static ValidationResult ValidateRange(float value, float min, float max, string fieldName)
        {
            if (value < min || value > max)
            {
                return ValidationResult.Error(
                    $"{fieldName} is out of range: {value} (expected {min}-{max})",
                    $"Value must be between {min} and {max} inclusive",
                    $"Set {fieldName} to a value within [{min}, {max}]"
                );
            }
            return null;
        }

        // ============================================================
        // Enum Validation
        // ============================================================

        /// <summary>
        /// Validates that an enum value is defined in its enum type.
        /// Alias for ValidateEnumDefined for backward compatibility.
        /// </summary>
        public static ValidationResult ValidateEnum(object enumValue, string fieldName)
        {
            return ValidateEnumDefined(enumValue, fieldName);
        }

        /// <summary>
        /// Validates that an enum value is defined in its enum type.
        /// </summary>
        public static ValidationResult ValidateEnumDefined(object enumValue, string fieldName)
        {
            if (enumValue == null)
            {
                return ValidationResult.Error(
                    $"{fieldName} is null",
                    "Enum values cannot be null",
                    $"Assign a valid enum value to {fieldName}"
                );
            }

            Type enumType = enumValue.GetType();
            if (!enumType.IsEnum)
            {
                return ValidationResult.Error(
                    $"{fieldName} is not an enum type: {enumType.Name}",
                    "ValidateEnumDefined requires an enum value",
                    $"Ensure {fieldName} is an enum type"
                );
            }

            if (!Enum.IsDefined(enumType, enumValue))
            {
                return ValidationResult.Error(
                    $"{fieldName} has undefined enum value: {enumValue}",
                    $"Value is not defined in {enumType.Name} enum",
                    $"Set {fieldName} to a valid {enumType.Name} value"
                );
            }

            return null;
        }

        // ============================================================
        // Collection Validation
        // ============================================================

        /// <summary>
        /// Validates that a collection contains no duplicate elements.
        /// </summary>
        public static ValidationResult ValidateUnique<T>(IEnumerable<T> collection, string fieldName)
        {
            if (collection == null)
            {
                return ValidationResult.Error(
                    $"{fieldName} is null",
                    "Collection cannot be null for uniqueness validation",
                    $"Initialize {fieldName} with a valid collection"
                );
            }

            var list = collection.ToList();
            var distinctCount = list.Distinct().Count();

            if (distinctCount < list.Count)
            {
                var duplicates = list.GroupBy(x => x)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key?.ToString() ?? "null")
                    .ToList();

                return ValidationResult.Error(
                    $"{fieldName} contains {list.Count - distinctCount} duplicate(s): {string.Join(", ", duplicates)}",
                    "Duplicate elements are not allowed in this collection",
                    $"Remove duplicate entries from {fieldName}"
                );
            }

            return null;
        }

        // ============================================================
        // Asset Reference Validation
        // ============================================================

        /// <summary>
        /// Validates that a Unity asset reference is not null.
        /// </summary>
        public static ValidationResult ValidateAssetReference(UnityEngine.Object asset, string fieldName)
        {
            if (asset == null)
            {
                return ValidationResult.Error(
                    $"{fieldName} is null",
                    "Asset reference is required but not assigned",
                    $"Assign a valid asset to the {fieldName} field in the Inspector"
                );
            }
            return null;
        }
    }
}
