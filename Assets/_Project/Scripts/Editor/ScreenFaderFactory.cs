using Guaxinim.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera o prefab do fade entre cenas (passo 5 das telas, GDD §13). Fica em <c>Resources</c> para o
    /// <see cref="ScreenFader"/> criá-lo antes da primeira cena: o jogo abre direto em qualquer cena, no Editor
    /// ou no build. Rodar de novo atualiza o prefab, mantendo o GUID.
    /// </summary>
    public static class ScreenFaderFactory
    {
        public const string PrefabPath = "Assets/_Project/Resources/ScreenFader.prefab";

        private const string ResourcesFolder = "Assets/_Project/Resources";
        private const int SortingOrder = 5000; // acima de toda tela do jogo

        [MenuItem("Tools/Guaxinim/Greybox/Fade entre Cenas")]
        public static void CreatePrefab()
        {
            EditorAssets.EnsureFolder(ResourcesFolder);

            var root = new GameObject("ScreenFader") { layer = EditorUI.UILayer };
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            root.AddComponent<GraphicRaycaster>();
            var group = root.AddComponent<CanvasGroup>();

            var cover = new GameObject("Cover", typeof(RectTransform)) { layer = EditorUI.UILayer };
            cover.transform.SetParent(root.transform, false);
            EditorUI.Stretch((RectTransform)cover.transform);
            cover.AddComponent<Image>().color = Color.black; // recebe cliques: nada passa enquanto está escuro

            var fader = root.AddComponent<ScreenFader>();
            EditorAssets.SetReference(fader, "_group", group);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }
    }
}
