using Guaxinim.Shared;
using UnityEditor;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera os prefabs do kit de cenário (passo 1.4, GDD §11 e §14.4) em Prefabs/Environment/:
    /// cada um tem o modelo do Blender e um BoxCollider simples. Paredes e gôndolas ficam na layer
    /// Obstacle (bloqueiam a visão dos seguranças); o piso fica na Default. Todos são estáticos para batching.
    /// </summary>
    public static class EnvironmentKitFactory
    {
        public const string FloorPath = "Assets/_Project/Prefabs/Environment/Floor_2m.prefab";
        public const string WallPath = "Assets/_Project/Prefabs/Environment/Wall_2m.prefab";
        public const string Shelf2mPath = "Assets/_Project/Prefabs/Environment/Shelf_Aisle_2m.prefab";
        public const string Shelf4mPath = "Assets/_Project/Prefabs/Environment/Shelf_Aisle_4m.prefab";
        public const string DenPath = "Assets/_Project/Prefabs/Environment/Den_Manhole.prefab";

        private const string ModelsFolder = "Assets/_Project/Art/Models/Environment/";
        private const float TileSize = 2f;
        private const float FloorThickness = 0.2f;
        private const float WallHeight = 2.5f;
        private const float WallThickness = 0.2f;
        private const float ShelfHeight = 1.8f;
        private const float ShelfDepth = 0.9f;

        [MenuItem("Tools/Guaxinim/Greybox/Kit de Cenário")]
        internal static void CreateKitPrefabs()
        {
            int obstacle = LayerMask.NameToLayer(Layers.Obstacle);
            int defaultLayer = LayerMask.NameToLayer("Default");

            // O piso tem o topo em y = 0, então o collider fica abaixo; o resto fica apoiado no chão.
            CreatePiece("Floor_2m", FloorPath, new Bounds(Vector3.down * FloorThickness / 2f,
                new Vector3(TileSize, FloorThickness, TileSize)), defaultLayer);
            CreatePiece("Wall_2m", WallPath, StandingBox(TileSize, WallHeight, WallThickness), obstacle);
            CreatePiece("Shelf_Aisle_2m", Shelf2mPath, StandingBox(2f, ShelfHeight, ShelfDepth), obstacle);
            CreatePiece("Shelf_Aisle_4m", Shelf4mPath, StandingBox(4f, ShelfHeight, ShelfDepth), obstacle);
            CreatePiece("Den_Manhole", DenPath, null, defaultLayer);
        }

        private static Bounds StandingBox(float width, float height, float depth) =>
            new Bounds(Vector3.up * height / 2f, new Vector3(width, height, depth));

        private static void CreatePiece(string modelName, string prefabPath, Bounds? collider, int layer)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelsFolder + modelName + ".fbx");
            if (model == null)
            {
                Debug.LogError($"Model not found: {ModelsFolder}{modelName}.fbx");
                return;
            }

            var root = new GameObject(modelName);
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
            visual.name = "Model";

            if (collider.HasValue)
            {
                var box = root.AddComponent<BoxCollider>();
                box.center = collider.Value.center;
                box.size = collider.Value.size;
            }

            EditorAssets.SetLayerRecursively(root, layer);
            SetStaticRecursively(root);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        private static void SetStaticRecursively(GameObject target)
        {
            GameObjectUtility.SetStaticEditorFlags(target, StaticEditorFlags.BatchingStatic);
            foreach (Transform child in target.transform)
            {
                SetStaticRecursively(child.gameObject);
            }
        }
    }
}
