using System.Linq;
using Guaxinim.Core;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Cria a cena Level_01_Mercadinho (passo 1.10, GDD §11): mercadinho de 12 × 10 m com 2 corredores,
    /// o Seu Zé patrulhando e a toca no fundo, onde o guaxinim começa. Luz noturna com no máximo 4 luzes
    /// em tempo real (GDD §14.6), NavMesh assado e a cena como primeira do Build Profile.
    /// Rodar de novo recria a cena do zero.
    /// </summary>
    public static class Level01Builder
    {
        private const string LevelsFolder = "Assets/_Project/Scenes/Levels";
        private const string ScenePath = LevelsFolder + "/Level_01_Mercadinho.unity";
        private const string NavMeshDataPath = LevelsFolder + "/Level_01_Mercadinho_NavMesh.asset";
        private const string LevelDataPath = "Assets/_Project/Data/Levels/Level_01_Mercadinho.asset";
        private const string LevelName = "Mercadinho do Seu Zé";
        private const int FloorColumns = 6;
        private const int FloorRows = 5;
        private const float StoreLightHeight = 2.4f;
        private const float StoreLightRange = 7f;
        private const float StoreLightIntensity = 1.2f;
        private const float MoonlightIntensity = 0.35f;
        private const float ShelfBaseTop = 0.12f;

        private static readonly string[] ShoppingListItems = { "Bread", "Banana", "Cheese" }; // GDD §11

        private static readonly Vector3 DenPosition = new Vector3(-5f, 0f, 4f);
        private static readonly Vector3 RaccoonStart = new Vector3(-4.2f, 0f, 2.6f);

        // Começa no canto da frente, longe da toca, e dá a volta pelos dois corredores.
        private static readonly Vector3[] SeuZeWaypoints =
        {
            new Vector3(5f, 0f, -3.5f),
            new Vector3(-5f, 0f, -3.5f),
            new Vector3(-5f, 0f, 0f),
            new Vector3(5f, 0f, 0f),
        };

        // Na base das gôndolas (0,12 m), na borda virada para o corredor, ao alcance do guaxinim.
        private static readonly ItemSpot[] ItemSpots =
        {
            new ItemSpot("Bread", new Vector3(-2.5f, ShelfBaseTop, 1.12f), 0f),
            new ItemSpot("Banana", new Vector3(2.5f, ShelfBaseTop, -1.12f), 20f),
            new ItemSpot("Cheese", new Vector3(-1f, ShelfBaseTop, -1.88f), -15f),
            new ItemSpot("Cookies", new Vector3(3f, ShelfBaseTop, 1.88f), 10f),
        };

        private static readonly Color MoonlightColor = new Color(0.55f, 0.6f, 0.95f); // GDD §15.1: sombras frias
        private static readonly Color StoreLightColor = new Color(1f, 0.82f, 0.55f); // pontos de luz quente
        private static readonly Color AmbientColor = new Color(0.22f, 0.2f, 0.32f);
        private static readonly Color BackgroundColor = new Color(0.06f, 0.05f, 0.09f);

        [MenuItem("Tools/Guaxinim/Montar Fase 1")]
        private static void BuildLevel()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            LevelBuildKit.GenerateAllPrefabs();
            EditorAssets.EnsureFolder(LevelsFolder);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("Level_01");
            LevelBuildKit.BuildRoom(root.transform, FloorColumns, FloorRows);
            BuildShelves(root.transform);
            LevelData level = LevelBuildKit.LoadOrCreateLevelData(LevelDataPath, LevelName, ShoppingListItems);
            GameObject systems = LevelBuildKit.PlaceLevelSystems(root.transform, level);
            LevelBuildKit.PlaceDen(root.transform, systems, DenPosition, 0f);
            GameObject raccoon = LevelBuildKit.PlacePlayer(root.transform, systems, RaccoonStart, 180f);
            LevelBuildKit.PlaceGuard(root.transform, GuardFactory.SeuZePrefabPath, SeuZeWaypoints, 0, -90f);
            PlaceItems(root.transform);
            SetUpNightLighting(root.transform, raccoon.scene);
            BakeNavMesh(root);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddAsFirstBuildScene();
            Selection.activeGameObject = raccoon;
        }

        private static void BuildShelves(Transform root)
        {
            foreach (float z in new[] { 1.5f, -1.5f })
            {
                LevelBuildKit.Place(EnvironmentKitFactory.Shelf4mPath, root, new Vector3(-1.5f, 0f, z), 0f);
                LevelBuildKit.Place(EnvironmentKitFactory.Shelf2mPath, root, new Vector3(2.7f, 0f, z), 0f);
                // Vão de 1,2 m entre as gôndolas: atalho só do guaxinim, e só sem carga volumosa (GDD §7.4).
                LevelBuildKit.Place(EnvironmentKitFactory.NarrowGapPath, root, new Vector3(1.1f, 0f, z), 0f);
            }
        }

        /// <summary>GDD §11: lista da Fase 1 (pão, banana e queijo) e o biscoito de bônus, na prateleira de baixo.</summary>
        private static void PlaceItems(Transform root)
        {
            var items = new GameObject("Items").transform;
            items.SetParent(root, false);
            foreach (ItemSpot spot in ItemSpots)
            {
                LevelBuildKit.Place(ItemFactory.PrefabPath(spot.ItemName), items, spot.Position, spot.YawDegrees);
            }
        }

        private static void SetUpNightLighting(Transform root, Scene scene)
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = AmbientColor;

            Light moon = CreateLight("Moonlight", root, LightType.Directional, MoonlightColor, MoonlightIntensity);
            moon.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
            moon.shadows = LightShadows.Soft;

            foreach (float z in new[] { 3f, -3f })
            {
                Light storeLight = CreateLight("StoreLight", root, LightType.Point, StoreLightColor, StoreLightIntensity);
                storeLight.transform.position = new Vector3(0f, StoreLightHeight, z);
                storeLight.range = StoreLightRange;
                storeLight.shadows = LightShadows.None;
            }

            SetCameraBackground(scene);
        }

        private static Light CreateLight(string name, Transform parent, LightType type, Color color, float intensity)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            var light = lightObject.AddComponent<Light>();
            light.type = type;
            light.color = color;
            light.intensity = intensity;
            return light;
        }

        private static void SetCameraBackground(Scene scene)
        {
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                foreach (Camera sceneCamera in rootObject.GetComponentsInChildren<Camera>(true))
                {
                    sceneCamera.clearFlags = CameraClearFlags.SolidColor;
                    sceneCamera.backgroundColor = BackgroundColor;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(sceneCamera);
                }
            }
        }

        private static void BakeNavMesh(GameObject root)
        {
            NavMeshSurface surface = LevelBuildKit.AddNavMeshSurface(root);
            surface.BuildNavMesh();
            AssetDatabase.DeleteAsset(NavMeshDataPath); // recriar a fase troca o NavMesh antigo
            AssetDatabase.CreateAsset(surface.navMeshData, NavMeshDataPath);
        }

        private static void AddAsFirstBuildScene()
        {
            var scenes = EditorBuildSettings.scenes.Where(buildScene => buildScene.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private readonly struct ItemSpot
        {
            public ItemSpot(string itemName, Vector3 position, float yawDegrees)
            {
                ItemName = itemName;
                Position = position;
                YawDegrees = yawDegrees;
            }

            public string ItemName { get; }
            public Vector3 Position { get; }
            public float YawDegrees { get; }
        }
    }
}
