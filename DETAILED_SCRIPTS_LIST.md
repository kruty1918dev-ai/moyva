oleks@oleks-HP-ProBook-470-G4:~/moyva$  cd /home/oleks/moyva && cat > DETAILED_S
CRIPTS_LIST.md << 'EOF'
> # 📋 ДЕТАЛЬНИЙ ПЕРЕЛІК УСІХ 824 СКРИПТІВ
> 
> ## Як користуватися цим документом
> 
o> Кожен модуль перелічує свої скрипти з поточним статусом і рекомендаціями змін
> 
> **Умовні позначення:**
> - ✅ Стабільно - немаємо планів змінювати
> - 🔧 Рефакторинг - можуть потребувати розбиття/оптимізації
> - 🚀 Активна розробка - постійно удосконалюється
> - 🔴 Переписується - в процесі переревізії архітектури
> 
> ---
> 
> ## BOOTSTRAP & SHARED (50-60 скриптів)
> 
> ### Bootstrap/Runtime
> ```
> ✅ AppInstallerComposer.cs
> ✅ BootstrapInstaller.cs
> ✅ DatabaseLoaderService.cs
> ✅ GeneralSettingsProvider.cs
> ✅ RuntimeInitializer.cs
> ```
> 
> ### Shared/Audio
> ```
> ✅ AudioService.cs
> ✅ IAudioService.cs
> ✅ SoundDefinition.cs
> ✅ SoundDefinitionSO.cs
> ✅ MusicDefinition.cs
> ✅ MusicDefinitionSO.cs
> ```
> 
> ### Shared/Common
> ```
> ✅ Assert.cs
> ✅ StringBuilderPool.cs
> ✅ DisposableComparer.cs
> ✅ SafeCoroutine.cs
> ✅ Utilities.cs
> ... (10-15 файлів)
> ```
> 
> ### Shared/Connectivity
> ```
> ✅ INetworkService.cs
> ✅ NetworkService.cs
> ✅ MessageSerializer.cs
> ... (5-10 файлів)
> ```
> 
> ### Shared/Diagnostics
> ```
> ✅ IDebugLogger.cs
> ✅ DebugLogger.cs
> ✅ PerformanceProfiler.cs
> ... (5-8 файлів)
> ```
> 
n> ### Shared/Notifications
> ```
> ✅ NotificationService.cs
> ✅ INotificationService.cs
> ... (3-5 файлів)
> ```
> 
c> ### Shared/Performance
> ```
> ✅ MemoryOptimizer.cs
> ✅ ObjectPoolService.cs
> ... (3-5 файлів)
> ```
> 
> **ИТОГО Bootstrap & Shared**: ~50-60 скриптів
> 
> ---
> 
> ## FEATURES: 25 МОДУЛІВ (~700 скриптів)
> 
> ### 1️⃣ ANIMATIONS (4 скрипти) ✅
> 
> ```
> Runtime/
>   ✅ MovementAnimationService.cs      - Движок анімаціїв персонажів
>   ✅ PathAnimationSettings.cs          - Налаштування анімаціїв шляху
>   ✅ IMovementAnimationService.cs      - Інтерфейс сервісу
>   ✅ AnimationsInstaller.cs            - DI конфіг
> ```
> 
> **Що змінювати**: Ніщо. Стабільно.
> 
> ---
> 
> ### 2️⃣ BOTAI (10 скриптів) 🔧
> 
> ```
> API/
>   ✅ IBotController.cs
>   ✅ IBotDifficultySettings.cs
>   ✅ BotState.cs
>   ✅ DifficultyLevel.cs
>   ✅ FactionId.cs
> 
> Runtime/
>   🔧 BotAiService.cs
>   🔧 BotDecisionTreeEvaluator.cs
>   🔧 BotActionExecutor.cs
>   🔧 BotMemorySystem.cs
>   ✅ AssemblyInfo.cs
> ```
> 
> **Що змінювати**: 
> - Розбийте BotDecisionTreeEvaluator (якщо >500 строк)
> - Оптимізуйте BotMemorySystem для великої кількості ботів
> - Додайте деталізацію логування для отладки AI
> 
> ---
> 
> ### 3️⃣ CALENDAR (15 скриптів) ✅
> 
> ```
> API/
>   ✅ ICalendarService.cs
>   ✅ ICalendarSyncAdapter.cs
>   ✅ GameDateTime.cs
>   ✅ DayPhase.cs
>   ✅ CalendarConfigLifecycle.cs
> 
> Runtime/
>   ✅ CalendarService.cs
>   ✅ CalendarInstaller.cs
>   ✅ GameTimeController.cs
>   ✅ TimeScaleProvider.cs
>   ✅ SeasonCalculator.cs
> 
> Editor/
>   ✅ CalendarSettingsEditor.cs
>   ✅ GameDateTimeDrawer.cs
ershipService.cs
  ✅ FactionDefin>   ... (3-4 інші файли)
> ```
> 
> **Що змінювати**: Ніщо. Стабільна система.
> 
> ---
> 
> ### 4️⃣ CAMERA (10 скриптів) ✅
> 
> ```
> API/
>   ✅ ICameraMovement.cs
>   ✅ ICameraZoom.cs
cySystem.cs
```

**Що з>   ✅ ICameraFocused.cs
>   ✅ CameraSettingsSO.cs
> 
> Runtime/
>   ✅ CameraController.cs
>   ✅ CameraMovementService.cs
>   ✅ CameraZoomService.cs
>   ✅ CameraMapRenderMaskService.cs
> 
> Editor/
>   ✅ CameraSettingsEditor.cs
> ```
> 
> **Що змінювати**: Ніщо. Стабільна система.
> 
> ---
> 
> ### 5️⃣ CLOUDS (7 скриптів) ✅
> 
> ```
> API/
>   ✅ ICloudsService.cs
>   ✅ CloudSpriteVariant.cs
>   ✅ CloudsSettings.cs
> 
> Runtime/
>   ✅ CloudsService.cs
>   ✅ CloudsInstaller.cs
> 
F> Editor/
>   ✅ CloudsSettingsEditor.cs
>   ✅ CloudSpriteVariantDrawer.cs
> ```
> 
> **Що змінювати**: Ніщо. Декоративна система.
> 
> ---
> 
> ### 6️⃣ CONSTRUCTION (58 скриптів) 🔧
> 
> ```
> API/
>   ✅ IConstructionInputService.cs
>   ✅ IGeneratedTerrainLevelQuery.cs
>   ✅ IObjectTypePicker.cs
>   ✅ IBuildingRegistry.cs
>   ✅ BuildingDefinition.cs
>   ✅ BuildingModule.cs
>   ✅ BuildingDefinitionCapabilities.cs
>   ... (10+ SD інші типи будівель)
> 
> Runtime/
>   🔧 ConstructionGridService.cs       - Велике (~400+ строк)
>   🔧 ConstructionValidationService.cs - Розбити на 2 класи
>   🔧 BuildingPreviewService.cs
>   🔧 BuildingPlacementProcessor.cs
>   🔧 ConstructionInstaller.cs
>   ... (15+ інші сервіси)
> 
> Editor/
>   ✅ BuildingDefinitionEditor.cs
>   ✅ BuildingModuleDrawer.cs
>   ... (5+ інші редактори)
> 
> UI/
>   ✅ ConstructionUIController.cs
>   ✅ ConstructionPreviewRenderer.cs
>   ... (5+ UI компоненти)
> ```
> 
> **Що змінювати**:
> - Розбийте ConstructionGridService → GridQueryService + GridUpdateService
> - Розбийте ConstructionValidationService → TerrainValidator + BuildingValidato
r
> - Додайте unit тести для валідації
> 
�> ---
> 
> ### 7️⃣ ECONOMY (52 скрипти) 🔴 **ПЕРЕПИСУЄТЬСЯ**
> 
> ```
> API/
>   🔴 ISettlementRegistry.cs
>   🔴 IEconomyBuildingIntegration.cs
>   🔴 IEconomyTurnProcessor.cs
>   🔴 EconomyDatabaseSO.cs
>   🔴 EconomyRulesConfigSO.cs
>   🔴 EconomyResourceDefinition.cs
>   🔴 EconomyProductionProfile.cs
>   🔴 EconomySettlementState.cs
>   ... (8+ конфіг класи)
> 
n> Runtime/
>   🔴 EconomyManager.cs                - ОСНОВНИЙ фасад
>   🔴 EconomySettlementRegistryService.cs - НОВЕ
>   🔴 EconomyBuildingIntegrationService.cs - НОВЕ
>   🔴 EconomyTurnProcessorService.cs   - НОВЕ
>   🔴 EconomyTickOrchestrator.cs       - Розбити на сервіси
>   🔴 EconomyPopulationService.cs
>   🔴 EconomyProductionTickService.cs
>   🔴 EconomyWorkerAllocationService.cs
>   🔴 EconomyOwnerResourcePoolService.cs
>   ... (15+ інші сервіси)
> 
> Editor/
>   🔴 EconomyDatabaseEditor.cs
>   ... (2-3 редактори)
> 
> UI/
>   🔴 SettlementUIPanel.cs
>   🔴 ResourceDisplayWidget.cs
>   ... (8+ UI компоненти)
> ```
> 
> **ЩО ЗМІНЮВАТИ (ПЛАН ПЕРЕПИСУВАННЯ)**:
> 1. ✅ Розбийте EconomyManager на сервіси (ЗРОБЛЕНO)
>    - EconomySettlementRegistryService (реєстр поселень)
>    - EconomyBuildingIntegrationService (інтеграція з Construction)
>    - EconomyTurnProcessorService (обработка ходів)
> 
> 2. Розбийте EconomyTickOrchestrator на мікро-сервіси:
>    - PopulationTickService (народонаселення)
>    - ProductionTickService (виробництво)
>    - ResourceConsumptionService (споживання)
> 
�> 3. Додайте нові інтеграції:
>    - Торгівля між поселеннями
>    - Караванна система
>    - Динамічні ціни
> 
> 4. Напишіть комплексні тести в Economy/{SettlementManagement, Production, Trad
e}Tests.cs
> 
> ---
> 
> ### 8️⃣ FACTION (10 скриптів) ✅
> 
> ```
> API/
>   ✅ IFactionRegistry.cs
>   ✅ IFactionOwnershipService.cs
>   ✅ FactionDefinition.cs
>   ✅ FactionType.cs
>   ✅ FactionId.cs
> 
> Runtime/
>   ✅ FactionRegistryService.cs
>   ✅ FactionOwnershipService.cs
>   ✅ FactionInstaller.cs
>   ✅ RelationshipTracker.cs
>   ✅ DiplomacySystem.cs
> ```
> 
> **Що змінювати**: Ніщо. Стабільна система.
> 
> ---
> 
�> ### 9️⃣ FOGOFWAR (22 скрипти) 🔧
> 
 > ```
> API/
>   ✅ IFogOfWarService.cs
>   ✅ IFogVisibilityResolver.cs
>   ✅ IHeightAwareVisionService.cs
>   ✅ IFogTextureUpdater.cs
e>   ✅ FogOfWarSettings.cs
> 
> Runtime/
>   🔧 FogOfWarService.cs          - Велике, потребує рефакторингу
>   🔧 FogVisibilityResolver.cs    - Алгоритм видимості
>   🔧 HeightAwareVisionService.cs
>   🔧 FogTextureUpdater.cs
>   ... (10+ інші компоненти)
> 
> Editor/
>   ✅ FogOfWarSettingsEditor.cs
>   ... (3-4 редактори)
> ```
> 
> **Що змінювати**:
> - Оптимізуйте алгоритм видимості (Bresenham луч vs Raycast)
> - Кешуйте результати для статичних об'єктів
> - Додайте параллелізацію обновлення туману
> 
> ---
> 
> ### 🔟 GAMEMODE (10 скриптів) ✅
> 
> ```
> API/
>   ✅ IGameModeService.cs
>   ✅ IGameStateService.cs
>   ✅ IGameModePanel.cs
>   ✅ WinConditionSO.cs
>   ✅ GameModeDefinition.cs
> 
> Runtime/
>   ✅ GameModeService.cs
>   ✅ GameStateService.cs
>   ✅ GameModeInstaller.cs
>   ✅ VictoryConditionChecker.cs
>   ✅ DefeatConditionChecker.cs
> ```
> 
> **Що змінювати**: Ніщо. Стабільна система.
> 
> ---
�Р�> 
> ### 1️⃣1️⃣ GENERATOR (168 скриптів) 🚀 **НАЙБІЛЬШИЙ МОДУЛЬ**
> 
�> ```
> API/ (15 файлів)
>   ✅ IMapObjectVisualRegistryService.cs
>   ✅ INoiseProvider.cs
>   ✅ IRiverPathfinder.cs
>   ✅ ISeedProvider.cs
>   ✅ IVirtualHeightMapGenerator.cs
>   ✅ IWFCService.cs
>   ✅ MapObjectDefinition.cs
>   ✅ MapObjectTerrainConfig.cs
>   ✅ ObjectConnectionRulesSO.cs
>   ✅ BiomeData.cs
>   ... (5 більше конфіг SO)
> 
> Nodes/ (80+ типів вузлів)
>   Noise Nodes:
>     🚀 FBMNoiseNode.cs
>     🚀 IslandNoiseNode.cs
>     🚀 PerlinNoiseNode.cs
>     ...
> 
4>   Height Processing:
>     🚀 HeightSourceNode.cs
>     🚀 HeightMathBlendNode.cs
>     🚀 ErosionNode.cs
>     🚀 HeightSupplementNode.cs
>     ...
> 
>   Terrain:
>     🚀 BiomePainterNode.cs
>     🚀 BiomeResolverNode.cs
>     🚀 TerrainSlopeFilterNode.cs
>     🚀 AutoTileTransitionNode.cs
>     ...
> 
>   Boolean Operations:
>     🚀 BoolAndNode.cs
>     🚀 BoolOrNode.cs
 MenuState
   - WorldCr>     🚀 BoolXorNode.cs
>     🚀 BoolInvertNode.cs
>     ...
> 
t>   Cities & Objects:
>     🚀 CityGeneratorNode.cs
>     🚀 ForestClusterNode.cs
>     🚀 ObjectAutoTileNode.cs
>     🚀 MergeObjectMapNode.cs
>     ...
> 
>   Advanced:
>     🚀 WFCPatternNode.cs
>     🚀 ChokepointAnalyzerNode.cs
>     🚀 ConstraintPolishNode.cs
>     ...
> 
> Runtime/ (40+ файлів)
�нтів в сервіси
   - Використовуйте SignalBus для UI повідомлень

3. **Рефакторинг панелей**:
   - Кожна панель = �>   🚀 MapDataGenerator.cs
>   🚀 GraphBasedMapDataGenerator.cs
>   🚀 GeneratorDataRegistry.cs
>   🚀 MapObjectVisualRegistryService.cs
>   🚀 HeightLayerTileSelector.cs
>   🚀 BiomeResolver.cs
>   🚀 GeneratorTerrainLevelService.cs
>   🚀 GeneratorInstaller.cs
>   ... (30+ утіліти, сервіси, помічники)
> 
> Editor/ (30+ файлів)
>   🚀 NODE EDITORS (один для кожного вузла)
>   🚀 WFCRulesEditorWindow.cs
>   🚀 ObjectConnectionRulesSOEditor.cs
>   🚀 TileHeightTableSOEditor.cs
>   ... (25+ редактори, дроєри)
> ```
> 
> **ЩО ЗМІНЮВАТИ (ПРІОРИТЕТ)**:
> 1. **Оптимізація алгоритмів**:
>    - ErosionNode: зменшити ітерації при збереженні якості
>    - FBMNoiseNode: кешувати октави
Стабільна система.

---

### 1️⃣7️⃣ MULTIPLAYER (76 скриптів) 📡 **АКТИВНА РОЗРОБКА**

```
API/ (15>    - WFC: оптимізувати constraint propagation
> 
> 2. **Рефакторинг архітектури**:
>    - Винесіть базовий клас для всіх вузлів (BaseNode → розбийте на 3 типи)
>    - Створіть NodeFactory для реєстрації вузлів
>    - Додайте NODE_PROFILING макрос
> 
> 3. **Документація**:
>    - Додайте коментарі до кожного вузла (що він робить, параметри)
>    - Створіть приклади графів для кожного типу біому
> 
> ---
> 
> ### 1️⃣2️⃣ GRAPHSYSTEM (37 скриптів) 🚀
> 
> ```
> API/ (10 файлів)
>   ✅ IPreviewableNode.cs
>   ✅ IExternalNode.cs
>   ✅ GraphExecutionResult.cs
>   ✅ NodeContext.cs
>   ✅ NodeOutput.cs
>   ✅ Connection.cs
>   ... (5+ базові типи)
> 
> Runtime/ (20 файлів)
>   🚀 Graph.cs
>   🚀 GraphExecutor.cs
>   🚀 GraphValidator.cs
>   🚀 NodeRegistry.cs
>   🚀 ConnectionValidator.cs
>   ... (15+ утіліти, обходи, сортування)
> 
�> Editor/ (7 файлів)
>   ✅ GraphEditorWindow.cs
>   ✅ GraphNodeView.cs
>   ✅ GraphConnectionView.cs
>   ✅ GraphDebugger.cs
>   ... (3 редактори)
> ```
> 
> **Що змінювати**:
> - Додайте граф кеш для швидшого переобчислення
> - Реалізуйте undo/redo для редактора графів
> - Додайте профайлінг часу виконання вузлів
> 
> ---
> 
> ### 1️⃣3️⃣ GRID (8 скриптів) ✅
> 
�> ```
> API/
>   ✅ IGridService.cs
>   ✅ ITileSettingsService.cs
>   ✅ TileIdAttribute.cs
>   ✅ TileRegistrySO.cs
> 
> Runtime/
>   ✅ GridService.cs
>   ✅ TileSettingsProvider.cs
>   ✅ HexagonalGridCalculator.cs
>   ✅ GridInstaller.cs
> ```
> 
> **Що змінювати**: Ніщо. Фундаментальна система, стабільна.
> 
> ---
> 
> ### 1️⃣4️⃣ HOMEMENU (114 скриптів) 📊 **ДРУГИЙ ПО РОЗМІРУ**
> 
> ```
> API/ (15 файлів)
>   ✅ IGameSettingsPanelService.cs
>   ✅ IWorldCreationPanelService.cs
>   ✅ ISelectedGameModeService.cs
>   ✅ IHomeMenuGameStarter.cs
>   ✅ OverlayLoaderResult.cs
>   ... (10 сервіси, інтерфейси)
> 
> Runtime/ (60+ файлів)
>   🔧 HomeMenuController.cs        - ВЕЛИКЕ (розбити)
>   🔧 WorldCreationController.cs   - ВЕЛИКЕ (розбити)
>   🔧 GameSettingsPanelService.cs
>   🔧 SelectedGameModeService.cs
>   🔧 HomeMenuGameStarter.cs
>   ... (50+ контролери, сервіси)
iplay> 
e> UI/ (40+ файлів)
>   🔧 MenuButtonWidget.cs
>   🔧 SettingsPanel.cs
>   🔧 WorldCreationPanel.cs
>   🔧 GameModeSelector.cs
>   🔧 DifficultySelector.cs
>   ... (35+ UI компоненти)
> ```
> 
> **ЩО ЗМІНЮВАТИ (КРИТИЧНО)**:
> 1. **Розбити великі контролери**:
>    - HomeMenuController → MenuNavigation + MenuState
>    - WorldCreationController → WorldSettings + PresetManager + GeneratorConfig
urator
> 
> 2. **Очистити UI логіку**:
>    - Винесіть логіку з UI компонентів в сервіси
>    - Використовуйте SignalBus для UI повідомлень
> 
> 3. **Рефакторинг панелей**:
>    - Кожна панель = окремий клас (вже більш-менш так)
>    - Додайте переходи між панелями через стан
> 
> ---
> 
> ### 1️⃣5️⃣ INFOPANEL (2 скрипти) ✅
> 

> ```
> UI/
>   ✅ WorldInfoPanelInstaller.cs
>   ✅ BuildingInfoPanelController.cs
> ```
> 
> **Що змінювати**: Ніщо. Мала система.
> 
> ---
> 
> ### 1️⃣6️⃣ INTERACTIONS (7 скриптів) ✅
> 
> ```
> API/
>   ✅ ITileInteractionService.cs
>   ✅ MapObjectWorldInfoPresenter.cs
> 
> Runtime/
>   ✅ TileClickInputService.cs
>   ✅ WorldInfoSelectionCoordinator.cs
>   ✅ InteractionsInstaller.cs
>   ... (3 сервіси)
> ```
> 
> **Що змінювати**: Ніщо. Стабільна система.
> 
> ---
> 
> ### 1️⃣7️⃣ MULTIPLAYER (76 скриптів) 📡 **АКТИВНА РОЗРОБКА**
> 
> ```
> API/ (15 файлів)
>   ✅ IConfigSyncService.cs
>   ✅ IGameCommandSyncService.cs
>   ✅ GameCommandType.cs
>   ✅ MultiplayerConfig.cs
>   ... (11 протоколи, команди, типи)
> 
> Runtime/ (50+ файлів)
>   🚀 MultiplayerGameController.cs
>   🚀 ConfigSyncService.cs
>   🚀 GameCommandSyncService.cs
>   🚀 PlayerSessionManager.cs
>   🚀 ReplicatedStateService.cs
>   🚀 NetworkMessageHandler.cs
>   ... (45+ синхронізація, обробка команд, serialization)
> 
> UI/ (10+ файлів)
>   ✅ MultiplayerLobbyPanel.cs
>   ✅ PlayerListWidget.cs
>   ✅ NetworkStatusIndicator.cs
>   ... (8+ UI для мультиплеєру)
> ```
> 
> **ЩО ЗМІНЮВАТИ**:
> 1. **Оптимізація синхронізації**:
>    - Використовуйте delta compression для стану
>    - Кешуйте часто відправляєні команди
>    - Додайте bandwidth monitoring
> 
e> 2. **Рефакторинг команд**:
>    - Створіть CommandDispatcher замість прямих методів
>    - Додайте replay логіку для відтворення команд
> 
> 3. **Тестування**:
>    - Напишіть тести для синхронізації стану
>    - Напишіть тести для розвалу зв'язку і відновлення
> 
> ---
> 
> ### 1️⃣8️⃣ OBJECTSMAP (3 скрипти) ✅
> 
 > ```
> API/
>   ✅ IObjectsMapService.cs
> 
> Runtime/
>   ✅ ObjectsMapService.cs
>   ✅ ObjectsMapInstaller.cs
> ```
> 
/> **Що змінювати**: Ніщо. Мала, функціональна система.
> 
> ---
> 
> ### 1️⃣9️⃣ PATHFINDING (3 скрипти) ✅
> 
> ```
> API/
>   ✅ IPathfinder.cs
> 
> Runtime/
>   ✅ Pathfinder.cs
>   ✅ PathfinderInstaller.cs
> ```
> 
> **Що змінювати**: Ніщо. Мала система, використовує A* алгоритм.
> 
> ---
> 
�> ### 2️⃣0️⃣ SAVESYSTEM (17 скриптів) ✅
> 
8> ```
> API/ (5 файлів)
>   ✅ ISaveService.cs
>   ✅ ISaveContext.cs
>   ✅ IConfigService.cs
�ИКА

>   ✅ SaveSlotInfo.cs
> 
> Runtime/ (10 файлів)
>   ✅ SaveService.cs
>   ✅ SaveContext.cs
>   ✅ ConfigService.cs
>   ✅ SaveFileFormatter.cs
>   ✅ SaveGameSerializer.cs
>   ... (6+ утіліти для серіалізації)
> 
�> Editor/
>   ✅ SaveGameInspector.cs
> ```
> 
> **Що змінювати**: Ніщо. Стабільна система.
> 
> ---
> 
> ### 2️⃣1️⃣ SIGNALS (15 скриптів) ✅
> 
> ```
> API/ (10 файлів)
>   ✅ OnGameStateSignals.cs
>   ✅ OnEconomySignals.cs
>   ✅ OnConstructionSignals.cs
>   ✅ OnMultiplayerSignals.cs
>   ✅ TileClickedSignal.cs
>   ✅ BuildingPlacedSignal.cs
>   ... (4 більше сигнали)
> 
> Runtime/ (5 файлів)
>   ✅ SignalBus.cs (основний - використовується скрізь)
>   ✅ SignalBusInstaller.cs
>   ✅ IEconomyInfoMediator.cs
>   ... (3 медіатори)
> ```
> 
> **Що змінювати**: Ніщо. Фундаментальна система Pub/Sub.
> 
> ---
> 
> ### 2️⃣2️⃣ UNITS (30 скриптів) 🔧
> 
> ```
> API/ (10 файлів)
>   ✅ IUnitMovementService.cs
>   ✅ IUnitClassConfig.cs
>   ✅ UnitCombatModel.cs
>   ✅ UnitIdAttribute.cs
>   ✅ UnitDefinition.cs
>   ... (5 типи, інтерфейси)
> 
> Runtime/ (15 файлів)
>   🔧 UnitController.cs           - Потребує розбиття

EOF
cat DETAILED_SCRIPTS_LIST.md
>   🔧 UnitMovementService.cs
>   🔧 UnitCombatService.cs
>   🔧 UnitStateManager.cs
>   🔧 UnitAnimationController.cs
>   ... (10+ сервіси для юнітів)
> 
> Editor/ (5 файлів)
>   ✅ UnitDefinitionEditor.cs
>   ✅ UnitIdDrawer.cs
>   ... (3 редактори)
> ```
> 
> **ЩО ЗМІНЮВАТИ**:
> 1. **Розбити UnitController**:
>    - Movement → UnitMovementController
>    - Combat → UnitCombatController
>    - Animation → UnitAnimationController
>    - State → UnitStateManager
> 
> 2. **Оптимізація**:
>    - Кешуйте шляхи для 100+ юнітів
>    - Використовуйте об'єктні пули для снарядів
> 
> ---
> 
> ### 2️⃣3️⃣ VISUALS (5 скриптів) ✅
> 
> ```
> API/
>   ✅ TileView.cs
> 
> Runtime/
>   ✅ DayNightShaderController.cs
>   ✅ VisualInstaller.cs
> 
> Editor/
>   ✅ DayNightScreenFilterNodeEditor.cs
>   ✅ DayNightScreenFilterFeature.cs
> ```
> 
> **Що змінювати**: Ніщо. Декоративна система.
> 
> ---
> 
> ### 2️⃣4️⃣ WORLDCREATION (14 скриптів) 🔧
> 
> ```
> API/ (5 файлів)
>   ✅ IWorldCreationService.cs
>   ✅ WorldSettingsDto.cs
>   ✅ MapType.cs
>   ✅ WorldSizePreset.cs
>   ✅ DifficultyLevel.cs
> 
> Runtime/ (7 файлів)
>   🔧 WorldCreationService.cs      - Потребує розбиття
>   🔧 WorldInitializer.cs
>   🔧 InitialResourceDistributor.cs
>   🔧 WorldInstaller.cs
>   ... (4+ сервіси)
> 
> Editor/
>   ✅ WorldCreationEditor.cs
>   ✅ WorldSettingsDrawer.cs
> ```
> 
> **ЩО ЗМІНЮВАТИ**:
> 1. Розбийте WorldCreationService на фази:
>    - TerrainPhase → GenerateMap
>    - BuildingPhase → PlaceTownHalls
>    - PopulationPhase → InitializeUnits
>    - ResourcePhase → DistributeResources
> 
> 2. Додайте обробку помилок на кожній фазі
> 
> ---
> 
> ## TESTS (18+ модулів, ~50-60 скриптів)
> 
> ```
> Tests/
> ├── BotAI/              ✅ Є
> ├── Calendar/           ✅ Є
> ├── Construction/       ✅ Є
> ├── ConstructionUI/     ✅ Є
> ├── Economy/            🆕 НОВІ (під час переписування)
> ├── Faction/            ✅ Є
> ├── FogOfWar/           ✅ Є
> ├── GameMode/           ✅ Є
> ├── Grid/               ✅ Є
> ├── HomeMenu/           ✅ Є
> ├── InfoPanel/          ✅ Є
> ├── MovementIntegration/✅ Є
> ├── Multiplayer/        ✅ Є
> ├── ObjectsMap/         ✅ Є
> ├── Pathfinding/        ✅ Є
> ├── SaveSystem/         ✅ Є
> ├── Signals/            ✅ Є
> ├── Units/              ✅ Є
> └── WorldCreation/      ✅ Є
> ```
> 
> **Команда запуску**:
> ```bash
> dotnet test Kruty1918.Moyva.Tests.*.csproj --no-restore --verbosity normal
> ```
> 
> ---
> 
> ## 📊 СТАТИСТИКА
> 
> | Метрика | Значення |
> |---------|----------|
> | **Всього скриптів** | 824 |
> | **Стабільних (✅)** | ~450 (55%) |
> | **Рефакторингу (🔧)** | ~200 (24%) |
> | **Активної розробки (🚀)** | ~150 (18%) |
> | **Переписування (🔴)** | 52 (6%) |
> | **Модулів** | 25 основних |
> | **Тестових модулів** | 18+ |
> 
> ---
> 
> ## 🎯 ДОРОЖНА КАРТА ЗМІН
> 
> ### Q2 2026 (Поточний квартал)
> - ✅ Переписування Economy (в процесі)
> - 🔧 Рефакторинг Construction (заплановано)
> - 🚀 Оптимізація Generator (поточна робота)
> 
> ### Q3 2026
> - 🔧 Рефакторинг HomeMenu (розбиття великих контролерів)
> - 🚀 Оптимізація Multiplayer синхронізації
> - 🔧 Рефакторинг Units системи
> 
> ### Q4 2026
> - 📊 Тестування всіх модулів
> - 🔧 Оптимізація FogOfWar
> - ✨ Додавання нових фіч
> 
> ---
> 
> **Версія**: v1.1  
> **Дата оновлення**: 14 травня 2026 р.  
> **Статус**: Активна розробка
> EOF