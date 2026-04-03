# Fog of War — Stub системи збереження

← [README](README.md)

---

> **Статус (2026):** SaveSystem реалізований і доступний у `Features/SaveSystem/`.
> `FogSaveDataStub` залишається заглушкою, що чекає на інтеграцію з `ISaveModule`.
> Деталі інтеграції: [docs/systems/save-system.md](../save-system.md).

---

## Чому stub?

Система збереження Moyva ще не реалізована. `FogSaveDataStub` є placeholder, що:

- При `LoadExploredData()` повертає `null` → нова гра починається з чистою картою
- При `SaveExploredData()` лише логує у Console і нічого не зберігає

Це не впливає на ігровий процес — `FogOfWarService` коректно обробляє `null` від `LoadExploredData()`.

---

## Що буде зберігатися (у майбутньому)

`bool[,] _exploredTiles` — масив розміром `mapWidth × mapHeight`. Кожен елемент `true` означає, що цей тайл колись бачили.

**Що НЕ зберігається:**
- `int[,] _visibilityCounters` — при завантаженні гри юніти не розставлені, тому лічильники завжди стартують з 0
- Позиції юнітів — це відповідальність системи юнітів

---

## Розмір даних

Для карти 100×100 = 10 000 булів = ~1.25 KB (якщо упакований як bits) або ~10 KB (якщо byte per bool). Прийнятно.

---

## Як підключити реальну реалізацію

1. Створіть клас, що реалізує `IFogSaveDataProvider`:

```csharp
using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    public class JsonFogSaveDataProvider : IFogSaveDataProvider
    {
        private const string SaveKey = "fog_explored";

        public bool[,] LoadExploredData()
        {
            // Завантажити з PlayerPrefs / файлу / Cloud
            // Десеріалізувати з JSON/binary
            return null; // якщо нова гра
        }

        public void SaveExploredData(bool[,] explored)
        {
            // Серіалізувати і зберегти
        }
    }
}
```

2. У `FogOfWarInstaller` замініть:

```csharp
// Було:
Container.Bind<IFogSaveDataProvider>().To<FogSaveDataStub>().AsSingle();

// Стало:
Container.Bind<IFogSaveDataProvider>().To<JsonFogSaveDataProvider>().AsSingle();
```

3. Після завантаження гри виклик `LoadExploredData()` відбувається автоматично в `FogOfWarService.Initialize(w,h)`.
4. Для збереження при виході з гри — підписайтесь на відповідний Application lifecycle event і викличте `_saveProvider.SaveExploredData(_fogService.GetExploredSnapshot())`.
