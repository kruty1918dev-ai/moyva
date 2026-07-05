namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Обробляє запит підтвердження будівництва з пріоритетом.
    /// Перший executor, що повернув true, вважається власником обробки.
    /// </summary>
    public interface IConstructionConfirmRequestExecutor
    {
        int Priority { get; }

        bool TryHandleConfirmRequest();
    }
}
