using System.Collections.Generic;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Сервіс відкриття зовнішніх посилань на соціальні мережі.
    /// Список записів конфігурується через ScriptableObject (SocialLinksConfigSO).
    /// </summary>
    public interface ISocialLinksService
    {
        /// <summary>Всі доступні посилання у порядку відображення.</summary>
        IReadOnlyList<SocialLinkEntry> Links { get; }

        /// <summary>
        /// Відкриває посилання з указаним <paramref name="id"/>. Повертає <c>true</c> якщо
        /// запис знайдено і валідний URL. Фактичне відкриття робить <c>Application.OpenURL</c>.
        /// </summary>
        bool OpenLink(string id);

        /// <summary>
        /// Відкриває запис із наперед отриманою структурою <see cref="SocialLinkEntry"/>.
        /// </summary>
        bool OpenLink(SocialLinkEntry entry);
    }
}
