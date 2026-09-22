using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Shared.UI;
using NUnit.Framework;

namespace Kruty1918.Moyva.Shared.UI.Tests
{
    // P082: the gate must be a real frame boundary — if it completed inline,
    // synchronous init would still preempt the first loading frame.
    public sealed class RenderedFrameGateTests
    {
        [Test]
        public void NextAsync_DoesNotCompleteSynchronously()
        {
            Task task = RenderedFrameGate.NextAsync();
            Assert.That(task.IsCompleted, Is.False,
                "the gate completed inline — no frame boundary before heavy init");
        }

        [Test]
        public void NextAsync_CompletesOnNextEditorUpdate()
        {
            // In EditMode there is no render pass; the gate falls back to the
            // next editor update. Awaiting must not hang.
            Task task = RenderedFrameGate.NextAsync();
            Assert.DoesNotThrowAsync(async () => await task);
        }

        [Test]
        public void NextAsync_ThrowsOnPrecancelledToken()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.ThrowsAsync<System.OperationCanceledException>(
                async () => await RenderedFrameGate.NextAsync(cts.Token));
        }
    }
}
