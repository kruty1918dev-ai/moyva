#!/bin/bash
# Паралельне навчання з кількома аренами та підвищеною швидкістю

# Параметри навчання
ARENAS=8              # Кількість паралельних арен
TIME_SCALE=5.0        # Швидкість навчання (1.0 = нормальна, 5.0 = 5x швидше)
WORLD_SIZE=24         # Розмір світу
STAGE=1               # Етап навчання
MAX_STEPS=1000000     # Максимальна кількість кроків
SEED=1918             # Seed для відтворюваності

# Налаштування шляхів
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PYTHON_CMD="${MOYVA_PYTHON:-python3}"

echo "=== Паралельне навчання Moyva ==="
echo "Арен: $ARENAS"
echo "Швидкість: ${TIME_SCALE}x"
echo "Макс. кроків: $MAX_STEPS"
echo "Seed: $SEED"
echo ""

# Запуск навчання
cd "$PROJECT_ROOT"

$PYTHON_CMD tools/ai/moyva_train.py train \
    --arenas $ARENAS \
    --time-scale $TIME_SCALE \
    --world-size $WORLD_SIZE \
    --stage $STAGE \
    --max-steps $MAX_STEPS \
    --seed $SEED \
    --run-id "moyva-parallel-$(date +%Y%m%d-%H%M%S)" \
    --headless
