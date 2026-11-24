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
    /// Gera o prefab LevelSystems, que vai em toda cena de fase: anéis de som, LevelFlow (pego, vitória e
    /// reinício), ShoppingList com o contorno dos itens, o aviso "Você foi pego!", as telas de resultado e de
    /// pausa, a HUD e o EventSystem. A lista de compras e o jogador são ligados na montagem da fase.
    /// </summary>
    public static class LevelSystemsFactory
    {
        public const string SystemsPrefabPath = "Assets/_Project/Prefabs/Systems/LevelSystems.prefab";

        private const string FlowDataPath = "Assets/_Project/Data/Levels/LevelFlowData.asset";
        private const string CaughtText = "Você foi pego!";
        private const string WinTitle = "Fase completa!";
        private const string NextLevelText = "Próxima fase";
        private const string PlayAgainText = "Jogar de novo";
        private const string CityMapText = "Mapa";
        private const string PauseTitle = "Pausado";
        private const string ResumeText = "Continuar";
        private const string RestartText = "Reiniciar fase";
        private const int StarCount = 3;
        private const float StarSize = 90f;
        private const float StarSpacing = 0.08f;
        private const int CaughtFontSize = 110;
        private const int TitleFontSize = 90;
        private const int DetailsFontSize = 40;
        private const int HudSortingOrder = 1;
        private const int HudFontSize = 32;
        private const int TimerFontSize = 48;
        private const float BarBlockHeight = 80f;
        private const float BarLabelHeight = 40f;
        private const int CaughtSortingOrder = 10;
        private const int ResultSortingOrder = 20;
        private const int PauseSortingOrder = 30;

        private static readonly Vector2 BarSize = new Vector2(380f, 22f);
        private static readonly Color HudPanelColor = new Color(0.05f, 0.03f, 0.1f, 0.6f);

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
            var flow = systems.AddComponent<LevelFlow>();
            EditorAssets.SetReference(flow, "_data", EditorAssets.LoadOrCreate<LevelFlowData>(FlowDataPath));
            EditorAssets.SetReference(flow, "_shoppingList", shoppingList);

            AddHud(systems.transform, flow, shoppingList);
            AddCaughtMessage(systems.transform, flow);
            AddResultScreen(systems.transform, flow, shoppingList);
            AddPauseScreen(systems.transform, flow);
            EditorUI.AddEventSystem(systems.transform);

            PrefabUtility.SaveAsPrefabAsset(systems, SystemsPrefabPath);
            Object.DestroyImmediate(systems);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(SystemsPrefabPath);
        }

        private static void AddCaughtMessage(Transform parent, LevelFlow flow)
        {
            GameObject canvas = EditorUI.CreateCanvas("CaughtCanvas", parent, CaughtSortingOrder);
            Text message = EditorUI.CreateText("Message", canvas.transform, CaughtText, CaughtFontSize,
                EditorUI.LightTextColor);
            EditorUI.Stretch((RectTransform)message.transform);
            EditorUI.AddOutline(message.gameObject);

            var caughtUI = canvas.AddComponent<CaughtMessageUI>();
            EditorAssets.SetReference(caughtUI, "_levelFlow", flow);
            EditorAssets.SetReference(caughtUI, "_message", message.gameObject);
            message.gameObject.SetActive(false);
        }

        /// <summary>Resultado (GDD §13): estrelas e detalhes, com Mapa, Jogar de novo e Próxima fase na base.</summary>
        private static void AddResultScreen(Transform parent, LevelFlow flow, ShoppingList shoppingList)
        {
            GameObject panel = EditorUI.CreateScreenPanel("ResultCanvas", parent, ResultSortingOrder);

            Text title = EditorUI.CreateText("Title", panel.transform, WinTitle, TitleFontSize, EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)title.transform, new Vector2(0.5f, 0.88f), new Vector2(1400f, 140f));
            EditorUI.AddOutline(title.gameObject);
            var stars = new Object[StarCount];
            for (int i = 0; i < StarCount; i++)
            {
                stars[i] = CreateStar(panel.transform, new Vector2(0.5f + (i - 1) * StarSpacing, 0.74f));
            }

            Text details = EditorUI.CreateText("Details", panel.transform, string.Empty, DetailsFontSize, Color.white);
            details.fontStyle = FontStyle.Normal;
            EditorUI.Place((RectTransform)details.transform, new Vector2(0.5f, 0.43f), new Vector2(1100f, 480f));
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
            EditorAssets.SetReference(resultUI, "_nextLevelButton", nextLevel);
            EditorAssets.SetReference(resultUI, "_playAgainButton", playAgain);
            UnityEventTools.AddPersistentListener(cityMap.onClick, resultUI.GoToCityMap);
            UnityEventTools.AddPersistentListener(playAgain.onClick, resultUI.PlayAgain);
            UnityEventTools.AddPersistentListener(nextLevel.onClick, resultUI.PlayNextLevel);
            panel.SetActive(false);
        }

        /// <summary>Pausa (GDD §13): Continuar, Reiniciar fase e Mapa.</summary>
        private static void AddPauseScreen(Transform parent, LevelFlow flow)
        {
            var pause = parent.gameObject.AddComponent<PauseController>();
            EditorAssets.SetReference(pause, "_levelFlow", flow);

            GameObject panel = EditorUI.CreateScreenPanel("PauseCanvas", parent, PauseSortingOrder);
            Text title = EditorUI.CreateText("Title", panel.transform, PauseTitle, TitleFontSize, EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)title.transform, new Vector2(0.5f, 0.72f), new Vector2(1000f, 140f));
            EditorUI.AddOutline(title.gameObject);
            Button resume = EditorUI.CreateButton("ResumeButton", panel.transform, ResumeText, new Vector2(0.5f, 0.5f));
            Button restart = EditorUI.CreateButton("RestartButton", panel.transform, RestartText, new Vector2(0.5f, 0.36f));
            Button cityMap = EditorUI.CreateButton("CityMapButton", panel.transform, CityMapText, new Vector2(0.5f, 0.22f));
            UnityEventTools.AddPersistentListener(resume.onClick, pause.Resume);
            UnityEventTools.AddPersistentListener(restart.onClick, pause.RestartLevel);
            UnityEventTools.AddPersistentListener(cityMap.onClick, pause.GoToCityMap);

            var pauseUI = panel.transform.parent.gameObject.AddComponent<PauseMenuUI>();
            EditorAssets.SetReference(pauseUI, "_pause", pause);
            EditorAssets.SetReference(pauseUI, "_panel", panel);
            EditorAssets.SetReference(pauseUI, "_resumeButton", resume);
            panel.SetActive(false);
        }

        /// <summary>Estrela greybox: um losango (quadrado girado), colorido pela tela de resultado.</summary>
        private static Image CreateStar(Transform parent, Vector2 anchor)
        {
            var star = new GameObject("Star", typeof(RectTransform)) { layer = EditorUI.UILayer };
            star.transform.SetParent(parent, false);
            var rect = (RectTransform)star.transform;
            EditorUI.Place(rect, anchor, Vector2.one * StarSize);
            rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var image = star.AddComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        /// <summary>
        /// HUD (GDD §13): lista no canto superior esquerdo, cronômetro no superior direito, peso e volume no
        /// inferior esquerdo e efeitos no inferior direito. O guaxinim é ligado na montagem da fase.
        /// </summary>
        private static void AddHud(Transform parent, LevelFlow flow, ShoppingList shoppingList)
        {
            GameObject canvas = EditorUI.CreateCanvas("HudCanvas", parent, HudSortingOrder);

            Text list = CreateHudText("ShoppingList", canvas.transform, new Vector2(0f, 1f), new Vector2(30f, -30f),
                new Vector2(560f, 320f), TextAnchor.UpperLeft);
            var listUI = canvas.AddComponent<ShoppingListUI>();
            EditorAssets.SetReference(listUI, "_shoppingList", shoppingList);
            EditorAssets.SetReference(listUI, "_text", list);

            Text timer = CreateHudText("Timer", canvas.transform, new Vector2(1f, 1f), new Vector2(-30f, -30f),
                new Vector2(240f, 80f), TextAnchor.MiddleRight);
            timer.fontSize = TimerFontSize;
            var timerUI = canvas.AddComponent<LevelTimerUI>();
            EditorAssets.SetReference(timerUI, "_levelFlow", flow);
            EditorAssets.SetReference(timerUI, "_text", timer);

            Text effects = CreateHudText("Effects", canvas.transform, new Vector2(1f, 0f), new Vector2(-30f, 30f),
                new Vector2(380f, 140f), TextAnchor.LowerRight);
            EditorAssets.SetReference(canvas.AddComponent<EffectsUI>(), "_text", effects);

            AddLoadBars(canvas);
        }

        private static void AddLoadBars(GameObject canvas)
        {
            var loadUI = canvas.AddComponent<CarryLoadUI>();
            var serialized = new SerializedObject(loadUI);
            CreateBar("Volume", canvas.transform, new Vector2(30f, 30f), serialized, "_volumeFill", "_volumeFillImage",
                "_volumeLabel");
            CreateBar("Weight", canvas.transform, new Vector2(30f, 30f + BarBlockHeight), serialized, "_weightFill",
                "_weightFillImage", "_weightLabel");
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Rótulo e barra (fundo escuro com preenchimento pela âncora), presos no canto inferior esquerdo.</summary>
        private static void CreateBar(string name, Transform canvas, Vector2 offset, SerializedObject loadUI,
            string fillProperty, string fillImageProperty, string labelProperty)
        {
            Text label = CreateHudText(name + "Label", canvas, Vector2.zero, offset + Vector2.up * BarSize.y,
                new Vector2(BarSize.x, BarLabelHeight), TextAnchor.LowerLeft);

            var background = new GameObject(name + "Bar", typeof(RectTransform)) { layer = EditorUI.UILayer };
            background.transform.SetParent(canvas, false);
            var backgroundRect = (RectTransform)background.transform;
            EditorUI.PinToCorner(backgroundRect, Vector2.zero, offset, BarSize);
            background.AddComponent<Image>().color = HudPanelColor;

            var fill = new GameObject("Fill", typeof(RectTransform)) { layer = EditorUI.UILayer };
            fill.transform.SetParent(background.transform, false);
            var fillRect = (RectTransform)fill.transform;
            EditorUI.Stretch(fillRect);
            fillRect.anchorMax = new Vector2(0f, 1f);
            var fillImage = fill.AddComponent<Image>();

            loadUI.FindProperty(fillProperty).objectReferenceValue = fillRect;
            loadUI.FindProperty(fillImageProperty).objectReferenceValue = fillImage;
            loadUI.FindProperty(labelProperty).objectReferenceValue = label;
        }

        private static Text CreateHudText(string name, Transform canvas, Vector2 corner, Vector2 offset, Vector2 size,
            TextAnchor alignment)
        {
            Text text = EditorUI.CreateText(name, canvas, string.Empty, HudFontSize, Color.white);
            text.alignment = alignment;
            text.fontStyle = FontStyle.Normal;
            EditorUI.PinToCorner((RectTransform)text.transform, corner, offset, size);
            EditorUI.AddOutline(text.gameObject);
            return text;
        }
    }
}
