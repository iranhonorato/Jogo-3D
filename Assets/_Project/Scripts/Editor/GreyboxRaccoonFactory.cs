using Guaxinim.Core;
using Guaxinim.Noise;
using Guaxinim.Player;
using Guaxinim.Shared;
using Guaxinim.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera o prefab do guaxinim (GDD §4): o modelo do Blender com um CharacterController de 0,6 m na layer
    /// Player, os componentes do jogador (input, movimento, som, inventário, interação, carga e frágeis) e os dados.
    /// Rodar de novo atualiza o prefab e mantém os ScriptableObjects existentes (e os números ajustados neles).
    /// </summary>
    public static class GreyboxRaccoonFactory
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/Player/Raccoon.prefab";
        private const string DataPath = "Assets/_Project/Data/Player/PlayerData.asset";
        private const string ModelPath = "Assets/_Project/Art/Models/Characters/Char_Raccoon.fbx";

        private const float Height = 0.6f;
        private const float Radius = 0.2f;
        private const float StepOffset = 0.15f;
        private const float SkinWidth = 0.02f;
        private const float SlopeLimitDegrees = 45f;
        private const string PuddlePrefabPath = "Assets/_Project/Prefabs/Items/Puddle.prefab";
        private const string PuddleMaterialPath = "Assets/_Project/Art/Materials/Greybox/M_Puddle.mat";
        private const float ShakeIconHeight = 0.95f;
        private const int VignetteSortingOrder = 5; // abaixo do aviso de pego (ordem 10)

        private static readonly Vector3 PuddleScale = new Vector3(0.9f, 0.003f, 0.7f);
        private static readonly Color PuddleColor = new Color(0.95f, 0.6f, 0.2f, 0.6f);
        private static readonly Color ShakeLowColor = new Color(1f, 1f, 1f);
        private static readonly Color ShakeFullColor = new Color(0.95f, 0.15f, 0.1f);

        [MenuItem("Tools/Guaxinim/Greybox/Prefab do Guaxinim")]
        internal static void CreateRaccoonPrefab()
        {
            var root = new GameObject("Raccoon");
            ConfigureCharacterController(root.AddComponent<CharacterController>());
            var input = root.AddComponent<InputReader>();
            var data = EditorAssets.LoadOrCreate<PlayerData>(DataPath);
            // PlayerState e Inventory antes do controller: o RequireComponent dele não cria uma segunda cópia.
            root.AddComponent<PlayerState>();
            var inventory = root.AddComponent<Inventory>();
            EditorAssets.SetReference(inventory, "_data", data);
            var controller = root.AddComponent<RaccoonController>();
            EditorAssets.SetReference(controller, "_data", data);
            EditorAssets.SetReference(controller, "_input", input);
            AddNoiseMaker(root, input, controller, inventory);
            AddInteractor(root, data, input);
            AddDropper(root, input);
            AddBulkyLoad(root, data);
            AddFragileShake(root, data);
            GameObject model = AddModel(root.transform);
            AddHiding(root, data, model);
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

        private static void AddNoiseMaker(GameObject root, InputReader input, RaccoonController controller,
            Inventory inventory)
        {
            var noiseMaker = root.AddComponent<RaccoonNoiseMaker>();
            EditorAssets.SetReference(noiseMaker, "_noiseData",
                EditorAssets.LoadOrCreate<NoiseData>(NoiseSystemFactory.NoiseDataPath));
            EditorAssets.SetReference(noiseMaker, "_input", input);
            EditorAssets.SetReference(noiseMaker, "_controller", controller);
            EditorAssets.SetReference(noiseMaker, "_inventory", inventory);
        }

        private static void AddInteractor(GameObject root, PlayerData data, InputReader input)
        {
            var interactor = root.AddComponent<RaccoonInteractor>();
            EditorAssets.SetReference(interactor, "_data", data);
            EditorAssets.SetReference(interactor, "_input", input);
            var serialized = new SerializedObject(interactor);
            serialized.FindProperty("_interactableMask").intValue = LayerMask.GetMask(Layers.Interactable);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddDropper(GameObject root, InputReader input)
        {
            var dropper = root.AddComponent<RaccoonDropper>();
            EditorAssets.SetReference(dropper, "_input", input);
            EditorAssets.SetReference(dropper, "_noiseData",
                EditorAssets.LoadOrCreate<NoiseData>(NoiseSystemFactory.NoiseDataPath));
            var serialized = new SerializedObject(dropper);
            serialized.FindProperty("_obstacleMask").intValue = LayerMask.GetMask(Layers.Obstacle);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Carga volumosa (GDD §7.4) e a vinheta na tela, num canvas próprio dentro do prefab.</summary>
        private static void AddBulkyLoad(GameObject root, PlayerData data)
        {
            var bulkyLoad = root.AddComponent<RaccoonBulkyLoad>();
            EditorAssets.SetReference(bulkyLoad, "_data", data);

            var canvasObject = new GameObject("BulkyVignette");
            canvasObject.transform.SetParent(root.transform, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = VignetteSortingOrder;

            var imageObject = new GameObject("Image", typeof(RectTransform));
            imageObject.transform.SetParent(canvasObject.transform, false);
            var rect = (RectTransform)imageObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = imageObject.AddComponent<RawImage>();
            image.raycastTarget = false;

            var vignette = canvasObject.AddComponent<BulkyVignetteUI>();
            EditorAssets.SetReference(vignette, "_bulkyLoad", bulkyLoad);
            EditorAssets.SetReference(vignette, "_image", image);
        }

        /// <summary>Barra de chacoalhar dos frágeis (GDD §7.5), com a barra sobre a cabeça e a poça.</summary>
        private static void AddFragileShake(GameObject root, PlayerData data)
        {
            var shake = root.AddComponent<FragileShake>();
            EditorAssets.SetReference(shake, "_data", data);
            EditorAssets.SetReference(shake, "_noiseData",
                EditorAssets.LoadOrCreate<NoiseData>(NoiseSystemFactory.NoiseDataPath));
            EditorAssets.SetReference(shake, "_puddlePrefab", CreatePuddlePrefab());
            WorldMeterIconBuilder.Build("ShakeIcon", root.transform, ShakeIconHeight, shake, ShakeLowColor,
                ShakeFullColor);
        }

        private static GameObject CreatePuddlePrefab()
        {
            GameObject puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            puddle.name = "Puddle";
            Object.DestroyImmediate(puddle.GetComponent<Collider>());
            puddle.transform.localScale = PuddleScale;
            var renderer = puddle.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = EditorAssets.LoadOrCreateTransparentUnlitMaterial(PuddleMaterialPath, PuddleColor);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(puddle, PuddlePrefabPath);
            Object.DestroyImmediate(puddle);
            return prefab;
        }

        /// <summary>Visual: o modelo do Blender (0,6 m, frente em +Z), sem collider; a colisão é do CharacterController.</summary>
        private static GameObject AddModel(Transform root)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                Debug.LogError($"Model not found: {ModelPath}");
                return null;
            }

            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model, root);
            visual.name = "Model";
            return visual;
        }

        /// <summary>Entrar e sair da lixeira (GDD §9); o modelo some lá dentro.</summary>
        private static void AddHiding(GameObject root, PlayerData data, GameObject model)
        {
            var hiding = root.AddComponent<RaccoonHiding>();
            EditorAssets.SetReference(hiding, "_data", data);
            EditorAssets.SetReference(hiding, "_model", model);
        }
    }
}
