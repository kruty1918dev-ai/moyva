# Fog of War — Алгоритм видимості

← [README](README.md)

---

## Height-Aware Visibility

Поточний алгоритм видимості побудований навколо двох рівнів логіки:

1. `FogVisibilityResolver` перебирає тайли в квадраті пошуку навколо юніта.
2. `HeightAwareVisionService` вирішує, чи справді тайл видно, з огляду на висоту, штрафи, бонуси й line of sight.

---

## Базові правила

| Правило | Поведінка |
|---|---|
| Мінімальний радіус | `visionRange` затискається до `1` |
| Один рівень висоти | Видимість без штрафу |
| Спостерігач вище | Може бачити далі |
| Ціль вище | Дальність до цілі зменшується |
| Рельєф між точками | Може повністю закрити видимість |

---

## Обчислення радіуса пошуку

Спершу система рахує не фактичну видимість, а максимальний радіус, у якому варто перевіряти тайли.

Формула на рівні ідеї така:

```text
searchRadius = clamp(baseVisionRange + observerHeightBonus, 1, maxVisionRange)
```

`observerHeightBonus` залежить від висоти тайлу, на якому стоїть юніт, і від параметрів `FogOfWarSettings`.

---

## Перевірка конкретного тайлу

Для кожного тайлу-кандидата система рахує ефективну дальність до нього:

```text
effectiveRange = clamp(
    baseVisionRange
    + observerHeightBonus
    + downhillVisionBonus
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

## Line of Sight по HeightMap

Після перевірки дальності виконується line of sight уздовж дискретної лінії між `origin` і `target`.

Алгоритм:

1. Береться висота спостерігача `originHeight`.
2. Береться висота цілі `targetHeight`.
3. Обчислюється нахил до цілі:

```text
targetSlope = (targetHeight - originHeight) / distance
```

4. Для кожної проміжної точки обчислюється її локальний нахил:

```text
sampleSlope = (sampleHeight - originHeight) / stepIndex
```

5. Якщо `sampleSlope > targetSlope + OcclusionSlopeBias`, проміжний рельєф перекриває огляд.

Це дає такі ефекти:

| Ситуація | Результат |
|---|---|
| Юніт стоїть високо, дивиться вниз | Добра дальність, часто видно далі базового радіуса |
| Юніт стоїть низько, дивиться на височину | Дальність падає, інколи ціль повністю невидима |
| Юніт і ціль на одному рівні | Тайл видно в межах нормальної дальності |
| Між юнітом і ціллю є вищий хребет | Огляд блокується |

---

## Межі карти

`FogVisibilityResolver` завжди перевіряє `IsInBounds(tile, mapWidth, mapHeight)`. Тайли поза межами не потрапляють у результат навіть якщо бонус висоти збільшує радіус пошуку.

---

## Джерело HeightMap

Карта висот надходить із `WorldGeneratedDataSignal.HeightMap` після завершення генерації світу.

`FogOfWarService`:

1. передає її в `IFogVisibilityResolver.SetHeightMap(...)`
2. очищає лічильники видимості
3. заново рахує видимі тайли для всіх зареєстрованих юнітів
4. перебудовує fog texture

---

## Посилання

- [Red Blob Games — Field of View](https://www.redblobgames.com/grids/hexagons/#field-of-view)
