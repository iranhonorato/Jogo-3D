using Guaxinim.Core;
using Guaxinim.Noise;
using Guaxinim.Player;
using Guaxinim.Shared;
using UnityEditor;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera o prefab greybox do guaxinim (GDD §4): cápsula de 0,6 m na layer Player, com
    /// CharacterController, InputReader, RaccoonController e RaccoonNoiseMaker, mais os dados e o material.
    /// Rodar de novo atualiza o prefab e mantém os ScriptableObjects existentes (e os números ajustados neles).
    /// </summary>
    public static class GreyboxRaccoonFactory
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/Player/Raccoon.prefab";
        private const string DataPath = "Assets/_Project/Data/Player/PlayerData.asset";
        private const string BodyMaterialPath = "Assets/_Project/Art/Materials/Greybox/M_Greybox_Raccoon.mat";
        private const string NoseMaterialPath = "Assets/_Project/Art/Materials/Greybox/M_Greybox_RaccoonNose.mat";

        private const float Height = 0.6f;
        private const float Radius = 0.2f;
        private const float StepOffset = 0.15f;
        private const float SkinWidth = 0.02f;
        private const float SlopeLimitDegrees = 45f;
        private const float UnityCapsuleHeight = 2f;
        private const float UnityCapsuleDiameter = 1f;
        private const float NoseHeightFraction = 0.65f;

        private static readonly Vector3 NoseSize = new Vector3(0.1f, 0.08f, 0.12f);
        private static readonly Color RaccoonColor = new Color(0.62f, 0.52f, 0.42f); // GDD §15.2: tons quentes
        private static readonly Color NoseColor = new Color(0.15f, 0.13f, 0.12f);

        [MenuItem("Tools/Guaxinim/Greybox/Prefab do Guaxinim")]
        internal static void CreateRaccoonPrefab()
        {
            var root = new GameObject("Raccoon");
            ConfigureCharacterController(root.AddComponent<CharacterController>());
            var input = root.AddComponent<InputReader>();
            var controller = root.AddComponent<RaccoonController>();
            EditorAssets.SetReference(controller, "_data", EditorAssets.LoadOrCreate<PlayerData>(DataPath));
            EditorAssets.SetReference(controller, "_input", input);
            AddNoiseMaker(root, input, controller);
            AddBody(root.transform);
            EditorAssets.SetLayerRecursively(root, LayerMask.NameToLayer(Layers.Player));

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        private static void ConfigureCharacterController(CharacterController characterController)
        {
            characterController.height = Height;
            characterController.radius = Radius;
            characterController.center = new Vector3(0f, Height / 2f, 0f);
            characterController.stepOffset = StepOffset;
            characterController.skinWidth = SkinWidth;
            characterController.slopeLimit = SlopeLimitDegrees;
            characterController.minMoveDistance = 0f;
        }

        private static void AddNoiseMaker(GameObject root, InputReader input, RaccoonController controller)
        {
            var noiseMaker = root.AddComponent<RaccoonNoiseMaker>();
            EditorAssets.SetReference(noiseMaker, "_noiseData",
                EditorAssets.LoadOrCreate<NoiseData>(NoiseSystemFactory.NoiseDataPath));
            EditorAssets.SetReference(noiseMaker, "_input", input);
            EditorAssets.SetReference(noiseMaker, "_controller", controller);
        }

        private static void AddBody(Transform root)
        {
            Material bodyMaterial = EditorAssets.LoadOrCreateLitMaterial(BodyMaterialPath, RaccoonColor);
            GameObject body = CreateVisual(PrimitiveType.Capsule, "Body", root, bodyMaterial);
            float diameterScale = Radius * 2f / UnityCapsuleDiameter;
            body.transform.localScale = new Vector3(diameterScale, Height / UnityCapsuleHeight, diameterScale);
            body.transform.localPosition = new Vector3(0f, Height / 2f, 0f);

            // O "nariz" mostra para onde o guaxinim está virado (+Z).
            Material noseMaterial = EditorAssets.LoadOrCreateLitMaterial(NoseMaterialPath, NoseColor);
            GameObject nose = CreateVisual(PrimitiveType.Cube, "Nose", root, noseMaterial);
            nose.transform.localScale = NoseSize;
            nose.transform.localPosition = new Vector3(0f, Height * NoseHeightFraction, Radius);
        }

        private static GameObject CreateVisual(PrimitiveType type, string name, Transform parent, Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(type);
            visual.name = name;
            visual.transform.SetParent(parent, false);
            Object.DestroyImmediate(visual.GetComponent<Collider>()); // a colisão é do CharacterController
            visual.GetComponent<MeshRenderer>().sharedMaterial = material;
            return visual;
        }
    }
}
