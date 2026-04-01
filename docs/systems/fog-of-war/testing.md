# Fog of War — Тести

← [README](README.md)

---

## Розташування

`Assets/Moyva/Scripts/Tests/FogOfWar/`

Assembly: `Kruty1918.Moyva.Tests.FogOfWar` (Editor-only)

---

## FogOfWarServiceTests.cs

Використовує `ZenjectUnitTestFixture` з реальним `SignalBus`.

| № | Тест | Що перевіряє |
|---|---|---|
| 1 | `Initialize_SetsAllTilesToUnexplored` | Після Initialize() всі тайли Unexplored |
| 2 | `RegisterUnit_AddsVisibilityToTilesInRange` | RegisterUnit робить тайли Visible |
| 3 | `RegisterUnit_MarksVisibleTilesAsExplored` | Видимі тайли відразу стають Explored |
| 4 | `UpdateUnitPosition_RemovesVisibilityFromOldTiles` | Після руху старі тайли не Visible |
| 5 | `TwoUnits_SameTile_CounterIsTwo_TileRemainsVisibleWhenOneLeaves` | Тайл лишається Visible якщо другий юніт ще там |
| 6 | `LastUnit_Leaves_TileBecomesExplored_NotUnexplored` | Коли останній юніт відходить — Explored, не Unexplored |
| 7 | `UnregisterUnit_RemovesAllVision` | UnregisterUnit знімає Visible |
| 8 | `LoadFromSnapshot_RestoresExploredState` | Snapshot правильно завантажується |
| 9 | `GetFogState_ReturnsCorrectEnum_ForAllStates` | Всі три стани повертаються правильно |
| 10 | `NullSettings_DoesNotThrow_UsesDefaults` | Без SO — не кидає виняток |
| 11 | `UpdateUnitPosition_UnknownUnit_DoesNotThrow` | Невідомий юніт — не кидає виняток |
| 12 | `SignalBus_UnitCreatedSignal_RegistersUnit` | Сигнал автоматично реєструє юніта |
| 13 | `SignalBus_UnitMovedSignal_UpdatesPosition` | Сигнал оновлює позицію |
| 14 | `SignalBus_UnitDestroyedSignal_UnregistersUnit` | Сигнал знімає реєстрацію |

---

## FogVisibilityResolverTests.cs

Plain `[TestFixture]` без Zenject.

| № | Тест | Що перевіряє |
|---|---|---|
| 1 | `ComputeVisibleTiles_AlwaysIncludesOrigin` | Позиція юніта завжди видима |
| 2 | `ComputeVisibleTiles_RangeZero_ReturnsOnlyOrigin` | visionRange=0 → тільки origin |
| 3 | `ComputeVisibleTiles_Range1_ReturnsAtLeastOriginAndNeighbours` | visionRange=1 → origin + сусіди |
| 4 | `ComputeVisibleTiles_NoDuplicates` | Немає дублікатів у результаті |
| 5 | `ComputeVisibleTiles_AllTilesWithinMapBounds` | Всі тайли в межах [0,w)×[0,h) |
| 6 | `ComputeVisibleTiles_LargerRange_MoreTiles` | Більший радіус → більше тайлів |
| 7 | `NullGridService_FallbackToCircle_DoesNotThrow` | Null gridService → не падає |

---

## FogNoiseGeneratorTests.cs

Plain `[TestFixture]` без Zenject або Unity runtime.

| № | Тест | Що перевіряє |
|---|---|---|
| 1 | `Generate_ReturnsCorrectSize` | Розмір масиву відповідає параметрам |
| 2 | `Generate_ValuesInRangeZeroToOne` | Всі значення в [0,1] |
| 3 | `Generate_SameSeed_SameOutput` | Однаковий seed → однаковий результат |
| 4 | `Generate_DifferentSeeds_DifferentOutput` | Різні seeds → різні результати |

---

## Як запустити

1. Unity → `Window → General → Test Runner`
2. Вкладка `EditMode`
3. Виберіть тести з `Kruty1918.Moyva.Tests.FogOfWar`
4. Натисніть `Run All` або запустіть окремий тест
