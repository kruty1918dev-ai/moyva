using System;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    [CreateAssetMenu(fileName = "SocialLinks", menuName = "Moyva/Home Menu/Social Links")]
    public sealed class SocialLinksConfigSO : ScriptableObject
    {
        public SocialLinkEntry[] entries = Array.Empty<SocialLinkEntry>();
    }

    [Serializable]
    public struct SocialLinkEntry
    {
        public string Id;
        public string DisplayName;
        public string Url;
        public Sprite Icon;
    }
}