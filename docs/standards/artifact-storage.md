# Зберігання артефактів і моделей

Мета: Git містить лише source і обов'язкові runtime assets. Відтворювані builds,
training results, checkpoints і marketing output живуть поза репозиторієм.

## Інвентар tracked-артефактів (стан до cleanup, гілка chore/repository-artifact-hygiene)

| Шлях | Файлів | Розмір | Категорія | Куди |
|---|---:|---:|---|---|
| `Results/MoyvaTraining.backup-*/` | 1130 | ~1354 MB | тимчасові training results + evaluation builds | artifact storage |
| `Exports/Models/*.zip` + `.sha256` | 9 | ~103 MB | release artifacts (пакети моделей) | GitHub Releases |
| `MarketingOutput/` | 348 | ~40 MB | marketing output (screenshots, video masters) | artifact storage |
| `Assets/Moyva/Presets/AI/Resources/AI/Models/MoyvaStrategy_Normal.onnx` | 1 | ~2.6 MB | обов'язковий runtime asset | **Git** |
| `Assets/Plugins/**/*.dll` (Sirenix, DOTween, Zenject) | 20 | ~10 MB | vendored dependencies | **Git** |
| `Assets/ThirdParty/**` (текстури, аудіо, fbx) | — | ~1.3 GB | vendored game assets | **Git** |
| `.moyva_patch_state.json` | 1 | <1 KB | machine-local state | не в Git |

`Results/MoyvaTraining.backup-*` — це три копії тих самих запусків: чекпоінти
`.pt`/`.onnx`, evaluation player builds (`UnityPlayer.so`, `MoyvaEvaluation_Data/`),
run logs, telemetry, конфіги. Повністю відтворювані через `./moyva-train train`.

## Категорії та призначення

- **Обов'язкові runtime assets** → Git: production ONNX у
  `Presets/AI/Resources/AI/Models/` (завантажується через `Resources`,
  `modelResourcePath` у `MoyvaBotDifficultyRegistry.json`), vendored plugin DLLs,
  ThirdParty assets.
- **Потрібні pretrained models** → Git, якщо ≤ ~10 MB і потрібні runtime;
  більші — Git LFS (окреме рішення, наразі немає потреби).
- **Release artifacts** (`Exports/Models/*.zip`) → GitHub Releases репозиторію.
- **Тимчасові training results** (`Results/`) → локально + зовнішнє artifact
  storage (диск/хмара); у Git не потрапляють.
- **Marketing output** (`MarketingOutput/`) → зовнішнє artifact storage;
  відтворювано Marketing Content Studio (`./moyva` → studio).
- **Повністю відтворювані builds/logs** → не зберігаються в Git ніде.

## Що зроблено у гілці cleanup

1. `.gitignore`: `/Results/` (ціле дерево, не лише `MoyvaTraining`), `/Exports/`,
   `*.pt`, `*.pth`, `*.ckpt`, `/.moyva_patch_state.json`, `/nul`.
   `/MarketingOutput/` і `/Build/` уже були ігноровані.
2. `git rm -r --cached` для `Results/`, `Exports/`, `MarketingOutput/`,
   `.moyva_patch_state.json`. Локальні файли залишились на диску.
3. CI: `.github/workflows/artifact-hygiene.yml` +
   `tools/quality/check-artifact-hygiene.sh` блокують повторний коміт builds,
   DLLs, checkpoints і training logs у PR.
4. Production `MoyvaStrategy_Normal.onnx` **не чіпався** — він у Git як runtime
   asset. Окреме підтвердження потрібне для будь-якої його заміни/видалення.

## Як зберігати й відновлювати

### Моделі (Exports → GitHub Releases)

```bash
./pack-model <run-id>                 # Results/MoyvaTraining/<run> -> Exports/Models/*.zip + .sha256
gh release create model-<run-id> Exports/Models/<file>.zip Exports/Models/<file>.zip.sha256
```

Відновлення production-моделі з релізу:

```bash
gh release download model-<run-id> -p '*.zip' --dir Temp/
unzip Temp/<file>.zip -d Temp/model/
cp Temp/model/<run>/run/MoyvaStrategy/MoyvaStrategy-*.onnx \
   "Assets/Moyva/Presets/AI/Resources/AI/Models/MoyvaStrategy_Normal.onnx"
```

Після копіювання Unity імпортує `.onnx` як `ModelAsset`; шлях ресурсу має
збігатися з `modelResourcePath` у `MoyvaBotDifficultyRegistry.json`.

### Training results (Results → artifact storage)

`Results/` — локальний вихід `./moyva-train` і `./moyva`. Для довгострокового
зберігання архівуйте вибрані запуски у зовнішнє сховище:

```bash
tar -czf moyva-run-<run-id>.tar.gz -C Results/MoyvaTraining <run-id>
# перенесіть архів на зовнішній диск / у cloud bucket
```

Відновлення для resume/inspect: розпакувати назад у
`Results/MoyvaTraining/<run-id>/` — `./moyva-train resume --run-id <id>` і
`inspect` працюють з цим деревом.

### Marketing output

`MarketingOutput/` генерується Marketing Content Studio на вимогу. Готові
masters/скріншоти, які треба зберегти, переносьте у зовнішнє сховище
(cloud drive / asset storage) за run-папками (`<date>_<recipe>/`).
Видалення папки безпечне — studio відтворить output із рецептів у
`Assets/Moyva/Presets/Marketing/`.

## Очищення старої історії (вручну, не в цій гілці)

Поточний cleanup прибирає артефакти лише з HEAD. ~1.5 GB бінарників лишаються
у Git-історії. Очищення історії переписує коміти і вимагає force-push +
re-clone всією командою — виконувати лише після узгодження:

```bash
# 1. Мирор-клон
git clone --mirror https://github.com/kruty1918dev-ai/moyva.git moyva-mirror.git
cd moyva-mirror.git

# 2. git filter-repo (https://github.com/newren/git-filter-repo)
git filter-repo --invert-paths \
  --path Results --path Exports --path MarketingOutput \
  --path-glob '*.pt' --path-glob '*.pth' --path-glob '*.ckpt'

# 3. Перевірка розміру, потім force-push за планом команди
git count-objects -vH
```

Після force-push усі клонують репозиторій наново; старі клони не push'ать.
