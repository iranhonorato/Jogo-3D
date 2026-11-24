using Guaxinim.Audio;
using UnityEditor;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera o prefab Resources/GameAudio: a música de fundo em loop (Audio/Music, de Tools/Audio/make_music.py) e o
    /// clique de navegação dos menus. O GameAudio o cria antes da primeira cena.
    /// </summary>
    public static class GameAudioFactory
    {
        private const string PrefabPath = "Assets/_Project/Resources/GameAudio.prefab";
        private const string MusicPath = "Assets/_Project/Audio/Music/Music_Sneak.wav";
        private const string SelectPath = "Assets/_Project/Audio/SFX/Sfx_UISelect.wav";
        private const float MusicVolume = 0.25f;
        private const float SelectVolume = 0.5f;
        private const float SelectPitchJitter = 0.05f;

        [MenuItem("Tools/Guaxinim/Greybox/Áudio Global")]
        internal static void CreatePrefab()
        {
            var root = new GameObject("GameAudio");
            AudioSource music = SoundDataFactory.AddSource(root);
            music.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicPath);
            music.loop = true;
            music.volume = MusicVolume;
            AudioSource ui = SoundDataFactory.AddSource(root);
            if (music.clip == null)
            {
                Debug.LogWarning($"Music not found at {MusicPath} (run Tools/Audio/make_music.py).");
            }

            var gameAudio = root.AddComponent<GameAudio>();
            var serialized = new SerializedObject(gameAudio);
            serialized.FindProperty("_music").objectReferenceValue = music;
            serialized.FindProperty("_interface").objectReferenceValue = ui;
            SerializedProperty select = serialized.FindProperty("_select");
            select.FindPropertyRelative("_clip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SelectPath);
            select.FindPropertyRelative("_volume").floatValue = SelectVolume;
            select.FindPropertyRelative("_pitchJitter").floatValue = SelectPitchJitter;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorAssets.EnsureFolder("Assets/_Project/Resources");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }
    }
}
