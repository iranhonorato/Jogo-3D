using Guaxinim.Core;
using Guaxinim.Guards;
using Guaxinim.Player;
using Guaxinim.Shared;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Peças comuns para montar uma fase por script (Sandbox e fases): gerar os prefabs, a sala de
    /// piso e paredes, o guaxinim com a câmera, os seguranças com waypoints e o NavMesh.
    /// </summary>
    public static class LevelBuildKit
    {
        public const float TileSize = 2f;

        private const string MainCameraTag = "MainCamera";
        private const float WallThicknessOffset = 0.1f;

        /// <summary>Gera (ou atualiza) todos os prefabs usados nas fases.</summary>
        public static void GenerateAllPrefabs()
        {
            EnvironmentKitFactory.CreateKitPrefabs();
            LevelSystemsFactory.CreateLevelSystemsPrefab();
            GreyboxRaccoonFactory.CreateRaccoonPrefab();
            CameraRigFactory.CreateCameraRigs();
            GuardFactory.CreateGuardPrefabs();
        }

        public static GameObject Place(string prefabPath, Transform parent, Vector3 position, float yawDegrees)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yawDegrees, 0f));
            return instance;
        }

        /// <summary>Piso de columns × rows placas de 2 m, centrado na origem, com paredes em volta.</summary>
        public static void BuildRoom(Transform root, int columns, int rows)
        {
            float halfWidth = columns * TileSize / 2f;
            float halfDepth = rows * TileSize / 2f;
            for (int column = 0; column < columns; column++)
            {
                float x = -halfWidth + TileSize * (column + 0.5f);
                for (int row = 0; row < rows; row++)
                {
                    float z = -halfDepth + TileSize * (row + 0.5f);
                    Place(EnvironmentKitFactory.FloorPath, root, new Vector3(x, 0f, z), 0f);
                }

                Place(EnvironmentKitFactory.WallPath, root, new Vector3(x, 0f, halfDepth + WallThicknessOffset), 0f);
                Place(EnvironmentKitFactory.WallPath, root, new Vector3(x, 0f, -halfDepth - WallThicknessOffset), 0f);
            }

            for (int row = 0; row < rows; row++)
            {
                float z = -halfDepth + TileSize * (row + 0.5f);
                Place(EnvironmentKitFactory.WallPath, root, new Vector3(halfWidth + WallThicknessOffset, 0f, z), 90f);
                Place(EnvironmentKitFactory.WallPath, root, new Vector3(-halfWidth - WallThicknessOffset, 0f, z), 90f);
            }
        }

        /// <summary>Põe o LevelSystems, o guaxinim e o rig de câmera, e liga tudo entre si.</summary>
        public static GameObject PlacePlayer(Transform root, Vector3 start, float yawDegrees)
        {
            GameObject systems = Place(LevelSystemsFactory.SystemsPrefabPath, root, Vector3.zero, 0f);
            GameObject raccoon = Place(GreyboxRaccoonFactory.PrefabPath, root, start, yawDegrees);
            EditorAssets.SetReference(systems.GetComponent<LevelFlow>(), "_player", raccoon.GetComponent<PlayerState>());

            GameObject cameraRig = Place(CameraRigFactory.PerspectivePath, root, Vector3.zero, 0f);
            DisableOtherMainCameras(cameraRig);
            ConnectCamera(cameraRig, raccoon);
            return raccoon;
        }

        public static GameObject PlaceGuard(Transform root, string prefabPath, Vector3[] waypointPositions,
            int startWaypoint, float yawDegrees)
        {
            var waypointsRoot = new GameObject("GuardWaypoints").transform;
            waypointsRoot.SetParent(root, false);
            var waypoints = new Object[waypointPositions.Length];
            for (int i = 0; i < waypointPositions.Length; i++)
            {
                var waypoint = new GameObject($"Waypoint_{i}").transform;
                waypoint.SetParent(waypointsRoot, false);
                waypoint.position = waypointPositions[i];
                waypoints[i] = waypoint;
            }

            GameObject guard = Place(prefabPath, root, waypointPositions[startWaypoint], yawDegrees);
            EditorAssets.SetReferenceArray(guard.GetComponent<GuardAI>(), "_waypoints", waypoints);
            return guard;
        }

        /// <summary>NavMeshSurface com os colliders dos filhos; personagens não viram obstáculo.</summary>
        public static NavMeshSurface AddNavMeshSurface(GameObject root)
        {
            var surface = root.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~LayerMask.GetMask(Layers.Player, Layers.Guard);
            return surface;
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
            EditorAssets.SetReference(raccoon.GetComponent<RaccoonController>(), "_cameraTransform", mainCamera);

            var isometricCamera = cameraRig.GetComponentInChildren<CinemachineCamera>();
            isometricCamera.Target.TrackingTarget = raccoon.transform;
            PrefabUtility.RecordPrefabInstancePropertyModifications(isometricCamera); // senão se perde ao salvar
        }
    }
}
