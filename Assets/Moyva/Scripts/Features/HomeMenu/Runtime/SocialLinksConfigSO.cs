using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Список посилань на соціальні мережі, що відображаються у меню налаштувань.
    /// Заповнюється продюсером/маркетингом. У виробничому білді має містити лише
    /// валідні URL (https://...). Порожні записи пропускаються.
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/HomeMenu/Social Links", fileName = "SocialLinks")]
    public sealed class SocialLinksConfigSO : ScriptableObject
    {
        [Tooltip("Записи, впорядковані так, як мають показуватись у UI.")]
        [SerializeField]
        private List<SocialLinkEntry> entries = new()
        {
            new SocialLinkEntry { Id = "discord",  DisplayName = "Discord",  Url = "" },
            new SocialLinkEntry { Id = "youtube",  DisplayName = "YouTube",  Url = "" },
            new SocialLinkEntry { Id = "twitter",  DisplayName = "Twitter",  Url = "" },
            new SocialLinkEntry { Id = "website",  DisplayName = "Website",  Url = "" }
        };

        /// <summary>Усі налаштовані записи у порядку відображення.</summary>
        public IReadOnlyList<SocialLinkEntry> Entries => entries;

#if UNITY_INCLUDE_TESTS
        /// <summary>Замінює список записів. Використовується тільки у тестах.</summary>
        public void SetEntriesForTest(IEnumerable<SocialLinkEntry> newEntries)
        {
            entries = new List<SocialLinkEntry>(newEntries);
        }
#endif

        private void OnValidate()
        {
            if (entries == null) return;
            var seen = new HashSet<string>();
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (string.IsNullOrWhiteSpace(e.Id)) continue;
                if (!seen.Add(e.Id))
                    Debug.LogWarning($"[{nameof(SocialLinksConfigSO)}] Дублікат Id='{e.Id}' у записі {i}.");
            }
        }
    }
}
