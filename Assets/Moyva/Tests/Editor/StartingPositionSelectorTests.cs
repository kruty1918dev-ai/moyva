using System;
using System.Collections.Generic;
using System.Reflection;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Startup
{
    public sealed class StartingPositionSelectorTests
    {
        private static readonly Type SelectorType = typeof(StartingPositionInitializerSettings)
            .Assembly.GetType("Kruty1918.Moyva.Bootstrap.Runtime.StartingPositionSelector", true);

        [Test]
        public void DistantCandidatesMeetMinimumWithoutSearchingRoutes()
        {
            var routes = new CountingPathfinder();
            object selector = CreateSelector(routes);
            Assert.That(Invoke<bool>(selector, "HasRequiredDistance",
                new Vector2Int(80, 80), new[] { Vector2Int.zero }), Is.True);
            Assert.That(routes.Calls, Is.Zero);
        }

        [TestCase(10, false)]
        [TestCase(40, true)]
        public void NearbyCandidatesStillUseActualRouteLength(int length, bool expected)
        {
            var routes = new CountingPathfinder { ForcedLength = length };
            object selector = CreateSelector(routes);
            Assert.That(Invoke<bool>(selector, "HasRequiredDistance",
                new Vector2Int(10, 0), new[] { Vector2Int.zero }), Is.EqualTo(expected));
            Assert.That(routes.Calls, Is.EqualTo(1));
        }

        [Test]
        public void UnreachableNearbyCandidateKeepsEuclideanFallback()
        {
            var routes = new CountingPathfinder { ForcedLength = -1 };
            object selector = CreateSelector(routes);
            Assert.That(Invoke<bool>(selector, "HasRequiredDistance",
                new Vector2Int(24, 24), new[] { Vector2Int.zero }), Is.True);
            Assert.That(routes.Calls, Is.EqualTo(1));
        }

        [Test]
        public void SeparationScoreUsesClosestPlayerAndSkipsSaturatedRoutes()
        {
            var routes = new CountingPathfinder();
            object selector = CreateSelector(routes);
            Assert.That(Invoke<int>(selector, "ScoreInterPlayerSeparation",
                Vector2Int.zero, new[] { new Vector2Int(100, 100), new Vector2Int(10, 0) }),
                Is.EqualTo(60));
            Assert.That(routes.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Exhaustive128MapKeepsWinnerWithoutRoutingEveryTile()
        {
            var routes = new CountingPathfinder();
            object selector = CreateSelector(routes);
            var signal = new WorldGeneratedDataSignal { Width = 128, Height = 128 };
            object[] arguments = { signal, new[] { Vector2Int.zero }, false, false, null };

            Assert.That(Invoke<bool>(selector, "TryPickBestEffortPosition", arguments), Is.True);
            // Uniform terrain: the first coordinate with maximum separation
            // wins, exactly as in a full scan that routes all 16,383 candidates.
            Assert.That((Vector2Int)arguments[4], Is.EqualTo(new Vector2Int(0, 50)));
            Assert.That(routes.Calls, Is.LessThanOrEqualTo(49));
        }

        [Test]
        public void ScorePruningPreservesCoordinateTieBreak()
        {
            MethodInfo method = SelectorType.GetMethod("CanImproveScore",
                BindingFlags.Static | BindingFlags.NonPublic);
            var best = new Vector2Int(5, 5);
            Assert.That((bool)method.Invoke(null,
                new object[] { new Vector2Int(4, 5), 1000, true, best, 1300 }), Is.True);
            Assert.That((bool)method.Invoke(null,
                new object[] { new Vector2Int(6, 5), 1000, true, best, 1300 }), Is.False);
            Assert.That((bool)method.Invoke(null,
                new object[] { Vector2Int.zero, 999, true, best, 1300 }), Is.False);
        }

        private static object CreateSelector(IPathfinder routes)
            => Activator.CreateInstance(SelectorType, new object[]
            {
                new StartingPositionInitializerSettings
                {
                    minAStarDistanceBetweenPlayers = 30,
                    minMarginFromBorder = 0,
                    relativeMarginFactor = 0,
                    preferWaterNearStart = false
                },
                routes
            });

        private static T Invoke<T>(object selector, string name, params object[] arguments)
            => (T)SelectorType.GetMethod(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Invoke(selector, arguments);

        private sealed class CountingPathfinder : IPathfinder
        {
            public int Calls;
            public int? ForcedLength;

            public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
            {
                Calls++;
                int length = ForcedLength ?? Mathf.Max(
                    Mathf.Abs(start.x - end.x), Mathf.Abs(start.y - end.y));
                var path = new List<Vector2Int>();
                for (int i = 0; i <= length; i++)
                    path.Add(start);
                return path;
            }

            public IEnumerable<Vector2Int> GetNeighbors(Vector2Int position)
                => Array.Empty<Vector2Int>();
        }
    }
}
