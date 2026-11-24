using Guaxinim.Animation;
using UnityEditor;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Cria os <see cref="MotionData"/> dos personagens (plano de próximos passos, Etapa 5) em Data/Animation, com o
    /// jeito de cada um: o guaxinim saltitante, o segurança de passo pesado e o faxineiro arrastado. Um asset que já
    /// existe não é sobrescrito (mantém os números ajustados).
    /// </summary>
    public static class MotionDataFactory
    {
        public const string RaccoonPath = "Assets/_Project/Data/Animation/MotionData_Raccoon.asset";
        public const string GuardPath = "Assets/_Project/Data/Animation/MotionData_Guard.asset";
        public const string JanitorPath = "Assets/_Project/Data/Animation/MotionData_Janitor.asset";

        private const string Folder = "Assets/_Project/Data/Animation";

        public static MotionData LoadOrCreateRaccoon() => LoadOrCreate(RaccoonPath, new MotionValues
        {
            StepLength = 0.4f, BounceHeight = 0.05f, SwayDegrees = 7f, FullStepSpeed = 1f,
            LeanDegrees = 14f, LeanReferenceSpeed = 6f, BreathAmplitude = 0.025f, BreathPeriodSeconds = 2.2f,
            SquashAmount = 0.15f, HopHeight = 0.2f,
        });

        public static MotionData LoadOrCreateGuard() => LoadOrCreate(GuardPath, new MotionValues
        {
            StepLength = 0.75f, BounceHeight = 0.04f, SwayDegrees = 3f, FullStepSpeed = 1f,
            LeanDegrees = 15f, LeanReferenceSpeed = 5.5f, BreathAmplitude = 0.015f, BreathPeriodSeconds = 3f,
            SquashAmount = 0.06f, HopHeight = 0.3f,
        });

        public static MotionData LoadOrCreateJanitor() => LoadOrCreate(JanitorPath, new MotionValues
        {
            StepLength = 0.45f, BounceHeight = 0.025f, SwayDegrees = 9f, FullStepSpeed = 0.6f,
            LeanDegrees = 8f, LeanReferenceSpeed = 2.75f, BreathAmplitude = 0.02f, BreathPeriodSeconds = 3.2f,
            SquashAmount = 0.06f, HopHeight = 0.25f,
        });

        private static MotionData LoadOrCreate(string path, MotionValues values)
        {
            var existing = AssetDatabase.LoadAssetAtPath<MotionData>(path);
            if (existing != null)
            {
                return existing;
            }

            EditorAssets.EnsureFolder(Folder);
            MotionData data = EditorAssets.LoadOrCreate<MotionData>(path);
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_stepLength").floatValue = values.StepLength;
            serialized.FindProperty("_bounceHeight").floatValue = values.BounceHeight;
            serialized.FindProperty("_swayDegrees").floatValue = values.SwayDegrees;
            serialized.FindProperty("_fullStepSpeed").floatValue = values.FullStepSpeed;
            serialized.FindProperty("_leanDegrees").floatValue = values.LeanDegrees;
            serialized.FindProperty("_leanReferenceSpeed").floatValue = values.LeanReferenceSpeed;
            serialized.FindProperty("_breathAmplitude").floatValue = values.BreathAmplitude;
            serialized.FindProperty("_breathPeriodSeconds").floatValue = values.BreathPeriodSeconds;
            serialized.FindProperty("_squashAmount").floatValue = values.SquashAmount;
            serialized.FindProperty("_hopHeight").floatValue = values.HopHeight;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            return data;
        }

        /// <summary>Valores iniciais de um personagem (o resto fica no padrão do MotionData).</summary>
        private struct MotionValues
        {
            public float StepLength;
            public float BounceHeight;
            public float SwayDegrees;
            public float FullStepSpeed;
            public float LeanDegrees;
            public float LeanReferenceSpeed;
            public float BreathAmplitude;
            public float BreathPeriodSeconds;
            public float SquashAmount;
            public float HopHeight;
        }
    }
}
