# Top 100 Питань По Economy

Короткі відповіді для щоденної роботи: як щось додати, змінити, отримати, перевірити або відлагодити в економічній системі.

1. Як швидко отримати суму ресурсів гравця? — Через `IEconomyRuntimeApi.GetOwnerResourceTotals(ownerId)`.
2. Як отримати лише Food/Materials для HUD? — Через `GetFormattedOwnerCategoryTotals(ownerId)`.
3. Як отримати дані одного поселення? — Через `GetSettlementResourceTotals(settlementId)`.
4. Як отримати settlementId по клітинці? — Через `IEconomyInfoMediator.TryGetSettlementContext(position, out context)`.
5. Як отримати buildingId по клітинці? — Через `IEconomyInfoMediator.TryGetBuildingContext(position, out buildingId, out ownerId)`.
6. Де точка входу в economy-дані? — `EconomyDatabaseSO`.
7. Де зберігаються правила economy runtime? — У `EconomyRulesConfigSO`.
8. Де підключається Economy в сцені? — Через `EconomyInstaller`.
9. Що робити, якщо Economy не стартує? — Перевірити, що в `EconomyInstaller` призначено `EconomyDatabaseSO`.
10. Що робити, якщо немає RulesConfig? — Призначити `RulesConfig` у `EconomyDatabaseSO`.
11. Як створюється поселення? — При `BuildingPlacedSignal` для TownHall.
12. Як видаляється або деактивується поселення? — При знесенні TownHall або при 0 населення.
13. Як запустити перевірку конфігів? — Через Economy Hub Validation.
14. Де задається формат тексту для HUD? — У `EconomyRulesConfiguration` параметрами UI summary.
15. Як змінити формат Food рядка? — Оновити `ui-summary-food-format`.
16. Як змінити формат Materials рядка? — Оновити `ui-summary-materials-format`.
17. Що поверне API, якщо settlementId не знайдено? — Порожні/нульові агрегати.
18. Як прив'язати owner для UI автоматично? — Через `IConstructionService.GetActiveOwner()`.
19. Що якщо owner порожній? — Використовується `player_0`.
20. Як перевірити, що тік економіки відбувається? — Підписатися на `EconomyTickCompletedSignal`.
21. Як дізнатися про дефіцит ресурсу? — Через `ResourceDeficitSignal`.
22. Як відреагувати на створення поселення? — Підписатися на `SettlementCreatedSignal`.
23. Як відреагувати на деактивацію поселення? — Підписатися на `SettlementDeactivatedSignal`.
24. Як оновлювати UI ресурсів у реальному часі? — Слухати economy сигнали і перераховувати через API.
25. Де задається ліміт поселень? — У rules: `Settlement.MaxSettlements`.
26. Як змінити швидкість приросту населення? — У rules групи Population.
27. Як змінити виробничі коефіцієнти? — У rules групи Production.
28. Як змінити споживання по віку? — У rules групи Consumption.
29. Як змінити mortality-параметри? — У rules групи Mortality.
30. Як налаштувати ринок і ціни? — У rules групи Market.
31. Що робити, якщо ресурси не сумуються по owner? — Перевірити OwnerId у settlement state.
32. Як перевірити owner-scoped агрегацію? — Викликати `GetOwnerResourceTotals(ownerId)` і порівняти з settlement сумами.
33. Як дізнатися список поселень гравця? — `GetSettlementIdsForOwner(ownerId)`.
34. Як дізнатися ресурси складу? — `GetWarehouseResourceTotals(warehousePosition)`.
35. Як дізнатися суму по складах поселення? — `GetSettlementWarehousesTotal(settlementId)`.
36. Як уникнути null-референсів у UI? — Перевірити DI-inject та null-check перед викликами API.
37. Як інтегрувати economy з інфо-панеллю? — Використати `IEconomyInfoMediator`.
38. Як інтегрувати economy з ботом? — Використати `IEconomyRuntimeApi` для owner агрегатів.
39. Як інтегрувати economy з мультиплеєром? — Передавати owner-scoped totals або settlement payloads.
40. Як перевірити консистентність resourceId? — Validation в Economy Hub.
41. Що робити при дублях resourceId? — Унікалізувати id в базі ресурсів.
42. Як додати новий ресурс? — Через Economy Hub або створення `EconomyResourceDefinition`.
43. Як додати новий production profile? — Додати `EconomyProductionProfile` і прив'язати в базі.
44. Як додати нову політику складу? — Додати `EconomyWarehousePolicy`.
45. Як додати новий тип поселення? — Додати `EconomySettlementDefinition`.
46. Як додати караванний шаблон? — Додати `EconomyCaravanTemplate`.
47. Як додати AI-профіль економіки? — Додати `EconomyAiRuleProfile`.
48. Як швидко знайти всі economy assets? — Через `EconomyDatabaseSO` списки.
49. Що робити, якщо API повертає 0 по всіх ресурсах? — Перевірити, чи є активні поселення у потрібного owner.
50. Що робити, якщо settlement є, але ресурсів немає? — Перевірити production/consumption tick та дефіцити.
51. Як відслідкувати будівлю у поселенні? — Через `buildingId` і контекст `TryGetBuildingContext`.
52. Як дізнатися ім'я поселення для UI? — Через `EconomySettlementContext.SettlementName`.
53. Як перевірити, що TownHall правильно створює settlement? — Тестом на `BuildingPlacedSignal` + `SettlementCreatedSignal`.
54. Як перевірити, що знесення TownHall деактивує settlement? — Тестом на `BuildingDemolishedSignal` + `SettlementDeactivatedSignal`.
55. Як уникнути хардкоду коефіцієнтів? — Виносити значення в rules assets.
56. Як зробити зміну балансу без коду? — Редагувати RulesConfig у Economy Hub.
57. Як додати новий параметр rules? — Додати поле в rules class і відобразити у Hub.
58. Як контролювати версію схеми economy? — Через `EconomySchema` і migration service.
59. Що робити при зміні структури assets? — Додати migration step.
60. Як перевірити міграцію? — Прогнати migration + validation + regression tests.
61. Як зберегти стабільний формат UI? — Тримати шаблони в `EconomyRulesConfiguration`.
62. Як локалізувати тексти HUD? — Використати локалізовані format-шаблони.
63. Чи можна викликати API щокадру? — Можна, але краще кешувати і оновлювати по сигналам.
64. Як оптимізувати оновлення HUD? — Оновлювати тільки на economy сигнали.
65. Як уникнути зайвих алокацій словників? — Кешувати результати або використовувати переюз контейнерів.
66. Як відлагодити owner mismatch? — Логувати owner у construction і economy подіях.
67. Як перевірити що owner нормалізується? — Передавати null/empty і перевірити fallback `player_0`.
68. Як дізнатися, що склад створився? — Перевірити warehouse pool у settlement state.
69. Як дізнатися, що склад видалився? — Перевірити видалення warehouse key після demolish.
70. Як дізнатися виробничі цикли за тік? — З `EconomyTickCompletedSignal.ProductionCyclesCompleted`.
71. Як дізнатися прибуття/смерті за тік? — З `EconomyTickCompletedSignal.Arrivals/Deaths`.
72. Як дізнатися населення поселення? — Через сигнал тіку або state у manager.
73. Як додати новий UI віджет economy? — Бінд `IEconomyRuntimeApi`, підписка на сигнали, ререндер.
74. Як дізнатися ресурси owner у tooltip? — Взяти `GetOwnerResourceTotals(ownerId)`.
75. Як дізнатися ресурси конкретного тайла? — Спочатку контекст через mediator, потім totals.
76. Як перевірити коректність категоризації ресурсів? — Звірити `EconomyResourceDefinition.Category`.
77. Чому Food/Materials не змінюються? — Перевірити, чи ресурс має правильну категорію.
78. Як додати нову категорію ресурсу? — Розширити enum і місця агрегації категорій.
79. Як тестувати economy runtime швидко? — EditMode unit tests для сервісів і API.
80. Як тестувати інтеграцію з Construction? — Тестами на сигнали BuildingPlaced/Demolished.
81. Як тестувати інтеграцію з Calendar? — Тестом на виклик тіку при зміні години.
82. Як тестувати UI контролер? — Фейкові API/SignalBus і перевірка текстових полів.
83. Як перевірити правильність формату чисел? — Тест `GetFormattedOwnerCategoryTotals`.
84. Як відловити проблеми з правилами до запуску гри? — Validation у Hub перед play.
85. Як виправити типові проблеми правил швидко? — Auto-fix у Hub.
86. Як отримати дані для експорту аналітики? — Брати owner/settlement totals через API.
87. Як передати economy стан у save систему? — Серіалізувати settlement/resource pools і rules version.
88. Як відновити economy стан після load? — Відновити settlements і ресурси до старту тіку.
89. Як уникнути десинху між save і rules? — Зберігати schema/rules version у сейві.
90. Як працювати з unknown resourceId? — Логувати як warning і повертати category None.
91. Як додати нову бізнес-формулу? — Окремий runtime service + параметри rules + тести.
92. Як вбудувати economy в InfoPanel модуль? — Через `IEconomyInfoMediator` контракт.
93. Як вбудувати economy в AI модуль? — Через `IEconomyRuntimeApi` owner/settlement totals.
94. Як швидко знайти потрібну документацію? — Почати з `docs/systems/economy/README.md`.
95. Де дивитися перелік API-файлів? — `economy-api-files-reference.md`.
96. Де дивитися повну архітектуру? — `economy-complete-guide.md`.
97. Де дивитися runtime-план? — `economy-runtime-logic-plan.md`.
98. Де дивитися практичні кроки налаштування? — `economy-designer.md` і `tutorials.md`.
99. Як додати нову будівлю, щоб економіка її бачила? — Через Registry/Economy Hub з валідним `buildingId/resourceId`.
100. Який найшвидший шлях щось отримати з economy? — `IEconomyRuntimeApi` для агрегатів і `IEconomyInfoMediator` для контексту по позиції.
