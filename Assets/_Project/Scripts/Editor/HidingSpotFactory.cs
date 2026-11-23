using Guaxinim.Hazards;
using Guaxinim.Shared;
using UnityEditor;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera o prefab greybox da lixeira (passo 2.8, GDD §9) em Prefabs/Hazards/: corpo e tampa em cilindro,
    /// collider sólido na layer Interactable (o E encontra; seguranças desviam pelo NavMesh) e o ponto de
    /// saída à frente (+Z).
    /// </summary>
    public static class HidingSpotFactory
    {
        public const string BinPrefabPath = "Assets/_Project/Prefabs/Hazards/HidingSpot_Bin.prefab";

        private const string BodyMaterialPath = "Assets/_Project/Art/Materials/Greybox/M_Bin.mat";
        private const string LidMaterialPath = "Assets/_Project/Art/Materials/Greybox/M_BinLid.mat";
        private const float Height = 0.9f;
        private const float Radius = 0.35f;
        private const float LidThickness = 0.05f;
        private const float LidOverhang = 0.04f;
        private const float ExitDistance = 0.75f;
        private const float UnityCylinderHeight = 2f;

        private static readonly Color BodyColor = new Color(0.2f, 0.42f, 0.3f);
        private static readonly Color LidColor = new Color(0.12f, 0.26f, 0.18f);

        [MenuItem("Tools/Guaxinim/Greybox/Lixeira")]
        internal static void CreateBinPrefab()
        {
            var root = new GameObject("HidingSpot_Bin");
            var body = root.AddComponent<CapsuleCollider>();
            body.radius = Radius;
            body.height = Height;
            body.center = Vector3.up * Height / 2f;

            AddCylinder(root.transform, "Body", Radius, Height, 0f,
                EditorAssets.LoadOrCreateLitMaterial(BodyMaterialPath, BodyColor));
            AddCylinder(root.transform, "Lid", Radius + LidOverhang, LidThickness, Height,
                EditorAssets.LoadOrCreateLitMaterial(LidMaterialPath, LidColor));

            var exitPoint = new GameObject("ExitPoint").transform;
            exitPoint.SetParent(root.transform, false);
            exitPoint.localPosition = Vector3.forward * ExitDistance;
            var spot = root.AddComponent<HidingSpot>();
            EditorAssets.SetReference(spot, "_exitPoint", exitPoint);

            EditorAssets.SetLayerRecursively(root, LayerMask.NameToLayer(Layers.Interactable));
            PrefabUtility.SaveAsPrefabAsset(root, BinPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void AddCylinder(Transform parent, string name, float radius, float height, float baseY,
            Material material)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            Object.DestroyImmediate(cylinder.GetComponent<Collider>());
            cylinder.transform.localScale = new Vector3(radius * 2f, height / UnityCylinderHeight, radius * 2f);
            cylinder.transform.localPosition = Vector3.up * (baseY + height / 2f);
            cylinder.GetComponent<MeshRenderer>().sharedMaterial = material;
        }
    }
}
