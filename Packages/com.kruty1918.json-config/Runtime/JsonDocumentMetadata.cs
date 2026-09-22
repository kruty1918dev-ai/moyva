using Newtonsoft.Json.Linq;

namespace Kruty1918.JsonConfig
{
    /// <summary>
    /// Спільні імена службових JSON-гілок, які не є gameplay-даними.
    /// </summary>
    public static class JsonDocumentMetadata
    {
        public const string Editor = "editor";
        public const string DocumentLink = "documentLink";
        public const string DocumentLinkEnabled = "enabled";
        public const string DocumentLinkPath = "path";

        /// <summary>
        /// Клонує документ у вигляді, який може впливати на runtime snapshots і fingerprint.
        /// Editor-only гілка навмисно відкидається, щоб переміщення `.asset`-лінків не ламало multiplayer.
        /// </summary>
        public static JObject CloneRuntimeDocument(JObject source)
        {
            if (source == null)
                return new JObject();

            var clone = (JObject)source.DeepClone();
            clone.Remove(Editor);
            return clone;
        }

        /// <summary>
        /// Клонує лише payload config-моделі: без службових metadata і без editor-only налаштувань.
        /// </summary>
        public static JObject CloneConfigPayload(JObject source)
        {
            JObject payload = CloneRuntimeDocument(source);
            payload.Remove("$schema");
            payload.Remove("schema");
            payload.Remove("version");
            payload.Remove("id");
            payload.Remove("model");
            payload.Remove("sourceType");
            payload.Remove("sourceAssetGuid");
            payload.Remove("sourceAssetPath");
            payload.Remove("migration");
            return payload;
        }
    }
}
