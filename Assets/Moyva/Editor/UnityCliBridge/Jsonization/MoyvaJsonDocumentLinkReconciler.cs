using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    [InitializeOnLoad]
    public static class MoyvaJsonDocumentLinkReconciler
    {
        public const string LinksRoot = MoyvaJsonDocumentLinkPolicy.LinksRoot;

        private static readonly HashSet<string> PendingJsonImports =
            new(StringComparer.OrdinalIgnoreCase);

        private static bool _scheduled;
        private static bool _running;
        private static bool _runtimeSyncRequested;

        static MoyvaJsonDocumentLinkReconciler()
        {
            Schedule();
        }

        [MenuItem("Moyva/JSON/Rebuild Document Links")]
        public static void RebuildFromMenu()
        {
            _runtimeSyncRequested = true;
            ReconcileNow();
        }

        public static void Schedule(bool syncRuntime = false)
        {
            _runtimeSyncRequested |= syncRuntime;
            if (_scheduled)
                return;

            _scheduled = true;
            EditorApplication.delayCall += RunScheduled;
        }

        public static void ReconcileNow()
        {
            if (_running)
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                Schedule(_runtimeSyncRequested);
                return;
            }

            _running = true;
            _scheduled = false;
            try
            {
                ImportPendingJson();
                EnsureAssetFolder(LinksRoot);

                MoyvaJsonDocumentIndex index = MoyvaJsonDocumentIndex.Build();
                HashSet<string> expectedLinks = BuildExpectedLinks(index);
                DeleteOrphanLinks(expectedLinks);
                AssetDatabase.SaveAssets();

                if (_runtimeSyncRequested)
                {
                    _runtimeSyncRequested = false;
                    JsonizationExportService.BuildAssetCatalog(
                        "Temp/moyva-json-document-catalog.json");
                    JsonizationExportService.SyncGeneratedResources(
                        "Temp/moyva-json-document-sync.json");
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[MoyvaJsonDocuments] Reconciliation failed: {exception}");
            }
            finally
            {
                _running = false;
            }
        }

        internal static void UpdateMovedAssetPaths(
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (movedAssets == null || movedFromAssetPaths == null)
                return;

            int pairCount = Math.Min(movedAssets.Length, movedFromAssetPaths.Length);
            if (pairCount == 0 || !Directory.Exists(JsonizationEditorUtil.PresetsRoot))
                return;

            var movedByOldPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < pairCount; i++)
            {
                string oldPath = NormalizeAssetPath(movedFromAssetPaths[i]);
                string newPath = NormalizeAssetPath(movedAssets[i]);
                if (!string.IsNullOrWhiteSpace(oldPath) && !string.IsNullOrWhiteSpace(newPath))
                    movedByOldPath[oldPath] = newPath;
            }

            if (movedByOldPath.Count == 0)
                return;

            foreach (MoyvaJsonDocumentRecord record in MoyvaJsonDocumentIndex.Build().Documents)
            {
                string jsonPath = record.Path;
                JObject document = record.Document;
                if (document == null)
                    continue;

                bool changed = false;
                foreach (JObject assetReference in document
                             .DescendantsAndSelf()
                             .OfType<JObject>()
                             .Where(value => value.Property("$asset") != null))
                {
                    string oldPath = NormalizeAssetPath(
                        assetReference.Value<string>("editorPath"));
                    if (!movedByOldPath.TryGetValue(oldPath, out string newPath))
                        continue;

                    assetReference["editorPath"] = newPath;
                    changed = true;
                }

                if (!changed)
                    continue;

                File.WriteAllText(jsonPath, document.ToString(Formatting.Indented) + "\n");
                PendingJsonImports.Add(NormalizeAssetPath(jsonPath));
                _runtimeSyncRequested = true;
            }
        }

        internal static void UpdateMovedDocumentLinkPaths(string[] movedAssets)
        {
            if (movedAssets == null)
                return;

            foreach (string movedPath in movedAssets)
            {
                string newPath = NormalizeAssetPath(movedPath);
                MoyvaJsonDocumentLink link =
                    AssetDatabase.LoadAssetAtPath<MoyvaJsonDocumentLink>(newPath);
                string sourcePath = link?.Source != null
                    ? NormalizeAssetPath(AssetDatabase.GetAssetPath(link.Source))
                    : string.Empty;

                if (!IsCanonicalJson(sourcePath))
                    continue;

                JObject document;
                try
                {
                    document = JObject.Parse(File.ReadAllText(sourcePath));
                }
                catch
                {
                    continue;
                }

                string storedPath = string.Equals(
                        newPath,
                        MoyvaJsonDocumentLinkPolicy.DefaultPathFor(sourcePath),
                        StringComparison.OrdinalIgnoreCase)
                    ? string.Empty
                    : newPath;

                MoyvaJsonDocumentLinkPolicy.Write(document, true, storedPath);
                File.WriteAllText(sourcePath, document.ToString(Formatting.Indented) + "\n");
                PendingJsonImports.Add(sourcePath);
            }
        }

        private static void RunScheduled()
        {
            _scheduled = false;
            ReconcileNow();
        }

        private static HashSet<string> BuildExpectedLinks(MoyvaJsonDocumentIndex index)
        {
            var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (MoyvaJsonDocumentRecord record in index.RuntimeGameplayDocuments)
            {
                if (!MoyvaJsonDocumentLinkPolicy.TryValidateTarget(
                        record.Document,
                        record.Path,
                        out string pointer,
                        out string message))
                {
                    Debug.LogError(
                        $"[MoyvaJsonDocuments] {record.Path} [{record.Id}] {pointer}: {message}");
                    continue;
                }

                MoyvaJsonDocumentLinkTarget target = MoyvaJsonDocumentLinkPolicy.Read(
                    record.Document,
                    record.Path);
                if (!target.Enabled)
                    continue;

                string linkPath = NormalizeAssetPath(target.Path);
                if (!expected.Add(linkPath))
                {
                    Debug.LogError(
                        $"[MoyvaJsonDocuments] Duplicate document link target '{linkPath}' " +
                        $"requested by '{record.Path}'.");
                    continue;
                }

                EnsureAssetFolder(Path.GetDirectoryName(linkPath)?.Replace('\\', '/'));

                TextAsset source = AssetDatabase.LoadAssetAtPath<TextAsset>(record.Path);
                if (source == null)
                    continue;

                MoyvaJsonDocumentLink link =
                    AssetDatabase.LoadAssetAtPath<MoyvaJsonDocumentLink>(linkPath);
                if (link == null)
                {
                    link = ScriptableObject.CreateInstance<MoyvaJsonDocumentLink>();
                    link.name = Path.GetFileNameWithoutExtension(record.Path);
                    link.SetSource(source);
                    AssetDatabase.CreateAsset(link, linkPath);
                    continue;
                }

                if (link.Source == source)
                    continue;

                link.SetSource(source);
                EditorUtility.SetDirty(link);
            }

            return expected;
        }

        private static void DeleteOrphanLinks(HashSet<string> expectedLinks)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Moyva"))
                return;

            foreach (string guid in AssetDatabase.FindAssets(
                         "t:MoyvaJsonDocumentLink",
                         new[] { "Assets/Moyva" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MoyvaJsonDocumentLink link =
                    AssetDatabase.LoadAssetAtPath<MoyvaJsonDocumentLink>(path);
                string sourcePath = link?.Source != null
                    ? AssetDatabase.GetAssetPath(link.Source)
                    : string.Empty;
                bool ownedGeneratedLink =
                    IsCanonicalJson(sourcePath)
                    || path.StartsWith(LinksRoot + "/", StringComparison.OrdinalIgnoreCase);

                if (ownedGeneratedLink && !expectedLinks.Contains(path))
                    AssetDatabase.DeleteAsset(path);
            }
        }

        private static void ImportPendingJson()
        {
            if (PendingJsonImports.Count == 0)
                return;

            string[] paths = PendingJsonImports.ToArray();
            PendingJsonImports.Clear();
            for (int i = 0; i < paths.Length; i++)
                AssetDatabase.ImportAsset(paths[i], ImportAssetOptions.ForceUpdate);
        }

        private static void EnsureAssetFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || AssetDatabase.IsValidFolder(folder))
                return;

            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string name = Path.GetFileName(folder);
            EnsureAssetFolder(parent);
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static string NormalizeAssetPath(string path)
            => (path ?? string.Empty).Replace('\\', '/').Trim();

        private static bool IsCanonicalJson(string path)
        {
            string normalized = NormalizeAssetPath(path);
            return normalized.StartsWith(
                       JsonizationEditorUtil.PresetsRoot + "/",
                       StringComparison.OrdinalIgnoreCase)
                   && normalized.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                   && !normalized.Contains("/Schemas/", StringComparison.OrdinalIgnoreCase);
        }
    }

    internal sealed class MoyvaJsonDocumentLinkPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            MoyvaJsonDocumentLinkReconciler.UpdateMovedAssetPaths(
                movedAssets,
                movedFromAssetPaths);
            MoyvaJsonDocumentLinkReconciler.UpdateMovedDocumentLinkPaths(movedAssets);

            bool canonicalJsonChanged = HasCanonicalJson(importedAssets)
                                        || HasCanonicalJson(deletedAssets)
                                        || HasCanonicalJson(movedAssets)
                                        || HasCanonicalJson(movedFromAssetPaths);
            bool linksChanged = HasLinkAsset(importedAssets)
                                || HasLinkAsset(deletedAssets)
                                || HasLinkAsset(movedAssets)
                                || HasLinkAsset(movedFromAssetPaths);
            if (canonicalJsonChanged || linksChanged)
                MoyvaJsonDocumentLinkReconciler.Schedule(canonicalJsonChanged);
        }

        private static bool HasCanonicalJson(IEnumerable<string> paths)
        {
            if (paths == null)
                return false;

            return paths.Any(path =>
                path != null
                && path.StartsWith(
                    JsonizationEditorUtil.PresetsRoot + "/",
                    StringComparison.OrdinalIgnoreCase)
                && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                && !path.Contains("/Schemas/", StringComparison.OrdinalIgnoreCase));
        }

        private static bool HasLinkAsset(IEnumerable<string> paths)
        {
            if (paths == null)
                return false;

            return paths.Any(path => path != null && path.StartsWith(
                MoyvaJsonDocumentLinkReconciler.LinksRoot + "/",
                StringComparison.OrdinalIgnoreCase));
        }
    }
}
