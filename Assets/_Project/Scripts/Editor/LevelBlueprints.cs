using System.Collections.Generic;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// As descrições das fases, em ordem (GDD §11 e o plano de próximos passos). Cada fase reaproveita o kit de
    /// cenário; os números se ajustam aqui e se aplicam rodando o menu Montar Fase de novo.
    /// </summary>
    public static class LevelBlueprints
    {
        private const float Base = LevelBlueprint.ShelfBaseTop;

        /// <summary>Fases já descritas, da 1 em diante (as que faltam ainda aparecem como "Em breve").</summary>
        public static IReadOnlyList<LevelBlueprint> All { get; } = new[]
        {
            Level01Mercadinho(),
        };

        /// <summary>
        /// Fase 1, Mercadinho do Seu Zé: 12 × 10 m, 2 corredores com um vão entre as gôndolas, toca no fundo (onde o
        /// guaxinim começa), o Seu Zé (faxineiro) rondando e duas lixeiras. É o tutorial.
        /// </summary>
        private static LevelBlueprint Level01Mercadinho() => new LevelBlueprint
        {
            AssetName = "Level_01_Mercadinho",
            DisplayName = "Mercadinho do Seu Zé",
            Columns = 6,
            Rows = 5,
            TargetTimeSeconds = 90f,
            ShoppingList = new[] { "Bread", "Banana", "Cheese" },
            Den = new Placement(-5f, 4f),
            RaccoonStart = new Placement(-4.2f, 2.6f, 180f),
            Shelves = new[]
            {
                new ShelfPlacement(true, new Placement(-1.5f, 1.5f)),
                new ShelfPlacement(false, new Placement(2.7f, 1.5f)),
                new ShelfPlacement(true, new Placement(-1.5f, -1.5f)),
                new ShelfPlacement(false, new Placement(2.7f, -1.5f)),
            },
            NarrowGaps = new[] { new Placement(1.1f, 1.5f), new Placement(1.1f, -1.5f) },
            Bins = new[] { new Placement(-1f, 3.6f, 180f), new Placement(3.5f, 3.8f, 180f) },
            Items = new[]
            {
                new ItemPlacement("Bread", new Placement(-2.5f, 1.12f, 0f, Base)),
                new ItemPlacement("Banana", new Placement(2.5f, -1.12f, 20f, Base)),
                new ItemPlacement("Cheese", new Placement(-1f, -1.88f, -15f, Base)),
                new ItemPlacement("Cookies", new Placement(3f, 1.88f, 10f, Base)),
            },
            Enemies = new[]
            {
                // Começa no canto da frente, longe da toca, e dá a volta pelos dois corredores.
                new EnemyPlacement(EnemyType.Janitor, new[]
                {
                    new Vector3(5f, 0f, -3.5f),
                    new Vector3(-5f, 0f, -3.5f),
                    new Vector3(-5f, 0f, 0f),
                    new Vector3(5f, 0f, 0f),
                }, 0, -90f),
            },
            StoreLights = new[] { new Vector3(0f, 2.4f, 3f), new Vector3(0f, 2.4f, -3f) },
            HasTutorialTips = true,
        };
    }
}
