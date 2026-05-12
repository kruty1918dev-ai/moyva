# Система детальної інформації про амбар

## Опис функціональності

Коли гравець клікає на амбар, він бачить:

1. **Статистика населення**:
   - Всього жителів
   - Кількість дітей, дорослих, пенсіонерів
   - Кількість доступних робітників

2. **Розподіл робітників** - як розподілені робітники по будівлям

3. **Деталі жителів** - список перших 10 жителів з:
   - Віком
   - Статусом (Дитина, Робітник, Пенсіонер, Без дома)
   - HP (здоров'ям)

## Архітектура

### Компоненти системи

#### 1. Утиліта статистики (`BarnSettlementStatistics.cs`)
- `CalculateStatistics()` - обчислення статистики населення
- `FormatBarnInfoDetailed()` - форматування детальної інформації з іконками

Структури:
- `BarnStatistics` - агрегирована статистика
- `ResidentInfo` - інформація про одного жителя

#### 2. Presenter (`BuildingWorldInfoPresenter.cs`)
- Розширена логіка для амбарів
- Визначає тип будівлі та показує відповідну інформацію

### Потік даних

```
Клік на амбар
    ↓
BuildingInfoPanelRequestedSignal
    ↓
BuildingWorldInfoPresenter.OnBuildingInfoRequested()
    ↓
BuildingDefinitionCapabilities.IsBarn() - перевірка типу
    ↓
BarnSettlementStatistics.FormatBarnInfoDetailed()
    ↓
Вивід інформації про населення
```

## Як це працює

### 1. Визначення типу будівлі

```csharp
if (BuildingDefinitionCapabilities.IsBarn(definition))
{
    // Отримати стан поселення
    var settlementState = /* ... */
    // Форматувати інформацію про жителів
    return BarnSettlementStatistics.FormatBarnInfoDetailed(settlementState);
}
```

### 2. Обчислення статистики

Система аналізує список жителів (`EconomySettlementState.Residents`):
- **Дітей**: вік < 16
- **Дорослих**: 16 <= вік < 60
- **Пенсіонерів**: вік >= 60
- **Робітників**: доступні = дорослі без збитків дому

### 3. Форматування інформації

```
═══ АМБАР: Назва ═══

👥 НАСЕЛЕННЯ:
  • Всього: 45
  • Дітей: 10
  • Дорослих: 28
  • Пенсіонерів: 7
  • Доступних робітників: 25

🏗️ РОЗПОДІЛ РОБІТНИКІВ:
  • farm-1: 5 робітників
  • workshop-2: 8 робітників

📋 ДЕТАЛІ ЖИТЕЛІВ:
  👧 7р. • Дитина • HP: 100
  👨 23р. • Робітник • HP: 98.5
  👨 45р. • Робітник • HP: 100
  👴 68р. • Пенсіонер • HP: 87
  ...
```

## Функціональні можливості

### Статуси жителів

| Статус | Іконка | Умова |
|--------|--------|-------|
| Дитина | 👧 | Вік < 16 |
| Робітник | 👨 | 16 <= Вік < 60 |
| Пенсіонер | 👴 | Вік >= 60 |
| Без дома | 🏚️ | HouseCollapsed = true |

### Показана інформація

- **Всього жителів** - повна кількість населення
- **Розбиття по категоріям** - точна кількість для кожної групи
- **Робітники** - скільки можна використати для роботи
- **Розподіл** - де зараз призначені робітники
- **Деталі** - список перших 10 з віком та HP

## Розширення у майбутньому

1. **Більше деталей про кожного жителя**:
   - Їхній дім (якщо є)
   - Поточна професія/роль
   - Рівень комфорту
   - Вживання ресурсів

2. **Фільтрування**:
   - По віку
   - По статусу
   - По назначеню на роботу

3. **Управління населенням**:
   - Переміщення між будівлями
   - Звільнення з робіт
   - Переселення в нове житло

4. **Статистика**:
   - Громадяни по домам
   - Рівень задоволення
   - Показатель смертності/народжуваності

## Технічні деталі

### EconomySettlementState.Residents

```csharp
public List<EconomyResidentState> Residents;
```

Кожен житель має:
- `Age` - вік
- `Hp` - здоров'я (0-100)
- `Comfort` - комфорт
- `HouseCollapsed` - флаг зруйнованого дому

### WorkerAssignments

```csharp
public Dictionary<string, int> WorkerAssignments;
```

Ключ = ID будівлі, Значення = кількість робітників

## Тестування

Система готова до тестування. Щоб протестувати:

1. Запустіть гру
2. Розмістіть амбар у поселенні
3. Клікніть на амбар
4. Перевірте що:
   - ✅ Показується статистика населення
   - ✅ Видно розподіл робітників
   - ✅ Список жителів коректний
   - ✅ Іконки відповідають статусам

## Залежності

- `EconomyManager` - для отримання стану поселення
- `EconomySettlementState` - дані про жителів
- `BuildingDefinitionCapabilities` - перевірка типу будівлі

## API для інших модулів

### BarnSettlementStatistics

```csharp
/// Обчислити статистику жителів
public static BarnStatistics CalculateStatistics(EconomySettlementState settlementState)

/// Отримати форматовану рядок
public static string FormatBarnInfoDetailed(EconomySettlementState settlementState)
```

### Структури даних

```csharp
public struct BarnStatistics
{
    public int TotalPopulation;
    public int WorkersAvailable;
    public int ChildrenCount;
    public int AdultsCount;
    public int ElderlyCount;
    public Dictionary<string, int> WorkersByBuilding;
    public List<ResidentInfo> Residents;
}

public struct ResidentInfo
{
    public int Age;
    public float Hp;
    public float Comfort;
    public bool HouseCollapsed;
    public string Status;
}
```
