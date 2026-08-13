using System;
using System.Linq;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using NUnit.Framework;
using Zenject;

namespace Kruty1918.Moyva.Tests.BotAI
{
    public sealed class BotArchitectureCleanupTests
    {
        [Test]
        public void LegacyScheduler_IsNotAZenjectLifecycleService()
        {
            Type scheduler = typeof(BotTickScheduler);

            Assert.That(typeof(IInitializable).IsAssignableFrom(scheduler), Is.False);
            Assert.That(typeof(ITickable).IsAssignableFrom(scheduler), Is.False);
        }

        [Test]
        public void LegacyScheduler_CompatibilityMethodsAreInert()
        {
            var scheduler = new BotTickScheduler(null, null, null);

            Assert.That(scheduler.IsRuntimeEnabled, Is.False);
            Assert.DoesNotThrow(scheduler.Initialize);
            Assert.DoesNotThrow(scheduler.Tick);
            Assert.That(scheduler.IsRuntimeEnabled, Is.False);
        }

        [Test]
        public void TurnExecutorContract_ExposesOneTurnScopedBeginBoundary()
        {
            var methods = typeof(IBotTurnExecutor).GetMethods();
            Assert.That(methods.Select(method => method.Name), Is.EquivalentTo(new[] { "TryBeginTurn" }));

            var method = methods.Single();
            Assert.That(method.ReturnType, Is.EqualTo(typeof(bool)));
            var parameters = method.GetParameters();
            Assert.That(parameters.Length, Is.EqualTo(3));
            Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(string)));
            Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(long)));
            Assert.That(parameters[2].IsOut, Is.True);
            Assert.That(parameters[2].ParameterType, Is.EqualTo(typeof(string).MakeByRefType()));
        }
    }
}
