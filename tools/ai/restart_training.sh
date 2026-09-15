#!/bin/bash
# Перезапуск навчання з новими налаштуваннями

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

echo "=== Перезапуск навчання з новими налаштуваннями ==="
echo ""

# Зупинити поточне навчання
echo "Зупинення поточного навчання..."
pkill -f "mlagents.trainers.learn" 2>/dev/null
pkill -f "MoyvaTraining" 2>/dev/null
sleep 2

# Резервний бекап поточного стану
RESULTS_DIR="$PROJECT_ROOT/Results/MoyvaTraining"
if [ -d "$RESULTS_DIR" ]; then
    BACKUP_DIR="$RESULTS_DIR.backup-$(date +%Y%m%d-%H%M%S)"
    echo "Резервне копіювання поточного стану: $BACKUP_DIR"
    cp -r "$RESULTS_DIR" "$BACKUP_DIR"
fi

# Очистити стан навчального плану
CURRICULUM_STATE="$RESULTS_DIR/curriculum-state.json"
if [ -f "$CURRICULUM_STATE" ]; then
    echo "Очищення стану навчального плану"
    rm "$CURRICULUM_STATE"
fi

echo ""
echo "Запуск нового навчання з оновленими налаштуваннями"
echo "Налаштування foundation-legal-setup: 5 епізодів, макс. 5 рішень"
echo ""

# Запуск нового навчання
cd "$PROJECT_ROOT"
./tools/ai/train_simple.sh 8 5.0 1000000
