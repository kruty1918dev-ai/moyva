# Аналіз PR: Система будівництва

> Порівняння трьох відкритих pull request'ів (#21, #22, #23) щодо реалізації системи будівництва.

---

## Загальний огляд

| Критерій | PR #21 | PR #22 | PR #23 |
|---|---|---|---|
| **Назва модуля** | `Buildings` | `Buildings` | `Construction` ✅ |
| **Namespace** | `Kruty1918.Moyva.Buildings.API` | `Kruty1918.Moyva.Buildings.API` | `Kruty1918.Moyva.Construction.API` ✅ |
| **Assembly** | `Kruty1918.Moyva.Buildings` | `Kruty1918.Moyva.Buildings` | `Kruty1918.Moyva.Construction` ✅ |
| **Структура шарів** | API + Runtime + **UI** ⚠️ | API + Runtime ✅ | API + Runtime ✅ |
| **Кількість файлів** | 22 | 20 | 13 ✅ |
| **Назва файлу сигналів** | `OnBuildingChanged.cs` ⚠️ | `BuildingSignals.cs` ❌ | `OnConstructionSignals.cs` ✅ |
| **`internal sealed`** | ❌ | ❌ | ✅ |
| **ITickable (Zenject)** | ❌ | ❌ | ✅ |
| **BindInterfacesAndSelfTo** | ❌ | ❌ | ✅ |
| **Не чіпає чужі фічі** | ❌ | ✅ | ✅ |
| **Docs (нова сторінка)** | `buildings.md` | `buildings.md` | `construction.md` ✅ |

---

## Детальний аналіз

### PR #21 — «Buildings: menu, placement, undo/redo, wall chaining»

**Що реалізовано:**
- Меню 3 категорій (Military / Civilian / Industrial)
- Ghost-preview на основі `TileHoveredSignal`
- Session-based confirm/cancel, undo/redo стеки
- Система стін з 8 точками з'єднання та алгоритмом Брезенхема

**Відповідність TDD:**

| Правило TDD | Статус |
|---|---|
| Структура `API/Runtime` | ✅ Є, але додано зайвий `UI/` шар |
| Конфіги в правильному шарі | ⚠️ `BuildingRegistrySO` з `[CreateAssetMenu]` в `API/` замість `Runtime/` |
| Нема прямих залежностей між фічами | ❌ Модифікує `TileView.cs` у чужій фічі `Visuals` |
| Installer через Zenject | ✅ `BuildingsInstaller` |
| Іменування сигналів | ⚠️ `OnBuildingChanged.cs` — близько, але «Building» плутається з назвою модуля |
| `internal` для реалізацій | ❌ `WallConnectionPoint` — публічний MonoBehaviour у `Runtime/` |

**Проблеми:**
1. **Порушення ізоляції**: `TileView.cs` з фічі `Visuals` модифіковано для hover-ефекту. Це порушує TDD: «Зовнішні модулі не використовують конкретні runtime-класи фічі» і «Cross-feature комунікація йде через сигнали/API».
2. **Зайвий шар `UI/`**: TDD визначає `API/Runtime/Editor` — не `UI/`. Розміщення UI-контролерів у окремий підкаталог без asmdef-ізоляції не дає реальних переваг.
3. **`BuildingRegistrySO` в `API/`**: ScriptableObject з `[CreateAssetMenu]` — це конкретна реалізація, вона має бути в `Runtime/`.

---

### PR #22 — «feat: Buildings construction system»

**Що реалізовано:**
- Схожий функціонал до PR #21
- `IBuildingConfig` інтерфейс + `BuildingConfig` реалізація
- `BuildingInputHandler` MonoBehaviour для клавіш (Update-based)
- `BuildingPreviewView` ghost MonoBehaviour
- `WallPlacementController` + `WallCircleHandler`

**Відповідність TDD:**

| Правило TDD | Статус |
|---|---|
| Структура `API/Runtime` | ✅ Є |
| Конфіги в правильному шарі | ❌ `BuildingConfig` і `BuildingRegistrySO` в `Runtime/` замість `API/` та `Runtime/` |
| Нема прямих залежностей між фічами | ✅ Не чіпає інші фічі |
| Installer через Zenject | ✅ `BuildingsInstaller` |
| Іменування сигналів | ❌ `BuildingSignals.cs` — не відповідає конвенції `On<Domain>` |
| `internal` для реалізацій | ❌ Усі класи публічні |

**Проблеми:**
1. **`BuildingConfig` в `Runtime/`**: DTO-конфіги повинні бути в `API/`, щоб споживачі могли їх використовувати без посилання на `Runtime`. У проєкті `UnitClassConfig` знаходиться в `Units/API/`.
2. **Зайвий `IBuildingConfig`**: Непотрібна абстракція над простим `[Serializable]` класом. В проєкті жоден простий конфіг не має свого інтерфейсу.
3. **Назва файлу сигналів**: `BuildingSignals.cs` — проєкт використовує `On<Domain>Changed.cs` для груп сигналів (як `OnTileChanged.cs`).
4. **Keyboard через `Update()`**: `BuildingInputHandler : MonoBehaviour` замість `ITickable` — менш інтегрований з DI-контейнером.

---

### PR #23 — «Construction: categorized menu, placement, undo/redo, wall mechanics»

**Що реалізовано:**
- Окремий модуль `Construction` (не «Buildings»)
- 9 сигналів, що покривають весь lifecycle будівництва
- `IBuildingConstructionService` + `IWallConnectionService` в `API/`
- `BuildingConstructionService` реалізує `ITickable` + `IInitializable` + `IDisposable`
- `WallConnectionService` — `internal sealed` в `Runtime/`
- `ConstructionInstaller` з `BindInterfacesAndSelfTo`

**Відповідність TDD:**

| Правило TDD | Статус |
|---|---|
| Структура `API/Runtime` | ✅ Чітко API/Runtime, без зайвих шарів |
| Конфіги в правильному шарі | ✅ `BuildingConfig` в `API/`, `BuildingRegistrySO` в `Runtime/` |
| Нема прямих залежностей між фічами | ✅ Не чіпає жодної іншої фічі |
| Installer через Zenject | ✅ `BindInterfacesAndSelfTo<BuildingConstructionService>` |
| Іменування сигналів | ✅ `OnConstructionSignals.cs` → відповідає `OnTileChanged.cs` |
| `internal` для реалізацій | ✅ `WallConnectionService` — `internal sealed` |
| Назва модуля відображає домен | ✅ `Construction` — окремий домен від `Buildings` |

**Переваги:**
1. **Окремий модуль `Construction`**: Система будівництва — це процес/механіка, не просто колекція будівель. Виокремлення в `Construction` відповідає «Package by Feature» підходу ТДД.
2. **`ITickable` замість MonoBehaviour `Update()`**: Клавіатурний ввід через Zenject lifecycle — без зайвих MonoBehaviour з Update.
3. **`internal sealed`**: `WallConnectionService` прихований всередині модуля — споживачі бачать тільки `IWallConnectionService`.
4. **`BindInterfacesAndSelfTo`**: Один binding для `IBuildingConstructionService`, `ITickable`, `IInitializable`, `IDisposable`.
5. **Мінімальна кількість файлів (13)**: Більш стисле рішення без надлишкових UI-контролерів.

**Незначні зауваження:**
- `BuildingRegistrySO` у `Runtime/` — це правильно (конкретна SO-реалізація). У `API/` слід залишити лише `[Serializable] BuildingConfig`.
- UI-шар (Menu) відсутній — залишено на майбутній PR або окрему фічу.

---

## Висновок

### 🏆 PR #23 найкраще відповідає TDD та архітектурі проєкту

**Ключові причини:**

1. **Правильна назва модуля**: `Construction` відображає доменний смисл (процес будівництва), а не тип об'єктів.
2. **Чиста архітектура API/Runtime**: Інтерфейси в `API/`, реалізації в `Runtime/`, `internal sealed` для прихованих деталей.
3. **Zenject-патерн**: `BindInterfacesAndSelfTo` + `ITickable` — правильна інтеграція з DI-контейнером відповідно до TDD.
4. **Ізольованість**: Не модифікує жодну іншу фічу (на відміну від PR #21 з `TileView.cs`).
5. **Конвенції сигналів**: `OnConstructionSignals.cs` відповідає наявному `OnTileChanged.cs`.
6. **Мінімальність**: 13 файлів замість 20-22 — менша складність, легше підтримувати.

### Порівняльна таблиця відповідності TDD

| Правило TDD | PR #21 | PR #22 | PR #23 |
|---|:---:|:---:|:---:|
| API/Runtime без зайвих шарів | ⚠️ | ✅ | ✅ |
| Конфіги в `API/` | ⚠️ | ❌ | ✅ |
| Ізоляція від інших фіч | ❌ | ✅ | ✅ |
| `internal` для реалізацій | ❌ | ❌ | ✅ |
| Zenject `ITickable`/lifecycle | ❌ | ❌ | ✅ |
| `BindInterfacesAndSelfTo` | ❌ | ❌ | ✅ |
| Конвенція назв сигналів | ⚠️ | ❌ | ✅ |
| Мінімальна кількість файлів | ❌ | ⚠️ | ✅ |
| **Загальна оцінка** | **2/8** | **3/8** | **8/8** |
