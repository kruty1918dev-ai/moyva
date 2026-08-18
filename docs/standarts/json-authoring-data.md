# JSON Authoring Data

## Мета

JSON-файли є зрозумілим джерелом контентних даних, яке можна безпечно редагувати у Visual Studio або іншій IDE. Runtime продовжує читати ті самі `ScriptableObject`, тому перехід не змінює порядок ініціалізації, серіалізацію сцен, посилання чи save/load контракти.

## Потік даних

```text
JSON + JSON Schema -> editor validation/sync -> existing ScriptableObject -> unchanged runtime
```

- JSON — authoring source of truth для підключених каталогів.
- `ScriptableObject` — згенерований Unity-сумісний кеш і стабільна ціль для наявних посилань.
- Runtime не читає JSON напряму й не залежить від Newtonsoft JSON.
- Імпортер змінює asset лише тоді, коли серіалізовані дані справді відрізняються.
- Повторний sync незміненого JSON повинен повертати `changed=0`.

## Підключені каталоги

| Система | JSON | Згенерована ціль |
|---|---|---|
| Будівлі | `Assets/Moyva/Presets/Buildings/*.json` | `BuildingDefinitionAsset` + `BuildingRegistrySO` |
| Юніти | `Assets/Moyva/Presets/Units/unit-registry.json` | `UnitRegistrySO` |
| Економічні ресурси | `Assets/Moyva/Presets/Economy/resources.json` | `EconomyResourceDefinition` + `EconomyDatabaseSO` |

Схеми лежать у `Assets/Moyva/Presets/Schemas`. Поле `$schema` у кожному документі вмикає completion, описи полів, enum-підказки та перевірку помилкових ключів у IDE.

## Правила редагування

1. Не змінювати стабільні `id`/`typeId` після того, як вони потрапили у save-файли, без окремої міграції.
2. Для Unity assets використовувати читабельний `path` і стабільний `guid`; для sprite/sub-asset додатково зберігати `localId` та `subAsset`.
3. Нові поля додаються разом у DTO, JSON Schema, validator та importer/exporter.
4. Несумісна зміна формату збільшує `version` і отримує явну міграцію. Невідомі поля не ігноруються мовчки.
5. Видалення запису з JSON не видаляє Unity asset автоматично. Це окрема руйнівна операція з перевіркою посилань.
6. Для створення ресурсу додати запис із новим унікальним `asset` шляхом. Sync створить asset і додасть його до `EconomyDatabaseSO`.
7. Не редагувати одночасно JSON і згенерований asset. Якщо зміни спочатку зроблені в Unity Inspector, виконати відповідний `Export Current ...`, а потім продовжити роботу з JSON.

## Команди Unity Editor

- `Moyva/Data/JSON/Units/Validate and Sync`
- `Moyva/Data/JSON/Units/Export Current Registry`
- `Moyva/Data/JSON/Economy Resources/Validate and Sync`
- `Moyva/Data/JSON/Economy Resources/Export Current Database`
- наявні building preset-команди синхронізують пакет будівель.

Збереження JSON у `Assets/Moyva/Presets` запускає targeted auto-sync лише відповідного каталогу. Domain reload сам по собі більше не переписує building assets.

## Підключення наступної системи

Кожен новий JSON-backed каталог повинен мати:

1. версійований document DTO;
2. JSON Schema з `additionalProperties: false` для структурних вузлів;
3. semantic validator для правил, які не виражаються схемою;
4. детермінований importer без runtime-залежностей;
5. exporter із поточного asset для безпечної початкової міграції;
6. parity та idempotency перевірки.

Спільні читання/запис і Unity asset references реалізовані в `MoyvaJsonAuthoring`; система не повинна дублювати цей код.
