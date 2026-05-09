# API та архітектура — Як створювати власні ноди

← [Назад до Graph System](README.md)

---

## Архітектура системи

Graph System складається з трьох assembly:

```
┌─────────────────────────────────────────────┐
│              GraphSystem.Editor             │  ← EditorWindow, GraphView, NodeView
│  (UnityEditor.Experimental.GraphView)       │     Тільки в редакторі
├─────────────────────────────────────────────┤
│              GraphSystem.API                │  ← NodeBase, GraphAsset, PortDefinition
│  (чистий C# + UnityEngine)                 │     Класи які ви наслідуєте
├─────────────────────────────────────────────┤
│              GraphSystem.Runtime            │  ← GraphRunner, GraphValidator
│  (виконання графів)                        │     Запуск і валідація
└─────────────────────────────────────────────┘
```

### Потік даних

```
GraphAsset (SO) ──→ GraphValidator ──→ GraphRunner ──→ Результат
     │               перевірка           виконання
     │               циклів,             по порядку
     ├─ NodeBase[]    типів,             (Kahn's sort)
     └─ Connection[]  портів
```

---

## Базові класи та інтерфейси

### NodeBase — основа кожного ноду

```csharp
public abstract class NodeBase : ScriptableObject
{
    // Унікальний ID (автогенерація)
    public string NodeId { get; set; }
    
    // Позиція в редакторі (не чіпайте вручну)
    public Vector2 EditorPosition { get; set; }

    // --- ВИ РЕАЛІЗУЄТЕ ЦІ МЕТОДИ ---

    // Заголовок у редакторі
    public virtual string Title => GetType().Name;

    // Визначення портів
    public abstract PortDefinition[] Inputs { get; }
    public abstract PortDefinition[] Outputs { get; }

    // Логіка виконання (inputs = значення з вхідних портів)
    public abstract NodeOutput Execute(object[] inputs, NodeContext context);
}
```

### PortDefinition — порти вводу/виводу

```csharp
// Фабричні методи:
PortDefinition.Input<float[,]>("HeightMap")     // → зелений вхідний порт
PortDefinition.Output<string[,]>("TileMap")     // → блакитний вихідний порт
PortDefinition.Input<bool[,]>("Mask")           // → жовтий вхідний порт
PortDefinition.Input<int>("MapWidth")           // → сірий вхідний порт
```

### NodeOutput — результат виконання

```csharp
// Успіх з результатами (по одному на кожен Output порт, в порядку оголошення):
return NodeOutput.Success(resultHeightMap, resultTileMap);

// Попередження (результат є, але з нотаткою):
return NodeOutput.Warning("Деякі клітинки за межами", resultMap);

// Помилка (виконання графу зупиняється):
return NodeOutput.Error("Немає валідних кандидатів для POI");
```

### NodeContext — контекст виконання

```csharp
// Доступ до загальних параметрів:
int width = context.MapWidth;    // Ширина карти
int height = context.MapHeight;  // Висота карти
int seed = context.Seed;         // Глобальний seed

// Генератор випадкових чисел (детерміністичний):
System.Random rng = context.Random;

// Доступ до зовнішніх сервісів (через Zenject):
var noiseProvider = context.GetService<INoiseProvider>();
var biomeResolver = context.GetService<IBiomeResolver>();

// Cancellation (для async нодів):
context.Cancellation.ThrowIfCancellationRequested();
```

### Доступ до статичних даних генератора

Дані, які граф записав через ноду `Static Generator Data`, можна отримати з `IGeneratorDataRegistry` за ключем. Наприклад, якщо `Hill Generator` підключений до `Static Generator Data` з ключем `hill-levels`, runtime-система може прочитати рівень, висоту і тайл конкретної клітинки так:

```csharp
if (_generatorDataRegistry.TryGetHillLevelData("hill-levels", out var hillData) &&
    hillData.TryGetTile(x, y, out var tileData))
{
    int level = tileData.Level;
    float height = tileData.Height;
    string tileId = tileData.TileId;
}
```

`HillLevelDataMap` містить записи для всіх клітинок, що пройшли через `Hill Generator`, включно з тими, які нода не змінювала.

Кожен `HillLevelTileData` також містить `DirectionId` — напрямок hill-тайла, який поставив `Hill Generator` (`North`, `CornerSE`, `InnerCornerNW` тощо). Це поле потрібне для нодів на кшталт `Hill Level Tile Override`, які хочуть замінювати саме hill-тайли генератора, а не просто робити blind replace по `TileId`.

### Mask-scoped Hill override

`Hill Generator` підтримує override-профіль (окремі `levels/thresholds` і окремі `HillTileEntry[]`) для зони маски.

Пріоритет джерела маски:
1. `Mask (optional)` input (`bool[,]`) — якщо підключений і валідного розміру.
2. `LayerIndexMap (optional)` input (`int[,]`) + список target layer indices у налаштуваннях ноди.
3. Якщо жодне джерело не доступне — нода працює у базовому режимі без override.

Це дозволяє робити сценарій: `Height To Tile.LayerIndexMap -> Hill Generator.LayerIndexMap`, а далі переключати маскову зону через список індексів без додаткових mask-нодів.

### NodeInfoAttribute — метадані ноду

```csharp
[NodeInfo("My Node", "Category Name", "Короткий опис")]
public class MyNode : NodeBase { ... }
```
- **Назва** — відображається у вузлі та пошуку
- **Категорія** — група в меню пошуку (Generators, Processing, Features тощо)
- **Опис** — тултіп в меню пошуку

---

## Як створити свій нод

### Крок 1: Створіть файл

Створіть новий `.cs` файл у `Assets/Moyva/Scripts/Features/Generator/Nodes/`.

### Крок 2: Напишіть клас

```csharp
using UnityEngine;
using Kruty1918.Moyva.GraphSystem.API;

[NodeInfo("Invert Height", "Processing", "Інвертує карту висот (1.0 - height)")]
public class InvertHeightNode : NodeBase
{
    // Заголовок у вузлі
    public override string Title => "Invert Height";

    // ОДИН вхід: HeightMap
    public override PortDefinition[] Inputs => new[]
    {
        PortDefinition.Input<float[,]>("HeightMap")
    };

    // ОДИН вихід: InvertedMap
    public override PortDefinition[] Outputs => new[]
    {
        PortDefinition.Output<float[,]>("InvertedMap")
    };

    // Логіка
    public override NodeOutput Execute(object[] inputs, NodeContext context)
    {
        // inputs[0] = перший Input порт (HeightMap)
        var heightMap = inputs[0] as float[,];
        if (heightMap == null)
            return NodeOutput.Error("HeightMap не підключено");

        int w = heightMap.GetLength(0);
        int h = heightMap.GetLength(1);
        var result = new float[w, h];

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
            result[x, y] = 1f - heightMap[x, y];

        // Success з одним значенням → відповідає Outputs[0]
        return NodeOutput.Success(result);
    }
}
```

### Крок 3: Готово!

Зберіть проект (Ctrl+S в Unity). Ваш нод автоматично з'явиться в меню пошуку під категорією "Processing".

---

## Додаткові можливості

### Параметри в Inspector

Додайте `[SerializeField]` поля для налаштувань:

```csharp
public class MyNode : NodeBase
{
    [SerializeField, Range(0f, 1f)] private float _threshold = 0.5f;
    [SerializeField] private string _tileName = "grass";
    [SerializeField] private DataNoiseSettings _noiseSettings;  // SO inline editor

    public override NodeOutput Execute(object[] inputs, NodeContext context)
    {
        // Використовуйте _threshold, _tileName, _noiseSettings
    }
}
```

### Async ноди (для довгих операцій)

```csharp
using System.Threading.Tasks;

[NodeInfo("Heavy Node", "Processing", "Довге обчислення")]
public class HeavyNode : NodeBase, IAsyncNode
{
    public override NodeOutput Execute(object[] inputs, NodeContext context)
        => ExecuteAsync(inputs, context).GetAwaiter().GetResult();

    public async Task<NodeOutput> ExecuteAsync(object[] inputs, NodeContext context)
    {
        var map = inputs[0] as float[,];
        // ...довге обчислення...
        await Task.Yield(); // дозволити UI оновитись
        context.Cancellation.ThrowIfCancellationRequested();
        return NodeOutput.Success(result);
    }
}
```

### Preview ноди (показують зображення в редакторі)

```csharp
public class MyNode : NodeBase, IPreviewableNode
{
    public Texture2D GeneratePreview(object[] outputs, int maxSize)
    {
        var map = outputs[0] as float[,];
        // Створіть Texture2D з даних карти
        // maxSize обмежує розмір для швидкості
        return texture;
    }
}
```

### Кастомний Editor для ноду

```csharp
public class MyNode : NodeBase, ICustomEditorNode
{
    // GeneratorNodeView помітить цей інтерфейс
    // і використає спеціалізований редактор
}
```

---

## Ключові API класи

### GraphAsset

```csharp
public class GraphAsset : ScriptableObject
{
    public List<NodeBase> Nodes { get; }
    public List<Connection> Connections { get; }
    
    public NodeBase AddNode(System.Type nodeType);  // Створює нод як sub-asset
    public void RemoveNode(NodeBase node);
    public void AddConnection(Connection connection);
    public void RemoveConnection(Connection connection);
}
```

### Connection

```csharp
public class Connection
{
    public string FromNodeId;   // ID вихідного ноду
    public int FromPortIndex;   // Індекс output-порту
    public string ToNodeId;     // ID вхідного ноду
    public int ToPortIndex;     // Індекс input-порту
    public int SourceElementIndex; // Індекс елемента, якщо output-список підключений до scalar input
}
```

Якщо output-порт повертає список або одновимірний масив, а input-порт очікує один елемент цього типу, з'єднання лишається звичайним edge. У редакторі біля input-порту з'явиться поле `idx`: воно визначає, який елемент списку буде переданий у `inputs[]`. За замовчуванням використовується індекс `0`; після виконання графа поруч показується кількість доступних варіантів, якщо її можна визначити.

### GraphRunner

```csharp
public sealed class GraphRunner
{
    // Виконує граф синхронно
    public GraphExecutionResult Execute(GraphAsset graph, NodeContext context);
    
    // Виконує граф асинхронно (для UI без блокування)
    public async Task<GraphExecutionResult> ExecuteAsync(
        GraphAsset graph, NodeContext context);
}

public class GraphExecutionResult
{
    public string ErrorNodeId;            // null якщо все ОК
    public string ErrorMessage;           // null якщо все ОК
    public List<NodeExecutionLog> Logs;   // Лог кожного ноду з таймінгом
}
```

### GraphValidator

```csharp
public static class GraphValidator
{
    // Повертає список помилок (порожній = валідний)
    public static List<ValidationError> Validate(GraphAsset graph);
}
```

---

## Правила порядку inputs/outputs

**Порядок `inputs[]` відповідає порядку `Inputs` масиву:**

```csharp
public override PortDefinition[] Inputs => new[]
{
    PortDefinition.Input<float[,]>("HeightMap"),   // → inputs[0]
    PortDefinition.Input<string[,]>("TileMap"),    // → inputs[1]
    PortDefinition.Input<bool[,]>("Mask"),         // → inputs[2]
};

public override NodeOutput Execute(object[] inputs, NodeContext context)
{
    var heightMap = inputs[0] as float[,];  // HeightMap
    var tileMap   = inputs[1] as string[,]; // TileMap
    var mask      = inputs[2] as bool[,];   // Mask (може бути null якщо не підключено!)
}
```

**Порядок `NodeOutput.Success(...)` відповідає порядку `Outputs` масиву:**

```csharp
public override PortDefinition[] Outputs => new[]
{
    PortDefinition.Output<string[,]>("TileMap"),    // → Success(arg0, ...)
    PortDefinition.Output<bool[,]>("WaterMask"),    // → Success(..., arg1)
};

return NodeOutput.Success(resultTileMap, resultWaterMask);
//                        ↑ Outputs[0]   ↑ Outputs[1]
```

---

## Інтеграція з грою (Zenject)

`GraphBasedMapDataGenerator` адаптер реєструє сервіси:

```csharp
public class GraphBasedMapDataGenerator : IMapDataGenerator
{
    [Inject] private readonly INoiseProvider _noiseProvider;
    [Inject] private readonly IBiomeResolver _biomeResolver;
    [Inject] private readonly IRiverPathfinder _riverPathfinder;
    // ...

    public MapGenerationResult Generate(int width, int height, int seed)
    {
        var context = new NodeContext(seed, width, height, CancellationToken.None);
        context.RegisterService(_noiseProvider);
        context.RegisterService(_biomeResolver);
        // ...

        var runner = new GraphRunner();
        var result = runner.Execute(_graphAsset, context);

        // Читаємо OutputNode
        var output = _graphAsset.Nodes.OfType<OutputNode>().First();
        return new MapGenerationResult(
            output.BiomeMap,
            output.ObjectMap,
            output.HeightMap
        );
    }
}
```

Для доступу до сервісів у вашому ноді:

```csharp
public override NodeOutput Execute(object[] inputs, NodeContext context)
{
    var noise = context.GetService<INoiseProvider>();
    var map = noise.GenerateNoise(context.MapWidth, context.MapHeight, _settings);
    return NodeOutput.Success(map);
}
```
