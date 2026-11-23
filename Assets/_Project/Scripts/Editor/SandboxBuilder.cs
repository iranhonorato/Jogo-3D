using Unity.AI.Navigation;
using Unity.AI.Navigation.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Monta na cena aberta um mercadinho de teste com tudo o que já existe no jogo: gera os prefabs,
    /// põe piso, paredes e gôndolas, o guaxinim com a câmera, um segurança com waypoints e o NavMesh.
    /// Rodar de novo refaz tudo. A cena não é salva sozinha.
    /// </summary>
    public static class SandboxBuilder
    {
        private const string RootName = "Sandbox";
        private const int FloorColumns = 8;
        private const int FloorRows = 6;

        private static readonly Vector3 RaccoonStart = new Vector3(-6.5f, 0f, 4.5f);

        private static readonly Vector3[] GuardWaypoints =
        {
            new Vector3(-3.5f, 0f, 0.5f),
            new Vector3(3.5f, 0f, 0.5f),
            new Vector3(3.5f, 0f, -4f),
            new Vector3(-3.5f, 0f, -4f),
        };

        [MenuItem("Tools/Guaxinim/Montar Sandbox de Teste")]
        private static void BuildSandbox()
        {
            LevelBuildKit.GenerateAllPrefabs();
            RemovePreviousSandbox();

            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Build sandbox");
            LevelBuildKit.BuildRoom(root.transform, FloorColumns, FloorRows);
            BuildShelves(root.transform);
            LevelBuildKit.Place(EnvironmentKitFactory.DenPath, root.transform, new Vector3(6f, 0f, -4.5f), 0f);
            GameObject raccoon = LevelBuildKit.PlacePlayer(root.transform, RaccoonStart, 0f);
            LevelBuildKit.PlaceGuard(root.transform, GuardFactory.GuardPrefabPath, GuardWaypoints, 3, 90f);

            NavMeshSurface surface = LevelBuildKit.AddNavMeshSurface(root);
            NavMeshAssetManager.instance.StartBakingSurfaces(new Object[] { surface });

            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = raccoon;
        }

        private static void RemovePreviousSandbox()
        {
            GameObject previous = GameObject.Find(RootName);
            if (previous != null)
            {
                Undo.DestroyObjectImmediate(previous);
            }
        }

        private static void BuildShelves(Transform root)
        {
            LevelBuildKit.Place(EnvironmentKitFactory.Shelf4mPath, root, new Vector3(0f, 0f, 2.5f), 0f);
            LevelBuildKit.Place(EnvironmentKitFactory.Shelf4mPath, root, new Vector3(0f, 0f, -1.5f), 0f);
            LevelBuildKit.Place(EnvironmentKitFactory.Shelf2mPath, root, new Vector3(-5f, 0f, 1f), 90f);
            LevelBuildKit.Place(EnvironmentKitFactory.Shelf2mPath, root, new Vector3(5f, 0f, 1f), 90f);
        }
    }
}
