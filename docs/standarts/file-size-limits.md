# Ліміти розміру файлів (soft-limit)

Ці правила потрібні, щоб прискорити рев'ю, зменшити складність змін і полегшити масштабування модулів.

## Цільові ліміти

- Runtime-класи (шлях містить `/Runtime/`):
- soft warning: понад 300 рядків
- hard guardrail: понад 500 рядків

- Editor-вікна (клас успадковує `EditorWindow`):
- soft warning: понад 700 рядків
- hard guardrail: понад 900 рядків

## Як працює guardrail

Скрипт: `tools/quality/check-file-length-limits.sh`

- У CI для Pull Request перевіряються лише змінені C# файли у `Assets/Moyva/Scripts`.
- Якщо файл уже був понад hard-limit у базовій гілці, це маркується як `DEBT` (warning), а не фейл.
- Якщо новий або змінений файл вперше перевищив hard-limit — це маркується як `FAIL`.
- За замовчуванням перевірка працює в soft-режимі і не блокує build.
- Для блокування використовуйте `--strict`.

## Локальний запуск

```bash
tools/quality/check-file-length-limits.sh --base-ref origin/main --changed-only
```

Строгий режим (падає при нових hard-limit порушеннях):

```bash
tools/quality/check-file-length-limits.sh --base-ref origin/main --changed-only --strict
```

Або повний аудит:

```bash
tools/quality/check-file-length-limits.sh
```

## Практика декомпозиції

Коли файл росте понад soft-limit, розбиваємо його за ролями:

- orchestration: керує сценарієм, без важкої бізнес-логіки
- domain logic: правила й обчислення
- adapters/infrastructure: зовнішні інтеграції (мережа, I/O, API)
