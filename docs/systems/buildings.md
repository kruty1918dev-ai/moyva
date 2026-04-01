# Система будівництва (Buildings Feature)

## Огляд

Система будівництва реалізує меню побудови споруд з трьома розділами (Військові / Цивільні / Індустріальні), підтримкою попереднього перегляду, скасування/підтвердження та особливою логікою для стін.

## Структура

```
Features/Buildings/
├── API/
│   ├── BuildingCategory.cs         # Enum: Military, Civilian, Industrial
│   ├── BuildingConfig.cs           # Дані одного типу будівлі
│   ├── BuildingRegistrySO.cs       # ScriptableObject реєстр всіх будівель
│   ├── IBuildingPlacementService   # API режиму розміщення (сесія, undo/redo)
│   ├── IBuildingService            # API підтверджених будівель
│   ├── IWallService                # API будування стін
│   └── PlacedBuilding.cs          # DTO для підтвердженої будівлі
├── Runtime/
│   ├── BuildingPlacementService.cs # Сесія розміщення + undo/redo
│   ├── BuildingService.cs          # Зберігає підтверджені будівлі
│   ├── WallService.cs              # Точки з'єднання + шлях стін
│   ├── WallConnectionPoint.cs      # MonoBehaviour для кола з'єднання
│   └── BuildingsInstaller.cs       # Zenject MonoInstaller
└── UI/
    ├── BuildingMenuController.cs   # Головна панель меню
    ├── BuildingButtonUI.cs         # Кнопка окремої будівлі
    └── BuildingPreviewController.cs # Ghost-preview під курсором
```

## Налаштування сцени

### 1. ScriptableObject BuildingRegistrySO
- Меню: `Assets → Create → Moyva/Buildings/BuildingRegistry`
- Заповніть список будівель (TypeId, DisplayName, Category, Prefab, PreviewSprite, IsWall)

### 2. BuildingsInstaller (Zenject)
- Додайте `BuildingsInstaller` MonoInstaller до SceneContext
- Призначте `BuildingRegistrySO` в інспекторі
- (Опціонально) Призначте префаб `WallConnectionPointPrefab` для точок з'єднання стін

### 3. SignalBusInstaller
Вже оновлено: реєструє `TileHoveredSignal`, `BuildingPreviewPlacedSignal`, 
`BuildingPreviewRemovedSignal`, `BuildingConstructionConfirmedSignal`, 
`BuildingConstructionCanceledSignal`.

### 4. Canvas UI (BuildingMenuController)
Ієрархія Canvas:
```
Canvas
└── BuildingMenuPanel (BuildingMenuController)
    ├── MilitaryContainer (Vertical Layout Group)
    ├── CivilianContainer (Vertical Layout Group)
    ├── IndustrialContainer (Vertical Layout Group)
    ├── ConfirmButton (Button)
    └── CancelButton (Button)
```
- Призначте посилання на контейнери, кнопки та префаб `BuildingButtonUI`
- Zenject автоматично заін'єктить `IBuildingPlacementService` та `BuildingRegistrySO`

### 5. BuildingPreviewController (Ghost)
- Створіть порожній GameObject у сцені
- Додайте `SpriteRenderer` та `BuildingPreviewController`
- Zenject автоматично заін'єктить залежності

### 6. TileView оновлення
`TileView.OnMouseEnter()` тепер надсилає `TileHoveredSignal` — 
`BuildingPreviewController` слухає цей сигнал для відображення ghost.

## Функціональність

| Дія | Результат |
|-----|-----------|
| Вибрати будівлю в меню | `StartPlacement(typeId)` → вхід у режим розміщення |
| Навести курсор на тайл | Ghost з кольором: білий = вільно, червоний = зайнято |
| Клікнути на тайл | `TryPlace(position)` → розміщення сесійної будівлі |
| Ctrl+Z | `Undo()` → видалення останньої сесійної будівлі |
| Ctrl+Y | `Redo()` → відновлення скасованої будівлі |
| Кнопка "Підтвердити" | `Confirm()` → всі будівлі стають постійними |
| Кнопка "Скасувати" | `Cancel()` → всі сесійні будівлі видаляються |
| Розмістити стіну (IsWall=true) | Навколо неї з'являються 8 кіл з'єднання |
| Клік на коло з'єднання | Розміщує стіну на сусідньому тайлі |
| Drag від кола з'єднання | Прокладає шлях стін до позиції відпускання (алгоритм Брезенхема) |
