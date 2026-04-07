# Construction UI — Scaffold UI для будівництва

← [Назад до Construction](../construction.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/construction-ui)

---

## Призначення

Модуль **Construction UI** — це повний scaffold UI-шару для системи будівництва.
Він відокремлює відображення та взаємодію з гравцем від ігрової логіки (`IConstructionService`).

Функціональність:
- **Перемикання режиму**: кнопка на основному UI → будівельний UI відкривається, основний ховається.
- **Вибір будівлі за категорією**: вкладки (Військові / Цивільні / Індустріальні); іконка вибраної будівлі збільшується.
- **Preview на тайлі**: клік по тайлу → preview (зелений = вільно, червоний = зайнято).
- **Підтвердження / Скасування / Undo / Redo** розміщення.
- **Режим знесення**: знищити тільки власноруч поставлені будівлі.
- **Null-safe**: відсутні посилання логуються попередженням, а не виключенням.

---

## Архітектура

```
Construction/
└── UI/
    ├── Kruty1918.Moyva.Construction.UI.asmdef     ← збірка: залежить від GameMode + TextMeshPro
    │
    ├── ConstructionUIController.cs     ← головний адаптер UI ↔ IConstructionService ↔ IGameModeService
    ├── BuildingSelectionPanelUI.cs     ← панель вибору будівель з фільтром за категорією
    ├── BuildingCategoryTabsUI.cs       ← вкладки категорій (Всі / Військові / Цивільні / …)
    ├── BuildingButtonUI.cs             ← кнопка окремої будівлі (іконка + назва + виділення)
    ├── ConstructionActionBarUI.cs      ← Підтвердити / Скасувати / Відмінити / Повторити / Знести
    ├── ConstructionStatusUI.cs         ← відображення стану preview/сесії (TextMeshProUGUI)
    ├── ConstructionUIInstaller.cs      ← Zenject-інсталер для UI модуля
    │
    ├── BuildingListItemData.cs         ← DTO: id, назва, категорія, іконка (Sprite)
    └── ConstructionUIState.cs          ← snapshot поточного UI-стану (immutable)
```

**Залежності збірки:**
`Kruty1918.Moyva.Construction` · `Kruty1918.Moyva.Signals` · `Kruty1918.Moyva.GameMode` · `Zenject` · `Unity.TextMeshPro`

Бізнес-логіка (`ConstructionService`, `WallPlacementService` тощо) залишається в модулі
`Kruty1918.Moyva.Construction` і **не залежить** від цього UI модуля.

---

## Покроковий гайд підключення в Unity

### Крок 1 — Ієрархія GameObject

Рекомендована структура в сцені:

```
GameplayUI (GameObject)           ← gameplayUIRoot — ховається в режимі будівництва
└── ... (основний ігровий UI)
    └── BuildButton (Button)      ← OnClick → ConstructionUIController.EnterConstructionMode()

ConstructionUI (GameObject)       ← constructionUIRoot — показується в режимі будівництва
├── [ConstructionUIController]    ← головний компонент
├── [ConstructionUIInstaller]     ← Zenject installer
│
├── CategoryTabs (UI Panel)
│   └── [BuildingCategoryTabsUI]
│       ├── tabContainer (HorizontalLayoutGroup)
│       └── tabButtonPrefab ← Button - TextMeshPro (без BuildingCategoryTabsUI на prefab)
│
├── BuildingSelectionPanel (UI Panel)
│   ├── [BuildingSelectionPanelUI]
│   └── Content (VerticalLayoutGroup)  ← itemContainer
│
├── ActionBar (UI Panel)
│   ├── [ConstructionActionBarUI]
│   ├── ConfirmButton  (Button)
│   ├── CancelButton   (Button)
│   ├── UndoButton     (Button)
│   ├── RedoButton     (Button)
│   └── DemolishButton (Button)  ← необов'язковий
│
└── StatusPanel (UI Panel)
    ├── [ConstructionStatusUI]
    ├── StateLabel     (TextMeshProUGUI)
    ├── BuildingLabel  (TextMeshProUGUI)
    └── PreviewLabel   (TextMeshProUGUI)
```

### Крок 2 — ConstructionUIController

1. Додай компонент `ConstructionUIController` до кореневого `ConstructionUI` GameObject.
2. Перетягни в Inspector:
   - `selectionPanel`     → `BuildingSelectionPanelUI`
   - `actionBar`          → `ConstructionActionBarUI`
   - `statusDisplay`      → `ConstructionStatusUI`
   - `gameplayUIRoot`     → кореневий GameObject основного UI (наприклад `GameplayUI`)
   - `constructionUIRoot` → `ConstructionUI` GameObject (або залиш null — буде використано gameObject)
3. На кнопці «Будівництво» основного UI: `Button → OnClick → ConstructionUIController.EnterConstructionMode()`.

### Крок 3 — BuildingSelectionPanelUI

1. Додай компонент `BuildingSelectionPanelUI` до панелі.
2. Перетягни в Inspector:
   - `itemContainer` → Transform з VerticalLayoutGroup (для кнопок будівель)
   - `buttonPrefab`  → prefab кнопки будівлі (має містити `BuildingButtonUI`)
   - `categoryTabs`  → `BuildingCategoryTabsUI` (необов'язково — для фільтрації)

**Як створити prefab кнопки будівлі:**
1. GameObject → UI → Button - TextMeshPro
2. Додай компонент `BuildingButtonUI`
3. Підключи `label` → дочірній `TextMeshProUGUI` компонент
4. Додай дочірній `Image` для іконки; підключи до поля `iconImage`
5. `selectedScale` — масштаб при виборі (за замовчуванням 1.15)
6. Збережи як prefab і перетягни у поле `buttonPrefab`

### Крок 4 — BuildingCategoryTabsUI

1. Додай компонент `BuildingCategoryTabsUI` до панелі вкладок.
2. Перетягни в Inspector:
   - `tabContainer`    → Transform (HorizontalLayoutGroup)
   - `tabButtonPrefab` → простий Button - TextMeshPro prefab (без спеціального компонента)
   - `activeColor`     → колір активної вкладки
   - `inactiveColor`   → колір неактивної вкладки
3. Підключи цей компонент до поля `categoryTabs` у `BuildingSelectionPanelUI`.
   Вкладки генеруються автоматично при виклику `Populate`.

### Крок 5 — ConstructionActionBarUI

1. Додай компонент `ConstructionActionBarUI` до панелі з кнопками.
2. Перетягни в Inspector:
   - `confirmButton`  → Button «Підтвердити»
   - `cancelButton`   → Button «Скасувати»
   - `undoButton`     → Button «Відмінити»
   - `redoButton`     → Button «Повторити»
   - `demolishButton` → Button «Знести» (необов'язковий)

> Кнопки Confirm/Undo/Redo стають неактивними в режимі знесення або коли State == Idle.
> Кнопка Знести активна завжди поки відкритий будівельний режим.

**Альтернатива (без ActionBarUI):** Підключи кнопки напряму через Button → OnClick:
- Confirm → `ConstructionUIController.OnConfirmClicked()`
- Cancel  → `ConstructionUIController.OnCancelClicked()`
- Undo    → `ConstructionUIController.OnUndoClicked()`
- Redo    → `ConstructionUIController.OnRedoClicked()`
- Знести  → `ConstructionUIController.OnDemolishToggled()`

### Крок 6 — ConstructionStatusUI

1. Додай компонент `ConstructionStatusUI` до панелі статусу.
2. Перетягни в Inspector (всі необов'язкові):
   - `placementStateLabel`   → `TextMeshProUGUI` для стану (`Idle` / `Placing` / `Confirmed`)
   - `selectedBuildingLabel` → `TextMeshProUGUI` для назви вибраної будівлі
   - `previewStateLabel`     → `TextMeshProUGUI` для стану preview (`✓ Дійсно` / `✗ Заблоковано` / `--`)

### Крок 7 — Zenject Installer

1. Додай компонент `ConstructionUIInstaller` до SceneContext GameObject.
2. Перетягни `ConstructionUIController` у поле `uiController`.
3. Додай `ConstructionUIInstaller` до списку **Mono Installers** у SceneContext.

> Переконайся, що `ConstructionInstaller` (runtime) та `GameModeInstaller` також додані до SceneContext.

---

## Іконки будівель

Щоб відображати іконки у меню будівництва:

1. Відкрий `BuildingRegistrySO` (ScriptableObject реєстру будівель).
2. Для кожної будівлі заповни поле **Icon** → `Sprite` (іконка 64×64 або 128×128 px).
3. Якщо Icon не задано — `iconImage` на кнопці приховується автоматично.

---

## Режим знесення

- Гравець натискає кнопку «Знести» → `IConstructionService.ToggleDemolishMode()`.
- В режимі знесення клік по тайлу → `IConstructionService.TryDemolishAt(position)`.
- **Лише будівлі, підтверджені гравцем під час поточної сесії гри**, можуть бути знесені.
- При успішному знесенні надсилається `BuildingDemolishedSignal { BuildingId, Position }`.
- Якщо будівля не є гравецькою — повертається `false` та логується попередження.

---

## Підключення кліків по тайлу

```csharp
// У TileClickHandler або InputHandler:
[Inject] private ConstructionUIController _uiController;

private void OnTileClicked(Vector2Int gridPosition)
{
    _uiController.OnTileSelected(gridPosition);
}
```

`OnTileSelected` автоматично визначає режим:
- Якщо `IsDemolishMode` → `TryDemolishAt(position)`
- Інакше → `TryPreviewAt(position)` → `BuildingPreviewChangedSignal`

---

## Сигнали, на які реагує UI

| Сигнал | Реакція UI |
|---|---|
| `GameModeChangedSignal` | Показує/ховає будівельний та основний UI |
| `BuildingPreviewChangedSignal` | Оновлює `previewStateLabel` та стан action bar |
| `BuildingPlacedSignal` | Оновлює стан action bar та статус |
| `BuildingCancelledSignal` | Скидає вибір, знімає виділення кнопки, повертає у Normal режим |
| `BuildingDemolishedSignal` | (надсилається сервісом, підписник — спавнер/UI) |

Підписка/відписка керується автоматично через `IInitializable` / `IDisposable`.

---

## DTO / ViewModels

### `BuildingListItemData`
```csharp
// Immutable UI-DTO для кнопки будівлі
public sealed class BuildingListItemData
{
    public string Id { get; }
    public string DisplayName { get; }
    public BuildingCategory Category { get; }
    public Sprite Icon { get; }  // null якщо не задано
}
```

### `ConstructionUIState`
```csharp
// Snapshot поточного стану UI
public sealed class ConstructionUIState
{
    public BuildingPlacementState PlacementState { get; }
    public string SelectedBuildingId { get; }
    public BuildingPreviewState LastPreviewState { get; }
    public Vector2Int LastPreviewPosition { get; }
    public bool IsDemolishMode { get; }
    public bool IsConstructionModeActive { get; }
    public bool IsPlacing { get; }        // PlacementState == Placing
    public bool HasSelection { get; }     // SelectedBuildingId != null/empty
}
```

---

## Null-safe поведінка

Усі serialized-поля є необов'язковими. Якщо поле не підключено — компонент логує попередження і продовжує роботу без нього:

```
[ConstructionUIController] Поле 'selectionPanel' не призначено. Меню будівель не відображатиметься.
[ConstructionActionBarUI] Поле 'confirmButton' не призначено на 'ActionBar'.
[BuildingButtonUI] Поле 'label' не призначено на 'BuildingButton'.
```

---

## Залежності модуля

| Залежність | Звідки береться |
|---|---|
| `IConstructionService` | `ConstructionInstaller` |
| `IBuildingRegistry` | `ConstructionInstaller` |
| `IGameModeService` | `GameModeInstaller` |
| `SignalBus` | SceneContext сигнальні декларації |

---

## Пов'язані документи

- [construction/service.md](service.md) — `IConstructionService` API
- [construction/registry.md](registry.md) — `BuildingRegistrySO` та `BuildingDefinition`
- [signals.md](../signals.md) — будівельні сигнали
- [game-mode.md](../game-mode.md) — `IGameModeService`, `GameModeChangedSignal`
