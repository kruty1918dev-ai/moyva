using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Kruty1918.Moyva.Shared.Localization
{
    /// <summary>
    /// Guarantees glyph coverage for every supported language.
    /// The project fonts (LiberationSans-derived SDF, homerun, pixont) ship with
    /// ASCII-only static atlases, so Cyrillic and other scripts render as missing
    /// glyphs. This service creates one dynamic-atlas <see cref="TMP_FontAsset"/>
    /// from the raw LiberationSans TTF (full Latin + Cyrillic coverage) and wires
    /// it as a fallback both globally (<c>TMP_Settings.fallbackFontAssets</c>) and
    /// on every primary font asset registered via <see cref="RegisterPrimaryFont"/>
    /// (the <c>moyvaFont</c> global used by UnityHTML, tooltip fonts, etc).
    /// Fallback resolution in TMP keeps Latin metrics identical and only routes
    /// missing codepoints through the dynamic asset — no artifacts for existing text.
    /// </summary>
    public sealed class LocalizationFontService
    {
        private const string FontResourcePath = "Fonts/LiberationSans";
        private readonly HashSet<TMP_FontAsset> _registered = new HashSet<TMP_FontAsset>();
        private TMP_FontAsset _fallback;

        /// <summary>
        /// Підключає primary font asset до multilingual fallback-ланцюга.
        /// Викликається presenter-ами для шрифту, який передається у UnityHtmlHost.
        /// The dynamic fallback must come FIRST: serialized fallback entries such as
        /// TMP's "LiberationSans SDF - Fallback" are persistent dynamic font assets —
        /// when TMP grows their atlas at runtime, the editor reimports the asset and
        /// destroys the live object, leaving MissingReferenceException in every text
        /// component that still references it. The non-persistent runtime asset is
        /// never reimported, so routing glyph lookups through it first is safe.
        /// </summary>
        public void RegisterPrimaryFont(TMP_FontAsset primary)
        {
            if (primary == null || !_registered.Add(primary)) return;
            TMP_FontAsset fallback = EnsureFallbackFont();
            if (fallback == null || primary == fallback) return;
            primary.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
            primary.fallbackFontAssetTable.Remove(fallback);
            primary.fallbackFontAssetTable.Insert(0, fallback);
        }

        /// <summary>
        /// Попередньо прогріває динамічний атлас символами активної мови,
        /// щоб перший рендер після перемикання вже мав усі гліфи.
        /// </summary>
        public void Warmup(LocalizationLanguage language)
        {
            TMP_FontAsset fallback = EnsureFallbackFont();
            if (fallback == null) return;
            string charset = CharsetFor(language?.Id);
            if (charset.Length == 0) return;
            fallback.TryAddCharacters(charset);
        }

        /// <summary>
        /// Ледаче створення dynamic font asset + реєстрація у глобальному
        /// fallback-листі TMP (покриває tooltips, dynamic UGUI, TMP_Settings.defaultFontAsset).
        /// </summary>
        private TMP_FontAsset EnsureFallbackFont()
        {
            if (_fallback != null) return _fallback;
            Font source = Resources.Load<Font>(FontResourcePath);
            if (source == null)
            {
                Debug.LogError($"[Localization] Font '{FontResourcePath}' not found in Resources; " +
                               "non-ASCII glyphs will be missing.");
                return null;
            }
            _fallback = TMP_FontAsset.CreateFontAsset(source);
            _fallback.name = "LocalizationFallback (Dynamic)";
            _fallback.hideFlags = HideFlags.HideAndDontSave;
            if (TMP_Settings.instance != null && TMP_Settings.fallbackFontAssets != null)
            {
                // First position here too: glyph searches must reach the
                // non-persistent fallback before any persistent dynamic asset.
                TMP_Settings.fallbackFontAssets.Remove(_fallback);
                TMP_Settings.fallbackFontAssets.Insert(0, _fallback);
            }
            if (TMP_Settings.defaultFontAsset != null)
                RegisterPrimaryFont(TMP_Settings.defaultFontAsset);
            return _fallback;
        }

        /// <summary>
        /// Повертає набір символів, який потрібен мові понад ASCII.
        /// Кирилиця покриває uk/ru/be; латинка повністю є у primary шрифтах.
        /// </summary>
        private static string CharsetFor(string languageId)
        {
            var builder = new StringBuilder(128);
            switch (languageId)
            {
                case "uk":
                case "ru":
                case "be":
                    // U+0400–U+04FF — повний кириличний блок (вкл. Єє Її Іі Ґґ).
                    for (char c = 'Ѐ'; c <= 'ӿ'; c++) builder.Append(c);
                    builder.Append("—–«»„“”’‘…₴№");
                    break;
            }
            return builder.ToString();
        }
    }
}
