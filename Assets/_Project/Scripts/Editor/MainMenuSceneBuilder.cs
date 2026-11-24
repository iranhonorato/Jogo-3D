using Guaxinim.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Monta a tela do menu principal (GDD §13): Jogar, Como jogar, Créditos e Sair, mais os dois painéis de texto.
    /// Fica sobre a ilustração de fundo, com o título e os botões na coluna da esquerda, que é a parte vazia da
    /// imagem, para os personagens aparecerem inteiros.
    /// </summary>
    internal static class MainMenuSceneBuilder
    {
        private const string BackgroundPath = "Assets/_Project/Art/UI/Background_MainMenu.jpg";
        private const string GameTitle = "Operação\nGuaxinim";
        private const int GameTitleFontSize = 104;
        private const float MenuColumnX = 0.175f;
        private const int InfoTitleFontSize = 80;
        private const int InfoBodyFontSize = 36;
        private const string BackText = "Voltar";

        private const string HowToPlayTitle = "Como jogar";

        private const string HowToPlayBody =
            "Você é um guaxinim. Roube os itens da lista de compras e leve até a toca, sem ser pego pelos seguranças.\n\n" +
            "WASD  mover (silencioso)\n" +
            "Shift  correr (faz barulho)\n" +
            "E  pegar item, entrar e sair da lixeira\n" +
            "Q  largar o último item\n" +
            "F  assobiar (distrai os seguranças)\n" +
            "Tab (segurar)  ver a lista inteira\n" +
            "Esc  pausar\n\n" +
            "Item pesado deixa lento, volumoso tapa a visão e não passa em vãos estreitos, e frágil quebra se você correr.";

        private const string CreditsTitle = "Créditos";

        private const string CreditsBody =
            "Jogos Digitais, Jogos e Interação, Insper 2026.2\n\n" +
            "Modelos 3D e texturas: gerados por scripts do Blender escritos com o Claude (Anthropic)\n" +
            "Ferramentas: Unity 6, Blender e Claude Code (Anthropic)\n\n" +
            "Os créditos completos, com fonte e licença de cada asset, estão no README do repositório.";

        public static void Build(Transform root)
        {
            var menu = root.gameObject.AddComponent<MainMenuUI>();
            AddBackground(root);

            GameObject mainPanel = EditorUI.CreateScreenPanel("MainCanvas", root, 0);
            mainPanel.GetComponent<Image>().color = Color.clear; // sem o véu escuro: a ilustração aparece inteira
            Text title = EditorUI.CreateText("Title", mainPanel.transform, GameTitle, GameTitleFontSize, EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)title.transform, new Vector2(MenuColumnX, 0.76f), new Vector2(700f, 320f));
            EditorUI.AddOutline(title.gameObject);

            Button play = CreateMenuButton(mainPanel.transform, "PlayButton", "Jogar", 0.52f);
            Button howToPlay = CreateMenuButton(mainPanel.transform, "HowToPlayButton", "Como jogar", 0.39f);
            Button credits = CreateMenuButton(mainPanel.transform, "CreditsButton", "Créditos", 0.26f);
            Button quit = CreateMenuButton(mainPanel.transform, "QuitButton", "Sair", 0.13f);
            UnityEventTools.AddPersistentListener(play.onClick, menu.Play);
            UnityEventTools.AddPersistentListener(howToPlay.onClick, menu.ShowHowToPlay);
            UnityEventTools.AddPersistentListener(credits.onClick, menu.ShowCredits);
            UnityEventTools.AddPersistentListener(quit.onClick, menu.Quit);

            GameObject howToPlayPanel = CreateInfoPanel(root, "HowToPlayCanvas", HowToPlayTitle, HowToPlayBody,
                menu, out Button howToPlayBack);
            GameObject creditsPanel = CreateInfoPanel(root, "CreditsCanvas", CreditsTitle, CreditsBody, menu,
                out Button creditsBack);

            EditorAssets.SetReference(menu, "_mainPanel", mainPanel);
            EditorAssets.SetReference(menu, "_howToPlayPanel", howToPlayPanel);
            EditorAssets.SetReference(menu, "_creditsPanel", creditsPanel);
            EditorAssets.SetReference(menu, "_playButton", play);
            EditorAssets.SetReference(menu, "_howToPlayBackButton", howToPlayBack);
            EditorAssets.SetReference(menu, "_creditsBackButton", creditsBack);
            EditorAssets.SetReference(menu, "_quitButton", quit.gameObject);
        }

        /// <summary>
        /// A ilustração é carregada aqui, depois da cena nova: o NewScene descarrega os assets sem uso, e uma
        /// referência carregada antes viraria null. Sem a imagem, o menu fica com a cor de fundo da câmera.
        /// </summary>
        private static void AddBackground(Transform root)
        {
            var background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            if (background == null)
            {
                Debug.LogWarning($"Main menu background not found at {BackgroundPath}; using the plain color.");
                return;
            }

            EditorUI.CreateBackground(root, background);
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, float heightAnchor) =>
            EditorUI.CreateButton(name, parent, label, new Vector2(MenuColumnX, heightAnchor));

        /// <summary>Painel com título, texto e o botão Voltar, que leva ao menu principal.</summary>
        private static GameObject CreateInfoPanel(Transform root, string canvasName, string titleText, string bodyText,
            MainMenuUI menu, out Button backButton)
        {
            GameObject panel = EditorUI.CreateScreenPanel(canvasName, root, 0);
            Text title = EditorUI.CreateText("Title", panel.transform, titleText, InfoTitleFontSize, EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)title.transform, new Vector2(0.5f, 0.88f), new Vector2(1400f, 140f));
            EditorUI.AddOutline(title.gameObject);

            Text body = EditorUI.CreateText("Body", panel.transform, bodyText, InfoBodyFontSize, Color.white);
            body.fontStyle = FontStyle.Normal;
            body.alignment = TextAnchor.UpperLeft;
            EditorUI.Place((RectTransform)body.transform, new Vector2(0.5f, 0.5f), new Vector2(1300f, 620f));

            backButton = EditorUI.CreateButton("BackButton", panel.transform, BackText, new Vector2(0.5f, 0.1f));
            UnityEventTools.AddPersistentListener(backButton.onClick, menu.ShowMain);
            panel.SetActive(false);
            return panel;
        }
    }
}
