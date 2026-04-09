# Fog of War — Гайд дизайнера: FogOfWarSettings

← [README](README.md)

---

## Як створити ScriptableObject

1. У Project window → правий клік → `Create → Moyva → FogOfWarSettings`
2. Назвіть файл, наприклад: `FogOfWarSettings_Default`
3. Збережіть у `Assets/Moyva/Settings/` або поруч зі сценою

---

## Vision Tuner (Editor Tool)

Окрему документацію по інструменту винесено в:

[vision-tuner-guide.md](vision-tuner-guide.md)

Там описані всі секції, параметри та тултіпи, а також рекомендований workflow.

---

## Поля та рекомендовані значення

### Vision

| Поле | Тип | За замовчуванням | Опис |
|---|---|---|---|
| `DefaultVisionRange` | int | 5 | Радіус зору за замовчуванням для систем, які не передали власний радіус |
| `MinVisionRange` | int | 1 | Мінімально дозволений радіус. Для поточної системи має бути `1` |
| `MaxVisionRange` | int | 12 | Верхня межа ефективного радіуса після бонусів від висоти |

### Height Vision

| Поле | Тип | Рекомендація | Опис |
|---|---|---|---|
| `ElevationStep` | float | `0.15` або `0.25` | Крок різниці висот, після якого застосовується бонус або штраф |
| `ObserverHeightBonusPerStep` | int | `1` | Скільки радіуса отримує юніт за кожен крок висоти під ним |
| `DownhillVisionBonusPerStep` | int | `1` | Додатковий бонус, якщо ціль нижче спостерігача |
| `UphillVisionPenaltyPerStep` | int | `1` | Штраф, якщо ціль вища за спостерігача |
| `MaxObserverHeightBonus` | int | `4` | Стеля бонусу від висоти тайлу юніта |
| `MaxDownhillVisionBonus` | int | `2` | Стеля бонусу за погляд вниз |
| `MaxUphillVisionPenalty` | int | `6` | Максимальний штраф при погляді вгору |
| `OcclusionSlopeBias` | float | `0.02` | Малий допуск, щоб дрібні коливання висоти не блокували видимість занадто агресивно |

### Практичне правило

Якщо у вас нормалізована `HeightMap` у діапазоні `0..1`, зазвичай варто почати з таких значень:

| Сценарій | `ElevationStep` |
|---|---|
| М'який рельєф, невеликі перепади | `0.15` |
| Більш контрастні пагорби й висоти | `0.25` |

Менший `ElevationStep` робить зір чутливішим до висот. Більший робить систему стабільнішою і менш "нервовою".

### Fog Colors

| Поле | Тип | За замовчуванням | Опис |
|---|---|---|---|
| `UnexploredFogColor` | Color | (0.03, 0.03, 0.08, 1.0) | Колір непізнаної зони. Темно-синій, майже чорний |
| `ExploredFogColor` | Color | (0.08, 0.10, 0.14, 0.65) | Колір пізнаної зони. Напівпрозорий сіро-синій |

### Perlin Noise — Unexplored

| Поле | Що змінює візуально |
|---|---|
| `NoiseScaleUnexplored` | Більше = дрібніші хмари туману, менше = великі пласти |
| `NoiseSpeedUnexplored` | Швидкість анімації. 0 = статичний туман |
| `NoiseStrengthUnexplored` | Амплітуда мерехтіння alpha. 0 = рівна заливка |

**Рекомендовано:** Scale=3.5, Speed=0.04, Strength=0.25

### Perlin Noise — Explored

| Поле | Що змінює візуально |
|---|---|
| `NoiseScaleExplored` | Масштаб хмар у пізнаній зоні |
| `NoiseSpeedExplored` | Більш повільний рух для "старого" туману |
| `NoiseStrengthExplored` | Легше мерехтіння, щоб не відволікати від гри |

**Рекомендовано:** Scale=2.0, Speed=0.02, Strength=0.15

### Edge Bleeding

| Поле | Опис |
|---|---|
| `EdgeBleedRadius` | Наскільки далеко туман "заходить" на видимий тайл від сусіднього туманного |
| `EdgeBleedStrength` | Сила розмиття краю. 0 = чіткий край |

**Рекомендовано:** Radius=0.35, Strength=0.40

### Transitions

| Поле | Опис |
|---|---|
| `TransitionSoftness` | М'якість переходу між зонами. 0 = різкий, 0.5 = максимально плавний |

**Рекомендовано:** 0.12

### Texture

| Поле | Опис |
|---|---|
| `TextureFilter` | `Bilinear` — плавний перехід між тайлами; `Point` — піксельний стиль |

---

## Як призначити SO у Inspector

### FogOfWarInstaller

1. Виберіть GameObject зі Scene Context (або окремий)
2. На компоненті `FogOfWarInstaller` знайдіть поле `Settings`
3. Перетягніть ваш `FogOfWarSettings` SO

### FogQuadController

1. Виберіть `FogOfWarQuad` GameObject
2. На компоненті `FogQuadController` знайдіть поле `Settings`
3. Перетягніть той самий `FogOfWarSettings` SO

---

## Де задається радіус юніта

Базовий радіус конкретного юніта задається не в `FogOfWarSettings`, а в `UnitClassConfig.VisionRange`.

`FogOfWarSettings` лише задає:

| Поле | Роль |
|---|---|
| `DefaultVisionRange` | fallback для випадків без явного радіуса |
| `MinVisionRange` / `MaxVisionRange` | глобальні обмеження системи |
| Height Vision параметри | правила бонусів, штрафів і line of sight |

---

## Типові помилки

| Проблема | Причина | Рішення |
|---|---|---|
| Туман весь чорний, не оновлюється | SO не призначено або Material не призначено | Перевірте Inspector |
| Юніти не впливають на туман | `FogOfWarInstaller` не доданий до Scene Context | Додайте інсталятор |
| Туман оновлюється, але немає ефекту шуму | Шейдер не `Moyva/FogOfWar` | Перевірте матеріал |
| Юніт бачить занадто далеко або занадто близько | Невдало підібраний `ElevationStep` або `Max*Bonus/Penalty` | Підженіть Height Vision параметри під реальний діапазон `HeightMap` |
| Юніт із низини не бачить пагорб | Це може бути коректно: спрацював `UphillVisionPenalty` або блокування line of sight | Перевірте висоти тайлів і `OcclusionSlopeBias` |
| `DefaultVisionRange` не застосовується | FogOfWarService ініціалізується без SO | Перевірте прив'язку SO в FogOfWarInstaller |
