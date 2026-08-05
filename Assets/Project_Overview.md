This technical documentation provides a comprehensive overview of the **Moyva** project, a stylized strategy/simulation game built with Unity 6. The project uses a modular, service-oriented architecture centered around Zenject for dependency injection and a SignalBus for decoupled communication.

---

## 1. Project Description
**Moyva** is a stylized strategy game featuring settlement building, unit management, and resource economy. The project emphasizes a "Bad North" inspired visual style with procedural world generation and turn-based time progression.
- **Core Pillars**: Procedural island/world generation, settlement construction, turn-based exploration (Fog of War), and a resource-driven economy.
- **Target Audience**: Players who enjoy tactical strategy, survival-lite settlement management, and stylized low-poly/pixel-art aesthetics.

## 2. Gameplay Flow / User Loop
1.  **Boot & Menu**: The game starts in the `HomeMenu` scene. Players can start a new game or load an existing one via the `SaveService`.
2.  **World Generation**: Upon starting a new game, the `MapVisualWorldBuildOrchestrator` generates the environment using `TileWorldCreator`.
3.  **Bootstrap**: The `BootstrapInstaller` and `BootstrapGameInitializer` set up the player's starting position, reveal the local Fog of War, and grant a "Starter Pack" of resources.
4.  **Core Loop**:
    *   **Construction**: Build structures (Town Halls, Houses, etc.) using `ConstructionService`.
    *   **Exploration**: Move units across the hex/orthogonal grid. Moving units consumes stamina and advances the game time via `ICalendarService`.
    *   **Economy**: Settlement production cycles (Population → Workers → Production) run based on economic ticks.
5.  **Persistence**: The game state is saved either manually or automatically upon exit via `ISaveModule` implementations for each feature (Units, Economy, Construction).

## 3. Architecture
The project follows a **Service-Oriented Architecture (SOA)** combined with **Dependency Injection (Zenject)** and **Reactive Messaging (SignalBus)**.

*   **Dependency Injection**: Extensively uses `MonoInstaller` and `ScriptableObjectInstaller`. Key contexts include `ProjectContext` (global services) and scene-specific installers (e.g., `BootstrapInstaller`, `GridInstaller`).
*   **Decoupling**: Systems communicate primarily via `SignalBus`. For example, `UnitService` listens for `UnitMovedSignal` to update stamina and trigger turn progression.
*   **Execution Order**: Managed strictly within installers (e.g., `Container.BindExecutionOrder<StartingPositionInitializer>(101)`) to ensure world generation completes before unit spawning.

`Location: Assets/Moyva/Scripts/Infrastructure`

## 4. Game Systems & Domain Concepts

### Grid & Movement System
Handles spatial logic, tile metadata, and pathfinding.
- `IGridService`: Manages the underlying grid data and tile lookups.
- `ITileSettingsService`: Provides data on tile weights (movement cost) and types.
- `IGridProjection`: Handles coordinate conversion (Hex, Isometric, Orthogonal).
- `UnitService`: Manages unit stamina, positions, and registration.
`Location: Assets/Moyva/Scripts/Features/Grid` & `Assets/Moyva/Scripts/Features/Units`

### Fog of War (FoW) System
A multi-layered vision system that handles "shroud" (unexplored) and "fog" (hidden).
- `IFogOfWarService`: The main facade for fog state.
- `IFogVisibilityResolver`: Logic for line-of-sight and vision ranges.
- `IFogVisualUpdater`: Updates the visual representation (Volume or Texture-based).
`Location: Assets/Moyva/Scripts/Features/FogOfWar`

### Construction & Buildings
Handles settlement growth and building placement.
- `IConstructionService`: Manages the state of player-placed buildings.
- `BuildingDefinition`: ScriptableObject defining building costs, footprints, and visuals.
- `SettlementRegistry`: Tracks active settlements and their Town Hall locations.
`Location: Assets/Moyva/Scripts/Features/Construction`

### Economy & Settlement System
A tick-based simulation of resources and population.
- `EconomyManager`: Orchestrates the economic tick (Production/Consumption).
- `EconomyOwnerResourcePoolService`: Manages the actual resource counts for players.
- `EconomyTurnProcessorService`: Processes turn-based economic updates.
`Location: Assets/Moyva/Scripts/Features/Economy`

## 5. Scene Overview
*   **HomeMenu**: The entry point. Handles game settings, save slot selection, and menu-specific world previews.
*   **Gameplay_Scene**: The main simulation scene. Contains the `MapVisualWorldBuildOrchestrator` and all gameplay installers.
*   **HomeMenuUI / Test**: Sub-scenes or utility scenes used for UI overlay and isolated feature testing.
*   **Loading Flow**: `GameLaunchContext` stores the configuration (Save slot, New Game vs. Load) which is read by `DirectGameplayLaunchModeInitializer` during scene load.

## 6. UI System
The project uses **UGUI** (referenced via `com.unity.ugui`) and **TextMeshPro**.
- **Architecture**: UI is generally decoupled using "Presenters" or "Controllers" (e.g., `EconomyPlayerResourceSummaryUIController`) that listen to signals and update the view.
- **Construction UI**: `ConstructionUIController` manages the building menu, tabs, and placement state.
- **Styling**: Uses custom Atlases, Fonts, and Themes located in `Assets/Moyva/UI`.
`Location: Assets/Moyva/Scripts/Features/[FeatureName]/UI`

## 7. Asset & Data Model
*   **Persistence (.mvs)**: Custom binary format with CRC verification.
*   **ISaveModule**: Features implement this interface to write/read their state to the `ISaveContext`.
    *   `UnitsSaveModule`: Serializes unit types and positions.
    *   `ConstructionSaveModule`: Serializes building IDs and tile coordinates.
*   **ScriptableObjects**: Used for configuration:
    *   `BuildingDefinition`: Building stats and visuals.
    *   `ConstructionSystemProfileSO`: Global construction settings.
    *   `AudioRegistrySO`: Centralized audio clip management.
*   **Prefabs**: Categorized by `Buildings`, `Units`, `Tiles`, and `UI` within `Assets/Moyva/Prefabs`.

## 8. Notes, Caveats & Gotchas
*   **Direct Scene Start**: The `DirectGameplayLaunchModeInitializer` allows developers to start `Gameplay_Scene` directly in the Editor by configuring a "solo/no-save" test mode if no `GameLaunchContext` is provided.
*   **Time Progression**: Moving a unit calls `ICalendarService.AdvanceTurn()`. If time-sensitive systems (like Economy) are active, they will react to these turn increments.
*   **Partial Classes**: The `FogOfWarService` and several other large services use `partial` classes to separate responsibilities (e.g., API, Internal State, Rendering).
*   **Diagnosis**: A robust diagnostic system (`DiagnosticsInstaller`) tracks world generation and save/load flows, logging detailed performance and state data.