using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.SaveSystem
{
    /// <summary>
    /// Builds one deterministic execution plan for both save capture and restore.
    /// Known gameplay modules receive dependency-aware phases while unknown modules
    /// fall back to a stable type-name order. Explicit module order always wins.
    /// </summary>
    internal static class SaveModuleExecutionPlan
    {
        internal const int GeneratedWorldOrder = 100;
        internal const int ConstructionOrder = 200;
        internal const int EconomyOrder = 300;
        internal const int BootstrapStateOrder = 350;
        internal const int UnitsOrder = 400;
        internal const int LegacyRecruitmentOrder = 450;
        internal const int FogOfWarOrder = 500;
        internal const int DefaultOrder = 600;
        internal const int TurnStateOrder = 900;

        private const string GeneratedWorldModule =
            "Kruty1918.Moyva.Generator.Runtime.GeneratedWorldSaveModule";
        private const string ConstructionModule =
            "Kruty1918.Moyva.Construction.Runtime.ConstructionSaveModule";
        private const string EconomyModule =
            "Kruty1918.Moyva.Economy.Runtime.EconomySaveModule";
        private const string BootstrapStarterPackModule =
            "Kruty1918.Moyva.Bootstrap.Runtime.BootstrapStarterPackSaveModule";
        private const string UnitsModule =
            "Kruty1918.Moyva.Bootstrap.Runtime.UnitsSaveModule";
        private const string LegacyRecruitmentModule =
            "Kruty1918.Moyva.Recruitment.RecruitmentService";
        private const string FogOfWarModule =
            "Kruty1918.Moyva.FogOfWar.Runtime.FogOfWarSaveModule";
        private const string TurnModule =
            "Kruty1918.Moyva.Bootstrap.Runtime.TurnSaveModule";

        private readonly struct Candidate
        {
            public Candidate(ISaveModule module, int order, string typeName)
            {
                Module = module;
                Order = order;
                TypeName = typeName;
            }

            public ISaveModule Module { get; }
            public int Order { get; }
            public string TypeName { get; }
        }

        internal static List<ISaveModule> Build(IReadOnlyList<ISaveModule> modules)
        {
            if (modules == null || modules.Count == 0)
                return new List<ISaveModule>();

            var candidates = new List<Candidate>(modules.Count);
            var seenTypes = new HashSet<Type>();

            for (int index = 0; index < modules.Count; index++)
            {
                ISaveModule module = modules[index];
                if (module == null)
                    continue;

                Type type = module.GetType();
                if (!seenTypes.Add(type))
                {
                    Debug.LogWarning(
                        $"[SaveSystem] Duplicate save module type '{type.FullName ?? type.Name}' " +
                        "was registered more than once. Only one instance will participate in the pipeline.");
                    continue;
                }

                string typeName = type.FullName ?? type.Name;
                candidates.Add(new Candidate(module, ResolveOrder(module, typeName), typeName));
            }

            candidates.Sort(CompareCandidates);

            var result = new List<ISaveModule>(candidates.Count);
            for (int index = 0; index < candidates.Count; index++)
                result.Add(candidates[index].Module);
            return result;
        }

        internal static int ResolveBuiltInOrder(string fullTypeName)
        {
            if (string.Equals(fullTypeName, GeneratedWorldModule, StringComparison.Ordinal))
                return GeneratedWorldOrder;
            if (string.Equals(fullTypeName, ConstructionModule, StringComparison.Ordinal))
                return ConstructionOrder;
            if (string.Equals(fullTypeName, EconomyModule, StringComparison.Ordinal))
                return EconomyOrder;
            if (string.Equals(fullTypeName, BootstrapStarterPackModule, StringComparison.Ordinal))
                return BootstrapStateOrder;
            if (string.Equals(fullTypeName, UnitsModule, StringComparison.Ordinal))
                return UnitsOrder;
            if (string.Equals(fullTypeName, LegacyRecruitmentModule, StringComparison.Ordinal))
                return LegacyRecruitmentOrder;
            if (string.Equals(fullTypeName, FogOfWarModule, StringComparison.Ordinal))
                return FogOfWarOrder;
            if (string.Equals(fullTypeName, TurnModule, StringComparison.Ordinal))
                return TurnStateOrder;
            return DefaultOrder;
        }

        private static int ResolveOrder(ISaveModule module, string typeName)
        {
            if (module is ISaveModuleExecutionOrder ordered)
                return ordered.SaveLoadOrder;
            return ResolveBuiltInOrder(typeName);
        }

        private static int CompareCandidates(Candidate left, Candidate right)
        {
            int byOrder = left.Order.CompareTo(right.Order);
            if (byOrder != 0)
                return byOrder;
            return string.CompareOrdinal(left.TypeName, right.TypeName);
        }
    }
}
