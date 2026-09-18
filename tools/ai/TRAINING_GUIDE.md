# Довідка з навчання Moyva

## Огляд

Навчання агента керується двома входами:

- `./moyva` — центр керування: статус, запуск, логи, чекпоінти, TensorBoard.
- `./moyva-train` — прямий launcher (`tools/ai/moyva_train.py`): build/train/resume/watch/inspect/doctor.

Автономний навчальний план (`curriculum.autonomous.enabled`) веде агента сценаріями
з `Assets/Moyva/Presets/AI/Scenarios/` у порядку `manifest.json` і періодично запускає
заморожену оцінку чекпоінта окремим процесом.

## Безпечні межі

- **Одна арена з автономним навчальним планом.** `--arenas N > 1` відхиляється,
  коли `curriculum.autonomous.enabled=true`: усі процеси писали б у спільні файли
  `curriculum-state.json`, журнал рішень та стан оцінок.
- **Зупинка лише власних процесів.** `moyva stop <token>` перевіряє PID, час старту
  та виконуваний файл — жодних `pkill -f` по чужих процесах.
- **Контракт чекпоінтів.** Запуск, resume та deploy звіряють hash спостережень/дій
  з `Assets/Moyva/Presets/AI/Resources/MoyvaBotContract.json`. Чекпоінти,
  навчені під іншим контрактом, не підіймаються мовчки — тренуйте нові.

## Швидкий старт

```bash
./moyva doctor          # перевірка середовища, venv, Unity, дисків
./moyva-train train --headless --time-scale 5.0 --max-steps 1000000
./moyva status          # стан supervisor'а та навчання
./moyva status --watch  # live-рядок прогресу (Ctrl+C — вихід; --interval SECONDS)
./moyva logs --follow   # live-хвіст логів запуску
```

Перший запуск збирає training player автоматично (`./moyva-train build` — вручну).

## Налаштування

`Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json` — джерело правди:

- `environmentCount`: кількість середовищ (1 — єдине допустиме з автономним планом)
- `baseSeed`, `worldSize`, `randomizeWorldSize`, `minWorldSize`/`maxWorldSize`
- `generatorRecipeId`: рецепт генератора карти (`Presets/.../Generator/`)
- `spawnValidationUnitTypeId`, `castleBuildingTypeId`: канонічні id контенту
- `headlessTimeScale`, `trainingTimeScale`, `visualTimeScale`
- `presentationMode`: `HeadlessFast` | `Visual`
- `curriculum.autonomous.*`: `evaluationEverySteps` (10000), `evaluationEpisodes` (50),
  `masteryThreshold` (0.80), `regressionThreshold` (0.65), ваги вибору сценаріїв
- `watchdog*`: стагнація, частка forced-дій, покриття інтентів
- `observer*`: IPC для live-огляду рішень

## Сценарії та навчальний план

`Assets/Moyva/Presets/AI/Scenarios/`:

- `manifest.json` — єдиний порядок курикулуму (`curriculum: [...]`).
- Кожен `*.json` — сценарій: кроки, критерії, `availableCapabilities`,
  `requiredIntents`, `prerequisites`, `fullGame`, `combination`.

Додавання сценарію: створіть JSON, додайте id у `manifest.json` — каталог завантажить,
завалідує (дублікати, відсутні prereq, покриття інтентів можливостями) і включить
у вибірку без змін коду. Сценарій оцінки для frozen evaluation береться зі стану
курикулуму в порядку маніфесту.

## Параметри moyva-train

- `train [--visual|--headless] [--time-scale X] [--seed N] [--world-size N] [--stage N]`
- `--max-steps N`, `--checkpoint-interval N`, `--summary-freq N`
- `--initialize-from <checkpoint>`, `--resume`, `--force`
- `resume --run-id ID` — продовження (contract hash має збігатися)
- `watch|inspect --run-id ID [--checkpoint latest|N] [--scenario ID]` —
  візуальний Model Inspector замороженого чекпоінта (за замовчуванням —
  перший сценарій маніфесту)
- `build`, `doctor`, `tensorboard`, `info`

## Моніторинг наживо

- `./moyva` (без аргументів) — TUI: прогрес, сценарій, метрики, логи.
- `./moyva logs --follow` — потік логів активного запуску; `--source unity|mlagents|warning|error`.
- `./moyva logs <run-id>` — логи конкретного запуску.
- `./moyva-train tensorboard` + `http://localhost:6006` — криві навчання.
- `Results/MoyvaTraining/<run>/curriculum-state.json` — стан освоєння сценаріїв.

## Результати

`Results/MoyvaTraining/<run-id>/`:

- `mlagents.log`, `unity.log`, `cli.log`
- `curriculum-state.json` — освоєння сценаріїв, останній чекпоінт
- `evaluations/` — результати заморожених оцінок з provenance чекпоінта
- `MoyvaStrategy/` — чекпоінти `.onnx` з метаданими контракту

## Вимоги

- Python 3.10.1–3.10.12 (`./moyva setup` створює `.venv-training`)
- ML-Agents 1.1.0, Unity Editor 6000.3.10f1
- Побудований training player (автоматично при першому запуску)

## Вирішення проблем

- **`--arenas rejected`**: автономний план вимагає одну арену — приберіть прапорець
  або вимкніть `curriculum.autonomous.enabled`.
- **`contract hash mismatch`**: чекпоінт навчено під іншою схемою спостережень —
  тренуйте новий; `--force-contract-mismatch` лише для налагодження.
- **`No scenario manifest`**: додайте `manifest.json` або передайте `--scenario`.
- Логи запуску не з'являються: `./moyva doctor`, потім `./moyva processes`.
