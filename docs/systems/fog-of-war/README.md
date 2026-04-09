# Fog of War — Огляд системи

← [Назад до README](../../README.md)

---

## Що це?

**Fog of War** (Туман Війни) — система видимості для тайлової стратегії Moyva. Вона визначає, які тайли карти бачать гравцеві юніти, і відображає туман через динамічний URP 2D шейдер.

---

## Три стани туману

| Стан | Лічильник | R8 піксель | Візуальний вигляд |
|---|---|---|---|
| **Unexplored** | 0 (ніколи не бачили) | 0 (чорний) | Густа темрява |
| **Explored** | 0, але бачили раніше | 128 (сірий) | Напівпрозорий туман |
| **Visible** | ≥ 1 | 255 (білий) | Чистий тайл |

---

## Як це працює (коротко)

1. Єдиний `int[,]` масив зберігає **лічильник юнітів**, що бачать кожен тайл.
2. Кожен юніт має власний `VisionRange`, але система завжди затискає його до мінімуму `1`.
3. Коли юніт рухається — старі тайли отримують `−1`, нові `+1`.
4. Висота тайлу впливає на фактичний огляд: з висоти видно далі, вгору з низини видно гірше, тайли на тому самому рівні видно без штрафу.
5. Після отримання `WorldGeneratedDataSignal` система приймає `HeightMap` і перебудовує видимість усіх зареєстрованих юнітів.
6. Якщо лічильник падає до 0 — тайл стає `Explored` (не `Unexplored`), якщо він вже був відвіданий.
7. `Texture2D` (TextureFormat.R8) оновлюється лише для змінених тайлів або повністю перебудовується після зміни мапи висот.
8. Шейдер `Moyva/FogOfWar` читає текстуру і відображає відповідний туман.

---

## Нова модель зору

Поточна реалізація більше не використовує однорідний радіус огляду для всіх юнітів.

### Що враховується

| Фактор | Ефект |
|---|---|
| `UnitClassConfig.VisionRange` | Базовий радіус конкретного типу юніта |
| Мінімальний радіус | Не може бути меншим за `1` |
| Висота спостерігача | Дає бонус до дальності пошуку цілей |
| Ціль нижче спостерігача | Може дати додатковий бонус видимості вниз по схилу |
| Ціль вище спостерігача | Дає штраф до дальності або повністю блокує видимість |
| Проміжний рельєф | Може перекрити line of sight |

### Керуючий сервіс

Центр правил видимості винесено в `HeightAwareVisionService`.

Він відповідає за:

| Метод | Призначення |
|---|---|
| `SetHeightMap(float[,] heightMap)` | Зберігає актуальну карту висот |
| `GetSearchRadius(...)` | Рахує максимальний радіус пошуку з урахуванням висоти спостерігача |
| `IsTargetVisible(...)` | Визначає, чи видно конкретний тайл з урахуванням висоти й line of sight |

---

## Quick-Start

1. Створіть ScriptableObject `FogOfWarSettings` → [so-setup-guide.md](so-setup-guide.md)
2. Відкрийте `Moyva/Tools/Fog of War/Vision Tuner` і підберіть параметри Height Vision та `VisionRange` юнітів → [vision-tuner-guide.md](vision-tuner-guide.md)
3. Додайте `FogOfWarInstaller` до Scene Context → [scene-connection-guide.md](scene-connection-guide.md)
4. Створіть `FogOfWarQuad` (Quad + MeshRenderer + FogQuadController) → [scene-connection-guide.md](scene-connection-guide.md)
5. Призначте матеріал із шейдером `Moyva/FogOfWar`
6. Запустіть сцену

---

## Підсторінки

| Документ | Зміст |
|---|---|
| [architecture.md](architecture.md) | Шари API/Runtime, asmdef граф, fallback таблиця |
| [visibility-algorithm.md](visibility-algorithm.md) | Height-aware visibility, Line of Sight, правила висоти |
| [shader.md](shader.md) | URP 2D шейдер, Perlin fBm, smoothstep, edge bleeding |
| [texture-pipeline.md](texture-pipeline.md) | R8 текстура, dirty-tiles, SetPixelData |
| [signals-and-integration.md](signals-and-integration.md) | Підписки, data flow, порядок ініціалізації |
| [initialization-order.md](initialization-order.md) | ExecutionOrder=-5, місце в Zenject-послідовності |
| [so-setup-guide.md](so-setup-guide.md) | Гайд дизайнера: SO поля, рекомендовані значення |
| [vision-tuner-guide.md](vision-tuner-guide.md) | Окрема документація по Editor Tool `Vision Tuner` |
| [scene-connection-guide.md](scene-connection-guide.md) | Покрокова інструкція підключення в сцені |
| [testing.md](testing.md) | Тест-класи, що тестують, як запустити |
| [save-system-stub.md](save-system-stub.md) | Чому stub і як підключити реальну реалізацію |
