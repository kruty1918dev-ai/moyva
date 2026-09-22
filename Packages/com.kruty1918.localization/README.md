# Kruty1918 Localization

JSON-catalog localization runtime for Unity.

- `ILocalizationService` / `LocalizationService` — language switching, `T`/`TF`/`TN`
  lookups with plural-form support (`one`/`few`/`many`/`other`), persisted selection.
- `LocalizationCatalog` / `LocalizationLanguage` / `LocalizationPluralForms` — resolved snapshots.
- `LocalizationFontService` — creates a dynamic-atlas TMP fallback font and wires it
  into `TMP_Settings.fallbackFontAssets` and registered primary fonts.

The host supplies everything game-specific via options objects:

```csharp
var service = new LocalizationService(new LocalizationOptions
{
    Languages = new[] { new LocalizationLanguage("en", "English"), ... },
    DefaultLanguageId = "en",
    CatalogResourceFolder = "MyLocales",           // Resources/MyLocales/{id}.json
    PersistFilePath = ".../language.txt",
    SystemLanguageMap = lang => "en",
});
var fonts = new LocalizationFontService(new LocalizationFontOptions
{
    FontResourcePath = "Fonts/MyFont",
    CharsetForLanguage = id => null,
});
```

Catalog JSON shape: `{ "entries": { "Key": "text" | {"one":..,"few":..,"many":..,"other":..} } }`.
Lookup keys are the source text as written in code; missing entries fall back to the key.

No dependency on any host-game sources, scenes or assets.
