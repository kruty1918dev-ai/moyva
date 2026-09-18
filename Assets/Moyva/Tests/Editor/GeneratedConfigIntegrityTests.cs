using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Kruty1918.Moyva.Jsonization;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Startup
{
    /// <summary>
    /// Регресійний тест цілісності згенерованої конфігурації: кожен документ у
    /// Resources/MoyvaConfigGenerated має відображатися на зареєстрований runtime-тип.
    /// Сиротні файли після міграцій (наприклад, видалені моделі) вбивали весь LoadAll,
    /// бо рантайм відхиляє невідомі model/schema fail-closed.
    /// </summary>
    public sealed class GeneratedConfigIntegrityTests
    {
        [Test]
        public void GeneratedConfigRuntimeLoadsWithoutErrors()
        {
            Assert.DoesNotThrow(() => MoyvaJsonRuntime.EnsureLoaded());
            Assert.IsTrue(MoyvaJsonRuntime.IsLoaded);
        }

        [Test]
        public void EveryGeneratedDocumentResolvesToRegisteredType()
        {
            TextAsset[] files = Resources.LoadAll<TextAsset>("MoyvaConfigGenerated");
            Assert.IsNotEmpty(
                files,
                "Expected generated config JSON under Resources/MoyvaConfigGenerated.");

            var failures = new List<string>();
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (TextAsset file in files.OrderBy(f => f.name, StringComparer.Ordinal))
            {
                if (file == null || string.IsNullOrWhiteSpace(file.text))
                    continue;

                GeneratedDocHeader header;
                try
                {
                    header = JsonUtility.FromJson<GeneratedDocHeader>(file.text);
                }
                catch (Exception ex)
                {
                    failures.Add($"{file.name}: invalid JSON ({ex.Message})");
                    continue;
                }

                if (header == null)
                {
                    failures.Add($"{file.name}: invalid JSON (empty document)");
                    continue;
                }

                string model = header.model;
                string schema = header.schema;
                string id = header.id;

                if (string.IsNullOrWhiteSpace(model) ||
                    string.IsNullOrWhiteSpace(schema) ||
                    string.IsNullOrWhiteSpace(id))
                {
                    failures.Add($"{file.name}: missing model/schema/id metadata");
                    continue;
                }

                if (IsLegacyEditorOnlyDocument(header, model, schema))
                    continue;

                Type resolved = MoyvaJsonTypeRegistry.ResolveConfigModel(model, schema);
                if (resolved == null)
                {
                    failures.Add(
                        $"{file.name}: no allow-listed runtime config type " +
                        $"for model='{model}', schema='{schema}'. " +
                        "Remove the orphan generated file or register the model.");
                    continue;
                }

                string dupKey = resolved.FullName + "|" + id;
                if (!seenKeys.Add(dupKey))
                    failures.Add($"{file.name}: duplicate config id '{id}' for type '{resolved.FullName}'.");
            }

            Assert.IsEmpty(failures, string.Join("\n", failures));
        }

        // JsonUtility читає лише потрібні поля; решту документа ігнорує.
        [Serializable]
        private sealed class GeneratedDocHeader
        {
            public string model;
            public string schema;
            public string id;
            public MigrationHeader migration;
        }

        [Serializable]
        private sealed class MigrationHeader
        {
            public string sourceAssetPath;
        }

        // Mirrors MoyvaJsonRuntime.IsLegacyEditorOnlyConstructionDocument: Pass82 exported
        // editor-only Construction authoring docs that intentionally have no runtime model.
        private static bool IsLegacyEditorOnlyDocument(GeneratedDocHeader root, string model, string schema)
        {
            string sourceAssetPath = root?.migration?.sourceAssetPath;
            if (string.IsNullOrWhiteSpace(sourceAssetPath))
                return false;

            string normalizedPath = sourceAssetPath.Replace('\\', '/');
            const string legacyTemplateRoot =
                "Assets/Moyva/Data/ScriptableObjects/Construction/Templates/";
            if (!normalizedPath.StartsWith(legacyTemplateRoot, StringComparison.OrdinalIgnoreCase))
                return false;

            return
                (string.Equals(model, "building-archetype", StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(schema, "moyva.building-archetype", StringComparison.OrdinalIgnoreCase)) ||
                (string.Equals(model, "building-template-library", StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(schema, "moyva.building-template-library", StringComparison.OrdinalIgnoreCase));
        }
    }
}
