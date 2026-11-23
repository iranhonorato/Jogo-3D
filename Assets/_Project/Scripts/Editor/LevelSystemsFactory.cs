using Guaxinim.Core;
using Guaxinim.Noise;
using Guaxinim.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera o prefab LevelSystems, que vai em toda cena de fase: o NoiseRingSpawner (anéis de som), o
    /// LevelFlow (ser pego e reiniciar, GDD §6.5) e o aviso "Você foi pego!" na tela. O jogador do
    /// LevelFlow é ligado na cena.
    /// </summary>
    public static class LevelSystemsFactory
    {
        public const string SystemsPrefabPath = "Assets/_Project/Prefabs/Systems/LevelSystems.prefab";

        private const string FlowDataPath = "Assets/_Project/Data/Levels/LevelFlowData.asset";
        private const string CaughtText = "Você foi pego!";
        private const string BuiltinFontName = "LegacyRuntime.ttf";
        private const int CaughtFontSize = 110;
        private const int UILayer = 5;
        private const int CanvasSortingOrder = 10;

        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Color CaughtTextColor = new Color(1f, 0.88f, 0.7f);
        private static readonly Color OutlineColor = new Color(0.1f, 0.05f, 0.1f);
        private static readonly Vector2 OutlineDistance = new Vector2(4f, -4f);

        [MenuItem("Tools/Guaxinim/Greybox/Sistemas da Fase")]
        internal static void CreateLevelSystemsPrefab()
        {
            NoiseRingVFX ringPrefab = NoiseSystemFactory.CreateNoiseAssets();

            var systems = new GameObject("LevelSystems");
            var spawner = systems.AddComponent<NoiseRingSpawner>();
            EditorAssets.SetReference(spawner, "_ringPrefab", ringPrefab);
            var flow = systems.AddComponent<LevelFlow>();
            EditorAssets.SetReference(flow, "_data", EditorAssets.LoadOrCreate<LevelFlowData>(FlowDataPath));
            AddCaughtMessage(systems.transform, flow);

            PrefabUtility.SaveAsPrefabAsset(systems, SystemsPrefabPath);
            Object.DestroyImmediate(systems);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(SystemsPrefabPath);
        }

        private static void AddCaughtMessage(Transform parent, LevelFlow flow)
        {
            var canvasObject = new GameObject("CaughtCanvas") { layer = UILayer };
            canvasObject.transform.SetParent(parent, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CanvasSortingOrder;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject message = CreateMessageText(canvasObject.transform);
            var caughtUI = canvasObject.AddComponent<CaughtMessageUI>();
            EditorAssets.SetReference(caughtUI, "_levelFlow", flow);
            EditorAssets.SetReference(caughtUI, "_message", message);
            message.SetActive(false);
        }

        private static GameObject CreateMessageText(Transform canvas)
        {
            var textObject = new GameObject("Message", typeof(RectTransform)) { layer = UILayer };
            textObject.transform.SetParent(canvas, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = textObject.AddComponent<Text>();
            text.text = CaughtText;
            text.font = Resources.GetBuiltinResource<Font>(BuiltinFontName);
            text.fontSize = CaughtFontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = CaughtTextColor;
            text.raycastTarget = false;

            var outline = textObject.AddComponent<Outline>();
            outline.effectColor = OutlineColor;
            outline.effectDistance = OutlineDistance;
            return textObject;
        }
    }
}
