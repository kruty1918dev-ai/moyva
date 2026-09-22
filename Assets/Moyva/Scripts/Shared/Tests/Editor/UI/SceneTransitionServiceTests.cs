using System.Threading;
using System.Threading.Tasks;
using Kruty1918.UiFoundation;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Shared.UI.Tests
{
    // P081: one operation owns cover→load→reveal. Nested requests get a
    // join-or-refuse contract, and cover/reveal animations serialize instead
    // of interleaving raycast blocks on the shared overlay.
    public sealed class SceneTransitionServiceTests
    {
        private SceneTransitionService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new SceneTransitionService { AnimationDurationSeconds = 0f };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var leftover in Object.FindObjectsByType<CanvasGroup>(
                         FindObjectsInactive.Include))
                if (leftover != null && leftover.gameObject.name == "MoyvaSceneTransition")
                    Object.DestroyImmediate(leftover.gameObject);
        }

        [Test]
        public void TryBeginTransition_RefusesWhileOwned_ResumesAfterDispose()
        {
            ISceneTransitionLease first = _service.TryBeginTransition();
            Assert.That(first, Is.Not.Null);
            Assert.That(_service.IsTransitionActive, Is.True);

            Assert.That(_service.TryBeginTransition(), Is.Null,
                "a nested transition request must be refused while one is active");

            first.Dispose();
            Assert.That(_service.IsTransitionActive, Is.False);
            Assert.That(_service.TryBeginTransition(), Is.Not.Null,
                "a new transition must succeed once the previous lease released");
        }

        [Test]
        public async Task WaitForActiveTransitionAsync_CompletesWhenLeaseDisposes()
        {
            ISceneTransitionLease lease = _service.TryBeginTransition();
            Task waiter = _service.WaitForActiveTransitionAsync();
            await Task.Yield();
            Assert.That(waiter.IsCompleted, Is.False,
                "joining callers must wait for the active transition");

            lease.Dispose();
            await waiter; // completes, not hangs
            Assert.That(waiter.IsCompletedSuccessfully, Is.True);
        }

        [Test]
        public async Task WaitForActiveTransitionAsync_IdleCompletesImmediately()
        {
            await _service.WaitForActiveTransitionAsync();
        }

        [Test]
        public async Task CoverReveal_SerializeWithoutInterleaving()
        {
            // Fire cover+reveal concurrently: they must queue, not interleave —
            // and settle in the last request's state with a single overlay.
            Task cover = _service.CoverAsync();
            Task reveal = _service.RevealAsync();
            await Task.WhenAll(cover, reveal);

            CanvasGroup overlay = FindOverlay();
            Assert.That(overlay, Is.Not.Null, "the shared overlay must exist after animations");
            Assert.That(overlay.blocksRaycasts, Is.False,
                "a settled reveal must unblock input");
            Assert.That(CountOverlays(), Is.EqualTo(1), "duplicate overlays must never spawn");
        }

        [Test]
        public async Task Lease_CoverThenReveal_DrivesTheSharedOverlay()
        {
            using ISceneTransitionLease lease = _service.TryBeginTransition();
            Assert.That(lease, Is.Not.Null);

            await lease.CoverAsync();
            CanvasGroup overlay = FindOverlay();
            Assert.That(overlay.blocksRaycasts, Is.True, "a settled cover must block input");

            await lease.RevealAsync();
            Assert.That(overlay.blocksRaycasts, Is.False);
            Assert.That(overlay.gameObject.activeSelf, Is.False,
                "a settled reveal deactivates the overlay");
        }

        [Test]
        public async Task RedundantCovers_CompleteWithoutReanimating()
        {
            await _service.CoverAsync();
            CanvasGroup overlay = FindOverlay();
            bool firstScale = overlay.transform.GetChild(0).localScale.y > 0.99f;
            Assert.That(firstScale, Is.True, "covered state means stripes at full scale");

            // Second cover while already covered: completes, no flip-flop.
            await _service.CoverAsync();
            Assert.That(FindOverlay().blocksRaycasts, Is.True);
        }

        private static CanvasGroup FindOverlay()
        {
            foreach (var group in Object.FindObjectsByType<CanvasGroup>(
                         FindObjectsInactive.Include))
                if (group != null && group.gameObject.name == "MoyvaSceneTransition")
                    return group;
            return null;
        }

        private static int CountOverlays()
        {
            int count = 0;
            foreach (var group in Object.FindObjectsByType<CanvasGroup>(
                         FindObjectsInactive.Include))
                if (group != null && group.gameObject.name == "MoyvaSceneTransition")
                    count++;
            return count;
        }
    }
}
