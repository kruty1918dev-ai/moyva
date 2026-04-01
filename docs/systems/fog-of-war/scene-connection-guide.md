# Fog of War — Покрокова інструкція підключення сцени

← [README](README.md)

---

## Крок 1 — Створити ScriptableObject

1. `Create → Moyva → FogOfWarSettings`
2. Налаштуйте поля (детально: [so-setup-guide.md](so-setup-guide.md))

---

## Крок 2 — Створити FogOfWarQuad GameObject

1. У Hierarchy: `Create → 3D Object → Quad`
2. Перейменуйте в `FogOfWarQuad`
3. Додайте компонент `FogQuadController`
   - `Add Component → Scripts → Kruty1918.Moyva.FogOfWar.Runtime → FogQuadController`
4. Перевірте, що є також `MeshRenderer` (RequireComponent додає автоматично)

---

## Крок 3 — Створити Material із шейдером Moyva/FogOfWar

1. `Create → Material`
2. Назвіть `FogOfWarMaterial`
3. У Inspector матеріалу → Shader → виберіть `Moyva/FogOfWar`
4. Збережіть у `Assets/Moyva/Materials/`

---

## Крок 4 — Призначити Material до FogOfWarQuad

1. Виберіть `FogOfWarQuad`
2. На `MeshRenderer` → `Materials[0]` → перетягніть `FogOfWarMaterial`

---

## Крок 5 — Призначити SO до компонентів

1. На `FogQuadController` → поле `Settings` → перетягніть `FogOfWarSettings` SO
2. На `FogOfWarInstaller` → поле `Settings` → перетягніть той самий SO

---

## Крок 6 — Додати FogOfWarInstaller до Scene Context

1. Виберіть `SceneContext` у Hierarchy
2. `Mono Installers` → розширте список
3. Додайте `FogOfWarInstaller` **після** `SignalBusInstaller` і **після** `GridInstaller`
4. Порядок у Scene Context:
   ```
   1. SignalBusInstaller
   2. GridInstaller
   3. FogOfWarInstaller   ← додайте тут
   4. ...інші інсталятори...
   5. BootstrapInstaller
   ```

> FogOfWarInstaller можна розмістити на тому самому GameObject або на окремому.

---

## Крок 7 — Налаштувати SortingLayer

1. Виберіть `FogOfWarQuad → MeshRenderer`
2. `Sorting Layer` → виберіть або створіть шар `FogOfWar`
3. `Order in Layer` → рекомендовано: `100` (щоб бути поверх тайлів і юнітів, але під UI)

---

## Крок 8 — Перевірити в Play Mode

1. Запустіть сцену
2. Вся карта повинна бути покрита темним туманом
3. Після спавну юнітів — навколо них має з'явитися чистий простір
4. При русі юніта — туман за ним стає `Explored` (напівпрозорий)

---

## Крок 9 — Troubleshooting

| Симптом | Перевірте |
|---|---|
| Нічого не видно взагалі | SortingLayer і `Order in Layer`; чи є `FogOfWarInstaller` у Scene Context |
| Туман весь білий | `_FogTex` не встановлено на матеріал; перевірте `FogQuadController` |
| Туман не реагує на юнітів | `FogOfWarInstaller` не доданий або доданий після `BootstrapInstaller` |
| Помилка "SignalNotDeclaredException" | `FogStateChangedSignal` не оголошено у `SignalBusInstaller` |
| Шейдерна помилка у Console | Переконайтесь, що URP встановлений і `com.unity.render-pipelines.universal` є в Packages |
