# 🏪 How to Use: Warehouse Resources Display

**Time to test:** 2 minutes

## Quick Start

### What You Get

When you click on a **Warehouse** building in the game, you'll see:

```
═══════════════════════
Ресурси складу
───────────────────────
📦 ПРОВІЗІЯ: 250
  • Пшениця: 100
  • М'ясо: 150

🔨 МАТЕРІАЛИ: 500
  • Дерево: 300
  • Камінь: 200
═══════════════════════
```

**Showing:**
- 📦 **Total Food:** 250 (with breakdown)
- 🔨 **Total Materials:** 500 (with breakdown)
- Each resource sorted by quantity (highest first)

### How It Works

1. **Click on warehouse building** → Information panel opens
2. **System reads:** All resources in that warehouse
3. **System categorizes:** Food vs Materials
4. **System displays:** Nice formatted list with totals

## What You Need to Do

**Nothing!** It's already integrated. Just play the game and:

1. Build a **Warehouse** building
2. Click on it
3. See the formatted resource list in the info panel

## Customization

### Change Display Format

Edit `WarehouseInfoFormatter.cs`:

```csharp
// Change icons (line 55, 72, 88)
"📦 ПРОВІЗІЯ" → "🌾 ЇЖА"
"🔨 МАТЕРІАЛИ" → "⚒️ БУДІВЕЛЬНІ"

// Change borders (line 39, 40, 41)
"═══════════════════════" → "┌──────────────────┐"
"───────────────────────" → "├──────────────────┤"
"═══════════════════════" → "└──────────────────┘"

// Change number format (line 59, 76, 93)
{kvp.Value:0.#} → {kvp.Value:F0} // No decimals
```

### Add New Resource Category

1. Edit `EconomyEnums.cs`:
```csharp
public enum EconomyResourceCategory
{
    None = 0,
    Food = 1,
    Materials = 2,
    Gold = 3  // NEW
}
```

2. Edit `WarehouseInfoFormatter.cs` around line 75:
```csharp
var goldResources = new Dictionary<string, float>();

// Add to categorization loop (line ~50):
case EconomyResourceCategory.Gold:
    goldResources[kvp.Key] = kvp.Value;
    break;

// Add display (after Materials section):
if (goldResources.Count > 0)
{
    float goldTotal = goldResources.Values.Sum();
    sb.AppendLine($"💰 ЗОЛОТО: {goldTotal:0.#}");
    foreach (var kvp in goldResources.OrderByDescending(x => x.Value))
    {
        var displayName = GetResourceDisplayName(kvp.Key, database);
        sb.AppendLine($"  • {displayName}: {kvp.Value:0.#}");
    }
}
```

3. **Done!** New resources will automatically appear in their category

## Examples

### Scenario 1: View Food Storage
- Click on Food Warehouse
- See: 📦 ПРОВІЗІЯ: 250 (wheat: 100, meat: 150)

### Scenario 2: Mixed Resources
- Click on Generic Warehouse
- See: Both Food and Materials with their totals

### Scenario 3: Empty Warehouse
- Click on Empty Warehouse
- See: "Немає ресурсів." (No resources)

### Scenario 4: Unknown Category
- Resource has Category=None
- Shows in: ❓ ІНШІ section

## Technical Details

### Classes Involved

| Class | Location | Role |
|-------|----------|------|
| `WarehouseInfoFormatter` | `/Construction/Runtime/` | Formats warehouse resources |
| `BuildingWorldInfoPresenter` | `/Construction/Runtime/` | Triggers formatter for warehouses |
| `EconomyDatabaseSO` | `/Economy/API/` | Source of resource definitions |
| `IEconomyInfoMediator` | `/Economy/API/` | Provides warehouse resources |

### Data Flow

```
WarehouseBuilding
       ↓
Click (TileInteractionService)
       ↓
BuildingInfoPanelRequestedSignal
       ↓
BuildingWorldInfoPresenter
       ↓
IsWarehouse? → YES
       ↓
GetWarehouseResourceTotals()
       ↓
WarehouseInfoFormatter
  - Read database
  - Categorize resources
  - Calculate totals
  - Format output
       ↓
WorldInfoPanelRequestedSignal
       ↓
WorldInfoPanelController
       ↓
Display formatted text
```

## Troubleshooting

### Warehouse Info Not Showing

**Check:**
1. ✅ Building is actually a warehouse (check `BuildingDefinition`)
2. ✅ World Info Panel is properly set up (see QUICK_START.md)
3. ✅ Warehouse has some resources

**Fix:**
- Ensure `BuildingDefinitionCapabilities.IsWarehouse(definition)` returns true
- Verify `EconomyDatabaseSO` is assigned in scene

### Resources Show Wrong Category

**Check:**
- Each resource has `EconomyResourceDefinition` with correct `Category`

**Fix:**
- Open `EconomyDatabaseSO`
- Find the resource definition
- Set `Category` to Food or Materials

### "Немає ресурсів" (No Resources) Shows

**This is normal if:**
- Warehouse is genuinely empty
- You want it to have resources, add them via economy system

### Display Format Looks Wrong

**Fix:**
- Check terminal/console for Unicode support
- On Windows, enable UTF-8 in Terminal settings
- Or replace icons with ASCII: `📦` → `[F]`, `🔨` → `[M]`

## Advanced Features

### Custom Formatting Function

Create your own formatter:

```csharp
public static string FormatWarehouseCompact(
    IReadOnlyDictionary<string, float> resources,
    EconomyDatabaseSO database)
{
    // One-line format: Food: 100, Materials: 500
    float food = 0, materials = 0;
    
    foreach (var kvp in resources)
    {
        var category = GetResourceCategory(kvp.Key, database);
        if (category == EconomyResourceCategory.Food)
            food += kvp.Value;
        else if (category == EconomyResourceCategory.Materials)
            materials += kvp.Value;
    }
    
    return $"📦 {food:0.#} | 🔨 {materials:0.#}";
}
```

Then use it:
```csharp
return WarehouseInfoFormatter.FormatWarehouseCompact(resources, database);
```

### Translations

To support multiple languages, use localization:

```csharp
// Instead of hardcoded strings:
sb.AppendLine($"📦 ПРОВІЗІЯ: {foodTotal:0.#}");

// Use localization:
sb.AppendLine($"📦 {LocalizationManager.GetText("category_food")}: {foodTotal:0.#}");
```

## Performance Notes

- ✅ Minimal overhead - only categorizes when warehouse clicked
- ✅ No frame drops - formatting is instant
- ✅ Scalable - works with any number of resources
- ✅ Cached resource lookups - database queries optimized

## Files Changed

| File | Change | Time |
|------|--------|------|
| `WarehouseInfoFormatter.cs` | ✨ NEW | +90 lines |
| `BuildingWorldInfoPresenter.cs` | ⚙️ UPDATED | +3 lines |

**Total:** ~100 lines of code | ~5 minutes to implement

## Next Steps

- ✅ Test clicking on warehouses
- 🎨 Customize icons/format if needed
- 📝 Add translations if needed
- 🔧 Create more building types that show custom info

---

**Status:** ✅ Ready to use | Tested | Production quality
