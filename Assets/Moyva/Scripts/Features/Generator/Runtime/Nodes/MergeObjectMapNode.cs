using Kruty1918.Moyva.GraphSystem.API;
using System;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Merge ObjectMaps", "Utility", "Об'єднує дві карти об'єктів в одну. У разі конфлікту можна обрати, який вхід має пріоритет.")]
    public sealed class MergeObjectMapNode : NodeBase, IPreviewableNode
    {
        private enum ConflictPriority
        {
            InputA = 0,
            InputB = 1
        }

        [Header("Conflict Resolution")]
        [Tooltip("Який вхід перемагає, якщо в одній клітинці обидві ObjectMap мають різні непорожні об'єкти.")]
        [SerializeField] private ConflictPriority _conflictPriority = ConflictPriority.InputB;

        [NonSerialized] private string[,] _lastA;
        [NonSerialized] private string[,] _lastB;
        [NonSerialized] private string[,] _lastResult;
        [NonSerialized] private bool[,] _lastConflictMask;
        [NonSerialized] private bool[,] _lastAOnlyMask;
        [NonSerialized] private bool[,] _lastBOnlyMask;
        [NonSerialized] private int _lastAObjects;
        [NonSerialized] private int _lastBObjects;
        [NonSerialized] private int _lastResultObjects;
        [NonSerialized] private int _lastConflicts;

        public int LastAObjects => _lastAObjects;
        public int LastBObjects => _lastBObjects;
        public int LastResultObjects => _lastResultObjects;
        public int LastConflicts => _lastConflicts;

        public override string Title => "Merge ObjectMaps";
        public override string Category => "Utility";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("ObjectMapA"),
            PortDefinition.Input<string[,]>("ObjectMapB")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("ObjectMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var mapA = inputs[0] as string[,];
            var mapB = inputs[1] as string[,];

            if (mapA == null && mapB == null)
                return NodeOutput.Error("At least one ObjectMap input is required.");

            if (mapA == null)
                return BuildSingleInputOutput(null, mapB);
            if (mapB == null)
                return BuildSingleInputOutput(mapA, null);

            int w = mapA.GetLength(0);
            int h = mapA.GetLength(1);

            if (mapB.GetLength(0) != w || mapB.GetLength(1) != h)
                return NodeOutput.Error("ObjectMapA and ObjectMapB must have the same dimensions.");

            var result = (string[,])mapA.Clone();
            var conflictMask = new bool[w, h];
            var aOnlyMask = new bool[w, h];
            var bOnlyMask = new bool[w, h];

            int aObjects = 0;
            int bObjects = 0;
            int resultObjects = 0;
            int conflicts = 0;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    string a = mapA[x, y];
                    string b = mapB[x, y];
                    bool hasA = !string.IsNullOrEmpty(a);
                    bool hasB = !string.IsNullOrEmpty(b);

                    if (hasA) aObjects++;
                    if (hasB) bObjects++;

                    if (hasA && hasB)
                    {
                        bool conflict = !string.Equals(a, b, StringComparison.Ordinal);
                        if (conflict)
                        {
                            conflicts++;
                            conflictMask[x, y] = true;
                        }

                        result[x, y] = _conflictPriority == ConflictPriority.InputA ? a : b;
                    }
                    else if (hasB)
                    {
                        result[x, y] = b;
                        bOnlyMask[x, y] = true;
                    }
                    else if (hasA)
                    {
                        aOnlyMask[x, y] = true;
                    }

                    if (!string.IsNullOrEmpty(result[x, y]))
                        resultObjects++;
                }
            }

            CachePreviewData(mapA, mapB, result, conflictMask, aOnlyMask, bOnlyMask, aObjects, bObjects, resultObjects, conflicts);
            return NodeOutput.Success(result);
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            int tw = Mathf.Max(16, width);
            int th = Mathf.Max(16, height);
            var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            if (_lastResult == null)
            {
                DrawNoDataPattern(tex, tw, th);
                return tex;
            }

            int sw = _lastResult.GetLength(0);
            int sh = _lastResult.GetLength(1);
            for (int y = 0; y < th; y++)
            {
                int sy = y * sh / th;
                for (int x = 0; x < tw; x++)
                {
                    int sx = x * sw / tw;
                    tex.SetPixel(x, y, PreviewColorAt(sx, sy));
                }
            }

            tex.Apply(false, false);
            return tex;
        }

        private NodeOutput BuildSingleInputOutput(string[,] mapA, string[,] mapB)
        {
            var source = mapA ?? mapB;
            var result = (string[,])source.Clone();
            int w = result.GetLength(0);
            int h = result.GetLength(1);
            var emptyConflictMask = new bool[w, h];
            var aOnlyMask = new bool[w, h];
            var bOnlyMask = new bool[w, h];
            int count = 0;

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                bool hasObject = !string.IsNullOrEmpty(result[x, y]);
                if (!hasObject)
                    continue;

                count++;
                if (mapA != null) aOnlyMask[x, y] = true;
                else bOnlyMask[x, y] = true;
            }

            CachePreviewData(mapA, mapB, result, emptyConflictMask, aOnlyMask, bOnlyMask,
                mapA != null ? count : 0,
                mapB != null ? count : 0,
                count,
                0);

            return NodeOutput.Success(result);
        }

        private void CachePreviewData(
            string[,] mapA,
            string[,] mapB,
            string[,] result,
            bool[,] conflictMask,
            bool[,] aOnlyMask,
            bool[,] bOnlyMask,
            int aObjects,
            int bObjects,
            int resultObjects,
            int conflicts)
        {
            _lastA = mapA;
            _lastB = mapB;
            _lastResult = result;
            _lastConflictMask = conflictMask;
            _lastAOnlyMask = aOnlyMask;
            _lastBOnlyMask = bOnlyMask;
            _lastAObjects = aObjects;
            _lastBObjects = bObjects;
            _lastResultObjects = resultObjects;
            _lastConflicts = conflicts;
        }

        private Color PreviewColorAt(int x, int y)
        {
            bool hasResult = _lastResult != null && !string.IsNullOrEmpty(_lastResult[x, y]);
            if (!hasResult)
                return new Color(0.08f, 0.09f, 0.12f, 1f);

            if (_lastConflictMask != null && _lastConflictMask[x, y])
                return _conflictPriority == ConflictPriority.InputA
                    ? new Color(0.10f, 0.55f, 1.00f, 1f)
                    : new Color(1.00f, 0.62f, 0.12f, 1f);

            if (_lastAOnlyMask != null && _lastAOnlyMask[x, y])
                return new Color(0.14f, 0.42f, 0.95f, 1f);

            if (_lastBOnlyMask != null && _lastBOnlyMask[x, y])
                return new Color(0.95f, 0.62f, 0.16f, 1f);

            return Color.Lerp(ObjectIdColor(_lastResult[x, y]), Color.white, 0.15f);
        }

        private static Color ObjectIdColor(string objectId)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < objectId.Length; i++)
                    hash = hash * 31 + objectId[i];

                float hue = ((hash & 0x7FFFFFFF) % 360) / 360f;
                return Color.HSVToRGB(hue, 0.65f, 0.90f);
            }
        }

        private static void DrawNoDataPattern(Texture2D tex, int width, int height)
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                bool stripe = ((x / 8) + (y / 8)) % 2 == 0;
                tex.SetPixel(x, y, stripe
                    ? new Color(0.12f, 0.14f, 0.20f, 1f)
                    : new Color(0.20f, 0.15f, 0.10f, 1f));
            }

            tex.Apply(false, false);
        }
    }
}
