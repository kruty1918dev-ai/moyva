# Fog of War — Архітектура

← [README](README.md)

---

## Шари

| Шар | Папка | Assembly |
|---|---|---|
| **API** | `Features/FogOfWar/API/` | `Kruty1918.Moyva.FogOfWar` |
| **Runtime** | `Features/FogOfWar/Runtime/` | `Kruty1918.Moyva.FogOfWar` |
| **Tests** | `Tests/FogOfWar/` | `Kruty1918.Moyva.Tests.FogOfWar` |

---

## Файли API

| Файл | Призначення |
|---|---|
| `FogStateType.cs` | Enum: Unexplored / Explored / Visible |
| `IFogOfWarService.cs` | Головний сервіс видимості |
| `IFogVisibilityResolver.cs` | Обчислення видимих тайлів (FOV) |
| `IFogTextureUpdater.cs` | Оновлення Texture2D |
| `IFogSaveDataProvider.cs` | Завантаження/збереження explored стану |
| `FogOfWarSettings.cs` | ScriptableObject із параметрами шейдера та зору |

---

## Файли Runtime

| Файл | Призначення |
|---|---|
| `FogVisibilityResolver.cs` | Symmetric Shadowcasting (8 октантів) |
| `FogNoiseGenerator.cs` | CPU Perlin noise (детермінований) |
| `FogOfWarService.cs` | Реалізація `IFogOfWarService` + Zenject lifecycle |
| `FogTextureUpdater.cs` | Оновлення Texture2D (R8, dirty-tiles) |
| `FogSaveDataStub.cs` | Заглушка збереження |
| `FogQuadController.cs` | MonoBehaviour: розміщення quad + ініціалізація |
| `FogOfWarInstaller.cs` | Zenject MonoInstaller |
| `Shaders/FogOfWar.shader` | URP 2D шейдер |

---

## Граф залежностей

```
FogOfWarService
 ├── IFogVisibilityResolver (FogVisibilityResolver)
 │    └── IGridService
 ├── IFogTextureUpdater (FogTextureUpdater)
 ├── IFogSaveDataProvider (FogSaveDataStub)
 ├── SignalBus → UnitCreatedSignal, UnitMovedSignal, UnitDestroyedSignal
 └── FogOfWarSettings (optional)
```

---

## asmdef граф

```
Kruty1918.Moyva.FogOfWar
 ├── Kruty1918.Moyva.Signals
 ├── Kruty1918.Moyva.Grid
 └── Zenject

Kruty1918.Moyva.Tests.FogOfWar (Editor only)
 ├── Kruty1918.Moyva.FogOfWar
 ├── Kruty1918.Moyva.Signals
 ├── Kruty1918.Moyva.Grid
 ├── Zenject
 ├── Zenject-TestFramework
 └── nunit.framework.dll
```

---

## Fallback таблиця

| Ситуація | Поведінка |
|---|---|
| `IGridService == null` | FogVisibilityResolver логує WARNING, використовує прямий radius-circle |
| `FogOfWarSettings == null` | FogOfWarService логує WARNING, DefaultVisionRange = 5 |
| `Material == null` при Initialize | FogTextureUpdater логує ERROR, `_renderingDisabled = true`; логіка масиву працює |
| `Initialize(w,h)` не викликали | Будь-який виклик register/update/unregister логує WARNING і повертає |
| `UpdateUnitPosition` для невідомого юніта | Логує WARNING і повертає без краш |
