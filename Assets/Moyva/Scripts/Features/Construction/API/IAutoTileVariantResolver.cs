using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IAutoTileVariantResolver
    {
        /// <summary>Усі підтримувані типи топології.</summary>
        IReadOnlyList<TopologyCaseType> SupportedCases { get; }

        /// <summary>
        /// Повертає id варіанту за маскою сусідів і мапою доступних case->id.
        /// Якщо для конкретного кейсу id не задано, сервіс намагається застосувати fallback.
        /// </summary>
        bool TryResolveId(
            TopologyNeighborMask mask,
            IReadOnlyDictionary<TopologyCaseType, string> caseToId,
            out string resolvedId,
            out TopologyCaseType resolvedCase);
    }
}
