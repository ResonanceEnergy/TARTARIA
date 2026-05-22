using NUnit.Framework;
using UnityEngine;
using Tartaria.Save;

namespace Tartaria.Tests.Save
{
    /// <summary>
    /// Unit tests for data schema versioning and migration system.
    /// 
    /// Test coverage:
    ///   - SchemaVersion constants and helpers
    ///   - IDataMigrator interface compliance
    ///   - MigrationPipeline single-step migrations
    ///   - MigrationPipeline multi-step migrations (chaining)
    ///   - Migration validation
    ///   - Migration error handling
    ///   - Backward compatibility checks
    /// </summary>
    [TestFixture]
    public class MigrationSystemTests
    {
        #region SchemaVersion Tests

        [Test]
        public void SchemaVersion_CurrentVersions_AreValid()
        {
            Assert.GreaterOrEqual(SchemaVersion.CURRENT_SAVE, 1, "CURRENT_SAVE must be >= 1");
            Assert.GreaterOrEqual(SchemaVersion.CURRENT_ITEM, 1, "CURRENT_ITEM must be >= 1");
            Assert.GreaterOrEqual(SchemaVersion.CURRENT_QUEST, 1, "CURRENT_QUEST must be >= 1");
            Assert.GreaterOrEqual(SchemaVersion.CURRENT_ENEMY, 1, "CURRENT_ENEMY must be >= 1");
        }

        [Test]
        public void SchemaVersion_IsCompatible_AcceptsCurrentVersion()
        {
            bool compatible = SchemaVersion.IsCompatible(
                currentVersion: SchemaVersion.CURRENT_SAVE,
                dataVersion: SchemaVersion.CURRENT_SAVE);
            Assert.IsTrue(compatible, "Current version should be compatible");
        }

        [Test]
        public void SchemaVersion_IsCompatible_RejectsNewerVersion()
        {
            bool compatible = SchemaVersion.IsCompatible(
                currentVersion: 10,
                dataVersion: 15);
            Assert.IsFalse(compatible, "Newer version should be rejected");
        }

        [Test]
        public void SchemaVersion_IsCompatible_RejectsTooOldVersion()
        {
            bool compatible = SchemaVersion.IsCompatible(
                currentVersion: 20,
                dataVersion: 5,
                maxVersionsBack: 10);
            Assert.IsFalse(compatible, "Version >10 steps old should be rejected");
        }

        [Test]
        public void SchemaVersion_IsCompatible_AcceptsRecentVersion()
        {
            bool compatible = SchemaVersion.IsCompatible(
                currentVersion: 20,
                dataVersion: 15,
                maxVersionsBack: 10);
            Assert.IsTrue(compatible, "Version 5 steps old should be accepted");
        }

        [Test]
        public void SchemaVersion_GetChangelog_ReturnsEmptyForSameVersion()
        {
            string changelog = SchemaVersion.GetChangelog("SaveData", 10, 10);
            Assert.AreEqual("No changes", changelog);
        }

        [Test]
        public void SchemaVersion_GetCurrentVersion_ReturnsCorrectValues()
        {
            Assert.AreEqual(SchemaVersion.CURRENT_SAVE, SchemaVersion.GetCurrentVersion("SaveData"));
            Assert.AreEqual(SchemaVersion.CURRENT_ITEM, SchemaVersion.GetCurrentVersion("ItemData"));
            Assert.AreEqual(SchemaVersion.CURRENT_QUEST, SchemaVersion.GetCurrentVersion("QuestData"));
            Assert.AreEqual(1, SchemaVersion.GetCurrentVersion("UnknownType"), "Unknown type should default to v1");
        }

        #endregion

        #region MigrationPipeline Tests

        [Test]
        public void MigrationPipeline_NoMigrationNeeded_ReturnsSuccess()
        {
            var pipeline = new MigrationPipeline<TestData>();
            var data = new TestData { version = 1, value = "original" };

            var result = pipeline.Migrate(data, currentVersion: 1, targetVersion: 1);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("No changes needed", result.Changelog);
        }

        [Test]
        public void MigrationPipeline_SingleStepMigration_Works()
        {
            var pipeline = new MigrationPipeline<TestData>();
            pipeline.Register(new TestMigrator_V1_to_V2());

            var data = new TestData { version = 1, value = "original" };
            var result = pipeline.Migrate(data, currentVersion: 1, targetVersion: 2);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.FromVersion);
            Assert.AreEqual(2, result.ToVersion);
        }

        [Test]
        public void MigrationPipeline_MultiStepMigration_ChainsCorrectly()
        {
            var pipeline = new MigrationPipeline<TestData>();
            pipeline.Register(new TestMigrator_V1_to_V2());
            pipeline.Register(new TestMigrator_V2_to_V3());

            var data = new TestData { version = 1, value = "original" };
            var result = pipeline.Migrate(data, currentVersion: 1, targetVersion: 3);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.FromVersion);
            Assert.AreEqual(3, result.ToVersion);
            Assert.That(result.Changelog, Does.Contain("V1→V2"));
            Assert.That(result.Changelog, Does.Contain("V2→V3"));
        }

        [Test]
        public void MigrationPipeline_NoPathExists_ReturnsFailure()
        {
            var pipeline = new MigrationPipeline<TestData>();
            pipeline.Register(new TestMigrator_V1_to_V2());
            // No v2→v3 migrator registered

            var data = new TestData { version = 1, value = "original" };
            var result = pipeline.Migrate(data, currentVersion: 1, targetVersion: 3);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("No migration path"));
        }

        [Test]
        public void MigrationPipeline_BackwardMigration_ReturnsFailure()
        {
            var pipeline = new MigrationPipeline<TestData>();
            var data = new TestData { version = 3, value = "newer" };

            var result = pipeline.Migrate(data, currentVersion: 3, targetVersion: 1);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("backwards"));
        }

        [Test]
        public void MigrationPipeline_NullData_ReturnsFailure()
        {
            var pipeline = new MigrationPipeline<TestData>();
            var result = pipeline.Migrate(null, currentVersion: 1, targetVersion: 2);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("null"));
        }

        [Test]
        public void MigrationPipeline_ValidationFails_ReturnsFailure()
        {
            var pipeline = new MigrationPipeline<TestData>();
            pipeline.Register(new TestMigrator_V1_to_V2_WithValidation());

            var data = new TestData { version = 1, value = null }; // Invalid data
            var result = pipeline.Migrate(data, currentVersion: 1, targetVersion: 2);

            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Does.Contain("Validation failed"));
        }

        [Test]
        public void MigrationPipeline_DryRun_DoesNotModifyData()
        {
            var pipeline = new MigrationPipeline<TestData>();
            pipeline.Register(new TestMigrator_V1_to_V2());

            var data = new TestData { version = 1, value = "original" };
            var result = pipeline.Migrate(data, currentVersion: 1, targetVersion: 2, dryRun: true);

            Assert.IsTrue(result.Success);
            Assert.That(result.Changelog, Does.Contain("DRY RUN"));
            Assert.AreEqual("original", data.value, "Data should not be modified in dry run");
        }

        [Test]
        public void MigrationPipeline_CanMigrate_DetectsAvailablePaths()
        {
            var pipeline = new MigrationPipeline<TestData>();
            pipeline.Register(new TestMigrator_V1_to_V2());
            pipeline.Register(new TestMigrator_V2_to_V3());

            Assert.IsTrue(pipeline.CanMigrate(1, 2), "Should have path 1→2");
            Assert.IsTrue(pipeline.CanMigrate(1, 3), "Should have path 1→2→3");
            Assert.IsFalse(pipeline.CanMigrate(1, 4), "Should not have path 1→4");
            Assert.IsFalse(pipeline.CanMigrate(3, 1), "Should not migrate backwards");
        }

        [Test]
        public void MigrationPipeline_Performance_UnderBudget()
        {
            var pipeline = new MigrationPipeline<TestData>();
            pipeline.Register(new TestMigrator_V1_to_V2());
            pipeline.Register(new TestMigrator_V2_to_V3());

            var data = new TestData { version = 1, value = "original" };
            var result = pipeline.Migrate(data, currentVersion: 1, targetVersion: 3);

            Assert.IsTrue(result.Success);
            Assert.Less(result.DurationMs, 100f, "Migration should take < 100ms");
        }

        #endregion

        #region SaveData Migration Tests

        [Test]
        public void SaveDataMigrator_V17_to_V18_PreservesData()
        {
            var migrator = new SaveDataMigrator_V17_to_V18();
            var input = new SaveData { version = 17 };
            input.player.health = 75f;
            input.world.resonanceScore = 1234f;

            var output = migrator.Migrate(input);

            Assert.IsNotNull(output);
            Assert.AreEqual(SchemaVersion.SAVE_V18, output.version);
            Assert.AreEqual(75f, output.player.health, "Player data should be preserved");
            Assert.AreEqual(1234f, output.world.resonanceScore, "World data should be preserved");
        }

        [Test]
        public void SaveDataMigrator_V17_to_V18_Validation()
        {
            var migrator = new SaveDataMigrator_V17_to_V18();
            
            Assert.IsTrue(migrator.Validate(new SaveData { version = 17 }));
            Assert.IsFalse(migrator.Validate(null));
            Assert.IsFalse(migrator.Validate(new SaveData { version = 16 }));
        }

        [Test]
        public void SaveDataMigrator_V2_to_V17_InitializesNewBlocks()
        {
            var migrator = new SaveDataMigrator_V2_to_V17();
            var input = new SaveData { version = 2 };

            var output = migrator.Migrate(input);

            Assert.IsNotNull(output);
            Assert.AreEqual(SchemaVersion.SAVE_V17, output.version);
            // All v3-v17 blocks should be initialized (not null)
            Assert.IsNotNull(output.economy);
            Assert.IsNotNull(output.codex);
            Assert.IsNotNull(output.providerData);
        }

        #endregion

        #region Test Data Classes

        class TestData
        {
            public int version;
            public string value;
        }

        class TestMigrator_V1_to_V2 : IDataMigrator<TestData, TestData>
        {
            public int FromVersion => 1;
            public int ToVersion => 2;

            public TestData Migrate(TestData input)
            {
                return new TestData
                {
                    version = 2,
                    value = input.value + "_v2"
                };
            }

            public string GetChangeDescription() => "V1→V2: Append '_v2' to value";
        }

        class TestMigrator_V2_to_V3 : IDataMigrator<TestData, TestData>
        {
            public int FromVersion => 2;
            public int ToVersion => 3;

            public TestData Migrate(TestData input)
            {
                return new TestData
                {
                    version = 3,
                    value = input.value + "_v3"
                };
            }

            public string GetChangeDescription() => "V2→V3: Append '_v3' to value";
        }

        class TestMigrator_V1_to_V2_WithValidation : IDataMigrator<TestData, TestData>
        {
            public int FromVersion => 1;
            public int ToVersion => 2;

            public TestData Migrate(TestData input)
            {
                return new TestData { version = 2, value = input.value };
            }

            public bool Validate(TestData input)
            {
                return input != null && !string.IsNullOrEmpty(input.value);
            }

            public string GetChangeDescription() => "V1→V2 (with validation)";
        }

        #endregion
    }
}
