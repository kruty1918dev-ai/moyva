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
2. Коли юніт рухається — старі тайли отримують `−1`, нові `+1`.
3. Якщо лічильник падає до 0 — тайл стає `Explored` (не `Unexplored`), якщо він вже був відвіданий.
4. `Texture2D` (TextureFormat.R8) оновлюється лише для змінених тайлів.
5. Шейдер `Moyva/FogOfWar` читає текстуру і відображає відповідний туман з CPU Perlin noise та edge bleeding.

---

## Quick-Start

1. Створіть ScriptableObject `FogOfWarSettings` → [so-setup-guide.md](so-setup-guide.md)
2. Додайте `FogOfWarInstaller` до Scene Context → [scene-connection-guide.md](scene-connection-guide.md)
3. Створіть `FogOfWarQuad` (Quad + MeshRenderer + FogQuadController) → [scene-connection-guide.md](scene-connection-guide.md)
4. Призначте матеріал із шейдером `Moyva/FogOfWar`
5. Запустіть сцену

---

## Підсторінки

| Документ | Зміст |
|---|---|
| [architecture.md](architecture.md) | Шари API/Runtime, asmdef граф, fallback таблиця |
| [visibility-algorithm.md](visibility-algorithm.md) | Symmetric Shadowcasting, 8 октантів |
| [shader.md](shader.md) | URP 2D шейдер, Perlin fBm, smoothstep, edge bleeding |
| [texture-pipeline.md](texture-pipeline.md) | R8 текстура, dirty-tiles, SetPixelData |
| [signals-and-integration.md](signals-and-integration.md) | Підписки, data flow, порядок ініціалізації |
| [initialization-order.md](initialization-order.md) | ExecutionOrder=-5, місце в Zenject-послідовності |
| [so-setup-guide.md](so-setup-guide.md) | Гайд дизайнера: SO поля, рекомендовані значення |
| [scene-connection-guide.md](scene-connection-guide.md) | Покрокова інструкція підключення в сцені |
| [testing.md](testing.md) | Тест-класи, що тестують, як запустити |
| [save-system-stub.md](save-system-stub.md) | Чому stub і як підключити реальну реалізацію |
