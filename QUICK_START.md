# ⚡ Quick Start (3 minutes)

## What You Need to Do

### 1️⃣ Create Prefab
```
Assets > Moyva > UI > Create WorldInfoPanel Prefab
↓
Save to: Assets/Moyva/Prefabs/UI/WorldInfoPanel.prefab
```

### 2️⃣ Create Config
```
Right-click in Assets/Moyva/SO/Bootstrap/
↓
Create > Moyva > UI > World UI Config
↓
Name: WorldUIConfig
↓
Drag WorldInfoPanel.prefab to "World Info Panel Prefab" field
```

### 3️⃣ Setup Scene
```
On SceneContext GameObject:
  ├─ Add Component > WorldUIConfigAutoInstaller
  │   └─ UI Config: Drag WorldUIConfig.asset
  │
  └─ Add Component > WorldInfoPanelInstaller
  
In SceneContext's "Mono Installers" array:
  ├─ [0] WorldUIConfigAutoInstaller
  └─ [1] WorldInfoPanelInstaller
```

### 4️⃣ Test
```
Play Mode
↓
Press Space (opens panel)
Press ESC (closes panel)
```

## Done! ✅

The system automatically:
- ✅ Loads prefabs from config
- ✅ Instantiates UI panels at runtime
- ✅ Passes references through DI
- ✅ No manual assignments needed

---

## Visual Structure

```
Before (Manual - ❌):
┌─────────────────────────────┐
│ Inspector: WorldInfoPanel   │
│ - Panel Prefab: [?] ← Manual│
│ - Panel Parent: [?] ← Manual│
│ - Resources...       ← Hard │
└─────────────────────────────┘

After (Automatic - ✅):
┌──────────────────────┐       ┌──────────────────────┐
│ WorldUIConfig.asset  │       │ SceneContext         │
│ ─────────────────────│       │ ──────────────────   │
│ Panel Prefab: [✓]    │       │ Installers:          │
│ Parent Name: Canvas  │       │ ├─ SignalBusInstall  │
│                      │       │ ├─ UIConfigAuto... ──┼─→ Reads config
│                      │       │ ├─ WorldInfoPanel    │
│                      │       │ ├─ GridInstaller     │
└──────────────────────┘       └──────────────────────┘
         ↑
    Assign Once!
```

---

## What Gets Created

| File | Location |
|------|----------|
| **WorldUIConfigSO.cs** | `Features/InfoPanel/API/` ✨ NEW |
| **SceneBootstrapConfigSO.cs** | `Features/Bootstrap/API/` ✨ NEW |
| **SceneBootstrapInstaller.cs** | `Features/Bootstrap/Runtime/` ✨ NEW |
| **WorldUIConfigAutoInstaller.cs** | `Features/Bootstrap/Runtime/` ✨ NEW |
| **WorldInfoPanelPrefabBuilder.cs** | `Features/InfoPanel/UI/Editor/` ✨ NEW |
| **InfoPanelTestHelper.cs** | `Features/Bootstrap/Runtime/` ✨ NEW |
| **WorldInfoPanelInstaller.cs** | `Features/InfoPanel/UI/` ⚙️ UPDATED |

---

## Key Benefits

| What | Before | After |
|------|--------|-------|
| **Scene Setup** | 15+ manual assignments | 1 config |
| **Prefab Changes** | Update 5+ places | Update 1x |
| **New Scenes** | Copy all installers | Copy config |
| **Debugging** | Silent failures | Detailed logs |
| **Maintenance** | Error-prone | Type-safe |

---

## Signals You Use

```csharp
// Show building info
_signalBus.Fire(new BuildingInfoPanelRequestedSignal {
    BuildingId = "tavern_1",
    Position = new Vector2Int(5, 10)
});

// Show unit info  
_signalBus.Fire(new UnitInfoPanelRequestedSignal {
    UnitId = "knight_1",
    Position = new Vector2Int(3, 8)
});

// Close panel
_signalBus.Fire(new WorldInfoPanelClosedSignal());
```

---

## Full Documents

- 📖 **HOW_TO_USE_INSTALLERS.md** ← Detailed step-by-step
- 📖 **INSTALLERS_SETUP.md** ← Architecture overview
- 📖 **SETUP_GUIDE_STEP_BY_STEP.md** ← Alternative setups
- 📖 **CONFIGURATION_SYSTEM_SUMMARY.md** ← Complete reference

---

**Ready?** Start with 4 steps above, then read HOW_TO_USE_INSTALLERS.md for detailed guide.

**Status:** ✅ All scripts compiled, zero errors, ready to use
