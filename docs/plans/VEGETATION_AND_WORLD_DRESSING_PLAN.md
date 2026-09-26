# Vegetation & World Dressing Plan

Ціль: зеленіший, різноманітніший, візуально насиченіший світ MOYVA —
об'ємна трава, кущі, підлісок, дрібні природні деталі й варіативність
дерев, інтегровані з чинною генерацією (TWC, біоми, чанки, seed).

## 1. Інвентаризація (фактичний стан проєкту)

### Pipeline (готовий до розширення)

- `EnvironmentDecorationGenerator` — детермінований per-cell генератор:
  густина з шуму + кластерне згладжування, біом-множники, shoreline/
  footprint/exclusion правила, поверхневе заземлення.
  `Assets/Moyva/Scripts/Features/Generator/Runtime/EnvironmentDecorationGenerator.cs`
- `EnvironmentDecorationSpawner` — інстанціює під `MapChunk_X_Y/
  EnvironmentDecorations`; заземлення через `IGeneratorTerrainLevelService`
  (surface height + normal) і `EnvironmentObjectPlacementResolver`.
- Конфіг JSON `environment-decoration-config.json` → `JsonConfigRuntime`;
  сувора схема `environment-decoration.schema.json`.
- Реєстр `mapobjectregistry.json` (83 definіції) → `MapObjectRegistrySO`
  на `GeneratorInstaller`; `visualPrefab.$asset` резолвиться через
  `MoyvaRuntimeAssetCatalog.prefab` (key → GUID).
- Gameplay-об'єкти (POI `resource-lumber`/`resource-stone`, русла річок)
  живуть у `worldData.ObjectMap`, спавняться `ChunkFirstObjectSpawner`,
  зберігаються сейвом. Декорації — окремий візуальний шар, сейв не чіпає.

### Наявні ресурси

| Актив | Стан | Використання |
|---|---|---|
| KayKit trees A/B (single/small/medium/large ×2 набори) | у pool `tree` | дорослі/великі дерева |
| KayKit stumps (×4) | у pool `stump` | лісові пні |
| KayKit rocks A–E | у pool `rock` | каміння; і дрібний пісочник (scaled) |
| KayKit waterplants/lilies (×5) | у pool `waterplant` | водяна флора |
| `Grass_bilboard/kreuz` mesh+mat+prefab (2 варіанти) | у pool `grass` | єдині трави — замало |
| `bush_001..004.png` (RGBA картки), `bush_005.png` (широка лінія) | **не використовуються** | → кущі-картки |
| `signal-2026-*.png` (туфта) | не використовується | кандидат у tall-grass |
| `green-tree-object-*`, `green-forest-*`, `tree-oak`, `tree-pine`, palms | у реєстрі, поза pools | harvestable/логічні об'єкти — НЕ дублювати в декораціях |
| `DecorSharedStylized` shader | чинний для grass cards | alpha-clip, billboard, volume, outline, `multi_compile_instancing`; вітру нема |
| FlatKit Valley textures (grass2d-01..06, branches, bark) | demo assets | запасні текстури для litter/карток |

### Чого бракує (створюємо)

- Кущі з власних карток `bush_001..005` (зараз pool `bush` = small trees).
- Папороті/широколисті (процедурні low-poly меші + палітра).
- Квіти (pool порожній, density=0; процедурні low-poly куртини).
- Ground litter: гілки, листяні плями, мох, колоди.
- Молоді дерева (saplings): тонкий стовбур + мала крона, не scaled-копії.
- Очерет/сedge для берегів: тонкі високі леза (процедурний меш).
- Трав'яні куртини: ≥3 нові форми мешів × 2 чинні текстури → 5–6 pool id.
- Палітра-текстура `VegPalette.png` (UV→кольори) + спільний
  `VegFlat` матеріал (opaque, instancing ON) для процедурних мешів.

### Підтверджені дефекти поточної конфігурації

- `flowerDensity: 0` без pool; `bush` pool = small tree prefabs.
- `maxObjectsPerTile: 3` — total cap; трава програє деревам у вибірці.
- Генератор **не перевіряє `ObjectMap`** — декоративні дерева можуть
  перекривати harvestable POI. Потрібен per-type `skipObjectCells`.
- Матеріали мають `m_EnableInstancingVariants: 0` — без GPU instancing.

## 2. Модель розміщення: шарові правила (additive layers)

Залишаємо legacy-шлях (`typeDensities` weighted-exclusive, max 3/клітинку)
недоторканим для tree/stump/rock і додаємо **незалежні шари**:

```jsonc
"layers": [{
  "type": "grass",              // ключ assetPools
  "weight": 0.55,               // базова ймовірність спавну на клітинку
  "maxPerTile": 3,              // кілька екземплярів = об'єм
  "biomeBoost": {"forest":0.8,"rocky":0.5},   // множник поверх biomeRules
  "waterAffinity": "prefer",    // any|avoid|prefer
  "waterRadius": 2, "waterBoost": 1.8,
  "forestAffinity": "edge",     // any|interior|edge|avoid
  "nearTreeBoost": 0.75,        // додаткова вага біля деревних anchor-клітинок
  "nearTreeRadius": 1,
  "skipObjectCells": true,      // не ставити на клітинках з ObjectMap id
  "shorelineExclusion": false,  // heavy-правило: не ближче ніж shorelineExclusionCells до води
  "validateFootprint": true,    // heavy-footprint resolver
  "maxSlopeMeters": 0.7,        // 0 = без обмежень
  "minScale": 0.8, "maxScale": 1.3,   // 0 → наслідувати visualVariation
  "yOffset": 0.0
}]
```

Семантика:

- Кожна клітинка × кожен шар оцінюється незалежно (детерміновані хеші
  `seed+x+y+index`), що дозволяє grass під деревами, litter під кронами.
- Anchor-клітинки дерев (legacy tree/stump + шари з `validateFootprint`)
  збираються в першому проході → `nearTreeBoost` не залежить від порядку
  чанків чи шарів.
- `weight:0` + `nearTreeBoost` = "тільки під деревами".
- `forestAffinity`: `interior` = forest tile з усіма сусідами-forest у
  радіусі; `edge` = forest tile з ≥1 non-forest сусідом у радіусі;
  `avoid` = не forest.
- `waterAffinity: prefer` множить вагу на `waterBoost`, коли вода в межах
  `waterRadius`; `avoid` відсікає клітинки біля води.
- Per-layer noise map (`seed + layerIndex*7919`) дає плямистість,
  некорельовану між шарами.
- `skipObjectCells` default true — декорації не перекривають POI/русла.

### Початковий набір шарів

| Шар | Пул | Розміщення |
|---|---|---|
| `grass` | 2 чинні + 4 нових card-префаби | скрізь на лузі, max 3/tile, ліс ×0.8 |
| `tallgrass` | tall-варіанти + sedge | prefer water r2 ×1.8; окремий запис edge ×1.5 |
| `bush` | 5 нових карток | grassland/edge, групи (cluster noise), footprint |
| `fern` | 2 процедурні | forest interior, prefer water r2 |
| `litter` | twig/leaf/moss | weight 0.02 + nearTreeBoost 0.8 |
| `flower` | 2 процедурні | grassland ×1.5, weight 0.05, рідкі плями |
| `pebble` | kaykit rocks @ scale 0.25–0.45 | rocky ×2.5 |
| `log` | 1–2 процедурні | forest interior 0.05, footprint, shorelineExcl |
| `sapling` | 2–3 процедурні | forest edge 0.15, footprint, shorelineExcl, skipObjectCells |
| `reed` | 1–2 процедурні blades | prefer water r1 ×3, weight 0.1 |

## 3. Нові активи — editor-генератор

`Assets/Moyva/Scripts/Features/Generator/Editor/MoyvaVegetationAssetBuilder.cs`
(`MenuItem "Moyva/Vegetation/Rebuild Generated Assets"` + `-executeMethod`)
створює під `Assets/Moyva/Generated/Vegetation/`:

- `VegPalette.png` — палітра (leaf×3, trunk, twig, moss, litter, flower×3,
  stone, reed) → UV-маплені меші ділять один `VegFlat` матеріал.
- Meshes: grass-tri/tall/clump cards; sedge blades; fern ×2; flower patch;
  twig ×2; leafpatch ×2; moss; log; sapling ×2; pebble reuse kaykit.
- Materials: `VegFlat` (palette, opaque, instancing ON), card-мats
  bush_001..005 + повторне використання grass mats.
- Prefabs (GameObject+MeshFilter+MeshRenderer, pivot у базі, без colliders,
  прийнятні для `EnvironmentDecorationSpawner`).
- Реєстр: нові `definitions` у `mapobjectregistry.json` (`veg-*` id) +
  ключі в `MoyvaRuntimeAssetCatalog.prefab` (sorted insert).
- Pool-оновлення в `environment-decoration-config.json`.

Масштаб орієнтир (cellSize=1 m): трава 0.2–0.45 m, кущі 0.5–0.9 m,
папороть 0.35–0.55 m, sapling 0.8–1.4 m, log 0.6–1.0 m, litter ≤0.25 m.

## 4. Детермінізм, чанки, збереження

- Шари сіються від `worldData.Seed + SeedOffset` (+ per-layer salt) —
  незалежно від порядку чанків.
- Спавн під чанковими коренями — lifecycle успадковується; кластери,
  що перетинають межі чанків, узгоджені, бо розподіл рахується у
  світових координатах.
- `skipObjectCells` тримає декор геть від `resource-*` POI; зрубані
  дерева зберігаються сейвом незалежно — декорації їх не дублюють.
- Water/lake/river cells: land-шари ніколи не ставлять на воду
  (існуючий water/sheet guard); `reed` живе на сухих сусідніх клітинках.

## 5. Етапи (порядок користувача)

1. **Grass**: mesh-варіанти + palette + instancing + grass/tallgrass шари.
2. **Bushes**: картки bush_001..005 → pool `bush` + шар.
3. **Tree variation**: sapling pool + edge/inс interior правила + ширший
   scale-range для tree layer; (tint-варіанти — опціонально, через клони
   prefab з матеріалом-копією; відкладено як stretch-goal).
4. **Undergrowth/forest details**: fern, litter (twig/leaf/moss), log,
   nearTree affinity.
5. **Accents/rendering**: flower, pebble, reed; instancing-перевірка,
   perf-заміри, polish density.

## 6. Приймальні критерії та перевірка

- Об'ємна трава читається з ігрової камери; кущі/підлісок присутні;
  дерева різняться розміром/формою (single/group × small/medium/large +
  saplings); деталі біом-доречні; галявини залишаються.
- 0 floating/buried об'єктів на схилах (footprint+grounding чинні);
  0 декору на воді/піску/дорогах/POI-клітинках; межі чанків без швів.
- EditMode: детермінізм (same seed ×2 = ідентично; diff seed = різне),
  layer affinities (water prefer/avoid, forest interior/edge, nearTree),
  exclusion/footprint, `layers` відсутні → legacy поведінка байт-ідентична.
- Visual smoke: seeds 42/777/12345 — луг, ліс, узлісся, галявина,
  береги річки/озера, схил, межа чанків; default/far/close камери.
  `out=docs/qa/evidence/veg-seed*/`.
- Perf-заміри: generation ms у тесті; instance counts + render stats у
  smoke-log; прийнятний бюджет = відсутність регресії кадру на eyeball-smoke
  + instancing ON на нових матеріалах; документувати заміри.
- Після перевірки: оновити цей план і `docs/qa/WORK_STATE.md`
  (об'єкти, правила, параметри, результати, обмеження).

## 7. Явні обмеження v1

- Вітер у траві/кронах — `DecorSharedStylized` не має wind-входу; новий
  vertex-sway — окремий етап за потреби.
- LOD для дрібних деталей — натомість distance culling живе на рівні
  chunk-camera culling; billboard-LOD не додається.
- Tint-варіанти дерев — stretch-goal; базове різноманіття через pool mix.

## 8. Статус реалізації (2026-09-26)

### Реалізовано

- **Генератор**: `GenerateLayers` — адитивний прохід після legacy, детермінований
  за `(seed, x, y, layerIndex)`. Affinity: water any/prefer/avoid/require,
  forest any/interior/edge/avoid, nearTree boost, skipObjectCells
  (`ObjectMap` POI), shorelineExclusion, maxSlopeMeters, per-layer scale range.
- **Grove-семантика лісу** (`MarkTreeAnchorGroves`): оскільки рецептурний
  пайплайн не емітить `forest-*` tile ids (світ у manifest завжди
  grass/hill/sand/snow/water), "ліс" для forest-affinity = клітинки в радіусі 2
  від фактично заспавнених деревних anchors (+ лісні tile ids, якщо вони є).
  Interior = повністю вкрита гаєм клітинка, edge = межа гаю. Saplings з
  `feedsTreeAffinity` розширюють anchors для наступних шарів.
- **Активи**: `MoyvaVegetationAssetBuilder` (menu + `-executeMethod
  ...BuildFromMenu`) генерує 27 префабів `veg-*` у
  `Assets/Moyva/Generated/Vegetation/` — процедурні low-poly меші (куртини
  трави ×6, папороть ×2, квітка, гілки ×2, листяні плями ×2, мох, колода,
  садженці ×3, очерет) + картки кущів bush_001..005; матеріали на
  `DecorSharedStylized` з `m_EnableInstancingVariants:1`; зелений тинт
  трав'яних карток; палітра `VegPalette.png` + спільний `VegFlat`.
- **Instancing** увімкнено й на legacy `Grass_bilboard_Mat`,
  `Grass_kreuz_Mat`.
- **Каталог**: 27 `veg-*` ключів у `MoyvaRuntimeAssetCatalog.prefab`
  (детерміновані GUID = md5(path)); `mapobjectregistry.json` += 28 defs.
- **Preset** `environment-decoration-config.json` (обидві копії): 14 layer
  rules — grass .6, tallgrass prefer-water .14 + edge .22, bush .16,
  fern interior .22 + edge .12, litter nearTree .7/.03, flower .06,
  pebble .1, log .05, sapling edge .14 + sparse meadow .03,
  stump nearTree .45/.02, reed require-water .3; пули розширені `veg-*`.
  `skipObjectCells:true` на bush/log/sapling/stump.
- **Чанк-видимість**: `EnvironmentDecorationSpawner` реєструє кожен
  spawned renderer у `IMapVisualChunkRegistry` під власним
  `MapChunkCoord` — камера-culling і fog-hide тепер прибирають рослинність
  разом із TerrainMesh (раніше декор лишався видимим на прихованих чанках).
- **Серіалізація**: `[SerializeField]` на `BiomeBoost` (прибрано UAC1015).

### Тести

- `EnvironmentDecorationLayerTests` + `EnvironmentDecorationSpawnerTests`:
  50/50 — детермінізм (same/diff seed), require/avoid water,
  interior/edge по tile-forest І по tree-anchor grove, skipObjectCells,
  feedsTreeAffinity, chunk-registry реєстрація renderers, legacy unchanged
  при відсутніх layers.
- `WorldGeographyEngineTests.Generate_Forests_AppearAcrossSeedSpace` —
  ліс на рівні движка: Balanced 18/24, Pangaea 18/24, Continents 17/24,
  Highlands 13/24 seeds (1–24) містять forest cells; у recipe-сцені ці
  клітинки не доходять до TileMap — саме тому grove-семантика обов'язкова.
- Повний прогін EditMode: 875/875 PASS.

### Visual smoke (docs/qa/evidence/)

| Run | Світ | Результат |
|---|---|---|
| veg-seed42 | 48×48, launch default | PASS, worldHash 5D8E6C10 |
| veg-seed42-tuned | після збільшення карток + тинт | PASS, той самий worldHash |
| veg-seed9-forest | recipe seed=9 | PASS, гайки + трава читаються |
| veg-seed12-forest | recipe seed=12 | PASS |
| veg-seed777 / veg-seed12345 | recipe seeds | PASS, no forest tiles (як очікувалося) |
| veg-seed42-grove | після grove-fix | PASS, 1976 renderers, підлісок біля гайків видно |

Метрики (48×48): genTime 4.7–6.4 s, renderers 1658–2771,
gameObjects ~2000–3100, memMB ~1510–1540. Прямого before/after для тих
самих світів нема (старі маніфести без perf-поля; світи змінилися після
hydrology-робіт) — бюджет: genTime < ~10 s і renderers < ~4000 на 48×48
тримались без регресій.

### Залишкові обмеження

- `forest-*` tile ids не досягають TileMap у рецептурному пайплайні —
  forest-affinity працює через grove-модель (anchors), не через tile id.
- `ClassifyBiome`/`biomeBoost["forest"]` досі keyed на tile id — латентний
  множник; при появі forest tiles запрацює автоматично.
- Chopped resource trees: декорації ніколи не стоять на ObjectMap-клітинках
  з POI; якщо POI видалено сейвом, клітинка стає вільною для декору —
  косметичне відростання можливе, gameplay-стан не чіпається.
- Вітер — нема shader-підтримки; не реалізовано.
- Декор реєструється per-renderer у chunk-registry — ~2–3k entries на світ;
  витрати пам'яті лінійні, `ApplyVisibility` O(n) при зміні камери.
- Perf-базова лінія pre-change недоступна (старі маніфести без метрик).
