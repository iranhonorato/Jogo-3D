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
            Level03PaoDeMel(),
            Level04Carrecinco(),
            Level05Atacadaco(),
        };

        /// <summary>Fileira de gôndolas de 4 m ao longo de X, com os centros dados, na profundidade <paramref name="z"/>.</summary>
        private static ShelfPlacement[] LongRow(float z, params float[] centersX)
        {
            var row = new ShelfPlacement[centersX.Length];
            for (int i = 0; i < centersX.Length; i++)
            {
                row[i] = new ShelfPlacement(true, new Placement(centersX[i], z));
            }

            return row;
        }

        private static ShelfPlacement[] Join(params ShelfPlacement[][] rows)
        {
            var all = new List<ShelfPlacement>();
            foreach (ShelfPlacement[] row in rows)
            {
                all.AddRange(row);
            }

            return all.ToArray();
        }

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

        /// <summary>
        /// Fase 3, Pão de Mel: supermercado de 20 × 16 m com 4 corredores (GDD §11). Três fileiras de gôndolas, duas
        /// delas com vão estreito; toca no fundo à esquerda; uma coluna livre à direita e uma área larga na frente.
        /// Ensina os itens volumosos (salgadinho gigante longe da toca). 2 faxineiros e 1 segurança; o
        /// FlashlightBudget deixa acesas só as 2 lanternas mais perto do guaxinim.
        /// </summary>
        private static LevelBlueprint Level03PaoDeMel() => new LevelBlueprint
        {
            AssetName = "Level_03_PaoDeMel",
            DisplayName = "Pão de Mel",
            Columns = 10,
            Rows = 8,
            TargetTimeSeconds = 150f,
            ShoppingList = new[] { "Eggs", "Cheese", "GiantChips", "Cereal", "OliveOil" },
            Den = new Placement(-8.5f, 6.8f),
            RaccoonStart = new Placement(-7.6f, 5.8f, 180f),
            Shelves = new[]
            {
                // Fileira 1 (z = 4), com vão estreito.
                new ShelfPlacement(true, new Placement(-5f, 4f)),
                new ShelfPlacement(true, new Placement(0.2f, 4f)),
                new ShelfPlacement(false, new Placement(5f, 4f)),
                // Fileira 2 (z = 0,5), com passagens largas.
                new ShelfPlacement(true, new Placement(-4.5f, 0.5f)),
                new ShelfPlacement(true, new Placement(1f, 0.5f)),
                new ShelfPlacement(false, new Placement(5.5f, 0.5f)),
                // Fileira 3 (z = -3), com vão estreito.
                new ShelfPlacement(true, new Placement(-5f, -3f)),
                new ShelfPlacement(true, new Placement(0.2f, -3f)),
                new ShelfPlacement(false, new Placement(5f, -3f)),
            },
            NarrowGaps = new[] { new Placement(-2.4f, 4f), new Placement(-2.4f, -3f) },
            Bins = new[]
            {
                new Placement(-9.3f, 2.2f, 90f),
                new Placement(9.3f, -1f, -90f),
                new Placement(2.5f, 7.3f, 180f),
            },
            Items = new[]
            {
                new ItemPlacement("Eggs", new Placement(0f, 0.12f, 0f, Base)),
                new ItemPlacement("Cheese", new Placement(-5.5f, -2.62f, 15f, Base)),
                new ItemPlacement("GiantChips", new Placement(6f, -6f, 20f)),
                new ItemPlacement("Cereal", new Placement(5f, 3.62f, 0f, Base)),
                new ItemPlacement("OliveOil", new Placement(1f, -3.38f, -10f, Base)),
                new ItemPlacement("Popcorn", new Placement(-4.5f, 4.38f, 0f, Base)),
                new ItemPlacement("Bread", new Placement(5.2f, 0.88f, 10f, Base)),
                new ItemPlacement("EnergyDrink", new Placement(8.5f, 2f)),
                new ItemPlacement("EnergyDrink", new Placement(-8.6f, -5.5f)),
            },
            Enemies = new[]
            {
                // Faxineiro dos corredores do meio: vai e volta entre as fileiras 2 e 3 e as fileiras 1 e 2.
                new EnemyPlacement(EnemyType.Janitor, new[]
                {
                    new Vector3(-8.5f, 0f, -1.25f),
                    new Vector3(3.75f, 0f, -1.25f),
                    new Vector3(3.75f, 0f, 2.25f),
                    new Vector3(-8.5f, 0f, 2.25f),
                }, 0, 90f),
                // Faxineiro do corredor do fundo; começa longe da toca.
                new EnemyPlacement(EnemyType.Janitor, new[]
                {
                    new Vector3(-5f, 0f, 5.9f),
                    new Vector3(8.5f, 0f, 5.9f),
                    new Vector3(8.5f, 0f, 2.25f),
                }, 1, -90f),
                // Segurança da frente e da coluna da direita.
                new EnemyPlacement(EnemyType.Guard, new[]
                {
                    new Vector3(-8f, 0f, -5.7f),
                    new Vector3(8.3f, 0f, -5.7f),
                    new Vector3(8.3f, 0f, 2.25f),
                }, 0, 90f),
            },
            StoreLights = new[] { new Vector3(-3f, 2.4f, 2f), new Vector3(4f, 2.4f, -3f) },
        };

        /// <summary>
        /// Fase 4, Carrecinco: hipermercado de 26 × 20 m (GDD §11). Cinco fileiras de gôndolas com passagens largas,
        /// área aberta à direita e toca no fundo à esquerda. Os itens pesados ficam longe da toca: a lista passa do peso
        /// máximo de propósito, e o guaxinim precisa de várias viagens (ou da barra de proteína).
        /// </summary>
        private static LevelBlueprint Level04Carrecinco() => new LevelBlueprint
        {
            AssetName = "Level_04_Carrecinco",
            DisplayName = "Carrecinco",
            Columns = 13,
            Rows = 10,
            TargetTimeSeconds = 210f,
            ShoppingList = new[] { "Watermelon", "WaterJug", "Rice", "ToiletPaper", "Bread", "Banana" },
            Den = new Placement(-11.5f, 8.6f),
            RaccoonStart = new Placement(-10.6f, 7.5f, 180f),
            Shelves = Join(
                LongRow(6.5f, -7f, -1f, 5f),
                LongRow(3.5f, -7f, -1f, 5f),
                LongRow(0.5f, -7f, -1f, 5f),
                LongRow(-2.5f, -7f, -1f, 5f),
                LongRow(-5.5f, -7f, -1f, 5f)),
            Bins = new[]
            {
                new Placement(-12.3f, 4f, 90f),
                new Placement(12.3f, -5.5f, -90f),
                new Placement(2f, 9.3f, 180f),
                new Placement(-2f, -9.3f, 0f),
            },
            Items = new[]
            {
                new ItemPlacement("Watermelon", new Placement(11f, -8.5f)),
                new ItemPlacement("WaterJug", new Placement(10.8f, 1.5f, 15f)),
                new ItemPlacement("Rice", new Placement(1.5f, -8.3f, -10f)),
                new ItemPlacement("ToiletPaper", new Placement(-11.3f, -8.3f)),
                new ItemPlacement("Bread", new Placement(-1.5f, 6.12f, 0f, Base)),
                new ItemPlacement("Banana", new Placement(4.5f, -2.12f, 20f, Base)),
                new ItemPlacement("Pumpkin", new Placement(11.5f, 6f)),
                new ItemPlacement("Cereal", new Placement(-6.5f, 0.12f, 0f, Base)),
                new ItemPlacement("EnergyDrink", new Placement(-11.3f, 1f)),
                new ItemPlacement("EnergyDrink", new Placement(8.5f, -5f)),
                new ItemPlacement("ProteinBar", new Placement(8.5f, 8.4f)),
                new ItemPlacement("ProteinBar", new Placement(-4f, -8.4f)),
            },
            Enemies = new[]
            {
                // Faxineiros nos corredores da esquerda, cada um entre duas fileiras.
                new EnemyPlacement(EnemyType.Janitor, new[]
                {
                    new Vector3(-11.2f, 0f, 5f),
                    new Vector3(2f, 0f, 5f),
                    new Vector3(2f, 0f, 2f),
                    new Vector3(-11.2f, 0f, 2f),
                }, 1, -90f),
                new EnemyPlacement(EnemyType.Janitor, new[]
                {
                    new Vector3(-11.2f, 0f, -4f),
                    new Vector3(2f, 0f, -4f),
                    new Vector3(2f, 0f, -1f),
                    new Vector3(-11.2f, 0f, -1f),
                }, 0, 90f),
                // Segurança da área aberta da direita e da frente.
                new EnemyPlacement(EnemyType.Guard, new[]
                {
                    new Vector3(10.5f, 0f, 8.5f),
                    new Vector3(10.5f, 0f, -7.8f),
                    new Vector3(-8f, 0f, -7.8f),
                }, 1, 180f),
                // Segurança cruzando as fileiras pelas passagens do meio.
                new EnemyPlacement(EnemyType.Guard, new[]
                {
                    new Vector3(-4f, 0f, 8.5f),
                    new Vector3(-4f, 0f, -7.8f),
                    new Vector3(2f, 0f, -7.8f),
                    new Vector3(2f, 0f, 8.5f),
                }, 2, 0f),
            },
            StoreLights = new[] { new Vector3(-5f, 2.4f, 3f), new Vector3(6f, 2.4f, -3f) },
        };

        /// <summary>
        /// Fase 5, Atacadaço: atacado de 34 × 26 m (GDD §11), com seis fileiras longas de gôndolas de 4 m, uma área
        /// larga à direita e a lista mais longa. Três faxineiros nos corredores da esquerda e três seguranças em rotas
        /// que se cruzam. Pede várias viagens e o uso da barra de proteína.
        /// </summary>
        private static LevelBlueprint Level05Atacadaco() => new LevelBlueprint
        {
            AssetName = "Level_05_Atacadaco",
            DisplayName = "Atacadaço",
            Columns = 17,
            Rows = 13,
            TargetTimeSeconds = 270f,
            ShoppingList = new[] { "Watermelon", "Rice", "ToiletPaper", "GiantChips", "Eggs", "OliveOil", "Cheese" },
            Den = new Placement(-15.5f, 11.5f),
            RaccoonStart = new Placement(-14.6f, 10.4f, 180f),
            Shelves = Join(
                LongRow(9f, -12f, -6.5f, -1f, 4.5f),
                LongRow(5.5f, -12f, -6.5f, -1f, 4.5f),
                LongRow(2f, -12f, -6.5f, -1f, 4.5f),
                LongRow(-1.5f, -12f, -6.5f, -1f, 4.5f),
                LongRow(-5f, -12f, -6.5f, -1f, 4.5f),
                LongRow(-8.5f, -12f, -6.5f, -1f, 4.5f),
                new[]
                {
                    new ShelfPlacement(false, new Placement(9f, 5.5f)),
                    new ShelfPlacement(false, new Placement(9f, -1.5f)),
                    new ShelfPlacement(false, new Placement(9f, -8.5f)),
                }),
            Bins = new[]
            {
                new Placement(-16.3f, 6f, 90f),
                new Placement(16.3f, -8f, -90f),
                new Placement(6f, 12.3f, 180f),
                new Placement(-6f, -12.3f, 0f),
                new Placement(16.3f, 9f, -90f),
            },
            Items = new[]
            {
                new ItemPlacement("Watermelon", new Placement(15f, -11f)),
                new ItemPlacement("Rice", new Placement(14.5f, 4f, 10f)),
                new ItemPlacement("ToiletPaper", new Placement(-15.5f, -11f)),
                new ItemPlacement("GiantChips", new Placement(12.5f, 11f, -15f)),
                new ItemPlacement("Eggs", new Placement(-1.5f, 1.62f, 0f, Base)),
                new ItemPlacement("OliveOil", new Placement(4f, -4.62f, 10f, Base)),
                new ItemPlacement("Cheese", new Placement(-6f, 5.88f, -10f, Base)),
                new ItemPlacement("Pumpkin", new Placement(15.5f, -4f)),
                new ItemPlacement("Milk", new Placement(-12.5f, -8.12f, 0f, Base)),
                new ItemPlacement("EnergyDrink", new Placement(-15.5f, 2f)),
                new ItemPlacement("EnergyDrink", new Placement(11.5f, -8f)),
                new ItemPlacement("EnergyDrink", new Placement(0f, 11.2f)),
                new ItemPlacement("ProteinBar", new Placement(11.5f, 8.2f)),
                new ItemPlacement("ProteinBar", new Placement(-6f, -11f)),
                new ItemPlacement("ProteinBar", new Placement(15.5f, 0.5f)),
            },
            Enemies = new[]
            {
                // Três faxineiros, um para cada par de corredores da esquerda.
                new EnemyPlacement(EnemyType.Janitor, new[]
                {
                    new Vector3(-15.2f, 0f, 7.25f),
                    new Vector3(1.75f, 0f, 7.25f),
                    new Vector3(1.75f, 0f, 3.75f),
                    new Vector3(-15.2f, 0f, 3.75f),
                }, 1, -90f),
                new EnemyPlacement(EnemyType.Janitor, new[]
                {
                    new Vector3(-15.2f, 0f, 0.25f),
                    new Vector3(1.75f, 0f, 0.25f),
                    new Vector3(1.75f, 0f, -3.25f),
                    new Vector3(-15.2f, 0f, -3.25f),
                }, 0, 90f),
                new EnemyPlacement(EnemyType.Janitor, new[]
                {
                    new Vector3(-15.2f, 0f, -6.75f),
                    new Vector3(1.75f, 0f, -6.75f),
                    new Vector3(1.75f, 0f, -10.75f),
                    new Vector3(-15.2f, 0f, -10.75f),
                }, 2, -90f),
                // Segurança da área larga da direita.
                new EnemyPlacement(EnemyType.Guard, new[]
                {
                    new Vector3(13.5f, 0f, 11f),
                    new Vector3(13.5f, 0f, -10.75f),
                    new Vector3(11f, 0f, -10.75f),
                    new Vector3(11f, 0f, 11f),
                }, 1, 180f),
                // Seguranças cruzando as fileiras pelas passagens.
                new EnemyPlacement(EnemyType.Guard, new[]
                {
                    new Vector3(-9.25f, 0f, -10.75f),
                    new Vector3(-9.25f, 0f, 11f),
                    new Vector3(-3.75f, 0f, 11f),
                    new Vector3(-3.75f, 0f, -10.75f),
                }, 0, 0f),
                new EnemyPlacement(EnemyType.Guard, new[]
                {
                    new Vector3(7.25f, 0f, 11f),
                    new Vector3(7.25f, 0f, -10.75f),
                }, 1, 0f),
            },
            StoreLights = new[] { new Vector3(-6f, 2.4f, 2f), new Vector3(8f, 2.4f, -3f) },
        };
    }
}
