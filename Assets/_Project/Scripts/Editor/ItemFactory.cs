using Guaxinim.Items;
using Guaxinim.Shared;
using UnityEditor;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera os itens da tabela do GDD §7.8 (passo 2.1): um ItemData em Data/Items/ e um prefab greybox em
    /// Prefabs/Items/ para cada um. O prefab é uma caixa na cor saturada da categoria (GDD §15.2: itens se
    /// destacam do cenário), com tamanho pelo volume, trigger na layer Interactable e o PickupItem.
    /// Um ItemData que já existe não é sobrescrito (mantém os números ajustados).
    /// </summary>
    public static class ItemFactory
    {
        public const string PrefabsFolder = "Assets/_Project/Prefabs/Items";

        private const string DataFolder = "Assets/_Project/Data/Items";
        private const string MaterialsFolder = "Assets/_Project/Art/Materials/Greybox";
        private const float BaseSize = 0.18f;
        private const float SizePerVolume = 0.07f;
        private const float TriggerPadding = 0.1f;

        private static readonly ItemRow[] Rows =
        {
            new ItemRow("Bread", "Pão de forma", 1f, 1f),
            new ItemRow("Banana", "Banana", 1f, 1f),
            new ItemRow("Cheese", "Queijo", 1f, 1f),
            new ItemRow("Cookies", "Pacote de biscoito", 1f, 1f),
            new ItemRow("Milk", "Caixa de leite", 1f, 1f),
            new ItemRow("Watermelon", "Melancia", 6f, 2f),
            new ItemRow("WaterJug", "Galão de água", 5f, 2f),
            new ItemRow("Rice", "Saco de arroz 5 kg", 5f, 2f),
            new ItemRow("Pumpkin", "Abóbora", 4f, 2f),
            new ItemRow("GiantChips", "Salgadinho gigante", 1f, 4f),
            new ItemRow("ToiletPaper", "Fardo de papel higiênico", 2f, 5f),
            new ItemRow("Cereal", "Caixa de cereal", 1f, 3f),
            new ItemRow("Popcorn", "Pipoca de micro-ondas", 1f, 3f),
            new ItemRow("Juice", "Garrafa de suco", 2f, 1f, isFragile: true),
            new ItemRow("Eggs", "Caixa de ovos", 1f, 2f, isFragile: true),
            new ItemRow("Jam", "Pote de geleia", 1f, 1f, isFragile: true),
            new ItemRow("OliveOil", "Azeite", 2f, 1f, isFragile: true),
            new ItemRow("EnergyDrink", "Energético", 0f, 0f, isConsumable: true),
            new ItemRow("ProteinBar", "Barra de proteína", 0f, 0f, isConsumable: true),
        };

        public static string PrefabPath(string itemName) => $"{PrefabsFolder}/Item_{itemName}.prefab";

        [MenuItem("Tools/Guaxinim/Greybox/Itens da Tabela")]
        internal static void CreateItems()
        {
            EditorAssets.EnsureFolder(DataFolder);
            EditorAssets.EnsureFolder(PrefabsFolder);
            foreach (ItemRow row in Rows)
            {
                ItemData data = LoadOrCreateData(row);
                CreatePrefab(row, data);
            }
        }

        private static ItemData LoadOrCreateData(ItemRow row)
        {
            string path = $"{DataFolder}/Item_{row.Name}.asset";
            var data = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (data != null)
            {
                return data;
            }

            data = ScriptableObject.CreateInstance<ItemData>();
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_displayName").stringValue = row.DisplayName;
            serialized.FindProperty("_weight").floatValue = row.Weight;
            serialized.FindProperty("_volume").floatValue = row.Volume;
            serialized.FindProperty("_isFragile").boolValue = row.IsFragile;
            serialized.FindProperty("_isConsumable").boolValue = row.IsConsumable;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static void CreatePrefab(ItemRow row, ItemData data)
        {
            float size = BaseSize + SizePerVolume * row.Volume;
            var root = new GameObject($"Item_{row.Name}");
            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = Vector3.one * (size + TriggerPadding);
            trigger.center = Vector3.up * size / 2f;
            var pickup = root.AddComponent<PickupItem>();
            EditorAssets.SetReference(pickup, "_data", data);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = Vector3.one * size;
            visual.transform.localPosition = Vector3.up * size / 2f;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<MeshRenderer>().sharedMaterial = CategoryMaterial(row);

            EditorAssets.SetLayerRecursively(root, LayerMask.NameToLayer(Layers.Interactable));
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath(row.Name));
            Object.DestroyImmediate(root);
        }

        private static Material CategoryMaterial(ItemRow row)
        {
            (string category, Color color) = row switch
            {
                { IsConsumable: true } => ("Consumable", new Color(0.95f, 0.2f, 0.8f)),
                { IsFragile: true } => ("Fragile", new Color(0.2f, 0.9f, 0.95f)),
                { Volume: >= 3f } => ("Bulky", new Color(0.25f, 0.45f, 1f)),
                { Weight: >= 4f } => ("Heavy", new Color(1f, 0.45f, 0.1f)),
                _ => ("Normal", new Color(1f, 0.85f, 0.15f)),
            };
            return EditorAssets.LoadOrCreateLitMaterial($"{MaterialsFolder}/M_Item_{category}.mat", color);
        }

        private readonly struct ItemRow
        {
            public ItemRow(string name, string displayName, float weight, float volume, bool isFragile = false,
                bool isConsumable = false)
            {
                Name = name;
                DisplayName = displayName;
                Weight = weight;
                Volume = volume;
                IsFragile = isFragile;
                IsConsumable = isConsumable;
            }

            public string Name { get; }
            public string DisplayName { get; }
            public float Weight { get; }
            public float Volume { get; }
            public bool IsFragile { get; }
            public bool IsConsumable { get; }
        }
    }
}
