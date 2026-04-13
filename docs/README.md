# Moyva - Документація проєкту

Онлайн-версія: [kruty1918dev-ai.github.io/moyva](https://kruty1918dev-ai.github.io/moyva/#home)

## Точка входу

- [Модульний індекс документації](modules/README.md)
- [Стандарти](standarts/TDD.md)
- [Legacy документація систем](systems/)

## Коротко про проєкт

Moyva - покрокова стратегія на Unity 2D з DI (Zenject), сигналами через SignalBus, модульною архітектурою API/Runtime та окремими editor/runtime інструментами.

## Ієрархія модулів

<details>
<summary>Core система</summary>

- [Core Index](modules/core/README.md)
- [Bootstrap](modules/core/bootstrap/README.md)
- [Signals](modules/core/signals/README.md)
- [Initialization Order](modules/core/initialization-order/README.md)
- [Save System](modules/core/save-system/README.md)

</details>

<details>
<summary>World система</summary>

- [World Index](modules/world/README.md)
- [Grid](modules/world/grid/README.md)
- [Objects Map](modules/world/objects-map/README.md)
- [Visuals](modules/world/visuals/README.md)
- [Fog Of War](modules/world/fog-of-war/README.md)
- [Camera](modules/world/camera/README.md)

</details>

<details>
<summary>Gameplay система</summary>

- [Gameplay Index](modules/gameplay/README.md)
- [Units](modules/gameplay/units/README.md)
- [Interactions](modules/gameplay/interactions/README.md)
- [Pathfinding](modules/gameplay/pathfinding/README.md)
- [Game Mode](modules/gameplay/game-mode/README.md)
- [Construction](modules/gameplay/construction/README.md)
- [Wall System](modules/gameplay/wall-system/README.md)
- [Resolver](modules/gameplay/resolver/README.md)
- [Object Picker](modules/gameplay/object-picker/README.md)
- [Economy (legacy systems docs)](systems/economy/README.md)
- [Bot AI (legacy systems docs)](systems/bot-ai/README.md)

</details>

<details>
<summary>Tooling система</summary>

- [Tooling Index](modules/tooling/README.md)
- [Resolver Editor](modules/tooling/resolver-editor/README.md)
- [Registry Hub](modules/tooling/registry-hub/README.md)
- [Registries](modules/tooling/registries/README.md)
- [Save System Designer](modules/tooling/save-system-designer/README.md)
- [Economy Designer (legacy systems docs)](systems/economy/economy-designer.md)

</details>

<details>
<summary>Generation система</summary>

- [Generation Index](modules/generation/README.md)
- [Generator Pipeline](modules/generation/generator/README.md)
- [Graph System](modules/generation/graph-system/README.md)
- [Auto Tile Naming](modules/generation/auto-tile-naming/README.md)

</details>

## Окремий практичний гайд

- [Гайд: створення нової колекції в Resolver](modules/gameplay/resolver/create-collection-guide.md)
- [Гайд: Economy Tutorials](systems/economy/tutorials.md)

## Єдиний формат для всіх модулів

Кожен модуль має однакову структуру:

- README.md (індекс модуля)
- overview.md
- architecture.md
- data-model.md
- workflow.md
- api-contracts.md
- integration.md
- examples.md
- create-collection-guide.md (де це застосовно)

У великих розділах використано collapsible-блоки для зменшення візуального шуму.

