namespace Tartaria.Core.Validation
{
    /// <summary>
    /// Validation result — stores errors/warnings from data validation.
    /// Stub implementation (Phase 5) — full validation deferred.
    /// </summary>
    public class ValidationResult
    {
        public enum Severity { Error, Warning, Info }

        public Severity Level { get; private set; }
        public string Message { get; private set; }
        public string Context { get; private set; }
        public string FixHint { get; private set; }

        private ValidationResult(Severity level, string message, string context, string fixHint)
        {
            Level = level;
            Message = message;
            Context = context;
            FixHint = fixHint;
        }

        public static ValidationResult Error(string message, string context, string fixHint) =>
            new ValidationResult(Severity.Error, message, context, fixHint);

        public static ValidationResult Warning(string message, string context, string fixHint) =>
            new ValidationResult(Severity.Warning, message, context, fixHint);

        public static ValidationResult Info(string message, string context, string fixHint) =>
            new ValidationResult(Severity.Info, message, context, fixHint);
    }
}
