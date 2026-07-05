namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Заглушка для вводу будівельних команд (Ctrl+Z / Ctrl+Y / кнопки).
    /// Реалізація порожня поки Input System не підключено.
    /// </summary>
    public interface IConstructionInputService
    {
        /// <summary>Скасувати останнє розміщення (Ctrl+Z або кнопка Undo).</summary>
        void OnUndoRequested();

        /// <summary>Повернути скасоване розміщення (Ctrl+Y або кнопка Redo).</summary>
        void OnRedoRequested();
    }
}
