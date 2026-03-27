namespace Kruty1918.Moyva.Interactions.API
{
    public interface ITileInteractionService
    {
        // Можна викликати ззовні (наприклад, з UI або системи вводу)
        void HandleTileClick(UnityEngine.Vector2Int position);
    }
}