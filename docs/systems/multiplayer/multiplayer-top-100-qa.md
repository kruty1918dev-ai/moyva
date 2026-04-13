# Top 100 Питань По Multiplayer

Короткі відповіді для щоденної роботи: як створити сесію, підключитися, синхронізувати стан, відлагодити мережу, обробити reconnect, host migration і fallback.

1. Як швидко стартувати локальну сесію без мережі? — Використати `OfflineNetworkProvider` і запустити через `SessionManager`.
2. Як створити хост-сесію? — Викликати `CreateSession` у фасаді multiplayer з роллю хоста.
3. Як підключити клієнта до існуючої сесії? — Викликати `JoinSession(sessionCode)`.
4. Де зберігаються правила сесії? — У `SessionRules`.
5. Де зберігається загальна конфігурація multiplayer? — У `MultiplayerConfig` через `IConfigStore`.
6. Який максимальний розмір лобі за замовчуванням? — До 4 учасників (люди + боти).
7. Як збільшити ліміт гравців? — Оновити `SessionRules` і перевірити `ParticipantPolicyService`.
8. Як дізнатися, хто хост? — Через стан сесії у `SessionManager`.
9. Як перевірити, чи сесія активна? — Перевірити current session state на `Running`.
10. Як закрити сесію коректно? — Викликати `LeaveSession`/`StopSession` і закрити provider-з’єднання.
11. Що робити, якщо join повертає timeout? — Перевірити endpoint, transport і retry policy.
12. Як увімкнути повторні спроби підключення? — Через `IFailureHandlingPolicy`.
13. Як обробити тимчасову втрату мережі? — Тримати reconnect flow у `SessionManager`.
14. Де логувати network помилки? — Через `IMultiplayerLogger`.
15. Як зменшити шум у логах? — Використати рівні логування (Info/Warning/Error).
16. Як зберегти останню валідну конфігурацію? — Через `BinaryConfigStore.Save`.
17. Як відновити конфіг після рестарту гри? — Завантажити через `BinaryConfigStore.Load`.
18. Як перевірити, що provider обраний коректно? — Перевірити поле provider у `MultiplayerConfig`.
19. Як переключитися між Offline і Relay? — Змінити provider у config і перезапустити сесію.
20. Як працює bot fallback при нестачі людей? — `ParticipantFallbackService` додає ботів до цільового складу.
21. Як вимкнути автододавання ботів? — Вимкнути fallback-політику у rules/config.
22. Як заповнити лобі ботами до 4 слотів? — Увімкнути fallback + ціль `maxParticipants`.
23. Як зрозуміти, що бот був доданий автоматично? — Подія participant changed + лог fallback.
24. Як відрізнити людину від бота? — Через participant type/model flag.
25. Як перевірити унікальність participantId? — Валідувати при add/join у `ParticipantPolicyService`.
26. Що робити при дублікованому participantId? — Відхилити join і згенерувати новий id.
27. Як дізнатися поточний список учасників? — Через `SessionManager.GetParticipants()`.
28. Як перевірити готовність усіх перед стартом? — Звести ready-state і перевірити all ready.
29. Як синхронізувати ready-status? — Передавати через команду/подію стану лобі.
30. Як реалізувати kick учасника? — Хост відправляє команду removal і оновлює стан.
31. Як реалізувати lock/unlock лобі? — Прапорець join policy у session settings.
32. Як передати початковий стан світу клієнту? — Через `WorldSnapshot`.
33. Де зберігати snapshot-и? — Через `IWorldSnapshotStore`.
34. Як виконати snapshot перед стартом матчу? — `CaptureSnapshot` у consistency layer.
35. Як перевірити валідність snapshot? — Прогнати checksum/shape validation.
36. Як застосувати snapshot на клієнті? — `ApplySnapshot` до локального state bridge.
37. Як уникнути десинху після apply snapshot? — Підтвердити версію/тик і запустити ресинк.
38. Як працює versioning стану гри? — Через revision/tick у payload.
39. Що робити при out-of-order пакетах? — Буферизувати й застосовувати за sequence.
40. Як обробити duplicate packet? — Ігнорувати за packet id або sequence.
41. Як організувати командну синхронізацію? — Через `game-sync` pipeline команд.
42. Як додати новий тип команди? — Додати model + serializer + dispatch handler.
43. Де реєструється dispatch команд? — У синхронізаційному сервісі/router.
44. Як гарантувати idempotency команд? — Застосовувати dedupe ключі.
45. Як зменшити частоту передачі команд? — Батчинг або tick-based flush.
46. Як синхронізувати game-state без перевантаження? — Delta updates замість повних snapshot.
47. Коли відправляти повний snapshot? — На join/reconnect або при великому розсинхроні.
48. Як виявити розсинхрон між хостом і клієнтом? — Порівняння checksums/state hashes.
49. Що робити при checksum mismatch? — Запустити примусовий resync.
50. Як мінімізувати latency вплив на UX? — Локальний prediction + авторитарне підтвердження.
51. Як уникнути стрибків стану після підтвердження? — Reconciliation з плавним коригуванням.
52. Як працювати з RTT у логіці відправки? — Адаптивні таймаути і retry windows.
53. Як виміряти packet loss? — Метрики delivery/ack у transport layer.
54. Як увімкнути debug-режим синхронізації? — Debug flags у multiplayer config.
55. Де дивитися lifecycle сесії? — Логи `SessionManager` + події state changes.
56. Як протестувати create/join сценарій швидко? — Unit/integration тести wrapper-рівня.
57. Як протестувати reconnect? — Емуляція network drop і автоматичний rejoin.
58. Як протестувати host migration? — Передача host role і повторна ініціалізація provider.
59. Коли запускати host migration? — При виході/падінні поточного хоста.
60. Як обрати нового хоста? — За deterministic policy (rank/order/latency).
61. Як передати authority новому хосту? — Snapshot + epoch/version bump.
62. Що робити, якщо migration провалився? — Fallback на нову сесію + rejoin flow.
63. Як синхронізувати налаштування матчу між усіма? — Через config sync packet.
64. Як зберігати сталі seed-и генерації світу? — У session config/snapshot metadata.
65. Як забезпечити детермінізм симуляції? — Однакові rules, seed, tick order.
66. Як узгодити календар/таймери між клієнтами? — Авторитарний server time/tick.
67. Як інтегрувати save system з multiplayer? — Snapshot bridge у save/load pipeline.
68. Як відновити матч після reconnection? — Завантажити останній snapshot і догнати тики.
69. Як не дублювати застосування команд після reconnect? — Replay з watermark sequence.
70. Як обробити часткову втрату snapshot store? — Відкат до останнього валідного checkpoint.
71. Як виявляти несумісну версію клієнта? — Version handshake при join.
72. Що робити при version mismatch? — Відхилити join з поясненням версії.
73. Як узгодити протокол мережевих пакетів? — Contract tests для payload schema.
74. Як серіалізувати команди без зайвих алокацій? — Пул буферів і компактні DTO.
75. Як захиститися від invalid payload? — Валідація схеми перед apply.
76. Що робити при corrupt config file? — Safe defaults + warning + backup restore.
77. Як налаштувати graceful shutdown сесії? — Broadcast stop reason + clean disconnect.
78. Як відслідкувати причину disconnect? — Коди помилок transport/provider.
79. Як додати новий network provider? — Реалізувати `INetworkProvider` контракт.
80. Що перевірити перед інтеграцією provider-а? — Connect/send/receive/disconnect + error map.
81. Як ізолювати transport від домену гри? — Тримати adapter layer між API і provider.
82. Як не тягнути UnityEditor у runtime multiplayer? — Editor-код лише в editor asmdef.
83. Як організувати DI для multiplayer сервісів? — Інсталер з інтерфейсними binding-ами.
84. Як перевірити, що всі залежності інжектяться? — Startup smoke test + null guards.
85. Як гарантувати thread-safety при callback-ах мережі? — Серіалізація подій у main thread queue.
86. Як обмежити розмір черги вхідних команд? — Backpressure + drop policy.
87. Як уникнути memory leak у підписках? — Явне unsubscribe на stop/dispose.
88. Як відлагодити race condition у join flow? — Трасування state machine переходів.
89. Як документувати новий multiplayer сценарій? — Додати сторінку в `docs/systems/multiplayer` і `pages.json`.
90. Як швидко знайти архітектуру multiplayer? — `architecture.md`.
91. Де дивитися доменні моделі? — `domain-models.md`.
92. Де дивитися інтерфейси? — `interfaces.md`.
93. Де дивитися сценарії тестування? — `testing.md`.
94. Де дивитися хост міграцію? — `host-migration.md`.
95. Де дивитися синхронізацію команд? — `game-sync.md`.
96. Де дивитися ігровий цикл у мережі? — `game-state.md`.
97. Де дивитися налаштування хабу? — `config-hub-guide.md`.
98. Який мінімальний шлях для прототипу multiplayer? — `quickstart.md` + Offline provider.
99. Який рекомендований перший інваріант для тестів? — Create/Join/Sync/Leave без exception.
100. Який найшвидший спосіб інтегрувати модуль з multiplayer? — Працювати через публічні API/події `SessionManager` і не залежати від конкретного provider.
