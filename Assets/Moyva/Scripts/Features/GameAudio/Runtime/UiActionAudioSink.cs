using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.GameAudio.API;
using Kruty1918.Moyva.UIActions.API;
using UnityEngine;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Озвучує результат UiActionRouter (hotkey/escape/programmatic дії).
    /// Правила з audio-feedback пресету: перше співпадіння (source/status/actionId) виграє.
    /// </summary>
    public sealed class UiActionAudioSink : IUiActionFeedbackSink
    {
        private readonly IAudioService _audio;
        private readonly AudioFeedbackConfig _config;

        public UiActionAudioSink(
            [Zenject.InjectOptional] IAudioService audio,
            [Zenject.InjectOptional] AudioFeedbackConfig config)
        {
            _audio = audio;
            _config = config;
        }

        public void OnActionExecuted(in UiActionRequest request, in UiActionResult result)
        {
            if (_audio == null || _config?.uiActionSounds == null)
                return;

            foreach (var rule in _config.uiActionSounds)
            {
                if (rule == null || string.IsNullOrWhiteSpace(rule.soundKey))
                    continue;
                if (!Matches(rule.source, request.Source.ToString())
                    || !Matches(rule.status, result.Status.ToString())
                    || !Matches(rule.actionId, request.ActionId.Value))
                    continue;

                _audio.Play(rule.soundKey,
                    new AudioPlayOptions(volumeScale: rule.volumeScale));
                return;
            }
        }

        private static bool Matches(string pattern, string value)
            => string.IsNullOrWhiteSpace(pattern)
               || string.Equals(pattern, value, StringComparison.OrdinalIgnoreCase);
    }
}
