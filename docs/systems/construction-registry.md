# BuildingRegistry — Реєстр будівель

← [Назад до Construction](construction.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/construction-registry)

---

## Призначення

`BuildingRegistrySO` — це `ScriptableObject`-каталог, що містить усі доступні будівлі гри. Він надає зручний доступ до `BuildingDefinition` за ID або категорією та налаштовується у Unity Inspector без зміни коду.

---

## `BuildingDefinition` (DTO)

```csharp
namespace Kruty1918.Moyva.Construction.API
{
    [System.Serializable]
    public class BuildingDefinition
    {
        public string Id;               // Унікальний ідентифікатор, наприклад "barracks"
        public string DisplayName;      // Назва для UI, наприклад "Казарма"
        public BuildingCategory Category;
        public GameObject Prefab;       // Prefab будівлі (stub: null поки арт не готовий)
    }
}
```

---

## `BuildingCategory`

```csharp
namespace Kruty1918.Moyva.Construction.API
{
    public enum BuildingCategory
    {
        Military,     // Військові
        Civilian,     // Цивільні
        Industrial    // Індустріальні
    }
}
```

---

## `BuildingRegistrySO`

```csharp
namespace Kruty1918.Moyva.Construction.Runtime
{
    [CreateAssetMenu(menuName = "Moyva/Construction/BuildingRegistry")]
    public class BuildingRegistrySO : ScriptableObject
    {
        public BuildingDefinition[] Buildings;

        /// <summary>Знайти будівлю за її ID. Повертає null якщо не знайдено.</summary>
        public BuildingDefinition GetById(string id) =>
            System.Array.Find(Buildings, b => b.Id == id);

        /// <summary>Отримати всі будівлі заданої категорії.</summary>
        public BuildingDefinition[] GetByCategory(BuildingCategory category) =>
            System.Array.FindAll(Buildings, b => b.Category == category);
    }
}
```

---

## Приклади використання

### Заповнити меню категорії (UI код)

```csharp
[Inject] private BuildingRegistrySO _registry;

void PopulateMilitaryMenu()
{
    var militaryBuildings = _registry.GetByCategory(BuildingCategory.Military);
    foreach (var def in militaryBuildings)
    {
        // Створити кнопку def.DisplayName → при кліку: IConstructionService.SelectBuilding(def.Id)
    }
}
```

### Перевірити наявність будівлі перед розміщенням

```csharp
var def = _registry.GetById("barracks");
if (def == null)
    Debug.LogError("[Construction] BuildingDefinition 'barracks' не знайдено в реєстрі!");
```

---

## Поля Inspector

| Поле | Тип | Опис |
|---|---|---|
| `Buildings` | `BuildingDefinition[]` | Список усіх будівель, що можна розмістити |

> Кожен елемент `BuildingDefinition` серіалізується в Inspector:
> `Id`, `DisplayName`, `Category` (випадаючий список), `Prefab` (посилання на prefab).

---

## Реєстрація в Zenject

```csharp
// ConstructionInstaller.cs
Container.BindInstance(buildingRegistry).AsSingle();
```

`buildingRegistry` — поле `[SerializeField]` в `ConstructionInstaller`, призначається з Unity Inspector.

---

## Пов'язані системи

- [Construction (огляд)](construction.md)
- [construction-service.md](construction-service.md)
