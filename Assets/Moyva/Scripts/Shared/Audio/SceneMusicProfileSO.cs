using System;
using Kruty1918.Audio;
using UnityEngine;
using UnityEngine.Audio;

using Kruty1918.JsonConfig;
namespace Kruty1918.Moyva.Audio.Runtime
{


    // ─── ScriptableObject ────────────────────────────────────────────────────────

    /// <summary>
    /// ScriptableObject з профілем фонової музики для однієї або кількох сцен.
    /// Завантажується MusicService при зміні сцени.
    /// </summary>
[System.Serializable]
public sealed class SceneMusicProfileSO : JsonConfigObject, IMusicSceneProfile
    {
        [Header("Target Scenes")]
        [Tooltip("Список сцен, для яких діє цей профіль. Порожній список = Global Profile (застосовується до будь-якої сцени без власного профілю).")]
        [SerializeField] private SceneReference[] _targetScenes = Array.Empty<SceneReference>();

        [Tooltip("Якщо ввімкнено — цей профіль є глобальним fallback і застосовується до будь-якої сцени.")]
        [SerializeField] private bool _isGlobal;

        [Header("Default Background Music")]
        [SerializeField] private MusicTrackSettings _defaultMusic = new MusicTrackSettings();

        [Header("Epic Music")]
        [Tooltip("Альтернативний трек (бойова музика, атака, напруга тощо).")]
        [SerializeField] private MusicTrackSettings _epicMusic = new MusicTrackSettings();

        [Header("Behaviour")]
        [Tooltip("Якщо true і наступна сцена грає той самий AudioClip — трек не перезапускається.")]
        [SerializeField] private bool _preserveMusicBetweenScenes = false;

        [Tooltip("Тривалість crossfade при зміні сцени (якщо трек змінюється).")]
        [Min(0f)] [SerializeField] private float _sceneTransitionDuration = 2f;

        // ─── Public accessors ────────────────────────────────────────────

        public SceneReference[] TargetScenes => _targetScenes;
        public bool IsGlobal => _isGlobal || _targetScenes == null || _targetScenes.Length == 0;
        public MusicTrackSettings DefaultMusic => _defaultMusic;
        public MusicTrackSettings EpicMusic => _epicMusic;
        public bool PreserveMusicBetweenScenes => _preserveMusicBetweenScenes;
        public float SceneTransitionDuration => Mathf.Max(0f, _sceneTransitionDuration);

        /// <summary>Чи стосується цей профіль до заданої сцени.</summary>
        public bool MatchesScene(string sceneName)
        {
            if (IsGlobal) return true;
            if (_targetScenes == null) return false;
            for (int i = 0; i < _targetScenes.Length; i++)
            {
                var s = _targetScenes[i];
                if (s != null && string.Equals(s.SceneName, sceneName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        // ─── Runtime mutators (пункт 11 — editor values = initial values) ─

        /// <summary>Змінити гучність default-треку в runtime.</summary>
        public void SetDefaultVolume(float volume) => _defaultMusic.Volume = Mathf.Clamp01(volume);

        /// <summary>Змінити гучність epic-треку в runtime.</summary>
        public void SetEpicVolume(float volume) => _epicMusic.Volume = Mathf.Clamp01(volume);
    }
}
