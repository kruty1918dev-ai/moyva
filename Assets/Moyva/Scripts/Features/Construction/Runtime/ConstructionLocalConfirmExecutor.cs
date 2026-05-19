using Kruty1918.Moyva.Construction.API;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionLocalConfirmExecutor : IConstructionConfirmRequestExecutor
    {
        private readonly IConstructionService _constructionService;

        public ConstructionLocalConfirmExecutor(IConstructionService constructionService)
        {
            _constructionService = constructionService;
        }

        public int Priority => 0;

        public bool TryHandleConfirmRequest()
        {
            _constructionService.Confirm();
            return true;
        }
    }
}
