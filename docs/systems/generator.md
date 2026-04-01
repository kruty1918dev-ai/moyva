# Generator — Система процедурної генерації карти

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/docs/#systems/generator)

---

## Призначення

Система **Generator** відповідає за процедурну генерацію тайлового ігрового світу. Вона реалізує конвеєр (pipeline) із декількох незалежних стадій: шум → висоти → біоми → особливості (річки) → WFC-полірування → візуальна побудова.

---

## Конвеєр генерації

```
INoiseProvider
  └─► GenerateNoiseMap()          → float[,] heightMap

IVirtualHeightMapGenerator
  └─► GenerateVirtualHeightMap()  → string[,] (за висотами з HeightMapSettings)

IBiomeResolver
  └─► ResolveBiomes()             → string[,] biomeMap (з урахуванням вологості)

IMapFeatureGenerator[] (кожен по черзі)
  ├─► RiverFeatureGenerator       → прокладає річки по heightMap
  └─► WaterPostProcessor          → постобробка водних тайлів

IWFCService
  └─► Apply()                     → WFC-полірування переходів між тайлами

IMapInstantiator (MapVisualInstantiator)
  └─► BuildWorld()                → спавн GameObject'ів по biomeMap
```

---

## Підсистеми

| Клас / Інтерфейс | Роль |
|---|---|
| `IMapDataGenerator` / `MapDataGenerator` | Оркестратор усього конвеєру |
| `INoiseProvider` / `NoiseMapGeneratorService` | Генерація Perlin noise карти |
| `IVirtualHeightMapGenerator` / `VirtualHeightMapGenerator` | Конвертація висот у ID тайлів |
| `IBiomeResolver` / `BiomeResolver` | Призначення біомів за висотою + вологістю |
| `IMapFeatureGenerator` / `RiverFeatureGenerator` | Прокладання річок по карті |
| `IMapFeatureGenerator` / `WaterPostProcessor` | Постобробка водних зон |
| `IWFCService` / `WFCService` | Wave Function Collapse — полірування переходів |
| `IMapInstantiator` / `MapVisualInstantiator` | Побудова GameObject'ів в Unity |
| `IRiverPathfinder` / `RiverPathfinder` | A\* патфайндинг по градієнту висот для річок |

---

## Як працює внутрішньо

### Шум (`NoiseMapGeneratorService`)

Генерує `float[,]` через `Mathf.PerlinNoise` із підтримкою октав, persistance, lacunarity, seed та offset (з `DataNoiseSettings`).

### Висоти → Тайли (`VirtualHeightMapGenerator`)

Перебирає `HeightLayer[]` із `HeightMapSettings`. Для кожної клітинки шукає шар, в діапазон висот якого потрапляє значення.

### Біоми (`BiomeResolver`)

Генерує додаткову карту вологості (Perlin з рандомним offset). Для кожної клітинки перебирає `BiomeData[]` із `DataBiomesSettings` і знаходить біом за (height, moisture) — якщо підходить, замінює ID тайлу.

### Річки (`RiverFeatureGenerator` + `RiverPathfinder`)

- Вибирає випадкові стартові точки у гірській зоні та фінішні — у низовинах.
- `RiverPathfinder` шукає шлях через `IRiverPathfinder.FindRiverPath` (A\* по градієнту висот, зі зваженими типами тайлів).
- Накладає ID тайлів річки (vertical/horizontal/corner) по шляху.

### WFC (`WFCService`)

Застосовує правила `WFCTileRule[]` з `WFCDataSettings`: перевіряє сусідів кожного тайлу і замінює їх, якщо умова збіглася вище `MatchThreshold`, за кількість `PassCount` ітерацій.

### Побудова світу (`MapVisualInstantiator`)

- `BuildWorld()` викликає `IMapDataGenerator.GenerateMapData`, а по callback'у — ітерує по карті і спавнить `GameObject` із `TileRegistrySO` для кожного тайлу.

---

## Публічний API

### `IMapDataGenerator`

```csharp
public interface IMapDataGenerator
{
    // Запускає весь конвеєр і повертає результат через callback
    void GenerateMapData(
        int    width,
        int    height,
        Action<string[,], string[,], float[,]> onComplete
        // onComplete: biomeMap, objectMap, heightMap
    );
}
```

### `INoiseProvider`

```csharp
public interface INoiseProvider
{
    // Генерує float[,] значень [0..1] на основі Perlin noise
    float[,] GenerateNoiseMap(DataNoiseSettings settings, int width, int height);
}
```

### `IBiomeResolver`

```csharp
public interface IBiomeResolver
{
    // Перетворює heightMap + поточну карту у фінальну карту біомів
    void ResolveBiomes(float[,] heightMap, string[,] currentMap, Action<string[,]> onComplete);
}
```

### `IWFCService`

```csharp
public interface IWFCService
{
    // Полірує biomeMap за правилами WFCDataSettings
    void Apply(string[,] biomeMap, float[,] heightMap);
}
```

### `IMapInstantiator`

```csharp
public interface IMapInstantiator
{
    // Запускає генерацію і будує GameObject-и в сцені
    void BuildWorld();
}
```

### `IMapFeatureGenerator`

```csharp
public interface IMapFeatureGenerator
{
    // Модифікує карту — додає особливості (річки, постобробка)
    void ApplyFeatures(string[,] biomeMap, string[,] objectMap, float[,] heightMap, int width, int height);
}
```

### `IRiverPathfinder`

```csharp
public interface IRiverPathfinder
{
    // Знаходить шлях річки від startPoint до endPoint
    List<Vector2Int> FindRiverPath(
        Vector2Int    startPoint,
        Vector2Int    endPoint,
        string[,]     biomeMap,
        float[,]      heightMap,
        int           width,
        int           height,
        RiverDataConfig riverConfig);
}
```

---

## ScriptableObject конфіги

### `DataNoiseSettings`

| Поле | Тип | Опис |
|---|---|---|
| `Scale` | `float` | Масштаб шуму |
| `Octaves` | `int` | Кількість октав |
| `Persistance` | `float` | Відносна амплітуда кожної октави |
| `Lacunarity` | `float` | Збільшення частоти між октавами |
| `Seed` | `int` | Зерно для відтворюваності |
| `Offset` | `Vector2` | Зміщення карти |

### `HeightMapSettings`

Масив `HeightLayer[]`, кожен із `TileID`, `MinHeight`, `MaxHeight`.

### `DataBiomesSettings`

Масив `BiomeData[]` (TileID, MinHeight, MaxHeight, MinMoisture, MaxMoisture), плюс `DefaultTileID` і `MoistureScale`.

### `GenerationRules`

```csharp
public class GenerationRules : ScriptableObject
{
    public bool GenerateRivers = true;
    public bool GenerateBiomes = true;
    public bool ApplyWFC       = true;
}
```

### `WFCDataSettings`

Масив `WFCTileRule[]` з обмеженнями на сусідів, пріоритетами і порогом збігу.

---

## Залежності

| Залежність | Причина |
|---|---|
| [`IGridService`](grid.md) | `MapVisualInstantiator` встановлює `TileData` в сітці |
| `TileRegistrySO` | Пошук `VisualPrefab` за `TileTypeId` |
| [`SignalBus`](signals.md) | `OnMapObjectSpawnedSignal` |
| `DataNoiseSettings`, `HeightMapSettings`, `DataBiomesSettings`, `WFCDataSettings`, `GenerationRules`, `RiverDataConfig` | Конфіги генерації |

---

## Реєстрація в Zenject (`GeneratorInstaller`)

```csharp
public class GeneratorInstaller : MonoInstaller
{
    [SerializeField] private GenerationRules    _generationRules;
    [SerializeField] private HeightMapSettings  _heightMapSettings;
    [SerializeField] private RiverDataConfig    _riverConfig;
    [SerializeField] private DataNoiseSettings  _noiseSettings;
    [SerializeField] private DataBiomesSettings _biomesSettings;
    [SerializeField] private WFCDataSettings    _wfcDataSettings;

    public override void InstallBindings()
    {
        Container.BindInstance(_wfcDataSettings).AsSingle();
        Container.BindInstance(_riverConfig).AsSingle();
        Container.BindInstance(_noiseSettings).AsSingle();
        Container.BindInstance(_biomesSettings).AsSingle();
        Container.BindInstance(_generationRules).AsSingle();
        Container.BindInstance(_heightMapSettings).AsSingle();

        Container.Bind<IVirtualHeightMapGenerator>().To<VirtualHeightMapGenerator>().AsSingle();
        Container.Bind<IWFCService>().To<WFCService>().AsSingle();
        Container.Bind<IRiverPathfinder>().To<RiverPathfinder>().AsSingle();
        Container.Bind<INoiseProvider>().To<NoiseMapGeneratorService>().AsSingle();
        Container.Bind<IBiomeResolver>().To<BiomeResolver>().AsSingle();
        Container.Bind<IMapDataGenerator>().To<MapDataGenerator>().AsSingle();

        // AsTransient — кожен отримує окремий екземпляр (їх може бути декілька)
        Container.Bind<IMapFeatureGenerator>().To<RiverFeatureGenerator>().AsTransient();
        Container.Bind<IMapFeatureGenerator>().To<WaterPostProcessor>().AsTransient();

        Container.BindInterfacesTo<MapVisualInstantiator>().AsSingle();
    }

    public override void Start()
    {
        base.Start();
        Container.Resolve<IMapInstantiator>().BuildWorld();
    }
}
```

---

## Приклади використання

### Запустити генерацію вручну

```csharp
[Inject] private IMapDataGenerator _mapDataGenerator;

_mapDataGenerator.GenerateMapData(50, 50, (biomeMap, objectMap, heightMap) =>
{
    Debug.Log($"Генерація завершена. Карта: {biomeMap.GetLength(0)}x{biomeMap.GetLength(1)}");
    Debug.Log($"Тайл (25,25): {biomeMap[25, 25]}");
});
```

### Побудувати світ

```csharp
[Inject] private IMapInstantiator _mapInstantiator;

// Генерує і одразу спавнить тайли в сцені
_mapInstantiator.BuildWorld();
```

### Перевірка шуму окремо

```csharp
[Inject] private INoiseProvider _noiseProvider;
[Inject] private DataNoiseSettings _noiseSettings;

float[,] noise = _noiseProvider.GenerateNoiseMap(_noiseSettings, 100, 100);
Debug.Log($"Висота центру: {noise[50, 50]}");
```

---

## Пов'язані системи

- [Grid](grid.md) — `MapVisualInstantiator` заповнює `TileData`
- [Visuals](visuals.md) — спавнить `TileView` для кожного тайлу
- [Signals](signals.md) — `OnMapObjectSpawnedSignal`
