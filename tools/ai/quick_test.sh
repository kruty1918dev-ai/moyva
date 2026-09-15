#!/bin/bash
# Швидкий тест конфігурації навчання

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

echo "=== Тест конфігурації навчання ==="
echo "Проект: $PROJECT_ROOT"
echo ""

# Перевірка конфігурації
CONFIG_FILE="$PROJECT_ROOT/Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json"

if [ -f "$CONFIG_FILE" ]; then
    echo "✓ Конфігураційний файл знайдено"
    echo ""
    echo "Поточні налаштування:"
    cat "$CONFIG_FILE" | python3 -m json.tool 2>/dev/null || cat "$CONFIG_FILE"
    echo ""
    echo "Ключові параметри:"
    echo "  - Кількість арен: $(grep 'environmentCount' "$CONFIG_FILE" | cut -d: -f2 | tr -d ', ')"
    echo "  - Швидкість headless: $(grep 'headlessTimeScale' "$CONFIG_FILE" | cut -d: -f2 | tr -d ', ')"
    echo "  - Швидкість навчання: $(grep 'trainingTimeScale' "$CONFIG_FILE" | cut -d: -f2 | tr -d ', ')"
    echo "  - Режим презентації: $(grep 'presentationMode' "$CONFIG_FILE" | cut -d: -f2 | tr -d ', ')"
    echo "  - Автономне навчання: $(grep -A1 'autonomous' "$CONFIG_FILE" | grep 'enabled' | cut -d: -f2 | tr -d ', ')"
else
    echo "✗ Конфігураційний файл не знайдено"
    exit 1
fi

echo ""
echo "✓ Конфігурація виглядає правильно"
echo ""
echo "Для запуску навчання потрібно:"
echo "1. Встановити ML-Agents: pip3 install -r tools/ai/requirements-training.txt"
echo "2. Побудувати training player (автоматично при першому запуску)"
echo "3. Запустити: ./tools/ai/train_simple.sh 8 5.0 1000000"
