# Construction UI — Scaffold UI для будівництва

← [Назад до Construction](../construction.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/construction-ui)

---

## Призначення

Модуль **Construction UI** — це готовий scaffold UI-шару для системи будівництва.
Він відокремлює відображення та взаємодію з гравцем від ігрової логіки (`IConstructionService`).

Щоб почати використовувати: додай компоненти до сцени, підключи кнопки/лейбли в Inspector,
а вся логіка передасться до `IConstructionService` автоматично.

---

## Архітектура

```
Construction/
└── UI/
    ├── Kruty1918.Moyva.Construction.UI.asmdef
    │
    ├── ConstructionUIController.cs     ← головний адаптер UI ↔ IConstructionService
    ├── BuildingSelectionPanelUI.cs     ← панель вибору будівель
    ├── BuildingButtonUI.cs             ← компонент для кнопки окремої будівлі
    ├── ConstructionActionBarUI.cs      ← кнопки Confirm / Cancel / Undo / Redo
    ├── ConstructionStatusUI.cs         ← відображення стану preview/сесії
    ├── ConstructionUIInstaller.cs      ← Zenject-інсталер для UI модуля
    │
    ├── BuildingListItemData.cs         ← DTO: дані для кнопки будівлі
    └── ConstructionUIState.cs          ← snapshot поточного UI-стану
```

**Залежності:** `Kruty1918.Moyva.Construction` (API) + `Kruty1918.Moyva.Signals` + `Zenject`

Бізнес-логіка (`ConstructionService`, `WallPlacementService` тощо) залишається в модулі
`Kruty1918.Moyva.Construction` і не залежить від цього UI модуля.

---

## Покроковий гайд підключення в Unity

### Крок 1 — Ієрархія GameObject

Рекомендована структура в сцені:

```
ConstructionUI (GameObject)
├── [ConstructionUIController]      ← головний компонент
├── [ConstructionUIInstaller]       ← Zenject installer
│
├── BuildingSelectionPanel (UI Panel)
│   ├── [BuildingSelectionPanelUI]
│   └── Content (VerticalLayoutGroup) ← itemContainer
│
├── ActionBar (UI Panel)
│   ├── [ConstructionActionBarUI]
│   ├── ConfirmButton  (Button)
│   ├── CancelButton   (Button)
│   ├── UndoButton     (Button)
│   └── RedoButton     (Button)
│
└── StatusPanel (UI Panel)
    ├── [ConstructionStatusUI]
    ├── StateLabel     (Text)
    ├── BuildingLabel  (Text)
    └── PreviewLabel   (Text)
```

### Крок 2 — ConstructionUIController

1. Додай компонент `ConstructionUIController` до кореневого GameObject.
2. Перетягни в Inspector:
   - `selectionPanel` → `BuildingSelectionPanelUI`
   - `actionBar`      → `ConstructionActionBarUI`
   - `statusDisplay`  → `ConstructionStatusUI`

### Крок 3 — BuildingSelectionPanelUI

1. Додай компонент `BuildingSelectionPanelUI` до панелі.
2. Перетягни в Inspector:
   - `itemContainer` → Transform (наприклад VerticalLayoutGroup Content)
   - `buttonPrefab`  → prefab кнопки будівлі (має містити `BuildingButtonUI`)

**Як створити prefab кнопки будівлі:**
1. GameObject → UI → Button
2. Додай компонент `BuildingButtonUI`
3. Підключи `label` → дочірній Text компонент
4. Збережи як prefab

### Крок 4 — ConstructionActionBarUI

1. Додай компонент `ConstructionActionBarUI` до панелі з кнопками.
2. Перетягни в Inspector:
   - `confirmButton` → Button "Підтвердити"
   - `cancelButton`  → Button "Скасувати"
   - `undoButton`    → Button "Відмінити"
   - `redoButton`    → Button "Повторити"

> Кнопки стають неактивними (interactable = false) коли `State == Idle`.

**Альтернатива (без ActionBarUI):** Підключи кнопки безпосередньо до методів
`ConstructionUIController` через Button → OnClick у Inspector:
- Confirm → `ConstructionUIController.OnConfirmClicked()`
- Cancel  → `ConstructionUIController.OnCancelClicked()`
- Undo    → `ConstructionUIController.OnUndoClicked()`
- Redo    → `ConstructionUIController.OnRedoClicked()`

### Крок 5 — ConstructionStatusUI

1. Додай компонент `ConstructionStatusUI` до панелі статусу.
2. Перетягни в Inspector (всі необов'язкові):
   - `placementStateLabel`   → Text для стану (`Idle` / `Placing` / `Confirmed`)
   - `selectedBuildingLabel` → Text для назви вибраної будівлі
   - `previewStateLabel`     → Text для стану preview (`✓ Valid` / `✗ Blocked` / `--`)

### Крок 6 — Zenject Installer

1. Додай компонент `ConstructionUIInstaller` до SceneContext GameObject.
2. Перетягни `ConstructionUIController` у поле `uiController`.
3. Додай `ConstructionUIInstaller` до списку **Mono Installers** у SceneContext.

> Переконайся, що `ConstructionInstaller` (runtime) також доданий до SceneContext.

---

## Підключення кліків по тайлу

Щоб preview працював по кліку гравця на карту:

```csharp
// У твоєму TileClickHandler або InputHandler:
[Inject] private ConstructionUIController _uiController;

private void OnTileClicked(Vector2Int gridPosition)
{
    _uiController.OnTileSelected(gridPosition);
}
```

`ConstructionUIController.OnTileSelected` → `IConstructionService.TryPreviewAt` → `BuildingPreviewChangedSignal`.

---

## Сигнали, на які реагує UI

| Сигнал | Реакція UI |
|---|---|
| `BuildingPreviewChangedSignal` | Оновлює `previewStateLabel` та стан action bar |
| `BuildingPlacedSignal` | Оновлює стан action bar та статус |
| `BuildingCancelledSignal` | Скидає вибір будівлі та preview, оновлює всі лейбли |

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
    public bool IsPlacing { get; }
    public bool HasSelection { get; }
}
```

---

## Власні контролери / розширення

Ти можеш замінити або розширити будь-який sub-компонент:
- Замінити `UnityEngine.UI.Text` на `TextMeshProUGUI` (зміни тип поля у скрипті)
- Додати категорійні фільтри у `BuildingSelectionPanelUI`
- Підключити анімації через `OnConfirmClicked` / `OnCancelClicked` події
- Замінити `BuildingButtonUI` на власний компонент з іконкою

---

## Залежності модуля

| Залежність | Звідки береться |
|---|---|
| `IConstructionService` | `ConstructionInstaller` |
| `IBuildingRegistry` | `ConstructionInstaller` |
| `SignalBus` | SceneContext сигнальні декларації |

---

## Пов'язані документи

- [construction/service.md](service.md) — `IConstructionService` API
- [construction/registry.md](registry.md) — `BuildingRegistrySO` та `BuildingDefinition`
- [signals.md](../signals.md) — будівельні сигнали
