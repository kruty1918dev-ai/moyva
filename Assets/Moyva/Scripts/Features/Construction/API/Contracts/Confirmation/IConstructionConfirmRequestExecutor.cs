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

    /// <summary>
    /// Connects a scene-scoped construction service to a longer-lived
    /// authority/router without making the construction assembly depend on a
    /// concrete multiplayer implementation.
    /// </summary>
    public interface IConstructionAuthorityEndpointRegistry
    {
        void Attach(IConstructionService constructionService);

        void Detach(IConstructionService constructionService);
    }
}
