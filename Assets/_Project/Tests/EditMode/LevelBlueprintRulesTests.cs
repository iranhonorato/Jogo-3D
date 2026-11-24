using System.Collections.Generic;
using System.Linq;
using Guaxinim.EditorTools;
using NUnit.Framework;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>
    /// Regras da progressão das fases (plano de próximos passos, Etapa 2): cada fase tem de 1,5× a 2× a área da
    /// anterior, listas de 3 a 7 itens, os inimigos da tabela, só itens que existem e tudo dentro da sala.
    /// </summary>
    public class LevelBlueprintRulesTests
    {
        private const float MinAreaGrowth = 1.5f;
        private const float MaxAreaGrowth = 2f;
        private const float WallMargin = 0.3f;

        // Por fase, da 1 à 5.
        private static readonly int[] ListSizes = { 3, 4, 5, 6, 7 };
        private static readonly int[] Janitors = { 1, 1, 2, 2, 3 };
        private static readonly int[] Guards = { 0, 1, 1, 2, 3 };

        private static IEnumerable<int> LevelIndexes() => Enumerable.Range(0, LevelBlueprints.All.Count);

        [Test]
        public void Blueprints_NeverMoreThanFiveLevels()
        {
            Assert.LessOrEqual(LevelBlueprints.All.Count, ListSizes.Length);
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void ShoppingList_HasTheExpectedSize(int index)
        {
            Assert.AreEqual(ListSizes[index], LevelBlueprints.All[index].ShoppingList.Length);
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void Enemies_MatchTheProgressionTable(int index)
        {
            LevelBlueprint blueprint = LevelBlueprints.All[index];

            Assert.AreEqual(Janitors[index], blueprint.CountEnemies(EnemyType.Janitor), "faxineiros");
            Assert.AreEqual(Guards[index], blueprint.CountEnemies(EnemyType.Guard), "seguranças");
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void Area_GrowsBetweenOneAndAHalfAndTwiceThePrevious(int index)
        {
            if (index == 0)
            {
                Assert.Pass("A primeira fase não tem anterior.");
            }

            float growth = LevelBlueprints.All[index].Area / LevelBlueprints.All[index - 1].Area;

            Assert.GreaterOrEqual(growth, MinAreaGrowth);
            Assert.LessOrEqual(growth, MaxAreaGrowth);
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void Items_AllExistInTheItemTable(int index)
        {
            LevelBlueprint blueprint = LevelBlueprints.All[index];
            IEnumerable<string> names = blueprint.Items.Select(item => item.ItemName).Concat(blueprint.ShoppingList);

            foreach (string itemName in names)
            {
                Assert.IsNotNull(ItemFactory.LoadData(itemName), $"Item {itemName} não existe: rode Itens da Tabela.");
            }
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void ShoppingList_EveryItemIsPlacedInTheLevel(int index)
        {
            LevelBlueprint blueprint = LevelBlueprints.All[index];
            var placed = blueprint.Items.Select(item => item.ItemName).ToList();

            foreach (string listItem in blueprint.ShoppingList)
            {
                Assert.IsTrue(placed.Remove(listItem), $"{listItem} está na lista, mas não foi posto na fase.");
            }
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void Positions_AreInsideTheRoom(int index)
        {
            LevelBlueprint blueprint = LevelBlueprints.All[index];
            var points = new List<Vector3> { blueprint.Den.Position, blueprint.RaccoonStart.Position };
            points.AddRange(blueprint.Items.Select(item => item.Placement.Position));
            points.AddRange(blueprint.Bins.Select(bin => bin.Position));
            points.AddRange(blueprint.Enemies.SelectMany(enemy => enemy.Waypoints));

            foreach (Vector3 point in points)
            {
                Assert.IsTrue(blueprint.IsInsideRoom(point, WallMargin), $"{point} fora da sala de {blueprint.AssetName}.");
            }
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void Enemies_StartOnAValidWaypoint(int index)
        {
            foreach (EnemyPlacement enemy in LevelBlueprints.All[index].Enemies)
            {
                Assert.GreaterOrEqual(enemy.Waypoints.Length, 2, "a ronda precisa de pelo menos 2 waypoints");
                Assert.That(enemy.StartWaypoint, Is.InRange(0, enemy.Waypoints.Length - 1));
            }
        }
    }
}
