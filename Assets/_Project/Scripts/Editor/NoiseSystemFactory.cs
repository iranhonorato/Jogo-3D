using Guaxinim.Noise;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera os assets do sistema de som (passo 1.3, GDD §5): o NoiseData e o prefab do anel no chão.
    /// O anel é usado pelo NoiseRingSpawner do LevelSystems (ver LevelSystemsFactory).
    /// </summary>
    public static class NoiseSystemFactory
    {
        public const string NoiseDataPath = "Assets/_Project/Data/Noise/NoiseData.asset";

        private const string RingMaterialPath = "Assets/_Project/Art/Materials/Greybox/M_NoiseRing.mat";
        private const string RingPrefabPath = "Assets/_Project/Prefabs/Systems/NoiseRing.prefab";
        private const string SystemsFolder = "Assets/_Project/Prefabs/Systems";
        private const float RingLineWidth = 0.08f;

        [MenuItem("Tools/Guaxinim/Greybox/Sistema de Som")]
        internal static NoiseRingVFX CreateNoiseAssets()
        {
            EditorAssets.EnsureFolder(SystemsFolder);
            EditorAssets.LoadOrCreate<NoiseData>(NoiseDataPath);
            NoiseRingVFX ringPrefab = CreateRingPrefab();
            Selection.activeObject = ringPrefab.gameObject;
            return ringPrefab;
        }

        private static NoiseRingVFX CreateRingPrefab()
        {
            var ring = new GameObject("NoiseRing");
            var line = ring.AddComponent<LineRenderer>();
            line.sharedMaterial = EditorAssets.LoadOrCreateTransparentUnlitMaterial(RingMaterialPath, Color.white);
            line.widthMultiplier = RingLineWidth;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.alignment = LineAlignment.View;
            ring.AddComponent<NoiseRingVFX>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(ring, RingPrefabPath);
            Object.DestroyImmediate(ring);
            return prefab.GetComponent<NoiseRingVFX>();
        }
    }
}
