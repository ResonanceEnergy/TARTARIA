using NUnit.Framework;
using Tartaria.Save;
using UnityEngine;

namespace Tartaria.Tests.EditMode
{
    public class SaveDataRoundTripTests
    {
        [Test]
        public void Default_HasSchemaVersion9()
        {
            var data = new SaveData();
            Assert.AreEqual(9, data.header.schemaVersion);
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
    }
}
