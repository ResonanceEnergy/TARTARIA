namespace Tartaria.Core.Validation
{
    /// <summary>
    /// Represents a single validation result message.
    /// Used to report errors, warnings, or informational messages during data validation.
    /// Located in Core assembly to avoid circular dependencies.
    /// </summary>
    public class ValidationResult
    {
        public ValidationLevel Level { get; set; }
        public string Message { get; set; }
        public string Context { get; set; }
        public string FixSuggestion { get; set; }

        public ValidationResult(ValidationLevel level, string message, string context = "", string fixSuggestion = "")
        {
            Level = level;
            Message = message;
            Context = context;
            FixSuggestion = fixSuggestion;
        }

        /// <summary>
        /// Creates an error-level validation result.
        /// Errors indicate critical issues that will cause runtime failures.
        /// </summary>
        public static ValidationResult Error(string message, string context = "", string fixSuggestion = "")
        {
            return new ValidationResult(ValidationLevel.Error, message, context, fixSuggestion);
        }

        /// <summary>
        /// Creates a warning-level validation result.
        /// Warnings indicate potential issues or best practice violations.
        /// </summary>
        public static ValidationResult Warning(string message, string context = "", string fixSuggestion = "")
        {
            return new ValidationResult(ValidationLevel.Warning, message, context, fixSuggestion);
        }

        /// <summary>
        /// Creates an info-level validation result.
        /// Info messages provide helpful suggestions or status updates.
        /// </summary>
        public static ValidationResult Info(string message, string context = "")
        {
            return new ValidationResult(ValidationLevel.Info, message, context);
        }

        public override string ToString()
        {
            var result = $"[{Level}] {Message}";
            if (!string.IsNullOrEmpty(Context))
                result += $" (Context: {Context})";
            if (!string.IsNullOrEmpty(FixSuggestion))
                result += $"\n  → Fix: {FixSuggestion}";
            return result;
        }
    }

    /// <summary>
    /// Severity level for validation results.
    /// </summary>
    public enum ValidationLevel
    {
        Info,    // Informational message
        Warning, // Potential issue but not critical
        Error    // Critical issue that must be fixed
    }
}
