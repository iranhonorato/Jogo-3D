using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Peças de uGUI em greybox para os montadores de tela (HUD, pausa, resultado, menu e mapa): canvas,
    /// texto, botão e painel, no mesmo estilo. Tudo é criado por código para a cena ser reproduzível.
    /// </summary>
    internal static class EditorUI
    {
        public const int UILayer = 5;
        public const int ButtonFontSize = 44;
        public const int BackgroundSortingOrder = -1;

        private const string BuiltinFontName = "LegacyRuntime.ttf";

        public static readonly Vector2 ButtonSize = new Vector2(460f, 110f);
        public static readonly Color LightTextColor = new Color(1f, 0.88f, 0.7f);
        public static readonly Color PanelColor = new Color(0.05f, 0.03f, 0.1f, 0.85f);

        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Color OutlineColor = new Color(0.1f, 0.05f, 0.1f);
        private static readonly Vector2 OutlineDistance = new Vector2(4f, -4f);
        private static readonly Color ButtonColor = new Color(1f, 0.78f, 0.3f);
        private static readonly Color ButtonTextColor = new Color(0.12f, 0.07f, 0.15f);

        // O botão multiplica a cor da imagem: o selecionado fica na cor cheia e os outros um pouco mais escuros,
        // para o teclado e o controle sempre mostrarem onde está a seleção.
        private static readonly Color ButtonIdleTint = new Color(0.8f, 0.8f, 0.8f);
        private static readonly Color ButtonPressedTint = new Color(0.6f, 0.6f, 0.6f);

        /// <summary>
        /// Ilustração de fundo, atrás de todas as telas da cena. Cobre a tela inteira em qualquer proporção
        /// (corta as bordas em vez de esticar).
        /// </summary>
        public static void CreateBackground(Transform parent, Sprite sprite)
        {
            GameObject canvas = CreateCanvas("BackgroundCanvas", parent, BackgroundSortingOrder);
            var imageObject = new GameObject("Background", typeof(RectTransform)) { layer = UILayer };
            imageObject.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)imageObject.transform);

            var image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            var fitter = imageObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
        }

        public static GameObject CreateCanvas(string name, Transform parent, int sortingOrder)
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

        /// <summary>Canvas com um painel escuro de tela cheia, que recebe cliques (pausa, resultado e menus).</summary>
        public static GameObject CreateScreenPanel(string canvasName, Transform parent, int sortingOrder)
        {
            GameObject canvas = CreateCanvas(canvasName, parent, sortingOrder);
            canvas.AddComponent<GraphicRaycaster>();
            var panel = new GameObject("Panel", typeof(RectTransform)) { layer = UILayer };
            panel.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)panel.transform);
            panel.AddComponent<Image>().color = PanelColor;
            return panel;
        }

        public static Text CreateText(string name, Transform parent, string content, int fontSize, Color color)
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

        public static Button CreateButton(string name, Transform parent, string label, Vector2 anchor) =>
            CreateButton(name, parent, label, anchor, ButtonSize);

        public static Button CreateButton(string name, Transform parent, string label, Vector2 anchor, Vector2 size)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform)) { layer = UILayer };
            buttonObject.transform.SetParent(parent, false);
            Place((RectTransform)buttonObject.transform, anchor, size);
            var image = buttonObject.AddComponent<Image>();
            image.color = ButtonColor;
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = CreateButtonColors();

            Text text = CreateText("Label", buttonObject.transform, label, ButtonFontSize, ButtonTextColor);
            Stretch((RectTransform)text.transform);
            return button;
        }

        public static void AddEventSystem(Transform parent)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.transform.SetParent(parent, false);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>(); // só o Input System novo (GDD §0)
        }

        public static void AddOutline(GameObject target)
        {
            var outline = target.AddComponent<Outline>();
            outline.effectColor = OutlineColor;
            outline.effectDistance = OutlineDistance;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>Ancora no ponto (0 a 1 da tela) e centraliza com o tamanho dado.</summary>
        public static void Place(RectTransform rect, Vector2 anchor, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
        }

        public static void PinToCorner(RectTransform rect, Vector2 corner, Vector2 offset, Vector2 size)
        {
            rect.anchorMin = corner;
            rect.anchorMax = corner;
            rect.pivot = corner;
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;
        }

        private static ColorBlock CreateButtonColors()
        {
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = ButtonIdleTint;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = ButtonPressedTint;
            colors.disabledColor = Color.white; // fase bloqueada: a cor vem do próprio LevelButtonUI
            return colors;
        }
    }
}
