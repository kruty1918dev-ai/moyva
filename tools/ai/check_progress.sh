#!/bin/bash
# Перевірка поточного прогресу навчання

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

echo "=== Прогрес навчання Moyva ==="
echo ""

# Перевірка стану навчального плану
RESULTS_DIR="$PROJECT_ROOT/Results/MoyvaTraining"
CURRICULUM_STATE="$RESULTS_DIR/curriculum-state.json"

# Якщо файлу немає в корені, спробуй знайти в останньому запуску
if [ ! -f "$CURRICULUM_STATE" ] && [ -d "$RESULTS_DIR" ]; then
    LATEST_STATE=$(find "$RESULTS_DIR" -mindepth 2 -maxdepth 2 -type f -name curriculum-state.json -printf '%T@ %p\n' 2>/dev/null | sort -nr | head -1 | cut -d' ' -f2-)
    if [ -n "$LATEST_STATE" ]; then
        CURRICULUM_STATE="$LATEST_STATE"
    fi
fi

if [ -f "$CURRICULUM_STATE" ]; then
    echo "📋 Поточний стан навчального плану:"
    cat "$CURRICULUM_STATE" | python3 -m json.tool 2>/dev/null || cat "$CURRICULUM_STATE"
    echo ""
    echo "🎯 Активний сценарій:"
    ACTIVE_SCENARIO=$(grep 'activeScenarioId' "$CURRICULUM_STATE" | cut -d: -f2 | tr -d '" ')
    echo "  - ID: $ACTIVE_SCENARIO"
    echo ""
    echo "📊 Статистика:"
    TOTAL_DECISIONS=$(grep 'totalDecisions' "$CURRICULUM_STATE" | cut -d: -f2 | tr -d ', ')
    echo "  - Всього рішень: $TOTAL_DECISIONS"
    echo ""
    echo "🏆 Пройдені сценарії:"
    python3 << PYTHON_SCRIPT
import json
import sys
try:
    with open('$CURRICULUM_STATE', 'r') as f:
        data = json.load(f)
    if 'skills' in data:
        for skill in data['skills']:
            status = '✓ ПРОЙДЕНО' if skill.get('mastered') else '○ в процесі'
            episodes = skill.get('trainingEpisodes', 0)
            successes = skill.get('trainingSuccesses', 0)
            rate = f'{successes}/{episodes}' if episodes > 0 else '0/0'
            print(f'  - {skill["scenarioId"]}: {status} ({rate} епізодів)')
except Exception as e:
    print(f'  Помилка читання: {e}')
PYTHON_SCRIPT
else
    echo "⚠️  Файл стану навчального плану не знайдено"
    echo "  Очікується: $CURRICULUM_STATE"
    echo ""
    echo "Запустіть навчання для створення файлу стану:"
    echo "  ./tools/ai/train_simple.sh 8 5.0 1000000"
fi

echo ""
echo "📁 Перевірте додаткові логи:"
if [ -d "$RESULTS_DIR" ]; then
    echo "  - Результати: $RESULTS_DIR"
    LATEST_RUN=""
    if [ -n "${LATEST_STATE:-}" ]; then
        LATEST_RUN="$(basename "$(dirname "$LATEST_STATE")")"
    else
        LATEST_RUN=$(find "$RESULTS_DIR" -mindepth 2 -maxdepth 2 -type f -name curriculum-state.json -printf '%T@ %h\n' 2>/dev/null | sort -nr | head -1 | cut -d' ' -f2-)
        if [ -n "$LATEST_RUN" ]; then
            LATEST_RUN="$(basename "$LATEST_RUN")"
        fi
    fi
    if [ -n "$LATEST_RUN" ]; then
        echo "  - Останній запуск: $RESULTS_DIR/$LATEST_RUN"
        if [ -f "$RESULTS_DIR/$LATEST_RUN/mlagents.log" ]; then
            echo "  - ML-Agents лог: $RESULTS_DIR/$LATEST_RUN/mlagents.log"
        fi
        if [ -f "$RESULTS_DIR/$LATEST_RUN/unity.log" ]; then
            echo "  - Unity лог: $RESULTS_DIR/$LATEST_RUN/unity.log"
        fi
    fi
else
    echo "  - Директорія результатів ще не створена"
fi
