# Fog of War — Алгоритм видимості

← [README](README.md)

---

## Height-Aware Visibility

Поточний алгоритм видимості побудований навколо двох рівнів логіки:

1. `FogVisibilityResolver` перебирає тайли в квадраті пошуку навколо юніта.
2. `HeightAwareVisionService` вирішує, чи справді тайл видно, з огляду на висоту, штрафи, бонуси й line of sight.

Мапа лишається 2D, але кожен тайл має координати `X/Y` і висоту `Z`. Юніт вважається таким, що стоїть на поверхні свого тайла: його точка огляду дорівнює висоті тайла плюс `ObserverEyeHeightOffset`, а ціль перевіряється на висоті поверхні плюс `TargetSampleHeightOffset`.

---

## Базові правила

| Правило | Поведінка |
|---|---|
| Мінімальний радіус | `visionRange` затискається до `1` |
| Один рівень висоти | Видимість без штрафу |
| Спостерігач вище | Може бачити далі |
| Ціль вище | Дальність до цілі зменшується |
| Рельєф між точками | Може повністю закрити видимість |
| Край висоти | Створює локальний горизонт і blind zone під схилом |
| Ціль на верхньому краї | Може бути видимою знизу як crest/silhouette |

---

## Обчислення радіуса пошуку

Спершу система рахує не фактичну видимість, а максимальний радіус, у якому варто перевіряти тайли.

Формула на рівні ідеї така:

```text
searchRadius = clamp(
    baseVisionRange
    + observerHeightBonus
    + unitDownSlopeVisionBonus,
    1,
    maxVisionRange)
```

`observerHeightBonus` залежить від висоти тайлу, на якому стоїть юніт. `unitDownSlopeVisionBonus` приходить із `UnitClassConfig.DownSlopeVisionBonus` і тільки розширює область пошуку; фактичний бонус для конкретної цілі все одно перевіряється геометрією краю.

---

## Перевірка конкретного тайлу

Для кожного тайлу-кандидата система рахує ефективну дальність до нього:

```text
effectiveRange = clamp(
    baseVisionRange
    + observerHeightBonus
    + downhillVisionBonus
    + directionalDownSlopeVisionBonus
    - uphillVisionPenalty,
    0,
    maxVisionRange)
```

Якщо відстань до цілі більша за `effectiveRange`, тайл одразу вважається невидимим.

Відстань у поточній реалізації рахується через Chebyshev metric:

```text
distance = max(abs(dx), abs(dy))
```

---

`directionalDownSlopeVisionBonus` працює лише коли ціль нижче, а між спостерігачем і ціллю є downhill edge, до якого спостерігач стоїть достатньо близько. `uphillVisionPenalty` може бути зменшений, якщо ціль стоїть на верхньому краї або її силует відкритий.

## Line of Sight по HeightMap

Після перевірки дальності виконується line of sight уздовж променя між центром тайла спостерігача і кількома точками всередині тайла цілі.

Алгоритм:

1. Береться `originSightHeight = height(origin) + ObserverEyeHeightOffset`.
2. Для цілі береться кілька sample points: центр, кути й середини країв залежно від `TerrainRaySamplesPerTile`.
3. Для кожного sample point обчислюється `targetSightHeight`:

```text
targetSightHeight = terrainHeight(targetPoint)
    + TargetSampleHeightOffset
    + silhouetteHeightOffset
```

4. Уздовж променя з кроком `TerrainRayStepTiles` порівнюється очікувана висота лінії погляду з висотою рельєфу:

```text
expectedSightHeight = lerp(originSightHeight, targetSightHeight, t)
```

5. Якщо `terrainHeight(samplePoint) > expectedSightHeight + OcclusionSlopeBias`, цей промінь блокується.
6. Видимість тайла дорівнює частці променів, які пройшли. Якщо пройшла тільки частина променів, результат множиться на `PartialVisibilityDetectionMultiplier`.

Фінальна boolean-видимість визначається через `TerrainVisibilityThreshold`. Наприклад, при `0.5` тайл видно, якщо пройшла принаймні приблизно половина важливих sample rays.

Це дає такі ефекти:

| Ситуація | Результат |
|---|---|
| Юніт стоїть високо, дивиться вниз | Добра дальність, часто видно далі базового радіуса |
| Юніт стоїть низько, дивиться на височину | Дальність падає, інколи ціль повністю невидима |
| Юніт і ціль на одному рівні | Тайл видно в межах нормальної дальності |
| Між юнітом і ціллю є вищий хребет | Огляд блокується |
| Перешкода закриває лише частину тайла | Повертається fractional visibility, а не випадковий результат |

---

## Edge-Aware Line of Sight

Окрім загального нахилу, алгоритм окремо враховує різкі краї рельєфу. Це потрібно для ситуацій, де юніт стоїть на плато або пагорбі: висота дає перевагу, але не повинна автоматично відкривати все під обривом.

Правила:

| Ситуація | Поведінка |
|---|---|
| Юніт стоїть глибоко на плато | Перші тайли за різким downhill edge ховаються в blind zone |
| Юніт підходить до краю | Blind zone стискається або зникає, і підніжжя видно краще |
| Ціль стоїть на верхньому краю | Знизу її легше побачити, бо uphill penalty частково зменшується |
| Ціль стоїть глибше за краєм | Знизу вона знову може бути схована плато |
| Юніт на краю має високий `SilhouettePenalty` | Його легше побачити знизу, навіть якщо спостерігач має слабкий crest vision |

Uphill crest перевіряється напрямково: система йде від цілі назад до спостерігача по тій самій grid-лінії і шукає реальний перепад висоти на цьому промені. Тому ціль не стає видимою просто тому, що поруч із нею є будь-який край; край має лежати у напрямку погляду.

Ключові параметри в `FogOfWarSettings`:

| Параметр | Що контролює |
|---|---|
| `EnableTerrainEdgeLineOfSight` | Вмикає або вимикає edge-aware логіку |
| `TerrainEdgeHeightThreshold` | Мінімальний перепад висоти, який вважається краєм |
| `TerrainEdgePeekDistanceTiles` | Скільки тайлів від краю ще вважаються позицією “на краю” |
| `TerrainEdgeBlindZoneTiles` | Базова кількість нижніх тайлів, прихованих одразу за краєм |
| `TerrainEdgeBlindZoneDistanceScale` | Наскільки blind zone росте, якщо спостерігач стоїть далі від краю |
| `TerrainEdgeMaxBlindZoneTiles` | Максимальна глибина blind zone |
| `TerrainEdgeUphillPeekStrength` | Наскільки сильно зменшується uphill penalty для цілі на верхньому краю |

Пер-юнітні модифікатори задаються в `UnitClassConfig` і потрапляють у fog через `UnitCreatedSignal`:

| Поле | Що контролює |
|---|---|
| `CanSeeCrest` | Чи вміє юніт бачити верхній край схилу знизу |
| `CrestVisibilityFactor` | Наскільки сильно цей юніт зменшує uphill penalty для crest targets |
| `DownSlopeVisionBonus` | Додаткова дальність, коли юніт стоїть на краю і дивиться вниз |
| `SilhouettePenalty` | Наскільки сам юніт стає помітним на верхньому краю |

---

## Межі карти

`FogVisibilityResolver` завжди перевіряє `IsInBounds(tile, mapWidth, mapHeight)`. Тайли поза межами не потрапляють у результат навіть якщо бонус висоти збільшує радіус пошуку.

---

## Детермінізм

Алгоритм не використовує randomness. Для однакових `heightMap`, позицій, `FogOfWarSettings` і `FogVisionModifiers` результат завжди однаковий. Cache у `HeightAwareVisionService` ключується позиціями, дальністю, max range і сигнатурами модифікаторів, а також очищається при зміні налаштувань або height map.

## Джерело HeightMap

Карта висот надходить із `WorldGeneratedDataSignal` після завершення генерації світу. Якщо є `TerrainLevelMap`, Fog of War використовує його як стабільні рівні висоти; якщо його немає, бере fallback `HeightMap`.

`FogOfWarService`:

1. передає її в `IFogVisibilityResolver.SetHeightMap(...)`
2. очищає лічильники видимості
3. заново рахує видимі тайли для всіх зареєстрованих юнітів
4. перебудовує fog texture

---

## Посилання

- [Red Blob Games — Field of View](https://www.redblobgames.com/grids/hexagons/#field-of-view)
