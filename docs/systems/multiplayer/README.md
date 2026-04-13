# Multiplayer — Огляд системи

← [Назад до README](../../README.md)

---

## Що це таке

**Multiplayer** — архітектурний каркас (carcass) для мережевої гри у проєкті Moyva.

Система підтримує 4 режими гри:

| Режим | Опис |
|---|---|
| `PeacefulSolo` | Одиночна гра без ботів і без інших гравців |
| `SoloWithBots` | Одиночна гра з ботами |
| `MultiplayerHumans` | Мережева гра — лише люди |
| `MixedHumansAndBots` | Мережева гра з людьми і ботами |

Максимум 4 учасники (люди + боти), але ця кількість налаштовується.

---

## Де знаходиться код

```
Assets/Moyva/Scripts/Features/Multiplayer/
  Kruty1918.Moyva.Multiplayer.asmdef   ← runtime-збірка
  API/                                  ← публічні контракти, моделі, enum-и
  Runtime/                              ← конкретні реалізації
  Editor/                               ← інструмент налаштування (лише Editor)

Assets/Moyva/Scripts/Tests/Multiplayer/
  Kruty1918.Moyva.Tests.Multiplayer.asmdef
  *.Tests.cs                            ← unit-тести (NUnit)
```

---

## Namespaces

| Namespace | Призначення |
|---|---|
| `Kruty1918.Moyva.Multiplayer.Core` | Доменні моделі, сесії, учасники, consistency |
| `Kruty1918.Moyva.Multiplayer.Config` | Конфігурація, SessionRules, MultiplayerConfig |
| `Kruty1918.Moyva.Multiplayer.Networking` | INetworkProvider та його реалізації |
| `Kruty1918.Moyva.Multiplayer.Persistence` | WorldSnapshot, IWorldSnapshotStore |
| `Kruty1918.Moyva.Multiplayer.Runtime` | Конкретні сервіси (не API) |
| `Kruty1918.Moyva.Multiplayer.Editor` | EditorWindow (лише в Editor) |

---

## Ключові принципи

- **Хост-авторитарність** — хост тягне основну симуляцію, клієнти синхронізуються.
- **Provider-agnostic networking** — система не залежить від конкретного транспорту.
- **Offline режим** — та сама API, що і в multiplayer, але без мережі.
- **Незмінні моделі** — `SessionRules`, `MultiplayerConfig`, `WorldSnapshot` — sealed immutable.
- **SOLID/TDD** — кожен клас залежить від абстракцій; є unit-тести для кожного сервісу.
- **Відсутність UnityEditor у runtime** — Editor-інструменти у власній збірці.

---

## Документація

| Файл | Зміст |
|---|---|
| [architecture.md](architecture.md) | Архітектура, залежності, assembly definitions |
| [multiplayer-top-100-qa.md](multiplayer-top-100-qa.md) | Top 100 питань по Multiplayer |
| [domain-models.md](domain-models.md) | Усі доменні моделі та enum-и |
| [interfaces.md](interfaces.md) | Довідник усіх інтерфейсів |
| [session-manager.md](session-manager.md) | SessionManager — детальний опис |
| [network-providers.md](network-providers.md) | INetworkProvider, OfflineNetworkProvider |
| [config-store.md](config-store.md) | MultiplayerConfig та BinaryConfigStore |
| [participant-policy.md](participant-policy.md) | ParticipantPolicyService, правило 4 гравців |
| [world-consistency.md](world-consistency.md) | WorldConsistencyService, snapshots |
| [logging-and-errors.md](logging-and-errors.md) | Логування та обробка помилок |
| [config-hub-guide.md](config-hub-guide.md) | **Multiplayer Config Hub** — повний гайд |
| [testing.md](testing.md) | TDD, unit-тести |
| [quickstart.md](quickstart.md) | Швидкий старт |
| [host-migration.md](host-migration.md) | Міграція хоста |
| [game-state.md](game-state.md) | Стан гри (мережева синхронізація) |
| [game-sync.md](game-sync.md) | Синхронізація ігрових команд |

---

## Схема взаємодії компонентів

```
┌───────────────────────────────────────────────────────┐
│                    SessionManager                     │
│                                                       │
│  ┌──────────────┐  ┌───────────────────────────────┐ │
│  │INetworkProvider│  │ IParticipantPolicyService   │ │
│  │ (Offline/Relay │  │ IWorldConsistencyService    │ │
│  │  /Mirror)     │  │ IWorldSnapshotStore          │ │
│  └──────────────┘  │ IConfigStore                 │ │
│                    │ IMultiplayerLogger            │ │
│                    │ IFailureHandlingPolicy        │ │
│                    └───────────────────────────────┘ │
└───────────────────────────────────────────────────────┘
         │
         ▼
  MultiplayerConfig (binary file)
  WorldSnapshot (existing save system bridge)
```

---

## Швидкий старт

→ Дивись [quickstart.md](quickstart.md)

---

## Редактор конфігурації

→ Дивись [config-hub-guide.md](config-hub-guide.md)

Меню Unity: `Moyva → Multiplayer → Config Hub`
