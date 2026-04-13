# Multiplayer — Архітектура

← [Назад до огляду](README.md)

---

## Принципи проєктування

Multiplayer-система побудована на таких принципах:

| Принцип | Як реалізований |
|---|---|
| **SOLID** | Кожен клас залежить від інтерфейсу, а не від конкретної реалізації |
| **TDD** | Спочатку написані тести, потім реалізація |
| **Dependency Injection** | Усі залежності передаються через конструктор |
| **Carcass** | Лише архітектурний каркас — без ігрової логіки |
| **Editor isolation** | Runtime-код не залежить від `UnityEditor` |

---

## Шари системи

```
┌─────────────────────────────────────────────────────┐
│  Editor Layer                                       │
│  MultiplayerConfigEditorWindow                      │
│  (Kruty1918.Moyva.Multiplayer.Editor)               │
└─────────────────────────────────────────────────────┘
              │ залежить від
┌─────────────────────────────────────────────────────┐
│  Runtime Layer                                      │
│  SessionManager                                     │
│  ParticipantPolicyService                           │
│  WorldConsistencyService                            │
│  OfflineNetworkProvider                             │
│  RelayNetworkProvider                               │
│  WebSocketNetworkProvider                           │
│  FallbackNetworkProvider                            │
│  NetworkProviderFactory                             │
│  BinaryConfigStore                                  │
│  UnityMultiplayerLogger                             │
│  SimpleFailureHandlingPolicy                        │
│  (Kruty1918.Moyva.Multiplayer — Runtime/ folder)    │
└─────────────────────────────────────────────────────┘
              │ залежить від
┌─────────────────────────────────────────────────────┐
│  API Layer (контракти)                              │
│  Інтерфейси: INetworkProvider, ISessionManager,    │
│  IParticipantPolicyService, IWorldConsistencyService│
│  Моделі: SessionRules, MultiplayerConfig,          │
│  ParticipantIdentity, WorldSnapshot, ...           │
│  Enum-и: SessionMode, NetworkProviderType, ...     │
│  (Kruty1918.Moyva.Multiplayer — API/ folder)        │
└─────────────────────────────────────────────────────┘
```

---

## Assembly Definitions

### `Kruty1918.Moyva.Multiplayer`
- **Файл:** `Assets/Moyva/Scripts/Features/Multiplayer/Kruty1918.Moyva.Multiplayer.asmdef`
- **Тип:** Runtime
- **Залежності:** немає (autoReferenced: true)
- **Містить:** API/ і Runtime/ папки

### `Kruty1918.Moyva.Multiplayer.Editor`
- **Файл:** `Assets/Moyva/Scripts/Features/Multiplayer/Editor/Kruty1918.Moyva.Multiplayer.Editor.asmdef`
- **Тип:** Editor-only (`includePlatforms: ["Editor"]`)
- **Залежності:** `Kruty1918.Moyva.Multiplayer`
- **Містить:** `MultiplayerConfigEditorWindow.cs`

### `Kruty1918.Moyva.Tests.Multiplayer`
- **Файл:** `Assets/Moyva/Scripts/Tests/Multiplayer/Kruty1918.Moyva.Tests.Multiplayer.asmdef`
- **Тип:** Test-only (`defineConstraints: ["UNITY_INCLUDE_TESTS"]`)
- **Залежності:** `Kruty1918.Moyva.Multiplayer`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`
- **Містить:** усі unit-тести

---

## Структура папок

```
Assets/Moyva/Scripts/Features/Multiplayer/
├── Kruty1918.Moyva.Multiplayer.asmdef
├── API/
│   ├── ConsistencyCheckResult.cs     (enum)
│   ├── FailureCategory.cs            (enum)
│   ├── IConfigStore.cs               (interface)
│   ├── IConfigSyncService.cs         (interface)
│   ├── IFailureHandlingPolicy.cs     (interface)
│   ├── IHostMigrationService.cs      (interface)
│   ├── IMultiplayerLogger.cs         (interface)
│   ├── INetworkProvider.cs           (interface + DTOs)
│   ├── IParticipantFallbackService.cs(interface)
│   ├── IParticipantPolicyService.cs  (interface)
│   ├── ISessionManager.cs            (interface)
│   ├── IWorldCloneService.cs         (interface + SlotMapping)
│   ├── IWorldConsistencyService.cs   (interface)
│   ├── IWorldSnapshotStore.cs        (interface)
│   ├── MultiplayerConfig.cs          (sealed immutable)
│   ├── NetworkProviderType.cs        (enum)
│   ├── Participant.cs                (sealed)
│   ├── ParticipantIdentity.cs        (sealed + BotIdPrefix)
│   ├── ParticipantSlot.cs            (sealed)
│   ├── SessionConnectOptions.cs      (sealed DTO)
│   ├── SessionMode.cs                (enum)
│   ├── SessionRules.cs               (sealed immutable)
│   └── WorldSnapshot.cs              (sealed immutable)
├── Runtime/
│   ├── AssemblyInfo.cs               (InternalsVisibleTo tests)
│   ├── BinaryConfigStore.cs          (IConfigStore impl)
│   ├── OfflineNetworkProvider.cs     (INetworkProvider impl)
│   ├── ParticipantPolicyService.cs   (IParticipantPolicyService impl)
│   ├── SessionManager.cs             (ISessionManager impl)
│   ├── SimpleFailureHandlingPolicy.cs(IFailureHandlingPolicy impl)
│   ├── UnityMultiplayerLogger.cs     (IMultiplayerLogger impl)
│   └── WorldConsistencyService.cs    (IWorldConsistencyService impl)
└── Editor/
    ├── Kruty1918.Moyva.Multiplayer.Editor.asmdef
    └── MultiplayerConfigEditorWindow.cs

Assets/Moyva/Scripts/Tests/Multiplayer/
├── Kruty1918.Moyva.Tests.Multiplayer.asmdef
├── ParticipantPolicyServiceTests.cs
├── SessionManagerTests.cs
├── SessionRulesTests.cs
└── WorldConsistencyServiceTests.cs
```

---

## Граф залежностей

```
MultiplayerConfigEditorWindow
    └── BinaryConfigStore
    └── MultiplayerConfig
    └── SessionRules

SessionManager
    ├── INetworkProvider
    │       └── OfflineNetworkProvider (або RelayProvider, MirrorProvider)
    ├── IParticipantPolicyService
    │       └── ParticipantPolicyService
    │               └── IWorldSnapshotStore
    ├── IWorldConsistencyService
    │       └── WorldConsistencyService
    ├── IWorldSnapshotStore
    ├── IConfigStore
    │       └── BinaryConfigStore
    ├── IMultiplayerLogger
    │       └── UnityMultiplayerLogger
    └── IFailureHandlingPolicy
            └── SimpleFailureHandlingPolicy
```

---

## Потік сесії

```
                  ┌──────────┐
                  │  Config  │  load MultiplayerConfig
                  └────┬─────┘
                       │
              ┌────────▼─────────┐
              │  SessionManager  │  CreateOrJoinSessionAsync()
              └────────┬─────────┘
                       │
          ┌────────────▼─────────────┐
          │  Config consistency check │  ComputeConfigChecksum
          └────────────┬─────────────┘
                       │ ok
          ┌────────────▼─────────────┐
          │  Participant policy check │  CanJoin()
          └────────────┬─────────────┘
                       │ ok
          ┌────────────▼─────────────┐
          │  INetworkProvider        │  HostSessionAsync / JoinSessionAsync
          └────────────┬─────────────┘
                       │ ok
          ┌────────────▼─────────────┐
          │  Add Participant, log     │
          └──────────────────────────┘
```

---

## Заплановані майбутні розширення

- `RelayNetworkProvider` — Unity Relay backend ✅ (потребує MOYVA_UGS_RELAY define + UGS SDK)
- `WebSocketNetworkProvider` — WebSocket layer
- Конкретна реалізація `IHostMigrationService` (вибір нового хоста)
- Конкретна реалізація `IParticipantFallbackService` (замінити на бота)
- Конкретна реалізація `IConfigSyncService` (синхронізація конфігу між хостом і клієнтом)
- Locked participant set у `WorldSnapshot` для strict 4-player world lock
