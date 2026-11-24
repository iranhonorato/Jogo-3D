using Guaxinim.CameraSystem;
using Guaxinim.Core;
using Guaxinim.Guards;
using Guaxinim.Items;
using Guaxinim.Player;
using Guaxinim.Shared;
using Guaxinim.UI;
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
            ItemFactory.CreateItems();
            HidingSpotFactory.CreateBinPrefab();
            ScreenFaderFactory.CreatePrefab();
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

        /// <summary>Cria (ou carrega) o LevelData de uma fase com a lista de compras pelos nomes dos itens.</summary>
        public static LevelData LoadOrCreateLevelData(string path, string displayName, string[] shoppingListItems,
            float targetTimeSeconds)
        {
            var level = EditorAssets.LoadOrCreate<LevelData>(path);
            var serialized = new SerializedObject(level);
            serialized.FindProperty("_displayName").stringValue = displayName;
            serialized.FindProperty("_targetTimeSeconds").floatValue = targetTimeSeconds;
            SerializedProperty list = serialized.FindProperty("_shoppingList");
            list.arraySize = shoppingListItems.Length;
            for (int i = 0; i < shoppingListItems.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = ItemFactory.LoadData(shoppingListItems[i]);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return level;
        }

        /// <summary>Põe o LevelSystems com a lista de compras da fase.</summary>
        public static GameObject PlaceLevelSystems(Transform root, LevelData level)
        {
            GameObject systems = Place(LevelSystemsFactory.SystemsPrefabPath, root, Vector3.zero, 0f);
            var listItems = new Object[level.ShoppingList.Count];
            for (int i = 0; i < listItems.Length; i++)
            {
                listItems[i] = level.ShoppingList[i];
            }

            EditorAssets.SetReferenceArray(systems.GetComponent<ShoppingList>(), "_required", listItems);
            EditorAssets.SetReference(systems.GetComponent<LevelFlow>(), "_level", level);
            return systems;
        }

        /// <summary>Põe a toca, ligada à lista de compras da fase.</summary>
        public static GameObject PlaceDen(Transform root, GameObject systems, Vector3 position, float yawDegrees)
        {
            GameObject den = Place(EnvironmentKitFactory.DenPath, root, position, yawDegrees);
            EditorAssets.SetReference(den.GetComponent<Den>(), "_shoppingList", systems.GetComponent<ShoppingList>());
            return den;
        }

        /// <summary>Põe o guaxinim e o rig de câmera, e liga os dois aos sistemas da fase.</summary>
        public static GameObject PlacePlayer(Transform root, GameObject systems, Vector3 start, float yawDegrees)
        {
            GameObject raccoon = Place(GreyboxRaccoonFactory.PrefabPath, root, start, yawDegrees);
            EditorAssets.SetReference(systems.GetComponent<LevelFlow>(), "_player", raccoon.GetComponent<PlayerState>());
            EditorAssets.SetReference(systems.GetComponent<ShoppingListHighlighter>(), "_player", raccoon.transform);
            EditorAssets.SetReference(systems.GetComponent<FlashlightBudget>(), "_player", raccoon.transform);
            ConnectHud(systems, raccoon);

            GameObject cameraRig = Place(CameraRigFactory.PerspectivePath, root, Vector3.zero, 0f);
            DisableOtherMainCameras(cameraRig);
            ConnectCamera(cameraRig, raccoon);
            EditorAssets.SetReference(systems.GetComponentInChildren<InteractionPromptUI>(), "_camera",
                cameraRig.GetComponentInChildren<Camera>());
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

        /// <summary>A HUD vive no LevelSystems; o que é do guaxinim é ligado aqui, na cena.</summary>
        private static void ConnectHud(GameObject systems, GameObject raccoon)
        {
            var input = raccoon.GetComponent<InputReader>();
            EditorAssets.SetReference(systems.GetComponentInChildren<ShoppingListUI>(), "_input", input);
            EditorAssets.SetReference(systems.GetComponent<PauseController>(), "_input", input);
            var loadUI = systems.GetComponentInChildren<CarryLoadUI>();
            EditorAssets.SetReference(loadUI, "_inventory", raccoon.GetComponent<Inventory>());
            EditorAssets.SetReference(loadUI, "_effects", raccoon.GetComponent<RaccoonEffects>());
            EditorAssets.SetReference(loadUI, "_bulkyLoad", raccoon.GetComponent<RaccoonBulkyLoad>());
            EditorAssets.SetReference(systems.GetComponentInChildren<EffectsUI>(), "_effects",
                raccoon.GetComponent<RaccoonEffects>());

            var interactor = raccoon.GetComponent<RaccoonInteractor>();
            EditorAssets.SetReference(systems.GetComponentInChildren<InteractionPromptUI>(), "_interactor", interactor);
            var tipsUI = systems.GetComponentInChildren<TutorialTipsUI>();
            EditorAssets.SetReference(tipsUI, "_interactor", interactor);
            EditorAssets.SetReference(tipsUI, "_inventory", raccoon.GetComponent<Inventory>());
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

            EditorAssets.SetReference(cameraRig.GetComponentInChildren<ObstacleFader>(), "_target", raccoon.transform);

            var isometricCamera = cameraRig.GetComponentInChildren<CinemachineCamera>();
            isometricCamera.Target.TrackingTarget = raccoon.transform;
            PrefabUtility.RecordPrefabInstancePropertyModifications(isometricCamera); // senão se perde ao salvar
        }
    }
}
