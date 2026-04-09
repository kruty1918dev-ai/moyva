# GameModeChangeRequestedSignal

← [Назад до Signals](README.md)

---

## Опис

Сигнал-запит на зміну ігрового режиму. Надсилається з UI-контролерів і маршрутизується через роутер до сервісу зміни режимів.

---

## Оголошення

`Signals/API/OnGameModeSignals.cs`

```csharp
public struct GameModeChangeRequestedSignal
{
    public GameModeType RequestedMode;
}
```

---

## Поля

| Поле | Тип | Опис |
|---|---|---|
| `RequestedMode` | `GameModeType` | Запитаний режим |

---

## Хто надсилає

- `ConstructionUIController` — при натисканні кнопки будівництва
- `GameModeUIController` — при переключенні режиму через UI

## Хто отримує

- `GameModeChangeRequestRouter` — делегує до `IGameModeService.SetMode()`

---

## Реєстрація

```csharp
Container.DeclareSignal<GameModeChangeRequestedSignal>();
```

---

## Категорія

GameMode

---

## Пов'язані сигнали

- [GameModeChangedSignal](game-mode-changed.md)
