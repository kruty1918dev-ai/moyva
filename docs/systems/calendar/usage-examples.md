# Calendar — Приклади використання

← [Назад до огляду](README.md)

---

## 1. Шейдер дня/ночі (DayNight Shader)

Calendar не містить шейдерів — він лише надає `DayPhase` і `GameDateTime`.  
Ваш шейдерний контролер підписується на події:

```csharp
public sealed class DayNightShaderController : MonoBehaviour
{
    [SerializeField] private Material _skyMaterial;

    [Inject] private ICalendarService _calendar;

    private void Start()
    {
        _calendar.OnDayPhaseChanged += ApplyPhase;
        ApplyPhase(_calendar.CurrentDayPhase);   // ініціалізація при старті
    }

    private void OnDestroy()
    {
        _calendar.OnDayPhaseChanged -= ApplyPhase;
    }

    private void ApplyPhase(DayPhase phase)
    {
        float blend = phase switch
        {
            DayPhase.Day   => 1.0f,
            DayPhase.Night => 0.0f,
            DayPhase.Dawn  => 0.25f,
            DayPhase.Dusk  => 0.75f,
            _              => 0.5f
        };
        _skyMaterial.SetFloat("_DayNightBlend", blend);
    }
}
```

> **Ручний крок:** Додайте `DayNightShaderController` до GameObject у сцені і призначте Material у Inspector.  
> Шейдер та матеріал зберігайте у папці `Assets/Moyva/Visuals/DayNight/`.

---

## 2. UI Годинник / Дата

```csharp
public sealed class CalendarUIView : MonoBehaviour
{
    [SerializeField] private Text _dateLabel;
    [SerializeField] private Text _phaseLabel;

    [Inject] private ICalendarService _calendar;

    private void Start()
    {
        _calendar.OnHourChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        _calendar.OnHourChanged -= Refresh;
    }

    private void Refresh()
    {
        GameDateTime dt = _calendar.Current;
        _dateLabel.text  = $"Year {dt.Year}, Month {dt.Month:D2}, Day {dt.Day:D2}  {dt.Hour:D2}:00";
        _phaseLabel.text = _calendar.CurrentDayPhase.ToString();
    }
}
```

---

## 3. AI — Нічний спавн ворогів

```csharp
public sealed class NightSpawner : MonoBehaviour
{
    [Inject] private ICalendarService _calendar;

    private void Start()
    {
        _calendar.OnDayPhaseChanged += OnPhaseChanged;
    }

    private void OnDestroy()
    {
        _calendar.OnDayPhaseChanged -= OnPhaseChanged;
    }

    private void OnPhaseChanged(DayPhase phase)
    {
        if (phase == DayPhase.Night)
        {
            SpawnNightEnemies();
        }
        else if (phase == DayPhase.Dawn)
        {
            DespawnNightEnemies();
        }
    }

    private void SpawnNightEnemies() { /* ... */ }
    private void DespawnNightEnemies() { /* ... */ }
}
```

---

## 4. Економіка — Ціни змінюються в залежності від часу доби

```csharp
public sealed class MarketPriceModifier
{
    private readonly ICalendarService _calendar;

    public MarketPriceModifier(ICalendarService calendar)
    {
        _calendar = calendar;
    }

    public float GetPriceMultiplier()
    {
        return _calendar.CurrentDayPhase switch
        {
            DayPhase.Day  => 1.0f,   // стандартна ціна
            DayPhase.Dawn => 0.9f,   // ранкова знижка
            DayPhase.Dusk => 1.1f,   // вечірня надбавка
            DayPhase.Night => 1.3f,  // нічна надбавка
            _ => 1.0f
        };
    }
}
```

---

## 5. Квести — Умова "виконати вночі"

```csharp
public sealed class NightQuestCondition
{
    private readonly ICalendarService _calendar;

    public NightQuestCondition(ICalendarService calendar)
    {
        _calendar = calendar;
    }

    public bool IsNightTime() => _calendar.CurrentDayPhase == DayPhase.Night;

    public GameDateTime QuestDeadline(int daysFromNow)
    {
        // Обчислення дедлайну: поточний день + N
        GameDateTime now = _calendar.Current;
        long totalHours  = _calendar.TotalHoursSinceEpoch
                           + (long)daysFromNow * _calendar.Config.HoursInDay;
        return GameCalendarService.ComputeDateTime(_calendar.Config, totalHours);
    }
}
```

---

## 6. Перевірка часу доби без підписки (pull-based)

```csharp
// Якщо вам не потрібні події — просто читайте стан:
void Update()
{
    if (_calendar.CurrentDayPhase == DayPhase.Night)
    {
        // ...
    }
}
```

---

## Зведена таблиця події → реакція

| Подія | Кого слухає | Що робить |
|---|---|---|
| `OnHourChanged` | UI (clock) | Оновлює відображення часу |
| `OnDayChanged` | Economy / Quests | Новий ігровий день — reset cooldowns |
| `OnMonthChanged` | Economy | Зміна сезонних цін |
| `OnYearChanged` | Game Stats | Збільшення лічильника років |
| `OnDayPhaseChanged` | Shader, AI, Music | Зміна візуального та ігрового режиму |
