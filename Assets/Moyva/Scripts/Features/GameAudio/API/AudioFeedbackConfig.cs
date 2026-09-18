using System;
using UnityEngine;
using Kruty1918.Moyva.Jsonization;

namespace Kruty1918.Moyva.GameAudio.API
{
    /// <summary>
    /// JSON-мапа подій гри та UI на звукові ключі.
    /// Модель пресету: "audio-feedback" (схема moyva.audio-feedback).
    /// </summary>
    [Serializable]
    public sealed class AudioFeedbackConfig : MoyvaJsonConfigObject
    {
        /// <summary>Події домену → звук. eventName: "unit-moved", "building-placed" тощо.</summary>
        public AudioFeedbackEventRule[] eventSounds = Array.Empty<AudioFeedbackEventRule>();

        /// <summary>Правила для UiActionRouter (hotkey/escape/programmatic).</summary>
        public AudioUiActionRule[] uiActionSounds = Array.Empty<AudioUiActionRule>();

        /// <summary>Звуки вказівника для uGUI-елементів (HTML-кнопки — це Unity Selectable).</summary>
        public string uiPointerClick = "ui-click";
        public string uiPointerDenied = "ui-denied";
        public string uiPointerHover = "ui-hover";
        public bool uiPointerHoverEnabled = true;
        [Min(0f)] public float uiPointerHoverCooldown = 0.08f;
        [Min(0f)] public float uiPointerDragTickInterval = 0.07f;
        public string uiPointerDragTick = "ui-slider-tick";

        /// <summary>Звуки нотифікацій за GameplayNotificationKind.</summary>
        public string notificationInfo = "ui-notification-info";
        public string notificationSuccess = "ui-notification-success";
        public string notificationWarning = "ui-notification-warning";
        public string notificationError = "ui-notification-error";
    }

    /// <summary>Правило: доменна подія → звуковий ключ.</summary>
    [Serializable]
    public sealed class AudioFeedbackEventRule
    {
        public string eventName;
        public string soundKey;
        [Range(0f, 1f)] public float volumeScale = 1f;
        [Tooltip("Грати у 3D-позиції події, якщо вона відома.")]
        public bool atPosition = true;
    }

    /// <summary>
    /// Правило для UiActionRouter. Порожній source/status/actionId = будь-яке значення.
    /// Перше співпадіння виграє.
    /// </summary>
    [Serializable]
    public sealed class AudioUiActionRule
    {
        public string source;
        public string status;
        public string actionId;
        public string soundKey;
        [Range(0f, 1f)] public float volumeScale = 1f;
    }
}
