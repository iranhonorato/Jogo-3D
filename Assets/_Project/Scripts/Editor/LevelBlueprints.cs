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

        /// <summary>Ponto no chão (y = 0), para waypoints e poças de luz.</summary>
        private static Vector3 Floor(float x, float z) => new Vector3(x, 0f, z);

        /// <summary>
        /// Ronda de vai-e-volta: A, B, C e de volta a B (depois recomeça em A). Assim toda perna da ronda é uma linha
        /// reta que o jogador consegue prever, sem a volta em diagonal pelas gôndolas.
        /// </summary>
        private static Vector3[] PingPong(params Vector3[] points)
        {
            var route = new List<Vector3>(points);
            for (int i = points.Length - 2; i > 0; i--)
            {
                route.Add(points[i]);
            }

            return route.ToArray();
        }

        private static ShelfPlacement Long(float x, float z, float yawDegrees = 0f) =>
            new ShelfPlacement(true, new Placement(x, z, yawDegrees));

        private static ShelfPlacement Short(float x, float z, float yawDegrees = 0f) =>
            new ShelfPlacement(false, new Placement(x, z, yawDegrees));

        /// <summary>Item na prateleira de baixo de uma gôndola (a 0,38 m do centro dela, virado para o corredor).</summary>
        private static ItemPlacement OnShelf(string itemName, float x, float z, float yawDegrees = 0f) =>
            new ItemPlacement(itemName, new Placement(x, z, yawDegrees, Base));

        private static ItemPlacement OnFloor(string itemName, float x, float z, float yawDegrees = 0f) =>
            new ItemPlacement(itemName, new Placement(x, z, yawDegrees));

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
            Shelves = new[] { Long(-1.5f, 1.5f), Short(2.7f, 1.5f), Long(-1.5f, -1.5f), Short(2.7f, -1.5f) },
            NarrowGaps = new[] { new Placement(1.1f, 1.5f), new Placement(1.1f, -1.5f) },
            Bins = new[] { new Placement(-1f, 3.6f, 180f), new Placement(3.5f, 3.8f, 180f) },
            Items = new[]
            {
                OnShelf("Bread", -2.5f, 1.12f),
                OnShelf("Banana", 2.5f, -1.12f, 20f),
                OnShelf("Cheese", -1f, -1.88f, -15f),
                OnShelf("Cookies", 3f, 1.88f, 10f),
            },
            Enemies = new[]
            {
                // Começa no canto da frente, longe da toca, e dá a volta pelos dois corredores.
                new EnemyPlacement(EnemyType.Janitor,
                    new[] { Floor(5f, -3.5f), Floor(-5f, -3.5f), Floor(-5f, 0f), Floor(5f, 0f) }, 0, -90f),
            },
            StoreLights = new[] { new Vector3(0f, 2.4f, 3f), new Vector3(0f, 2.4f, -3f) },
            LightPools = new[] { Floor(0f, 3.3f), Floor(0f, -3.3f), Floor(-4.6f, 0f), Floor(4.6f, 0f) },
            HasTutorialTips = true,
        };

        /// <summary>
        /// Fase 2, XOXO 24h: loja de conveniência de 16 × 12 m, pequena e aberta (GDD §11). Três fileiras curtas à
        /// esquerda com os frágeis, um faxineiro dando a volta na fileira do meio, um segurança no corredor da frente e
        /// na área aberta da direita, toca no fundo à direita. Ensina os frágeis e o energético.
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
                Long(-3f, 2.5f), Short(1.7f, 2.5f), // fileira A, com passagem larga
                Long(-3f, -0.5f), Short(1.2f, -0.5f), // fileira B, com um vão estreito de 1,2 m
                Short(-5f, -3.5f), Short(-1.5f, -3.5f), // fileira C, duas gôndolas curtas
            },
            NarrowGaps = new[] { new Placement(-0.4f, -0.5f) },
            Bins = new[] { new Placement(6.8f, -1.8f, -90f), new Placement(-7.2f, -5.2f, 0f) },
            Items = new[]
            {
                OnShelf("Milk", -4f, 2.12f),
                OnShelf("Cookies", 1.5f, 2.88f, 10f),
                OnShelf("Juice", -3.5f, -0.88f),
                OnShelf("Jam", -1.5f, -3.12f, 15f),
                OnShelf("Cheese", -5f, -3.88f, -10f),
                OnFloor("EnergyDrink", 4.5f, -4f),
                OnFloor("EnergyDrink", -7.2f, -0.5f),
            },
            Enemies = new[]
            {
                // Faxineiro lento, dando a volta na fileira B, entre os frágeis.
                new EnemyPlacement(EnemyType.Janitor,
                    new[] { Floor(-6.6f, 1f), Floor(3.2f, 1f), Floor(3.2f, -2f), Floor(-6.6f, -2f) }, 0, 90f),
                // Segurança: corredor da frente e coluna da direita, em L; começa longe da toca.
                new EnemyPlacement(EnemyType.Guard, PingPong(Floor(-5f, -5f), Floor(5.8f, -5f), Floor(5.8f, 1f)), 0, 90f),
            },
            StoreLights = new[] { new Vector3(-2f, 2.4f, 1f), new Vector3(3f, 2.4f, -3f) },
            LightPools = new[] { Floor(-2f, 1f), Floor(4.5f, 3f), Floor(3f, -3f), Floor(-6f, -2f) },
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
                Long(-5f, 4f), Long(0.2f, 4f), Short(5f, 4f), // fileira 1, com vão estreito
                Long(-4.5f, 0.5f), Long(1f, 0.5f), Short(5.5f, 0.5f), // fileira 2, com passagens largas
                Long(-5f, -3f), Long(0.2f, -3f), Short(5f, -3f), // fileira 3, com vão estreito
            },
            NarrowGaps = new[] { new Placement(-2.4f, 4f), new Placement(-2.4f, -3f) },
            Bins = new[]
            {
                new Placement(-9.3f, 4.6f, 90f),
                new Placement(9.3f, -1f, -90f),
                new Placement(2.5f, 7.3f, 180f),
            },
            Items = new[]
            {
                OnShelf("Eggs", 0f, 0.12f),
                OnShelf("Cheese", -5.5f, -2.62f, 15f),
                OnFloor("GiantChips", 6f, -7.2f, 20f),
                OnShelf("Cereal", 5f, 3.62f),
                OnShelf("OliveOil", 1f, -3.38f, -10f),
                OnShelf("Popcorn", -4.5f, 4.38f),
                OnShelf("Bread", 5.2f, 0.88f, 10f),
                OnFloor("EnergyDrink", 9.2f, 5f),
                OnFloor("EnergyDrink", -9.2f, -4.2f),
            },
            Enemies = new[]
            {
                // Faxineiro dos corredores do meio: dá a volta na fileira 2.
                new EnemyPlacement(EnemyType.Janitor, new[]
                {
                    Floor(-8.5f, -1.25f), Floor(3.75f, -1.25f), Floor(3.75f, 2.25f), Floor(-8.5f, 2.25f),
                }, 0, 90f),
                // Faxineiro do corredor do fundo, vai e volta; começa longe da toca.
                new EnemyPlacement(EnemyType.Janitor, new[] { Floor(-5f, 5.9f), Floor(8.5f, 5.9f) }, 1, -90f),
                // Segurança: corredor da frente e coluna da direita, em L.
                new EnemyPlacement(EnemyType.Guard,
                    PingPong(Floor(-8f, -5.7f), Floor(8.3f, -5.7f), Floor(8.3f, 2.25f)), 0, 90f),
            },
            StoreLights = new[] { new Vector3(-3f, 2.4f, 2f), new Vector3(4f, 2.4f, -3f) },
            StoreLightRange = 9f,
            LightPools = new[]
            {
                Floor(-6f, 2.25f), Floor(2f, 2.25f), Floor(-2f, -1.25f), Floor(6f, -1.25f),
                Floor(0f, 5.9f), Floor(-4f, -5.7f), Floor(4f, -5.7f), Floor(8.3f, 0.5f),
            },
        };

        /// <summary>
        /// Fase 4, Carrecinco: hipermercado de 26 × 20 m (GDD §11). Dois blocos de gôndolas separados por um corredor
        /// largo que cruza a loja (onde um segurança vai e volta), caixas na frente, ilhas na área aberta da direita
        /// com os itens pesados e toca no fundo à esquerda. A lista passa do peso máximo de propósito: o guaxinim
        /// precisa de várias viagens (ou da barra de proteína). Três vãos estreitos: o fardo de papel higiênico
        /// volta pela coluna da esquerda.
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
            Shelves = new[]
            {
                // Bloco do fundo.
                Long(-7f, 7f), Long(-1.8f, 7f), Long(4f, 7f),
                Long(-7f, 4f), Short(-2f, 4f), Long(2.5f, 4f),
                // Bloco da frente, depois do corredor largo.
                Long(-8f, -1.5f), Long(-2f, -1.5f), Long(3.2f, -1.5f),
                Long(-8f, -4.5f), Long(-2.8f, -4.5f), Long(3.2f, -4.5f),
                // Caixas.
                Short(-8f, -7.5f), Short(-3f, -7.5f), Short(2f, -7.5f),
                // Ilhas da área aberta.
                Short(9.5f, 6f, 90f), Short(9.5f, -2f, 90f),
            },
            NarrowGaps = new[] { new Placement(-4.4f, 7f), new Placement(0.6f, -1.5f), new Placement(-5.4f, -4.5f) },
            Bins = new[]
            {
                new Placement(-12.3f, 4f, 90f),
                new Placement(-12.3f, -6.5f, 90f),
                new Placement(12.3f, -5.5f, -90f),
                new Placement(2f, 9.3f, 180f),
            },
            Items = new[]
            {
                OnFloor("Watermelon", 12.2f, -9.1f),
                OnFloor("WaterJug", 12.2f, -0.5f, 15f),
                OnFloor("Rice", 7.2f, -6.2f, -10f),
                OnFloor("ToiletPaper", -11.3f, -8.6f),
                OnShelf("Bread", -1.5f, 6.62f),
                OnShelf("Banana", 4f, -1.88f, 20f),
                OnFloor("Pumpkin", 12.2f, 6f),
                OnShelf("Cereal", -6.5f, 3.62f),
                OnFloor("EnergyDrink", -11.3f, 1f),
                OnFloor("EnergyDrink", 7.5f, -3f),
                OnFloor("ProteinBar", 7.5f, 8.6f),
                OnFloor("ProteinBar", -7.5f, -6f),
            },
            Enemies = new[]
            {
                // Faxineiros nos corredores de cada bloco, vai e volta.
                new EnemyPlacement(EnemyType.Janitor, new[] { Floor(-4f, 5.5f), Floor(7.5f, 5.5f) }, 1, -90f),
                new EnemyPlacement(EnemyType.Janitor, new[] { Floor(-11f, -3f), Floor(6.5f, -3f) }, 1, -90f),
                // Segurança da área aberta e da frente, em L.
                new EnemyPlacement(EnemyType.Guard,
                    PingPong(Floor(10.8f, 8.5f), Floor(10.8f, -8.9f), Floor(-6f, -8.9f)), 0, 180f),
                // Segurança do corredor largo; o lado da toca fica livre para a saída.
                new EnemyPlacement(EnemyType.Guard, new[] { Floor(-6f, 1.25f), Floor(8f, 1.25f) }, 1, -90f),
            },
            StoreLights = new[] { new Vector3(-4f, 2.4f, 1.25f), new Vector3(7f, 2.4f, -4f) },
            StoreLightRange = 10f,
            LightPools = new[]
            {
                Floor(-8f, 1.25f), Floor(0f, 1.25f), Floor(7f, 1.25f), Floor(10.8f, 5f), Floor(10.8f, -5f),
                Floor(-1f, 5.5f), Floor(0f, -3f), Floor(-3f, -8.9f), Floor(5f, -8.9f), Floor(-11.3f, -3f),
            },
        };

        /// <summary>
        /// Fase 5, Atacadaço: atacado de 34 × 26 m (GDD §11). À esquerda, dois blocos de fileiras de comprimentos
        /// variados, separados por um corredor largo; à direita, o depósito com ilhas de paletes e os itens pesados;
        /// na frente, a fila dos caixas. Três vãos estreitos, a lista mais longa, três faxineiros nos corredores e três
        /// seguranças (depósito, corredor largo e frente). Pede várias viagens e a barra de proteína.
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
            Shelves = new[]
            {
                // Bloco do fundo.
                Long(-12f, 10f), Long(-6.8f, 10f), Long(-1f, 10f), Long(4.5f, 10f),
                Long(-12f, 6.5f), Short(-7f, 6.5f), Long(-2.5f, 6.5f), Long(3.5f, 6.5f),
                Long(-12f, 3f), Long(-6f, 3f), Long(-0.2f, 3f), Short(4.8f, 3f),
                // Bloco da frente, depois do corredor largo.
                Long(-12f, -2.5f), Long(-6.5f, -2.5f), Long(-1.3f, -2.5f), Long(4f, -2.5f),
                Long(-12f, -6f), Long(-6.8f, -6f), Long(-0.5f, -6f), Long(5f, -6f),
                // Caixas.
                Short(-12f, -9.5f), Short(-7f, -9.5f), Short(-2f, -9.5f), Short(3f, -9.5f),
                // Depósito: ilhas de paletes.
                Short(10f, 8f, 90f), Short(10f, 1f, 90f), Short(10f, -6f, 90f),
                Short(14f, 8f, 90f), Short(14f, 1f, 90f), Short(14f, -6f, 90f),
            },
            NarrowGaps = new[] { new Placement(-9.4f, 10f), new Placement(-3.9f, -2.5f), new Placement(-9.4f, -6f) },
            Bins = new[]
            {
                new Placement(-16.3f, 6f, 90f),
                new Placement(16.3f, -5.5f, -90f),
                new Placement(6f, 12.3f, 180f),
                new Placement(-6f, -12.3f, 0f),
                new Placement(16.3f, 10f, -90f),
            },
            Items = new[]
            {
                OnFloor("Watermelon", 15.8f, -11.5f),
                OnFloor("Rice", 15.8f, 4.5f, 10f),
                OnFloor("ToiletPaper", -15.5f, -11.5f),
                OnFloor("GiantChips", 15.8f, 11.5f, -15f),
                OnShelf("Eggs", 0f, 2.62f),
                OnShelf("OliveOil", 4.5f, -6.38f, 10f),
                OnShelf("Cheese", -3f, 6.88f, -10f),
                OnFloor("Pumpkin", 15.8f, -3f),
                OnShelf("Milk", -12.5f, -6.38f),
                OnFloor("EnergyDrink", -15.5f, 2f),
                OnFloor("EnergyDrink", 16f, -8f),
                OnFloor("EnergyDrink", 0f, 12f),
                OnFloor("ProteinBar", 16f, 8f),
                OnFloor("ProteinBar", -5f, -11.5f),
                OnFloor("ProteinBar", -15.5f, -0.5f),
            },
            Enemies = new[]
            {
                // Três faxineiros, um em cada corredor comprido, vai e volta.
                new EnemyPlacement(EnemyType.Janitor, new[] { Floor(-8f, 8.25f), Floor(6f, 8.25f) }, 1, -90f),
                new EnemyPlacement(EnemyType.Janitor, new[] { Floor(-4f, 4.75f), Floor(6.5f, 4.75f) }, 1, -90f),
                new EnemyPlacement(EnemyType.Janitor, new[] { Floor(-15.5f, -4.25f), Floor(6.5f, -4.25f) }, 1, -90f),
                // Segurança do depósito: dá a volta nas ilhas da esquerda.
                new EnemyPlacement(EnemyType.Guard, new[]
                {
                    Floor(12f, 11.2f), Floor(12f, -11.2f), Floor(8.2f, -11.2f), Floor(8.2f, 11.2f),
                }, 1, -90f),
                // Segurança do corredor largo e segurança da frente (entre as gôndolas e os caixas).
                new EnemyPlacement(EnemyType.Guard, new[] { Floor(-10f, 0.25f), Floor(7.5f, 0.25f) }, 1, -90f),
                new EnemyPlacement(EnemyType.Guard, new[] { Floor(-10f, -7.75f), Floor(6.5f, -7.75f) }, 1, -90f),
            },
            StoreLights = new[] { new Vector3(-5f, 2.4f, 0.25f), new Vector3(12f, 2.4f, -2f) },
            StoreLightRange = 12f,
            LightPools = new[]
            {
                Floor(-12f, 0.25f), Floor(-4f, 0.25f), Floor(4f, 0.25f), Floor(12f, 5f), Floor(12f, -3f),
                Floor(-8f, -7.75f), Floor(2f, -7.75f), Floor(-4f, 11.5f), Floor(4f, 11.5f), Floor(-15.5f, -6f),
                Floor(-2f, 8.25f), Floor(-2f, -4.25f), Floor(14f, -10f),
            },
        };
    }
}
