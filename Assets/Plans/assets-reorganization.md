# План реорганізації структури папки Assets — проєкт Moyva

> **СТАТУС: ПЛАН. Нічого не переміщено, не перейменовано й не видалено.** Виконання починається лише після вашого підтвердження.

---

# Project Overview
- **Game Title:** Moyva
- **High-Level Concept:** Стратегія з процедурною генерацією світу, будівництвом, юнітами, туманом війни та мультиплеєром (на базі Zenject DI, TileWorldCreator, URP/FlatKit toon-стилізація).
- **Players:** Single-player + мережевий мультиплеєр (є `DefaultNetworkPrefabs.asset`, `Features/Multiplayer`).
- **Tone / Art Direction:** Стилізований low-poly / toon (KayKit, FlatKit, Stylized Water 3, PolyOne).
- **Target Platform:** StandaloneLinux64 (поточна ціль збірки).
- **Render Pipeline:** URP (кастомний `Moyva_RPAsset`, FlatKit render features).
- **Unity Version:** 6000.3.10f1
- **Input System:** New Input System (`.inputactions`).

## Мета реорганізації
1. Централізувати графіку, моделі, матеріали, текстури, префаби, аудіо, UI, сцени, дані та скрипти у зрозумілі категорії.
2. Прибрати хаотичне розкидання файлів по папках різних asset pack.
3. Зробити структуру придатною для великого проєкту.
4. **Не втратити GUID, посилання між asset-файлами, у сценах, префабах, ScriptableObject і матеріалах.**

## Затверджені рішення (від користувача)
| # | Рішення |
|---|---------|
| 1 | **Перейменувати + оновити код** — повна відповідність цільовій структурі, включно з `SO → Data/ScriptableObjects`. Усі жорсткі шляхи в C# оновлюються. |
| 2 | **Сторонні пакети → `Assets/ThirdParty/`** (з обґрунтованими винятками `Plugins/` та `TextMesh Pro/` — див. розділ ризиків). |
| 3 | **Prototype-контент не чіпаємо** (`Prefabs/Prototype`, `Art/PixelMapCreator`, `Art/Prototype`). |
| 4 | **VCS з .meta увімкнено** → переміщення виконуємо через `AssetDatabase.MoveAsset` (зберігає GUID). |

---

# Аналіз поточного стану (факти з відкритого Editor)

## Кореневі папки Assets/ (файлів без .meta)
| Папка | Файлів | Класифікація |
|-------|-------:|--------------|
| `Moyva/` | 9928 | **Власний код і контент** |
| `Plugins/` | 806 | Sirenix Odin + Zenject (**спец-папка, DLL**) |
| `NaPH - RPG & Fantasy Sounds Bundle/` | 1000 | Сторонній аудіо-пак |
| `KayKit/` | 929 | Сторонні 3D-моделі |
| `FlatKit/` | 892 | Сторонній shader/URP-пак (asmdef) |
| `Honeti/` | 757 | Сторонній пак |
| `TileWorldCreator/` | 485 | Сторонній генератор (asmdef) |
| `Stylized Water 3/` | 296 | Сторонній water-пак (asmdef) |
| `TextMesh Pro/` | 173 | **Спец-пак Unity (Resources/Settings)** |
| `UI Soundpack/` | 104 | Сторонній аудіо-пак |
| `Blink/` | 60 | Сторонній пак |
| `PolyOne/` | 26 | Сторонній пак |
| `Shaders/` | 2 | Службова (root) |
| `Render/` | 1 | Службова (root) |
| `_Recovery/` | 1 | **Автовідновлення Unity — не чіпати** |
| root loose | 1 | `DefaultNetworkPrefabs.asset` (Netcode — **спец-файл**) |

**Разом ≈ 20 000 файлів.**

## Розподіл за типами (весь Assets)
`.prefab` 7122 · `.cs` 2415 · `.asset` 1578 · `.png` 1370 · `.wav` 1055 · `.fbx` 659 · `.mat` 605 · `.unity` 93 · `.asmdef` 84 · `.asmref` 3 · `.shader` 51 · `.shadergraph` 23 · `.hlsl` 36 · `.ttf` 13 · `.psd` 12 · `.dll` 16 · `.inputactions` 1 · `.mixer` 1 · інші.

## Поточна внутрішня структура Moyva/
| Підпапка | Файлів | Підпапки |
|----------|-------:|----------|
| `Art/` | 1708 | `16x16 Pixel Art Item Icons`, `Icons`, `PixelMapCreator` (1297), `Prototype`, `UI` (361) |
| `Prefabs/` | 6341 | `Buildings` (20), `Objects` (79), `Prototype` (5986), `Tiles` (246), `UI` (6), `Units` (4) |
| `Scripts/` | 1645 | `Bootstrap`, `Editor`, `EditorShared`, `Features` (1419), `Infrastructure`, `Shared`, `Tests` (109) |
| `SO/` | 161 | `Bootstrap`, `Construction`, `Economy` (81), `Effects`, `Environment`, `FogOfWar`, `Generation` (33), `HomeMenu`, `Input`, `Player`, `ProjectDefaults`, `Tile`, `Units`, `URP` (10), `WorldCreation` |
| `Resources/` | 8 | `Moyva`, `MusicProfiles` + loose |
| `Shaders/` | 14 | — |
| `Materials/` | 5 | — |
| `Generated/` | 13 | `Grass` |
| `Scenes/` | 6 | — |
| `Tiles/` | 15 | — |
| `Audio/` | 1 | — |
| `Editor/` | 1 | — |
| `Presets/` | 1 | — |
| loose | 9 | `PlanarReflectionRenderer.cs`, `skybox.mat`, `TileWater.asset`, `ReflectionRenderTexture.renderTexture`, `sCLIFF.fbx`, `TilesCliff0,2.fbx`, `calendar_config.dat`, `link.xml`, `DefaultNetworkPrefabs.asset` |

## Спеціальні папки (розкидані)
- **Resources:** `Moyva/Resources`, `Moyva/SO/Economy/Resources`, `Moyva/Scripts/Features/Construction/API/Data/Resources` + сторонні (FlatKit, TextMesh Pro, Stylized Water 3, Plugins/Zenject).
- **Editor:** 25+ папок (у Moyva/Scripts кожен Feature має власний `Editor/`) + сторонні.
- **Plugins:** `Assets/Plugins` (Odin + Zenject).
- **StreamingAssets / Gizmos / AddressableAssetsData:** **відсутні** (Addressables не використовуються).
- **asmdef:** 84 (з них 60+ у `Moyva/Scripts`, решта — сторонні). **asmref:** 3 (усі сторонні).

## Аналіз жорстко прописаних шляхів (головний ризик)
- **`Shader.Find(...)`** — скрізь використовує **імена шейдерів**, не шляхи → **переміщення .shader безпечне** (за умови збереження рядка `Shader "Ім'я"` усередині файлу).
- **`Resources.Load<T>("...")`** — імена `"MoyvaAudioRegistry"`, `"MoyvaSceneAudioOverrides"` → **папку Resources можна перемістити, але вона мусить лишатись назви `Resources`, а asset'и не можна перейменовувати**.
- **`AssetDatabase.LoadAssetAtPath` + рядкові const шляхи `"Assets/Moyva/..."`** — знайдено в **~46 файлах** (переважно Editor/Tests). Критичні константи:
  - `SO/ProjectDefaults/MoyvaProjectSettings.asset` (`MoyvaProjectSettingsSO.DefaultAssetPath`)
  - `SO/Economy`, `SO/Bootstrap`, `SO/Camera`, `SO/Generation`
  - `Prefabs/Tiles`, `Prefabs/Objects`, `Prefabs/Units`, `Prefabs/Buildings`
  - `Resources/MoyvaStartupGraphics.asset`, `Resources/MoyvaAudioMixerBindings.asset`, `Resources/MusicProfiles`, `Resources/MoyvaAudioRegistry.asset`
  - `Audio/MoyvaMixer.mixer`
  - `Generated/Construction/BuildingPreviews`
  - Тестові шляхи в `Scripts/Tests/Generator/...`
- **Наявний інструмент:** `Moyva/Scripts/Editor/FolderSetupMenu.cs` + `EnsureFolder(...)` виклики — після реорганізації їх теж треба оновити.

## Дублікати назв (нічого не видаляємо)
23 групи дублікатів імен у Moyva, зокрема:
- grass/tile/water/sand/snow-префаби між `Prefabs/Prototype` та `Generated/Grass` (згенерований контент — **не чіпаємо, Prototype виключено**).
- `.asset`: `DataBiomesSettings.asset`, `DataNoiseSettings.asset`, `EditorPreviewSettings.asset` (по 2) — задокументувати, **не видаляти**.

---

# Цільова структура

```
Assets/
├── Moyva/
│   ├── Art/
│   │   ├── Models/           (Buildings/ Units/ Environment/ Tiles/) ← fbx з Moyva root + Art
│   │   ├── Materials/        (Environment/ ...) ← Moyva/Materials + loose .mat
│   │   ├── Textures/
│   │   ├── Sprites/          ← Art/Icons, 16x16 Item Icons
│   │   ├── Animations/       ← .anim/.controller
│   │   ├── Shaders/          ← Moyva/Shaders (+ root Shaders/)
│   │   └── VFX/
│   │   └── _Prototype/       ← Art/PixelMapCreator, Art/Prototype (ЗАЛИШАЄМО ЯК Є, лише під Art)
│   ├── Prefabs/
│   │   ├── Buildings/  Units/  Environment/  Tiles/  UI/  Systems/
│   │   └── Prototype/        ← НЕ ЧІПАЄМО (5986 файлів)
│   ├── UI/
│   │   ├── Icons/  Fonts/  Atlases/  Themes/   ← Art/UI (361)
│   ├── Audio/
│   │   ├── Music/  SFX/       ← Moyva/Audio (Mixer тут)
│   ├── Scenes/                ← без змін
│   ├── Scripts/               ← без змін розташування asmdef-меж (лише правки рядкових шляхів)
│   ├── Data/
│   │   ├── ScriptableObjects/ ← Moyva/SO/*  (перейменування)
│   │   └── Configs/           ← calendar_config.dat, link.xml, .json/.xml конфіги
│   ├── Resources/             ← ЗАЛИШАЄТЬСЯ назви "Resources" (спец-папка)
│   ├── Generated/             ← ЗАЛИШАЄТЬСЯ (hardcoded у коді)
│   ├── Settings/              ← URP-assets з SO/URP, render-налаштування
│   └── Tests/                 ← прив'язані до Scripts/Tests asmdef (лишаються з кодом)
├── ThirdParty/
│   ├── KayKit/  Honeti/  PolyOne/  Blink/
│   ├── FlatKit/  StylizedWater3/  TileWorldCreator/   (asmdef-пакети)
│   ├── Audio/ (NaPH Sounds, UI Soundpack)
├── Plugins/                   ← ЗАЛИШАЄТЬСЯ В КОРЕНІ (Odin + Zenject, спец-папка)
├── TextMesh Pro/             ← ЗАЛИШАЄТЬСЯ В КОРЕНІ (спец-пак Unity)
└── _Recovery/                ← НЕ ЧІПАЄМО (авто-recovery Unity)
```

---

# Key Asset & Context (що змінюється)
- **Переміщення asset'ів:** лише через `AssetDatabase.MoveAsset` / Project window drag (GUID зберігається, .meta йде разом).
- **Файли C# з правками рядкових шляхів (~46):** усі перелічені в розділі «Аналіз жорстко прописаних шляхів». Найважливіші:
  - `Scripts/Features/Grid/API/MoyvaProjectSettingsSO.cs` (`DefaultAssetPath`)
  - `Scripts/Editor/RegistryHubWindow.cs`, `RegistryFactoryEditorWindow.cs`, `UnitRegistryEditor.cs`, `MapObjectRegistryEditor.cs`
  - `Scripts/Editor/GameplayStartupGraphicsWindow.cs`, `SceneMusicEditorExtension.cs`, `CameraSettingsEditorWindow.cs`, `FolderSetupMenu.cs`
  - `Scripts/EditorShared/EconomyResourceEditorShared.cs`, `AdaptivePrefabPreviewUtility.cs`
  - `Scripts/Bootstrap/Editor/BootstrapInstallerConfigWindow.cs`, `PlayerSpawnPreviewWindow.cs`
  - `Scripts/Tests/Generator/*` (тестові шляхи)
- **Категорії, що НЕ переміщуються (з обґрунтуванням):** `Resources/`, `Generated/`, `Plugins/`, `TextMesh Pro/`, `_Recovery/`, `Scripts/Tests`, `DefaultNetworkPrefabs.asset`.

---

# Таблиця переміщень (репрезентативна, по категоріях)

| Поточний шлях | Новий шлях | Тип | Ризик | Залежності | Причина |
|---|---|---|---|---|---|
| `Moyva/SO/*` | `Moyva/Data/ScriptableObjects/*` | ScriptableObject | **Високий** | ~30 hardcoded-шляхів у Editor/Tests | Централізація даних (рішення користувача) |
| `Moyva/SO/ProjectDefaults/MoyvaProjectSettings.asset` | `Moyva/Data/ScriptableObjects/ProjectDefaults/...` | SO | **Високий** | `MoyvaProjectSettingsSO.DefaultAssetPath` | Оновити const у коді синхронно |
| `Moyva/SO/URP/*` | `Moyva/Settings/*` | RP assets | Середній | Graphics/Quality Settings посилаються за GUID | Розділити дані vs налаштування рендера |
| `Moyva/Materials/*`, root `skybox.mat` | `Moyva/Art/Materials/...` | Material | Низький | GUID-посилання (сцени/префаби) | Централізація матеріалів |
| `Moyva/Shaders/*`, root `Shaders/` | `Moyva/Art/Shaders/` | Shader | Низький | `Shader.Find` за іменем — безпечно | Централізація шейдерів |
| root `sCLIFF.fbx`, `TilesCliff0,2.fbx` | `Moyva/Art/Models/Tiles/` | Model | Низький | GUID у префабах | Прибрати loose-файли з root |
| `Moyva/Art/UI/*` | `Moyva/UI/{Icons,Fonts,Atlases,Themes}/` | Sprites/Fonts | Середній | Canvas-префаби, TMP | Централізація UI |
| `Moyva/Art/Icons`, `16x16 Item Icons` | `Moyva/Art/Sprites/` | Sprite | Низький | GUID | Групування спрайтів |
| `Moyva/Audio/*` (+ Mixer) | `Moyva/Audio/{Music,SFX}/` | Audio/Mixer | **Середній** | `SceneMusicEditorExtension.MixerPath` | Оновити const шляху міксера |
| `calendar_config.dat`, `link.xml` | `Moyva/Data/Configs/` | Config | Середній | `link.xml` впливає на IL2CPP-strip | Централізація конфігів; перевірити link.xml |
| `KayKit/`, `Honeti/`, `PolyOne/`, `Blink/` | `Assets/ThirdParty/<name>/` | Art packs | Низький | GUID-посилання | Ізоляція сторонніх (без asmdef/hardcode) |
| `FlatKit/`, `Stylized Water 3/`, `TileWorldCreator/` | `Assets/ThirdParty/<name>/` | Packs з asmdef | **Середній** | asmdef (GUID/name), внутрішні Resources/Shader.Find | MoveAsset зберігає посилання; потрібна перевірка компіляції |
| `NaPH Sounds`, `UI Soundpack` | `Assets/ThirdParty/Audio/<name>/` | Audio packs | Низький | GUID у AudioRegistry SO | Ізоляція сторонніх |
| `Moyva/Prefabs/{Buildings,Units,Tiles,Objects,UI}` | лишаються, `Objects → Environment/Systems` за призначенням | Prefab | **Високий** | `Prefabs/Objects\|Units\|Tiles\|Buildings` hardcoded | Оновити const у Registry-вікнах |
| **`Moyva/Prefabs/Prototype`** | **без змін** | Prefab | — | — | Виключено рішенням |
| **`Moyva/Art/PixelMapCreator`, `Art/Prototype`** | `Moyva/Art/_Prototype/` (лише перенос під Art, вміст не міняємо) | Mixed | Низький | GUID | Prototype виключено з реструктуризації |
| **`Plugins/` (Odin, Zenject)** | **без змін** | DLL/код | — | Ліцензія Odin, спец-папка | Виняток безпеки |
| **`TextMesh Pro/`** | **без змін** | TMP пак | — | TMP Settings/Resources | Виняток безпеки |
| **`_Recovery/`, `DefaultNetworkPrefabs.asset`** | **без змін** | Службові | — | Netcode/Unity | Спец-файли |

> Повна пофайлова таблиця (усі ~14 000 переміщуваних файлів) не наводиться поіменно — переміщення виконується покатегорійно через `AssetDatabase.MoveAsset`, що зберігає GUID для кожного файлу автоматично. Пофайловий лог буде згенеровано на етапі виконання.

---

# Implementation Steps

### Крок 0 — Передумови й бекап
- **Опис:** Переконатись у чистому робочому дереві Git, зробити коміт/тег «before-reorg». Закрити всі сцени, крім однієї. Переконатись, що консоль без помилок компіляції.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** No

### Крок 1 — Створити цільовий каркас папок
- **Опис:** Створити порожні цільові папки (`Moyva/Data/ScriptableObjects`, `Moyva/UI/*`, `Moyva/Art/{Models,Materials,Textures,Sprites,Animations,Shaders,VFX}`, `Moyva/Settings`, `Moyva/Data/Configs`, `Assets/ThirdParty/*`) через `AssetDatabase.CreateFolder`.
- **Assigned role:** developer
- **Dependencies:** Крок 0
- **Parallelizable:** No

### Крок 2 — Перемістити НИЗЬКОРИЗИКОВІ asset'и (без hardcoded-шляхів)
- **Опис:** Через `AssetDatabase.MoveAsset` перенести матеріали, шейдери, моделі, спрайти, root loose art-файли у `Moyva/Art/*`; сторонні art/audio-пакети (KayKit, Honeti, PolyOne, Blink, NaPH, UI Soundpack) у `ThirdParty/`. Після кожної групи — `AssetDatabase.Refresh`.
- **Assigned role:** developer
- **Dependencies:** Крок 1
- **Parallelizable:** Yes (незалежні групи)

### Крок 3 — Перемістити СЕРЕДНЬОРИЗИКОВІ сторонні пакети з asmdef
- **Опис:** Перенести FlatKit, Stylized Water 3, TileWorldCreator у `ThirdParty/`. Після кожного — дочекатись рекомпіляції, перевірити консоль на помилки asmdef/шейдерів.
- **Assigned role:** developer
- **Dependencies:** Крок 2
- **Parallelizable:** No (потрібна перевірка компіляції між пакетами)

### Крок 4 — Перейменування SO → Data/ScriptableObjects + синхронне оновлення коду
- **Опис:** Перемістити `Moyva/SO/*` у `Moyva/Data/ScriptableObjects/*`. **У тому ж кроці** оновити ВСІ рядкові константи в C# (`MoyvaProjectSettingsSO.DefaultAssetPath`, `EconomyResourceEditorShared`, Bootstrap-вікна, `SceneMusicEditorExtension`, `CameraSettingsEditorWindow`, тестові шляхи тощо). Перемістити `SO/URP` → `Settings`.
- **Assigned role:** developer
- **Dependencies:** Крок 3
- **Parallelizable:** No

### Крок 5 — Реорганізація Prefabs (курованих) + Audio + Configs + UI
- **Опис:** Перерозподілити `Prefabs/Objects` → `Environment`/`Systems`; оновити const у Registry-вікнах. Перенести `Art/UI` → `Moyva/UI/*`. Перенести Audio у `Music/SFX` + оновити `MixerPath`. Перенести `calendar_config.dat`, `link.xml`, конфіги у `Data/Configs` (перевірити, що `link.xml` усе ще підхоплюється — за потреби лишити в спец-місці).
- **Assigned role:** developer
- **Dependencies:** Крок 4
- **Parallelizable:** No

### Крок 6 — Оновити FolderSetupMenu та EnsureFolder-виклики
- **Опис:** Синхронізувати `FolderSetupMenu.cs` та всі `EnsureFolder("Assets/Moyva/...")` з новою структурою, щоб авто-створення папок не відроджувало стару ієрархію.
- **Assigned role:** developer
- **Dependencies:** Крок 4, Крок 5
- **Parallelizable:** No

### Крок 7 — Фінальна валідація
- **Опис:** Див. розділ Verification & Testing.
- **Assigned role:** developer
- **Dependencies:** Кроки 2–6
- **Parallelizable:** No

---

# Verification & Testing
1. **Компіляція:** Console без помилок/ворнінгів після кожного кроку переміщення коду.
2. **Missing references:** Скрипт-аудит по всіх сценах і префабах на `m_Script: {fileID: 0}` та відсутні GUID. Порівняти кількість до/після.
3. **Resources.Load:** Play-mode перевірка, що `MoyvaAudioRegistry` та `MoyvaSceneAudioOverrides` завантажуються (кнопковий звук, музика сцени).
4. **Shader.Find:** Візуальна перевірка сцен (`Gamplay_Scene`, `HomeMenu`) — немає рожевих (broken) матеріалів.
5. **Editor tools:** Відкрити RegistryHub, BuildingDesigner, UnitDesigner, EconomyDesigner, GraphEditor — переконатись, що вони знаходять asset'и за новими шляхами.
6. **Tests:** Прогнати EditMode/PlayMode тести (`Kruty1918.Moyva.Tests.*`), особливо `Tests/Generator` (містять hardcoded-шляхи).
7. **Build:** Тестова збірка StandaloneLinux64 (перевірка IL2CPP `link.xml` та Netcode `DefaultNetworkPrefabs`).
8. **Git diff:** Переконатись, що переміщення відображаються як renames зі збереженими .meta (GUID незмінні).

---

# Підсумок
- **Файлів до переміщення (оцінка):** ≈ 14 000 (весь Moyva-контент крім Prototype/Resources/Generated ≈ 3 500 + сторонні пакети ≈ 3 500, решта — Prototype 7 000+ **не чіпається**).
- **Файлів C# з правками шляхів:** ~46.
- **Файли/папки, які НЕ можна безпечно переносити (лишаються):**
  - `Assets/Plugins/` (Odin Inspector — ліцензія/спец-папка; Zenject — DLL/Editor).
  - `Assets/TextMesh Pro/` (TMP Settings/Resources).
  - `Moyva/Resources/` (назва обов'язкова для `Resources.Load`).
  - `Moyva/Generated/` (hardcoded у коді, згенерований контент).
  - `Moyva/Prefabs/Prototype`, `Art/PixelMapCreator`, `Art/Prototype` (виключено рішенням).
  - `Assets/_Recovery/`, `DefaultNetworkPrefabs.asset` (службові).
- **Можливі проблеми:**
  1. Пропущений hardcoded-шлях → NRE у Editor-вікні або тесті (мітигація: повний grep-аудит перед і після).
  2. Переміщення пакетів з asmdef може тимчасово зламати компіляцію до Refresh (мітигація: покроково + перевірка консолі).
  3. `link.xml` при переміщенні може перестати впливати на IL2CPP strip (мітигація: перевірити збіркою; за потреби лишити на місці).
  4. Дублікати імен (`DataBiomesSettings`, `DataNoiseSettings`, `EditorPreviewSettings`) — після зведення в одну папку можлива колізія; **не видаляємо**, розводимо підпапками.
  5. Ручні переміщення поза Unity зламали б GUID — виконуємо ВИКЛЮЧНО через `AssetDatabase.MoveAsset`.
- **Поетапний план:** Кроки 0–7 вище (бекап → каркас → низький ризик → сторонні asmdef → SO+код → Prefabs/Audio/UI → FolderSetup → валідація).

> **Очікую вашого підтвердження перед виконанням. На цьому етапі проєкт не змінено.**
