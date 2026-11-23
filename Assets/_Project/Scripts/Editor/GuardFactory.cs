using Guaxinim.Guards;
using Guaxinim.Shared;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera o prefab do segurança (passo 1.5, GDD §6) em Prefabs/Guards/: NavMeshAgent, collider,
    /// GuardAI com o GuardData e o modelo do Blender, na layer Guard. Os waypoints são ligados na cena.
    /// </summary>
    public static class GuardFactory
    {
        public const string GuardPrefabPath = "Assets/_Project/Prefabs/Guards/Guard.prefab";

        private const string DataPath = "Assets/_Project/Data/Guards/GuardData.asset";
        private const string ModelPath = "Assets/_Project/Art/Models/Characters/Char_Guard.fbx";
        private const float Height = 1.8f;
        private const float Radius = 0.35f;
        private const float AngularSpeedDegrees = 360f;
        private const float Acceleration = 12f;

        [MenuItem("Tools/Guaxinim/Greybox/Prefab do Segurança")]
        internal static void CreateGuardPrefab()
        {
            var root = new GameObject("Guard");
            ConfigureAgent(root.AddComponent<NavMeshAgent>());
            var body = root.AddComponent<CapsuleCollider>();
            body.height = Height;
            body.radius = Radius;
            body.center = Vector3.up * Height / 2f;

            var guard = root.AddComponent<GuardAI>();
            EditorAssets.SetReference(guard, "_data", EditorAssets.LoadOrCreate<GuardData>(DataPath));
            AddModel(root.transform);
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

        private static void AddModel(Transform root)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                Debug.LogError($"Model not found: {ModelPath}");
                return;
            }

            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model, root);
            visual.name = "Model";
        }
    }
}
