using System;
using Kruty1918.Moyva.Construction.API;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    // Заглушка для вводу будівельних команд.
    // Реалізація порожня — підключення Input System / UI-кнопок у майбутньому.
    internal sealed class ConstructionInputService : IConstructionInputService, IInitializable, IDisposable
    {
        private readonly IConstructionService _constructionService;

        [Inject]
        public ConstructionInputService(IConstructionService constructionService)
        {
            _constructionService = constructionService;
        }

        public void Initialize()
        {
            // TODO: підключити Unity Input System (Ctrl+Z / Ctrl+Y)
            // або підписатися на UI-кнопки через UnityEvent
        }

        public void Dispose()
        {
            // TODO: відписка від input
        }

        public void OnUndoRequested() => _constructionService.UndoLast();

        public void OnRedoRequested() => _constructionService.RedoLast();
    }
}
