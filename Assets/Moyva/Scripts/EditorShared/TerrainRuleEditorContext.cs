#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Editor.Shared
{
    public enum TerrainRuleEditorResolutionStatus
    {
        ResolvedFromActiveScene,
        ResolvedFromProjectContext,
        ResolvedFromSingleAsset,
        Missing,
        AmbiguousActiveScene,
        AmbiguousAssets,
    }

    public readonly struct TerrainRuleEditorResolution<T> where T : UnityEngine.Object
    {
        public TerrainRuleEditorResolution(
            T asset,
            TerrainRuleEditorResolutionStatus status,
            string message)
        {
            Asset = asset;
            Status = status;
            Message = message ?? string.Empty;
        }

        public T Asset { get; }
        public TerrainRuleEditorResolutionStatus Status { get; }
        public string Message { get; }
        public bool IsResolved => Asset != null;
    }

    public readonly struct TerrainRuleTagOption
    {
        public TerrainRuleTagOption(
            string value,
            string displayName,
            string description,
            bool isBuiltIn,
            bool isPresentInProfile)
        {
            Value = value ?? string.Empty;
            DisplayName = displayName ?? Value;
            Description = description ?? string.Empty;
            IsBuiltIn = isBuiltIn;
            IsPresentInProfile = isPresentInProfile;
        }

        public string Value { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public bool IsBuiltIn { get; }
        public bool IsPresentInProfile { get; }
        public string MenuLabel => string.Equals(DisplayName, Value, StringComparison.OrdinalIgnoreCase)
            ? Value
            : $"{DisplayName} ({Value})";
    }

    public readonly struct TerrainRuleLayerOption
    {
        public TerrainRuleLayerOption(string value, string displayName, bool isFallback)
        {
            Value = value ?? string.Empty;
            DisplayName = displayName ?? Value;
            IsFallback = isFallback;
        }

        public string Value { get; }
        public string DisplayName { get; }
        public bool IsFallback { get; }
        public string MenuLabel => string.Equals(DisplayName, Value, StringComparison.OrdinalIgnoreCase)
            ? Value
            : $"{DisplayName} ({Value})";
    }

    public sealed class TerrainRuleEditorCatalogSnapshot
    {
        public TerrainRuleEditorCatalogSnapshot(
            IReadOnlyList<TerrainRuleTagOption> tags,
            IReadOnlyList<TerrainRuleLayerOption> layers)
        {
            Tags = tags ?? Array.Empty<TerrainRuleTagOption>();
            Layers = layers ?? Array.Empty<TerrainRuleLayerOption>();
        }

        public IReadOnlyList<TerrainRuleTagOption> Tags { get; }
        public IReadOnlyList<TerrainRuleLayerOption> Layers { get; }
    }

    /// <summary>
    /// Editor-only resolver and catalogue used by construction rule inspectors.
    /// It never guesses between multiple scene objects or multiple project assets.
    /// </summary>
    public static class TerrainRuleEditorContext
    {
        private const string GridInstallerTypeName = "Kruty1918.Moyva.Grid.Runtime.GridInstaller";

        private readonly struct SuggestedTag
        {
            public SuggestedTag(string value, string displayName, string description)
            {
                Value = value;
                DisplayName = displayName;
                Description = description;
            }

            public string Value { get; }
            public string DisplayName { get; }
            public string Description { get; }
        }

        private static readonly SuggestedTag[] SuggestedTagDefinitions =
        {
            new("water", "Вода", "Річки, озера, море та інші водні тайли."),
            new("land", "Суша", "Звичайна земля, придатна для наземних будівель."),
            new("forest", "Ліс", "Тайли лісу або густої рослинності."),
            new("mountain", "Гора", "Гори, скелі та високі кам'яні тайли."),
            new("road", "Дорога", "Дороги та прокладені шляхи."),
            new("shore", "Берег", "Узбережжя або перехід між сушею та водою."),
            new("swamp", "Болото", "Болотиста або волога місцевість."),
            new("desert", "Пустеля", "Пісок і пустельна місцевість."),
            new("snow", "Сніг", "Снігова або крижана місцевість."),
        };

        private static readonly TerrainRuleTagOption[] BuiltInTagOptions =
            CreateBuiltInTagOptions();

        /// <summary>
        /// Canonical editor catalogue of semantic terrain tags. The returned options
        /// describe available suggestions; profile participation is resolved by
        /// <see cref="BuildCatalog"/>.
        /// </summary>
        public static IReadOnlyList<TerrainRuleTagOption> SuggestedTags =>
            BuiltInTagOptions;

        public static ConstructionPlacementRulesProfileSO ResolvePlacementRulesProfile()
            => ResolvePlacementRulesProfileDetailed().Asset;

        public static ConstructionPlacementRulesProfileSO ResolvePlacementRulesProfile(out string status)
        {
            TerrainRuleEditorResolution<ConstructionPlacementRulesProfileSO> resolution =
                ResolvePlacementRulesProfileDetailed();
            status = resolution.Message;
            return resolution.Asset;
        }

        public static TerrainLayerProfileSO ResolveTerrainLayerProfile()
            => ResolveTerrainLayerProfileDetailed().Asset;

        public static TerrainLayerProfileSO ResolveTerrainLayerProfile(out string status)
        {
            TerrainRuleEditorResolution<TerrainLayerProfileSO> resolution =
                ResolveTerrainLayerProfileDetailed();
            status = resolution.Message;
            return resolution.Asset;
        }

        public static TerrainRuleEditorResolution<ConstructionPlacementRulesProfileSO>
            ResolvePlacementRulesProfileDetailed()
        {
            List<ConstructionSceneContext> sceneContexts = FindInActiveScene<ConstructionSceneContext>();
            if (sceneContexts.Count > 1)
            {
                return Failure<ConstructionPlacementRulesProfileSO>(
                    TerrainRuleEditorResolutionStatus.AmbiguousActiveScene,
                    "В активній сцені знайдено кілька ConstructionSceneContext. Виберіть профіль явно.");
            }

            if (sceneContexts.Count == 1)
            {
                ConstructionPlacementRulesProfileSO sceneAsset = sceneContexts[0].ResolvePlacementRulesProfile();
                if (sceneAsset != null)
                {
                    return Success(
                        sceneAsset,
                        TerrainRuleEditorResolutionStatus.ResolvedFromActiveScene,
                        "Профіль взято з ConstructionSceneContext активної сцени.");
                }
            }

            ConstructionPlacementRulesProfileSO contextAsset =
                MoyvaProjectEditorContext.Get<ConstructionPlacementRulesProfileSO>();
            if (contextAsset != null)
            {
                return Success(
                    contextAsset,
                    TerrainRuleEditorResolutionStatus.ResolvedFromProjectContext,
                    "Профіль взято з контексту редактора Moyva.");
            }

            return ResolveSingleProjectAsset<ConstructionPlacementRulesProfileSO>(
                "Профіль глобальних правил будівництва не знайдено.",
                "Знайдено кілька профілів глобальних правил будівництва. Виберіть профіль явно.");
        }

        public static TerrainRuleEditorResolution<TerrainLayerProfileSO>
            ResolveTerrainLayerProfileDetailed()
        {
            List<MonoBehaviour> installers = FindBehavioursInActiveScene(GridInstallerTypeName);
            if (installers.Count > 1)
            {
                return Failure<TerrainLayerProfileSO>(
                    TerrainRuleEditorResolutionStatus.AmbiguousActiveScene,
                    "В активній сцені знайдено кілька GridInstaller. Виберіть каталог terrain явно.");
            }

            if (installers.Count == 1)
            {
                var serializedInstaller = new SerializedObject(installers[0]);
                SerializedProperty profileProperty = serializedInstaller.FindProperty("terrainLayerProfiles");
                if (profileProperty?.objectReferenceValue is TerrainLayerProfileSO sceneAsset)
                {
                    return Success(
                        sceneAsset,
                        TerrainRuleEditorResolutionStatus.ResolvedFromActiveScene,
                        "Каталог terrain взято з GridInstaller активної сцени.");
                }
            }

            TerrainLayerProfileSO contextAsset = MoyvaProjectEditorContext.Get<TerrainLayerProfileSO>();
            if (contextAsset != null)
            {
                return Success(
                    contextAsset,
                    TerrainRuleEditorResolutionStatus.ResolvedFromProjectContext,
                    "Каталог terrain взято з контексту редактора Moyva.");
            }

            return ResolveSingleProjectAsset<TerrainLayerProfileSO>(
                "Каталог terrain-шарів не знайдено.",
                "Знайдено кілька каталогів terrain-шарів. Виберіть каталог явно.");
        }

        public static TerrainRuleEditorCatalogSnapshot BuildCatalog(TerrainLayerProfileSO profileAsset = null)
        {
            profileAsset ??= ResolveTerrainLayerProfile();

            var discoveredTags = new List<string>();
            var layerCandidates = new List<TerrainRuleLayerOption>();
            if (profileAsset != null)
            {
                IReadOnlyList<TerrainLayerProfile> profiles = profileAsset.Profiles;
                for (int index = 0; index < (profiles?.Count ?? 0); index++)
                    AddProfile(profiles[index], false, discoveredTags, layerCandidates);

                AddProfile(profileAsset.GetProfile(null), true, discoveredTags, layerCandidates);
            }

            string[] normalizedDiscoveredTags = NormalizeValues(discoveredTags.ToArray());
            var tagOptions = new List<TerrainRuleTagOption>(
                SuggestedTagDefinitions.Length + normalizedDiscoveredTags.Length);

            for (int index = 0; index < SuggestedTagDefinitions.Length; index++)
            {
                SuggestedTag suggestion = SuggestedTagDefinitions[index];
                tagOptions.Add(new TerrainRuleTagOption(
                    suggestion.Value,
                    suggestion.DisplayName,
                    suggestion.Description,
                    true,
                    Contains(normalizedDiscoveredTags, suggestion.Value)));
            }

            for (int index = 0; index < normalizedDiscoveredTags.Length; index++)
            {
                string value = normalizedDiscoveredTags[index];
                if (ContainsSuggestedTag(value))
                    continue;

                tagOptions.Add(new TerrainRuleTagOption(
                    value,
                    value,
                    "Власний тег із каталогу terrain-шарів.",
                    false,
                    true));
            }

            tagOptions.Sort((left, right) => CompareStableValues(left.Value, right.Value));
            TerrainRuleLayerOption[] layers = NormalizeLayerOptions(layerCandidates);
            return new TerrainRuleEditorCatalogSnapshot(tagOptions, layers);
        }

        public static string[] NormalizeValues(string[] values)
        {
            if (values == null || values.Length == 0)
                return Array.Empty<string>();

            var result = new List<string>(values.Length);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < values.Length; index++)
            {
                string value = values[index]?.Trim();
                if (string.IsNullOrEmpty(value) || !seen.Add(value))
                    continue;

                result.Add(value);
            }

            result.Sort(CompareStableValues);
            return result.ToArray();
        }

        public static bool Contains(string[] values, string value)
        {
            string normalizedValue = value?.Trim();
            if (string.IsNullOrEmpty(normalizedValue) || values == null)
                return false;

            for (int index = 0; index < values.Length; index++)
            {
                if (string.Equals(
                        values[index]?.Trim(),
                        normalizedValue,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static string[] AddValue(string[] values, string value)
        {
            string normalizedValue = value?.Trim();
            if (string.IsNullOrEmpty(normalizedValue))
                return NormalizeValues(values);

            int sourceLength = values?.Length ?? 0;
            var combined = new string[sourceLength + 1];
            if (sourceLength > 0)
                Array.Copy(values, combined, sourceLength);
            combined[sourceLength] = normalizedValue;
            return NormalizeValues(combined);
        }

        public static string[] RemoveValue(string[] values, string value)
        {
            string normalizedValue = value?.Trim();
            if (string.IsNullOrEmpty(normalizedValue) || values == null || values.Length == 0)
                return NormalizeValues(values);

            var remaining = new List<string>(values.Length);
            for (int index = 0; index < values.Length; index++)
            {
                if (!string.Equals(
                        values[index]?.Trim(),
                        normalizedValue,
                        StringComparison.OrdinalIgnoreCase))
                {
                    remaining.Add(values[index]);
                }
            }

            return NormalizeValues(remaining.ToArray());
        }

        private static void AddProfile(
            TerrainLayerProfile profile,
            bool isFallback,
            ICollection<string> discoveredTags,
            ICollection<TerrainRuleLayerOption> layerCandidates)
        {
            if (profile == null)
                return;

            IReadOnlyList<string> tags = profile.Tags;
            for (int index = 0; index < (tags?.Count ?? 0); index++)
                discoveredTags.Add(tags[index]);

            string layerId = profile.LayerId?.Trim();
            if (string.IsNullOrEmpty(layerId))
                return;

            string displayName = profile.DisplayName?.Trim();
            layerCandidates.Add(new TerrainRuleLayerOption(
                layerId,
                string.IsNullOrEmpty(displayName) ? layerId : displayName,
                isFallback));
        }

        private static TerrainRuleLayerOption[] NormalizeLayerOptions(
            IReadOnlyList<TerrainRuleLayerOption> candidates)
        {
            var result = new List<TerrainRuleLayerOption>(candidates?.Count ?? 0);
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < (candidates?.Count ?? 0); index++)
            {
                TerrainRuleLayerOption candidate = candidates[index];
                string value = candidate.Value?.Trim();
                if (string.IsNullOrEmpty(value) || !seenIds.Add(value))
                    continue;

                string displayName = candidate.DisplayName?.Trim();
                result.Add(new TerrainRuleLayerOption(
                    value,
                    string.IsNullOrEmpty(displayName) ? value : displayName,
                    candidate.IsFallback));
            }

            result.Sort((left, right) =>
            {
                int displayComparison = CompareStableValues(left.DisplayName, right.DisplayName);
                return displayComparison != 0
                    ? displayComparison
                    : CompareStableValues(left.Value, right.Value);
            });
            return result.ToArray();
        }

        private static bool ContainsSuggestedTag(string value)
        {
            for (int index = 0; index < SuggestedTagDefinitions.Length; index++)
            {
                if (string.Equals(
                        SuggestedTagDefinitions[index].Value,
                        value,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static TerrainRuleTagOption[] CreateBuiltInTagOptions()
        {
            var options = new TerrainRuleTagOption[SuggestedTagDefinitions.Length];
            for (int index = 0; index < SuggestedTagDefinitions.Length; index++)
            {
                SuggestedTag suggestion = SuggestedTagDefinitions[index];
                options[index] = new TerrainRuleTagOption(
                    suggestion.Value,
                    suggestion.DisplayName,
                    suggestion.Description,
                    true,
                    false);
            }

            return options;
        }

        private static int CompareStableValues(string left, string right)
        {
            int comparison = StringComparer.OrdinalIgnoreCase.Compare(left, right);
            return comparison != 0 ? comparison : StringComparer.Ordinal.Compare(left, right);
        }

        private static List<T> FindInActiveScene<T>() where T : Component
        {
            var result = new List<T>();
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded)
                return result;

            GameObject[] roots = activeScene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                result.AddRange(roots[rootIndex].GetComponentsInChildren<T>(true));

            return result;
        }

        private static List<MonoBehaviour> FindBehavioursInActiveScene(string fullTypeName)
        {
            List<MonoBehaviour> behaviours = FindInActiveScene<MonoBehaviour>();
            var result = new List<MonoBehaviour>();
            for (int index = 0; index < behaviours.Count; index++)
            {
                MonoBehaviour behaviour = behaviours[index];
                if (behaviour != null
                    && string.Equals(
                        behaviour.GetType().FullName,
                        fullTypeName,
                        StringComparison.Ordinal))
                {
                    result.Add(behaviour);
                }
            }

            return result;
        }

        private static TerrainRuleEditorResolution<T> ResolveSingleProjectAsset<T>(
            string missingMessage,
            string ambiguousMessage) where T : UnityEngine.Object
        {
            List<T> assets = FindProjectAssets<T>();
            if (assets.Count == 1)
            {
                return Success(
                    assets[0],
                    TerrainRuleEditorResolutionStatus.ResolvedFromSingleAsset,
                    "Використано єдиний відповідний asset у проєкті.");
            }

            return assets.Count == 0
                ? Failure<T>(TerrainRuleEditorResolutionStatus.Missing, missingMessage)
                : Failure<T>(TerrainRuleEditorResolutionStatus.AmbiguousAssets, ambiguousMessage);
        }

        private static List<T> FindProjectAssets<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            Array.Sort(guids, StringComparer.OrdinalIgnoreCase);
            var result = new List<T>(guids.Length);
            var seenIds = new HashSet<int>();
            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null && seenIds.Add(asset.GetInstanceID()))
                    result.Add(asset);
            }

            return result;
        }

        private static TerrainRuleEditorResolution<T> Success<T>(
            T asset,
            TerrainRuleEditorResolutionStatus status,
            string message) where T : UnityEngine.Object
            => new(asset, status, message);

        private static TerrainRuleEditorResolution<T> Failure<T>(
            TerrainRuleEditorResolutionStatus status,
            string message) where T : UnityEngine.Object
            => new(null, status, message);
    }
}

#endif
