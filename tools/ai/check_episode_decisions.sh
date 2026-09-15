#!/bin/bash
# Перевірити скільки рішень робить модель в епізодах

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

echo "=== Аналіз рішень в епізодах ==="
echo ""

RESULTS_DIR="$PROJECT_ROOT/Results/MoyvaTraining"
LATEST_RUN=$(ls -t "$RESULTS_DIR" 2>/dev/null | head -1)

if [ -n "$LATEST_RUN" ]; then
    LOG_FILE="$RESULTS_DIR/$LATEST_RUN/mlagents.log"
    echo "Лог файл: $LOG_FILE"
    echo ""

    # Шукаємо записи про епізоди та рішення
    echo "📊 Статистика епізодів:"
    grep -i "episode\|decision\|step" "$LOG_FILE" | tail -20

    echo ""
    echo "🔍 Шукаємо записи про ScenarioSuccess:"
    grep -i "scenario\|success" "$LOG_FILE" | tail -10

    echo ""
    echo "📈 Останні кроки навчання:"
    grep "Step:" "$LOG_FILE" | tail -5
else
    echo "⚠️  Не знайдено запущеного навчання"
fi
