# Bootstrap — Система ініціалізації сцени

← [Назад до README](../README.md)

---

## Призначення

Система **Bootstrap** є точкою входу гри. Вона ініціалізує Zenject DI-контейнер сцени та запускає тестовий спавн юнітів одразу після завантаження.

---

## Компоненти

| Клас | Роль |
|---|---|
| `BootstrapInstaller` | `MonoInstaller` — реєструє початкові сервіси в DI-контейнері |
| `TestUnitSpawner` | `IInitializable` — тестовий клас для перевірки спавну юнітів |

---

## Як працює внутрішньо

1. **`BootstrapInstaller`** реєструє `TestUnitSpawner` через `BindInterfacesTo<TestUnitSpawner>().AsSingle().NonLazy()`.
2. Zenject автоматично викликає `Initialize()` одразу після побудови контейнера.
3. **`TestUnitSpawner.Initialize()`** викликає `IUnitFactory.CreateUnit(...)` для спавну тестових юнітів у задані позиції.

> **Примітка:** `TestUnitSpawner` — це тимчасовий клас для розробки/тестування. У продакшні його замінює реальна система завантаження рівнів або ігровий менеджер.

---

## Публічний API

`TestUnitSpawner` не має публічного API — він є `internal sealed` і спілкується лише через `IUnitFactory`.

---

## Реєстрація в Zenject (`BootstrapInstaller`)

```csharp
public class BootstrapInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // NonLazy() — гарантує, що Initialize() буде викликано навіть без явного Resolve<>
        Container.BindInterfacesTo<TestUnitSpawner>()
            .AsSingle()
            .NonLazy();
    }
}
```

---

## Приклади використання

### Тестовий спавн (реальний код з проекту)

```csharp
// TestUnitSpawner.cs
public void Initialize()
{
    Debug.Log("[Bootstrap] Початок тестового спавну юнітів...");

    // Юніт 1: Центр карти
    _unitFactory.CreateUnit("warrior", new Vector2Int(5, 5));

    // Юніт 2: Трохи далі
    _unitFactory.CreateUnit("warrior", new Vector2Int(7, 3));

    // Юніт 3: В кутку
    _unitFactory.CreateUnit("warrior", new Vector2Int(2, 8));

    Debug.Log("[Bootstrap] Тестові юніти створені.");
}
```

### Як розширити Bootstrap для продакшну

```csharp
// Замість TestUnitSpawner — реальний GameInitializer
public class GameInitializer : IInitializable
{
    private readonly IMapInstantiator _mapInstantiator;
    private readonly IUnitFactory    _unitFactory;

    public void Initialize()
    {
        _mapInstantiator.BuildWorld();
        // завантажити збережений стан гравця
        // спавнити юнітів із save-файлу
    }
}
```

---

## Залежності

| Залежність | Причина |
|---|---|
| [`IUnitFactory`](units.md) | Створення тестових юнітів при старті |

---

## Пов'язані системи

- [Units](units.md) — `IUnitFactory` для спавну
- [Generator](generator.md) — може бути ініційований з Bootstrap
