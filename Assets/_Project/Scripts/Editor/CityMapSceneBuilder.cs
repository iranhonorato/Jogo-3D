using Guaxinim.Core;
using Guaxinim.UI;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Monta o mapa da cidade (GDD §13): uma fachada por fase do catálogo e a tela de início da fase (nome, lista
    /// de compras e Começar). As fachadas são greybox: um cartão com nome, três estrelas e o estado da fase.
    /// </summary>
    internal static class CityMapSceneBuilder
    {
        private const string MapTitle = "Mapa da cidade";
        private const string MenuText = "Menu";
        private const string StartText = "Começar";
        private const string BackText = "Voltar";
        private const int TitleFontSize = 90;
        private const int BriefingBodyFontSize = 40;
        private const int LevelNameFontSize = 38;
        private const int LevelStatusFontSize = 30;
        private const int StarCount = 3;
        private const float StarSize = 34f;
        private const float StarSpacing = 64f;
        private const float StarHeightAnchor = 0.36f;
        private const float LevelButtonSpacing = 0.19f;
        private const float LevelTextPadding = 16f;
        private const int BriefingSortingOrder = 1;

        private static readonly Vector2 LevelButtonSize = new Vector2(340f, 380f);

        public static void Build(Transform root, LevelCatalog catalog)
        {
            var mapUI = root.gameObject.AddComponent<CityMapUI>();

            GameObject mapPanel = EditorUI.CreateScreenPanel("MapCanvas", root, 0);
            Text title = EditorUI.CreateText("Title", mapPanel.transform, MapTitle, TitleFontSize, EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)title.transform, new Vector2(0.5f, 0.88f), new Vector2(1400f, 140f));
            EditorUI.AddOutline(title.gameObject);

            var levelButtons = new Object[catalog.Levels.Count];
            for (int i = 0; i < levelButtons.Length; i++)
            {
                levelButtons[i] = CreateLevelButton(mapPanel.transform, i, levelButtons.Length);
            }

            Button menu = EditorUI.CreateButton("MenuButton", mapPanel.transform, MenuText, new Vector2(0.5f, 0.1f));
            UnityEventTools.AddPersistentListener(menu.onClick, mapUI.GoToMainMenu);

            GameObject briefingPanel = EditorUI.CreateScreenPanel("BriefingCanvas", root, BriefingSortingOrder);
            Text briefingTitle = EditorUI.CreateText("Title", briefingPanel.transform, string.Empty, TitleFontSize,
                EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)briefingTitle.transform, new Vector2(0.5f, 0.84f), new Vector2(1500f, 140f));
            EditorUI.AddOutline(briefingTitle.gameObject);
            Text briefingBody = EditorUI.CreateText("Body", briefingPanel.transform, string.Empty, BriefingBodyFontSize,
                Color.white);
            briefingBody.fontStyle = FontStyle.Normal;
            briefingBody.alignment = TextAnchor.UpperLeft;
            EditorUI.Place((RectTransform)briefingBody.transform, new Vector2(0.5f, 0.48f), new Vector2(1000f, 560f));
            Button start = EditorUI.CreateButton("StartButton", briefingPanel.transform, StartText, new Vector2(0.3f, 0.1f));
            Button back = EditorUI.CreateButton("BackButton", briefingPanel.transform, BackText, new Vector2(0.7f, 0.1f));
            UnityEventTools.AddPersistentListener(start.onClick, mapUI.StartSelectedLevel);
            UnityEventTools.AddPersistentListener(back.onClick, mapUI.CloseBriefing);

            EditorAssets.SetReference(mapUI, "_catalog", catalog);
            EditorAssets.SetReferenceArray(mapUI, "_levelButtons", levelButtons);
            EditorAssets.SetReference(mapUI, "_mapPanel", mapPanel);
            EditorAssets.SetReference(mapUI, "_menuButton", menu);
            EditorAssets.SetReference(mapUI, "_briefingPanel", briefingPanel);
            EditorAssets.SetReference(mapUI, "_briefingTitle", briefingTitle);
            EditorAssets.SetReference(mapUI, "_briefingBody", briefingBody);
            EditorAssets.SetReference(mapUI, "_startButton", start);
            briefingPanel.SetActive(false);
        }

        /// <summary>Cartão de uma fase: nome em cima, três estrelas no meio e o estado embaixo.</summary>
        private static LevelButtonUI CreateLevelButton(Transform parent, int index, int count)
        {
            float x = 0.5f + (index - (count - 1) / 2f) * LevelButtonSpacing;
            Button button = EditorUI.CreateButton($"Level_{index + 1}", parent, string.Empty, new Vector2(x, 0.5f),
                LevelButtonSize);

            Text nameLabel = button.GetComponentInChildren<Text>();
            nameLabel.fontSize = LevelNameFontSize;
            AnchorToArea(nameLabel, new Vector2(0f, 0.5f), Vector2.one);

            Text statusLabel = EditorUI.CreateText("Status", button.transform, string.Empty, LevelStatusFontSize,
                Color.black);
            AnchorToArea(statusLabel, Vector2.zero, new Vector2(1f, 0.25f));

            var stars = new Object[StarCount];
            for (int i = 0; i < StarCount; i++)
            {
                stars[i] = CreateStar(button.transform, i);
            }

            var levelButton = button.gameObject.AddComponent<LevelButtonUI>();
            EditorAssets.SetReference(levelButton, "_button", button);
            EditorAssets.SetReference(levelButton, "_background", button.GetComponent<Image>());
            EditorAssets.SetReference(levelButton, "_nameLabel", nameLabel);
            EditorAssets.SetReference(levelButton, "_statusLabel", statusLabel);
            EditorAssets.SetReferenceArray(levelButton, "_stars", stars);
            UnityEventTools.AddPersistentListener(button.onClick, levelButton.Select);
            return levelButton;
        }

        /// <summary>Estrela greybox: um losango (quadrado girado), colorido pelo LevelButtonUI.</summary>
        private static Image CreateStar(Transform parent, int index)
        {
            var star = new GameObject($"Star_{index + 1}", typeof(RectTransform)) { layer = EditorUI.UILayer };
            star.transform.SetParent(parent, false);
            var rect = (RectTransform)star.transform;
            EditorUI.Place(rect, new Vector2(0.5f, StarHeightAnchor), Vector2.one * StarSize);
            rect.anchoredPosition = new Vector2((index - (StarCount - 1) / 2f) * StarSpacing, 0f);
            rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var image = star.AddComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        /// <summary>Texto ocupando uma faixa do cartão, com uma margem pequena.</summary>
        private static void AnchorToArea(Text text, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rect = (RectTransform)text.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(LevelTextPadding, LevelTextPadding);
            rect.offsetMax = new Vector2(-LevelTextPadding, -LevelTextPadding);
        }
    }
}
