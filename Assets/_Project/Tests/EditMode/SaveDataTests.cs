using Guaxinim.Core;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>GDD §12: melhor resultado por fase, desbloqueio em sequência e JSON de ida e volta.</summary>
    public class SaveDataTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void IsUnlocked_FirstLevel_AlwaysTrue()
        {
            Assert.IsTrue(new SaveData().IsUnlocked("level_01", null));
        }

        [Test]
        public void IsUnlocked_NextLevel_OnlyAfterPreviousCompleted()
        {
            var save = new SaveData();
            Assert.IsFalse(save.IsUnlocked("level_02", "level_01"));

            save.RecordResult("level_01", 1, 100f);

            Assert.IsTrue(save.IsUnlocked("level_02", "level_01"));
        }

        [Test]
        public void RecordResult_KeepsBestStarsAndBestTimeSeparately()
        {
            var save = new SaveData();
            save.RecordResult("level_01", 3, 120f);

            save.RecordResult("level_01", 1, 80f);

            LevelRecord record = save.Find("level_01");
            Assert.AreEqual(3, record.BestStars);
            Assert.AreEqual(80f, record.BestTimeSeconds, Tolerance);
        }

        [Test]
        public void Json_RoundTrip_KeepsRecords()
        {
            var save = new SaveData();
            save.RecordResult("level_01", 2, 95.5f);

            SaveData loaded = SaveData.FromJson(save.ToJson());

            Assert.AreEqual(2, loaded.Find("level_01").BestStars);
            Assert.AreEqual(95.5f, loaded.Find("level_01").BestTimeSeconds, Tolerance);
        }

        [Test]
        public void RecordResult_FirstCompletion_IsNotARecord()
        {
            var save = new SaveData();

            bool isNewRecord = save.RecordResult("level_01", 2, 80f);

            Assert.IsFalse(isNewRecord);
        }

        [Test]
        public void RecordResult_FasterThanBest_IsARecord()
        {
            var save = new SaveData();
            save.RecordResult("level_01", 2, 80f);

            bool isNewRecord = save.RecordResult("level_01", 1, 70f);

            Assert.IsTrue(isNewRecord);
        }

        [Test]
        public void RecordResult_SlowerThanBest_IsNotARecord()
        {
            var save = new SaveData();
            save.RecordResult("level_01", 2, 80f);

            bool isNewRecord = save.RecordResult("level_01", 3, 95f);

            Assert.IsFalse(isNewRecord);
            Assert.AreEqual(80f, save.Find("level_01").BestTimeSeconds, Tolerance);
        }

        [Test]
        public void RecordResult_EmptyLevelId_IsIgnored()
        {
            var save = new SaveData();

            save.RecordResult(string.Empty, 3, 50f);

            Assert.IsNull(save.Find(string.Empty));
            Assert.IsFalse(SaveData.FromJson(save.ToJson()).IsCompleted(string.Empty));
        }

        [Test]
        public void FromJson_EmptyText_StartsFresh()
        {
            SaveData loaded = SaveData.FromJson(string.Empty);

            Assert.IsNull(loaded.Find("level_01"));
        }
    }
}
