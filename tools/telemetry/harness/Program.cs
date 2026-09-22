using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Telemetry.Core;
using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Fingerprinting;

namespace Kruty1918.Telemetry.Harness
{
    /// <summary>Smoke program — real test suite lives in Tests.cs / ScenarioA.cs.</summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            var tests = new List<(string, Action)>();
            CoreTests.Register(tests);
            ScenarioATests.Register(tests);
            return TestRunner.Run(tests);
        }

        /// <summary>A representative typed event used across the suite.</summary>
        [TelemetryEvent("demo.unitMoved", 1)]
        public struct UnitMovedEvent : ITelemetryEvent
        {
            public string UnitId;
            public int X, Y;
            public float Distance;

            public string EventType => "demo.unitMoved";
            public string ContractId => "demo.unitMoved";
            public int ContractVersion => 1;

            public void WriteTo(ITelemetryEventWriter w)
            {
                w.Field("unitId", UnitId);
                w.Field("x", X);
                w.Field("y", Y);
                w.Field("distance", Distance);
            }
        }

        public static EventContract UnitMovedContract(string unit = "tiles", double speed = 5)
            => new EventContract("demo.unitMoved", 1, "demo.unitMoved", "demo")
            {
                Description = "Unit moved between tiles.",
            }
                .Add(new ContractField("unitId", ContractFieldType.String) { Semantics = "unit instance id" })
                .Add(new ContractField("x", ContractFieldType.Int) { Unit = unit, Min = 0, Max = 4095, Semantics = "tile x" })
                .Add(new ContractField("y", ContractFieldType.Int) { Unit = unit, Min = 0, Max = 4095, Semantics = "tile y" })
                .Add(new ContractField("distance", ContractFieldType.Float) { Unit = unit, Semantics = $"distance at speed {speed}" });

        public static string TempRoot(string name)
        {
            var p = Path.Combine(Path.GetTempPath(), "telemetry-harness", name);
            if (Directory.Exists(p)) Directory.Delete(p, true);
            Directory.CreateDirectory(p);
            return p;
        }
    }
}
