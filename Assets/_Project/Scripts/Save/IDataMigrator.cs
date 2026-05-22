using UnityEngine;

namespace Tartaria.Save
{
    /// <summary>
    /// Data Migrator Interface — handles schema version upgrades.
    /// 
    /// Implementers transform data from version N to N+1.
    /// Chain multiple migrators in MigrationPipeline for multi-version jumps.
    /// 
    /// Example:
    ///   public class ItemMigrator_V1_to_V2 : IDataMigrator&lt;ItemData, ItemData&gt;
    ///   {
    ///       public int FromVersion => 1;
    ///       public int ToVersion => 2;
    ///       public ItemData Migrate(ItemData input) { ... }
    ///   }
    /// </summary>
    /// <typeparam name="TFrom">Input data type (can be same as TTo for in-place migration)</typeparam>
    /// <typeparam name="TTo">Output data type (can be different for structural changes)</typeparam>
    public interface IDataMigrator<in TFrom, out TTo>
    {
        /// <summary>Source schema version this migrator expects.</summary>
        int FromVersion { get; }

        /// <summary>Target schema version this migrator produces.</summary>
        int ToVersion { get; }

        /// <summary>
        /// Transform data from FromVersion to ToVersion.
        /// MUST NOT modify input (create new instance or clone).
        /// </summary>
        /// <param name="input">Data at FromVersion schema</param>
        /// <returns>Data at ToVersion schema</returns>
        TTo Migrate(TFrom input);

        /// <summary>
        /// Validate that migration is safe to perform.
        /// Check for required fields, data integrity, etc.
        /// </summary>
        /// <param name="input">Data to validate</param>
        /// <returns>True if migration can proceed, false if data is corrupted</returns>
        bool Validate(TFrom input)
        {
            // Default: always valid (override for stricter checks)
            return input != null;
        }

        /// <summary>
        /// Get human-readable description of changes this migrator applies.
        /// </summary>
        string GetChangeDescription()
        {
            return $"Migrate v{FromVersion} → v{ToVersion}";
        }
    }

    /// <summary>
    /// Migration Result — reports success/failure + changelog.
    /// </summary>
    public class MigrationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string Changelog { get; set; }
        public int FromVersion { get; set; }
        public int ToVersion { get; set; }
        public float DurationMs { get; set; }

        public static MigrationResult Ok(int from, int to, string changelog, float durationMs)
        {
            return new MigrationResult
            {
                Success = true,
                FromVersion = from,
                ToVersion = to,
                Changelog = changelog,
                DurationMs = durationMs
            };
        }

        public static MigrationResult Fail(int from, int to, string error)
        {
            return new MigrationResult
            {
                Success = false,
                FromVersion = from,
                ToVersion = to,
                ErrorMessage = error
            };
        }

        public override string ToString()
        {
            if (Success)
                return $"✓ Migration v{FromVersion}→v{ToVersion} ({DurationMs:F2}ms)\n{Changelog}";
            else
                return $"✗ Migration v{FromVersion}→v{ToVersion} FAILED: {ErrorMessage}";
        }
    }
}
