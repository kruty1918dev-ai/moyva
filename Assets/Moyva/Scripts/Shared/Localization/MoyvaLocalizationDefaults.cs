using System.Collections.Generic;
using System.IO;
using Kruty1918.Localization;
using UnityEngine;

namespace Kruty1918.Moyva.Shared.Localization
{
    /// <summary>
    /// Moyva-owned wiring for the com.kruty1918.localization package:
    /// supported languages, catalog Resources folder, persistence file,
    /// SystemLanguage mapping and the Cyrillic fallback font charset.
    /// </summary>
    public static class MoyvaLocalizationDefaults
    {
        public const string CatalogResourceFolder = "MoyvaLocales";
        public const string FontResourcePath = "Fonts/LiberationSans";
        public const string PersistFileName = "moyva-language.txt";

        public static LocalizationOptions CreateOptions()
        {
            return new LocalizationOptions
            {
                Languages = new List<LocalizationLanguage>
                {
                    new LocalizationLanguage("en", "English"),
                    new LocalizationLanguage("uk", "Українська"),
                },
                DefaultLanguageId = "en",
                CatalogResourceFolder = CatalogResourceFolder,
                PersistFilePath = Path.Combine(
                    Application.persistentDataPath, PersistFileName),
                SystemLanguageMap = MapSystemLanguage,
            };
        }

        /// <summary>Test seam: same options but persistence redirected to a temp file.</summary>
        public static LocalizationOptions CreateOptions(string persistFilePath)
        {
            LocalizationOptions options = CreateOptions();
            options.PersistFilePath = persistFilePath;
            return options;
        }

        public static LocalizationFontOptions CreateFontOptions()
        {
            return new LocalizationFontOptions
            {
                FontResourcePath = FontResourcePath,
                CharsetForLanguage = CyrillicCharset,
            };
        }

        private static string MapSystemLanguage(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Ukrainian: return "uk";
                case SystemLanguage.Russian:
                case SystemLanguage.Belarusian:
                    // Немає окремого каталогу — українська найближча за гліфами/аудиторією.
                    return "uk";
                default: return "en";
            }
        }

        /// <summary>Кирилиця покриває uk/ru/be; латинка повністю є у primary шрифтах.</summary>
        private static string CyrillicCharset(string languageId)
        {
            switch (languageId)
            {
                case "uk":
                case "ru":
                case "be":
                {
                    var builder = new System.Text.StringBuilder(128);
                    // U+0400–U+04FF — повний кириличний блок (вкл. Єє Її Іі Ґґ).
                    for (char c = 'Ѐ'; c <= 'ӿ'; c++) builder.Append(c);
                    builder.Append("—–«»„“”’‘…₴№");
                    return builder.ToString();
                }
                default:
                    return null;
            }
        }
    }
}
