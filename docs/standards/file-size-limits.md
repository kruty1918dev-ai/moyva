# Ліміти контексту вихідного коду

Ці правила обмежують мінімальний контекст, який потрібен для зміни коду.
Аналізатор рахує raw LOC, non-comment LOC і приблизний обсяг model context.

## Цільові ліміти

- API, installer, controller, coordinator, orchestrator і presenter:
  warning понад 350 effective LOC, hard limit понад 500.
- Інші production leaf-файли:
  warning понад 500 effective LOC, hard limit понад 800.
- Великі цілісні алгоритми дозволяються лише через
  `tools/quality/context-budget-allowlist.json` з локальним лімітом і причиною.
- `Editor`, `Tests` і `Development` не входять у production context budget.

## Як працює guardrail

Основний аналізатор: `tools/quality/context_budget.py`.
Сумісний wrapper: `tools/quality/check-file-length-limits.sh`.

- У CI для Pull Request перевіряються лише змінені C# файли у `Assets/Moyva/Scripts`.
- Існуюче перевищення, яке не виросло, маркується `DEBT`.
- Нове перевищення або збільшення наявного боргу маркується `FAIL`.
- Pull Request запускає changed-only перевірку у strict mode.

## Локальний запуск

```bash
tools/quality/check-file-length-limits.sh --base-ref origin/main --changed-only
```

Строгий режим (падає при нових hard-limit порушеннях):

```bash
tools/quality/check-file-length-limits.sh --base-ref origin/main --changed-only --strict
```

Повний звіт або звіт для одного feature:

```bash
python3 tools/quality/context_budget.py report
python3 tools/quality/context_budget.py report --feature Construction
python3 tools/quality/context_budget.py report \
  --path Assets/Moyva/Scripts/Features/FogOfWar --top 10
```

## Практика декомпозиції

Коли файл росте понад soft-limit, розбиваємо його за ролями:

- orchestration: керує сценарієм, без важкої бізнес-логіки
- domain logic: правила й обчислення
- adapters/infrastructure: зовнішні інтеграції (мережа, I/O, API)
