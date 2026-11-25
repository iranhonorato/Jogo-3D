using Guaxinim.Core;
using Guaxinim.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Monta o mapa da cidade (GDD §13): uma fachada por fase do catálogo sobre a ilustração do mapa, e a tela de
    /// início da fase (nome, lista de compras e Começar) com o mesmo mapa escurecido atrás. As fachadas são
    /// greybox: um cartão com nome, três estrelas e o estado da fase. Embaixo delas, a estrada pontilhada com uma
    /// parada por fase e o marcador do guaxinim, que viaja entre os mercados (Etapa 4).
    /// </summary>
    internal static class CityMapSceneBuilder
    {
        private const string BackgroundPath = "Assets/_Project/Art/UI/Background_CityMap.jpg";
        private const string MapTitle = "Mapa da cidade";
        private const string MenuText = "Menu";
        private const string StartText = "Começar";
        private const string BackText = "Voltar";
        private const int TitleFontSize = 90;
        private const int BriefingBodyFontSize = 40;
        private const int LevelNameFontSize = 38;
        private const int LevelStatusFontSize = 32;
        private const int StarCount = 3;
        private const float StarSize = 34f;
        private const float StarSpacing = 64f;
        private const float StarHeightAnchor = 0.36f;
        private const float LevelButtonSpacing = 0.19f;
        private const float LevelTextPadding = 16f;
        private const float BriefingVeilOpacity = 0.75f;
        private const int BriefingSortingOrder = 1;
        private const string DotSpritePath = "Assets/_Project/Art/UI/Dot.png";
        private const string RaccoonIconPath = "Assets/_Project/Art/UI/Icon_Raccoon.png";
        private const float RoadHeightAnchor = 0.24f;
        private const int DotsPerSegment = 5;
        private const float RoadDotSize = 16f;
        private const float StopSize = 34f;
        private const float MarkerSize = 96f;

        private static readonly Color StopColor = new Color32(0xFF, 0xF1, 0xD6, 0xFF);

        private static readonly Vector2 LevelButtonSize = new Vector2(340f, 380f);

        public static void Build(Transform root, LevelCatalog catalog)
        {
            var mapUI = root.gameObject.AddComponent<CityMapUI>();
            AddBackground(root);

            GameObject mapPanel = EditorUI.CreateScreenPanel("MapCanvas", root, 0);
            EditorUI.SetPanelOpacity(mapPanel, 0f); // o mapa ilustrado aparece inteiro
            Text title = EditorUI.CreateTitle("Title", mapPanel.transform, MapTitle, TitleFontSize, EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)title.transform, new Vector2(0.5f, 0.88f), new Vector2(1400f, 140f));

            var levelButtons = new Object[catalog.Levels.Count];
            for (int i = 0; i < levelButtons.Length; i++)
            {
                levelButtons[i] = CreateLevelButton(mapPanel.transform, i, levelButtons.Length);
            }

            var travel = root.gameObject.AddComponent<MapTravelAnimator>();
            BuildRoad(mapPanel.transform, travel, levelButtons.Length);
            EditorAssets.SetReference(mapUI, "_travel", travel);
            EditorAssets.SetReference(mapUI, "_mapGroup", mapPanel.AddComponent<CanvasGroup>());

            Button menu = EditorUI.CreateButton("MenuButton", mapPanel.transform, MenuText, new Vector2(0.5f, 0.1f));
            UnityEventTools.AddPersistentListener(menu.onClick, mapUI.GoToMainMenu);

            GameObject briefingPanel = EditorUI.CreateScreenPanel("BriefingCanvas", root, BriefingSortingOrder);
            EditorUI.SetPanelOpacity(briefingPanel, BriefingVeilOpacity); // o mesmo mapa, escurecido
            Text briefingTitle = EditorUI.CreateTitle("Title", briefingPanel.transform, string.Empty, TitleFontSize,
                EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)briefingTitle.transform, new Vector2(0.5f, 0.84f), new Vector2(1500f, 140f));
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

        /// <summary>
        /// Estrada embaixo das fachadas: uma parada por fase, pontos entre elas e o marcador do guaxinim, todos irmãos
        /// num mesmo pai (o MapTravelAnimator move o marcador pelas posições locais das paradas).
        /// </summary>
        private static void BuildRoad(Transform mapPanel, MapTravelAnimator travel, int stopCount)
        {
            var road = new GameObject("Road", typeof(RectTransform)) { layer = EditorUI.UILayer };
            road.transform.SetParent(mapPanel, false);
            EditorUI.Stretch((RectTransform)road.transform);
            var dotSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DotSpritePath);

            var dots = new Object[(stopCount - 1) * DotsPerSegment];
            for (int segment = 0; segment < stopCount - 1; segment++)
            {
                for (int i = 0; i < DotsPerSegment; i++)
                {
                    float t = (i + 1f) / (DotsPerSegment + 1f);
                    float x = Mathf.Lerp(LevelButtonX(segment, stopCount), LevelButtonX(segment + 1, stopCount), t);
                    dots[segment * DotsPerSegment + i] = CreateRoadImage($"Dot_{segment + 1}_{i + 1}", road.transform,
                        dotSprite, x, RoadDotSize, Color.white);
                }
            }

            var stops = new Object[stopCount];
            for (int i = 0; i < stopCount; i++)
            {
                stops[i] = CreateRoadImage($"Stop_{i + 1}", road.transform, dotSprite, LevelButtonX(i, stopCount),
                    StopSize, StopColor).rectTransform;
            }

            Image marker = CreateRoadImage("RaccoonMarker", road.transform,
                AssetDatabase.LoadAssetAtPath<Sprite>(RaccoonIconPath), LevelButtonX(0, stopCount), MarkerSize, Color.white);
            marker.rectTransform.pivot = new Vector2(0.5f, 0.15f); // em pé sobre a parada
            marker.preserveAspect = true;

            EditorAssets.SetReference(travel, "_marker", marker.rectTransform);
            EditorAssets.SetReferenceArray(travel, "_stops", stops);
            EditorAssets.SetReferenceArray(travel, "_roadDots", dots);
            var serializedTravel = new SerializedObject(travel);
            serializedTravel.FindProperty("_dotsPerSegment").intValue = DotsPerSegment;
            serializedTravel.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Image CreateRoadImage(string name, Transform road, Sprite sprite, float anchorX, float size,
            Color color)
        {
            var imageObject = new GameObject(name, typeof(RectTransform)) { layer = EditorUI.UILayer };
            imageObject.transform.SetParent(road, false);
            EditorUI.Place((RectTransform)imageObject.transform, new Vector2(anchorX, RoadHeightAnchor), Vector2.one * size);
            var image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>Âncora X da fachada (e da parada) da fase <paramref name="index"/>, centradas na tela.</summary>
        private static float LevelButtonX(int index, int count) => 0.5f + (index - (count - 1) / 2f) * LevelButtonSpacing;

        /// <summary>A ilustração é carregada aqui, depois da cena nova (ver o MainMenuSceneBuilder).</summary>
        private static void AddBackground(Transform root)
        {
            var background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            if (background == null)
            {
                Debug.LogWarning($"City map background not found at {BackgroundPath}; using the plain color.");
                return;
            }

            EditorUI.CreateBackground(root, background);
        }

        /// <summary>Cartão de uma fase: nome em cima, três estrelas no meio e o estado embaixo.</summary>
        private static LevelButtonUI CreateLevelButton(Transform parent, int index, int count)
        {
            Button button = EditorUI.CreateButton($"Level_{index + 1}", parent, string.Empty,
                new Vector2(LevelButtonX(index, count), 0.5f),
                LevelButtonSize, SelectableHighlight.HighlightStyle.Card);

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
            EditorAssets.SetReference(levelButton, "_highlight", button.GetComponent<SelectableHighlight>());
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
