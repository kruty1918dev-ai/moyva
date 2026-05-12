# Система детальної інформації про замок

## Опис функціональності

Коли гравець клікає на замок (капітал), він бачить:

1. **Основна статистика** - населення, всього будівель, складів, амбарів, домів
2. **Загальні ресурси** - список всіх ресурсів у поселенні
3. **Список будівель** - з кнопками для переміщення камери до кожної будівлі

При кліку на кнопку будівлі:
- Камера переміщується на позицію цієї будівлі
- Панель замку закривається
- Якщо клікнути на нову будівлю, інформація оновлюється

## Архітектура

### Компоненти системи

#### 1. Сигнали (`OnBuildingInfoSignals.cs`)
- `CameraFocusBuildingSignal` - сигнал для фокусування камери на будівлю

#### 2. Утиліта статистики (`CastleSettlementStatistics.cs`)
- `CalculateStatistics()` - обчислення статистики поселення
- `FormatCastleInfoDetailed()` - форматування детальної інформації з іконками

#### 3. Presenter (`BuildingWorldInfoPresenter.cs`)
- Розширена логіка для замків
- Інжектує `EconomyManager` для отримання повного стану поселення

#### 4. Services

**CameraFocusService** - обробляє сигнал переміщення камери:
```csharp
private void OnCameraFocusBuilding(CameraFocusBuildingSignal signal)
{
    _signalBus.Fire(new WorldInfoPanelClosedSignal()); // Закрити панель
    _cameraMovement.ForceMoveCameraToPosition(worldPosition); // Переміститися
}
```

**CastleDetailedInfoPresenter** - логіка для детальної панелі замку

#### 5. UI Controller (`CastleDetailedInfoPanelController.cs`)
- Отримує сигнали про запит інформації
- Парсить список будівель з контенту
- Створює кнопки програмно
- Обробляє клік на кнопку будівлі

### Потік даних

```
Клік на замок
    ↓
BuildingInfoPanelRequestedSignal (від BuildingWorldInfoPresenter)
    ↓
CastleDetailedInfoPanelController.OnWorldInfoRequested()
    ↓
Парсування списку будівель → Створення кнопок
    ↓
Клік на кнопку будівлі
    ↓
CameraFocusBuildingSignal
    ↓
CameraFocusService обробляє → Закриває панель + переміщує камеру
```

## Конфігурація

### WorldUIConfigSO

Додано поле:
```csharp
[SerializeField] private GameObject _castleDetailedPanelPrefab;
public GameObject CastleDetailedPanelPrefab => _castleDetailedPanelPrefab;
```

### Структура UI префаба для замку

Префаб повинен мати наступну структуру:

```
CastleDetailedPanel (Canvas)
├── CastleNameText (TMP_Text) - назва замку
├── StatisticsText (TMP_Text) - статистика
├── BuildingsListContainer (Transform) - контейнер для списку
│   └── (динамічно створені Button елементи)
└── CloseButton (Button) - кнопка закриття
```

## Розширення в майбутньому

1. **Категоризація будівель** - сортування по типу (склади, амбари, дома)
2. **Фільтрування** - показ тільки певного типу будівель
3. **Анімація списку** - плавна прокрутка при багатьох будівлях
4. **Детальні кнопки** - показ додаткової інформації на кнопках (кількість робітників, стан)
5. **Переміщення до поселення** - кнопка для переміщення до центру поселення (ратуші)

## Тестування

Система готова до тестування з наступних аспектів:

1. ✅ Компіляція - без помилок
2. ⚠️ UI префаб - потребує створення у сцені
3. ⚠️ Обробка сигналів - потребує запуску гри
4. ⚠️ Динамічне створення кнопок - працює програмно
5. ⚠️ Переміщення камери - залежить від CameraMovement реалізації

## Примітки реалізації

### Парсування списку будівель

```csharp
// Формат рядка в контенту:
// 📦 Склад (10, 20)
// 🌾 Амбар (15, 25)
// 🏠 Хата (20, 30)

// Парсер шукає:
// 1. Іконку (перший символ)
// 2. Назву
// 3. Позицію в дужках (x, y)
```

### Динамічне створення UI

Кнопки створюються програмно для максимальної гнучкості:
- Не потребують префаба
- Автоматично адаптуються до розміру контейнера
- Налаштування можна легко змінити в `CreateBuildingItemButton()`

## API для інших модулів

### CastleDetailedInfoPresenter

```csharp
/// Навігувати до будівлі
public void NavigateToBuildingAt(Vector2Int position, string buildingId)
```

### CastleSettlementStatistics

```csharp
/// Обчислити статистику
public static CastleStatistics CalculateStatistics(...)

/// Отримати форматовану рядок
public static string FormatCastleInfoDetailed(...)
```

## Залежності

- `Zenject` - для інжекції залежностей
- `TextMeshPro` - для UI тексту
- `UnityEngine.UI` - для кнопок та панелей
- `IEconomyInfoMediator` - для отримання економічних даних
- `ICameraMovement` - для переміщення камери
