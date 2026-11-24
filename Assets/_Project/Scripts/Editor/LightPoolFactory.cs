using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Poça de luz no chão (GDD §15.1, pontos de luz quente): um quadrado sem luz, aditivo, com um degradê redondo.
    /// Dá a cara de luminária acesa sem ser uma luz em tempo real (GDD §14.6: no máximo 5 por cena no WebGL).
    /// </summary>
    public static class LightPoolFactory
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/Environment/LightPool.prefab";

        private const string MaterialPath = "Assets/_Project/Art/Materials/M_LightPool.mat";
        private const string GradientPath = "Assets/_Project/Art/Textures/Effects/LightPool_Gradient.png";
        private const float DiameterMeters = 4.5f;
        private const float FloorOffset = 0.015f; // acima do piso e das fitas dos vãos estreitos (0,01 m)
        private const float AdditiveBlend = 2f; // valor de _Blend dos shaders de partícula da URP

        private static readonly Color WarmColor = new Color(1f, 0.78f, 0.45f, 0.22f);

        public static void CreatePrefab()
        {
            GameObject pool = GameObject.CreatePrimitive(PrimitiveType.Quad);
            pool.name = "LightPool";
            Object.DestroyImmediate(pool.GetComponent<Collider>());
            var visual = pool.transform;
            visual.localPosition = Vector3.up * FloorOffset;
            visual.localRotation = Quaternion.Euler(90f, 0f, 0f); // o quad olha para cima
            visual.localScale = Vector3.one * DiameterMeters;

            var renderer = pool.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = LoadOrCreateMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;

            var root = new GameObject("LightPool");
            pool.transform.SetParent(root.transform, false);
            pool.isStatic = true;
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }

        private static Material LoadOrCreateMaterial()
        {
            Material material = EditorAssets.LoadOrCreateTransparentUnlitMaterial(MaterialPath, WarmColor);
            material.SetFloat("_Blend", AdditiveBlend);
            material.SetFloat("_DstBlend", (float)BlendMode.One);
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(GradientPath));
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
