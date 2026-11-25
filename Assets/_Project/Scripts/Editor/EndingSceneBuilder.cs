using Guaxinim.Core;
using Guaxinim.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Monta a tela de fim de jogo (GDD §13), aberta pelo botão Concluir da última fase: título, total de estrelas
    /// e tempo, com Recomeçar, Menu inicial e Créditos, sobre uma ilustração de fundo comemorativa.
    /// </summary>
    internal static class EndingSceneBuilder
    {
        private const string BackgroundPath = "Assets/_Project/Art/UI/Background_Ending.jpg";
        private const string EndingTitle = "Você saqueou a cidade!";
        private const int TitleFontSize = 100;
        private const int SummaryFontSize = 52;
        private const float BackgroundBrightness = 0.75f;
        private const float VeilOpacity = 0.3f;

        public static void Build(Transform root, LevelCatalog catalog)
        {
            var ending = root.gameObject.AddComponent<EndingUI>();
            AddBackground(root);

            GameObject panel = EditorUI.CreateScreenPanel("EndingCanvas", root, 0);
            EditorUI.SetPanelOpacity(panel, VeilOpacity);
            Text title = EditorUI.CreateTitle("Title", panel.transform, EndingTitle, TitleFontSize, EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)title.transform, new Vector2(0.5f, 0.8f), new Vector2(1700f, 170f));
            Text summary = EditorUI.CreateText("Summary", panel.transform, string.Empty, SummaryFontSize, Color.white);
            EditorUI.Place((RectTransform)summary.transform, new Vector2(0.5f, 0.62f), new Vector2(1200f, 200f));

            Button restart = EditorUI.CreateButton("RestartButton", panel.transform, "Recomeçar", new Vector2(0.5f, 0.4f));
            Button mainMenu = EditorUI.CreateButton("MainMenuButton", panel.transform, "Menu inicial", new Vector2(0.5f, 0.27f));
            Button credits = EditorUI.CreateButton("CreditsButton", panel.transform, "Créditos", new Vector2(0.5f, 0.14f));
            UnityEventTools.AddPersistentListener(restart.onClick, ending.Restart);
            UnityEventTools.AddPersistentListener(mainMenu.onClick, ending.GoToMainMenu);
            UnityEventTools.AddPersistentListener(credits.onClick, ending.ShowCredits);

            EditorAssets.SetReference(ending, "_catalog", catalog);
            EditorAssets.SetReference(ending, "_summary", summary);
            EditorAssets.SetReference(ending, "_restartButton", restart);
        }

        /// <summary>A ilustração é carregada aqui, depois da cena nova (ver o MainMenuSceneBuilder).</summary>
        private static void AddBackground(Transform root)
        {
            var background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            if (background == null)
            {
                Debug.LogWarning($"Ending background not found at {BackgroundPath}; using the plain color.");
                return;
            }

            EditorUI.CreateBackground(root, background, BackgroundBrightness);
        }
    }
}
