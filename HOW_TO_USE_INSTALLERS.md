# 🚀 How to Use: Configuration-Based Installers

**⏱️ Complete setup time: 10 minutes**

## 📋 Checklist

- [ ] Create WorldInfoPanel Prefab
- [ ] Create WorldUIConfig
- [ ] Add Installers to SceneContext
- [ ] Test with InfoPanelTestHelper
- [ ] Integrate into your game scenes

---

## Part 1️⃣: Create WorldInfoPanel Prefab (2 min)

### Step 1: Open any scene
```
File > Open Scene > (any scene)
```

### Step 2: Create the prefab
```
Assets > Moyva > UI > Create WorldInfoPanel Prefab
```

### Step 3: Save location
```
Save as: Assets/Moyva/Prefabs/UI/WorldInfoPanel.prefab
```

**Result:** ✅ Prefab with correct structure created

---

## Part 2️⃣: Create WorldUIConfig (2 min)

### Step 1: In Project window, navigate to
```
Assets/Moyva/SO/Bootstrap/
```

### Step 2: Right-click
```
Create > Moyva > UI > World UI Config
```

### Step 3: Name it
```
WorldUIConfig
```

### Step 4: Configure in Inspector
```
World Info Panel Prefab: [Drag WorldInfoPanel.prefab here] ✅
Panel Parent Name: Canvas (default is fine)
```

**Result:** ✅ UI Config created and configured

---

## Part 3️⃣: Add Installers to Scene (3 min)

### Step 1: Open your game scene
```
Your game scene with SceneContext
```

### Step 2: Find/Create SceneContext GameObject
- If exists: Click it
- If doesn't exist: Create new GameObject → add `SceneContext` component

### Step 3: Add WorldUIConfigAutoInstaller

1. In SceneContext Inspector, click **Add Component**
2. Search: `WorldUIConfigAutoInstaller`
3. Click to add

### Step 4: Configure WorldUIConfigAutoInstaller
```
UI Config: [Drag WorldUIConfig.asset here] ✅
Auto Load From Resources: OFF (unchecked)
```

### Step 5: Add WorldInfoPanelInstaller

1. Click **Add Component** on SceneContext
2. Search: `WorldInfoPanelInstaller`
3. Click to add

**Result:** Both installers added

### Step 6: Add to Mono Installers list

1. On **SceneContext** component, find **Mono Installers** array
2. Click **+** to add new element
3. Drag **WorldUIConfigAutoInstaller** component here
4. Click **+** again
5. Drag **WorldInfoPanelInstaller** component here

**Important:** WorldUIConfigAutoInstaller must be BEFORE WorldInfoPanelInstaller

---

## Part 4️⃣: Test (3 min)

### Step 1: Add test helper to scene

1. On SceneContext GameObject, click **Add Component**
2. Search: `InfoPanelTestHelper`
3. Click to add

### Step 2: Run Play Mode
```
Press Space or Click Play button
```

### Step 3: Test controls
```
Space    → Panel should appear with test data
ESC      → Panel should close
```

### Step 4: Check Console
```
✓ [WorldUIConfigAutoInstaller] ✓ WorldUIConfigSO передано у контейнер
✓ [WorldInfoPanelInstaller] ✓ World Info Panel успішно встановлена з конфіга
```

**Result:** ✅ System working! Panel opens/closes correctly

---

## Part 5️⃣: Use in Your Game (unlimited)

### In TileInteractionService / Other systems:

```csharp
// Simply fire signals when you want to show info:

_signalBus.Fire(new BuildingInfoPanelRequestedSignal
{
    BuildingId = buildingId,
    Position = position,
});

// To close:
_signalBus.Fire(new WorldInfoPanelClosedSignal());
```

**That's it!** Panel will automatically show/hide based on signals.

---

## 🎯 Complete Scene Setup Example

```
SceneContext (GameObject)
│
├─ Component: SceneContext
│   ├─ Mono Installers [2]:
│   │  ├─ WorldUIConfigAutoInstaller
│   │  └─ WorldInfoPanelInstaller
│   └─ ... (other installers as before)
│
├─ Component: WorldUIConfigAutoInstaller
│  └─ UI Config: WorldUIConfig.asset ✅
│
├─ Component: WorldInfoPanelInstaller
│  └─ (auto-configured from container)
│
└─ Component: InfoPanelTestHelper (optional, for testing)

Canvas (GameObject)
└─ [WorldInfoPanel will be instantiated here at runtime]

[Your other scene objects...]
```

---

## ⚙️ File Locations Reference

| File | Purpose |
|------|---------|
| `Assets/Moyva/Prefabs/UI/WorldInfoPanel.prefab` | UI Panel prefab |
| `Assets/Moyva/SO/Bootstrap/WorldUIConfig.asset` | UI Configuration |
| `Assets/Moyva/Scripts/Features/InfoPanel/API/WorldUIConfigSO.cs` | Config class |
| `Assets/Moyva/Scripts/Features/InfoPanel/UI/WorldInfoPanelInstaller.cs` | Installer |
| `Assets/Moyva/Scripts/Features/Bootstrap/Runtime/WorldUIConfigAutoInstaller.cs` | Auto-load installer |
| `Assets/Moyva/Scripts/Features/Bootstrap/Runtime/InfoPanelTestHelper.cs` | Test helper |

---

## 🐛 Troubleshooting

### Panel doesn't appear in Play Mode

**Check:**
1. ✅ WorldUIConfig.asset has WorldInfoPanelPrefab assigned
2. ✅ Canvas exists in scene
3. ✅ WorldUIConfigAutoInstaller is in Mono Installers BEFORE others
4. ✅ Console shows success messages

**Fix:** Re-check steps 2 and 3 above

### "WorldUIConfigSO not found" error

**Check:**
1. ✅ WorldUIConfigAutoInstaller has UI Config assigned in Inspector
2. ✅ Auto Load From Resources is OFF

**Fix:** Assign WorldUIConfig.asset in Inspector

### Panel appears but looks wrong

**Check:**
1. ✅ WorldInfoPanel prefab structure is correct:
   - TitleText ✅
   - SubtitleText ✅
   - ResourcesText ✅
   - CloseButton ✅

**Fix:** Recreate prefab using Assets > Moyva > UI > Create WorldInfoPanel Prefab

---

## 📚 Additional Resources

- **Architecture Overview:** `INSTALLERS_SETUP.md`
- **Step-by-Step Setup:** `SETUP_GUIDE_STEP_BY_STEP.md`
- **System Summary:** `CONFIGURATION_SYSTEM_SUMMARY.md`

---

## ✨ Next Steps

After verification works:

1. **Remove InfoPanelTestHelper** from production scenes
2. **Use signals** from your game code to show panels:
   - BuildingInfoPanelRequestedSignal
   - UnitInfoPanelRequestedSignal
   - MapObjectInfoPanelRequestedSignal
3. **Create more UI configs** if you need additional panels
4. **Extend WorldUIConfigSO** with more prefab references as needed

---

**🎉 Congratulations!** 

You now have a professional configuration-based installer system.
No more manual prefab assignments. Everything flows through DI.

**Status:** Ready for production use ✅
