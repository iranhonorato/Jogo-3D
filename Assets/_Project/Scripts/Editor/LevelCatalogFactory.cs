using System.IO;
using Guaxinim.Core;
using UnityEditor;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Mantém o catálogo das 5 fases (GDD §11): cria o <see cref="LevelData"/> de cada uma, com id e nome do mercado,
    /// e só marca a cena quando o arquivo dela existe. Uma fase sem cena aparece como "Em breve" no mapa. Rodar de
    /// novo depois de montar uma fase nova atualiza tudo, sem apagar a lista e o tempo já ajustados nos assets.
    /// </summary>
    public static class LevelCatalogFactory
    {
        public const string CatalogPath = "Assets/_Project/Data/Levels/LevelCatalog.asset";

        private const string DataFolder = "Assets/_Project/Data/Levels";
        private const string ScenesFolder = "Assets/_Project/Scenes/Levels";

        // GDD §11 (nomes e ordem) e §14.2 (nomes das cenas).
        private static readonly LevelRow[] Rows =
        {
            new LevelRow("Level_01_Mercadinho", "level_01_mercadinho", "Mercadinho do Seu Zé"),
            new LevelRow("Level_02_XOXO", "level_02_xoxo", "XOXO 24h"),
            new LevelRow("Level_03_PaoDeMel", "level_03_pao_de_mel", "Pão de Mel"),
            new LevelRow("Level_04_Carrecinco", "level_04_carrecinco", "Carrecinco"),
            new LevelRow("Level_05_Atacadaco", "level_05_atacadaco", "Atacadaço"),
        };

        public static LevelCatalog LoadOrCreateCatalog() => EditorAssets.LoadOrCreate<LevelCatalog>(CatalogPath);

        [MenuItem("Tools/Guaxinim/Catálogo de Fases")]
        public static void CreateOrUpdate()
        {
            EditorAssets.EnsureFolder(DataFolder);
            var levels = new Object[Rows.Length];
            for (int i = 0; i < Rows.Length; i++)
            {
                levels[i] = CreateOrUpdateLevel(Rows[i]);
            }

            EditorAssets.SetReferenceArray(LoadOrCreateCatalog(), "_levels", levels);
            AssetDatabase.SaveAssets();
        }

        private static LevelData CreateOrUpdateLevel(LevelRow row)
        {
            string path = $"{DataFolder}/{row.AssetName}.asset";
            bool isNew = AssetDatabase.LoadAssetAtPath<LevelData>(path) == null;
            var level = EditorAssets.LoadOrCreate<LevelData>(path);

            var serialized = new SerializedObject(level);
            serialized.FindProperty("_id").stringValue = row.Id;
            serialized.FindProperty("_sceneName").stringValue = SceneExists(row.AssetName) ? row.AssetName : string.Empty;
            if (isNew)
            {
                serialized.FindProperty("_displayName").stringValue = row.DisplayName;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            return level;
        }

        private static bool SceneExists(string sceneName) => File.Exists($"{ScenesFolder}/{sceneName}.unity");

        private readonly struct LevelRow
        {
            public LevelRow(string assetName, string id, string displayName)
            {
                AssetName = assetName;
                Id = id;
                DisplayName = displayName;
            }

            /// <summary>Nome do asset de dados e da cena (os dois são iguais).</summary>
            public string AssetName { get; }

            public string Id { get; }
            public string DisplayName { get; }
        }
    }
}
