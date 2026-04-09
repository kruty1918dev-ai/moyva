# WorldBuiltSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається після завершення побудови світу. Порожня структура, яка слугує маркером готовності карти для систем пост-генерації.

---

## Оголошення

`Signals/API/OnTileChanged.cs`

```csharp
public struct WorldBuiltSignal { }
```

---

## Хто надсилає

- `MapVisualInstantiator` — після завершення побудови світу

## Хто отримує

- Системи пост-генерації (FogOfWar, Bootstrap тощо) — ініціалізуються після готовності карти

---

## Реєстрація

```csharp
Container.DeclareSignal<WorldBuiltSignal>();
```

---

## Категорія

Ядро

---

## Пов'язані сигнали

- [WorldGeneratedDataSignal](world-generated-data.md)
- [OnMapObjectSpawnedSignal](map-object-spawned.md)
