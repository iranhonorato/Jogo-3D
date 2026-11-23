using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Monta na cena aberta uma vitrine com os modelos de Art/Models, para conferir escala e
    /// materiais lado a lado. Tudo fica sob o objeto "ModelShowcase", que pode ser apagado depois.
    /// A cena não é salva sozinha.
    /// </summary>
    public static class ModelShowcaseMenu
    {
        private const string MenuPath = "Tools/Guaxinim/Vitrine de Modelos";
        private const string ModelsFolder = "Assets/_Project/Art/Models/";
        private const string RootName = "ModelShowcase";
        private const float TileSize = 2f;
        private const int FloorColumns = 5;
        private const int FloorRows = 3;
        private const float BackWallZ = 5.1f;
        private const float CeilingHeight = 2.5f;

        private static readonly Quaternion FacingCamera = Quaternion.Euler(0f, 180f, 0f);

        [MenuItem(MenuPath)]
        private static void BuildShowcase()
        {
            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Build model showcase");

            BuildFloorAndWall(root.transform);
            Place(root.transform, "Environment/Shelf_Aisle_4m", new Vector3(-2f, 0f, 3.5f), Quaternion.identity);
            Place(root.transform, "Environment/Shelf_Aisle_2m", new Vector3(3f, 0f, 3.5f), Quaternion.identity);
            Place(root.transform, "Environment/Den_Manhole", new Vector3(-3.5f, 0f, 0.5f), Quaternion.identity);
            Place(root.transform, "Characters/Char_Raccoon", new Vector3(-1.5f, 0f, 1f), FacingCamera);
            Place(root.transform, "Characters/Char_Guard", new Vector3(0f, 0f, 1f), FacingCamera);
            Place(root.transform, "Characters/Char_SeuZe", new Vector3(1.5f, 0f, 1f), FacingCamera);
            Place(root.transform, "Hazards/Hazard_SecurityCamera", new Vector3(3.5f, CeilingHeight, 1.5f), FacingCamera);

            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        private static void BuildFloorAndWall(Transform root)
        {
            float firstX = -TileSize * (FloorColumns - 1) / 2f;
            for (int column = 0; column < FloorColumns; column++)
            {
                float x = firstX + column * TileSize;
                for (int row = 0; row < FloorRows; row++)
                {
                    Place(root, "Environment/Floor_2m", new Vector3(x, 0f, row * TileSize), Quaternion.identity);
                }

                Place(root, "Environment/Wall_2m", new Vector3(x, 0f, BackWallZ), Quaternion.identity);
            }
        }

        private static void Place(Transform root, string modelPath, Vector3 position, Quaternion rotation)
        {
            string assetPath = ModelsFolder + modelPath + ".fbx";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (model == null)
            {
                Debug.LogError($"Model not found: {assetPath}");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, root);
            instance.transform.SetPositionAndRotation(position, rotation);
        }
    }
}
