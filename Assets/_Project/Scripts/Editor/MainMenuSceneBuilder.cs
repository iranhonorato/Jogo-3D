using Guaxinim.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Monta a tela do menu principal (GDD §13): Jogar, Como jogar, Créditos e Sair, mais os painéis Como jogar
    /// (comandos no teclado e no controle) e Créditos. Fica sobre a ilustração de fundo, com o título e os botões na
    /// coluna da esquerda, que é a parte vazia da imagem, para os personagens aparecerem inteiros.
    /// </summary>
    internal static class MainMenuSceneBuilder
    {
        private const string BackgroundPath = "Assets/_Project/Art/UI/Background_MainMenu.jpg";
        private const string GameTitle = "Operação\nGuaxinim";
        private const int GameTitleFontSize = 116; // a Lilita One é mais estreita que a fonte padrão
        private const float MenuColumnX = 0.175f;
        private const int InfoTitleFontSize = 80;
        private const int InfoBodyFontSize = 38;
        private const string BackText = "Voltar";

        private const string HowToPlayTitle = "Como jogar";
        private const int HowToPlayFontSize = 36;
        private const int HowToPlayNotesFontSize = 30;
        private const float HowToPlayWidth = 1500f;

        private const string HowToPlayIntro =
            "Você é um guaxinim. Roube os itens da lista de compras e leve até a toca, sem ser pego pelos seguranças.";

        private const string HowToPlayRules =
            "Item pesado deixa lento, volumoso tapa a visão e não passa em vãos estreitos, e frágil quebra se você correr.";

        private const string CreditsTitle = "Créditos";

        private const string CreditsBody =
            "Alunos do 5º semestre de Ciência da Computação do Insper\n" +
            "Esthefanny Sousa Farias\n" +
            "Antonio Iranilson Honorato\n" +
            "Enzo Perego\n" +
            "Pedro Henrique Mourao\n\n" +
            "Jogos Digitais, Jogos e Interação, Insper 2026.2\n" +
            "Modelos 3D, texturas, música e efeitos sonoros feitos para este projeto\n" +
            "Ferramentas: Unity 6 e Blender\n" +
            "Fontes: Lilita One e Nunito (SIL Open Font License 1.1)\n" +
            "Os créditos completos, com fonte e licença de cada asset, estão no README do repositório.";

        // Colunas da tabela de comandos (fração da largura): ação, teclado e controle.
        private static readonly ControlsTable.Columns HowToPlayColumns = new ControlsTable.Columns(
            new Vector2(0f, 0.5f), new Vector2(0.52f, 0.7f), new Vector2(0.72f, 1f));

        public static void Build(Transform root)
        {
            var menu = root.gameObject.AddComponent<MainMenuUI>();
            AddBackground(root);

            GameObject mainPanel = EditorUI.CreateScreenPanel("MainCanvas", root, 0);
            mainPanel.GetComponent<Image>().color = Color.clear; // sem o véu escuro: a ilustração aparece inteira
            Text title = EditorUI.CreateTitle("Title", mainPanel.transform, GameTitle, GameTitleFontSize, EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)title.transform, new Vector2(MenuColumnX, 0.76f), new Vector2(700f, 320f));

            Button play = CreateMenuButton(mainPanel.transform, "PlayButton", "Jogar", 0.52f);
            Button howToPlay = CreateMenuButton(mainPanel.transform, "HowToPlayButton", "Como jogar", 0.39f);
            Button credits = CreateMenuButton(mainPanel.transform, "CreditsButton", "Créditos", 0.26f);
            Button quit = CreateMenuButton(mainPanel.transform, "QuitButton", "Sair", 0.13f);
            UnityEventTools.AddPersistentListener(play.onClick, menu.Play);
            UnityEventTools.AddPersistentListener(howToPlay.onClick, menu.ShowHowToPlay);
            UnityEventTools.AddPersistentListener(credits.onClick, menu.ShowCredits);
            UnityEventTools.AddPersistentListener(quit.onClick, menu.Quit);

            GameObject howToPlayPanel = CreateHowToPlayPanel(root, menu, out Button howToPlayBack);
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

        /// <summary>
        /// Como jogar: o objetivo, a tabela de comandos (ação, teclado e controle) e as regras dos itens, com as
        /// dicas dos menus e dos botões do PlayStation no rodapé.
        /// </summary>
        private static GameObject CreateHowToPlayPanel(Transform root, MainMenuUI menu, out Button backButton)
        {
            GameObject panel = CreatePanelFrame(root, "HowToPlayCanvas", HowToPlayTitle, menu, out backButton);

            Text intro = EditorUI.CreateText("Intro", panel.transform, HowToPlayIntro, HowToPlayFontSize, Color.white);
            EditorUI.Place((RectTransform)intro.transform, new Vector2(0.5f, 0.765f), new Vector2(HowToPlayWidth, 100f));

            var table = new GameObject("ControlsTable", typeof(RectTransform)) { layer = EditorUI.UILayer };
            table.transform.SetParent(panel.transform, false);
            var tableRect = (RectTransform)table.transform;
            EditorUI.Place(tableRect, new Vector2(0.5f, 0.52f), new Vector2(HowToPlayWidth, 400f));
            ControlsTable.BuildFull(tableRect, HowToPlayColumns, HowToPlayFontSize);

            string notesText = HowToPlayRules + "\n" + ControlsTable.MenusNote + " " + ControlsTable.GamepadNote;
            Text notes = EditorUI.CreateText("Notes", panel.transform, notesText, HowToPlayNotesFontSize, Color.white);
            notes.alignment = TextAnchor.UpperLeft;
            EditorUI.Place((RectTransform)notes.transform, new Vector2(0.5f, 0.25f), new Vector2(HowToPlayWidth, 170f));
            return panel;
        }

        /// <summary>Painel com título e um texto corrido (Créditos), mais o botão Voltar.</summary>
        private static GameObject CreateInfoPanel(Transform root, string canvasName, string titleText, string bodyText,
            MainMenuUI menu, out Button backButton)
        {
            GameObject panel = CreatePanelFrame(root, canvasName, titleText, menu, out backButton);
            Text body = EditorUI.CreateText("Body", panel.transform, bodyText, InfoBodyFontSize, Color.white);
            body.alignment = TextAnchor.UpperLeft;
            body.verticalOverflow = VerticalWrapMode.Overflow; // texto longo cresce para baixo sem cortar a última linha
            EditorUI.Place((RectTransform)body.transform, new Vector2(0.5f, 0.5f), new Vector2(1300f, 680f));
            return panel;
        }

        /// <summary>Painel escuro de tela cheia com o título e o botão Voltar, que leva ao menu principal.</summary>
        private static GameObject CreatePanelFrame(Transform root, string canvasName, string titleText, MainMenuUI menu,
            out Button backButton)
        {
            GameObject panel = EditorUI.CreateScreenPanel(canvasName, root, 0);
            Text title = EditorUI.CreateTitle("Title", panel.transform, titleText, InfoTitleFontSize, EditorUI.LightTextColor);
            EditorUI.Place((RectTransform)title.transform, new Vector2(0.5f, 0.88f), new Vector2(1400f, 140f));

            backButton = EditorUI.CreateButton("BackButton", panel.transform, BackText, new Vector2(0.5f, 0.1f));
            UnityEventTools.AddPersistentListener(backButton.onClick, menu.ShowMain);
            panel.SetActive(false);
            return panel;
        }
    }
}
