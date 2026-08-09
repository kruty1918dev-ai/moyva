using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Editor.Shared
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MoyvaJsonAssetReference
    {
        [JsonProperty("path", Required = Required.Always, Order = 0)]
        public string Path;

        [JsonProperty("guid", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        public string Guid;

        [JsonProperty("subAsset", NullValueHandling = NullValueHandling.Ignore, Order = 2)]
        public string SubAsset;

        [JsonProperty("localId", DefaultValueHandling = DefaultValueHandling.Ignore, Order = 3)]
        public long LocalId;
    }

    public static class MoyvaJsonAssetReferenceResolver
    {
        public static MoyvaJsonAssetReference FromObject(Object asset)
        {
            if (asset == null)
                return null;

            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException($"'{asset.name}' is not a persistent Unity asset.");

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out long localId);
            Object mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            return new MoyvaJsonAssetReference
            {
                Path = path,
                Guid = AssetDatabase.AssetPathToGUID(path),
                SubAsset = mainAsset == asset ? null : asset.name,
                LocalId = localId,
            };
        }

        public static T Resolve<T>(MoyvaJsonAssetReference reference, bool required, string context)
            where T : Object
        {
            if (reference == null || string.IsNullOrWhiteSpace(reference.Path))
            {
                if (required)
                    throw new InvalidDataException($"{context}: required asset reference is missing.");

                return null;
            }

            string path = reference.Path.Replace('\\', '/').Trim();
            if (!path.StartsWith("Assets/", StringComparison.Ordinal))
                throw new InvalidDataException($"{context}: asset path must start with 'Assets/': {path}");

            if (!string.IsNullOrWhiteSpace(reference.Guid))
            {
                string currentPath = AssetDatabase.GUIDToAssetPath(reference.Guid.Trim());
                if (!string.IsNullOrWhiteSpace(currentPath))
                    path = currentPath;
            }

            Object[] candidates = AssetDatabase.LoadAllAssetsAtPath(path);
            if (reference.LocalId != 0)
            {
                for (int index = 0; index < candidates.Length; index++)
                {
                    Object candidate = candidates[index];
                    if (!(candidate is T typedCandidate))
                        continue;

                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(candidate, out _, out long candidateLocalId);
                    if (candidateLocalId == reference.LocalId)
                        return typedCandidate;
                }
            }

            if (!string.IsNullOrWhiteSpace(reference.SubAsset))
            {
                for (int index = 0; index < candidates.Length; index++)
                {
                    if (candidates[index] is T typedCandidate
                        && string.Equals(typedCandidate.name, reference.SubAsset, StringComparison.Ordinal))
                    {
                        return typedCandidate;
                    }
                }
            }

            T direct = AssetDatabase.LoadAssetAtPath<T>(path);
            if (direct != null)
                return direct;

            if (required)
                throw new InvalidDataException($"{context}: cannot resolve {typeof(T).Name} at '{path}'.");

            return null;
        }
    }

    public static class MoyvaJsonFile
    {
        private static readonly JsonSerializerSettings ReadSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Error,
            NullValueHandling = NullValueHandling.Include,
        };

        private static readonly JsonSerializerSettings WriteSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
        };

        public static T Read<T>(string assetPath)
        {
            string fullPath = ToFullPath(assetPath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("JSON source file not found.", assetPath);

            T result = JsonConvert.DeserializeObject<T>(File.ReadAllText(fullPath, Encoding.UTF8), ReadSettings);
            if (result == null)
                throw new InvalidDataException($"{assetPath}: JSON root is null.");

            return result;
        }

        public static void Write<T>(string assetPath, T value)
        {
            string fullPath = ToFullPath(assetPath);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            string json = JsonConvert.SerializeObject(value, Formatting.Indented, WriteSettings) + Environment.NewLine;
            File.WriteAllText(fullPath, json, new UTF8Encoding(false));
        }

        private static string ToFullPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath) || !assetPath.StartsWith("Assets/", StringComparison.Ordinal))
                throw new ArgumentException("Expected a project-relative path beginning with 'Assets/'.", nameof(assetPath));

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                                 ?? throw new InvalidOperationException("Cannot resolve Unity project root.");
            return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
        }
    }
}
