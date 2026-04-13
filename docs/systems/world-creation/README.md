# World Creation UI — Екран створення нового світу

← [Назад до систем](../README.md)

---

## Призначення

Модуль **WorldCreation** — повний scaffold UI-шару для налаштування та запуску нової ігрової сесії.
Дозволяє гравцеві сконфігурувати всі параметри світу перед запуском генератора.

Гравець може:

| Категорія | Дія |
|---|---|
| **Основні параметри** | Ввести назву світу |
| | Ввести seed або згенерувати випадковий |
| | Вибрати розмір карти (Small / Medium / Large / Custom) |
| | Задати власний розмір (ширина × висота) при виборі Custom |
| | Вибрати тип карти (Balanced / Continental / Island / Mountain / Plains) |
| **Правила гри** | Вибрати рівень складності (Easy / Normal / Hard / Brutal) |
| | Увімкнути/вимкнути ботів |
| | Задати кількість людських гравців (1–4) |
| | Задати кількість ботів (0–4) |
| | Задати стартові ресурси (золото, їжа) |
| **Генерація** | Налаштувати щільність лісів / гір / водойм / POI (слайдери 0..1) |
| | Увімкнути/вимкнути генерацію річок |
| | Увімкнути/вимкнути генерацію біомів |
| | Увімкнути/вимкнути WFC-полірування тайлів |
| **Дії** | Скинути всі поля до стандартних значень |
| | Натиснути «Створити світ» (з валідацією) |
| | Натиснути «Скасувати» (повернутись у головне меню) |

---

## Архітектура

```
Features/WorldCreation/
├── API/
│   ├── IWorldCreationService.cs        ← контракт сервісу
│   ├── WorldCreationConfig.cs          ← багатий C# клас конфігу з валюються-зручними властивостями
│   ├── WorldCreationDefaultsSO.cs      ← ScriptableObject із типовими значеннями для дизайнерів
│   ├── WorldSizePreset.cs              ← enum: Small / Medium / Large / Custom
│   ├── MapTypePreset.cs                ← enum: Balanced / Continental / Island / Mountain / Plains
│   └── DifficultyLevel.cs             ← enum: Easy / Normal / Hard / Brutal
│
├── Runtime/
│   ├── Kruty1918.Moyva.WorldCreation.asmdef   ← залежить від Signals + Zenject
│   ├── WorldCreationService.cs         ← реалізація IWorldCreationService (чистий C#)
│   └── WorldCreationInstaller.cs       ← Zenject MonoInstaller
│
└── UI/
    ├── Kruty1918.Moyva.WorldCreation.UI.asmdef  ← залежить від WorldCreation + Signals + TextMeshPro
    ├── WorldCreationUIController.cs    ← головний адаптер UI ↔ IWorldCreationService
    └── WorldCreationUIInstaller.cs     ← Zenject MonoInstaller
```

**Залежності збірок:**

| Збірка | Залежить від |
|---|---|
| `Kruty1918.Moyva.WorldCreation` | `Kruty1918.Moyva.Signals`, `Zenject` |
| `Kruty1918.Moyva.WorldCreation.UI` | `Kruty1918.Moyva.WorldCreation`, `Kruty1918.Moyva.Signals`, `Zenject`, `Unity.TextMeshPro` |

**Сигнали:**

| Сигнал | Де оголошено | Хто надсилає | Хто отримує |
|---|---|---|---|
| `WorldCreationConfirmedSignal` | `Signals/API/OnWorldCreationSignals.cs` | `WorldCreationUIController` | SceneLoader / Bootstrap |
| `WorldCreationCancelledSignal` | `Signals/API/OnWorldCreationSignals.cs` | `WorldCreationUIController` | SceneLoader / MainMenuController |

---

## Потік даних

```
Гравець → UI поля
    ↓
WorldCreationUIController.ReadConfigFromUI()
    ↓
IWorldCreationService.UpdateConfig(config)      ← зберігає стан
    ↓
[«Створити світ»]
    ↓
IWorldCreationService.ValidateConfig(config)    ← перевіряє обмеження
    ↓ (якщо валідний)
IWorldCreationService.ToSignalData(config)      ← конвертує в плоску структуру
    ↓
SignalBus.Fire(WorldCreationConfirmedSignal)     ← сповіщає Bootstrap / SceneLoader
```

---

## Підключення до сцени (покроково)

### 1. Створи ScriptableObject із дефолтами

`Assets → Create → Moyva → WorldCreation → Defaults`

Налаштуй у Inspector усі типові значення.

### 2. Додай WorldCreationInstaller

```
SceneContext GameObject
└── WorldCreationInstaller (Mono Installer)
       └── defaults → [твій WorldCreationDefaults.asset]
```

### 3. Побудуй UI-ієрархію

```
WorldCreationScreen (Canvas/Panel)
└── WorldCreationUIController (цей MonoBehaviour)
    ├── worldNameField          ← TMP_InputField
    ├── seedField               ← TMP_InputField
    ├── randomSeedButton        ← Button
    ├── sizePresetDropdown      ← TMP_Dropdown  (варіанти: Small/Medium/Large/Custom)
    ├── customSizeGroup         ← GameObject (показується тільки при Custom)
    │   ├── customWidthField    ← TMP_InputField
    │   └── customHeightField   ← TMP_InputField
    ├── mapTypeDropdown         ← TMP_Dropdown
    ├── difficultyDropdown      ← TMP_Dropdown
    ├── enableBotsToggle        ← Toggle
    ├── botSettingsGroup        ← GameObject (показується тільки якщо боти увімкнені)
    │   ├── humanPlayerCountSlider  ← Slider  (min=1, max=4, wholeNumbers=true)
    │   ├── humanPlayerCountLabel   ← TMP_Text
    │   ├── botCountSlider          ← Slider  (min=0, max=4, wholeNumbers=true)
    │   └── botCountLabel           ← TMP_Text
    ├── startingGoldField       ← TMP_InputField
    ├── startingFoodField       ← TMP_InputField
    ├── forestDensitySlider     ← Slider (min=0, max=1)
    ├── mountainDensitySlider   ← Slider (min=0, max=1)
    ├── waterDensitySlider      ← Slider (min=0, max=1)
    ├── villageDensitySlider    ← Slider (min=0, max=1)
    ├── generateRiversToggle    ← Toggle
    ├── generateBiomesToggle    ← Toggle
    ├── applyWFCToggle          ← Toggle
    ├── createWorldButton       ← Button  «Створити світ»
    ├── cancelButton            ← Button  «Скасувати»
    ├── resetDefaultsButton     ← Button  «Скинути до стандартних»
    └── validationErrorText     ← TMP_Text (прихований за замовчуванням)
```

### 4. Налаштуй Dropdown параметри

**sizePresetDropdown** — варіанти (index = значення enum):
| Index | Текст | WorldSizePreset |
|---|---|---|
| 0 | Малий (32×32) | Small |
| 1 | Середній (64×64) | Medium |
| 2 | Великий (128×128) | Large |
| 3 | Custom | Custom |

**mapTypeDropdown** — варіанти:
| Index | Текст | MapTypePreset |
|---|---|---|
| 0 | Збалансований | Balanced |
| 1 | Континентальний | Continental |
| 2 | Острівний | Island |
| 3 | Гірський | Mountain |
| 4 | Рівнинний | Plains |

**difficultyDropdown** — варіанти:
| Index | Текст | DifficultyLevel |
|---|---|---|
| 0 | Легко | Easy |
| 1 | Нормально | Normal |
| 2 | Важко | Hard |
| 3 | Брутально | Brutal |

### 5. Додай WorldCreationUIInstaller

```
SceneContext GameObject
└── WorldCreationUIInstaller (Mono Installer)
       └── uiController → [WorldCreationUIController зі сцени]
```

### 6. Оброби сигнал підтвердження

Підпишись на `WorldCreationConfirmedSignal` у своєму Bootstrap або SceneLoader:

```csharp
[Inject]
public void Construct(SignalBus signalBus)
{
    signalBus.Subscribe<WorldCreationConfirmedSignal>(OnWorldCreationConfirmed);
}

private void OnWorldCreationConfirmed(WorldCreationConfirmedSignal signal)
{
    var data = signal.Config;
    // data.WorldName, data.Seed, data.SizePresetIndex, etc.
    // Завантаж ігрову сцену / запусти генератор з цими параметрами
}
```

---

## API

### `IWorldCreationService`

```csharp
WorldCreationConfig CurrentConfig { get; }
void UpdateConfig(WorldCreationConfig config);
void ResetToDefaults();
int  GenerateRandomSeed();   // зберігає seed у CurrentConfig, повертає значення
bool ValidateConfig(WorldCreationConfig config, out string errorMessage);
WorldCreationConfigData ToSignalData(WorldCreationConfig config);
```

### `WorldCreationConfig` — основні властивості

| Властивість | Тип | За замовчуванням | Опис |
|---|---|---|---|
| `WorldName` | `string` | `"Новий світ"` | Назва світу |
| `Seed` | `int` | `0` | 0 = автогенерація перед підтвердженням |
| `SizePreset` | `WorldSizePreset` | `Medium` | Розмір карти |
| `CustomWidth` | `int` | `64` | Актуально лише для Custom |
| `CustomHeight` | `int` | `64` | Актуально лише для Custom |
| `MapType` | `MapTypePreset` | `Balanced` | Тип карти |
| `Difficulty` | `DifficultyLevel` | `Normal` | Складність |
| `EnableBots` | `bool` | `true` | Увімкнення ботів |
| `HumanPlayerCount` | `int` | `1` | 1–4 |
| `BotCount` | `int` | `1` | 0–4 |
| `StartingGold` | `int` | `200` | Ресурс: золото |
| `StartingFood` | `int` | `100` | Ресурс: їжа |
| `ForestDensity` | `float` | `0.4` | 0..1 |
| `MountainDensity` | `float` | `0.3` | 0..1 |
| `WaterDensity` | `float` | `0.25` | 0..1 |
| `VillageDensity` | `float` | `0.2` | 0..1 |
| `GenerateRivers` | `bool` | `true` | |
| `GenerateBiomes` | `bool` | `true` | |
| `ApplyWFC` | `bool` | `true` | |
| `ResolvedWidth` *(read-only)* | `int` | — | Фактична ширина з урахуванням пресету |
| `ResolvedHeight` *(read-only)* | `int` | — | Фактична висота з урахуванням пресету |
| `TotalFactions` *(read-only)* | `int` | — | `HumanPlayerCount + BotCount` |

### Валідація

`ValidateConfig` перевіряє:
- Назва не порожня і ≤ 64 символів
- Custom-розмір у діапазоні 16–512
- HumanPlayerCount: 1–4
- BotCount: 0–4
- Якщо EnableBots = false, BotCount = 0
- TotalFactions ≥ 2
- StartingGold / StartingFood ≥ 0

---

## Сигнали

### `WorldCreationConfirmedSignal`
```csharp
public struct WorldCreationConfirmedSignal
{
    public WorldCreationConfigData Config;
}
```
Оголошений у `Kruty1918.Moyva.Signals` як `.OptionalSubscriber()`.

### `WorldCreationCancelledSignal`
```csharp
public struct WorldCreationCancelledSignal { }
```
Оголошений у `Kruty1918.Moyva.Signals` як `.OptionalSubscriber()`.

### `WorldCreationConfigData` (payload структура)
Плоска структура з примітивними полями — передається в сигналі щоб Signals assembly не залежав від WorldCreation assembly:

```
WorldName · Seed · SizePresetIndex · CustomWidth · CustomHeight
MapTypePresetIndex · DifficultyIndex · EnableBots · HumanPlayerCount · BotCount
StartingGold · StartingFood · ForestDensity · MountainDensity · WaterDensity
VillageDensity · GenerateRivers · GenerateBiomes · ApplyWFC
```

---

## Примітки для дизайнерів

- **WorldCreationDefaultsSO** — єдине місце для зміни типових значень без правки коду.
  Якщо поле `defaults` не призначено в WorldCreationInstaller, сервіс використовує
  вбудовані C#-дефолти з `WorldCreationConfig`.
- **customSizeGroup** та **botSettingsGroup** — вмикаються/вимикаються автоматично.
  Не потрібно додавати свою логіку show/hide.
- Усі поля є null-safe: відсутнє посилання логується попередженням, а не кидає exception.
