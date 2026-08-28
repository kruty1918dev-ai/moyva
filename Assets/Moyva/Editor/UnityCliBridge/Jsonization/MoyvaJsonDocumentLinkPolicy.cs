using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    /// <summary>
    /// Єдине правило, де і чи створювати editor-only MoyvaJsonDocumentLink asset.
    /// </summary>
    /// <remarks>
    /// За замовчуванням link створюється автоматично у папці JsonDocuments, дзеркалячи
    /// шлях JSON під Presets. У самому JSON можна додати:
    ///
    /// "editor": { "documentLink": { "enabled": false } }
    /// або
    /// "editor": { "documentLink": { "path": "Assets/Moyva/Editor/MyLinks" } }
    ///
    /// `path` може бути як повним шляхом `.asset`, так і папкою. Gameplay runtime цю
    /// гілку ігнорує, тому вона не змінює gameplay snapshots і fingerprint.
    /// </remarks>
    internal static class MoyvaJsonDocumentLinkPolicy
    {
        public const string LinksRoot = "Assets/Moyva/Editor/JsonDocuments";

        public static MoyvaJsonDocumentLinkTarget Read(
            JObject document,
            string jsonPath)
        {
            JObject linkSettings = LinkSettings(document);
            JToken enabledToken = linkSettings?[
                MoyvaJsonDocumentMetadata.DocumentLinkEnabled];
            bool enabled = enabledToken?.Type == JTokenType.Boolean
                ? enabledToken.Value<bool>()
                : true;
            JToken pathToken = linkSettings?[
                MoyvaJsonDocumentMetadata.DocumentLinkPath];
            string rawPath = pathToken?.Type == JTokenType.String
                ? pathToken.Value<string>()
                : string.Empty;

            if (!enabled)
                return MoyvaJsonDocumentLinkTarget.Disabled(rawPath);

            string resolvedPath = ResolvePath(document, jsonPath, rawPath);
            return new MoyvaJsonDocumentLinkTarget(
                enabled: true,
                path: resolvedPath,
                rawPath: rawPath,
                hasCustomPath: !string.IsNullOrWhiteSpace(rawPath));
        }

        public static bool TryValidateTarget(
            JObject document,
            string jsonPath,
            out string pointer,
            out string message)
        {
            pointer = null;
            message = null;

            JToken editorToken = document?[MoyvaJsonDocumentMetadata.Editor];
            if (editorToken == null)
                return true;

            if (editorToken is not JObject editor)
            {
                pointer = "/" + MoyvaJsonDocumentMetadata.Editor;
                message = "editor metadata must be an object";
                return false;
            }

            foreach (JProperty property in editor.Properties())
            {
                if (property.Name != MoyvaJsonDocumentMetadata.DocumentLink)
                {
                    pointer = "/" + MoyvaJsonDocumentMetadata.Editor + "/" + property.Name;
                    message = "unknown editor metadata property";
                    return false;
                }
            }

            JToken linkToken = editor[MoyvaJsonDocumentMetadata.DocumentLink];
            if (linkToken == null)
                return true;

            if (linkToken is not JObject link)
            {
                pointer = "/editor/documentLink";
                message = "documentLink must be an object";
                return false;
            }

            foreach (JProperty property in link.Properties())
            {
                if (property.Name != MoyvaJsonDocumentMetadata.DocumentLinkEnabled
                    && property.Name != MoyvaJsonDocumentMetadata.DocumentLinkPath)
                {
                    pointer = "/editor/documentLink/" + property.Name;
                    message = "unknown documentLink property";
                    return false;
                }
            }

            JToken enabledToken = link[MoyvaJsonDocumentMetadata.DocumentLinkEnabled];
            if (enabledToken != null && enabledToken.Type != JTokenType.Boolean)
            {
                pointer = "/editor/documentLink/enabled";
                message = "enabled must be boolean";
                return false;
            }

            JToken pathToken = link[MoyvaJsonDocumentMetadata.DocumentLinkPath];
            if (pathToken != null && pathToken.Type != JTokenType.String)
            {
                pointer = "/editor/documentLink/path";
                message = "path must be a string";
                return false;
            }

            MoyvaJsonDocumentLinkTarget target = Read(document, jsonPath);
            if (!target.Enabled)
                return true;

            if (IsValidLinkPath(target.Path))
                return true;

            pointer = string.IsNullOrWhiteSpace(target.RawPath)
                ? "/editor/documentLink/path"
                : "/editor/documentLink/path";
            message = "path must resolve to an .asset file under Assets, outside Presets and Resources";
            return false;
        }

        public static void Write(JObject document, bool enabled, string pathOrFolder)
        {
            if (document == null)
                return;

            string normalizedPath = Normalize(pathOrFolder);
            if (enabled && string.IsNullOrWhiteSpace(normalizedPath))
            {
                RemoveEditorLinkMetadataIfDefault(document);
                return;
            }

            JObject editor = document[MoyvaJsonDocumentMetadata.Editor] as JObject;
            if (editor == null)
            {
                editor = new JObject();
                document[MoyvaJsonDocumentMetadata.Editor] = editor;
            }

            var link = new JObject
            {
                [MoyvaJsonDocumentMetadata.DocumentLinkEnabled] = enabled,
            };

            if (!string.IsNullOrWhiteSpace(normalizedPath))
                link[MoyvaJsonDocumentMetadata.DocumentLinkPath] = normalizedPath;

            editor[MoyvaJsonDocumentMetadata.DocumentLink] = link;
        }

        public static string DefaultPathFor(string jsonPath)
        {
            string normalizedJsonPath = Normalize(jsonPath);
            string relative = normalizedJsonPath.StartsWith(
                    JsonizationEditorUtil.PresetsRoot,
                    StringComparison.OrdinalIgnoreCase)
                ? normalizedJsonPath.Substring(JsonizationEditorUtil.PresetsRoot.Length)
                    .TrimStart('/')
                : Path.GetFileName(normalizedJsonPath);

            string withoutExtension = relative.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? relative.Substring(0, relative.Length - ".json".Length)
                : relative;
            return $"{LinksRoot}/{withoutExtension}.asset";
        }

        private static string ResolvePath(
            JObject document,
            string jsonPath,
            string pathOrFolder)
        {
            string requested = Normalize(pathOrFolder);
            if (string.IsNullOrWhiteSpace(requested))
                return DefaultPathFor(jsonPath);

            if (requested.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                return requested;

            string fileName = JsonizationEditorUtil.Slug(document?.Value<string>("id"));
            return requested.TrimEnd('/') + "/" + fileName + ".asset";
        }

        private static bool IsValidLinkPath(string assetPath)
        {
            string path = Normalize(assetPath);
            return path.StartsWith("Assets/", StringComparison.Ordinal)
                   && path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)
                   && !path.StartsWith(JsonizationEditorUtil.PresetsRoot + "/", StringComparison.OrdinalIgnoreCase)
                   && !path.Contains("/Resources/", StringComparison.OrdinalIgnoreCase);
        }

        private static JObject LinkSettings(JObject document)
            => document?[MoyvaJsonDocumentMetadata.Editor] is JObject editor
               && editor[MoyvaJsonDocumentMetadata.DocumentLink] is JObject link
                ? link
                : null;

        private static void RemoveEditorLinkMetadataIfDefault(JObject document)
        {
            if (document[MoyvaJsonDocumentMetadata.Editor] is not JObject editor)
                return;

            editor.Remove(MoyvaJsonDocumentMetadata.DocumentLink);
            if (!editor.Properties().GetEnumerator().MoveNext())
                document.Remove(MoyvaJsonDocumentMetadata.Editor);
        }

        private static string Normalize(string path)
            => (path ?? string.Empty).Replace('\\', '/').Trim();
    }

    internal readonly struct MoyvaJsonDocumentLinkTarget
    {
        public MoyvaJsonDocumentLinkTarget(
            bool enabled,
            string path,
            string rawPath,
            bool hasCustomPath)
        {
            Enabled = enabled;
            Path = path ?? string.Empty;
            RawPath = rawPath ?? string.Empty;
            HasCustomPath = hasCustomPath;
        }

        public bool Enabled { get; }
        public string Path { get; }
        public string RawPath { get; }
        public bool HasCustomPath { get; }

        public static MoyvaJsonDocumentLinkTarget Disabled(string rawPath)
            => new(
                enabled: false,
                path: string.Empty,
                rawPath: rawPath,
                hasCustomPath: !string.IsNullOrWhiteSpace(rawPath));
    }
}
