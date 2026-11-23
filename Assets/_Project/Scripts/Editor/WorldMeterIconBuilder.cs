using Guaxinim.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Monta a barra sobre a cabeça (<see cref="WorldMeterIcon"/>) dentro de um prefab: fundo escuro e barra
    /// colorida, ligados a um componente que implementa IMeterValue.
    /// </summary>
    public static class WorldMeterIconBuilder
    {
        private const string BackgroundMaterialPath = "Assets/_Project/Art/Materials/Greybox/M_Icon_Background.mat";
        private const string FillMaterialPath = "Assets/_Project/Art/Materials/Greybox/M_Icon_Fill.mat";
        private const float Width = 0.8f;
        private const float BarHeight = 0.12f;
        private const float Border = 0.06f;

        private static readonly Color BackgroundColor = new Color(0.08f, 0.07f, 0.1f);

        public static WorldMeterIcon Build(string name, Transform parent, float height, MonoBehaviour source,
            Color lowColor, Color fullColor)
        {
            var iconObject = new GameObject(name);
            iconObject.transform.SetParent(parent, false);
            iconObject.transform.localPosition = Vector3.up * height;
            var visual = new GameObject("Visual");
            visual.transform.SetParent(iconObject.transform, false);

            Material backgroundMaterial = EditorAssets.LoadOrCreateUnlitMaterial(BackgroundMaterialPath, BackgroundColor);
            GameObject background = CreateQuad("Background", visual.transform, backgroundMaterial);
            background.transform.localScale = new Vector3(Width + Border, BarHeight + Border, 1f);
            Material fillMaterial = EditorAssets.LoadOrCreateUnlitMaterial(FillMaterialPath, Color.white);
            GameObject fill = CreateQuad("Fill", visual.transform, fillMaterial);

            var icon = iconObject.AddComponent<WorldMeterIcon>();
            var serialized = new SerializedObject(icon);
            serialized.FindProperty("_source").objectReferenceValue = source;
            serialized.FindProperty("_visualRoot").objectReferenceValue = visual;
            serialized.FindProperty("_fill").objectReferenceValue = fill.transform;
            serialized.FindProperty("_fillRenderer").objectReferenceValue = fill.GetComponent<MeshRenderer>();
            serialized.FindProperty("_lowColor").colorValue = lowColor;
            serialized.FindProperty("_fullColor").colorValue = fullColor;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            visual.SetActive(false);
            return icon;
        }

        private static GameObject CreateQuad(string name, Transform parent, Material material)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return quad;
        }
    }
}
