using Guaxinim.Core;
using Guaxinim.Items;
using Guaxinim.Noise;
using Guaxinim.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera o prefab LevelSystems, que vai em toda cena de fase: anéis de som, LevelFlow (pego, vitória), ShoppingList
    /// com o contorno dos itens, a HUD, as telas "Você foi pego!", de resultado e de pausa e o EventSystem. A lista
    /// de compras, o jogador e a câmera são ligados na montagem da fase.
    /// </summary>
    public static class LevelSystemsFactory
    {
        public const string SystemsPrefabPath = "Assets/_Project/Prefabs/Systems/LevelSystems.prefab";

        private const string FlowDataPath = "Assets/_Project/Data/Levels/LevelFlowData.asset";
        private const string CaughtTipsFolder = "Assets/_Project/Data/UI";
        private const string CaughtTipsPath = CaughtTipsFolder + "/CaughtTips.asset";
        private const string CaughtTitle = "Você foi pego!";
        private const string TryAgainText = "Tentar de novo";
        private const string WinTitle = "Fase completa!";
        private const string NextLevelText = "Próxima fase";
        private const string PlayAgainText = "Jogar de novo";
        private const string RecordText = "Novo recorde!";
        private const string CityMapText = "Mapa";
        private const string MainMenuText = "Menu inicial";
        private const string PauseTitle = "Pausado";
        private const string ResumeText = "Continuar";
        private const string RestartText = "Reiniciar fase";

        private const string ControlsText =
            "Controles\n\nWASD  mover\nShift  correr\nE  pegar e esconder\nQ  largar o item\nF  assobiar\n" +
            "Tab  lista inteira\nEsc  pausar";

        private const int StarCount = LevelResult.MaxStars;
        private const float StarSize = 90f;
        private const float StarSpacing = 0.08f;
        private const int TitleFontSize = 90;
        private const int CaughtTitleFontSize = 110;
        private const int DetailsFontSize = 40;
        private const int RecordFontSize = 46;
        private const int ControlsFontSize = 34;
        private const int CaughtSortingOrder = 10;
        private const int ResultSortingOrder = 20;
        private const int PauseSortingOrder = 30;

        private static readonly Color AmberColor = new Color32(0xFF, 0xC8, 0x4D, 0xFF);
        private static readonly Color StarOutlineColor = new Color32(0xFF, 0xE0, 0xB3, 0xFF);
        private static readonly Color ControlsBoxColor = new Color(0.08f, 0.05f, 0.14f, 0.7f);

        [MenuItem("Tools/Guaxinim/Greybox/Sistemas da Fase")]
        internal static void CreateLevelSystemsPrefab()
        {
            NoiseRingVFX ringPrefab = NoiseSystemFactory.CreateNoiseAssets();

            var systems = new GameObject("LevelSystems");
            var spawner = systems.AddComponent<NoiseRingSpawner>();
            EditorAssets.SetReference(spawner, "_ringPrefab", ringPrefab);
            var shoppingList = systems.AddComponent<ShoppingList>();
            var highlighter = systems.AddComponent<ShoppingListHighlighter>();
            EditorAssets.SetReference(highlighter, "_shoppingList", shoppingList);
            systems.AddComponent<Guards.FlashlightBudget>(); // o guaxinim é ligado na montagem da fase
            var flow = systems.AddComponent<LevelFlow>();
            EditorAssets.SetReference(flow, "_data", EditorAssets.LoadOrCreate<LevelFlowData>(FlowDataPath));
            EditorAssets.SetReference(flow, "_shoppingList", shoppingList);

            HudBuilder.Build(systems.transform, flow, shoppingList);
            AddCaughtScreen(systems.transform, flow, shoppingList);
            AddResultScreen(systems.transform, flow, shoppingList);
            AddPauseScreen(systems.transform, flow);
            EditorUI.AddEventSystem(systems.transform);

            PrefabUtility.SaveAsPrefabAsset(systems, SystemsPrefabPath);
            Object.DestroyImmediate(systems);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(SystemsPrefabPath);
        }

        /// <summary>Pego (GDD §13): título, resumo, dica sorteada e Tentar de novo, Mapa e Menu inicial.</summary>
        private static void AddCaughtScreen(Transform parent, LevelFlow flow, ShoppingList shoppingList)
        {
            GameObject panel = EditorUI.CreateScreenPanel("CaughtCanvas", parent, CaughtSortingOrder);

            Text title = EditorUI.CreateTitle("Title", panel.transform, CaughtTitle, CaughtTitleFontSize,
                EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)title.transform, new Vector2(0.5f, 0.84f), new Vector2(1500f, 160f));
            EditorUI.AddOutline(title.gameObject);
            Text summary = EditorUI.CreateText("Summary", panel.transform, string.Empty, DetailsFontSize, Color.white);
            EditorUI.Place((RectTransform)summary.transform, new Vector2(0.5f, 0.69f), new Vector2(1400f, 70f));
            Text tip = EditorUI.CreateText("Tip", panel.transform, string.Empty, DetailsFontSize, AmberColor);
            tip.fontStyle = FontStyle.Normal;
            EditorUI.Place((RectTransform)tip.transform, new Vector2(0.5f, 0.58f), new Vector2(1400f, 110f));

            Button tryAgain = EditorUI.CreateButton("TryAgainButton", panel.transform, TryAgainText, new Vector2(0.5f, 0.4f));
            Button cityMap = EditorUI.CreateButton("CityMapButton", panel.transform, CityMapText, new Vector2(0.5f, 0.27f));
            Button mainMenu = EditorUI.CreateButton("MainMenuButton", panel.transform, MainMenuText, new Vector2(0.5f, 0.14f));

            EditorAssets.EnsureFolder(CaughtTipsFolder);
            var caughtUI = panel.transform.parent.gameObject.AddComponent<CaughtScreenUI>();
            EditorAssets.SetReference(caughtUI, "_levelFlow", flow);
            EditorAssets.SetReference(caughtUI, "_shoppingList", shoppingList);
            EditorAssets.SetReference(caughtUI, "_tips", EditorAssets.LoadOrCreate<CaughtTipsData>(CaughtTipsPath));
            EditorAssets.SetReference(caughtUI, "_panel", panel);
            EditorAssets.SetReference(caughtUI, "_summary", summary);
            EditorAssets.SetReference(caughtUI, "_tip", tip);
            EditorAssets.SetReference(caughtUI, "_tryAgainButton", tryAgain);
            UnityEventTools.AddPersistentListener(tryAgain.onClick, caughtUI.TryAgain);
            UnityEventTools.AddPersistentListener(cityMap.onClick, caughtUI.GoToCityMap);
            UnityEventTools.AddPersistentListener(mainMenu.onClick, caughtUI.GoToMainMenu);
            panel.SetActive(false);
        }

        /// <summary>Resultado (GDD §13): estrelas, "Novo recorde!" e detalhes, com Mapa, Jogar de novo e Próxima fase.</summary>
        private static void AddResultScreen(Transform parent, LevelFlow flow, ShoppingList shoppingList)
        {
            GameObject panel = EditorUI.CreateScreenPanel("ResultCanvas", parent, ResultSortingOrder);

            Text title = EditorUI.CreateTitle("Title", panel.transform, WinTitle, TitleFontSize, EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)title.transform, new Vector2(0.5f, 0.89f), new Vector2(1400f, 140f));
            EditorUI.AddOutline(title.gameObject);
            var stars = new Object[StarCount];
            for (int i = 0; i < StarCount; i++)
            {
                stars[i] = CreateStar(panel.transform, new Vector2(0.5f + (i - 1) * StarSpacing, 0.77f));
            }

            Text record = EditorUI.CreateText("Record", panel.transform, RecordText, RecordFontSize, AmberColor);
            EditorUI.Place((RectTransform)record.transform, new Vector2(0.5f, 0.68f), new Vector2(900f, 70f));
            EditorUI.AddOutline(record.gameObject);
            Text details = EditorUI.CreateText("Details", panel.transform, string.Empty, DetailsFontSize, Color.white);
            details.fontStyle = FontStyle.Normal;
            EditorUI.Place((RectTransform)details.transform, new Vector2(0.5f, 0.42f), new Vector2(1100f, 470f));

            Button cityMap = EditorUI.CreateButton("CityMapButton", panel.transform, CityMapText, new Vector2(0.22f, 0.1f));
            Button playAgain = EditorUI.CreateButton("PlayAgainButton", panel.transform, PlayAgainText,
                new Vector2(0.5f, 0.1f));
            Button nextLevel = EditorUI.CreateButton("NextLevelButton", panel.transform, NextLevelText,
                new Vector2(0.78f, 0.1f));

            var resultUI = panel.transform.parent.gameObject.AddComponent<LevelResultUI>();
            EditorAssets.SetReference(resultUI, "_levelFlow", flow);
            EditorAssets.SetReference(resultUI, "_shoppingList", shoppingList);
            EditorAssets.SetReference(resultUI, "_catalog", LevelCatalogFactory.LoadOrCreateCatalog());
            EditorAssets.SetReference(resultUI, "_panel", panel);
            EditorAssets.SetReferenceArray(resultUI, "_stars", stars);
            EditorAssets.SetReference(resultUI, "_details", details);
            EditorAssets.SetReference(resultUI, "_recordLabel", record);
            EditorAssets.SetReference(resultUI, "_nextLevelButton", nextLevel);
            EditorAssets.SetReference(resultUI, "_nextLevelLabel", nextLevel.GetComponentInChildren<Text>());
            EditorAssets.SetReference(resultUI, "_playAgainButton", playAgain);
            UnityEventTools.AddPersistentListener(cityMap.onClick, resultUI.GoToCityMap);
            UnityEventTools.AddPersistentListener(playAgain.onClick, resultUI.PlayAgain);
            UnityEventTools.AddPersistentListener(nextLevel.onClick, resultUI.ContinueCampaign);
            record.gameObject.SetActive(false);
            panel.SetActive(false);
        }

        /// <summary>Pausa (GDD §13): Continuar, Reiniciar fase, Mapa e Menu inicial, com o quadro dos controles.</summary>
        private static void AddPauseScreen(Transform parent, LevelFlow flow)
        {
            var pause = parent.gameObject.AddComponent<PauseController>();
            EditorAssets.SetReference(pause, "_levelFlow", flow);

            GameObject panel = EditorUI.CreateScreenPanel("PauseCanvas", parent, PauseSortingOrder);
            Text title = EditorUI.CreateTitle("Title", panel.transform, PauseTitle, TitleFontSize, EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)title.transform, new Vector2(0.5f, 0.84f), new Vector2(1000f, 140f));
            EditorUI.AddOutline(title.gameObject);
            AddControlsBox(panel.transform);

            Button resume = EditorUI.CreateButton("ResumeButton", panel.transform, ResumeText, new Vector2(0.5f, 0.64f));
            Button restart = EditorUI.CreateButton("RestartButton", panel.transform, RestartText, new Vector2(0.5f, 0.51f));
            Button cityMap = EditorUI.CreateButton("CityMapButton", panel.transform, CityMapText, new Vector2(0.5f, 0.38f));
            Button mainMenu = EditorUI.CreateButton("MainMenuButton", panel.transform, MainMenuText, new Vector2(0.5f, 0.25f));
            UnityEventTools.AddPersistentListener(resume.onClick, pause.Resume);
            UnityEventTools.AddPersistentListener(restart.onClick, pause.RestartLevel);
            UnityEventTools.AddPersistentListener(cityMap.onClick, pause.GoToCityMap);
            UnityEventTools.AddPersistentListener(mainMenu.onClick, pause.GoToMainMenu);

            var pauseUI = panel.transform.parent.gameObject.AddComponent<PauseMenuUI>();
            EditorAssets.SetReference(pauseUI, "_pause", pause);
            EditorAssets.SetReference(pauseUI, "_panel", panel);
            EditorAssets.SetReference(pauseUI, "_resumeButton", resume);
            panel.SetActive(false);
        }

        /// <summary>Quadro com os controles, à esquerda dos botões da pausa.</summary>
        private static void AddControlsBox(Transform parent)
        {
            var box = new GameObject("ControlsBox", typeof(RectTransform)) { layer = EditorUI.UILayer };
            box.transform.SetParent(parent, false);
            EditorUI.Place((RectTransform)box.transform, new Vector2(0.16f, 0.5f), new Vector2(520f, 560f));
            var background = box.AddComponent<Image>();
            background.color = ControlsBoxColor;
            background.raycastTarget = false;

            Text text = EditorUI.CreateText("Text", box.transform, ControlsText, ControlsFontSize, Color.white);
            text.fontStyle = FontStyle.Normal;
            text.alignment = TextAnchor.UpperLeft;
            var rect = (RectTransform)text.transform;
            EditorUI.Stretch(rect);
            rect.offsetMin = new Vector2(36f, 24f);
            rect.offsetMax = new Vector2(-24f, -24f);
        }

        /// <summary>
        /// Estrela greybox: um losango (quadrado girado) com contorno. Ganha (cheia, âmbar) ou perdida (apagada, só o
        /// contorno): quem pinta é a tela de resultado.
        /// </summary>
        private static Image CreateStar(Transform parent, Vector2 anchor)
        {
            var star = new GameObject("Star", typeof(RectTransform)) { layer = EditorUI.UILayer };
            star.transform.SetParent(parent, false);
            var rect = (RectTransform)star.transform;
            EditorUI.Place(rect, anchor, Vector2.one * StarSize);
            rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var image = star.AddComponent<Image>();
            image.raycastTarget = false;
            var outline = star.AddComponent<Outline>();
            outline.effectColor = StarOutlineColor;
            outline.effectDistance = new Vector2(3f, -3f);
            return image;
        }
    }
}
