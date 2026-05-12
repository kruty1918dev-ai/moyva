# Setup Guide: Configuration-based Installers

## Quick Start (5 minutes)

### Step 1: Create WorldInfoPanel Prefab

1. Open any scene
2. Go to **Assets > Moyva > UI > Create WorldInfoPanel Prefab**
3. Save as `Assets/Moyva/Prefabs/UI/WorldInfoPanel.prefab`
4. Close the prefab editor

### Step 2: Create WorldUIConfig

1. **Right-click** in Project window → **Create > Moyva > UI > World UI Config**
2. Save as `Assets/Moyva/SO/Bootstrap/WorldUIConfig.asset`
3. In Inspector:
   - **World Info Panel Prefab**: Drag `WorldInfoPanel.prefab` here
   - **Panel Parent Name**: Keep as `Canvas` (default)

### Step 3: Setup Your Game Scene

1. Open your game scene (наприклад, GameplayScene)
2. Find/Create **SceneContext** GameObject (якщо його немає, створ новий)
3. On SceneContext add these Installers (in order):
   - **SignalBusInstaller** (вже повинен бути)
   - **WorldUIConfigAutoInstaller** (new)
   - **WorldInfoPanelInstaller** (updated)
   - **FactionInstaller** та інші (як вже є)

4. На **WorldUIConfigAutoInstaller**:
   - **UI Config** field: Drag `WorldUIConfig.asset` here
   - **Auto Load From Resources**: Toggle OFF (у вас вже є конфіг)

5. На **WorldInfoPanelInstaller**:
   - Його конфіг буде автоматично отриманий з контейнера

### Step 4: Verify

1. Запустити Play Mode
2. У Console повинні бути логи:
   ```
   [WorldUIConfigAutoInstaller] ✓ WorldUIConfigSO передано у контейнер
   [WorldInfoPanelInstaller] ✓ World Info Panel успішно встановлена з конфіга
   ```

## Alternative: Using SceneBootstrapInstaller (Advanced)

Якщо хочеш мати один конфіг для всієї сцени:

### Step 1: Create SceneBootstrapConfig

1. **Right-click** → **Create > Moyva > Scenes > Scene Bootstrap Config**
2. Save as `Assets/Moyva/SO/Bootstrap/GameSceneBootstrap.asset`
3. In Inspector:
   - **World UI Config Prefab**: NULL (не потрібно для цього варіанту)
   - **Game Session Config Prefab**: NULL (або твоя GameSessionConfig)

### Step 2: Update SceneContext

1. На SceneContext додай **SceneBootstrapInstaller**
2. Assign **Scene Bootstrap Config**: `GameSceneBootstrap.asset`
3. Додай інші інсталери як зазвичай

### Step 3: Result

SceneBootstrapInstaller буде завантажувати конфіги і передавати їх іншим інсталерам.

## File Structure

```
Assets/Moyva/
├── SO/
│   └── Bootstrap/
│       ├── WorldUIConfig.asset          ← Конфіг UI
│       ├── GameSceneBootstrap.asset     ← (Optional) Конфіг сцени
│       └── GameSessionConfig.asset      ← Вже існує
│
├── Prefabs/
│   └── UI/
│       └── WorldInfoPanel.prefab        ← Створено утилітою
│
└── Scripts/
    └── Features/
        ├── InfoPanel/
        │   ├── API/
        │   │   └── WorldUIConfigSO.cs   ← (NEW) Конфіг клас
        │   └── UI/
        │       ├── WorldInfoPanelInstaller.cs  ← (UPDATED)
        │       └── Editor/
        │           └── WorldInfoPanelPrefabBuilder.cs  ← (NEW) Утиліта
        │
        └── Bootstrap/
            ├── API/
            │   └── SceneBootstrapConfigSO.cs     ← (NEW) Конфіг сцени
            └── Runtime/
                ├── SceneBootstrapInstaller.cs    ← (NEW) Основний інсталер
                └── WorldUIConfigAutoInstaller.cs ← (NEW) Auto-load інсталер
```

## Troubleshooting

### Problem: "WorldUIConfigSO не присвоєно"
**Solution:** 
- Переконайся що ти додав `WorldUIConfigAutoInstaller` до SceneContext
- Або присвой конфіг вручну в інспекторі

### Problem: "WorldInfoPanelPrefab не знайдено"
**Solution:**
- Відкрий `WorldUIConfig.asset` 
- Перевір що поле **World Info Panel Prefab** заповнено
- Якщо ні, перетягни префаб туди

### Problem: "Панель не з'являється в игре"
**Solution:**
- Запусти сцену у Play Mode
- Перевір Console на помилки
- Переконайся що Canvas існує у сцені
- Перевір що **Panel Parent Name** правильне (за замовч. "Canvas")

## Next Steps

### For More UI Panels
1. Create new prefab using the builder
2. Add to **WorldUIConfigSO** (create new fields if needed)
3. Update **WorldUIConfigAutoInstaller** to pass new configs to container

### For Different Scenes
1. Duplicate **WorldUIConfig.asset** → rename → update prefab references
2. Assign new config to appropriate scene's SceneContext

### For Resources-based Loading
1. Create folder: `Assets/Moyva/Resources/Configs/`
2. Move `WorldUIConfig.asset` there
3. Rename to match path: `WorldUIConfig.asset`
4. On `WorldUIConfigAutoInstaller` enable **Auto Load From Resources**
5. Set **Resources Path**: `Configs/WorldUIConfig`

## Example: Full Scene Setup

```
GameObject: SceneContext
├── Script: SceneContext
├── Script: SignalBusInstaller
├── Script: WorldUIConfigAutoInstaller
│   └── UI Config: WorldUIConfig
├── Script: WorldInfoPanelInstaller
├── Script: GridInstaller
├── Script: FactionInstaller
│   └── Session Config: GameSessionConfig
├── Script: EconomyInstaller
└── Script: UnitsInstaller

GameObject: Canvas
└── [будуть з'являтися UI панелі при завантаженні]

GameObject: Grid
└── [Гейм об'єкти]
```

## Files Modified/Created

**Created (NEW):**
- ✅ `WorldUIConfigSO.cs` - Конфіг для UI панелей
- ✅ `SceneBootstrapConfigSO.cs` - Конфіг для всієї сцени
- ✅ `SceneBootstrapInstaller.cs` - Інсталер для завантаження конфігів
- ✅ `WorldUIConfigAutoInstaller.cs` - Auto-load інсталер з Resources
- ✅ `WorldInfoPanelPrefabBuilder.cs` - Утиліта для створення префаба

**Modified (UPDATED):**
- ✅ `WorldInfoPanelInstaller.cs` - Тепер читає конфіг з контейнера

**Documentation:**
- ✅ `INSTALLERS_SETUP.md` - Архітектура системи
- ✅ `SETUP_GUIDE_STEP_BY_STEP.md` - Цей файл
