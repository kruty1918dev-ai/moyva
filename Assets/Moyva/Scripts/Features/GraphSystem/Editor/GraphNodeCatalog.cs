using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    public sealed class NodeCatalogEntry
    {
        public NodeCatalogEntry(NodeDescriptor descriptor, Type twcModifierType = null)
        {
            Descriptor = descriptor;
            TwcModifierType = twcModifierType;
        }

        public NodeDescriptor Descriptor { get; }
        public Type TwcModifierType { get; }
        public string Path => $"{Descriptor.Category}/{Descriptor.Title}";
        public bool IsTwcModifier => TwcModifierType != null;
        public override string ToString() => Descriptor?.Title ?? "<Invalid Node>";
    }

    /// <summary>
    /// Єдине джерело metadata для меню, фабрики, документації та contract-тестів.
    /// </summary>
    public static class GraphNodeCatalog
    {
        private static IReadOnlyList<NodeCatalogEntry> _entries;

        public static IReadOnlyList<NodeCatalogEntry> Entries =>
            _entries ??= Build();

        public static IEnumerable<NodeCatalogEntry> CreatableEntries =>
            Entries.Where(entry => entry?.Descriptor?.IsCreatable == true);

        public static bool TryGet(Type nodeType, out NodeCatalogEntry entry)
        {
            entry = Entries.FirstOrDefault(candidate =>
                !candidate.IsTwcModifier
                && candidate.Descriptor.NodeType == nodeType);
            return entry != null;
        }

        public static bool TryGet(string stableId, out NodeCatalogEntry entry)
        {
            entry = Entries.FirstOrDefault(candidate =>
                string.Equals(
                    candidate.Descriptor.StableId,
                    stableId,
                    StringComparison.Ordinal));
            return entry != null;
        }

        public static bool TryGetTwcModifier(
            Type modifierType,
            out NodeCatalogEntry entry)
        {
            entry = Entries.FirstOrDefault(candidate =>
                candidate.IsTwcModifier
                && candidate.TwcModifierType == modifierType);
            return entry != null;
        }

        internal static void Invalidate()
        {
            _entries = null;
        }

        private static IReadOnlyList<NodeCatalogEntry> Build()
        {
            var result = new List<NodeCatalogEntry>();
            var nodeTypes = TypeCache.GetTypesDerivedFrom<NodeBase>()
                .Where(type => type != null && !type.IsAbstract && !type.IsGenericType)
                .OrderBy(type => type.FullName, StringComparer.Ordinal);

            foreach (var nodeType in nodeTypes)
                result.Add(new NodeCatalogEntry(BuildDescriptor(nodeType)));

            foreach (var item in TwcModifierCatalog.MenuItems)
                result.Add(BuildTwcEntry(item));

            var duplicateIds = result
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Descriptor.StableId))
                .GroupBy(entry => entry.Descriptor.StableId, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToHashSet(StringComparer.Ordinal);

            if (duplicateIds.Count > 0)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    var entry = result[i];
                    if (!duplicateIds.Contains(entry.Descriptor.StableId))
                        continue;

                    result[i] = new NodeCatalogEntry(
                        CopyWithUnavailableReason(
                            entry.Descriptor,
                            $"Duplicate stable node ID '{entry.Descriptor.StableId}'."),
                        entry.TwcModifierType);
                }
            }

            return result
                .OrderBy(entry => NodeCategoryOrder.Get(entry.Descriptor.Category))
                .ThenBy(entry => entry.Descriptor.Category, StringComparer.Ordinal)
                .ThenBy(entry => entry.Descriptor.Order)
                .ThenBy(entry => entry.Descriptor.Title, StringComparer.Ordinal)
                .ToArray();
        }

        private static NodeDescriptor BuildDescriptor(Type nodeType)
        {
            var info = nodeType.GetCustomAttribute<NodeInfoAttribute>(false);
            string title = info?.Title ?? ObjectNames.NicifyVariableName(nodeType.Name);
            string category = info?.Category ?? "Advanced";
            string description = info?.Description ?? string.Empty;
            string stableId = info?.StableId;
            int order = info?.Order ?? 1000;
            var lifecycle = info?.Lifecycle ?? NodeLifecycle.Hidden;
            string previewOutput = info?.PreviewOutput;
            var capabilities = info?.Capabilities ?? NodeCapabilities.None;
            string unavailableReason = null;
            PortDefinition[] inputs = Array.Empty<PortDefinition>();
            PortDefinition[] outputs = Array.Empty<PortDefinition>();
            NodeBase instance = null;

            if (string.IsNullOrWhiteSpace(stableId))
                unavailableReason = "Stable node ID is missing.";
            else if (ContainsCyrillic(title) || ContainsCyrillic(category))
                unavailableReason = "Назва вузла й категорія мають бути англійською.";
            else if (!ContainsCyrillic(description))
                unavailableReason = "Опис вузла має бути українською.";

            try
            {
                instance = ScriptableObject.CreateInstance(nodeType) as NodeBase;
                if (instance == null)
                {
                    unavailableReason ??= "Type cannot be instantiated as NodeBase.";
                }
                else
                {
                    inputs = instance.Inputs ?? Array.Empty<PortDefinition>();
                    outputs = instance.Outputs ?? Array.Empty<PortDefinition>();
                    unavailableReason ??= ValidatePorts(inputs, outputs, previewOutput);
                    if (lifecycle == NodeLifecycle.Active
                        && unavailableReason == null
                        && !NativeNodeContractProbe.TryValidate(
                            nodeType,
                            instance,
                            out string contractError))
                    {
                        unavailableReason = contractError;
                    }
                }
            }
            catch (Exception exception)
            {
                unavailableReason ??= $"Descriptor construction failed: {exception.Message}";
            }
            finally
            {
                if (instance != null)
                    UnityEngine.Object.DestroyImmediate(instance);
            }

            if (Attribute.IsDefined(nodeType, typeof(StaticGraphNodeAttribute))
                && lifecycle == NodeLifecycle.Active)
                lifecycle = NodeLifecycle.Hidden;

            return new NodeDescriptor(
                stableId,
                nodeType,
                title,
                category,
                order,
                description,
                lifecycle,
                previewOutput,
                capabilities,
                inputs,
                outputs,
                unavailableReason);
        }

        private static NodeCatalogEntry BuildTwcEntry(TwcModifierMenuItem item)
        {
            bool valid = TwcModifierContractProbe.TryGetValidation(
                item.ModifierType,
                item.IsGenerator,
                out string reason);
            string stableId = "twc." + (item.ModifierType?.FullName ?? item.DisplayName);
            var inputs = item.IsGenerator
                ? Array.Empty<PortDefinition>()
                : new[] { PortDefinition.Input<bool[,]>("Source", "in.source") };
            var outputs = new[]
            {
                PortDefinition.Output<bool[,]>("Mask", "out.mask")
            };
            string description = item.IsGenerator
                ? "Зовнішній генератор TileWorldCreator, перевірений на детермінізм і точний розмір карти."
                : "Зовнішній модифікатор TileWorldCreator, перевірений на детермінізм і точний розмір карти.";
            var descriptor = new NodeDescriptor(
                stableId,
                typeof(TwcModifierNode),
                item.DisplayName,
                item.MenuCategory,
                1000,
                description,
                valid ? NodeLifecycle.Active : NodeLifecycle.Hidden,
                "out.mask",
                NodeCapabilities.ExternalDependency
                | NodeCapabilities.Deterministic
                | NodeCapabilities.RectangularMaps
                | NodeCapabilities.LogicalPreview,
                inputs,
                outputs,
                valid ? null : reason);
            return new NodeCatalogEntry(descriptor, item.ModifierType);
        }

        private static string ValidatePorts(
            IReadOnlyList<PortDefinition> inputs,
            IReadOnlyList<PortDefinition> outputs,
            string previewOutput)
        {
            string error = ValidatePortSet(inputs, PortDirection.Input);
            if (error != null)
                return error;
            error = ValidatePortSet(outputs, PortDirection.Output);
            if (error != null)
                return error;

            if (!string.IsNullOrEmpty(previewOutput)
                && !outputs.Any(port =>
                    string.Equals(port?.Id, previewOutput, StringComparison.Ordinal)))
                return $"Preview output '{previewOutput}' does not exist.";

            return null;
        }

        private static string ValidatePortSet(
            IReadOnlyList<PortDefinition> ports,
            PortDirection direction)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < ports.Count; i++)
            {
                var port = ports[i];
                if (port == null)
                    return $"{direction} port at index {i} is null.";
                if (port.Direction != direction)
                    return $"Port '{port.Name}' has the wrong direction.";
                if (string.IsNullOrWhiteSpace(port.Id))
                    return $"Port '{port.Name}' has no stable ID.";
                if (!ids.Add(port.Id))
                    return $"Port ID '{port.Id}' is duplicated.";
            }

            return null;
        }

        private static NodeDescriptor CopyWithUnavailableReason(
            NodeDescriptor source,
            string reason)
        {
            return new NodeDescriptor(
                source.StableId,
                source.NodeType,
                source.Title,
                source.Category,
                source.Order,
                source.Description,
                source.Lifecycle,
                source.PreviewOutput,
                source.Capabilities,
                source.Inputs,
                source.Outputs,
                reason);
        }

        private static bool ContainsCyrillic(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (character >= '\u0400' && character <= '\u04ff')
                    return true;
            }

            return false;
        }
    }

    internal static class TwcModifierContractProbe
    {
        private static readonly Dictionary<Type, (bool valid, string reason)> Results = new();
        private static readonly Queue<(Type type, bool isGenerator)> Pending = new();
        private static readonly HashSet<Type> PendingTypes = new();
        private static bool _scheduled;

        public static bool TryGetValidation(
            Type modifierType,
            bool isGenerator,
            out string reason)
        {
            if (modifierType == null)
            {
                reason = "TWC modifier type is missing.";
                return false;
            }

            if (Results.TryGetValue(modifierType, out var cached))
            {
                reason = cached.reason;
                return cached.valid;
            }

            Schedule(modifierType, isGenerator);
            reason = "Contract smoke-test is pending.";
            return false;
        }

        internal static bool TryValidateNow(
            Type modifierType,
            bool isGenerator,
            out string reason)
        {
            if (modifierType == null)
            {
                reason = "TWC modifier type is missing.";
                return false;
            }

            bool valid;
            try
            {
                ulong first = ExecuteAndHash(modifierType, isGenerator);
                ulong second = ExecuteAndHash(modifierType, isGenerator);
                valid = first == second;
                reason = valid
                    ? null
                    : "Contract smoke-test detected a non-deterministic result.";
            }
            catch (Exception exception)
            {
                valid = false;
                reason = $"Contract smoke-test failed: {exception.Message}";
            }

            Results[modifierType] = (valid, reason);
            return valid;
        }

        private static void Schedule(Type modifierType, bool isGenerator)
        {
            if (!PendingTypes.Add(modifierType))
                return;

            Pending.Enqueue((modifierType, isGenerator));
            if (_scheduled)
                return;

            _scheduled = true;
            EditorApplication.delayCall += ProcessNext;
        }

        private static void ProcessNext()
        {
            _scheduled = false;
            if (Pending.Count == 0)
                return;

            var item = Pending.Dequeue();
            PendingTypes.Remove(item.type);
            TryValidateNow(item.type, item.isGenerator, out _);
            GraphNodeCatalog.Invalidate();

            if (Pending.Count > 0)
            {
                _scheduled = true;
                EditorApplication.delayCall += ProcessNext;
            }
        }

        private static ulong ExecuteAndHash(Type modifierType, bool isGenerator)
        {
            const int width = 7;
            const int height = 5;
            TwcModifierNode node = null;
            ScriptableObject modifier = null;
            try
            {
                node = ScriptableObject.CreateInstance<TwcModifierNode>();
                node.NodeId = "twc-contract-probe";
                node.ConfigureModifier(modifierType);
                modifier = node.ModifierAsset;
                if (modifier == null)
                    throw new InvalidOperationException("Modifier instance was not created.");

                var source = new bool[width, height];
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    source[x, y] = (x + y) % 3 != 0;

                var context = new NodeContext(1729)
                {
                    MapSize = new Vector2Int(width, height)
                };
                var output = node.Execute(
                    isGenerator ? Array.Empty<object>() : new object[] { source },
                    context);
                if (output == null || output.Status == NodeStatus.Error)
                    throw new InvalidOperationException(output?.Message ?? "Modifier returned null.");
                if (output.Values == null
                    || output.Values.Length != 1
                    || output.Values[0] is not bool[,] mask)
                    throw new InvalidOperationException("Modifier returned an invalid output contract.");
                if (mask.GetLength(0) != width || mask.GetLength(1) != height)
                    throw new InvalidOperationException(
                        $"Modifier returned {mask.GetLength(0)}x{mask.GetLength(1)} instead of {width}x{height}.");

                return Hash(mask);
            }
            finally
            {
                if (modifier != null)
                    UnityEngine.Object.DestroyImmediate(modifier);
                if (node != null)
                    UnityEngine.Object.DestroyImmediate(node);
            }
        }

        private static ulong Hash(bool[,] mask)
        {
            const ulong offset = 1469598103934665603UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            for (int y = 0; y < mask.GetLength(1); y++)
            for (int x = 0; x < mask.GetLength(0); x++)
            {
                hash ^= mask[x, y] ? (byte)1 : (byte)0;
                hash *= prime;
            }

            return hash;
        }
    }
}
