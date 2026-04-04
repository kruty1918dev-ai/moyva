# Conditional Switch

← [Утиліти](../nodes-utility.md) · [Graph System](../README.md)

**Категорія:** Utility · **Файл:** `ConditionalSwitchNode.cs`

**Мультиплексор** — обирає між двома картами висот попіксельно за булевою маскою.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟢 | A | `float[,]` | Карта для true-зон |
| **Input** 🟢 | B | `float[,]` | Карта для false-зон |
| **Input** 🟡 | Condition | `bool[,]` | Маска вибору |
| **Output** 🟢 | Result | `float[,]` | Результат |

## Формула

```
result[x,y] = condition[x,y] ? A[x,y] : B[x,y]
```

## Приклад: Два рельєфи за висотою

```
HeightSource₁ → A ───┐
                      ├──► ConditionalSwitch ──► далі
HeightSource₂ → B ───┤
                      │
Mask(>0.5)  → Cond ───┘

Результат: де висота > 0.5 — використовує карту A, інакше карту B
```
