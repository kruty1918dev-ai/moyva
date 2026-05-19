using System;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// ScriptableObject-конфігурація набору соціальних посилань для HomeMenu.
    /// Залежності: використовується UI-елементами соціальних кнопок та runtime factory.
    /// </summary>
    [CreateAssetMenu(fileName = "SocialLinks", menuName = "Moyva/Home Menu/Social Links")]
    public sealed class SocialLinksConfigSO : ScriptableObject
    {
        /// <summary>Масив усіх доступних соціальних посилань.</summary>
        public SocialLinkEntry[] entries = Array.Empty<SocialLinkEntry>();
    }
}