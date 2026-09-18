namespace Kruty1918.Moyva.Interactions.API
{
    public interface ITileInteractionService
    {
        // Можна викликати ззовні (наприклад, з UI або системи вводу)
        void HandleTileClick(UnityEngine.Vector2Int position);

        /// <summary>Whether group-merge mode is armed for the selected unit.</summary>
        bool IsGroupMergeArmed { get; }

        /// <summary>
        /// Toggles group-merge mode for the currently selected unit. While
        /// armed, the next click on another own unit merges it with the
        /// selected unit into one group (joining the target's group when it
        /// already belongs to one). Returns false when no own unit is selected.
        /// </summary>
        bool ToggleGroupMergeArm();

        /// <summary>
        /// Disbands the group the currently selected unit belongs to.
        /// Returns false when the selected unit is not in a group.
        /// </summary>
        bool TryDisbandSelectedGroup();
    }
}
