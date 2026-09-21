using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Kruty1918.Moyva.Signals.Tests
{
    /// <summary>
    /// Architecture guard: gameplay signals ARE the single event layer.
    /// The parallel `*DomainEvent` mirror (`Kruty1918.Moyva.Signals.DomainEvents`
    /// namespace + `SignalDomainEventBridge`) was removed — subscribers consume
    /// canonical `*Signal` types directly. This test fails if the parallel
    /// layer is reintroduced.
    /// </summary>
    public sealed class SignalsArchitectureTests
    {
        [Test]
        public void NoDomainEventTypes_InAnyMoyvaAssembly()
        {
            var offenders = MoyvaTypes()
                .Where(t => t.Name.EndsWith("DomainEvent", StringComparison.Ordinal)
                    || (t.Namespace != null
                        && t.Namespace.EndsWith(".DomainEvents", StringComparison.Ordinal)))
                .Select(t => t.FullName)
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToArray();

            Assert.IsEmpty(offenders,
                "Parallel domain-event layer detected; subscribe to canonical *Signal types instead: "
                + string.Join(", ", offenders));
        }

        [Test]
        public void NoSignalMirrorBridge_InAnyMoyvaAssembly()
        {
            var bridges = MoyvaTypes()
                .Where(t => t.Name.EndsWith("DomainEventBridge", StringComparison.Ordinal)
                    || t.Name == "SignalDomainEventBridge")
                .Select(t => t.FullName)
                .ToArray();

            Assert.IsEmpty(bridges,
                "Signal→DomainEvent mirror bridge detected: " + string.Join(", ", bridges));
        }

        private static IEnumerable<Type> MoyvaTypes()
        {
            foreach (var assembly in UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies())
            {
                if (!assembly.GetName().Name.StartsWith("Kruty1918.Moyva", StringComparison.Ordinal))
                    continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                }

                foreach (var type in types)
                    yield return type;
            }
        }
    }
}
