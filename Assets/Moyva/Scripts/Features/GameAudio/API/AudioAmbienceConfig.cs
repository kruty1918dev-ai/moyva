using System;
using UnityEngine;
using Kruty1918.Moyva.Jsonization;

namespace Kruty1918.Moyva.GameAudio.API
{
    /// <summary>
    /// JSON-конфігурація ambient-шару гри: глобальні шари (beds),
    /// позиційні емітери світу та zoom-політика audibility.
    /// Модель пресету: "audio-ambience" (схема moyva.audio-ambience).
    /// </summary>
    [Serializable]
    public sealed class AudioAmbienceConfig : MoyvaJsonConfigObject
    {
        public AudioAmbienceZoom zoom = new AudioAmbienceZoom();
        public AudioAmbienceBed[] beds = Array.Empty<AudioAmbienceBed>();
        public AudioAmbienceEmitter[] emitters = Array.Empty<AudioAmbienceEmitter>();
        public AudioAmbienceOneShot[] oneShots = Array.Empty<AudioAmbienceOneShot>();
        [Min(0)] public int maxEmittersTotal = 24;
    }

    /// <summary>
    /// Як нормалізований зум камери (0 = максимально близько, 1 = максимально далеко)
    /// впливає на чутність світу.
    /// </summary>
    [Serializable]
    public sealed class AudioAmbienceZoom
    {
        public bool enabled = true;

        /// <summary>Швидкість згладжування zoom-фактора (більше = швидше).</summary>
        [Min(0.1f)] public float smoothing = 5f;

        /// <summary>zoomT, з якого позиційні емітери починають затихати.</summary>
        [Range(0f, 1f)] public float emitterFadeStart = 0.45f;

        /// <summary>zoomT, на якому позиційні емітери повністю затихають.</summary>
        [Range(0f, 1f)] public float emitterFadeEnd = 0.9f;

        /// <summary>Lowpass-частота на bed-шарах при близькій камері.</summary>
        [Range(200f, 22000f)] public float nearCutoff = 22000f;

        /// <summary>Lowpass-частота на bed-шарах при далекій камері ("глухий" світ).</summary>
        [Range(200f, 22000f)] public float farCutoff = 900f;
    }

    /// <summary>Глобальний циклічний шар амбієнсу; гучність інтерполюється між near/far вагами за zoomT.</summary>
    [Serializable]
    public sealed class AudioAmbienceBed
    {
        public string soundKey;
        [Range(0f, 1f)] public float volume = 0.5f;
        [Range(0f, 1f)] public float nearWeight = 1f;
        [Range(0f, 1f)] public float farWeight = 0f;
        public bool lowpassWithZoom = true;
        [Tooltip("<=0 → спільний zoom.farCutoff; інакше — жорсткіша per-bed межа на далекому zoom.")]
        [Range(0f, 22000f)] public float farCutoff = 0f;
    }

    /// <summary>
    /// Позиційний loop-емітер: прив'язується до типу тайла або buildingId.
    /// Тайли кластеризуються у комірки clusterStep×clusterStep — один емітер на кластер.
    /// </summary>
    [Serializable]
    public sealed class AudioAmbienceEmitter
    {
        public string id;
        [Tooltip("TileTypeId тайла, до якого прив'язаний емітер.")]
        public string matchTileTypeId;
        [Tooltip("BuildingId будівлі (точне значення або префікс).")]
        public string matchBuildingId;
        public string soundKey;
        [Range(0f, 1f)] public float volume = 1f;
        [Min(0.01f)] public float minDistance = 2f;
        [Min(0.01f)] public float maxDistance = 14f;
        [Min(1)] public int clusterStep = 6;
        [Min(1)] public int maxInstances = 8;
        [Tooltip("<0 → глобальний zoom.emitterFadeStart.")]
        public float zoomFadeStart = -1f;
        [Tooltip("<0 → глобальний zoom.emitterFadeEnd.")]
        public float zoomFadeEnd = -1f;
    }

    /// <summary>Випадковий 2D-стингер (птах, комаха) з інтервалом і zoom-діапазоном.</summary>
    [Serializable]
    public sealed class AudioAmbienceOneShot
    {
        public string soundKey;
        [Range(0f, 1f)] public float volume = 0.6f;
        [Min(0.05f)] public float minInterval = 4f;
        [Min(0.05f)] public float maxInterval = 12f;
        [Tooltip("zoomT-діапазон, у якому стингер може звучати.")]
        [Range(0f, 1f)] public float minZoom = 0f;
        [Range(0f, 1f)] public float maxZoom = 0.7f;
    }
}
