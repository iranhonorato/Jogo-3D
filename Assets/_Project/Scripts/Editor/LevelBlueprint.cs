using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Descrição de uma fase como dado (plano de próximos passos, Etapa 2): o <see cref="LevelSceneBuilder"/> transforma
    /// isto numa cena, e os testes conferem a progressão entre as fases. Medidas em metros, com a sala centrada na
    /// origem e o fundo da loja em +Z.
    /// </summary>
    public class LevelBlueprint
    {
        /// <summary>Altura do topo da base das gôndolas, onde ficam os itens da prateleira de baixo.</summary>
        public const float ShelfBaseTop = 0.12f;

        /// <summary>Nome da cena e do LevelData (ex.: "Level_01_Mercadinho").</summary>
        public string AssetName { get; set; }

        public string DisplayName { get; set; }

        /// <summary>Largura da sala em pisos de 2 m (eixo X).</summary>
        public int Columns { get; set; }

        /// <summary>Profundidade da sala em pisos de 2 m (eixo Z).</summary>
        public int Rows { get; set; }

        public float TargetTimeSeconds { get; set; }

        /// <summary>Itens obrigatórios, pelos nomes da tabela do GDD §7.8.</summary>
        public string[] ShoppingList { get; set; } = new string[0];

        public Placement Den { get; set; }
        public Placement RaccoonStart { get; set; }
        public ShelfPlacement[] Shelves { get; set; } = new ShelfPlacement[0];

        /// <summary>Vãos de 1,2 m entre gôndolas: atalho só do guaxinim sem carga volumosa (GDD §7.4).</summary>
        public Placement[] NarrowGaps { get; set; } = new Placement[0];

        public Placement[] Bins { get; set; } = new Placement[0];

        /// <summary>Todos os itens do mundo: os da lista, os bônus e os consumíveis.</summary>
        public ItemPlacement[] Items { get; set; } = new ItemPlacement[0];

        public EnemyPlacement[] Enemies { get; set; } = new EnemyPlacement[0];

        /// <summary>Luzes quentes da loja, além da lua (GDD §14.6: no máximo 4 ou 5 em tempo real com as lanternas).</summary>
        public Vector3[] StoreLights { get; set; } = new Vector3[0];

        /// <summary>A fase liga as dicas do tutorial (só a Fase 1).</summary>
        public bool HasTutorialTips { get; set; }

        public float Width => Columns * LevelBuildKit.TileSize;
        public float Depth => Rows * LevelBuildKit.TileSize;
        public float Area => Width * Depth;

        /// <summary>O ponto está dentro da sala, a pelo menos <paramref name="margin"/> metros das paredes?</summary>
        public bool IsInsideRoom(Vector3 point, float margin) =>
            Mathf.Abs(point.x) <= Width / 2f - margin && Mathf.Abs(point.z) <= Depth / 2f - margin;

        public int CountEnemies(EnemyType type)
        {
            int count = 0;
            foreach (EnemyPlacement enemy in Enemies)
            {
                if (enemy.Type == type)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
