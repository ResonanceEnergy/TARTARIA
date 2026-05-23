using System.Collections.Generic;

namespace Tartaria.Core.Validation
{
    /// <summary>
    /// Interface for all ScriptableObjects that support validation.
    /// Implement this to enable editor-time validation of data assets.
    /// Located in Core assembly to avoid circular dependencies.
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Validates the data and returns a list of validation results.
        /// </summary>
        /// <returns>List of validation messages (errors, warnings, info)</returns>
        List<ValidationResult> Validate();
    }
}
