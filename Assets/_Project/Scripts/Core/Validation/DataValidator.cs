using System.Collections.Generic;

namespace Tartaria.Core.Validation
{
    /// <summary>
    /// Data validation helper methods.
    /// Stub implementation (Phase 5) — returns null (no validation) for now.
    /// Full validation deferred to later phase.
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

        public static ValidationResult ValidateID(string id, string fieldName)
        {
            // Stub: always pass for now
            return null;
        }

        public static ValidationResult ValidateIDFormat(string id, string fieldName)
        {
            // Stub: always pass for now
            return null;
        }

        public static ValidationResult ValidateDisplayName(string name, string fieldName)
        {
            // Stub: always pass for now
            return null;
        }

        public static ValidationResult ValidateEnum(object enumValue, string fieldName)
        {
            // Stub: always pass for now
            return null;
        }

        public static ValidationResult ValidateNonNegative(int value, string fieldName)
        {
            // Stub: always pass for now
            return null;
        }

        public static ValidationResult ValidateNonNegative(float value, string fieldName)
        {
            // Stub: always pass for now
            return null;
        }
    }
}
