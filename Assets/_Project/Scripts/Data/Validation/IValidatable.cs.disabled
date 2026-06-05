using System.Collections.Generic;

namespace Tartaria.Data.Validation
{
    /// <summary>
    /// Interface for all ScriptableObjects that support validation.
    /// Implement this to enable editor-time validation of data assets.
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
