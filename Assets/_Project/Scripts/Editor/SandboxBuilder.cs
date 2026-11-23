using Guaxinim.Shared;
using Unity.AI.Navigation;
using Unity.AI.Navigation.Editor;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Monta na cena aberta um mercadinho de teste com tudo o que já existe no jogo: gera os
    /// prefabs (guaxinim, câmeras, som e kit), põe piso, paredes e gôndolas, o guaxinim, o rig de
    /// câmera ligado a ele, o LevelSystems e o NavMesh. Rodar de novo refaz tudo. A cena não é salva sozinha.
    /// </summary>
    public static class SandboxBuilder
    {
        private const string RootName = "Sandbox";
        private const string MainCameraTag = "MainCamera";
        private const int FloorColumns = 8;
        private const int FloorRows = 6;
        private const float TileSize = 2f;
        private const float WallThicknessOffset = 0.1f;

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
            GenerateAllPrefabs();
            RemovePreviousSandbox();

            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Build sandbox");
            BuildFloorAndWalls(root.transform);
            BuildShelves(root.transform);
            Place(EnvironmentKitFactory.DenPath, root.transform, new Vector3(6f, 0f, -4.5f), 0f);
            GameObject systems = Place(LevelSystemsFactory.SystemsPrefabPath, root.transform, Vector3.zero, 0f);

            GameObject raccoon = Place(GreyboxRaccoonFactory.PrefabPath, root.transform, RaccoonStart, 0f);
            EditorAssets.SetReference(systems.GetComponent<Core.LevelFlow>(), "_player",
                raccoon.GetComponent<Player.PlayerState>());
            GameObject cameraRig = Place(CameraRigFactory.PerspectivePath, root.transform, Vector3.zero, 0f);
            DisableOtherMainCameras(cameraRig);
            ConnectCamera(cameraRig, raccoon);
            PlaceGuard(root.transform);
            BakeNavMesh(root);

            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = raccoon;
        }

        private static void GenerateAllPrefabs()
        {
            EnvironmentKitFactory.CreateKitPrefabs();
            LevelSystemsFactory.CreateLevelSystemsPrefab();
            GreyboxRaccoonFactory.CreateRaccoonPrefab();
            CameraRigFactory.CreateCameraRigs();
            GuardFactory.CreateGuardPrefab();
        }

        private static void PlaceGuard(Transform root)
        {
            var waypointsRoot = new GameObject("GuardWaypoints").transform;
            waypointsRoot.SetParent(root, false);
            var waypoints = new Object[GuardWaypoints.Length];
            for (int i = 0; i < GuardWaypoints.Length; i++)
            {
                var waypoint = new GameObject($"Waypoint_{i}").transform;
                waypoint.SetParent(waypointsRoot, false);
                waypoint.position = GuardWaypoints[i];
                waypoints[i] = waypoint;
            }

            GameObject guard = Place(GuardFactory.GuardPrefabPath, root, GuardWaypoints[GuardWaypoints.Length - 1], 90f);
            EditorAssets.SetReferenceArray(guard.GetComponent<Guards.GuardAI>(), "_waypoints", waypoints);
        }

        private static void RemovePreviousSandbox()
        {
            GameObject previous = GameObject.Find(RootName);
            if (previous != null)
            {
                Undo.DestroyObjectImmediate(previous);
            }
        }

        private static void BuildFloorAndWalls(Transform root)
        {
            float halfWidth = FloorColumns * TileSize / 2f;
            float halfDepth = FloorRows * TileSize / 2f;
            for (int column = 0; column < FloorColumns; column++)
            {
                float x = -halfWidth + TileSize * (column + 0.5f);
                for (int row = 0; row < FloorRows; row++)
                {
                    float z = -halfDepth + TileSize * (row + 0.5f);
                    Place(EnvironmentKitFactory.FloorPath, root, new Vector3(x, 0f, z), 0f);
                }

                Place(EnvironmentKitFactory.WallPath, root, new Vector3(x, 0f, halfDepth + WallThicknessOffset), 0f);
                Place(EnvironmentKitFactory.WallPath, root, new Vector3(x, 0f, -halfDepth - WallThicknessOffset), 0f);
            }

            for (int row = 0; row < FloorRows; row++)
            {
                float z = -halfDepth + TileSize * (row + 0.5f);
                Place(EnvironmentKitFactory.WallPath, root, new Vector3(halfWidth + WallThicknessOffset, 0f, z), 90f);
                Place(EnvironmentKitFactory.WallPath, root, new Vector3(-halfWidth - WallThicknessOffset, 0f, z), 90f);
            }
        }

        private static void BuildShelves(Transform root)
        {
            Place(EnvironmentKitFactory.Shelf4mPath, root, new Vector3(0f, 0f, 2.5f), 0f);
            Place(EnvironmentKitFactory.Shelf4mPath, root, new Vector3(0f, 0f, -1.5f), 0f);
            Place(EnvironmentKitFactory.Shelf2mPath, root, new Vector3(-5f, 0f, 1f), 90f);
            Place(EnvironmentKitFactory.Shelf2mPath, root, new Vector3(5f, 0f, 1f), 90f);
        }

        private static GameObject Place(string prefabPath, Transform parent, Vector3 position, float yawDegrees)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yawDegrees, 0f));
            return instance;
        }

        private static void DisableOtherMainCameras(GameObject cameraRig)
        {
            foreach (GameObject cameraObject in GameObject.FindGameObjectsWithTag(MainCameraTag))
            {
                if (!cameraObject.transform.IsChildOf(cameraRig.transform))
                {
                    Undo.RecordObject(cameraObject, "Disable old main camera");
                    cameraObject.SetActive(false);
                }
            }
        }

        private static void ConnectCamera(GameObject cameraRig, GameObject raccoon)
        {
            Transform mainCamera = cameraRig.GetComponentInChildren<Camera>().transform;
            var controller = raccoon.GetComponent<Player.RaccoonController>();
            EditorAssets.SetReference(controller, "_cameraTransform", mainCamera);

            var isometricCamera = cameraRig.GetComponentInChildren<CinemachineCamera>();
            isometricCamera.Target.TrackingTarget = raccoon.transform;
            PrefabUtility.RecordPrefabInstancePropertyModifications(isometricCamera); // senão se perde ao salvar
        }

        private static void BakeNavMesh(GameObject root)
        {
            var surface = root.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~LayerMask.GetMask(Layers.Player, Layers.Guard); // personagens não viram obstáculo
            NavMeshAssetManager.instance.StartBakingSurfaces(new Object[] { surface });
        }
    }
}
