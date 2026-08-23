using Kruty1918.Moyva.BotAI.Runtime;
using Kruty1918.Moyva.Construction.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI
{
    public sealed class BotPlacementProbeDiagnosticsTests
    {
        [Test]
        public void LegalButUnaffordable_IsNotClassifiedAsIllegal()
        {
            var legality =
                new ConstructionPlacementQueryResult(
                    availabilityValid: true,
                    spatialValid: true,
                    resourcesValid: true,
                    authorityValid: true,
                    isGateReplacement: false,
                    reason: null,
                    evaluationResult: null,
                    diagnostic: null);

            var commit =
                new ConstructionPlacementQueryResult(
                    availabilityValid: true,
                    spatialValid: true,
                    resourcesValid: false,
                    authorityValid: true,
                    isGateReplacement: false,
                    reason: "resources",
                    evaluationResult: null,
                    diagnostic: null);

            var probe =
                new BotPlacementProbe(
                    legality,
                    commit);

            Assert.That(probe.IsLegal, Is.True);
            Assert.That(probe.IsAffordable, Is.False);
            Assert.That(probe.CanCommit, Is.False);

            var diagnostics =
                new BotPlacementRejectionAccumulator();

            diagnostics.Observe(probe);

            Assert.That(diagnostics.Legal, Is.EqualTo(1));
            Assert.That(
                diagnostics.ResourceRejected,
                Is.EqualTo(1));

            Assert.That(
                diagnostics.SpatialRejected,
                Is.Zero);
        }
    }
}
