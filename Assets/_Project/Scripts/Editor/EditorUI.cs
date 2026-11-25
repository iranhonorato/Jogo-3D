using Guaxinim.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Peças de uGUI para os montadores de tela (HUD, pausa, resultado, menu e mapa): canvas, texto, botão e painel,
    /// no mesmo estilo. Duas fontes de licença livre (OFL): Lilita One nos títulos e botões, e Nunito ExtraBold no
    /// resto (arredondada, limpa e legível mesmo pequena). Todo texto avulso ganha contorno escuro e sombra, para ser
    /// lido sobre as ilustrações e sobre a fase 3D. As cores dos botões vêm do <see cref="UITheme"/>. Tudo é criado
    /// por código para a cena ser reproduzível.
    /// </summary>
    internal static class EditorUI
    {
        public const int UILayer = 5;
        public const int ButtonFontSize = 44;
        public const int BackgroundSortingOrder = -1;

        private const string ThemePath = "Assets/_Project/Data/UI/UITheme.asset";
        private const string TitleFontPath = "Assets/_Project/Art/Fonts/LilitaOne-Regular.ttf";
        private const string BodyFontPath = "Assets/_Project/Art/Fonts/Nunito-ExtraBold.ttf";

        // Contorno proporcional à letra: fino nos textos pequenos, grosso nos títulos (unidades da tela de 1080p).
        private const float OutlinePerFontSize = 0.08f;
        private const float MinOutlineWidth = 2f;
        private const float MaxOutlineWidth = 6f;
        private const float ShadowExtraDrop = 2f;
        private const string DataFolder = "Assets/_Project/Data/UI";
        private const string BuiltinFontName = "LegacyRuntime.ttf";
        private const int ArrowFontSize = 36;
        private const float ArrowOffset = 24f;
        private const float ArrowSize = 60f;

        public static readonly Vector2 ButtonSize = new Vector2(460f, 110f);
        public static readonly Color LightTextColor = new Color(1f, 0.88f, 0.7f);
        public static readonly Color PanelColor = new Color(0.05f, 0.03f, 0.1f, 0.85f);

        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Color OutlineColor = new Color(0.06f, 0.03f, 0.08f, 0.95f);
        private static readonly Color ShadowColor = new Color(0f, 0f, 0f, 0.55f);

        /// <summary>O tema das telas. Carregue dentro da cena nova: o NewScene descarrega os assets sem uso.</summary>
        public static UITheme LoadTheme()
        {
            EditorAssets.EnsureFolder(DataFolder);
            return EditorAssets.LoadOrCreate<UITheme>(ThemePath);
        }

        /// <summary>
        /// Usa a fonte de título (Lilita One, licença OFL) no texto. Sem o arquivo da fonte, o texto fica com a fonte
        /// padrão. Carregue dentro da cena nova: o NewScene descarrega os assets sem uso.
        /// </summary>
        public static void ApplyTitleFont(Text text)
        {
            text.font = LoadFont(TitleFontPath);
            text.fontStyle = FontStyle.Normal; // a Lilita One já é pesada: o negrito falso só borra
        }

        /// <summary>Título ou aviso grande, na fonte de título (Lilita One).</summary>
        public static Text CreateTitle(string name, Transform parent, string content, int fontSize, Color color)
        {
            Text title = CreateText(name, parent, content, fontSize, color);
            ApplyTitleFont(title);
            return title;
        }

        /// <summary>
        /// Ilustração de fundo, atrás de todas as telas da cena. Cobre a tela inteira em qualquer proporção
        /// (corta as bordas em vez de esticar). O brilho escurece a imagem para o texto aparecer.
        /// </summary>
        public static void CreateBackground(Transform parent, Sprite sprite, float brightness = 1f)
        {
            GameObject canvas = CreateCanvas("BackgroundCanvas", parent, BackgroundSortingOrder);
            var imageObject = new GameObject("Background", typeof(RectTransform)) { layer = UILayer };
            imageObject.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)imageObject.transform);

            var image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = new Color(brightness, brightness, brightness, 1f);
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

        /// <summary>Muda a opacidade do véu escuro do painel (0 deixa a ilustração de fundo aparecer inteira).</summary>
        public static void SetPanelOpacity(GameObject panel, float alpha)
        {
            Color color = PanelColor;
            color.a = alpha;
            panel.GetComponent<Image>().color = color;
        }

        /// <summary>Texto avulso (HUD, títulos, painéis), com contorno e sombra para ser lido sobre qualquer fundo.</summary>
        public static Text CreateText(string name, Transform parent, string content, int fontSize, Color color)
        {
            Text text = CreateLabel(name, parent, content, fontSize, color);
            AddReadability(text);
            return text;
        }

        /// <summary>
        /// Contorno escuro (proporcional ao tamanho da letra) e sombra caindo para baixo. Os rótulos dos botões não
        /// usam: já ficam sobre um fundo sólido.
        /// </summary>
        private static void AddReadability(Text text)
        {
            float width = Mathf.Clamp(text.fontSize * OutlinePerFontSize, MinOutlineWidth, MaxOutlineWidth);
            var shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = ShadowColor;
            shadow.effectDistance = new Vector2(0f, -(width + ShadowExtraDrop));
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = OutlineColor;
            outline.effectDistance = new Vector2(width, -width);
        }

        /// <summary>Texto sem efeitos, para os rótulos dos botões.</summary>
        private static Text CreateLabel(string name, Transform parent, string content, int fontSize, Color color)
        {
            var textObject = new GameObject(name, typeof(RectTransform)) { layer = UILayer };
            textObject.transform.SetParent(parent, false);
            var text = textObject.AddComponent<Text>();
            text.text = content;
            text.font = LoadFont(BodyFontPath);
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Normal; // a Nunito ExtraBold já é pesada: o negrito falso só borra
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        public static Button CreateButton(string name, Transform parent, string label, Vector2 anchor) =>
            CreateButton(name, parent, label, anchor, ButtonSize, SelectableHighlight.HighlightStyle.Button);

        /// <summary>
        /// Botão com o destaque de seleção (<see cref="SelectableHighlight"/>). O estilo Cartão é das fachadas do
        /// mapa: só contorno e tamanho mudam, e as cores ficam com o <see cref="LevelButtonUI"/>.
        /// </summary>
        public static Button CreateButton(string name, Transform parent, string label, Vector2 anchor, Vector2 size,
            SelectableHighlight.HighlightStyle style)
        {
            UITheme theme = LoadTheme();
            var buttonObject = new GameObject(name, typeof(RectTransform)) { layer = UILayer };
            buttonObject.transform.SetParent(parent, false);
            Place((RectTransform)buttonObject.transform, anchor, size);

            var image = buttonObject.AddComponent<Image>();
            bool isCard = style == SelectableHighlight.HighlightStyle.Card;
            image.color = isCard ? theme.SelectedBackground : theme.NormalBackground;
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None; // o SelectableHighlight cuida de todos os estados

            var outline = buttonObject.AddComponent<Outline>();
            outline.effectColor = theme.NormalOutline;
            outline.effectDistance = new Vector2(theme.ThinOutline, -theme.ThinOutline);

            Text text = CreateLabel("Label", buttonObject.transform, label, ButtonFontSize,
                isCard ? theme.SelectedText : theme.NormalText);
            if (!isCard)
            {
                ApplyTitleFont(text); // as fachadas do mapa têm nome e estrelas: ficam com a fonte de texto
            }

            Stretch((RectTransform)text.transform);

            Text arrow = isCard ? null : CreateArrow(buttonObject.transform, theme);
            AddHighlight(buttonObject, theme, style, button, image, text, outline, arrow);
            return button;
        }

        public static void AddEventSystem(Transform parent)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.transform.SetParent(parent, false);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>(); // só o Input System novo (GDD §0)
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

        /// <summary>
        /// Fonte do projeto. Sem o arquivo, cai na fonte padrão da Unity com um aviso. Carregue dentro da cena nova:
        /// o NewScene descarrega os assets sem uso.
        /// </summary>
        private static Font LoadFont(string path)
        {
            var font = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(path);
            if (font != null)
            {
                return font;
            }

            Debug.LogWarning($"Font not found at {path}; using the default font.");
            return Resources.GetBuiltinResource<Font>(BuiltinFontName);
        }

        /// <summary>Seta ao lado esquerdo do botão, escondida até ele ser selecionado.</summary>
        private static Text CreateArrow(Transform button, UITheme theme)
        {
            Text arrow = CreateLabel("Arrow", button, theme.SelectionArrow, ArrowFontSize, theme.SelectedText);
            arrow.font = Resources.GetBuiltinResource<Font>(BuiltinFontName); // as fontes do jogo não têm o ►
            var rect = (RectTransform)arrow.transform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = Vector2.one * ArrowSize;
            rect.anchoredPosition = new Vector2(ArrowOffset, 0f);
            arrow.alignment = TextAnchor.MiddleLeft;
            arrow.enabled = false;
            return arrow;
        }

        private static void AddHighlight(GameObject buttonObject, UITheme theme,
            SelectableHighlight.HighlightStyle style, Button button, Image image, Text label, Outline outline, Text arrow)
        {
            var highlight = buttonObject.AddComponent<SelectableHighlight>();
            EditorAssets.SetReference(highlight, "_theme", theme);
            EditorAssets.SetReference(highlight, "_button", button);
            EditorAssets.SetReference(highlight, "_background", image);
            EditorAssets.SetReference(highlight, "_label", label);
            EditorAssets.SetReference(highlight, "_outline", outline);
            EditorAssets.SetReference(highlight, "_arrow", arrow);
            var serialized = new UnityEditor.SerializedObject(highlight);
            serialized.FindProperty("_style").enumValueIndex = (int)style;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
