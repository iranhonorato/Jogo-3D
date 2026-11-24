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
    /// Montador único das fases (plano de próximos passos, Etapa 2): transforma um <see cref="LevelBlueprint"/> numa
    /// cena em Scenes/Levels, com o LevelData, a luz noturna (GDD §14.6), o NavMesh assado, o catálogo de fases e o
    /// Build Profile atualizados. Rodar de novo recria a cena do zero.
    /// </summary>
    public static class LevelSceneBuilder
    {
        private const string LevelsFolder = "Assets/_Project/Scenes/Levels";
        private const string LevelDataFolder = "Assets/_Project/Data/Levels";
        private const string MenuRoot = "Tools/Guaxinim/Montar Fase/";
        private const float StoreLightRange = 7f;
        private const float StoreLightIntensity = 1.2f;
        private const float MoonlightIntensity = 0.35f;

        private static readonly Quaternion MoonRotation = Quaternion.Euler(55f, -35f, 0f);
        private static readonly Color MoonlightColor = new Color(0.55f, 0.6f, 0.95f); // GDD §15.1: sombras frias
        private static readonly Color StoreLightColor = new Color(1f, 0.82f, 0.55f); // pontos de luz quente
        private static readonly Color AmbientColor = new Color(0.22f, 0.2f, 0.32f);
        private static readonly Color BackgroundColor = new Color(0.06f, 0.05f, 0.09f);

        [MenuItem(MenuRoot + "1 Mercadinho do Seu Zé")]
        private static void BuildLevel1() => BuildFromMenu(0);

        [MenuItem(MenuRoot + "2 XOXO 24h")]
        private static void BuildLevel2() => BuildFromMenu(1);

        [MenuItem(MenuRoot + "2 XOXO 24h", true)]
        private static bool CanBuildLevel2() => LevelBlueprints.All.Count > 1;

        [MenuItem(MenuRoot + "3 Pão de Mel")]
        private static void BuildLevel3() => BuildFromMenu(2);

        [MenuItem(MenuRoot + "3 Pão de Mel", true)]
        private static bool CanBuildLevel3() => LevelBlueprints.All.Count > 2;

        [MenuItem(MenuRoot + "4 Carrecinco")]
        private static void BuildLevel4() => BuildFromMenu(3);

        [MenuItem(MenuRoot + "4 Carrecinco", true)]
        private static bool CanBuildLevel4() => LevelBlueprints.All.Count > 3;

        [MenuItem(MenuRoot + "5 Atacadaço")]
        private static void BuildLevel5() => BuildFromMenu(4);

        [MenuItem(MenuRoot + "5 Atacadaço", true)]
        private static bool CanBuildLevel5() => LevelBlueprints.All.Count > 4;

        /// <summary>Gera os prefabs uma vez e monta todas as fases descritas, com o catálogo e o Build Profile.</summary>
        [MenuItem("Tools/Guaxinim/Montar Todas as Fases")]
        internal static void BuildAllLevels()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            LevelBuildKit.GenerateAllPrefabs();
            GameObject lastRaccoon = null;
            foreach (LevelBlueprint blueprint in LevelBlueprints.All)
            {
                lastRaccoon = BuildScene(blueprint);
            }

            FinishBuild(lastRaccoon);
        }

        private static void BuildFromMenu(int index)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            LevelBuildKit.GenerateAllPrefabs();
            FinishBuild(BuildScene(LevelBlueprints.All[index]));
        }

        private static void FinishBuild(GameObject raccoon)
        {
            LevelCatalogFactory.CreateOrUpdate(); // marca as cenas que existem nos LevelData e atualiza o catálogo
            BuildSceneList.Refresh();
            Selection.activeGameObject = raccoon;
        }

        /// <summary>Monta e salva a cena de uma fase. Devolve o guaxinim (fica selecionado no fim).</summary>
        private static GameObject BuildScene(LevelBlueprint blueprint)
        {
            EditorAssets.EnsureFolder(LevelsFolder);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject(blueprint.AssetName);

            LevelBuildKit.BuildRoom(root.transform, blueprint.Columns, blueprint.Rows);
            PlaceShelvesAndGaps(root.transform, blueprint);
            LevelData level = LevelBuildKit.LoadOrCreateLevelData($"{LevelDataFolder}/{blueprint.AssetName}.asset",
                blueprint.DisplayName, blueprint.ShoppingList, blueprint.TargetTimeSeconds);
            if (blueprint.HasTutorialTips)
            {
                TutorialTipsFactory.AssignToLevel01(level);
            }

            GameObject systems = LevelBuildKit.PlaceLevelSystems(root.transform, level);
            LevelBuildKit.PlaceDen(root.transform, systems, blueprint.Den.Position, blueprint.Den.YawDegrees);
            GameObject raccoon = LevelBuildKit.PlacePlayer(root.transform, systems, blueprint.RaccoonStart.Position,
                blueprint.RaccoonStart.YawDegrees);
            PlaceEnemies(root.transform, blueprint);
            PlaceItems(root.transform, blueprint);
            PlaceAll(root.transform, HidingSpotFactory.BinPrefabPath, blueprint.Bins);
            SetUpNightLighting(root.transform, scene, blueprint);
            BakeNavMesh(root, $"{LevelsFolder}/{blueprint.AssetName}_NavMesh.asset");

            EditorSceneManager.SaveScene(scene, $"{LevelsFolder}/{blueprint.AssetName}.unity");
            return raccoon;
        }

        private static void PlaceShelvesAndGaps(Transform root, LevelBlueprint blueprint)
        {
            foreach (ShelfPlacement shelf in blueprint.Shelves)
            {
                string prefab = shelf.IsLong ? EnvironmentKitFactory.Shelf4mPath : EnvironmentKitFactory.Shelf2mPath;
                LevelBuildKit.Place(prefab, root, shelf.Placement.Position, shelf.Placement.YawDegrees);
            }

            PlaceAll(root, EnvironmentKitFactory.NarrowGapPath, blueprint.NarrowGaps);
        }

        private static void PlaceEnemies(Transform root, LevelBlueprint blueprint)
        {
            foreach (EnemyPlacement enemy in blueprint.Enemies)
            {
                string prefab = enemy.Type == EnemyType.Guard ? GuardFactory.GuardPrefabPath : GuardFactory.JanitorPrefabPath;
                LevelBuildKit.PlaceGuard(root, prefab, enemy.Waypoints, enemy.StartWaypoint, enemy.YawDegrees);
            }
        }

        private static void PlaceItems(Transform root, LevelBlueprint blueprint)
        {
            var items = new GameObject("Items").transform;
            items.SetParent(root, false);
            foreach (ItemPlacement item in blueprint.Items)
            {
                LevelBuildKit.Place(ItemFactory.PrefabPath(item.ItemName), items, item.Placement.Position,
                    item.Placement.YawDegrees);
            }
        }

        private static void PlaceAll(Transform root, string prefabPath, Placement[] placements)
        {
            foreach (Placement placement in placements)
            {
                LevelBuildKit.Place(prefabPath, root, placement.Position, placement.YawDegrees);
            }
        }

        private static void SetUpNightLighting(Transform root, Scene scene, LevelBlueprint blueprint)
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = AmbientColor;

            Light moon = CreateLight("Moonlight", root, LightType.Directional, MoonlightColor, MoonlightIntensity);
            moon.transform.rotation = MoonRotation;
            moon.shadows = LightShadows.Soft;

            foreach (Vector3 position in blueprint.StoreLights)
            {
                Light storeLight = CreateLight("StoreLight", root, LightType.Point, StoreLightColor, StoreLightIntensity);
                storeLight.transform.position = position;
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

        private static void BakeNavMesh(GameObject root, string navMeshDataPath)
        {
            NavMeshSurface surface = LevelBuildKit.AddNavMeshSurface(root);
            surface.BuildNavMesh();
            AssetDatabase.DeleteAsset(navMeshDataPath); // recriar a fase troca o NavMesh antigo
            AssetDatabase.CreateAsset(surface.navMeshData, navMeshDataPath);
        }
    }
}
