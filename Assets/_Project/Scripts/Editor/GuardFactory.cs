using Guaxinim.Guards;
using Guaxinim.Shared;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera o prefab do segurança (passos 1.5 e 1.6, GDD §6) em Prefabs/Guards/: NavMeshAgent, collider,
    /// GuardAI, visão (VisionCone e o cone desenhado no chão), a lanterna (luz spot no Socket_Flashlight)
    /// e o modelo do Blender, na layer Guard. Os waypoints são ligados na cena.
    /// </summary>
    public static class GuardFactory
    {
        public const string GuardPrefabPath = "Assets/_Project/Prefabs/Guards/Guard.prefab";

        private const string DataPath = "Assets/_Project/Data/Guards/GuardData.asset";
        private const string VisionDataPath = "Assets/_Project/Data/Guards/VisionData_Guard.asset";
        private const string ConeMaterialPath = "Assets/_Project/Art/Materials/Greybox/M_VisionCone_Guard.mat";
        private const string ModelPath = "Assets/_Project/Art/Models/Characters/Char_Guard.fbx";
        private const string FlashlightSocket = "Socket_Flashlight";

        private const float Height = 1.8f;
        private const float Radius = 0.35f;
        private const float AngularSpeedDegrees = 360f;
        private const float Acceleration = 12f;
        private const float ConeFloorOffset = 0.02f;
        private const float FlashlightTiltDegrees = 20f;
        private const float FlashlightIntensity = 6f;

        private static readonly Color ConeColor = new Color(1f, 0.85f, 0.45f, 0.22f); // GDD §15.2: luz quente
        private static readonly Color FlashlightColor = new Color(1f, 0.86f, 0.6f);

        [MenuItem("Tools/Guaxinim/Greybox/Prefab do Segurança")]
        internal static void CreateGuardPrefab()
        {
            var root = new GameObject("Guard");
            ConfigureAgent(root.AddComponent<NavMeshAgent>());
            AddBodyCollider(root);

            var guard = root.AddComponent<GuardAI>();
            EditorAssets.SetReference(guard, "_data", EditorAssets.LoadOrCreate<GuardData>(DataPath));
            VisionData visionData = EditorAssets.LoadOrCreate<VisionData>(VisionDataPath);
            VisionCone visionCone = AddVision(root, visionData);
            AddConeMesh(root.transform, visionCone);
            GameObject model = AddModel(root.transform);
            AddFlashlight(root.transform, model, visionData);
            EditorAssets.SetLayerRecursively(root, LayerMask.NameToLayer(Layers.Guard));

            PrefabUtility.SaveAsPrefabAsset(root, GuardPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void ConfigureAgent(NavMeshAgent agent)
        {
            agent.height = Height;
            agent.radius = Radius;
            agent.angularSpeed = AngularSpeedDegrees;
            agent.acceleration = Acceleration;
            agent.stoppingDistance = 0f;
            agent.autoBraking = true;
        }

        private static void AddBodyCollider(GameObject root)
        {
            var body = root.AddComponent<CapsuleCollider>();
            body.height = Height;
            body.radius = Radius;
            body.center = Vector3.up * Height / 2f;
        }

        private static VisionCone AddVision(GameObject root, VisionData visionData)
        {
            var visionCone = root.AddComponent<VisionCone>();
            EditorAssets.SetReference(visionCone, "_data", visionData);
            var serialized = new SerializedObject(visionCone);
            serialized.FindProperty("_obstacleMask").intValue = LayerMask.GetMask(Layers.Obstacle);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return visionCone;
        }

        private static void AddConeMesh(Transform root, VisionCone visionCone)
        {
            var coneObject = new GameObject("VisionConeMesh");
            coneObject.transform.SetParent(root, false);
            coneObject.transform.localPosition = Vector3.up * ConeFloorOffset;
            coneObject.AddComponent<MeshFilter>();
            var renderer = coneObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = EditorAssets.LoadOrCreateTransparentUnlitMaterial(ConeMaterialPath, ConeColor);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var coneMesh = coneObject.AddComponent<VisionConeMesh>();
            EditorAssets.SetReference(coneMesh, "_visionCone", visionCone);
        }

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

        /// <summary>GDD §14.6: as lanternas são as únicas luzes em tempo real, sem sombras (WebGL).</summary>
        private static void AddFlashlight(Transform root, GameObject model, VisionData visionData)
        {
            Transform socket = model != null ? FindDeep(model.transform, FlashlightSocket) : null;
            var lightObject = new GameObject("Flashlight");
            lightObject.transform.SetParent(socket != null ? socket : root, false);
            if (socket == null)
            {
                lightObject.transform.localPosition = Vector3.up * visionData.EyeHeight;
            }

            lightObject.transform.rotation = root.rotation * Quaternion.Euler(FlashlightTiltDegrees, 0f, 0f);
            var spot = lightObject.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = visionData.RangeMeters;
            spot.spotAngle = visionData.AngleDegrees;
            spot.color = FlashlightColor;
            spot.intensity = FlashlightIntensity;
            spot.shadows = LightShadows.None;
        }

        private static Transform FindDeep(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }

                Transform found = FindDeep(child, childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
