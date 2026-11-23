using Guaxinim.Core;
using Guaxinim.Items;
using Guaxinim.Noise;
using Guaxinim.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera o prefab LevelSystems, que vai em toda cena de fase: anéis de som, LevelFlow (pego, vitória e
    /// reinício), ShoppingList com o contorno dos itens, o aviso "Você foi pego!", a tela de resultado e o
    /// EventSystem. A lista de compras e o jogador são ligados na montagem da fase.
    /// </summary>
    public static class LevelSystemsFactory
    {
        public const string SystemsPrefabPath = "Assets/_Project/Prefabs/Systems/LevelSystems.prefab";

        private const string FlowDataPath = "Assets/_Project/Data/Levels/LevelFlowData.asset";
        private const string CaughtText = "Você foi pego!";
        private const string WinTitle = "Fase completa!";
        private const string PlayAgainText = "Jogar de novo";
        private const string BuiltinFontName = "LegacyRuntime.ttf";
        private const int CaughtFontSize = 110;
        private const int TitleFontSize = 90;
        private const int DetailsFontSize = 40;
        private const int ButtonFontSize = 44;
        private const int UILayer = 5;
        private const int HudSortingOrder = 1;
        private const int HudFontSize = 32;
        private const int TimerFontSize = 48;
        private const float BarBlockHeight = 80f;
        private const float BarLabelHeight = 40f;
        private const int CaughtSortingOrder = 10;
        private const int ResultSortingOrder = 20;

        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Vector2 ButtonSize = new Vector2(460f, 110f);
        private static readonly Color CaughtTextColor = new Color(1f, 0.88f, 0.7f);
        private static readonly Color OutlineColor = new Color(0.1f, 0.05f, 0.1f);
        private static readonly Vector2 OutlineDistance = new Vector2(4f, -4f);
        private static readonly Vector2 BarSize = new Vector2(380f, 22f);
        private static readonly Color HudPanelColor = new Color(0.05f, 0.03f, 0.1f, 0.6f);
        private static readonly Color PanelColor = new Color(0.05f, 0.03f, 0.1f, 0.85f);
        private static readonly Color ButtonColor = new Color(1f, 0.78f, 0.3f);
        private static readonly Color ButtonTextColor = new Color(0.12f, 0.07f, 0.15f);

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
            AddEventSystem(systems.transform);

            PrefabUtility.SaveAsPrefabAsset(systems, SystemsPrefabPath);
            Object.DestroyImmediate(systems);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(SystemsPrefabPath);
        }

        private static void AddCaughtMessage(Transform parent, LevelFlow flow)
        {
            GameObject canvas = CreateCanvas("CaughtCanvas", parent, CaughtSortingOrder);
            Text message = CreateText("Message", canvas.transform, CaughtText, CaughtFontSize, CaughtTextColor);
            Stretch((RectTransform)message.transform);
            AddOutline(message.gameObject);

            var caughtUI = canvas.AddComponent<CaughtMessageUI>();
            EditorAssets.SetReference(caughtUI, "_levelFlow", flow);
            EditorAssets.SetReference(caughtUI, "_message", message.gameObject);
            message.gameObject.SetActive(false);
        }

        private static void AddResultScreen(Transform parent, LevelFlow flow, ShoppingList shoppingList)
        {
            GameObject canvas = CreateCanvas("ResultCanvas", parent, ResultSortingOrder);
            canvas.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("Panel", typeof(RectTransform)) { layer = UILayer };
            panel.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)panel.transform);
            panel.AddComponent<Image>().color = PanelColor;

            Text title = CreateText("Title", panel.transform, WinTitle, TitleFontSize, CaughtTextColor);
            Place((RectTransform)title.transform, new Vector2(0.5f, 0.82f), new Vector2(1400f, 140f));
            AddOutline(title.gameObject);
            Text details = CreateText("Details", panel.transform, string.Empty, DetailsFontSize, Color.white);
            Place((RectTransform)details.transform, new Vector2(0.5f, 0.5f), new Vector2(1000f, 420f));
            Button playAgain = CreateButton(panel.transform, PlayAgainText);

            var resultUI = canvas.AddComponent<LevelResultUI>();
            EditorAssets.SetReference(resultUI, "_levelFlow", flow);
            EditorAssets.SetReference(resultUI, "_shoppingList", shoppingList);
            EditorAssets.SetReference(resultUI, "_panel", panel);
            EditorAssets.SetReference(resultUI, "_details", details);
            EditorAssets.SetReference(resultUI, "_playAgainButton", playAgain);
            UnityEventTools.AddPersistentListener(playAgain.onClick, resultUI.PlayAgain);
            panel.SetActive(false);
        }

        /// <summary>
        /// HUD (GDD §13): lista no canto superior esquerdo, cronômetro no superior direito, peso e volume no
        /// inferior esquerdo e efeitos no inferior direito. O guaxinim é ligado na montagem da fase.
        /// </summary>
        private static void AddHud(Transform parent, LevelFlow flow, ShoppingList shoppingList)
        {
            GameObject canvas = CreateCanvas("HudCanvas", parent, HudSortingOrder);

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

            var background = new GameObject(name + "Bar", typeof(RectTransform)) { layer = UILayer };
            background.transform.SetParent(canvas, false);
            var backgroundRect = (RectTransform)background.transform;
            PinToCorner(backgroundRect, Vector2.zero, offset, BarSize);
            background.AddComponent<Image>().color = HudPanelColor;

            var fill = new GameObject("Fill", typeof(RectTransform)) { layer = UILayer };
            fill.transform.SetParent(background.transform, false);
            var fillRect = (RectTransform)fill.transform;
            Stretch(fillRect);
            fillRect.anchorMax = new Vector2(0f, 1f);
            var fillImage = fill.AddComponent<Image>();

            loadUI.FindProperty(fillProperty).objectReferenceValue = fillRect;
            loadUI.FindProperty(fillImageProperty).objectReferenceValue = fillImage;
            loadUI.FindProperty(labelProperty).objectReferenceValue = label;
        }

        private static Text CreateHudText(string name, Transform canvas, Vector2 corner, Vector2 offset, Vector2 size,
            TextAnchor alignment)
        {
            Text text = CreateText(name, canvas, string.Empty, HudFontSize, Color.white);
            text.alignment = alignment;
            text.fontStyle = FontStyle.Normal;
            PinToCorner((RectTransform)text.transform, corner, offset, size);
            AddOutline(text.gameObject);
            return text;
        }

        private static void PinToCorner(RectTransform rect, Vector2 corner, Vector2 offset, Vector2 size)
        {
            rect.anchorMin = corner;
            rect.anchorMax = corner;
            rect.pivot = corner;
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;
        }

        private static Button CreateButton(Transform parent, string label)
        {
            var buttonObject = new GameObject("PlayAgainButton", typeof(RectTransform)) { layer = UILayer };
            buttonObject.transform.SetParent(parent, false);
            Place((RectTransform)buttonObject.transform, new Vector2(0.5f, 0.18f), ButtonSize);
            var image = buttonObject.AddComponent<Image>();
            image.color = ButtonColor;
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText("Label", buttonObject.transform, label, ButtonFontSize, ButtonTextColor);
            Stretch((RectTransform)text.transform);
            return button;
        }

        private static void AddEventSystem(Transform parent)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.transform.SetParent(parent, false);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>(); // só o Input System novo (GDD §0)
        }

        private static GameObject CreateCanvas(string name, Transform parent, int sortingOrder)
        {
            var canvasObject = new GameObject(name) { layer = UILayer };
            canvasObject.transform.SetParent(parent, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            return canvasObject;
        }

        private static Text CreateText(string name, Transform parent, string content, int fontSize, Color color)
        {
            var textObject = new GameObject(name, typeof(RectTransform)) { layer = UILayer };
            textObject.transform.SetParent(parent, false);
            var text = textObject.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>(BuiltinFontName);
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static void AddOutline(GameObject target)
        {
            var outline = target.AddComponent<Outline>();
            outline.effectColor = OutlineColor;
            outline.effectDistance = OutlineDistance;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
