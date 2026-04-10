# Calendar — Гайд по редактору (Calendar Config Hub)

← [Назад до огляду](README.md)

---

## Відкриття редактора

Меню Unity: **Moyva → Calendar → Config Hub**

Або через пошук: `Ctrl+P` → `Calendar Config Hub`.

---

## Огляд панелей

### 1. Calendar Structure

| Поле | Опис |
|---|---|
| Months per Year | Скільки місяців у ігровому році (1–24) |
| Days per Month | Днів у кожному місяці (1–60, фіксоване для всіх місяців) |
| Hours per Day | Годин у добі (1–48; для реалізму зазвичай 24) |

### 2. Start Date / Time

| Поле | Опис |
|---|---|
| Start Year | Рік початку нової гри |
| Start Month | Місяць початку (1..Months per Year) |
| Start Day | День початку (1..Days per Month) |
| Start Hour | Година початку (0..Hours per Day - 1) |

### 3. Day / Night Boundaries

| Поле | Опис |
|---|---|
| Day Start Hour | З якої години починається день |
| Night Start Hour | З якої години починається ніч |
| Dawn Duration (hours) | Скільки годин тривають світанкові переходи |
| Dusk Duration (hours) | Скільки годин тривають сутінки |

Формула:
- Світанок: `[DayStart - DawnDuration, DayStart)`
- День: `[DayStart, NightStart - DuskDuration)`
- Сутінки: `[NightStart - DuskDuration, NightStart)`
- Ніч: все інше

### 4. Multiplayer / Turns

| Поле | Опис |
|---|---|
| Hours per Turn | Скільки ігрових годин за один хід |
| (readonly label) | Скільки ходів у грі = 1 ігрова доба |

**Приклад:** Hours per Day = 24, Hours per Turn = 1 → 24 ходи = 1 доба.

### 5. Day Phase Preview

Слайдер **Preview Hour** дозволяє перевірити, яка фаза буде активна в певну годину.  
Колір фону мітки змінюється:

| Фаза | Колір фону |
|---|---|
| Day | Жовтий (сонячний) |
| Night | Темно-синій |
| Dawn | Помаранчевий |
| Dusk | Фіолетовий |

---

## Валідація

Редактор показує попередження / помилки прямо в панелі:

| Умова | Тип |
|---|---|
| Day Start Hour ≥ Night Start Hour | ❌ Error (кнопка Save заблокована) |
| Dawn Duration ≥ Day Start Hour | ⚠ Warning |
| Night Start + Dusk Duration > Hours in Day | ⚠ Warning |

---

## Кнопки

| Кнопка | Дія |
|---|---|
| **Save Config** | Зберігає конфіг у `Assets/Moyva/calendar_config.dat` |
| **Load Config** | Завантажує конфіг із файлу (або дефолт якщо файл відсутній) |
| **Reset to Defaults** | Застосовує `CalendarConfig.Default()` без збереження |

Після натискання **Save Config** автоматично викликається `AssetDatabase.Refresh()`, тому Unity-провідник оновлюється.

---

## Ручні кроки після налаштування конфігу

1. **Перевірте**, що файл `Assets/Moyva/calendar_config.dat` з'явився у провіднику Unity.
2. **Додайте** `CalendarInstaller` (ваш Zenject MonoInstaller) до сцени гри або `GameContext`.
3. **Вкажіть** шлях `"Assets/Moyva/calendar_config.dat"` або залиште дефолт (`Application.persistentDataPath`) в залежності від вашої стратегії деплою.
4. Якщо ви додаєте шейдер день/ніч — підпишіть його на `ICalendarService.OnDayPhaseChanged`. Детальніше: [usage-examples.md](usage-examples.md).

---

## Де зберігається файл конфігурації

| Контекст | Шлях |
|---|---|
| Editor (Config Hub) | `Assets/Moyva/calendar_config.dat` |
| Runtime (за замовчуванням) | `Application.persistentDataPath/calendar_config.dat` |
| Тести | Передається як аргумент конструктора |

> **Примітка:** файл `.dat` є бінарним — не редагуйте вручну. Для змін завжди використовуйте Config Hub.
