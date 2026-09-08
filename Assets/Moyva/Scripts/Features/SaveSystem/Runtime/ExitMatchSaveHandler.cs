using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.GameMode.API;

namespace Kruty1918.Moyva.SaveSystem
{
    internal sealed class ExitMatchSaveHandler : IExitMatchSaveHandler
    {
        private readonly ISaveService _saveService;

        public ExitMatchSaveHandler(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public Task SaveBeforeExitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _saveService.Save(GameLaunchContext.SaveSlot);
            return Task.CompletedTask;
        }
    }
}
