#if MOYVA_LEGACY_SCRIPTABLEOBJECT_TESTS
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Construction
{
    public sealed class FirstCastlePerformanceReportTests
    {
        [Test]
        public void CalculateSummary_AcceptsFiveBudgetSignalsWhenWithinThresholds()
        {
            var report = CreateReport(mainMilliseconds: 10f);

            report.CalculateSummary();

            Assert.That(report.Accepted, Is.True);
            Assert.That(report.MainThreadP95Milliseconds, Is.EqualTo(10f));
            Assert.That(report.BuildCommitMaxMilliseconds, Is.EqualTo(8f));
            Assert.That(report.GcCollectionFrames, Is.Zero);
        }

        [Test]
        public void CalculateSummary_ReportsEachReleaseGateViolation()
        {
            var report = CreateReport(mainMilliseconds: 40f);
            CastleFrameSample sample = report.Frames[0];
            sample.BuildCommitMilliseconds = 20f;
            sample.GcCollectionMilliseconds = 1f;
            report.Frames[0] = sample;

            report.CalculateSummary();

            Assert.That(report.Accepted, Is.False);
            Assert.That(report.AcceptanceViolations, Has.Count.EqualTo(4));
            Assert.That(report.AcceptanceViolations[0], Does.Contain("BuildCommit"));
            Assert.That(report.AcceptanceViolations[3], Does.Contain("GC collection"));
        }

        private static FirstCastlePerformanceReport CreateReport(
            float mainMilliseconds)
        {
            var frames = new List<CastleFrameSample>(120);
            for (int index = 0; index < 120; index++)
            {
                frames.Add(new CastleFrameSample
                {
                    Frame = index,
                    MainThreadMilliseconds = mainMilliseconds,
                    RenderThreadMilliseconds = 6f,
                    BuildCommitMilliseconds = 8f,
                    GcAllocatedBytes = 0L,
                });
            }

            return new FirstCastlePerformanceReport
            {
                Frames = frames,
            };
        }
    }
}

#endif
