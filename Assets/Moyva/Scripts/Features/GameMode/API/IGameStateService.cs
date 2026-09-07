namespace Kruty1918.Moyva.GameMode.API
{
    public enum GameStateType { Idle, Playing, Paused, GameOver }

    public interface IGameStateService
    {
        GameStateType CurrentState { get; }
        void StartGame();
        void PauseGame();
        void ResumeGame();
        void EndGame(string winnerId);
    }

    public interface IGameResultStateStore
    {
        bool IsGameOver { get; }
        string WinnerId { get; }
        void RestoreResult(bool isGameOver, string winnerId);
    }
}
