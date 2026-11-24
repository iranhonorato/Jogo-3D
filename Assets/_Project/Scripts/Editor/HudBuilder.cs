using Guaxinim.Core;
using Guaxinim.Items;
using Guaxinim.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Monta a HUD da fase (GDD §13): lista no canto superior esquerdo, cronômetro no superior direito, peso e
    /// volume no inferior esquerdo, efeitos no inferior direito, a faixa das dicas do tutorial no topo e a
    /// indicação "[E] Pegar" sobre o objeto ao alcance e a seta da toca. O que é do guaxinim (inventário,
    /// interação e câmera) e a toca são ligados na montagem da fase.
    /// </summary>
    internal static class HudBuilder
    {
        private const int HudSortingOrder = 1;
        private const int PromptSortingOrder = 2;
        private const int HudFontSize = 32;
        private const int TimerFontSize = 48;
        private const int PromptFontSize = 30;
        private const int TipFontSize = 36;
        private const float BarBlockHeight = 80f;
        private const float BarLabelHeight = 40f;
        private const float TipTopMargin = 24f;
        private const int DenLabelFontSize = 26;
        private const string DenArrowSpritePath = "Assets/_Project/Art/UI/Arrow_Den.png";

        private static readonly Vector2 BarSize = new Vector2(380f, 22f);
        private static readonly Vector2 PromptSize = new Vector2(230f, 58f);
        private static readonly Vector2 TipSize = new Vector2(1100f, 84f);
        private static readonly Vector2 DenArrowSize = new Vector2(76f, 76f);
        private static readonly Vector2 DenLabelSize = new Vector2(140f, 40f);
        private static readonly Color HudPanelColor = new Color(0.05f, 0.03f, 0.1f, 0.6f);
        private static readonly Color PillColor = new Color(0.05f, 0.03f, 0.1f, 0.82f);

        public static void Build(Transform parent, LevelFlow flow, ShoppingList shoppingList)
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
            AddTutorialBanner(canvas, flow);
            AddInteractionPrompt(parent, flow);
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

        /// <summary>Faixa no topo da tela com a dica do momento; escondida até a primeira dica.</summary>
        private static void AddTutorialBanner(GameObject canvas, LevelFlow flow)
        {
            GameObject banner = CreatePill("TutorialBanner", canvas.transform, TipSize, TipFontSize, out Text text);
            var rect = (RectTransform)banner.transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -TipTopMargin);

            var tipsUI = canvas.AddComponent<TutorialTipsUI>();
            EditorAssets.SetReference(tipsUI, "_levelFlow", flow);
            EditorAssets.SetReference(tipsUI, "_banner", banner);
            EditorAssets.SetReference(tipsUI, "_text", text);
            banner.SetActive(false);
        }

        /// <summary>Balão "[E] Pegar" que acompanha o objeto ao alcance. A câmera e o guaxinim são ligados na fase.</summary>
        private static void AddInteractionPrompt(Transform parent, LevelFlow flow)
        {
            GameObject canvas = EditorUI.CreateCanvas("PromptCanvas", parent, PromptSortingOrder);
            GameObject prompt = CreatePill("InteractionPrompt", canvas.transform, PromptSize, PromptFontSize, out Text text);

            var promptUI = canvas.AddComponent<InteractionPromptUI>();
            EditorAssets.SetReference(promptUI, "_levelFlow", flow);
            EditorAssets.SetReference(promptUI, "_prompt", (RectTransform)prompt.transform);
            EditorAssets.SetReference(promptUI, "_text", text);
            prompt.SetActive(false);
            AddDenIndicator(canvas, flow);
        }

        /// <summary>Seta âmbar com o rótulo "Toca", que aponta para a toca enquanto o guaxinim carrega algo.</summary>
        private static void AddDenIndicator(GameObject canvas, LevelFlow flow)
        {
            var indicator = new GameObject("DenIndicator", typeof(RectTransform)) { layer = EditorUI.UILayer };
            indicator.transform.SetParent(canvas.transform, false);
            ((RectTransform)indicator.transform).sizeDelta = DenArrowSize;

            var arrow = new GameObject("Arrow", typeof(RectTransform)) { layer = EditorUI.UILayer };
            arrow.transform.SetParent(indicator.transform, false);
            ((RectTransform)arrow.transform).sizeDelta = DenArrowSize;
            var arrowImage = arrow.AddComponent<Image>();
            arrowImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DenArrowSpritePath);
            arrowImage.raycastTarget = false;
            if (arrowImage.sprite == null)
            {
                Debug.LogWarning($"Den arrow sprite not found at {DenArrowSpritePath}.");
            }

            Text label = EditorUI.CreateText("Label", indicator.transform, "Toca", DenLabelFontSize, Color.white);
            var labelRect = (RectTransform)label.transform;
            labelRect.sizeDelta = DenLabelSize;
            labelRect.anchoredPosition = new Vector2(0f, -DenArrowSize.y * 0.75f);
            EditorUI.AddOutline(label.gameObject);

            var denUI = canvas.AddComponent<DenIndicatorUI>();
            EditorAssets.SetReference(denUI, "_levelFlow", flow);
            EditorAssets.SetReference(denUI, "_indicator", (RectTransform)indicator.transform);
            EditorAssets.SetReference(denUI, "_arrow", (RectTransform)arrow.transform);
            indicator.SetActive(false);
        }

        /// <summary>Cápsula escura com um texto claro no meio, centralizada; quem usa a posiciona.</summary>
        private static GameObject CreatePill(string name, Transform parent, Vector2 size, int fontSize, out Text text)
        {
            var pill = new GameObject(name, typeof(RectTransform)) { layer = EditorUI.UILayer };
            pill.transform.SetParent(parent, false);
            var rect = (RectTransform)pill.transform;
            rect.sizeDelta = size;
            var background = pill.AddComponent<Image>();
            background.color = PillColor;
            background.raycastTarget = false;

            text = EditorUI.CreateText("Text", pill.transform, string.Empty, fontSize, EditorUI.LightTextColor);
            EditorUI.Stretch((RectTransform)text.transform);
            return pill;
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
