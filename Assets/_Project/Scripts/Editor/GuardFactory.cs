using Guaxinim.Guards;
using Guaxinim.Noise;
using Guaxinim.Shared;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera os prefabs dos dois tipos de inimigo (GDD §6.2) em Prefabs/Guards/: o segurança (rápido) e o faxineiro
    /// (metade da velocidade, com o modelo do Seu Zé). Cada um tem NavMeshAgent, collider, GuardAI com o seu
    /// GuardData, visão (VisionCone e o cone no chão), a lanterna (luz spot no Socket_Flashlight) e o modelo do
    /// Blender, na layer Guard. Os waypoints são ligados na cena.
    /// </summary>
    public static class GuardFactory
    {
        public const string GuardPrefabPath = "Assets/_Project/Prefabs/Guards/Guard.prefab";
        public const string JanitorPrefabPath = "Assets/_Project/Prefabs/Guards/Janitor.prefab";

        /// <summary>O faxineiro anda, desconfia, persegue e procura a esta fração da velocidade do segurança.</summary>
        public const float JanitorSpeedFactor = 0.5f;

        private const string GuardModelPath = "Assets/_Project/Art/Models/Characters/Char_Guard.fbx";
        private const string SeuZeModelPath = "Assets/_Project/Art/Models/Characters/Char_SeuZe.fbx";
        public const string GuardDataPath = "Assets/_Project/Data/Guards/GuardData_Guard.asset";
        public const string JanitorDataPath = "Assets/_Project/Data/Guards/GuardData_Janitor.asset";

        // Caminhos antigos: renomeados com MoveAsset (mantém o GUID e as referências das cenas).
        private const string LegacyGuardDataPath = "Assets/_Project/Data/Guards/GuardData.asset";
        private const string LegacySeuZePrefabPath = "Assets/_Project/Prefabs/Guards/SeuZe.prefab";
        private const string VisionDataPath = "Assets/_Project/Data/Guards/VisionData_Guard.asset";
        private const string DetectionDataPath = "Assets/_Project/Data/Guards/DetectionData.asset";
        private const string ConeMaterialPath = "Assets/_Project/Art/Materials/Greybox/M_VisionCone_Guard.mat";
        private const string FlashlightSocket = "Socket_Flashlight";

        private const float Height = 1.8f;
        private const float Radius = 0.35f;
        private const float AngularSpeedDegrees = 360f;
        private const float Acceleration = 12f;
        private const float ConeFloorOffset = 0.02f;
        private const float FlashlightTiltDegrees = 20f;
        private const float FlashlightIntensity = 6f;
        private const float IconHeight = 2.25f;

        private static readonly Color ConeColor = new Color(1f, 0.85f, 0.45f, 0.22f); // GDD §15.2: luz quente
        private static readonly Color FlashlightColor = new Color(1f, 0.86f, 0.6f);
        private static readonly Color DetectionLowColor = new Color(1f, 0.85f, 0.2f);
        private static readonly Color DetectionFullColor = new Color(0.95f, 0.15f, 0.1f);

        /// <summary>Gera o segurança e o faxineiro (na Fase 1, o faxineiro é o Seu Zé, dono do mercadinho).</summary>
        [MenuItem("Tools/Guaxinim/Greybox/Prefabs dos Seguranças")]
        internal static void CreateGuardPrefabs()
        {
            RenameLegacyAssets();
            GuardData guardData = EditorAssets.LoadOrCreate<GuardData>(GuardDataPath);
            GuardData janitorData = LoadOrCreateJanitorData(guardData);
            CreateGuardPrefab("Guard", GuardModelPath, GuardPrefabPath, guardData);
            CreateGuardPrefab("Janitor", SeuZeModelPath, JanitorPrefabPath, janitorData);
        }

        private static void RenameLegacyAssets()
        {
            MoveIfOnlyOldExists(LegacyGuardDataPath, GuardDataPath);
            MoveIfOnlyOldExists(LegacySeuZePrefabPath, JanitorPrefabPath);
        }

        private static void MoveIfOnlyOldExists(string oldPath, string newPath)
        {
            bool hasOld = AssetDatabase.LoadMainAssetAtPath(oldPath) != null;
            bool hasNew = AssetDatabase.LoadMainAssetAtPath(newPath) != null;
            if (!hasOld || hasNew)
            {
                return;
            }

            string error = AssetDatabase.MoveAsset(oldPath, newPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"Could not rename {oldPath} to {newPath}: {error}");
            }
        }

        /// <summary>
        /// Faxineiro: as quatro velocidades do segurança vezes <see cref="JanitorSpeedFactor"/>, escritas uma vez
        /// ao criar o asset. Depois, os números se ajustam no próprio asset.
        /// </summary>
        private static GuardData LoadOrCreateJanitorData(GuardData guardData)
        {
            var janitorData = AssetDatabase.LoadAssetAtPath<GuardData>(JanitorDataPath);
            if (janitorData != null)
            {
                return janitorData;
            }

            janitorData = Object.Instantiate(guardData);
            var serialized = new SerializedObject(janitorData);
            ScaleSpeed(serialized, "_patrolSpeed");
            ScaleSpeed(serialized, "_suspiciousSpeed");
            ScaleSpeed(serialized, "_chaseSpeed");
            ScaleSpeed(serialized, "_searchSpeed");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(janitorData, JanitorDataPath);
            return janitorData;
        }

        private static void ScaleSpeed(SerializedObject data, string propertyName)
        {
            SerializedProperty speed = data.FindProperty(propertyName);
            speed.floatValue *= JanitorSpeedFactor;
        }

        private static void CreateGuardPrefab(string name, string modelPath, string prefabPath, GuardData data)
        {
            var root = new GameObject(name);
            ConfigureAgent(root.AddComponent<NavMeshAgent>());
            AddBodyCollider(root);

            var guard = root.AddComponent<GuardAI>();
            EditorAssets.SetReference(guard, "_data", data);
            VisionData visionData = EditorAssets.LoadOrCreate<VisionData>(VisionDataPath);
            VisionCone visionCone = AddVision(root, visionData);
            AddConeMesh(root.transform, visionCone);
            DetectionMeter meter = AddDetection(root, visionCone);
            EditorAssets.SetReference(guard, "_vision", visionCone);
            EditorAssets.SetReference(guard, "_meter", meter);
            EditorAssets.SetReference(guard, "_hearing", root.AddComponent<NoiseListener>());
            GameObject model = AddModel(root.transform, modelPath);
            AddFlashlight(root.transform, model, visionData);
            EditorAssets.SetLayerRecursively(root, LayerMask.NameToLayer(Layers.Guard));

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
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

        private static DetectionMeter AddDetection(GameObject root, VisionCone visionCone)
        {
            var meter = root.AddComponent<DetectionMeter>();
            EditorAssets.SetReference(meter, "_visionCone", visionCone);
            EditorAssets.SetReference(meter, "_data", EditorAssets.LoadOrCreate<DetectionData>(DetectionDataPath));

            WorldMeterIconBuilder.Build("DetectionIcon", root.transform, IconHeight, meter, DetectionLowColor,
                DetectionFullColor);
            return meter;
        }

        private static GameObject AddModel(Transform root, string modelPath)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
            {
                Debug.LogError($"Model not found: {modelPath}");
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
