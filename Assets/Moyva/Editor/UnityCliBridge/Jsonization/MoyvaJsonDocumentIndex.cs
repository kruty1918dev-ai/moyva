using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    /// <summary>
    /// Легкий одноразовий індекс canonical JSON-документів.
    /// </summary>
    /// <remarks>
    /// Editor tooling часто потребує ті самі відповіді: які існують config IDs,
    /// чи документ є runtime gameplay config, де лежить model/id. Цей індекс тримає
    /// ці відповіді в одному місці, щоб validator, inspector і reconciler не
    /// сканували Presets кожен власним способом.
    /// </remarks>
    internal sealed class MoyvaJsonDocumentIndex
    {
        private readonly Dictionary<string, MoyvaJsonDocumentRecord> _byPath;
        private readonly Dictionary<string, List<string>> _pathsByModelId;

        private MoyvaJsonDocumentIndex(
            IReadOnlyList<MoyvaJsonDocumentRecord> documents,
            IReadOnlyList<MoyvaJsonDocumentRecord> runtimeGameplayDocuments,
            Dictionary<string, MoyvaJsonDocumentRecord> byPath,
            Dictionary<string, List<string>> pathsByModelId,
            IReadOnlyDictionary<string, string[]> idsByModel)
        {
            Documents = documents;
            RuntimeGameplayDocuments = runtimeGameplayDocuments;
            _byPath = byPath;
            _pathsByModelId = pathsByModelId;
            IdsByModel = idsByModel;
        }

        public IReadOnlyList<MoyvaJsonDocumentRecord> Documents { get; }
        public IReadOnlyList<MoyvaJsonDocumentRecord> RuntimeGameplayDocuments { get; }
        public IReadOnlyDictionary<string, string[]> IdsByModel { get; }

        public static MoyvaJsonDocumentIndex Build(
            string overridePath = null,
            JObject overrideDocument = null)
        {
            string normalizedOverridePath = NormalizeAssetPath(overridePath);
            var documents = new List<MoyvaJsonDocumentRecord>();
            var runtimeDocuments = new List<MoyvaJsonDocumentRecord>();
            var byPath = new Dictionary<string, MoyvaJsonDocumentRecord>(
                StringComparer.OrdinalIgnoreCase);
            var pathsByModelId = new Dictionary<string, List<string>>(
                StringComparer.OrdinalIgnoreCase);
            var idsByModel = new Dictionary<string, HashSet<string>>(
                StringComparer.OrdinalIgnoreCase);

            foreach (string file in CanonicalJsonFiles())
            {
                string path = NormalizeAssetPath(file);
                JObject root;
                try
                {
                    root = string.Equals(path, normalizedOverridePath, StringComparison.OrdinalIgnoreCase)
                        ? overrideDocument
                        : JObject.Parse(File.ReadAllText(file));
                }
                catch
                {
                    continue;
                }

                if (!HasMoyvaRootMetadata(root))
                    continue;

                string model = root.Value<string>("model") ?? string.Empty;
                string schema = root.Value<string>("schema") ?? string.Empty;
                string id = root.Value<string>("id") ?? string.Empty;
                bool runtime = IsRuntimeGameplayDocument(model, schema);
                var record = new MoyvaJsonDocumentRecord(path, root, model, schema, id, runtime);

                documents.Add(record);
                byPath[path] = record;
                if (runtime)
                    runtimeDocuments.Add(record);

                string key = ModelIdKey(model, id);
                if (!pathsByModelId.TryGetValue(key, out List<string> paths))
                {
                    paths = new List<string>();
                    pathsByModelId.Add(key, paths);
                }
                paths.Add(path);

                if (!idsByModel.TryGetValue(model, out HashSet<string> ids))
                {
                    ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    idsByModel.Add(model, ids);
                }
                ids.Add(id);
            }

            var idArrays = idsByModel.ToDictionary(
                pair => pair.Key,
                pair => pair.Value
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                StringComparer.OrdinalIgnoreCase);

            return new MoyvaJsonDocumentIndex(
                documents.AsReadOnly(),
                runtimeDocuments.AsReadOnly(),
                byPath,
                pathsByModelId,
                idArrays);
        }

        public bool ContainsConfig(string model, string id)
            => !string.IsNullOrWhiteSpace(model)
               && !string.IsNullOrWhiteSpace(id)
               && _pathsByModelId.ContainsKey(ModelIdKey(model, id));

        public bool TryGetPaths(string model, string id, out List<string> paths)
            => _pathsByModelId.TryGetValue(ModelIdKey(model, id), out paths);

        public bool TryGetDocument(string path, out MoyvaJsonDocumentRecord record)
            => _byPath.TryGetValue(NormalizeAssetPath(path), out record);

        internal static string NormalizeAssetPath(string path)
            => (path ?? string.Empty).Replace('\\', '/').Trim();

        internal static string ModelIdKey(string model, string id)
            => (model ?? string.Empty).Trim() + "|" + (id ?? string.Empty).Trim();

        private static bool HasMoyvaRootMetadata(JObject root)
            => root != null
               && root.Property("schema") != null
               && root.Property("id") != null
               && root.Property("model") != null;

        private static IEnumerable<string> CanonicalJsonFiles()
        {
            if (!Directory.Exists(JsonizationEditorUtil.PresetsRoot))
                return Array.Empty<string>();

            return Directory
                .GetFiles(JsonizationEditorUtil.PresetsRoot, "*.json", SearchOption.AllDirectories)
                .Select(NormalizeAssetPath)
                .Where(path => !path.Contains("/Schemas/", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsRuntimeGameplayDocument(string model, string schema)
        {
            HashSet<string> keys = RuntimeModelKeys.Value;
            return keys.Contains("model:" + (model ?? string.Empty))
                   || keys.Contains("schema:" + (schema ?? string.Empty));
        }

        private static readonly Lazy<HashSet<string>> RuntimeModelKeys =
            new(BuildRuntimeModelKeys);

        private static HashSet<string> BuildRuntimeModelKeys()
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Type type in TypeCache.GetTypesDerivedFrom<MoyvaJsonConfigObject>())
            {
                if (type == null || type.IsAbstract)
                    continue;

                string typeNamespace = type.Namespace ?? string.Empty;
                if (!typeNamespace.StartsWith(
                        JsonizationEditorUtil.MoyvaNamespace,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                keys.Add("model:" + MoyvaJsonTypeRegistry.StableId(type));
                keys.Add("model:" + type.Name);
                keys.Add("schema:" + MoyvaJsonTypeRegistry.SchemaForConfigType(type));
            }

            return keys;
        }
    }

    internal readonly struct MoyvaJsonDocumentRecord
    {
        public MoyvaJsonDocumentRecord(
            string path,
            JObject document,
            string model,
            string schema,
            string id,
            bool isRuntimeGameplay)
        {
            Path = path ?? string.Empty;
            Document = document;
            Model = model ?? string.Empty;
            Schema = schema ?? string.Empty;
            Id = id ?? string.Empty;
            IsRuntimeGameplay = isRuntimeGameplay;
        }

        public string Path { get; }
        public JObject Document { get; }
        public string Model { get; }
        public string Schema { get; }
        public string Id { get; }
        public bool IsRuntimeGameplay { get; }
    }
}
