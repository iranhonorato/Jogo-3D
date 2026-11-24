using System.Collections.Generic;
using System.Linq;
using Guaxinim.EditorTools;
using NUnit.Framework;
using UnityEngine;

namespace Guaxinim.Tests
{
    /// <summary>
    /// Regras da progressão das fases (plano de próximos passos, Etapa 2): cada fase tem de 1,5× a 2× a área da
    /// anterior, listas de 3 a 7 itens, os inimigos da tabela, só itens que existem e tudo dentro da sala. E a
    /// geometria escrita à mão: gôndolas sem sobreposição, nada encostado nelas, vãos estreitos entre duas gôndolas,
    /// rondas em linha reta pelos corredores (fáceis de prever) e o limite de luzes do WebGL.
    /// </summary>
    public class LevelBlueprintRulesTests
    {
        private const float MinAreaGrowth = 1.5f;
        private const float MaxAreaGrowth = 2f;
        private const float WallMargin = 0.3f;
        private const float ShelfClearance = 0.35f; // raio do agente dos inimigos
        private const float WaypointClearance = 0.9f;
        private const float AxisTolerance = 0.01f;
        private const float NarrowGapHalfWidth = 0.6f;
        private const int MaxStoreLights = 2; // GDD §14.6: lua + 2 da loja + 2 lanternas = 5

        // Por fase, da 1 à 5.
        private static readonly int[] ListSizes = { 3, 4, 5, 6, 7 };
        private static readonly int[] Janitors = { 1, 1, 2, 2, 3 };
        private static readonly int[] Guards = { 0, 1, 1, 2, 3 };

        private static IEnumerable<int> LevelIndexes() => Enumerable.Range(0, LevelBlueprints.All.Count);

        private static bool IsOnFloor(ItemPlacement item) => item.Placement.Position.y < LevelBlueprint.ShelfBaseTop / 2f;

        private static IEnumerable<ItemPlacement> FloorItems(LevelBlueprint blueprint) => blueprint.Items.Where(IsOnFloor);

        private static Vector2 Flat(Vector3 point) => new Vector2(point.x, point.z);

        private static Rect Grow(Rect rect, float margin) =>
            new Rect(rect.xMin - margin, rect.yMin - margin, rect.width + 2f * margin, rect.height + 2f * margin);

        private static bool IsInSameRow(ShelfPlacement shelf, Vector2 gapCenter) =>
            IsNear(shelf.Footprint.center.y, gapCenter.y);

        private static bool IsNear(float value, float expected) => Mathf.Abs(value - expected) < AxisTolerance;

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
        public void Shelves_DoNotOverlapAndStayInsideTheRoom(int index)
        {
            LevelBlueprint blueprint = LevelBlueprints.All[index];
            for (int i = 0; i < blueprint.Shelves.Length; i++)
            {
                Rect shelf = blueprint.Shelves[i].Footprint;
                Assert.IsTrue(blueprint.IsInsideRoom(new Vector3(shelf.xMin, 0f, shelf.yMin), WallMargin) &&
                    blueprint.IsInsideRoom(new Vector3(shelf.xMax, 0f, shelf.yMax), WallMargin),
                    $"gôndola {i} fora da sala de {blueprint.AssetName}");

                for (int j = i + 1; j < blueprint.Shelves.Length; j++)
                {
                    Assert.IsFalse(shelf.Overlaps(blueprint.Shelves[j].Footprint),
                        $"gôndolas {i} e {j} se sobrepõem em {blueprint.AssetName}");
                }
            }
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void FloorPoints_AreClearOfShelves(int index)
        {
            LevelBlueprint blueprint = LevelBlueprints.All[index];
            var points = new List<Vector3> { blueprint.Den.Position, blueprint.RaccoonStart.Position };
            points.AddRange(FloorItems(blueprint).Select(item => item.Placement.Position));
            points.AddRange(blueprint.Bins.Select(bin => bin.Position));
            points.AddRange(blueprint.Enemies.SelectMany(enemy => enemy.Waypoints));

            foreach (Vector3 point in points)
            {
                foreach (ShelfPlacement shelf in blueprint.Shelves)
                {
                    Assert.IsFalse(Grow(shelf.Footprint, ShelfClearance).Contains(Flat(point)),
                        $"{point} encostado numa gôndola em {blueprint.AssetName}");
                }
            }
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void ShelfItems_SitOnAShelf(int index)
        {
            LevelBlueprint blueprint = LevelBlueprints.All[index];
            foreach (ItemPlacement item in blueprint.Items.Where(item => !IsOnFloor(item)))
            {
                Assert.IsTrue(blueprint.Shelves.Any(shelf => shelf.Footprint.Contains(Flat(item.Placement.Position))),
                    $"{item.ItemName} na prateleira, mas sem gôndola em {blueprint.AssetName}");
            }
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void Waypoints_KeepClearOfFloorItemsAndBins(int index)
        {
            LevelBlueprint blueprint = LevelBlueprints.All[index];
            var obstacles = FloorItems(blueprint).Select(item => item.Placement.Position)
                .Concat(blueprint.Bins.Select(bin => bin.Position)).ToList();

            foreach (Vector3 waypoint in blueprint.Enemies.SelectMany(enemy => enemy.Waypoints))
            {
                foreach (Vector3 obstacle in obstacles)
                {
                    Assert.Greater(Vector3.Distance(waypoint, obstacle), WaypointClearance,
                        $"waypoint {waypoint} em cima de {obstacle} em {blueprint.AssetName}");
                }
            }
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void PatrolLegs_AreStraightAlongTheAisles(int index)
        {
            foreach (EnemyPlacement enemy in LevelBlueprints.All[index].Enemies)
            {
                for (int i = 0; i < enemy.Waypoints.Length; i++)
                {
                    Vector3 from = enemy.Waypoints[i];
                    Vector3 to = enemy.Waypoints[(i + 1) % enemy.Waypoints.Length]; // a última perna volta à primeira
                    bool isStraight = Mathf.Abs(from.x - to.x) < AxisTolerance || Mathf.Abs(from.z - to.z) < AxisTolerance;
                    Assert.IsTrue(isStraight, $"perna em diagonal de {from} a {to} em {LevelBlueprints.All[index].AssetName}");
                }
            }
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void NarrowGaps_SitBetweenTwoShelvesOnePointTwoMetresApart(int index)
        {
            LevelBlueprint blueprint = LevelBlueprints.All[index];
            foreach (Placement gap in blueprint.NarrowGaps)
            {
                Vector2 center = Flat(gap.Position);
                bool hasLeft = blueprint.Shelves.Any(shelf =>
                    IsInSameRow(shelf, center) && IsNear(center.x - shelf.Footprint.xMax, NarrowGapHalfWidth));
                bool hasRight = blueprint.Shelves.Any(shelf =>
                    IsInSameRow(shelf, center) && IsNear(shelf.Footprint.xMin - center.x, NarrowGapHalfWidth));
                Assert.IsTrue(hasLeft && hasRight, $"vão {gap.Position} sem gôndola dos dois lados em {blueprint.AssetName}");
            }
        }

        [TestCaseSource(nameof(LevelIndexes))]
        public void StoreLights_LeaveRoomForTwoFlashlights(int index)
        {
            Assert.LessOrEqual(LevelBlueprints.All[index].StoreLights.Length, MaxStoreLights);
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
