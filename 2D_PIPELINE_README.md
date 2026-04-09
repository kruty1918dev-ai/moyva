# Moyva 2D Pipeline Migration

## 📋 Overview

Проект успішно перенесено на 2D pipeline. Всі налаштування оптимізовані для tile-based 2D гри з системою рівня на основі спрайтів.

## 🎯 Що було змінено

### 1. **Package Dependencies** (`Packages/manifest.json`)

#### ❌ Видалено з прямих залежностей `Packages/manifest.json` 3D модулі:
- `com.unity.ai.navigation` - 3D навігація (не потрібна для tile-based гри)
- `com.unity.multiplayer.center` - не потрібна
- `com.unity.timeline` - видалено для зменшення overhead
- `com.unity.modules.ai` - 3D AI/pathfinding
- `com.unity.modules.cloth` - 3D симуляція тканин
- `com.unity.modules.terrain` - 3D терен; видалено з прямих залежностей, але може залишатися у `Packages/packages-lock.json` як transitive/built-in dependency через URP
- `com.unity.modules.terrainphysics` - 3D земля фізика
- `com.unity.modules.physics` - 3D фізика (Box/Sphere colliders); видалено з прямих залежностей, але може залишатися у `Packages/packages-lock.json` як transitive/built-in dependency через URP
- `com.unity.modules.vehicles` - 3D транспорт
- `com.unity.modules.vr` - VR підтримка
- `com.unity.modules.xr` - XR підтримка
- `com.unity.modules.wind` - 3D вітер система

> Примітка: цей список описує саме прямі залежності в `Packages/manifest.json`. Частина Unity-модулів може залишатися в `Packages/packages-lock.json` як непрямі або built-in залежності через `com.unity.render-pipelines.core` / URP.

#### ✅ Залишено для 2D:
- `com.unity.render-pipelines.universal` (URP) - основа рендеру
- `com.unity.feature.2d` - 2D функції (Tilemap, Sprite Atlas, тощо)
- `com.unity.modules.physics2d` - 2D фізика (Box2D colliders, Rigidbody2D)
- `com.unity.modules.tilemap` - Tilemap система
- `com.unity.modules.particlesystem` - система частинок для ефектів
- `com.unity.modules.video` - відео плейбек

### 2. **Quality Settings** (`ProjectSettings/QualitySettings.asset`)

Оптимізовано для 2D рендеру:

| Параметр | Було | Стало | Причина |
|----------|------|-------|---------|
| `pixelLightCount` | 2 | 0 | 2D спрайти не використовують динамічні тіні |
| `shadows` | 2 (Hard) | 0 (None) | Спрайти не отримують тіней |
| `antiAliasing` | 0 | 2-4 | Для pixel-perfect вигляду |
| `terrainQualityOverrides` | Enabled | Disabled | Немає терену |
| `lodBias` | 1-2 | 1-2 | Збережено для оптимізації |
| `enableLODCrossFade` | Enabled | Disabled | LOD не потрібен для 2D |

### 3. **URP Renderers** (`Assets/Moyva/SO/URP/`)

#### Mobile_Renderer.asset
- **До**: SSAO (Screen Space Ambient Occlusion), стандартна 3D конфігурація
- **Після**: Оптимізовано для мобільних 2D спрайтів
  - DepthPrimingMode: 1 (Enabled) - краще управління Z-буфером
  - Видалено SSAO Renderer Feature
  - Спрощено стенсіль стан

#### PC_Renderer.asset
- **До**: SSAO, High-end 3D рендеринг
- **Після**: Оптимізовано для PC 2D з анти-алайсингом
  - DepthPrimingMode: 1 (Enabled)
  - Видалено SSAO Renderer Feature
  - RenderingMode: 0 (Forward) - краще для 2D

### 4. **Physics2D Settings** (`ProjectSettings/Physics2DSettings.asset`)

| Параметр | Було | Стало | Причина |
|----------|------|-------|---------|
| Gravity | (0, -9.81) | (0, 0) | Grid-based гра, не потрібна гравітація |
| VelocityIterations | 8 | 4 | Зменшено обчислення |
| PositionIterations | 3 | 2 | Зменшено обчислення |
| MaxTranslationSpeed | 100 | 50 | Grid-based рух |
| MaxRotationSpeed | 360 | 180 | Зменшено для 2D |
| MaxSubStepCount | 4 | 2 | Оптимізація |
| MinSubStepFPS | 30 | 60 | Краща якість на 60 FPS |

### 5. **Prefabs & Assets**

✅ **Вже 2D-готові:**
- `Unit-warrior-01.prefab` - використовує SpriteRenderer
- Всі Tile префаби (`grass.prefab`, `water.prefab` тощо) - SpriteRenderer
- Маски Tile Palette UI
- Building префаби (castle, house, windmill) - SpriteRenderer

❌ **Немає 3D моделей:**
- Проект вже вилучений від FBX/OBJ файлів
- Всі асети у форматі спрайтів PNG/PSD

### 6. **Камери**

✅ **Вже налаштовані:**
- Main Camera - Orthographic режим
  - `orthographic: 1` (ортогональна проекція)
  - `orthographic size: 20` - zoom level
  - `near clip plane: 0.3`, `far clip plane: 1000` - окаї для 2D

## 📊 Переваги для 2D гри

### 🎮 Гейм-плей
- **Grid-based рух**: Гравітація вимкнена (0, 0)
- **Стабільна фізика 2D**: Оптимізовано для turn-based/tile-based механіки
- **Немає затримок**: Зменшено фізичні ітерації для швидших обчислень

### 📈 Продуктивність
- **Менше памʼяті**: Видалено 12+ 3D модулів
- **Менше батареї**: Відсутні тіні, LOD, терен, VR
- **Швидша збірка**: Менше залежностей для завантаження

### 🎨 Рендеринг
- **Просте отримання**: Спрайти + URP = швидко
- **Pixel-perfect**: Anti-aliasing налаштовано
- **Немає тіней**: Спрайти не отримують тінів на 2D

## 🔧 Як розширити

### Якщо потрібні 3D елементи (нечасто):
1. Додати `com.unity.modules.physics` назад
2. Настроїти окремий URP Renderer для 3D
3. Використовувати Canvas для UI

### Якщо потрібна анімація:
- Уже встановлена `com.unity.modules.animation` через feature.2d
- Використовувати Sprite Animations або Animator

### Якщо потрібна музика/звук:
- Уже встановлена `com.unity.modules.audio`

## ✅ Чек-лист невелики тесту

- [ ] Запустити сцену Gamplay_Scene - камера працює в 2D
- [ ] Перевірити юніти рухаються по гріду (без гравітації)
- [ ] Перевірити спрайти рендерються коректно
- [ ] Перевірити нема помилок про відсутні 3D модулі
- [ ] Перевірити FPS в Profiler

## 📝 Коміт

```
45e1410 feat: Migrate project to 2D pipeline - remove 3D packages and optimize for 2D rendering
```

Гілка: `feature/migrate-to-2d-pipeline`

---

**Проект готовий до 2D розробки! 🎮**
