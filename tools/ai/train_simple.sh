#!/bin/bash
# Простий скрипт для швидкого запуску навчання з параметрами

# Використання: ./train_simple.sh [арени] [швидкість] [макс_кроки]

ARENAS=${1:-8}
TIME_SCALE=${2:-5.0}
MAX_STEPS=${3:-1000000}

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PYTHON_CMD="${MOYVA_PYTHON:-python3}"

echo "=== Швидке навчання Moyva ==="
echo "Арен: $ARENAS"
echo "Швидкість: ${TIME_SCALE}x"
echo "Макс. кроків: $MAX_STEPS"
echo ""

cd "$PROJECT_ROOT"

$PYTHON_CMD tools/ai/moyva_train.py train \
    --arenas $ARENAS \
    --time-scale $TIME_SCALE \
    --max-steps $MAX_STEPS \
    --headless
