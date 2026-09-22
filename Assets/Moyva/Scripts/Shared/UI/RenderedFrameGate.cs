using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Kruty1918.Moyva.Shared.UI
{
    /// <summary>
    /// Чесний перший кадр для стартових переходів: завершується лише після того,
    /// як кадр із loading UI реально пройшов render-прохід. Один Task.Yield
    /// відновлюється до рендеру, тож не гарантує видимого кадру.
    /// </summary>
    internal static class RenderedFrameGate
    {
        public static async Task NextAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // EditMode не має render-циклу — виходимо на наступному
                // editor update замість зависання в EndOfFrame.
                await Task.Yield();
                return;
            }
#endif
            await Awaitable.EndOfFrameAsync(ct);
        }
    }
}
