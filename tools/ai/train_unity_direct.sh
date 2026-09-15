#!/bin/bash
# Прямий запуск навчання через Unity без ML-Agents trainer

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
UNITY_PATH="${MOYVA_UNITY:-/home/oleksiy/Unity/Hub/Editor/6000.3.10f1/Editor/Unity}"

echo "=== Запуск навчання через Unity ==="
echo "Проект: $PROJECT_ROOT"
echo "Unity: $UNITY_PATH"
echo "Налаштування: 8 арен, 5x швидкість"
echo ""

# Запуск Unity в режимі навчання з налаштуваннями
"$UNITY_PATH" \
    -batchmode \
    -nographics \
    -quit \
    -projectPath "$PROJECT_ROOT" \
    -logFile "$PROJECT_ROOT/Temp/ai/training-direct.log" \
    -executeMethod "Kruty1918.Moyva.AI.Training.Editor.TrainingPlayerBuilder.ValidateScope" \
    -moyvaEnvironmentCount 8 \
    -moyvaHeadlessTimeScale 5.0 \
    -moyvaTrainingTimeScale 5.0

echo "Unity навчання завершено"
echo "Перевірте логи: $PROJECT_ROOT/Temp/ai/training-direct.log"
