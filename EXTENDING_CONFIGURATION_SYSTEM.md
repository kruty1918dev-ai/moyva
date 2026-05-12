# 🔧 Advanced: Extending the Configuration System

This guide shows how to apply the same configuration pattern to other UI systems.

## Pattern Template

### Step 1: Create Config Class

```csharp
using UnityEngine;

namespace Yournamespace.API
{
    [CreateAssetMenu(menuName = "Moyva/UI/[Your System] Config")]
    public sealed class YourSystemConfigSO : ScriptableObject
    {
        [SerializeField] private GameObject _mainPrefab;
        [SerializeField] private GameObject _secondaryPrefab;
        [SerializeField] private string _parentName = "Canvas";

        public GameObject MainPrefab => _mainPrefab;
        public GameObject SecondaryPrefab => _secondaryPrefab;
        public string ParentName => _parentName;
    }
}
```

### Step 2: Update Installer

```csharp
using UnityEngine;
using Zenject;
using Yournamespace.API;

namespace Yournamespace.UI
{
    public class YourSystemInstaller : MonoInstaller
    {
        [SerializeField] private YourSystemConfigSO _config;

        public override void InstallBindings()
        {
            if (_config == null)
            {
                Debug.LogWarning("[YourSystemInstaller] Config not assigned");
                return;
            }

            var prefab = _config.MainPrefab;
            var parentName = _config.ParentName;

            // Instantiate from config...
            // Register in container...

            Debug.Log("[YourSystemInstaller] ✓ System installed from config");
        }
    }
}
```

### Step 3: Create Auto-Installer (Optional)

```csharp
using UnityEngine;
using Zenject;
using Yournamespace.API;

namespace Yournamespace.Runtime
{
    public sealed class YourSystemAutoInstaller : MonoInstaller
    {
        [SerializeField] private YourSystemConfigSO _config;
        [SerializeField] private bool _autoLoad = true;
        [SerializeField] private string _resourcesPath = "Configs/YourSystemConfig";

        public override void InstallBindings()
        {
            var config = _config;

            if (config == null && _autoLoad)
            {
                config = Resources.Load<YourSystemConfigSO>(_resourcesPath);
            }

            if (config != null)
            {
                Container.BindInstance(config).AsSingle();
                Debug.Log("[YourSystemAutoInstaller] ✓ Config installed");
            }
        }
    }
}
```

---

## Examples: Extending for Common Systems

### 🎮 Construction UI

```csharp
// ConstructionUIConfigSO.cs
public sealed class ConstructionUIConfigSO : ScriptableObject
{
    [SerializeField] private GameObject _buildMenuPrefab;
    [SerializeField] private GameObject _destructionConfirmPrefab;
    
    public GameObject BuildMenuPrefab => _buildMenuPrefab;
    public GameObject DestructionConfirmPrefab => _destructionConfirmPrefab;
}
```

### 🗂️ Inventory UI

```csharp
// InventoryUIConfigSO.cs
public sealed class InventoryUIConfigSO : ScriptableObject
{
    [SerializeField] private GameObject _inventoryWindowPrefab;
    [SerializeField] private GameObject _itemSlotPrefab;
    
    public GameObject InventoryWindowPrefab => _inventoryWindowPrefab;
    public GameObject ItemSlotPrefab => _itemSlotPrefab;
}
```

### 🎯 Settings Menu

```csharp
// SettingsMenuConfigSO.cs
public sealed class SettingsMenuConfigSO : ScriptableObject
{
    [SerializeField] private GameObject _settingsPanelPrefab;
    [SerializeField] private GameObject _videoSettingsPrefab;
    [SerializeField] private GameObject _audioSettingsPrefab;
    
    public GameObject SettingsPanelPrefab => _settingsPanelPrefab;
    public GameObject VideoSettingsPrefab => _videoSettingsPrefab;
    public GameObject AudioSettingsPrefab => _audioSettingsPrefab;
}
```

---

## Consolidating All Configs

### Create Master Config

```csharp
// GameUIConfigSO.cs - Single config for all UI
public sealed class GameUIConfigSO : ScriptableObject
{
    [Header("World Info")]
    [SerializeField] private WorldUIConfigSO _worldUIConfig;
    
    [Header("Construction")]
    [SerializeField] private ConstructionUIConfigSO _constructionConfig;
    
    [Header("Inventory")]
    [SerializeField] private InventoryUIConfigSO _inventoryConfig;
    
    [Header("Settings")]
    [SerializeField] private SettingsMenuConfigSO _settingsConfig;

    public WorldUIConfigSO WorldUIConfig => _worldUIConfig;
    public ConstructionUIConfigSO ConstructionConfig => _constructionConfig;
    public InventoryUIConfigSO InventoryConfig => _inventoryConfig;
    public SettingsMenuConfigSO SettingsConfig => _settingsConfig;
}
```

### Create Master Installer

```csharp
// GameUIInstaller.cs - Installs all UI systems
public sealed class GameUIInstaller : MonoInstaller
{
    [SerializeField] private GameUIConfigSO _uiConfig;

    public override void InstallBindings()
    {
        if (_uiConfig == null)
        {
            Debug.LogError("[GameUIInstaller] GameUIConfigSO not assigned!");
            return;
        }

        // Bind all sub-configs
        if (_uiConfig.WorldUIConfig != null)
            Container.BindInstance(_uiConfig.WorldUIConfig).AsSingle();
            
        if (_uiConfig.ConstructionConfig != null)
            Container.BindInstance(_uiConfig.ConstructionConfig).AsSingle();
            
        if (_uiConfig.InventoryConfig != null)
            Container.BindInstance(_uiConfig.InventoryConfig).AsSingle();
            
        if (_uiConfig.SettingsConfig != null)
            Container.BindInstance(_uiConfig.SettingsConfig).AsSingle();

        Debug.Log("[GameUIInstaller] ✓ All UI configs installed");
    }
}
```

### Scene Setup Becomes Simple

```
SceneContext:
├─ SignalBusInstaller
├─ GameUIInstaller ← Only this for ALL UI!
│   └─ UI Config: GameUIConfig.asset
├─ GridInstaller
└─ FactionInstaller
```

---

## Benefits of This Pattern

✅ **Single Responsibility** - Each config handles one system
✅ **Reusable** - Same pattern for new systems
✅ **Type-Safe** - No magic strings
✅ **Centralized** - Master config ties everything
✅ **Scalable** - Easy to add more systems
✅ **Testable** - Mock configs for unit tests
✅ **Documented** - Each config is self-documenting

---

## Folder Structure for Large Projects

```
Assets/Moyva/SO/UI/
├── WorldUIConfig.asset
├── ConstructionUIConfig.asset
├── InventoryUIConfig.asset
├── SettingsMenuConfig.asset
└── GameUIConfig.asset (Master)

Assets/Moyva/Prefabs/UI/
├── World/
│   └── WorldInfoPanel.prefab
├── Construction/
│   ├── BuildMenu.prefab
│   └── DestructionConfirm.prefab
├── Inventory/
│   ├── InventoryWindow.prefab
│   └── ItemSlot.prefab
└── Settings/
    ├── SettingsPanel.prefab
    ├── VideoSettings.prefab
    └── AudioSettings.prefab
```

---

## Real-World Example: Complete Game Setup

```csharp
// 1. Master game config
[CreateAssetMenu(menuName = "Moyva/Game/Master Config")]
public class GameMasterConfigSO : ScriptableObject
{
    [SerializeField] private GameUIConfigSO _uiConfig;
    [SerializeField] private GameplayConfigSO _gameplayConfig;
    [SerializeField] private AudioConfigSO _audioConfig;

    public GameUIConfigSO UIConfig => _uiConfig;
    public GameplayConfigSO GameplayConfig => _gameplayConfig;
    public AudioConfigSO AudioConfig => _audioConfig;
}

// 2. Master installer
public sealed class GameMasterInstaller : MonoInstaller
{
    [SerializeField] private GameMasterConfigSO _config;

    public override void InstallBindings()
    {
        Container.BindInstance(_config.UIConfig).AsSingle();
        Container.BindInstance(_config.GameplayConfig).AsSingle();
        Container.BindInstance(_config.AudioConfig).AsSingle();
    }
}

// 3. Scene now only needs one installer!
// SceneContext > Add Component > GameMasterInstaller
// Assign GameMasterConfig.asset
// Done!
```

---

## Performance Considerations

### Lazy Loading (For Large Projects)

```csharp
public override void InstallBindings()
{
    // Don't instantiate all prefabs, just load when needed
    Container.BindFactory<YourPrefab, YourPrefabFactory>()
        .FromFactory(new PrefabFactory(_config.MyPrefab))
        .AsTransient();
}
```

### Resources vs Direct References

```csharp
// Option 1: Direct reference (current approach)
[SerializeField] private GameObject _prefab;

// Option 2: Resources loading (for memory efficiency)
private GameObject _prefab => Resources.Load<GameObject>("Prefabs/MyPrefab");

// Option 3: Addressables (for advanced scenarios)
private async Task<GameObject> LoadPrefab()
{
    return await Addressables.LoadAssetAsync<GameObject>("MyPrefab");
}
```

---

## Checklist for New System

- [ ] Create `YourSystemConfigSO : ScriptableObject`
- [ ] Create `YourSystemAutoInstaller : MonoInstaller` (optional)
- [ ] Update existing installer to read from config
- [ ] Create SO asset with prefab references
- [ ] Add installer to SceneContext
- [ ] Test in Play Mode
- [ ] Document in project README

---

## Common Patterns to Reuse

| Pattern | Use Case |
|---------|----------|
| **Master Config** | Multiple related systems |
| **Auto-Installer** | Optional Resources loading |
| **PrefabFactory** | Dynamic prefab instantiation |
| **ConfigSO inheritance** | Shared base configuration |
| **Signal-driven** | UI activation/deactivation |

---

This pattern keeps your codebase clean, scalable, and maintainable.
Once you set it up for one system, extending to others becomes trivial.

**Status:** Pattern established and documented ✅
