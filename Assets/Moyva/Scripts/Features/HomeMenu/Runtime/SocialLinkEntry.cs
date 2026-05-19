using System;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Опис одного соціального посилання для відображення в меню.
    /// Залежності: використовується SocialLinksConfigSO та відповідним UI.
    /// </summary>
    [Serializable]
    public struct SocialLinkEntry
    {
        /// <summary>Унікальний ідентифікатор посилання.</summary>
        public string Id;

        /// <summary>Назва, що показується користувачу.</summary>
        public string DisplayName;

        /// <summary>URL-адреса переходу.</summary>
        public string Url;

        /// <summary>Іконка соціальної платформи.</summary>
        public Sprite Icon;
    }
}
