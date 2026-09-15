#!/bin/bash
# Запуск Unity сцени навчання для тестування конфігурації

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
UNITY_PATH="${MOYVA_UNITY:-/home/oleksiy/Unity/Hub/Editor/6000.3.10f1/Editor/Unity}"

echo "=== Запуск Unity сцени навчання ==="
echo "Проект: $PROJECT_ROOT"
echo "Unity: $UNITY_PATH"
echo "Налаштування конфігурації: 8 арен, 5x швидкість"
echo ""

# Запуск Unity з відкритою сценою навчання
"$UNITY_PATH" \
    -projectPath "$PROJECT_ROOT" \
    -logFile "$PROJECT_ROOT/Temp/ai/training-scene.log" \
    -quit \
    -batchmode \
    -nographics \
    -executeMethod "EditorApplication.OpenScene" \
    -scenePath "Assets/Moyva/AI/Training/Scenes/MoyvaTraining.unity"

echo "Сцена навчання відкрита"
echo "Перевірте конфігурацію в Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json"
echo "Потім запустіть сцену в Unity для тестування"
