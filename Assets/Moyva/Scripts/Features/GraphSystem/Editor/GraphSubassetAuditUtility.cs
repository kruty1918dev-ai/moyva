using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    internal sealed class GraphSubassetAuditEntry
    {
        public GraphSubassetAuditEntry(Object asset, long localId, string reason)
        {
            Asset = asset;
            LocalId = localId;
            Reason = reason;
        }

        public Object Asset { get; }
        public long LocalId { get; }
        public string Reason { get; }
        public string TypeName => Asset != null
            ? Asset.GetType().FullName
            : "<Missing>";
        public string DisplayName => Asset != null
            ? Asset.name
            : "<Missing>";
    }

    internal sealed class GraphSubassetAuditReport
    {
        public GraphAsset Graph { get; set; }
        public string AssetPath { get; set; }
        public string AssetGuid { get; set; }
        public Hash128 DependencyHash { get; set; }
        public IReadOnlyList<GraphSubassetAuditEntry> Reachable { get; set; }
        public IReadOnlyList<GraphSubassetAuditEntry> Orphaned { get; set; }
        public IReadOnlyList<string> ExternalReferences { get; set; }
        public IReadOnlyList<string> Warnings { get; set; }
        public bool CleanupSafe { get; set; }
        public string ReportPath { get; set; }
    }

    /// <summary>
    /// Аудитує subassets графа через реальні serialized object references.
    /// Cleanup ніколи не запускається автоматично й завжди створює dry-run звіт.
    /// </summary>
    internal static class GraphSubassetAuditUtility
    {
        private static readonly Regex DocumentHeaderRegex = new(
            @"^--- !u!\d+ &(?<id>-?\d+)\s*$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        [MenuItem("Moyva/Graph Maintenance/Dry Run Selected Graph Subassets")]
        private static void AuditSelectedGraph()
        {
            var graph = Selection.activeObject as GraphAsset;
            if (graph == null)
            {
                EditorUtility.DisplayDialog(
                    "Graph subasset audit",
                    "Оберіть GraphAsset у Project window.",
                    "OK");
                return;
            }

            var report = Audit(graph, writeReport: true);
            EditorUtility.DisplayDialog(
                "Graph subasset audit",
                BuildDialogSummary(report),
                "OK");
        }

        [MenuItem("Moyva/Graph Maintenance/Delete Proven Orphans From Selected Graph")]
        private static void DeleteSelectedGraphOrphans()
        {
            var graph = Selection.activeObject as GraphAsset;
            if (graph == null)
            {
                EditorUtility.DisplayDialog(
                    "Graph subasset cleanup",
                    "Оберіть GraphAsset у Project window.",
                    "OK");
                return;
            }

            var report = Audit(graph, writeReport: true);
            if (!report.CleanupSafe)
            {
                EditorUtility.DisplayDialog(
                    "Cleanup blocked",
                    "Аудит не може довести повну досяжність subassets. " +
                    $"Нічого не видалено.\n\nЗвіт: {report.ReportPath}",
                    "OK");
                return;
            }

            if (report.Orphaned.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Graph subasset cleanup",
                    $"Доведених orphan subassets немає.\n\nЗвіт: {report.ReportPath}",
                    "OK");
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Delete proven orphan subassets?",
                $"Буде видалено {report.Orphaned.Count} subassets із '{graph.name}'.\n" +
                "Зовнішні та serialized-посилання збережено.\n\n" +
                $"Dry-run звіт: {report.ReportPath}",
                "Delete proven orphans",
                "Cancel");
            if (!confirmed)
                return;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Delete Proven Graph Orphans");
            Undo.RecordObject(graph, "Delete Proven Graph Orphans");
            int removed = 0;
            foreach (var entry in report.Orphaned)
            {
                if (entry?.Asset == null || entry.Asset == graph)
                    continue;

                Undo.DestroyObjectImmediate(entry.Asset);
                removed++;
            }

            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(
                report.AssetPath,
                ImportAssetOptions.ForceUpdate);
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log(
                $"[GraphSubassetAudit] Removed {removed} proven orphan subassets " +
                $"from '{report.AssetPath}'. Dry-run report: {report.ReportPath}");
        }

        internal static GraphSubassetAuditReport Audit(
            GraphAsset graph,
            bool writeReport)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            string assetPath = AssetDatabase.GetAssetPath(graph);
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new InvalidOperationException(
                    "GraphAsset must be saved before subasset audit.");
            }

            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            var warnings = new List<string>();
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .Where(asset => asset != null)
                .Distinct()
                .ToArray();
            var localById = new Dictionary<long, Object>();
            foreach (var asset in allAssets)
            {
                if (TryGetLocalId(asset, assetGuid, out long localId))
                    localById[localId] = asset;
            }

            string targetYaml = TryReadUnityYaml(assetPath, warnings);
            var documentIds = ParseDocumentIds(targetYaml);
            var unresolvedDocumentIds = documentIds
                .Where(id => !localById.ContainsKey(id))
                .ToArray();
            if (unresolvedDocumentIds.Length > 0)
            {
                warnings.Add(
                    $"{unresolvedDocumentIds.Length} YAML document(s) could not be loaded " +
                    "through AssetDatabase. Cleanup is blocked because their references cannot be inspected.");
            }

            var reasonByAsset = new Dictionary<Object, string>();
            var queue = new Queue<Object>();
            AddRoot(graph, "GraphAsset root", reasonByAsset, queue);
            foreach (var node in graph.Nodes)
            {
                if (node != null)
                    AddRoot(node, "GraphAsset.Nodes", reasonByAsset, queue);
            }

            var externalReferences = FindExternalReferences(
                assetPath,
                assetGuid,
                localById,
                warnings,
                out bool externalScanComplete);
            foreach (var reference in externalReferences)
            {
                if (localById.TryGetValue(reference.LocalId, out var target))
                {
                    AddRoot(
                        target,
                        $"External reference from {reference.AssetPath}",
                        reasonByAsset,
                        queue);
                }
            }

            bool traversalComplete = TraverseLocalReferences(
                assetPath,
                reasonByAsset,
                queue,
                warnings);
            var reachable = new List<GraphSubassetAuditEntry>();
            var orphaned = new List<GraphSubassetAuditEntry>();
            foreach (var pair in localById.OrderBy(pair => pair.Key))
            {
                if (pair.Value == graph)
                    continue;

                if (reasonByAsset.TryGetValue(pair.Value, out string reason))
                {
                    reachable.Add(
                        new GraphSubassetAuditEntry(
                            pair.Value,
                            pair.Key,
                            reason));
                }
                else
                {
                    orphaned.Add(
                        new GraphSubassetAuditEntry(
                            pair.Value,
                            pair.Key,
                            "No serialized or external path from GraphAsset"));
                }
            }

            var report = new GraphSubassetAuditReport
            {
                Graph = graph,
                AssetPath = assetPath,
                AssetGuid = assetGuid,
                DependencyHash = AssetDatabase.GetAssetDependencyHash(assetPath),
                Reachable = reachable,
                Orphaned = orphaned,
                ExternalReferences = externalReferences
                    .Select(reference =>
                        $"{reference.AssetPath} -> fileID {reference.LocalId}")
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                Warnings = warnings,
                CleanupSafe = externalScanComplete
                              && traversalComplete
                              && unresolvedDocumentIds.Length == 0
            };
            if (writeReport)
                report.ReportPath = WriteReport(report);
            return report;
        }

        private static bool TraverseLocalReferences(
            string assetPath,
            IDictionary<Object, string> reasonByAsset,
            Queue<Object> queue,
            ICollection<string> warnings)
        {
            bool complete = true;
            while (queue.Count > 0)
            {
                var source = queue.Dequeue();
                if (source == null)
                    continue;

                try
                {
                    var serialized = new SerializedObject(source);
                    var property = serialized.GetIterator();
                    bool enterChildren = true;
                    while (property.Next(enterChildren))
                    {
                        enterChildren = true;
                        if (property.propertyType
                            != SerializedPropertyType.ObjectReference)
                        {
                            continue;
                        }

                        var referenced = property.objectReferenceValue;
                        if (referenced == null
                            || !string.Equals(
                                AssetDatabase.GetAssetPath(referenced),
                                assetPath,
                                StringComparison.Ordinal))
                        {
                            continue;
                        }

                        AddRoot(
                            referenced,
                            $"{source.name}.{property.propertyPath}",
                            reasonByAsset,
                            queue);
                    }
                }
                catch (Exception exception)
                {
                    complete = false;
                    warnings.Add(
                        $"Could not inspect '{source.name}' ({source.GetType().FullName}): " +
                        exception.Message);
                }
            }

            return complete;
        }

        private static IReadOnlyList<ExternalReference> FindExternalReferences(
            string targetPath,
            string targetGuid,
            IReadOnlyDictionary<long, Object> localById,
            ICollection<string> warnings,
            out bool complete)
        {
            complete = true;
            var references = new List<ExternalReference>();
            var regex = new Regex(
                $@"fileID:\s*(?<id>-?\d+),\s*guid:\s*{Regex.Escape(targetGuid)}\b",
                RegexOptions.Compiled);

            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (string.Equals(path, targetPath, StringComparison.Ordinal)
                    || !path.StartsWith("Assets/", StringComparison.Ordinal)
                    || !File.Exists(path))
                {
                    continue;
                }

                if (!IsPotentialSerializedContainer(path))
                    continue;

                string text = TryReadUnityYaml(path, null);
                if (text != null)
                {
                    foreach (Match match in regex.Matches(text))
                    {
                        if (!long.TryParse(
                                match.Groups["id"].Value,
                                out long localId))
                        {
                            continue;
                        }

                        if (localById.ContainsKey(localId))
                            references.Add(new ExternalReference(path, localId));
                    }
                    continue;
                }

                try
                {
                    if (AssetDatabase.GetDependencies(path, false)
                        .Contains(targetPath, StringComparer.Ordinal))
                    {
                        complete = false;
                        warnings.Add(
                            $"Binary asset '{path}' depends on the graph container, " +
                            "but its exact subasset reference cannot be proven.");
                    }
                }
                catch (Exception exception)
                {
                    complete = false;
                    warnings.Add(
                        $"Could not inspect external asset '{path}': {exception.Message}");
                }
            }

            return references;
        }

        private static string TryReadUnityYaml(
            string path,
            ICollection<string> warnings)
        {
            try
            {
                using var stream = File.OpenRead(path);
                if (stream.Length < 5)
                    return null;

                var prefix = new byte[5];
                int read = stream.Read(prefix, 0, prefix.Length);
                if (read != prefix.Length
                    || prefix[0] != '%'
                    || prefix[1] != 'Y'
                    || prefix[2] != 'A'
                    || prefix[3] != 'M'
                    || prefix[4] != 'L')
                {
                    return null;
                }

                stream.Position = 0;
                using var reader = new StreamReader(
                    stream,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true);
                return reader.ReadToEnd();
            }
            catch (Exception exception)
            {
                warnings?.Add(
                    $"Could not read YAML '{path}': {exception.Message}");
                return null;
            }
        }

        private static HashSet<long> ParseDocumentIds(string yaml)
        {
            var result = new HashSet<long>();
            if (string.IsNullOrEmpty(yaml))
                return result;

            foreach (Match match in DocumentHeaderRegex.Matches(yaml))
            {
                if (long.TryParse(match.Groups["id"].Value, out long localId))
                    result.Add(localId);
            }

            return result;
        }

        private static bool IsPotentialSerializedContainer(string path)
        {
            string extension = Path.GetExtension(path);
            return extension.Equals(".asset", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".unity", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".prefab", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".mat", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".controller", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".overrideController", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".playable", StringComparison.OrdinalIgnoreCase);
        }

        private static void AddRoot(
            Object asset,
            string reason,
            IDictionary<Object, string> reasonByAsset,
            Queue<Object> queue)
        {
            if (asset == null || reasonByAsset.ContainsKey(asset))
                return;

            reasonByAsset[asset] = reason;
            queue.Enqueue(asset);
        }

        private static bool TryGetLocalId(
            Object asset,
            string expectedGuid,
            out long localId)
        {
            localId = 0;
            return asset != null
                   && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                       asset,
                       out string guid,
                       out localId)
                   && string.Equals(guid, expectedGuid, StringComparison.Ordinal);
        }

        private static string WriteReport(GraphSubassetAuditReport report)
        {
            string directory = Path.Combine(
                "Library",
                "Moyva",
                "Reports");
            Directory.CreateDirectory(directory);
            string safeName = Regex.Replace(
                report.Graph.name,
                @"[^A-Za-z0-9_.-]+",
                "-");
            string path = Path.Combine(
                directory,
                $"{safeName}-subassets-latest.md");
            File.WriteAllText(path, BuildMarkdown(report), Encoding.UTF8);
            Debug.Log(
                $"[GraphSubassetAudit] Dry-run complete: " +
                $"{report.Reachable.Count} reachable, " +
                $"{report.Orphaned.Count} proven orphan candidates. " +
                $"Report: {Path.GetFullPath(path)}");
            return Path.GetFullPath(path);
        }

        private static string BuildMarkdown(GraphSubassetAuditReport report)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Graph subasset dry-run");
            builder.AppendLine();
            builder.AppendLine($"- Graph: `{report.AssetPath}`");
            builder.AppendLine($"- GUID: `{report.AssetGuid}`");
            builder.AppendLine($"- Dependency hash: `{report.DependencyHash}`");
            builder.AppendLine($"- Cleanup safe: `{report.CleanupSafe}`");
            builder.AppendLine($"- Reachable subassets: `{report.Reachable.Count}`");
            builder.AppendLine($"- Proven orphan candidates: `{report.Orphaned.Count}`");
            AppendEntries(builder, "Reachable", report.Reachable);
            AppendEntries(builder, "Proven orphan candidates", report.Orphaned);

            builder.AppendLine();
            builder.AppendLine("## External references");
            builder.AppendLine();
            if (report.ExternalReferences.Count == 0)
                builder.AppendLine("- None.");
            else
                foreach (string reference in report.ExternalReferences)
                    builder.AppendLine($"- `{reference}`");

            builder.AppendLine();
            builder.AppendLine("## Warnings");
            builder.AppendLine();
            if (report.Warnings.Count == 0)
                builder.AppendLine("- None.");
            else
                foreach (string warning in report.Warnings)
                    builder.AppendLine($"- {warning}");

            return builder.ToString();
        }

        private static void AppendEntries(
            StringBuilder builder,
            string title,
            IReadOnlyList<GraphSubassetAuditEntry> entries)
        {
            builder.AppendLine();
            builder.AppendLine($"## {title}");
            builder.AppendLine();
            if (entries.Count == 0)
            {
                builder.AppendLine("- None.");
                return;
            }

            foreach (var entry in entries)
            {
                builder.AppendLine(
                    $"- `{entry.LocalId}` · `{entry.TypeName}` · " +
                    $"`{entry.DisplayName}` · {entry.Reason}");
            }
        }

        private static string BuildDialogSummary(
            GraphSubassetAuditReport report)
        {
            return
                $"Reachable: {report.Reachable.Count}\n" +
                $"Proven orphan candidates: {report.Orphaned.Count}\n" +
                $"Cleanup safe: {report.CleanupSafe}\n\n" +
                $"Звіт: {report.ReportPath}";
        }

        private readonly struct ExternalReference
        {
            public ExternalReference(string assetPath, long localId)
            {
                AssetPath = assetPath;
                LocalId = localId;
            }

            public string AssetPath { get; }
            public long LocalId { get; }
        }
    }
}
