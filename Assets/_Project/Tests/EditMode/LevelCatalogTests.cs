using Guaxinim.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>GDD §11 e §12: cada fase só libera depois da anterior, e a próxima fase vem do catálogo.</summary>
    public class LevelCatalogTests
    {
        private LevelData _first;
        private LevelData _second;
        private LevelData _comingSoon;
        private LevelCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _first = CreateLevel("level_01", "Scene_01");
            _second = CreateLevel("level_02", "Scene_02");
            _comingSoon = CreateLevel("level_03", string.Empty);
            _catalog = ScriptableObject.CreateInstance<LevelCatalog>();
            var serialized = new SerializedObject(_catalog);
            SerializedProperty levels = serialized.FindProperty("_levels");
            levels.arraySize = 3;
            levels.GetArrayElementAtIndex(0).objectReferenceValue = _first;
            levels.GetArrayElementAtIndex(1).objectReferenceValue = _second;
            levels.GetArrayElementAtIndex(2).objectReferenceValue = _comingSoon;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_first);
            Object.DestroyImmediate(_second);
            Object.DestroyImmediate(_comingSoon);
            Object.DestroyImmediate(_catalog);
        }

        [Test]
        public void IsUnlocked_FirstLevel_AlwaysTrue()
        {
            Assert.IsTrue(_catalog.IsUnlocked(0, new SaveData()));
        }

        [Test]
        public void IsUnlocked_SecondLevel_OnlyAfterFirstCompleted()
        {
            var save = new SaveData();
            Assert.IsFalse(_catalog.IsUnlocked(1, save));

            save.RecordResult("level_01", 1, 60f);

            Assert.IsTrue(_catalog.IsUnlocked(1, save));
        }

        [Test]
        public void IsUnlocked_LevelWithoutScene_AlwaysFalse()
        {
            var save = new SaveData();
            save.RecordResult("level_01", 3, 60f);
            save.RecordResult("level_02", 3, 60f);

            Assert.IsFalse(_catalog.IsUnlocked(2, save));
        }

        [Test]
        public void IsUnlocked_IndexOutOfRange_False()
        {
            Assert.IsFalse(_catalog.IsUnlocked(-1, new SaveData()));
            Assert.IsFalse(_catalog.IsUnlocked(3, new SaveData()));
        }

        [Test]
        public void TryGetNext_AfterCompletingCurrent_ReturnsNextLevel()
        {
            var save = new SaveData();
            save.RecordResult("level_01", 2, 70f);

            bool found = _catalog.TryGetNext(_first, save, out LevelData next);

            Assert.IsTrue(found);
            Assert.AreSame(_second, next);
        }

        [Test]
        public void TryGetNext_NextHasNoScene_ReturnsFalse()
        {
            var save = new SaveData();
            save.RecordResult("level_02", 2, 70f);

            bool found = _catalog.TryGetNext(_second, save, out LevelData next);

            Assert.IsFalse(found);
            Assert.IsNull(next);
        }

        [Test]
        public void TryGetNext_LevelNotInCatalog_ReturnsFalse()
        {
            LevelData sandbox = CreateLevel(string.Empty, string.Empty);

            bool found = _catalog.TryGetNext(sandbox, new SaveData(), out LevelData next);

            Assert.IsFalse(found);
            Assert.IsNull(next);
            Object.DestroyImmediate(sandbox);
        }

        [Test]
        public void IndexOf_NullLevel_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, _catalog.IndexOf(null));
        }

        [Test]
        public void PlayableCount_CountsOnlyLevelsWithScene()
        {
            Assert.AreEqual(2, _catalog.PlayableCount);
        }

        [Test]
        public void FirstPlayable_ReturnsFirstLevelWithScene()
        {
            Assert.AreSame(_first, _catalog.FirstPlayable);
        }

        [Test]
        public void IsLastPlayable_SkipsComingSoonLevelsAfterIt()
        {
            Assert.IsTrue(_catalog.IsLastPlayable(_second));
            Assert.IsFalse(_catalog.IsLastPlayable(_first));
            Assert.IsFalse(_catalog.IsLastPlayable(_comingSoon));
        }

        [Test]
        public void IsLastPlayable_LevelNotInCatalog_ReturnsFalse()
        {
            LevelData sandbox = CreateLevel(string.Empty, "Sandbox");

            Assert.IsFalse(_catalog.IsLastPlayable(sandbox));
            Object.DestroyImmediate(sandbox);
        }

        [Test]
        public void SumBestStars_AddsBestStarsOfPlayableLevels()
        {
            var save = new SaveData();
            save.RecordResult("level_01", 2, 60f);
            save.RecordResult("level_02", 3, 45f);

            Assert.AreEqual(5, _catalog.SumBestStars(save));
            Assert.AreEqual(0, _catalog.SumBestStars(new SaveData()));
        }

        [Test]
        public void SumBestTimeSeconds_AddsBestTimesOfCompletedLevels()
        {
            var save = new SaveData();
            save.RecordResult("level_01", 2, 60f);
            save.RecordResult("level_02", 1, 45f);
            save.RecordResult("level_02", 1, 90f);

            Assert.AreEqual(105f, _catalog.SumBestTimeSeconds(save), 0.001f);
        }

        private static LevelData CreateLevel(string id, string sceneName)
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            var serialized = new SerializedObject(level);
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_sceneName").stringValue = sceneName;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return level;
        }
    }
}
