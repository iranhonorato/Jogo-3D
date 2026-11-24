using Guaxinim.Audio;
using UnityEditor;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Cria o Data/Audio/SoundData com os efeitos de Audio/SFX (gerados por Tools/Audio/make_sfx.py) e põe a fonte de
    /// som 2D nos prefabs. Um SoundData que já existe não é sobrescrito (mantém os volumes ajustados).
    /// </summary>
    public static class SoundDataFactory
    {
        private const string DataPath = "Assets/_Project/Data/Audio/SoundData.asset";
        private const string SfxFolder = "Assets/_Project/Audio/SFX";

        public static SoundData LoadOrCreate()
        {
            var data = AssetDatabase.LoadAssetAtPath<SoundData>(DataPath);
            if (data != null)
            {
                return data;
            }

            EditorAssets.EnsureFolder("Assets/_Project/Data/Audio");
            data = ScriptableObject.CreateInstance<SoundData>();
            var serialized = new SerializedObject(data);
            SetCue(serialized, "_runStep", "Sfx_RunStep", 0.35f, 0.15f);
            SetCue(serialized, "_whistle", "Sfx_Whistle", 0.7f, 0.05f);
            SetCue(serialized, "_pickup", "Sfx_Pickup", 0.6f, 0.1f);
            SetCue(serialized, "_drop", "Sfx_Drop", 0.7f, 0.1f);
            SetCue(serialized, "_glassBreak", "Sfx_GlassBreak", 0.9f, 0.05f);
            SetCue(serialized, "_binUse", "Sfx_BinClank", 0.6f, 0.1f);
            SetCue(serialized, "_deliver", "Sfx_Deliver", 0.7f, 0f);
            SetCue(serialized, "_spotted", "Sfx_Spotted", 0.6f, 0f);
            SetCue(serialized, "_caught", "Sfx_Caught", 0.8f, 0f);
            SetCue(serialized, "_win", "Sfx_Win", 0.8f, 0f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(data, DataPath);
            return data;
        }

        /// <summary>Fonte de som 2D (a câmera fica longe, então posição no espaço só atrapalharia o volume).</summary>
        public static AudioSource AddSource(GameObject target)
        {
            var source = target.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }

        private static void SetCue(SerializedObject data, string propertyName, string clipName, float volume,
            float pitchJitter)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxFolder}/{clipName}.wav");
            if (clip == null)
            {
                Debug.LogWarning($"Sound effect not found: {SfxFolder}/{clipName}.wav (run Tools/Audio/make_sfx.py).");
            }

            SerializedProperty cue = data.FindProperty(propertyName);
            cue.FindPropertyRelative("_clip").objectReferenceValue = clip;
            cue.FindPropertyRelative("_volume").floatValue = volume;
            cue.FindPropertyRelative("_pitchJitter").floatValue = pitchJitter;
        }
    }
}
