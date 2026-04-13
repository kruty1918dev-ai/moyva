# Складність бота (`DifficultyLevel`)

## Огляд

Рівень складності AI-бота налаштовується через `IBotDifficultySettings` та `BotDifficultySettings`.

## Параметри налаштувань

| Параметр | Тип | Опис |
|---|---|---|
| `Difficulty` | `DifficultyLevel` | Рівень складності (Easy / Normal / Hard) |
| `TickInterval` | `float` | Інтервал між тіками AI в секундах. Менший = реактивніший бот |
| `AttackThreshold` | `int` | Мінімальна кількість юнітів для переходу в режим атаки |
| `DefendThreshold` | `int` | Кількість юнітів, нижче якої бот переходить у режим захисту |

## Пресети

```csharp
IBotDifficultySettings easy   = BotDifficultySettings.Easy();
IBotDifficultySettings normal = BotDifficultySettings.Normal();
IBotDifficultySettings hard   = BotDifficultySettings.Hard();
```

| Пресет | TickInterval | AttackThreshold | DefendThreshold |
|---|---|---|---|
| Easy   | 4.0 с | 5 | 2 |
| Normal | 2.0 с | 3 | 1 |
| Hard   | 1.0 с | 2 | 1 |

## Як змінити складність через Zenject

У `BotInstaller.cs` замініть прив'язку на потрібний пресет:

```csharp
// За замовчуванням — Normal
Container.Bind<IBotDifficultySettings>()
    .FromInstance(BotDifficultySettings.Normal())
    .AsSingle();

// Hard:
Container.Bind<IBotDifficultySettings>()
    .FromInstance(BotDifficultySettings.Hard())
    .AsSingle();
```

## Кастомні налаштування

Реалізуйте власний `IBotDifficultySettings`:

```csharp
public sealed class CustomBotSettings : IBotDifficultySettings
{
    public DifficultyLevel Difficulty    => DifficultyLevel.Hard;
    public float           TickInterval  => 0.5f;
    public int             AttackThreshold => 1;
    public int             DefendThreshold => 0;
}
```

Зареєструйте у Zenject:

```csharp
Container.Bind<IBotDifficultySettings>()
    .To<CustomBotSettings>()
    .AsSingle();
```
