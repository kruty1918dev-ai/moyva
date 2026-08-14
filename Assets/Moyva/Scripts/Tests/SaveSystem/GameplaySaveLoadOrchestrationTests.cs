using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.SaveSystem;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.SaveSystem
{
    [TestFixture]
    public sealed class GameplaySaveLoadOrchestrationTests
    {
        private abstract class ProbeModule : ISaveModule
        {
            protected ProbeModule(List<string> log, string label)
            {
                Log = log;
                Label = label;
            }

            protected List<string> Log { get; }
            protected string Label { get; }

            public void OnSave(ISaveContext context)
            {
                context.Writer.Write(Label);
            }

            public void OnLoad(ISaveContext context)
            {
                string payload = context.Reader.ReadString();
                Log.Add($"{Label}:{payload}");
            }
        }

        private sealed class EarlyModule : ProbeModule, ISaveModuleExecutionOrder
        {
            public EarlyModule(List<string> log) : base(log, "early") { }
            public int SaveLoadOrder => 10;
        }

        private sealed class LateModule : ProbeModule, ISaveModuleExecutionOrder
        {
            public LateModule(List<string> log) : base(log, "late") { }
            public int SaveLoadOrder => 800;
        }

        private sealed class AlphaPlainModule : ProbeModule
        {
            public AlphaPlainModule(List<string> log) : base(log, "alpha") { }
        }

        private sealed class ZuluPlainModule : ProbeModule
        {
            public ZuluPlainModule(List<string> log) : base(log, "zulu") { }
        }

        [Test]
        public void BuiltInGameplayOrder_IsDependencyAwareAndTurnStateIsLast()
        {
            Assert.Less(
                SaveModuleExecutionPlan.ResolveBuiltInOrder("Kruty1918.Moyva.Generator.Runtime.GeneratedWorldSaveModule"),
                SaveModuleExecutionPlan.ResolveBuiltInOrder("Kruty1918.Moyva.Construction.Runtime.ConstructionSaveModule"));
            Assert.Less(
                SaveModuleExecutionPlan.ResolveBuiltInOrder("Kruty1918.Moyva.Construction.Runtime.ConstructionSaveModule"),
                SaveModuleExecutionPlan.ResolveBuiltInOrder("Kruty1918.Moyva.Economy.Runtime.EconomySaveModule"));
            Assert.Less(
                SaveModuleExecutionPlan.ResolveBuiltInOrder("Kruty1918.Moyva.Economy.Runtime.EconomySaveModule"),
                SaveModuleExecutionPlan.ResolveBuiltInOrder("Kruty1918.Moyva.Bootstrap.Runtime.UnitsSaveModule"));
            Assert.Less(
                SaveModuleExecutionPlan.ResolveBuiltInOrder("Kruty1918.Moyva.Bootstrap.Runtime.UnitsSaveModule"),
                SaveModuleExecutionPlan.ResolveBuiltInOrder("Kruty1918.Moyva.FogOfWar.Runtime.FogOfWarSaveModule"));
            Assert.Less(
                SaveModuleExecutionPlan.ResolveBuiltInOrder("Kruty1918.Moyva.FogOfWar.Runtime.FogOfWarSaveModule"),
                SaveModuleExecutionPlan.ResolveBuiltInOrder("Kruty1918.Moyva.Bootstrap.Runtime.TurnSaveModule"));
        }

        [Test]
        public void ExplicitOrder_WinsOverFallbackOrder()
        {
            var log = new List<string>();
            List<ISaveModule> ordered = SaveModuleExecutionPlan.Build(new ISaveModule[]
            {
                new LateModule(log),
                new EarlyModule(log),
            });

            Assert.IsInstanceOf<EarlyModule>(ordered[0]);
            Assert.IsInstanceOf<LateModule>(ordered[1]);
        }

        [Test]
        public void UnknownModules_AreDeterministicByTypeName()
        {
            var log = new List<string>();
            List<ISaveModule> ordered = SaveModuleExecutionPlan.Build(new ISaveModule[]
            {
                new ZuluPlainModule(log),
                new AlphaPlainModule(log),
            });

            Assert.IsInstanceOf<AlphaPlainModule>(ordered[0]);
            Assert.IsInstanceOf<ZuluPlainModule>(ordered[1]);
        }

        [Test]
        public void DuplicateModuleType_IsIncludedOnlyOnce()
        {
            var log = new List<string>();
            List<ISaveModule> ordered = SaveModuleExecutionPlan.Build(new ISaveModule[]
            {
                new AlphaPlainModule(log),
                new AlphaPlainModule(log),
            });

            Assert.AreEqual(1, ordered.Count);
        }

        [Test]
        public void CollectBlocks_UsesExecutionPlanInsteadOfRegistrationOrder()
        {
            var log = new List<string>();
            var early = new EarlyModule(log);
            var late = new LateModule(log);

            List<(uint blockId, byte[] payload)> blocks = SavePipelineHelper.CollectBlocks(
                new ISaveModule[] { late, early });

            Assert.AreEqual(2, blocks.Count);
            Assert.AreEqual(SaveFileCodec.ComputeBlockId(typeof(EarlyModule)), blocks[0].blockId);
            Assert.AreEqual(SaveFileCodec.ComputeBlockId(typeof(LateModule)), blocks[1].blockId);
        }

        [Test]
        public void ExecuteLoad_IgnoresLegacyPhysicalBlockOrder()
        {
            var log = new List<string>();
            var early = new EarlyModule(log);
            var late = new LateModule(log);

            byte[] bytes = SaveFileCodec.Encode(new List<(uint blockId, byte[] payload)>
            {
                (SaveFileCodec.ComputeBlockId(typeof(LateModule)), WritePayload("encoded-late")),
                (SaveFileCodec.ComputeBlockId(typeof(EarlyModule)), WritePayload("encoded-early")),
            });

            bool loaded = SavePipelineHelper.ExecuteLoad(
                bytes,
                new ISaveModule[] { late, early },
                "reversed legacy test");

            Assert.IsTrue(loaded);
            CollectionAssert.AreEqual(
                new[] { "early:encoded-early", "late:encoded-late" },
                log);
        }

        [Test]
        public void ExecuteLoad_DuplicateBlockIdRejectsBeforeAnyModuleMutation()
        {
            var log = new List<string>();
            var early = new EarlyModule(log);
            uint id = SaveFileCodec.ComputeBlockId(typeof(EarlyModule));

            byte[] bytes = SaveFileCodec.Encode(new List<(uint blockId, byte[] payload)>
            {
                (id, WritePayload("first")),
                (id, WritePayload("second")),
            });

            bool loaded = SavePipelineHelper.ExecuteLoad(
                bytes,
                new ISaveModule[] { early },
                "duplicate block test");

            Assert.IsFalse(loaded);
            Assert.AreEqual(0, log.Count);
        }

        [Test]
        public void UnknownBlocks_DoNotPreventKnownModulesFromRestoring()
        {
            var log = new List<string>();
            var early = new EarlyModule(log);

            byte[] bytes = SaveFileCodec.Encode(new List<(uint blockId, byte[] payload)>
            {
                (0xABCDEF01u, new byte[] { 1, 2, 3 }),
                (SaveFileCodec.ComputeBlockId(typeof(EarlyModule)), WritePayload("known")),
            });

            bool loaded = SavePipelineHelper.ExecuteLoad(
                bytes,
                new ISaveModule[] { early },
                "unknown block test");

            Assert.IsTrue(loaded);
            CollectionAssert.AreEqual(new[] { "early:known" }, log);
        }

        private static byte[] WritePayload(string value)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(value);
            writer.Flush();
            return stream.ToArray();
        }
    }
}
