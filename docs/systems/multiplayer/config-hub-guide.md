# Multiplayer Config Hub — Повний гайд

← [Назад до огляду](README.md)

---

## Для кого цей документ

Для геймдизайнерів, технічних гейм-дизайнерів і розробників, які хочуть:
- налаштувати параметри мультиплеєра без написання коду,
- зберегти конфіг у бінарний файл,
- перевірити правильність налаштувань перед запуском гри.

---

## Як відкрити

У Unity Editor:

```
Moyva → Multiplayer → Config Hub
```

Мінімальний розмір вікна: **480 × 520 px**.

Якщо файл конфігурації ще не існує, відкриється вікно з **дефолтними значеннями**.

---

## Де зберігається конфіг

```
Assets/Moyva/multiplayer_config.dat
```

Це бінарний файл. Його не можна читати/редагувати як текст.  
Для перегляду і редагування використовуйте виключно **Config Hub**.

---

## Інтерфейс вікна

### 1. Заголовок

```
Multiplayer Configuration Hub
```

Інформаційна підказка нагадує, що налаштування зберігаються у бінарному файлі.

---

### 2. Розділ «Network Provider»

#### Provider Type

Визначає, який мережевий транспорт використовується.

| Значення | Коли використовувати |
|---|---|
| `Offline` | Одиночна гра або тестування без мережі |
| `Relay` | Unity Relay — хмарне з'єднання, NAT traversal |
| `Mirror` | Mirror Networking — LAN або прямий IP |

> Поточна реалізація підтримує лише `Offline` (конкретний код для Relay/Mirror буде доданий пізніше).  
> Значення зберігається і використовується для вибору провайдера під час запуску.

---

### 3. Розділ «Session Rules»

#### Default Session Mode

Режим гри за замовчуванням для нових сесій.

| Значення | Опис |
|---|---|
| `PeacefulSolo` | Один гравець без ботів |
| `SoloWithBots` | Один гравець + боти |
| `MultiplayerHumans` | Лише люди |
| `MixedHumansAndBots` | Люди + боти |

---

#### Max Participants (1–4)

Максимальна кількість учасників у сесії **включно з ботами**.

- Жорстке обмеження: **не більше 4**.
- Якщо поставити > 4, з'явиться помилка і кнопка **Save Config** буде вимкнена.

---

#### Max Humans (0–4)

Максимальна кількість гравців-людей.  
Повинна бути ≤ **Max Participants**.

---

#### Max Bots (0–4)

Максимальна кількість ботів.  
Повинна бути ≤ **Max Participants**.

> **Важливо:** `Max Humans + Max Bots` повинно бути ≤ `Max Participants`.  
> При порушенні виводиться **Error** і збереження блокується.

---

### 4. Розділ «Flags»

#### Strict 4-Player World Lock

Якщо `true` — певний світ може бути продовжений лише тими самими 4 гравцями, які починали.

Приклад:
- Якщо `Player A, B, C, D` грали у **World-001**, то лише ці четверо можуть відновити World-001.
- Новий гравець `Player E` не зможе зайти, навіть якщо слот порожній.
- Клони цього світу можуть мати інші правила.

> ⚠️ Якщо `Strict 4-Player Lock = true` і `Max Bots > 0` — виводиться **Warning**: боти і strict lock можуть конфліктувати при відновленні сесії.

---

#### Allow Bots Fallback On Leave

Якщо `true` — коли гравець-людина виходить, його місце займає бот.  
При поверненні гравця — бот видаляється.

---

#### Enable Matchmaking

Якщо `true` — система дозволяє пошук випадкових матчів (matchmaking).  
Поточна реалізація: лише прапорець конфігу; фактична логіка matchmaking буде реалізована пізніше.

---

#### Allow Match Save For Analysis

Якщо `true` — після завершення матчу гравцям дозволяється зберегти стан матчу для аналізу.

---

#### Enforce Config Consistency

Якщо `true` — при вході до сесії клієнт надсилає **checksum** свого конфігу.  
Хост порівнює checksums:

- Якщо збігаються → підключення дозволено.
- Якщо не збігаються → клієнт отримує відмову (або ініціюється синхронізація конфігу у майбутньому).

Вимикати тільки для дебагу або тестів з різними конфігами.

---

### 5. Блок валідації

Відображається автоматично під прапорцями.  
Кольорова підказка:

| Тип | Колір | Значення |
|---|---|---|
| **Error** | Червоний | Налаштування заблоковані, Save Config вимкнений |
| **Warning** | Жовтий | Потенційна проблема, збереження дозволено |
| **Info** | Синій | Рекомендація |

**Перелік можливих повідомлень:**

| Ситуація | Тип |
|---|---|
| `Max Participants > 4` | Error |
| `Max Humans + Max Bots > Max Participants` | Error |
| `Strict 4-Player Lock = true` і `Max Bots > 0` | Warning |
| `PeacefulSolo` і `Max Participants > 1` | Info |

---

### 6. Кнопки

#### Save Config

Зберігає поточні налаштування у файл `Assets/Moyva/multiplayer_config.dat`.

- Вимкнена при наявності **Error**.
- Після збереження викликається `AssetDatabase.Refresh()`.

---

#### Load Config

Перезавантажує налаштування з файлу.  
Усі незбережені зміни у вікні будуть **скасовані**.

---

#### Reset to Defaults

Скидає усі поля до дефолтних значень:

| Поле | Дефолт |
|---|---|
| Provider Type | `Offline` |
| Default Session Mode | `MultiplayerHumans` |
| Max Participants | `4` |
| Max Humans | `4` |
| Max Bots | `0` |
| Strict 4-Player Lock | `false` |
| Allow Bots Fallback | `false` |
| Enable Matchmaking | `false` |
| Allow Match Save | `false` |
| Enforce Config Consistency | `true` |

> **Reset to Defaults** не зберігає автоматично — потрібно натиснути **Save Config** окремо.

---

## Покрокові сценарії

### Сценарій 1: Налаштувати локальну offline-гру 2-гравців

1. Відкрити `Moyva → Multiplayer → Config Hub`.
2. Provider Type → `Offline`.
3. Default Session Mode → `MultiplayerHumans`.
4. Max Participants → `2`.
5. Max Humans → `2`, Max Bots → `0`.
6. Усі прапорці → `false`.
7. Натиснути **Save Config**.

---

### Сценарій 2: Налаштувати гру з ботами (2 людини + 2 боти)

1. Відкрити Config Hub.
2. Default Session Mode → `MixedHumansAndBots`.
3. Max Participants → `4`.
4. Max Humans → `2`.
5. Max Bots → `2`.
6. Allow Bots Fallback On Leave → `true`.
7. Натиснути **Save Config**.

---

### Сценарій 3: Strict 4-player world lock

1. Відкрити Config Hub.
2. Max Participants → `4`, Max Humans → `4`, Max Bots → `0`.
3. **Strict 4-Player World Lock** → `true`.
4. Enforce Config Consistency → `true`.
5. Натиснути **Save Config**.

> Тепер лише ті самі 4 гравці можуть продовжити конкретний збережений світ.

---

### Сценарій 4: Matchmaking

1. Відкрити Config Hub.
2. Provider Type → `Relay`.
3. Default Session Mode → `MultiplayerHumans`.
4. **Enable Matchmaking** → `true`.
5. **Allow Match Save For Analysis** → `true`.
6. Натиснути **Save Config**.

---

## Технічні деталі

### Де зберігається файл

```
Assets/Moyva/multiplayer_config.dat
```

Шлях жорстко прописаний у `MultiplayerConfigEditorWindow` як константа `ConfigPath`.

---

### Формат файлу

Бінарний, sequential, без стиснення:

```
[int32]  SchemaVersion
[int32]  ProviderType (enum int)
[bool]   StrictParticipantLock
[bool]   EnforceConfigConsistency
[bool]   MatchmakingEnabled
[int32]  SessionRules.Mode (enum int)
[int32]  SessionRules.MaxParticipants
[int32]  SessionRules.MaxHumans
[int32]  SessionRules.MaxBots
[bool]   SessionRules.AllowBotsFallbackOnLeave
[bool]   SessionRules.AllowMatchSaveForAnalysis
[bool]   SessionRules.StrictParticipantLock
```

Версія схеми: `MultiplayerConfig.CurrentSchemaVersion = 1`.

---

### Runtime завантаження

```csharp
var store = new BinaryConfigStore("Assets/Moyva/multiplayer_config.dat");
var config = store.Load();  // повертає Default() якщо файл не існує
```

`BinaryConfigStore` є runtime-safe — не залежить від `UnityEditor`.  
Шлях за замовчуванням: `Application.persistentDataPath/multiplayer_config.dat`.

---

## Файл коду

```
Assets/Moyva/Scripts/Features/Multiplayer/Editor/MultiplayerConfigEditorWindow.cs
```

Assembly: `Kruty1918.Moyva.Multiplayer.Editor` (Editor-only).
