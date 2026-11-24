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
            Level02Xoxo(),
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

        /// <summary>
        /// Fase 2, XOXO 24h: loja de conveniência de 16 × 12 m, pequena e aberta (GDD §11). Três fileiras curtas à
        /// esquerda com os frágeis, um faxineiro rondando por eles, um segurança no corredor da frente e na área aberta
        /// da direita, toca no fundo à direita. Ensina os frágeis e o energético. Luzes: lua, 2 da loja e 2 lanternas.
        /// </summary>
        private static LevelBlueprint Level02Xoxo() => new LevelBlueprint
        {
            AssetName = "Level_02_XOXO",
            DisplayName = "XOXO 24h",
            Columns = 8,
            Rows = 6,
            TargetTimeSeconds = 120f,
            ShoppingList = new[] { "Milk", "Juice", "Jam", "Cookies" },
            Den = new Placement(6.5f, 4.8f),
            RaccoonStart = new Placement(5.6f, 3.6f, 180f),
            Shelves = new[]
            {
                // Fileira A (z = 2,5), com passagem larga entre as duas gôndolas.
                new ShelfPlacement(true, new Placement(-3f, 2.5f)),
                new ShelfPlacement(false, new Placement(1.7f, 2.5f)),
                // Fileira B (z = -0,5), com um vão estreito de 1,2 m entre as gôndolas.
                new ShelfPlacement(true, new Placement(-3f, -0.5f)),
                new ShelfPlacement(false, new Placement(1.2f, -0.5f)),
                // Fileira C (z = -3,5), duas gôndolas curtas.
                new ShelfPlacement(false, new Placement(-5f, -3.5f)),
                new ShelfPlacement(false, new Placement(-1.5f, -3.5f)),
            },
            NarrowGaps = new[] { new Placement(-0.4f, -0.5f) },
            Bins = new[] { new Placement(6.8f, -1.8f, -90f), new Placement(-7.2f, -5.2f, 0f) },
            Items = new[]
            {
                new ItemPlacement("Milk", new Placement(-4f, 2.12f, 0f, Base)),
                new ItemPlacement("Cookies", new Placement(1.5f, 2.88f, 10f, Base)),
                new ItemPlacement("Juice", new Placement(-3.5f, -0.88f, 0f, Base)),
                new ItemPlacement("Jam", new Placement(-1.5f, -3.12f, 15f, Base)),
                new ItemPlacement("Cheese", new Placement(-5f, -3.88f, -10f, Base)),
                new ItemPlacement("EnergyDrink", new Placement(4.5f, -4f)),
                new ItemPlacement("EnergyDrink", new Placement(-6.5f, 0.8f)),
            },
            Enemies = new[]
            {
                // Faxineiro lento, rondando os frágeis pelos corredores da esquerda.
                new EnemyPlacement(EnemyType.Janitor, new[]
                {
                    new Vector3(-6.6f, 0f, 1f),
                    new Vector3(-6.6f, 0f, -2f),
                    new Vector3(-1.5f, 0f, -2f),
                }, 0, 180f),
                // Segurança no corredor da frente e na área aberta; começa longe da toca.
                new EnemyPlacement(EnemyType.Guard, new[]
                {
                    new Vector3(-5f, 0f, -5f),
                    new Vector3(5.8f, 0f, -5f),
                    new Vector3(5.8f, 0f, 1f),
                    new Vector3(3.5f, 0f, 1f),
                }, 0, 90f),
            },
            StoreLights = new[] { new Vector3(-2f, 2.4f, 1f), new Vector3(3f, 2.4f, -3f) },
        };
    }
}
