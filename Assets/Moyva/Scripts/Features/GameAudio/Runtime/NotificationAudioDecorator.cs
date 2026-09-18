using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.GameAudio.API;
using Kruty1918.Moyva.Notifications.API;
using Zenject;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Декоратор IGameplayNotificationService: додає звук за kind нотифікації.
    /// Не змінює показ — лише озвучує.
    /// </summary>
    public sealed class NotificationAudioDecorator : IGameplayNotificationService
    {
        private readonly IGameplayNotificationService _inner;
        private readonly IAudioService _audio;
        private readonly AudioFeedbackConfig _config;

        public NotificationAudioDecorator(
            IGameplayNotificationService inner,
            [InjectOptional] IAudioService audio,
            [InjectOptional] AudioFeedbackConfig config)
        {
            _inner = inner;
            _audio = audio;
            _config = config;
        }

        public void Show(string message, GameplayNotificationKind kind = GameplayNotificationKind.Info,
            float? holdDuration = null, string dedupKey = null)
        {
            PlayFor(kind);
            _inner.Show(message, kind, holdDuration, dedupKey);
        }

        public void Show(GameplayNotificationRequest request)
        {
            PlayFor(request.Kind);
            _inner.Show(request);
        }

        public void Clear() => _inner.Clear();

        private void PlayFor(GameplayNotificationKind kind)
        {
            if (_audio == null || _config == null)
                return;

            string key = kind switch
            {
                GameplayNotificationKind.Success => _config.notificationSuccess,
                GameplayNotificationKind.Warning => _config.notificationWarning,
                GameplayNotificationKind.Error => _config.notificationError,
                _ => _config.notificationInfo,
            };

            if (!string.IsNullOrWhiteSpace(key))
                _audio.Play(key);
        }
    }
}
