# GameModeChangedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал, що надсилається при зміні ігрового режиму. Містить новий режим гри (Normal або Construction).

---

## Оголошення

`Signals/API/OnGameModeSignals.cs`

```csharp
public struct GameModeChangedSignal
{
    public GameModeType NewMode;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `NewMode` | `GameModeType` | Новий режим (`Normal` або `Construction`) |

---

## Хто надсилає

- `GameModeService.SetMode()` — при переключенні режиму

## Хто отримує

- `TileInteractionService` — змінює логіку обробки кліків
- `ConstructionService` — активує/деактивує режим будівництва
- `GameModePanelController` — оновлює панель режимів
- `GameModeUIController` — оновлює UI індикатор режиму
- `ConstructionUIController` — показує/ховає панель будівництва

---

## Реєстрація

```csharp
Container.DeclareSignal<GameModeChangedSignal>();
```

---

## Категорія

GameMode

---

## Пов'язані сигнали

- [GameModeChangeRequestedSignal](game-mode-change-requested.md)
