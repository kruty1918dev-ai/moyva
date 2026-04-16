# Фікс: Водяний шейдер рендерить невидимо / прозоро

**Дата:** 2026-04-15  
**Бранч:** `plan/economy-logic`  
**Статус:** Виправлено

---

## Симптоми

- Водяні тайли на карті повністю прозорі/невидимі
- В Console Unity виводяться попередження:
  ```
  [GeneratorInstaller] Water material is not assigned. Water shader fallback will be used.
  [GeneratorInstaller] Tile blend material is not assigned. Terrain blend shader fallback will be used.
  ```
- Шейдер `Moyva/2D/Water` компілюється без помилок

---

## Кореневі причини (5 штук)

### 1. Матеріали використовують НЕПРАВИЛЬНИЙ шейдер

**Файли:** `Assets/Moyva/Materials/WaterMaterial.mat`, `Assets/Moyva/Materials/TileBlendMaterial.mat`

**Проблема:** Обидва `.mat` файли посилались на GUID шейдера `933532a4fcc9baf4fa0491de14d08ed7` — це стандартний **URP Lit** (3D шейдер), а не кастомні 2D шейдери проекту.

| Матеріал | Потрібний шейдер | Правильний GUID |
|----------|-----------------|-----------------|
| WaterMaterial | `Moyva/2D/Water` | `9a6f8c7fc1c557d53a3cac6a061176a7` |
| TileBlendMaterial | `Moyva/2D/TileBlend` | `2c70315ffebd8cce188d68dcd2fd88a9` |

**Як трапилось:** При створенні матеріалів через Unity Editor, вони автоматично отримали дефолтний URP Lit шейдер, і шейдер ніхто не змінив на кастомний.

**Як перевірити:** Відкрити `.mat` файл у текстовому редакторі → шукати `m_Shader: {fileID: 4800000, guid: ...}`. GUID має збігатися з GUID із `.shader.meta` файлу.

### 2. Матеріали не прив'язані в сцені

**Файл:** `Assets/Moyva/Scenes/Gameplay_Scene.unity`

**Проблема:** Компонент `GeneratorInstaller` в сцені не мав серіалізованих значень для полів `_waterMaterial`, `_tileBlendMaterial`, `_tileTextureAtlas`. Ці поля були додані до C# класу, але **ніколи не призначені в Inspector**.

**Як перевірити:** Пошукати в YAML сцени серіалізацію `GeneratorInstaller` → перевірити що є рядки `_waterMaterial`, `_tileBlendMaterial`, `_tileTextureAtlas` з валідними GUID.

### 3. Неправильні property в матеріалах

**Проблема:** `.mat` файли містили property від URP Lit (`_BaseColor`, `_Metallic`, `_Smoothness`, `_BumpScale`, тощо) замість property кастомних шейдерів.

**Water.shader очікує:**
- `_ShallowColor`, `_DeepColor`, `_FoamColor` (Color)
- `_FoamWidth`, `_WaveSpeed`, `_WaveAmplitude`, `_WaveFrequency`, `_MaxDepth`, `_PixelSize` (Float)
- `_MainTex` (Texture)
- Per-instance: `_ShoreDistance`, `_ShoreMask` (через MaterialPropertyBlock)

**TileBlend.shader очікує:**
- `_Color` (Color)
- `_BlendWidth` (Float)
- `_MainTex`, `_AtlasTex` (Texture)
- Per-instance: `_TileRect`, `_NeighborMask`, `_NeighborRectN/E/S/W` (через MaterialPropertyBlock)

### 4. Несумісна база шейдера для SpriteRenderer

**Файли:** `Assets/Moyva/Shaders/2D/Water.shader`, `Assets/Moyva/Shaders/2D/TileBlend.shader`

Початкові шейдери були написані на базі `Core.hlsl` з кастомним vertex/fragment пайплайном. Для SpriteRenderer в URP 2D це часто ламає сумісність із внутрішніми sprite-даними (flip, renderer color, external alpha, skinned sprite, batching).

**Фікс:** обидва шейдери переписані на архітектуру `Sprite-Unlit-Default`:
- `#include ".../Shaders/2D/Include/Core2D.hlsl"`
- `#include ".../Shaders/2D/Include/2DCommon.hlsl"`
- `CommonUnlitVertex(...)` + `CommonUnlitFragment(...)`
- legacy sprite properties (`_RendererColor`, `_AlphaTex`, `_EnableExternalAlpha`, `PixelSnap`)

Після цього SpriteRenderer стабільно рендерить шейдери як звичайні sprite-матеріали.

### 5. Tile atlas не був доступний у runtime

**Файл:** `Assets/Moyva/Scripts/Features/Visuals/API/TileTextureAtlasSO.cs`

`TileTextureAtlasSO` зберігає `_atlas` як `[NonSerialized]`, тому після перезапуску Unity `IsBuilt == false` доки хтось явно не викличе `BuildAtlas()`.

**Наслідок:** `MapVisualInstantiator` не вважав `TileBlend` готовим (`hasBlendMaterial == false`), або `_AtlasTex` не був заповнений.

**Фікс у runtime:** в `BuildWorldFromData()` додано fallback:
- якщо атлас не збудований → викликати `_tileAtlas.BuildAtlas()`
- якщо атлас є → `_tileBlendMaterial.SetTexture("_AtlasTex", _tileAtlas.Atlas)`

---

## Додаткова проблема: Розрив у data pipeline

**Файл:** `GraphBasedMapDataGenerator.cs`

`IMapDataGenerator.GenerateMapData` callback передає лише 4 масиви: `biomeMap, objectMap, heightMap, buildingMap`. Але `OutputNode` повертає 7 outputs (ще: `ShoreDistanceMap`, `ShoreMask`, `NeighborMask`). `GraphBasedMapDataGenerator` ігнорує outputs[4-6].

**Наслідок:** `GeneratedWorldData.ShoreDistanceMap/ShoreMask/NeighborMask` завжди `null`.

**Чому це не блокує:** `MapVisualInstantiator.BuildWorldFromData()` має fallback — якщо ці дані `null` І матеріал != null, він обчислює їх з `BiomeMap` через `ComputeWaterData()` та `ComputeNeighborMask()`. Тобто після фіксу матеріалів система працює, просто графік обчислює дані даремно.

---

## Виконані фікси

### Фікс 1: WaterMaterial.mat
- Замінено `m_Shader` GUID: `933532a4fcc9baf4fa0491de14d08ed7` → `9a6f8c7fc1c557d53a3cac6a061176a7`
- Видалено всі URP Lit property
- Додано правильні property водяного шейдера
- `m_EnableInstancingVariants: 1` (потрібно для per-instance `_ShoreDistance`/`_ShoreMask`)
- `m_CustomRenderQueue: 3000` (Transparent)
- `stringTagMap: RenderType: Transparent`

### Фікс 2: TileBlendMaterial.mat
- Замінено `m_Shader` GUID: `933532a4fcc9baf4fa0491de14d08ed7` → `2c70315ffebd8cce188d68dcd2fd88a9`
- Видалено всі URP Lit property
- Додано правильні property (`_BlendWidth`, `_Color`, `_MainTex`, `_AtlasTex`)
- `m_EnableInstancingVariants: 1`
- `m_CustomRenderQueue: 3000` (Transparent)

### Фікс 3: Gamplay_Scene.unity
Додано серіалізовані посилання до `GeneratorInstaller`:
```yaml
_waterMaterial: {fileID: 2100000, guid: c928dfb99fea47d8ebc166d9a49387fb, type: 2}
_tileBlendMaterial: {fileID: 2100000, guid: e46a4d71e771abfaeb75c6cd8a61231d, type: 2}
_tileTextureAtlas: {fileID: 11400000, guid: 0810248a040141055a494fd83f652eb6, type: 2}
```

### Фікс 4: Water.shader і TileBlend.shader (архітектурний)
- Переписано обидва шейдери на URP 2D `Sprite-Unlit` шаблон.
- Збережено ефекти (вода/shore, blend сусідів), але рендер тепер проходить через `CommonUnlitFragment`.
- Прозорість тепер береться з реального sprite sample (`spriteSample.a`) замість "голого" кастомного кольору.

### Фікс 5: MapVisualInstantiator runtime fallback для атласу
- У `BuildWorldFromData()` додано runtime побудову атласу при `!_tileAtlas.IsBuilt`.
- Після побудови атлас автоматично прокидається в матеріал: `_tileBlendMaterial.SetTexture("_AtlasTex", _tileAtlas.Atlas)`.

### Фікс 6: Керований перехід берег → глибина і хвиля до берега
- У `Water.shader` додано окремі параметри:
  - `_DepthStart` — де починається перехід від мілкої води
  - `_DepthEnd` — де вода вважається вже глибокою
  - `_DepthExponent` — крива переходу
- У `MapVisualInstantiator` тепер для кожного водного тайлу обчислюється `_ShoreFlow` — нормалізований напрямок до найближчого берега.
- Хвилі більше не йдуть глобально в один бік: фаза будується вздовж `_ShoreFlow`, тому на різних ділянках берегу хвиля заходить із різних 8 напрямків.

### Фікс 7: Налаштовувана біла берегова лінія на НЕ-водному тайлі
- У `TileBlend.shader` додано окремий shoreline overlay, який малюється лише на тайлах суходолу, що межують із водою.
- Лінія НЕ заходить на водний тайл: її відстань рахується тільки всередину суходільного тайла від межі з водою.
- Додані параметри налаштування:
  - `_ShorelineColor`
  - `_ShorelineWidth`
  - `_ShorelineSoftness`
  - `_ShorelineBaseInset`
  - `_ShorelineTravel`
  - `_ShorelineWaveAmplitude`
  - `_ShorelineWaveFrequency`
  - `_ShorelineSpeed`
  - `_ShorelineIntensity`
- Анімація берегової лінії тепер хвилеподібна вздовж берега і може заходити вглиб суходолу та повертатися назад, але центр смуги завжди затиснутий `max(0, ...)`, тому вона не перетинає водну сторону межі.

---

## Ланцюжок виконання (як це працює)

```
GeneratorInstaller.InstallBindings()
  ├─ BindInstance(_waterMaterial).WithId("WaterMaterial")    ← MAT повинен мати правильний шейдер
  ├─ BindInstance(_tileBlendMaterial).WithId("TileBlendMaterial")
  └─ BindInstance(_tileTextureAtlas).AsSingle()

MapVisualInstantiator (через Zenject DI)
  ├─ _waterMaterial = [Inject(Id="WaterMaterial")]           ← повинен бути != null
  ├─ _tileBlendMaterial = [Inject(Id="TileBlendMaterial")]
  └─ _tileAtlas = [Inject(Optional=true)] TileTextureAtlasSO

BuildWorldFromData()
  ├─ hasWaterMaterial = _waterMaterial != null                ← true після фіксу
  ├─ ComputeWaterData(biomeMap) → shoreDistanceMap, shoreMask ← fallback обчислення
  ├─ ComputeNeighborMask(biomeMap) → neighborMask
  └─ for each tile:
      ├─ if water → ApplyWaterMaterial(tileGO, shoreDistance, shoreMask)
      │     ├─ sr.sharedMaterial = _waterMaterial             ← Moyva/2D/Water
      │     └─ sr.SetPropertyBlock(_ShoreDistance, _ShoreMask)
      └─ if land → ApplyBlendMaterial(tileGO, ...)
            ├─ sr.sharedMaterial = _tileBlendMaterial          ← Moyva/2D/TileBlend
            └─ sr.SetPropertyBlock(_TileRect, _NeighborMask, ...)
```

---

## Чеклист для майбутнього: "Кастомний шейдер не рендерить"

1. **Перевір GUID шейдера в .mat файлі** — відкрий `.mat` у текстовому редакторі, порівняй `m_Shader.guid` з `guid` у `.shader.meta`
2. **Перевір Inspector прив'язки** — чи матеріал призначений у MonoBehaviour/Installer у сцені
3. **Перевір property** — `.mat` повинен мати property відповідного шейдера, а не дефолтного URP Lit
4. **Перевір GPU Instancing** — якщо шейдер використовує `UNITY_INSTANCING_BUFFER`, увімкни `m_EnableInstancingVariants: 1`
5. **Перевір RenderQueue** — для Transparent шейдерів: `m_CustomRenderQueue: 3000`, `stringTagMap.RenderType: Transparent`
6. **Перевір LightMode tag** — для 2D URP: `Tags { "LightMode" = "Universal2D" }`
7. **Перевір DI null-check** — якщо матеріал inject-ується через `[Inject(Optional=true)]`, код може пропускати застосування коли матеріал null
8. **Перевір Console warnings** — `[GeneratorInstaller] Water material is not assigned` = матеріал не прив'язаний в Inspector
9. **Для SpriteRenderer в URP 2D стартуй від `Sprite-Unlit-Default`** — не від `Core.hlsl`-мінімалістики
10. **Перевір runtime-джерела текстур** — якщо шейдер семплить atlas/lookup texture, переконайся що texture реально встановлена в material або MaterialPropertyBlock

---

## Нові параметри для тюнінгу в Inspector

### WaterMaterial
- `_DepthStart`: старт мілкої зони від берега
- `_DepthEnd`: кінець переходу в глибоку воду
- `_DepthExponent`: форма переходу від мілкого до глибокого
- `_FoamWidth`: товщина піни на воді біля берегу
- `_FoamSoftness`: м'якість краю піни
- `_WaveSpeed`: швидкість хвилі
- `_WaveAmplitude`: амплітуда хвилі
- `_WaveFrequency`: базова частота хвиль
- `_WaveSecondaryFrequency`: додатковий дрібніший шар хвиль
- `_WaveCurl`: бокове викривлення хвилі вздовж берегу
- `_WaveChop`: дрібна рубаність поверхні
- `_BreakDistance`: наскільки далеко від берега відчувається прибій
- `_BreakStrength`: наскільки посилюється хвиля біля берегу

### TileBlendMaterial
- `_ShorelineColor`: колір обводки на березі
- `_ShorelineWidth`: товщина смуги
- `_ShorelineSoftness`: розмиття/м'якість краю
- `_ShorelineBaseInset`: базове зміщення вглиб суходолу
- `_ShorelineTravel`: максимальний додатковий рух смуги вглиб і назад
- `_ShorelineWaveAmplitude`: хвилястість лінії вздовж берегу
- `_ShorelineWaveFrequency`: частота хвилі вздовж берегу
- `_ShorelineSpeed`: швидкість анімації берегової лінії
- `_ShorelineIntensity`: інтенсивність змішування з базовим кольором
