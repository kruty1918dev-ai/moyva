using System;
using System.Collections;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    /// <summary>
    /// Швидкий default-contract probe для native Moyva nodes. Результат кешується
    /// на домен, тому відкриття Odin selector не виконує вузли повторно.
    /// </summary>
    internal static class NativeNodeContractProbe
    {
        private const int Width = 7;
        private const int Height = 5;
        private const int Seed = 1729;

        private static readonly Dictionary<Type, (bool valid, string reason)>
            Results = new();

        internal static bool TryValidate(
            Type nodeType,
            NodeBase instance,
            out string reason)
        {
            if (nodeType == null || instance == null)
            {
                reason = "Native contract smoke-test could not instantiate the node.";
                return false;
            }

            if (Results.TryGetValue(nodeType, out var cached))
            {
                reason = cached.reason;
                return cached.valid;
            }

            bool valid;
            try
            {
                valid = Validate(instance, out reason);
            }
            catch (Exception exception)
            {
                valid = false;
                reason =
                    $"Native contract smoke-test failed: {exception.Message}";
            }

            Results[nodeType] = (valid, reason);
            return valid;
        }

        private static bool Validate(NodeBase node, out string reason)
        {
            node.NodeId = "native-contract-probe";
            NodeOutput first = Execute(node, out var firstInputs);
            try
            {
                if (!ValidateOutput(node, first, out reason))
                    return false;

                ulong firstHash = HashValues(first.Values);
                Type firstArtifactType = first.Artifact?.GetType();

                NodeOutput second = Execute(node, out var secondInputs);
                try
                {
                    if (!ValidateOutput(node, second, out reason))
                        return false;
                    if (firstHash != HashValues(second.Values))
                    {
                        reason =
                            "Native contract smoke-test detected a non-deterministic result.";
                        return false;
                    }
                    if (firstArtifactType != second.Artifact?.GetType())
                    {
                        reason =
                            "Native contract smoke-test detected an unstable artifact type.";
                        return false;
                    }
                }
                finally
                {
                    DestroyFixtures(secondInputs);
                }
            }
            finally
            {
                DestroyFixtures(firstInputs);
            }

            reason = null;
            return true;
        }

        private static NodeOutput Execute(
            NodeBase node,
            out FixtureSet fixtures)
        {
            fixtures = BuildInputs(node.Inputs);
            var context = new NodeContext(Seed)
            {
                MapSize = new Vector2Int(Width, Height)
            };
            using var randomScope = new GraphRandomScope(Seed);
            return node.Execute(fixtures.Values, context);
        }

        private static bool ValidateOutput(
            NodeBase node,
            NodeOutput output,
            out string reason)
        {
            if (output == null)
            {
                reason = "Native contract smoke-test returned null NodeOutput.";
                return false;
            }
            if (output.Status == NodeStatus.Error)
            {
                reason =
                    $"Native contract smoke-test execution error: {output.Message}";
                return false;
            }

            var definitions = node.Outputs ?? Array.Empty<PortDefinition>();
            var values = output.Values ?? Array.Empty<object>();
            if (values.Length != definitions.Length)
            {
                reason =
                    $"Native contract smoke-test expected {definitions.Length} output(s), " +
                    $"but received {values.Length}.";
                return false;
            }

            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                object value = values[i];
                if (value == null)
                {
                    if (definition.AllowNull)
                        continue;

                    reason =
                        $"Native contract smoke-test output '{definition.Id}' is null.";
                    return false;
                }
                if (!definition.AcceptsAnyValue
                    && !definition.ValueType.IsInstanceOfType(value))
                {
                    reason =
                        $"Native contract smoke-test output '{definition.Id}' has " +
                        $"runtime type {value.GetType().FullName}, expected " +
                        $"{definition.ValueType.FullName}.";
                    return false;
                }
                if (definition.MapSizePolicy == PortMapSizePolicy.MatchContext
                    && value is Array map
                    && (map.Rank != 2
                        || map.GetLength(0) != Width
                        || map.GetLength(1) != Height))
                {
                    reason =
                        $"Native contract smoke-test output '{definition.Id}' has " +
                        $"invalid map size; expected {Width}x{Height}.";
                    return false;
                }
            }

            if (node is IGraphOutputNode && output.Artifact == null)
            {
                reason =
                    "Native contract smoke-test Output node did not return an artifact.";
                return false;
            }

            reason = null;
            return true;
        }

        private static FixtureSet BuildInputs(
            IReadOnlyList<PortDefinition> definitions)
        {
            var fixtures = new FixtureSet(definitions?.Count ?? 0);
            if (definitions == null)
                return fixtures;

            for (int i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                fixtures.Values[i] = CreateValue(
                    definition.ValueType,
                    fixtures.OwnedObjects);
                if (fixtures.Values[i] == null
                    && definition.IsRequired
                    && !definition.AllowNull)
                {
                    throw new InvalidOperationException(
                        $"No contract fixture exists for required input " +
                        $"'{definition.Id}' ({definition.ValueType.FullName}).");
                }
            }

            return fixtures;
        }

        private static object CreateValue(
            Type type,
            ICollection<Object> ownedObjects)
        {
            if (type == typeof(object) || type == typeof(bool[,]))
                return CreateBoolMap();
            if (type == typeof(float[,]))
                return CreateFloatMap();
            if (type == typeof(int[,]))
                return CreateIntMap();
            if (type == typeof(string[,]))
                return CreateStringMap();
            if (type == typeof(bool))
                return true;
            if (type == typeof(int))
                return 3;
            if (type == typeof(float))
                return 0.375f;
            if (type == typeof(string))
                return "contract";
            if (type == typeof(Texture2D))
            {
                var texture = new Texture2D(
                    2,
                    2,
                    TextureFormat.RGBA32,
                    false);
                texture.SetPixels(new[]
                {
                    Color.black,
                    Color.white,
                    Color.red,
                    Color.green
                });
                texture.Apply(false, false);
                ownedObjects.Add(texture);
                return texture;
            }
            if (type == typeof(ScatterMask))
            {
                return new ScatterMask(
                    CreateBoolMap(),
                    new bool[Width, Height],
                    CreateFloatMap());
            }
            if (type == typeof(List<ScatterCandidate>))
            {
                return new List<ScatterCandidate>
                {
                    new(
                        new Vector2Int(1, 1),
                        new Vector2(0.1f, 0.2f),
                        0.8f,
                        30f,
                        1f),
                    new(
                        new Vector2Int(4, 3),
                        Vector2.zero,
                        0.6f,
                        90f,
                        0.9f)
                };
            }
            if (type == typeof(GrassCardSettings))
                return new GrassCardSettings();
            if (type == typeof(ObjectPlacementLayer))
            {
                var layer = new ObjectPlacementLayer("Contract Objects");
                layer.Candidates.Add(
                    new ScatterCandidate(
                        new Vector2Int(2, 2),
                        Vector2.zero,
                        1f,
                        0f,
                        1f));
                return layer;
            }

            return null;
        }

        private static bool[,] CreateBoolMap()
        {
            var result = new bool[Width, Height];
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                result[x, y] = (x + y) % 3 != 0;
            return result;
        }

        private static float[,] CreateFloatMap()
        {
            var result = new float[Width, Height];
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                result[x, y] = (x * 0.17f + y * 0.31f) % 1f;
            return result;
        }

        private static int[,] CreateIntMap()
        {
            var result = new int[Width, Height];
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                result[x, y] = (x + y) % 5;
            return result;
        }

        private static string[,] CreateStringMap()
        {
            var result = new string[Width, Height];
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                result[x, y] = (x + y) % 2 == 0 ? "A" : "B";
            return result;
        }

        private static ulong HashValues(IReadOnlyList<object> values)
        {
            const ulong offset = 1469598103934665603UL;
            ulong hash = offset;
            if (values == null)
                return hash;

            for (int i = 0; i < values.Count; i++)
                hash = Mix(hash, HashValue(values[i]));
            return hash;
        }

        private static ulong HashValue(object value)
        {
            if (value == null)
                return 0UL;
            if (value is Array array)
                return HashArray(array);
            if (value is ScatterMask scatterMask)
                return HashArray(scatterMask.BuildAllowedMask());
            if (value is IReadOnlyList<ScatterCandidate> candidates)
            {
                ulong hash = 1469598103934665603UL;
                for (int i = 0; i < candidates.Count; i++)
                {
                    hash = Mix(hash, unchecked((uint)candidates[i].Cell.x));
                    hash = Mix(hash, unchecked((uint)candidates[i].Cell.y));
                    hash = Mix(
                        hash,
                        unchecked((uint)BitConverter.SingleToInt32Bits(
                            candidates[i].Score)));
                }
                return hash;
            }
            if (value is ObjectPlacementLayer objectLayer)
            {
                ulong hash = unchecked((uint)GlobalSeed.StableHash(
                    objectLayer.LayerName));
                hash = Mix(hash, (ulong)objectLayer.Candidates.Count);
                hash = Mix(hash, (ulong)objectLayer.Prefabs.Count);
                return hash;
            }

            return value switch
            {
                bool boolean => boolean ? 1UL : 0UL,
                int number => unchecked((uint)number),
                float number =>
                    unchecked((uint)BitConverter.SingleToInt32Bits(number)),
                string text =>
                    unchecked((uint)GlobalSeed.StableHash(text)),
                _ => unchecked((uint)GlobalSeed.StableHash(
                    value.GetType().FullName))
            };
        }

        private static ulong HashArray(Array array)
        {
            ulong hash = 1469598103934665603UL;
            hash = Mix(hash, (ulong)array.Rank);
            for (int dimension = 0; dimension < array.Rank; dimension++)
                hash = Mix(hash, (ulong)array.GetLength(dimension));
            foreach (object value in array)
                hash = Mix(hash, HashValue(value));
            return hash;
        }

        private static ulong Mix(ulong hash, ulong value)
        {
            const ulong prime = 1099511628211UL;
            hash ^= value;
            hash *= prime;
            return hash;
        }

        private static void DestroyFixtures(FixtureSet fixtures)
        {
            if (fixtures?.OwnedObjects == null)
                return;

            foreach (var ownedObject in fixtures.OwnedObjects)
            {
                if (ownedObject != null)
                    Object.DestroyImmediate(ownedObject);
            }
        }

        private sealed class FixtureSet
        {
            public FixtureSet(int inputCount)
            {
                Values = new object[inputCount];
            }

            public object[] Values { get; }
            public List<Object> OwnedObjects { get; } = new();
        }
    }
}
