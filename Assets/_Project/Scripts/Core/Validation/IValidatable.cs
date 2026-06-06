using System.Collections.Generic;

namespace Tartaria.Core.Validation
{
    /// <summary>
    /// Interface for data objects that can be validated.
    /// Stub implementation (Phase 5) — full validation deferred.
    /// </summary>
    public interface IValidatable
    {
        List<ValidationResult> Validate();
    }
}
