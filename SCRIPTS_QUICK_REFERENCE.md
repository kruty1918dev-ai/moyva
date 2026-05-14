# 🚀 QUICK REFERENCE: 824 СКРИПТІВ MOYVA

## ⚡ ШВИДКА НАВІГАЦІЯ

### Знаходитеся в якому модулі?

```
Assets/Moyva/Scripts/Features/{ВАШ_МОДУЛЬ}/
├── API/          ← Інтерфейси та SO конфіги
├── Runtime/      ← Основна логіка
├── Editor/       ← Редактор утиліти (опціонально)
└── UI/           ← UI компоненти (опціонально)
```

---

## 📋 СПИСОК МОДУЛІВ + СКІЛЬКИ СКРИПТІВ

```
┌─────────────────┬──────┬──────────────┐
│ Модуль          │ Кількість │ Статус    │
├─────────────────┼──────┼──────────────┤
│ Generator       │ 168  │ 🚀 Активна   │
│ HomeMenu        │ 114  │ 🔧 Рефакт.   │
│ Multiplayer     │ 76   │ 🚀 Активна   │
│ Construction    │ 58   │ 🔧 Рефакт.   │
│ Economy         │ 52   │ 🔴 Переписання
│ GraphSystem     │ 37   │ 🚀 Активна   │
│ Units           │ 30   │ 🔧 Рефакт.   │
│ FogOfWar        │ 22   │ 🔧 Рефакт.   │
│ SaveSystem      │ 17   │ ✅ Стабільна │
│ Calendar        │ 15   │ ✅ Стабільна │
│ Signals         │ 15   │ ✅ Стабільна │
│ WorldCreation   │ 14   │ 🔧 Рефакт.   │
│ BotAI           │ 10   │ 🔧 Рефакт.   │
│ Camera          │ 10   │ ✅ Стабільна │
│ Faction         │ 10   │ ✅ Стабільна │
│ GameMode        │ 10   │ ✅ Стабільна │
│ Interactions    │ 7    │ ✅ Стабільна │
│ Clouds          │ 7    │ ✅ Стабільна │
│ Grid            │ 8    │ ✅ Стабільна │
│ Visuals         │ 5    │ ✅ Стабільна │
│ Animations      │ 4    │ ✅ Стабільна │
│ ObjectsMap      │ 3    │ ✅ Стабільна │
│ Pathfinding     │ 3    │ ✅ Стабільна │
│ InfoPanel       │ 2    │ ✅ Стабільна │
├─────────────────┼──────┼──────────────┤
│ ИТОГО           │ 824  │              │
└─────────────────┴──────┴──────────────┘
```

---

## 🎯 ЧТО ЗМІНИТИ? (МАТРИЦЯ РІШЕНЬ)

### ЯКЩО ХОЧЕТЕ ДОДАТИ НОВУ ФІЧ

```
┌─────────────────────────────────────┐
│ 1. Виберіть модуль                  │
│    └─ Де це логічно розташовується? │
│                                      │
│ 2. Додайте інтерфейс                │
│    └─ ModuleName/API/INewService.cs │
│                                      │
│ 3. Реалізуйте сервіс                │
│    └─ ModuleName/Runtime/NewService │
│                                      │
│ 4. Зареєструйте в DI                │
│    └─ ModuleInstaller.cs            │
│                                      │
│ 5. Напишіть тести                   │
│    └─ Tests/ModuleName/NewTest.cs   │
│                                      │
│ 6. Коміт: feat(ModuleName): add ...│
└─────────────────────────────────────┘
```

### ЯКЩО ЗНАЙШЛИ БАГ

```
┌─────────────────────────────────────┐
│ 1. Визначте, в якому модулі          │
│ 2. Відкрийте Runtime/{Bug}.cs        │
│ 3. Виправте логіку                  │
│ 4. Запустіть тести модуля:           │
│    dotnet test Kruty1918.Moyva...   │
│                                      │
│ Коміт: fix(ModuleName): resolve ... │
└─────────────────────────────────────┘
```

### ЯКЩО ПОТРІБНА ОПТИМІЗАЦІЯ

```
Найбільші модулі (рекомендовані для оптимізації):
1. Generator   (168) - Процедурна генерація
2. HomeMenu    (114) - UI система
3. Multiplayer (76)  - Синхронізація мережі
4. Construction(58)  - Розміщення будівель
5. Economy     (52)  - Ресурси та населення

Команди для профайлінгу:
dotnet build -c Release
# Запустіть гру з Unity Profiler
```

---

## 🔥 НАЙПОПУЛЯРНІШІ ФАЙЛИ (що часто змінюються)

```
1. EconomyManager.cs              - 🔴 ПЕРЕПИСУЄТЬСЯ
2. ConstructionGridService.cs     - 🔧 ВЕЛИКЕ (400+ строк)
3. WorldCreationService.cs        - 🔧 ВЕЛИКЕ (300+ строк)
4. HomeMenuController.cs          - 🔧 ВЕЛИКЕ (250+ строк)
5. MultiplayerGameController.cs   - 🚀 АКТИВНА (мережа)
6. GeneratorDataRegistry.cs       - 🚀 АКТИВНА (генерація)
7. UnitController.cs              - 🔧 ВЕЛИКЕ (200+ строк)
8. BotAiService.cs                - 🔧 АКТИВНА (AI)
9. FogOfWarService.cs             - 🔧 ВЕЛИКЕ (туман)
10. GraphExecutor.cs              - 🚀 АКТИВНА (граф вузлів)
```

---

## ⚙️ РЕФАКТОРИНГ: ТОП-10 ПРІОРИТЕТІВ

```
1. Economy (52 скрипти)
   └─ 🔴 ПЕРЕПИСУЄТЬСЯ - розбити на сервіси ✅

2. Construction (58 скриптів)
   └─ 🔧 Розбити ConstructionGridService → 2 класи

3. HomeMenu (114 скриптів)
   └─ 🔧 Розбити контролери → меньші компоненти

4. Units (30 скриптів)
   └─ 🔧 Розбити UnitController → 4 сервіси

5. WorldCreation (14 скриптів)
   └─ 🔧 Розбити на фази генерації

6. Generator (168 скриптів)
   └─ 🚀 Оптимізувати алгоритми

7. Multiplayer (76 скриптів)
   └─ 🚀 Оптимізувати синхронізацію

8. FogOfWar (22 скрипти)
   └─ 🔧 Оптимізувати алгоритм видимості

9. BotAI (10 скриптів)
   └─ 🔧 Розбити дерево рішень

10. GraphSystem (37 скриптів)
    └─ 🚀 Додати кешування результатів
```

---

## 🧪 ТЕСТУВАННЯ

### Запустити ВСІ тести
```bash
dotnet test --no-restore --verbosity normal
```

### Запустити тести ОДНІЄЇ модуля
```bash
# Приклади:
dotnet test Kruty1918.Moyva.Tests.Economy.csproj
dotnet test Kruty1918.Moyva.Tests.Construction.csproj
dotnet test Kruty1918.Moyva.Tests.BotAI.csproj
```

### Запустити КОНКРЕТНИЙ тест
```bash
dotnet test --filter "ClassName"
dotnet test --filter "TestMethodName"
```

---

## 🔍 ПОШУК ФАЙЛУ

### За ім'ям
```bash
find Assets/Moyva/Scripts -name "*YourSearchTerm*" -type f
```

### За змістом (grep)
```bash
grep -r "YourSearchTerm" Assets/Moyva/Scripts --include="*.cs"
```

### За типом класу
```bash
# Всі інтерфейси
find Assets/Moyva/Scripts -name "I*.cs" | wc -l

# Всі SO конфіги
find Assets/Moyva/Scripts -name "*SO.cs" | wc -l

# Всі тести
find Assets/Moyva/Scripts/Tests -name "*Test*.cs" | wc -l
```

---

## 📊 СТАТИСТИКА ПО ТИПАМ

| Тип | Кількість | Приклад |
|-----|-----------|---------|
| **Interfaces** | ~90 | `IGridService.cs`, `IEconomyService.cs` |
| **ScriptableObjects** | ~45 | `EconomyDatabaseSO.cs`, `CameraSettingsSO.cs` |
| **Services/Managers** | ~120 | `EconomyManager.cs`, `GridService.cs` |
| **MonoBehaviour** | ~180 | `CameraController.cs`, `GridView.cs` |
| **Data Models** | ~130 | `EconomySettlementState.cs`, `BuildingDefinition.cs` |
| **Utilities** | ~70 | `MapArrayUtils.cs`, `BiomeResolver.cs` |
| **Nodes** | ~75 | `FBMNoiseNode.cs`, `ErosionNode.cs` |
| **UI Components** | ~85 | `MenuButtonWidget.cs`, `SettingsPanel.cs` |
| **Tests** | ~55 | `EconomyTests.cs`, `GridTests.cs` |
| **Editor Tools** | ~65 | `BuildingDefinitionEditor.cs`, `NodeEditor.cs` |

---

## 🎓 ПРОЧИТАТИ ПЕРШИМ

1. **SCRIPTS_MANAGEMENT_PLAN.md** - Загальний план управління
2. **DETAILED_SCRIPTS_LIST.md** - Детальний перелік з рекомендаціями
3. **THIS FILE** - Швидка навігація

---

## 📞 НАЙПОПУЛЯРНІШІ КЛАСИ (в інших модулях використовуються часто)

```
✅ SignalBus          - Event bus (усі модулі)
✅ IGridService       - Сітка (Construction, Units, FogOfWar)
✅ ICalendarService   - Календар (Economy, GameMode)
✅ IBuildingRegistry  - Будівлі (Construction, Economy)
✅ IFactionRegistry   - Фракції (BotAI, Economy)
✅ ISaveService       - Зберігання (усі модулі)
```

---

## 🚨 КРИТИЧНІ СИСТЕМИ (тестувати завжди)

1. **Signals** - Якщо зломіть pub/sub, упаде все
2. **Grid** - Координати використовуються скрізь
3. **SaveSystem** - Втрата даних 💀
4. **Calendar** - Хідці залежять від цього
5. **Economy** - ПЕРЕПИСУЄТЬСЯ зараз 🔴

---

## 💡 ПРИКЛАДИ ЗМІН

### Додати новий параметр в Economy
```csharp
// 1. EconomyResourceDefinition.cs (API/)
public class EconomyResourceDefinition {
    public string ResourceId { get; set; }
    public float NewParameter { get; set; }  // ← ДОДАЄМО
}

// 2. EconomyManager.cs (Runtime/)
void ProcessTurn() {
    var newParam = _database.GetResource(id).NewParameter;
    // Використовуємо newParam...
}

// 3. EconomyTests.cs
[Test]
public void TestNewParameter() {
    var def = new EconomyResourceDefinition { NewParameter = 100 };
    Assert.AreEqual(100, def.NewParameter);
}
```

### Оптимізувати алгоритм
```csharp
// БУЛО (повільно):
for (int i = 0; i < 1000; i++) {
    var result = ExpensiveCalculation();
}

// СТАЛО (швидко):
var cache = new Dictionary<int, Result>();
for (int i = 0; i < 1000; i++) {
    if (!cache.TryGetValue(i, out var result)) {
        result = ExpensiveCalculation();
        cache[i] = result;
    }
}
```

---

## 📚 ДОКУМЕНТАЦІЯ

- **README.md** в кожній папці Features/ (якщо є)
- **XML comments** над публічними методами
- **Graph diagrams** для сложних систем (Generator, Multiplayer)
- **Configuration examples** в ScriptableObjects

---

**ВЕРСІЯ**: v2.0  
**ДАТА**: 14 травня 2026 р.  
**STATУС**: Готовий до використання ✅
