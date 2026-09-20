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
        /// <summary>Ключ звуку відмови дії вказівника в UI.</summary>
        public string uiPointerDenied = "ui-denied";
        /// <summary>Ключ звуку наведення вказівника в UI.</summary>
        public string uiPointerHover = "ui-hover";
        /// <summary>Чи ввімкнено звук наведення вказівника в UI.</summary>
        public bool uiPointerHoverEnabled = true;
        /// <summary>Ключ звуку тику під час drag у слайдерах UI.</summary>
        [Min(0f)] public float uiPointerHoverCooldown = 0.08f;
        [Min(0f)] public float uiPointerDragTickInterval = 0.07f;
        public string uiPointerDragTick = "ui-slider-tick";

        /// <summary>Звуки нотифікацій за GameplayNotificationKind.</summary>
        public string notificationInfo = "ui-notification-info";
        /// <summary>Ключ звуку успішного сповіщення.</summary>
        public string notificationSuccess = "ui-notification-success";
        /// <summary>Ключ звуку сповіщення-попередження.</summary>
        public string notificationWarning = "ui-notification-warning";
        /// <summary>Ключ звуку сповіщення-помилки.</summary>
        public string notificationError = "ui-notification-error";

        /// <summary>Epic (combat) музика під час бою: вмикається на атаку, вимикається після тиші.</summary>
        public bool combatMusicEnabled = true;
        [Min(1f)] public float combatMusicQuietSeconds = 25f;
    }

    /// <summary>
    /// Правило: доменна подія → звуковий ключ.
    /// Необов'язковий context уточнює подію ("unit:archer", "tile:water") —
    /// спочатку шукається правило eventName+context, інакше базове eventName.
    /// </summary>
    [Serializable]
    public sealed class AudioFeedbackEventRule
    {
        /// <summary>Назва gameplay-події, що тригерить звук.</summary>
        public string eventName;
        /// <summary>Ключ звуку, який відтворюється на подію.</summary>
        public string soundKey;

        /// <summary>Контекстний фільтр події (порожній — будь-який).</summary>
        [Tooltip("Контекст події: 'unit:<unitTypeId>' або 'tile:<tileTypeId>'. Порожньо — будь-який.")]
        public string context = string.Empty;

        /// <summary>Чи відтворювати звук у позиції події у світі.</summary>
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
        /// <summary>Джерело події для фільтрації.</summary>
        public string source;
        /// <summary>Статус події для фільтрації.</summary>
        public string status;
        /// <summary>Ідентифікатор дії для фільтрації.</summary>
        public string actionId;
        /// <summary>Ключ звуку, що відповідає відфільтрованій події.</summary>
        public string soundKey;
        [Range(0f, 1f)] public float volumeScale = 1f;
    }
}
