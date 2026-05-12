# System Installers - Configuration-based Scene Setup

## Overview

Інсталяція всіх компонентів сцени відбувається через конфіги (ScriptableObject), без необхідності ручного призначення посилань в інспекторі.

## Architecture

```
SceneBootstrapInstaller (основний інсталер)
  ├── SceneBootstrapConfigSO (конфіг сцени)
  │   ├── WorldUIConfigSO (конфіг UI компонентів)
  │   │   └── WorldInfoPanelPrefab
  │   └── GameSessionConfigSO (конфіг гейм-сесії)
  │
  └── Інші інсталери, які читають конфіги з контейнера
      ├── WorldInfoPanelInstaller
      ├── FactionInstaller
      ├── GridInstaller
      └── ... інші
```

## How to Setup

### 1. Create WorldInfoPanel Prefab

1. Перейти в **Assets > Moyva > UI > Create WorldInfoPanel Prefab**
2. Вибрати папку для збереження (наприклад, `Assets/Moyva/Prefabs/UI/`)
3. Префаб буде створений з готовою структурою

### 2. Create WorldUIConfig

1. **Right-click** в Project вікні
2. **Create > Moyva > UI > World UI Config**
3. Назвати `WorldUIConfig`
4. Перетягти **WorldInfoPanel** префаб у поле **World Info Panel Prefab**

### 3. Create SceneBootstrapConfig

1. **Right-click** в Project вікні
2. **Create > Moyva > Scenes > Scene Bootstrap Config**
3. Назвати `SceneBootstrapConfig`
4. Перетягти **WorldUIConfig** префаб у поле **World UI Config Prefab** (якщо це префаб)

### 4. Add to Scene

1. Відкрити сцену
2. На **SceneContext** GameObject:
   - Додати **SceneBootstrapInstaller** компонент
   - Перетягти **SceneBootstrapConfig** у поле інсталера
   - Додати його в список **Mono Installers** до інших інсталерів
3. На **SceneContext** GameObject також додати:
   - **WorldInfoPanelInstaller** компонент
   - Інші необхідні інсталери

## Updated Installers

### WorldInfoPanelInstaller

Тепер читає конфіг замість ручного призначення:

```csharp
[SerializeField] private WorldUIConfigSO _uiConfig;

public override void InstallBindings()
{
    var panelPrefab = _uiConfig.WorldInfoPanelPrefab;
    // Решта логіки...
}
```

### SceneBootstrapInstaller (NEW)

Завантажує всі конфіги на сцену та передає через DI контейнер:

```csharp
[SerializeField] private SceneBootstrapConfigSO _bootstrapConfig;

public override void InstallBindings()
{
    var worldUIConfig = Instantiate(_bootstrapConfig.WorldUIConfigPrefab)
        .GetComponent<WorldUIConfigSO>();
    Container.BindInstance(worldUIConfig).AsSingle();
}
```

## Example Scene Structure

```
GameObject: SceneContext
├── Component: SceneContext (Zenject)
├── Component: SceneBootstrapInstaller
│   └── Field: Scene Bootstrap Config → SceneBootstrapConfig
├── Component: WorldInfoPanelInstaller
│   └── Field: UI Config → WorldUIConfig
├── Component: SignalBusInstaller
├── Component: GridInstaller
├── Component: UnitsInstaller
└── ... інші інсталери
```

## Benefits

✅ **No Manual Assignments** - Всі посилання передаються програмно
✅ **Centralized Config** - Один конфіг для всіх UI компонентів
✅ **Easy Scene Reuse** - Новій сцені просто копіюємо конфіги
✅ **Type Safe** - Все типізовано, немає "magic strings"
✅ **Easy to Debug** - Логування показує що завантажується

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "WorldUIConfigSO не присвоєно" | Переконатися, що SceneBootstrapConfig має посилання на WorldUIConfig |
| "WorldInfoPanelPrefab не знайдено" | Перевірити що префаб додано в WorldUIConfig |
| "Панель не з'являється" | Переконатися що SceneBootstrapInstaller в списку Installers першим |

## Files Created/Modified

**Created:**
- `WorldUIConfigSO.cs` - Конфіг для UI панелей
- `SceneBootstrapConfigSO.cs` - Конфіг для сцени
- `SceneBootstrapInstaller.cs` - Інсталер для завантаження конфігів
- `WorldInfoPanelPrefabBuilder.cs` - Утиліта для створення префаба

**Modified:**
- `WorldInfoPanelInstaller.cs` - Тепер читає конфіг замість ручного призначення
