# План: Додавання ноду PerlinNoiseMaskNode та інших нодів графа генератора

## Мета
Створити новий нод `PerlinNoiseMaskNode` для графа генератора, який генерує перліновий шум і перетворює його у булеву маску за допомогою налаштовуваного порігу.

## Аналіз існуючої архітектури

### Існуючі ноди генерації
- `BaseNoiseSettings` - налаштування шуму (scale, octaves, persistance, lacunarity, offset)
- `SeedNode` - налаштування сіду
- `LayerMaskReferenceNode` - посилання на маску іншого шару

### Існуючі логічні ноди
- `BoolAndNode`, `BoolOrNode`, `BoolXorNode`, `BoolInvertNode`, `BoolSubtractNode` - операції над масками
- `AddNode` - математичні операції з підтримкою різних типів

### Ключові API
- `NodeBase` - базовий клас для всіх нодів
- `NodeInfoAttribute` - атрибут реєстрації ноду
- `IPreviewableNode` - інтерфейс для генерації превью
- `NodeContext.MapSize` - розмір карти для генерації
- `ProceduralNoiseUtility` - вже існуючі методи шуму

## Реалізація PerlinNoiseMaskNode (Оновлено)

### Файл: `Assets/Moyva/Scripts/Features/Generator/Runtime/Nodes/PerlinNoiseMaskNode.cs`

```csharp
using System;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Generator.Runtime.Noise;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Шум Пerlін Mask",
        "Генерація",
        "Створює булеву маску на основі перлінового шуму. Поріг визначає, які клітини будуть марковані як 'true' (дають участь у результаті). Корисний для створення випадкових ділянок, континентів, островів або інших природних структур.")]
    public sealed class PerlinNoiseMaskNode : NodeBase, IPreviewableNode
    {
        [SerializeField, Min(0.0001f)]
        [InlineEditable("масштаб")]
        [Tooltip("Масштаб шуму. Великі значення — плавні області, малі — дрібні деталі. Приклад: 50 — великі континенти, 5 — дрібні острови.")]
        private float _scale = 20f;

        [SerializeField, Range(1, 12)]
        [InlineEditable("октави")]
        [Tooltip("Кількість октав. Визначає, скільки шарів шуму буде накладено. 1 — гладко, 8 — багато деталей. Приклад: 4 — баланс між деталізацією та продуктивністю.")]
        private int _octaves = 4;

        [SerializeField, Range(0.01f, 1f)]
        [InlineEditable("амплітуда")]
        [Tooltip("Як швидко зменшується амплітуда шуму для кожної октави. 0.3 — плавно, 0.8 — багато дрібних деталей. Приклад: 0.5 — природний рельєф.")]
        private float _persistence = 0.5f;

        [SerializeField, Min(1f)]
        [InlineEditable("частота")]
        [Tooltip("Як швидко зростає частота шуму для кожної октави. 2 — типовий для природних карт, 3+ — дуже 'шумно'. Приклад: 2 — класика для перлинного шуму.")]
        private float _lacunarity = 2f;

        [SerializeField]
        [InlineEditable("зсув")]
        [Tooltip("Зсув карти шуму по X та Y. Дозволяє зміщувати карту без зміни інших параметрів. Приклад: (100, 200) — карта зміщена праворуч і вгору.")]
        private Vector2 _offset = Vector2.zero;

        [SerializeField, Range(0f, 1f)]
        [InlineEditable("поріг")]
        [Tooltip("Поріг для перетворення значення шуму у булеву маску. 0.5 — середнє значення. Нижче порігу буде тайл, выше — ні. Тести: 0.3 — більше та, 0.7 — менше та.")]
        private float _threshold = 0.5f;

        [NonSerialized] private bool[,] _lastMask;
        [NonSerialized] private float[,] _lastNoiseValues;

        public override string Title => "Шум Пerlін Mask";
        public override string Category => "Генерація";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Маска"),
            PortDefinition.Output<float[,]>("Значення шуму")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            int w = Mathf.Max(1, context?.MapSize.x ?? 0);
            int h = Mathf.Max(1, context?.MapSize.y ?? 0);

            _lastNoiseValues = new float[w, h];
            _lastMask = new bool[w, h];

            int seed = GlobalSeed.Combine(context?.Seed ?? GlobalSeed.DefaultSeed, NodeId.GetHashCode());

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x + _offset.x) / w / _scale;
                    float ny = (y + _offset.y) / h / _scale;

                    float noise = ProceduralNoiseUtility.SampleFbm(
                        nx, ny,
                        _octaves, _lacunarity, _persistence,
                        seed, false);

                    _lastNoiseValues[x, y] = noise;
                    _lastMask[x, y] = noise >= _threshold;
                }
            }

            return NodeOutput.Success(_lastMask, _lastNoiseValues);
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            // ... (preview implementation)
        }
    }
}
```

## Додаткові рекомендовані ноди генерації

### 1. Шум Пerlін Map (float[,])
**Категорія:** Генерація

Нод, який повертає не булеву маску, а саму карту значень шуму (float[,]). Корисний для:
- Висотних мап (HeightMap)
- Вагових масок (Weight maps)
- Щільності об'єктів (Object density)
- Параметрів перетворень (Displacement)

**Параметри:**
- Масштаб (scale)
- Октави (octaves)
- Амплітуда (persistence)
- Частота (lacunarity)
- Зсув (offset)

### 2. Шум Вороная Mask
**Категорія:** Генерація

Генерує маску на основі алгоритму Вороная для створення ділянкових структур:
- Вихід: bool[,] маска з ділянками
- Параметри: кількість клітин, відстань до центра, радіус, випадковість

**Використання:**
- Розбиття карти на власницькі ділянки
- Створення клітинної структури
- Розміщення об'єктів за ділянками

### 3. Градієнтний Mask
**Категорія:** Генерація

Генерує маску на основі градієнтів різних форм:
- Прямокутний градієнт (зліва/справа/зверху/знизу)
- Круговий градієнт (від центру)
- Radial градієнт (від кордонів до центру)
- Діагональний градієнт

**Параметри:**
- Тип градієнту (enum)
- Напрямок
- Інверсія
- Масштаб

### 4. Шахова фішка Mask
**Категорія:** Генерація

Генерує шахову фішку для створення регульярних патернів:
- Розмір клітин (у клітинах)
- Підкреслення чи не підкреслення
- Границі клітин (чи ні)
- Діагональний варіант

**Використання:**
- Створення мережевих структур
- Тестування алгоритмів
- Розміщення об'єктів у ритмічних патернах

### 5. Поле відстані
**Категорія:** Генерація

Генерує поле відстані до найближчої активної області:
- Вхід: bool[,] маска
- Вихід: float[,] поле відстані
- Параметри: максимальна відстань, нормалізація

**Використання:**
- Створення ефектів розмиття маски
- Підготовка масок для об'єктів з ефектом "attract/repel"
- Визначення зон впливу

## Додаткові рекомендовані ноди логіки та математики

### 6. Шум Combine
**Категорія:** Математика

Об'єднує кілька шумових масок за допомогою різних операцій:
- Average (середнє)
- Max (максимум)
- Min (мінімум)
- Weighted blend (зважене змішування)
- Fractal combine (комбінація з різними масштабами)

**Вхідні порти:**
- Шум A (float[,])
- Шум B (float[,])
- Вага A (float, optional)
- Вага B (float, optional)

### 7. Threshold Map
**Категорія:** Математика

Перетворює числову мапу у булеву маску за порігом:
- Вхід: float[,] мапа
- Вихід: bool[,] маска
- Параметри: поріг, операція (>, <, >=, <=, ==), інверсія

### 8. Smoothstep Mask
**Категорія:** Математика

Застосовує сгладжувану функцію до маски:
- Вхід: bool[,] маска або float[,] мапа
- Вихід: float[,] мапа зі сгладженими кордонами
- Параметри: радіус сгладження, тип функції

### 9. Cell Noise
**Категорія:** Генерація

Генерує випадкові клітини/патерни:
- Вихід: int[,] карта клітин або bool[,] маска
- Параметри: розмір клітин, випадковість, сім

### 10. Simplex Noise Mask
**Категорія:** Генерація

Аналог Perlin Noise Mask, але з Simplex шумом:
- Краща якість на великих масштабах
- Швидше обчислення з великою кількістю октав
- Відповідність Perlin Noise Mask усім параметрам

## Інтерфейсні рекомендації

### 11. Mask Blend
**Категорія:** Математика

Спеціалізований варіант AddNode для роботи з масками:
- Blend modes: Normal, Multiply, Screen, Overlay, Soft Light, Hard Light
- Підтримка як bool[,], так і float[,]

### 12. Noise Threshold Curve
**Категорія:** Математика

Використовує криву для нелінійного перетворення значень шуму:
- Вхід: float[,] мапа
- Вихід: float[,] мапа
- Параметри: AnimationCurve поріг, крива сгладження

## Задачі для реалізації

### ✅ Виконано
1. [x] Проаналізовано існуючу архітектуру нодів
2. [x] Визначено структуру нового ноду PerlinNoiseMaskNode
3. [x] Підготовлено код нового ноду
4. [x] Складено перелік рекомендованих нодів
5. [x] Створено файл `PerlinNoiseMaskNode.cs`

### ✅ Виконано
1. [x] Проаналізовано існуючу архітектуру нодів
2. [x] Визначено структуру нового ноду PerlinNoiseMaskNode
3. [x] Підготовлено код нового ноду
4. [x] Складено перелік рекомендованих нодів
5. [x] Створено файл `PerlinNoiseMaskNode.cs`
6. [x] Перевірено синтаксис та відповідність стилю проекту

### 🔄 Що залишилось

#### Основний нод (обов'язково для реалізації)
3. [ ] Тестування у графі генератора в Unity Editor

#### Додаткові ноди (опціонально)

##### Генераційні ноди
4. [ ] Створити `Assets/Moyva/Scripts/Features/Generator/Runtime/Nodes/PerlinNoiseMapNode.cs` (float[,] вихід)
5. [ ] Створити `Assets/Moyva/Scripts/Features/Generator/Runtime/Nodes/VoronoiMaskNode.cs`
6. [ ] Створити `Assets/Moyva/Scripts/Features/Generator/Runtime/Nodes/GradientMaskNode.cs`
7. [ ] Створити `Assets/Moyva/Scripts/Features/Generator/Runtime/Nodes/CheckerboardMaskNode.cs`
8. [ ] Створити `Assets/Moyva/Scripts/Features/Generator/Runtime/Nodes/DistanceFieldNode.cs`
9. [ ] Створити `Assets/Moyva/Scripts/Features/Generator/Runtime/Nodes/SimplexNoiseMaskNode.cs`

##### Математичні ноди
10. [ ] Створити `Assets/Moyva/Scripts/Features/Generator/Runtime/Nodes/ThresholdMapNode.cs`
11. [ ] Створити `Assets/Moyva/Scripts/Features/Generator/Runtime/Nodes/SmoothstepMaskNode.cs`
12. [ ] Створити `Assets/Moyva/Scripts/Features/Generator/Runtime/Nodes/NoiseCombineNode.cs`
13. [ ] Створити `Assets/Moyva/Scripts/Features/Generator/Runtime/Nodes/NoiseThresholdCurveNode.cs`

##### Інтерфейсні ноди
14. [ ] Створити `Assets/Moyva/Scripts/Features/Generator/Runtime/Nodes/MaskBlendNode.cs`

### 📋 Випробування

Для кожного нового ноду необхідно:
1. Переконатися, що нод з'являється в пошуковій дереві (Create Node)
2. Перевірити роботу виколнення з валідним контекстом
3. Перевірити превью (якщо реалізовано IPreviewableNode)
4. Перевірити обробку помилок (null вхідні дані, невідповідні розміри)
5. Перевірити серіалізацію налаштувань

## Важливі моменти реалізації

1. **MapSize**: Розмір карти береться з `context.MapSize`
2. **Seed**: Використовується `context.Seed` або за замовчуванням 1
3. **Превью**: Потрібно реалізувати `IPreviewableNode.GeneratePreview()`
4. **Атрибути**: Використовувати `NodeInfo`, `SerializeField`, `InlineEditable`, `Tooltip`, `Min`, `Range`
5. **Namespace**: `Kruty1918.Moyva.Generator.Runtime.Nodes`
6. **Базовий клас**: `NodeBase`, інтерфейс `IPreviewableNode` (для превью)
7. **Назви в UKR**: Використовуй українські назви у всіх атрибутах (NodeInfo, InlineEditable, підписи портів)
8. **Категорії**: "Генерація" замість "Generators", "Математика" замість "Math", "Графік" замість "Core"

## Архітектурні рекомендації

### Ноди без вхідних портів
Ноди, які генерують дані без залежностей від інших нодів, мають:
```csharp
public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();
```

### Ноди з двома вихідними портами
Коли нод повертає кілька значень (наприклад, маску та відповідні значення):
```csharp
public override PortDefinition[] Outputs => new[]
{
    PortDefinition.Output<bool[,]>("Mask"),
    PortDefinition.Output<float[,]>("Values")
};
```

### Робота з контекстом
Використовуйте `context?.MapSize` для отримання розміру карти:
```csharp
int w = Mathf.Max(1, context?.MapSize.x ?? 0);
int h = Mathf.Max(1, context?.MapSize.y ?? 0);
```

### Генерація превью
Для нодів з превью реалізуйте інтерфейс `IPreviewableNode`:
```csharp
public Texture2D GeneratePreview(int width, int height)
{
    // Повернути текстуру з візуалізацією результату
}
```

## Переваги нових нодів

### Perlin Noise Mask Node
1. **Гнучкість**: Налаштовуваний поріг дозволяє отримувати різні маски
2. **Контроль якості**: Параметри октав та persistency
3. **Превью**: Візуальна перевірка шуму перед застосуванням
4. **Сумісність**: Працює з існуючою системою графа

### Додаткові ноди
1. **Різноманітність**: Різні типи шуму та патернів
2. **Модульність**: Кожен нод відповідає за одну задачу
3. **Конструктивність**: Легко поєднуються в складні графи
4. **Производительность**: Оптимізовані алгоритми