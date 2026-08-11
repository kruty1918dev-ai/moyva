using System;
using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// ScriptableObject-конфігурація набору соціальних посилань для HomeMenu.
    /// Залежності: використовується UI-елементами соціальних кнопок та runtime factory.
    /// </summary>
[System.Serializable]
public sealed class SocialLinksConfigSO : MoyvaJsonConfigObject
    {
        /// <summary>Масив усіх доступних соціальних посилань.</summary>
        public SocialLinkEntry[] entries = Array.Empty<SocialLinkEntry>();
    }
}