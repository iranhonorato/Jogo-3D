using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Cria as cenas MainMenu e CityMap (passo 3.8, GDD §13) e deixa o Build Profile na ordem do jogo.
    /// Rodar de novo recria as duas cenas do zero, com o catálogo de fases atualizado.
    /// </summary>
    public static class MenuScenesBuilder
    {
        private static readonly Color BackgroundColor = new Color(0.06f, 0.05f, 0.09f);

        [MenuItem("Tools/Guaxinim/Montar Menu e Mapa")]
        private static void BuildMenuScenes()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            LevelCatalogFactory.CreateOrUpdate();

            // O catálogo é carregado dentro da cena nova: NewScene descarrega os assets sem uso, e uma referência
            // carregada antes viraria null ao ser ligada ao CityMapUI.
            BuildScene(BuildSceneList.CityMapPath, "CityMap",
                root => CityMapSceneBuilder.Build(root, LevelCatalogFactory.LoadOrCreateCatalog()));
            BuildScene(BuildSceneList.MainMenuPath, "MainMenu", MainMenuSceneBuilder.Build);
            BuildSceneList.Refresh();
        }

        /// <summary>Cena nova com câmera, EventSystem e a raiz; fica aberta no Editor depois de salva.</summary>
        private static void BuildScene(string scenePath, string rootName, System.Action<Transform> buildContent)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            var root = new GameObject(rootName);
            EditorUI.AddEventSystem(root.transform);
            buildContent(root.transform);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        /// <summary>Só a cor de fundo: a tela é toda uGUI, mas sem câmera o Unity avisa e mostra a tela preta de aviso.</summary>
        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.cullingMask = 0;
            cameraObject.AddComponent<AudioListener>();
        }
    }
}
