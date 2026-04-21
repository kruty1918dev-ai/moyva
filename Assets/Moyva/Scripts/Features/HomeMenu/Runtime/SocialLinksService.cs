using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Production-реалізація <see cref="ISocialLinksService"/>.
    /// Відкриває URL через <see cref="Application.OpenURL"/>.
    /// </summary>
    internal sealed class SocialLinksService : ISocialLinksService
    {
        private readonly List<SocialLinkEntry> _links;
        private readonly Action<string> _urlOpener;

        /// <inheritdoc/>
        public IReadOnlyList<SocialLinkEntry> Links => _links;

        /// <summary>
        /// Створює сервіс, використовуючи <see cref="Application.OpenURL"/> як відкривач посилань.
        /// </summary>
        public SocialLinksService(SocialLinksConfigSO config)
            : this(config, Application.OpenURL)
        {
        }

        /// <summary>
        /// Створює сервіс з власним відкривачем URL. Використовується у тестах.
        /// </summary>
        public SocialLinksService(SocialLinksConfigSO config, Action<string> urlOpener)
        {
            _urlOpener = urlOpener ?? throw new ArgumentNullException(nameof(urlOpener));
            _links = new List<SocialLinkEntry>();

            if (config == null) return;
            foreach (var entry in config.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Id)) continue;
                _links.Add(entry);
            }
        }

        /// <inheritdoc/>
        public bool OpenLink(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            for (int i = 0; i < _links.Count; i++)
            {
                if (!string.Equals(_links[i].Id, id, StringComparison.OrdinalIgnoreCase)) continue;
                return OpenLink(_links[i]);
            }
            return false;
        }

        /// <inheritdoc/>
        public bool OpenLink(SocialLinkEntry entry)
        {
            if (!entry.IsValid)
            {
                Debug.LogWarning($"[SocialLinksService] Пропущено відкриття '{entry.Id}': " +
                                 $"URL '{entry.Url}' не валідний (порожній або без http/https).");
                return false;
            }

            _urlOpener(entry.Url);
            return true;
        }
    }
}
