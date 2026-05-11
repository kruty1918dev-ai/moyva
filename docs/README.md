# Moyva - Документація проєкту

Онлайн-версія: [kruty1918dev-ai.github.io/moyva](https://kruty1918dev-ai.github.io/moyva/#home)

## Що таке Moyva

Moyva - це покрокова 2D-стратегія про розбудову та виживання королівства.
Гравець керує розвитком поселень, логістикою ресурсів, будівництвом, безпекою територій і поступовим формуванням власної історії кампанії.

Ключова ідея гри:
- не лише бої, а повний цикл життя світу;
- генерація унікальної мапи (рельєф, ріки, POI, дороги, ресурси);
- стратегічні рішення у мирі та в кризі (дефіцит, сезонність, смертність, оборона, розширення);
- подієва взаємодія між системами через сигнали та модульну архітектуру.

## Поточний фокус проєкту

Проєкт уже містить повний технічний каркас стратегії:
- процедурна генерація світу (Graph System + ноди + WFC);
- базові бойово-геймплейні системи (Grid, Units, Pathfinding, Interactions, Construction, Fog of War);
- економічний контур (runtime + designer + валідація + API для UI/інтеграцій);
- мультиплеєрний каркас (session manager, sync, fallback, host migration);
- календар і тики симуляції;
- save/load пайплайн;
- модульна документація + tooling для дизайнерів.

## Архітектурна суть

- Unity 2D + C#
- Zenject для DI
- SignalBus для подій між модулями
- Package-by-feature (API/Runtime/Editor)
- asmdef-модулі для ізоляції залежностей
- Editor-інструменти для налаштування без hardcode в runtime

## Карта систем (актуальна)

### 1) Головний вхід
- [Про проєкт Moyva](README.md)

### 2) Швидкий старт і туторіали
- [Швидкий старт](systems/graph-system/quickstart.md)
- [Ініціалізація сцени](systems/bootstrap.md)
- [Порядок ініціалізації](systems/initialization-order.md)
- [Туторіали](systems/construction/walls-setup-guide.md)

### 3) Генерація світу
- [Generator Pipeline](systems/generator.md)
- [Graph System](systems/graph-system/README.md)
- [API та власні ноди](systems/graph-system/api-reference.md)
- [Довідник нодів](systems/graph-system/nodes-generators.md)

### 4) Ігрові системи
- [Grid](systems/grid.md)
- [Objects Map](systems/objects-map.md)
- [Visuals](systems/visuals.md)
- [Animations](systems/animations.md)
- [Camera](systems/camera.md)
- [Interactions](systems/interactions.md)
- [Units](systems/units.md)
- [Pathfinding](systems/pathfinding.md)
- [Game Mode](systems/game-mode.md)
- [World Creation UI](systems/world-creation/README.md)

### 5) Будівництво
- [Construction - огляд](systems/construction.md)
- [ConstructionService](systems/construction/service.md)
- [BuildingRegistry](systems/construction/registry.md)
- [Construction UI](systems/construction/ui.md)
- [WallPlacement](systems/construction/wall-placement.md)

### 6) Fog of War
- [Fog of War - огляд](systems/fog-of-war/README.md)
- [Архітектура](systems/fog-of-war/architecture.md)
- [Алгоритм видимості](systems/fog-of-war/visibility-algorithm.md)
- [Texture Pipeline](systems/fog-of-war/texture-pipeline.md)
- [Інтеграція і тести](systems/fog-of-war/testing.md)

### 7) Save/Load
- [SaveSystem - огляд](systems/save-system/README.md)

### 8) Сигнальна шина
- [Signals - огляд](systems/signals/README.md)
- [Сигнали будівництва](systems/signals/building-placed.md)
- [Сигнали режимів гри](systems/signals/game-mode-changed.md)
- [Сигнали юнітів](systems/signals/unit-moved.md)

### 9) Економіка
- [Economy - огляд](systems/economy/README.md)
- [Economy Designer](systems/economy/economy-designer.md)
- [Economy API](systems/economy/economy-api-tab.md)
- [Каталог інтерфейсів](systems/economy/economy-interface-catalog.md)
- [Top 100 питань](systems/economy/economy-top-100-qa.md)

### 10) Мультиплеєр
- [Multiplayer - огляд](systems/multiplayer/README.md)
- [Архітектура](systems/multiplayer/architecture.md)
- [Синхронізація команд](systems/multiplayer/game-sync.md)
- [Host Migration](systems/multiplayer/host-migration.md)
- [Top 100 питань](systems/multiplayer/multiplayer-top-100-qa.md)

### 11) Календар
- [Calendar - огляд](systems/calendar/README.md)
- [Архітектура](systems/calendar/architecture.md)
- [Інтеграція з Multiplayer](systems/calendar/multiplayer-integration.md)

### 12) Bot AI
- [BotAI - огляд](systems/bot-ai/README.md)
- [BotBrain](systems/bot-ai/brain.md)

### 13) Стандарти і модульні індекси
- [Модульний індекс](modules/README.md)
- [TDD стандарт](standarts/TDD.md)
- [Naming Policy](standarts/naming-policy.md)
- [Ліміти розміру файлів](standarts/file-size-limits.md)
- [Common Utilities](standarts/common-utilities.md)
- [ProjectContext Data Policy](standarts/project-context-data-policy.md)
- [Runtime Config Lifecycle](standarts/runtime-config-lifecycle.md)
- [Domain Result Pattern](standarts/domain-result-pattern.md)
- [Reflection Startup Validation](standarts/reflection-startup-validation.md)
- [Service Mode Profiles](standarts/service-mode-profiles.md)
- [Feature Toggles for Risky Features](standarts/feature-toggles-risky-features.md)
- [Multiplayer Config Schema + Migration Pipeline](standarts/multiplayer-config-schema-migration.md)
- [Domain Events Layer](standarts/domain-events-layer.md)
- [Tech Debt Register](standarts/tech-debt-register.md)
- [Tech Debt Register Items](architecture/tech-debt-register.md)
- [Unit Designer Modular Facades](standarts/unit-designer-modular-facades.md)
- [Unit Designer Safe Edit Mode](standarts/unit-designer-safe-edit-mode.md)
- [Unit Designer Pre-Save Validation](standarts/unit-designer-pre-save-validation.md)
- [Unit Designer Batch Operations](standarts/unit-designer-batch-operations.md)
- [Designer Preset System](standarts/designer-preset-system.md)
- [Semantic Folders для Editor Tools](standarts/editor-tools-semantic-folders.md)
- [Dependency Map фіч-модулів](architecture/feature-dependency-map.md)
- [ADR Journal](architecture/adr/README.md)

## Суть ігрового циклу

У типовому сценарії гравець:
1. Генерує карту і стартує сесію.
2. Досліджує мапу, ставить ключові споруди, розгортає економіку.
3. Керує населенням, робітниками, запасами, логістикою і ризиками.
4. Реагує на події світу (дефіцит, сезонні зміни, втрати, туман війни).
5. Розвиває оборону й армію, утримує стабільність поселень.
6. За потреби грає в мультиплеєрі з синхронізацією стану і reconnect flow.

## Для кого ця документація

- Для gameplay-програмістів: API, сигнали, інтеграційні контракти.
- Для технічних дизайнерів: Economy/Calendar/Registry/Config Hub інструменти.
- Для QA і тест-інженерів: системні гайди, сценарії, точки валідації.

## Формат документації

Документація підтримує 2 взаємодоповнюючі структури:
- systems/* - практична системна документація для швидкої навігації;
- modules/* - глибока модульна структура (overview/architecture/data-model/workflow/api-contracts/integration/examples).

Якщо потрібен швидкий старт - починайте з systems.
Якщо потрібне глибоке впровадження в модуль - переходьте в modules.

