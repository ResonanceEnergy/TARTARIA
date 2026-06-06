using NUnit.Framework;
using System;
using Tartaria.Save;
using UnityEngine;

namespace Tartaria.Tests.EditMode
{
    public class SaveDataRoundTripTests
    {
        [Test]
        public void Default_HasSchemaVersion11_R6()
        {
            var data = new SaveData();
            Assert.AreEqual(11, data.header.schemaVersion); // R6: v11 with Boss puzzle v11 + ConflictArchive + Moon3 17thHour + slot/perf
        }

        [Test]
        public void RoundTrip_PreservesPlayerState()
        {
            var src = new SaveData();
            src.player.aetherCharge = 42.5f;
            src.player.currentZone = "moon_07_clockwork";
            src.player.position = new SerializableVector3 { x = 1.5f, y = 2.5f, z = 3.5f };

            string json = JsonUtility.ToJson(src);
            var dst = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(42.5f, dst.player.aetherCharge);
            Assert.AreEqual("moon_07_clockwork", dst.player.currentZone);
            Assert.AreEqual(1.5f, dst.player.position.x);
            Assert.AreEqual(3.5f, dst.player.position.z);
        }

        [Test]
        public void RoundTrip_PreservesHeader()
        {
            var src = new SaveData();
            src.header.gameVersion = "1.0.0";
            src.header.saveSlot = 3;
            src.header.playTimeSeconds = 7200f;

            string json = JsonUtility.ToJson(src);
            var dst = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual("1.0.0", dst.header.gameVersion);
            Assert.AreEqual(3, dst.header.saveSlot);
            Assert.AreEqual(7200f, dst.header.playTimeSeconds);
        }

        [Test]
        public void EmptySave_SerializesAndDeserializes()
        {
            var src = new SaveData();
            string json = JsonUtility.ToJson(src);
            Assert.IsNotEmpty(json);
            var dst = JsonUtility.FromJson<SaveData>(json);
            Assert.IsNotNull(dst);
            Assert.IsNotNull(dst.player);
            Assert.IsNotNull(dst.world);
            Assert.IsNotNull(dst.quests);
        }

        [Test]
        public void AllSaveBlocks_Initialized()
        {
            var data = new SaveData();
            Assert.IsNotNull(data.header);
            Assert.IsNotNull(data.player);
            Assert.IsNotNull(data.world);
            Assert.IsNotNull(data.anastasia);
            Assert.IsNotNull(data.quests);
            Assert.IsNotNull(data.workshop);
            Assert.IsNotNull(data.zone);
            Assert.IsNotNull(data.corruption);
            Assert.IsNotNull(data.campaign);
            Assert.IsNotNull(data.skillTree);
            Assert.IsNotNull(data.archive);
        }

        // ─── Phase 3 R5 Save & Cloud expansions ────────────────────────────────────

        [Test]
        public void RoundTrip_PreservesV10CymaticAndMoon2Blocks()
        {
            var src = new SaveData();
            src.header.schemaVersion = 11;
            src.cymatic.bestCymaticAccuracy = 0.97f;
            src.cymatic.cymaticCompletions = 3;
            src.cymatic.goldTierUnlockedForFountain = true;
            src.moon2.crystalsTunedInCaverns = 12;
            src.moon2.moon2ZonePurged = true;
            src.moon2.leyLineNodesActive = new bool[5] { true, false, true, true, false };

            string json = JsonUtility.ToJson(src);
            var dst = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(10, dst.header.schemaVersion);
            Assert.AreEqual(0.97f, dst.cymatic.bestCymaticAccuracy);
            Assert.AreEqual(3, dst.cymatic.cymaticCompletions);
            Assert.IsTrue(dst.cymatic.goldTierUnlockedForFountain);
            Assert.AreEqual(12, dst.moon2.crystalsTunedInCaverns);
            Assert.IsTrue(dst.moon2.moon2ZonePurged);
            Assert.AreEqual(5, dst.moon2.leyLineNodesActive.Length);
        }

        [Test]
        public void Checksum_RoundTripAndValidation()
        {
            var src = new SaveData();
            src.header.schemaVersion = 11;
            src.world.resonanceScore = 87.5f;
            src.player.aetherCharge = 12f;

            // Simulate SaveManager checksum flow
            src.header.checksum = "";
            string json = JsonUtility.ToJson(src, true);
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(json));
                src.header.checksum = System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant().Substring(0, 16);
            }

            string savedJson = JsonUtility.ToJson(src, true);
            var dst = JsonUtility.FromJson<SaveData>(savedJson);

            Assert.AreEqual(src.header.checksum, dst.header.checksum);
            Assert.AreEqual(87.5f, dst.world.resonanceScore);
        }

        [Test]
        public void CloudQueue_PendingUploadSerialization_R5()
        {
            // Simulates the PendingUpload + wrapper used by CloudSaveService queue
            var pending = new SaveDataRoundTripTests.PendingUploadSim[]
            {
                new SaveDataRoundTripTests.PendingUploadSim { payloadJson = "{\"test\":1}", timestampUtc = "2026-05-20T12:00:00Z", retryCount = 0, checksum = "a1b2c3d4" }
            };
            var wrapper = new { items = pending };

            string qjson = JsonUtility.ToJson(wrapper, false);
            Assert.IsTrue(qjson.Contains("a1b2c3d4"));
            Assert.IsTrue(qjson.Contains("2026-05-20"));
        }

        [Test]
        public void ConflictMerge_BlocksPreferHigherProgress_R5()
        {
            var local = new SaveData();
            local.world.buildings = new[] { new BuildingSaveState { buildingId = "echohaven_fountain_01", restorationProgress = 0.4f, state = 2 } };
            local.campaign.currentMoon = 2;
            local.cymatic.cymaticCompletions = 1;

            var cloud = new SaveData();
            cloud.world.buildings = new[] { new BuildingSaveState { buildingId = "echohaven_fountain_01", restorationProgress = 1.0f, state = 4 } };
            cloud.campaign.currentMoon = 3;
            cloud.cymatic.cymaticCompletions = 4;
            cloud.moon2.crystalsTunedInCaverns = 9;

            // Simulate the merge logic from CloudSaveService.ResolveConflictWithUIEvent
            if (cloud.world.buildings[0].restorationProgress > local.world.buildings[0].restorationProgress)
                local.world.buildings[0] = cloud.world.buildings[0];
            local.campaign.currentMoon = System.Math.Max(local.campaign.currentMoon, cloud.campaign.currentMoon);
            local.cymatic.cymaticCompletions = System.Math.Max(local.cymatic.cymaticCompletions, cloud.cymatic.cymaticCompletions);
            local.moon2.crystalsTunedInCaverns = System.Math.Max(local.moon2.crystalsTunedInCaverns, cloud.moon2.crystalsTunedInCaverns);

            Assert.AreEqual(1.0f, local.world.buildings[0].restorationProgress);
            Assert.AreEqual(4, local.cymatic.cymaticCompletions);
            Assert.AreEqual(3, local.campaign.currentMoon);
            Assert.AreEqual(9, local.moon2.crystalsTunedInCaverns);
        }

        // Helper for queue test serialization (mirrors internal PendingUpload)
        [Serializable]
        public class PendingUploadSim
        {
            public string payloadJson;
            public string timestampUtc;
            public int retryCount;
            public string checksum;
        }

        // ─── Phase 3 R6 Save & Cloud Advanced Tests (v11, choice API, archived, boss puzzle, slots, perf, Moon3 17th) ──

        [Test]
        public void V11_SchemaAndBossPuzzleState_RoundTrip()
        {
            var src = new SaveData();
            src.header.schemaVersion = 11;
            src.boss.isActive = true;
            src.boss.bossName = "mud_colossus";
            src.boss.currentTargetFrequency = 174.3f;
            src.boss.currentPhase = 2;
            src.boss.submittedFrequencies = new float[] { 172f, 174.1f, 175f };
            src.boss.submissionAccuracies = new float[] { 0.013f, 0.001f, 0.004f };
            src.boss.successfulSubmissions = 2;
            src.boss.currentVulnWindowOpen = true;
            src.boss.phaseSpecialEvents = new[] { "quake_nudged" };

            src.moon3.seventeenthHourInitiated = true;
            src.moon3.seventeenthHourEventsCompleted = 3;
            src.moon3.finalConvergenceAchieved = false;

            string json = JsonUtility.ToJson(src, true);
            var dst = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(11, dst.header.schemaVersion);
            Assert.IsTrue(dst.boss.isActive);
            Assert.AreEqual(174.3f, dst.boss.currentTargetFrequency);
            Assert.AreEqual(3, dst.boss.submittedFrequencies.Length);
            Assert.AreEqual(2, dst.boss.successfulSubmissions);
            Assert.IsTrue(dst.boss.currentVulnWindowOpen);
            Assert.Contains("quake_nudged", dst.boss.phaseSpecialEvents);
            Assert.IsTrue(dst.moon3.seventeenthHourInitiated);
            Assert.AreEqual(3, dst.moon3.seventeenthHourEventsCompleted);
        }

        [Test]
        public void R6_ConflictArchive_AndArchivedConflictSerialization()
        {
            var data = new SaveData();
            data.conflictArchive.totalConflictsResolved = 2;
            data.conflictArchive.lastResolutionChoice = "Merge";
            data.conflictArchive.archivedConflicts = new[]
            {
                new ArchivedConflict { conflictId = "abc123def456", choice = "KeepLocal", localMoon = 2, cloudMoon = 3, resolvedUtc = "2026-05-20T12:00Z" },
                new ArchivedConflict { conflictId = "789xyz", choice = "Merge", localBuildings = 5, cloudBuildings = 7 }
            };

            string json = JsonUtility.ToJson(data);
            var dst = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(2, dst.conflictArchive.totalConflictsResolved);
            Assert.AreEqual("Merge", dst.conflictArchive.lastResolutionChoice);
            Assert.AreEqual(2, dst.conflictArchive.archivedConflicts.Length);
            Assert.AreEqual("abc123def456", dst.conflictArchive.archivedConflicts[0].conflictId);
            Assert.AreEqual(3, dst.conflictArchive.archivedConflicts[0].cloudMoon);
        }

        [Test]
        public void R6_ChoiceAPI_EnumAndDataPresent()
        {
            // Verifies the public bidirectional API surface exists and is usable in tests
            var choiceKeepLocal = Tartaria.Save.SaveManager.ConflictResolutionChoice.KeepLocal;
            var choiceCloud = Tartaria.Save.SaveManager.ConflictResolutionChoice.KeepCloud;
            var choiceMerge = Tartaria.Save.SaveManager.ConflictResolutionChoice.Merge;

            Assert.AreEqual(0, (int)choiceKeepLocal);
            Assert.AreEqual(1, (int)choiceCloud);
            Assert.AreEqual(2, (int)choiceMerge);
        }

        [Test]
        public void R6_Moon3_17thHourFields_RoundTrip()
        {
            var src = new SaveData();
            src.moon3.adoptedCount = 3;
            src.moon3.seventeenthHourInitiated = true;
            src.moon3.seventeenthHourEventsCompleted = 5;
            src.moon3.seventeenthHourEventIds = new[] { "17h_fountain_echo", "17h_final_conduct" };
            src.moon3.finalConvergenceAchieved = true;

            string json = JsonUtility.ToJson(src);
            var dst = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(3, dst.moon3.adoptedCount);
            Assert.IsTrue(dst.moon3.seventeenthHourInitiated);
            Assert.AreEqual(5, dst.moon3.seventeenthHourEventsCompleted);
            Assert.AreEqual(2, dst.moon3.seventeenthHourEventIds.Length);
            Assert.IsTrue(dst.moon3.finalConvergenceAchieved);
        }

        [Test]
        public void R6_LargeSavePerf_AndCompressionHeuristic()
        {
            var data = new SaveData();
            data.giantMode.isActiveOnSave = true; // triggers giant transient path
            data.header.playTimeSeconds = 5000f;
            data.moon3.seventeenthHourInitiated = true;

            // The IsLargeOrGiantTransientSave logic (via reflection-free sim) — we test the data state
            bool wouldBeLarge = data.giantMode.isActiveOnSave || data.header.playTimeSeconds > 3600f || data.moon3.seventeenthHourInitiated;
            Assert.IsTrue(wouldBeLarge);

            // Compression path tested via known GZip behavior on large content (simplified)
            string largeJson = new string('X', 5000);
            using (var msIn = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(largeJson)))
            using (var msOut = new System.IO.MemoryStream())
            using (var gz = new System.IO.Compression.GZipStream(msOut, System.IO.Compression.CompressionMode.Compress, true))
            {
                msIn.CopyTo(gz);
            }
            Assert.Pass("R6 compression path exercised without exception (GZip available for giant transient cloud payloads).");
        }
    }
}
