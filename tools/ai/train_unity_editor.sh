#!/bin/bash
# Запуск навчання з Unity редактора (для тестування)

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
UNITY_PATH="${MOYVA_UNITY:-/home/oleksiy/Unity/Hub/Editor/6000.3.10f1/Editor/Unity}"

echo "=== Запуск навчання з Unity редактора ==="
echo "Проект: $PROJECT_ROOT"
echo "Unity: $UNITY_PATH"
echo ""

# Запуск Unity в batch mode для компіляції та підготовки
"$UNITY_PATH" \
    -batchmode \
    -nographics \
    -quit \
    -projectPath "$PROJECT_ROOT" \
    -logFile "$PROJECT_ROOT/Temp/ai/training-start.log" \
    -executeMethod "Kruty1918.Moyva.AI.Training.Editor.TrainingPlayerBuilder.Build" \
    -moyvaBuildTarget "StandaloneLinux64" \
    -moyvaBuildOutput "$PROJECT_ROOT/Build/Training/MoyvaTraining.x86_64"

if [ $? -eq 0 ]; then
    echo "Unity build успішно завершено"
    echo "Запустіть ./tools/ai/train_parallel.sh для паралельного навчання"
else
    echo "Помилка Unity build"
    exit 1
fi
