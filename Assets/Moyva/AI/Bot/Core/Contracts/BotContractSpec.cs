using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kruty1918.Moyva.AI.Bot
{
    // Editable source of truth: Assets/Moyva/Presets/AI/Resources/MoyvaBotContract.json.
    // The canonical signature below is mirrored byte-for-byte by
    // tools/ai/moyva_train.py contract_signature(); both sides must agree.
    [Serializable]
    public sealed class BotContractFeature
    {
        public int index;
        public string name;
        public int count = 1;
        // Game-side resource keys mapped into this feature slot. Aliases are
        // adapter semantics only; they never enter the contract signature.
        public string[] aliases;
    }

    [Serializable]
    public sealed class BotContractSpec
    {
        public int contractVersion;
        public int observationSchemaVersion;
        public int candidateSchemaVersion;
        public int actionSchemaVersion;
        public int maxCandidateSlots;
        public int globalFeatureCount;
        public int candidateFeatureCount;
        public int spatialSize;
        public int spatialChannels;
        public BotContractFeature[] global = Array.Empty<BotContractFeature>();
        public string[] candidate = Array.Empty<string>();
        public string[] intents = Array.Empty<string>();

        public int SpatialFeatureCount => spatialSize * spatialSize * spatialChannels;
        public int ObservationCount => globalFeatureCount + SpatialFeatureCount + maxCandidateSlots * candidateFeatureCount;

        public BotContractFeature Feature(int featureIndex)
        {
            if (global == null) return null;
            foreach (var feature in global)
                if (feature != null && featureIndex >= feature.index
                    && featureIndex < feature.index + feature.count)
                    return feature;
            return null;
        }

        public string[] Aliases(int featureIndex)
            => Feature(featureIndex)?.aliases ?? Array.Empty<string>();

        public string Label(int featureIndex)
        {
            var feature = Feature(featureIndex);
            if (feature == null) return featureIndex.ToString();
            var alias = feature.aliases;
            return alias != null && alias.Length > 0 && !string.IsNullOrWhiteSpace(alias[0])
                ? alias[0]
                : feature.name ?? featureIndex.ToString();
        }

        public string Signature()
        {
            var builder = new StringBuilder("MoyvaBot:");
            builder.Append("v").Append(contractVersion)
                .Append(":o").Append(observationSchemaVersion)
                .Append(":c").Append(candidateSchemaVersion)
                .Append(":a").Append(actionSchemaVersion)
                .Append(":slots").Append(maxCandidateSlots)
                .Append(":global").Append(globalFeatureCount)
                .Append(":spatial").Append(spatialSize).Append('x').Append(spatialSize).Append('x').Append(spatialChannels)
                .Append(":candidate").Append(candidateFeatureCount)
                .Append(":global=");
            for (int i = 0; i < global.Length; i++)
            {
                var feature = global[i];
                if (i > 0) builder.Append(',');
                builder.Append(feature.index).Append(feature.name);
                if (feature.count > 1) builder.Append(feature.count);
            }
            builder.Append(":candidate=").Append(string.Join(",", candidate))
                .Append(":intents=").Append(string.Join(",", intents));
            return builder.ToString();
        }

        public void Validate()
        {
            if (contractVersion < 1 || observationSchemaVersion < 1 || candidateSchemaVersion < 1 || actionSchemaVersion < 1)
                throw new ArgumentException("Contract schema versions must be positive.");
            if (maxCandidateSlots < 1 || globalFeatureCount < 1 || candidateFeatureCount < 1
                || spatialSize < 1 || spatialChannels < 1)
                throw new ArgumentException("Contract dimensions must be positive.");
            if (global == null || global.Length == 0 || candidate == null || candidate.Length == 0
                || intents == null || intents.Length == 0)
                throw new ArgumentException("Contract feature/intent lists cannot be empty.");

            var names = new HashSet<string>(StringComparer.Ordinal);
            var covered = new List<KeyValuePair<int, int>>();
            foreach (var feature in global)
            {
                if (feature == null || feature.index < 0 || feature.index >= globalFeatureCount
                    || string.IsNullOrWhiteSpace(feature.name) || feature.count < 1
                    || feature.index + feature.count > globalFeatureCount)
                    throw new ArgumentException("Invalid contract feature entry.");
                if (!names.Add(feature.name))
                    throw new ArgumentException("Duplicate contract feature name: " + feature.name);
                covered.Add(new KeyValuePair<int, int>(feature.index, feature.index + feature.count - 1));
            }
            var sorted = covered.OrderBy(range => range.Key).ToList();
            for (int i = 1; i < sorted.Count; i++)
                if (sorted[i].Key <= sorted[i - 1].Value)
                    throw new ArgumentException("Contract feature index ranges overlap.");
        }
    }
}
