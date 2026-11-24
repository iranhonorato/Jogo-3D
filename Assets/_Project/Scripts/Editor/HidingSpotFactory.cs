using Guaxinim.Hazards;
using Guaxinim.Shared;
using UnityEditor;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera o prefab da lixeira (passo 2.8, GDD §9) em Prefabs/Hazards/: o modelo do Blender
    /// (Art/Models/Hazards/Hazard_Bin.fbx, de Tools/Blender/hazard_bin.py) ou, sem ele, corpo e tampa em cilindro;
    /// collider sólido na layer Interactable (o E encontra; seguranças desviam pelo NavMesh) e o ponto de
    /// saída à frente (+Z). A tampa (Lid) tem o pivô na dobradiça, e o BinLidAnimator a abre ao entrar e sair.
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
        private const string ModelPath = "Assets/_Project/Art/Models/Hazards/Hazard_Bin.fbx";
        private const string LidName = "Lid";

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

            // Visual separado do collider, para balançar; a tampa gira numa dobradiça no fundo (Etapa 5.4).
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Transform hinge = model != null ? AddModel(visual, model) : AddGreybox(visual);

            var exitPoint = new GameObject("ExitPoint").transform;
            exitPoint.SetParent(root.transform, false);
            exitPoint.localPosition = Vector3.forward * ExitDistance;
            var spot = root.AddComponent<HidingSpot>();
            EditorAssets.SetReference(spot, "_exitPoint", exitPoint);
            var lidAnimator = root.AddComponent<BinLidAnimator>();
            EditorAssets.SetReference(lidAnimator, "_lid", hinge);
            EditorAssets.SetReference(lidAnimator, "_visual", visual);
            SoundDataFactory.AddSource(root);
            EditorAssets.SetReference(root.AddComponent<Audio.BinSounds>(), "_sounds", SoundDataFactory.LoadOrCreate());

            EditorAssets.SetLayerRecursively(root, LayerMask.NameToLayer(Layers.Interactable));
            PrefabUtility.SaveAsPrefabAsset(root, BinPrefabPath);
            Object.DestroyImmediate(root);
        }

        /// <summary>O modelo do Blender; a tampa é o filho Lid, já com o pivô na dobradiça.</summary>
        private static Transform AddModel(Transform visual, GameObject model)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, visual);
            foreach (Transform child in instance.GetComponentsInChildren<Transform>())
            {
                if (child.name == LidName)
                {
                    return child;
                }
            }

            Debug.LogError($"Bin model has no child named {LidName}: the lid will not open.");
            return instance.transform;
        }

        private static Transform AddGreybox(Transform visual)
        {
            AddCylinder(visual, "Body", Radius, Height, 0f,
                EditorAssets.LoadOrCreateLitMaterial(BodyMaterialPath, BodyColor));
            float lidRadius = Radius + LidOverhang;
            var hinge = new GameObject(LidName).transform;
            hinge.SetParent(visual, false);
            hinge.localPosition = new Vector3(0f, Height, -lidRadius);
            AddCylinder(hinge, "LidMesh", lidRadius, LidThickness, 0f,
                EditorAssets.LoadOrCreateLitMaterial(LidMaterialPath, LidColor));
            hinge.GetChild(0).localPosition += Vector3.forward * lidRadius;
            return hinge;
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
