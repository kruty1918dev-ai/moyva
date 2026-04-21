using System;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Опис посилання на соціальну мережу, що відображається у меню налаштувань.
    /// Використовується з <see cref="ISocialLinksService"/>.
    /// </summary>
    [Serializable]
    public struct SocialLinkEntry
    {
        [Tooltip("Унікальний ідентифікатор (напр. \"discord\", \"youtube\").")]
        public string Id;

        [Tooltip("Назва, що показується в UI (Tooltip/label).")]
        public string DisplayName;

        [Tooltip("Повний URL (https://...). Використовується у Application.OpenURL.")]
        public string Url;

        [Tooltip("Іконка для кнопки. Може бути null.")]
        public Sprite Icon;

        /// <summary>Чи має запис валідний URL для відкриття.</summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(Url) &&
            (Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
             Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
    }
}
