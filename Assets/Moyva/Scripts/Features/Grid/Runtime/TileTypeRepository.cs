using System;
using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Jsonization;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Grid.Runtime
{
    internal sealed class TileTypeRepository : ITileTypeRepository
    {
        private readonly Dictionary<string, TileTypeSnapshot> _byId =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _canonicalByIdOrAlias =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly IReadOnlyCollection<TileTypeSnapshot> _all;

        [Inject]
        public TileTypeRepository(
            [InjectOptional] TileRegistrySO legacyRegistry = null,
            [InjectOptional] TerrainLayerProfileSO legacyProfiles = null)
            : this(LoadCanonicalConfigs(), legacyRegistry, legacyProfiles)
        {
        }

        internal TileTypeRepository(
            IEnumerable<TileTypeConfig> configs,
            TileRegistrySO legacyRegistry = null,
            TerrainLayerProfileSO legacyProfiles = null)
        {
            // JSON має пріоритет. Legacy SO додаються тільки як тимчасова сумісність для сцен/сейвів,
            // які ще можуть посилатися на старі visual IDs.
            foreach (TileTypeConfig config in configs ?? Array.Empty<TileTypeConfig>())
                AddCanonical(config);

            AddLegacyRegistryEntries(legacyRegistry);
            AddLegacyTerrainProfiles(legacyProfiles);

            if (_byId.Count == 0)
            {
                throw new InvalidOperationException(
                    "[MoyvaTiles] No tile definitions were loaded from JSON or legacy configuration.");
            }

            TileTypeSnapshot[] ordered = _byId.Values
                .OrderBy(value => value.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            _all = Array.AsReadOnly(ordered);
            Revision = ComputeRevision(ordered.Select(value => value.Id));
        }

        public int Revision { get; }
        public IReadOnlyCollection<TileTypeSnapshot> All => _all;

        public bool TryGet(string tileTypeId, out TileTypeSnapshot tileType)
        {
            tileType = null;
            return TryResolveId(tileTypeId, out string canonicalId)
                && _byId.TryGetValue(canonicalId, out tileType);
        }

        public bool TryResolveId(string tileTypeIdOrAlias, out string canonicalTileTypeId)
        {
            canonicalTileTypeId = null;
            if (string.IsNullOrWhiteSpace(tileTypeIdOrAlias))
                return false;

            return _canonicalByIdOrAlias.TryGetValue(
                tileTypeIdOrAlias.Trim(),
                out canonicalTileTypeId);
        }

        private static IReadOnlyList<TileTypeConfig> LoadCanonicalConfigs()
        {
            try
            {
                return MoyvaJsonRuntime.GetAll<TileTypeConfig>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "[MoyvaTiles] Failed to load canonical tile JSON documents.",
                    ex);
            }
        }

        private void AddCanonical(TileTypeConfig config)
        {
            if (config == null)
                return;

            string id = RequireId(config.JsonId, "tile-type");
            ValidateVisual(id, config.Visual);

            string traversalClassId = config.TraversalClassId?.Trim();
            if (string.IsNullOrWhiteSpace(traversalClassId))
            {
                throw ValidationError(
                    id,
                    "/traversalClassId",
                    "a non-empty traversal class is required");
            }

            var snapshot = new TileTypeSnapshot(
                id,
                config.DisplayName,
                traversalClassId,
                config.Aliases,
                config.Tags,
                config.Visual);

            AddSnapshot(snapshot, failOnCollision: true);
        }

        private void AddLegacyRegistryEntries(TileRegistrySO registry)
        {
            if (registry?.Definitions == null)
                return;

            for (int i = 0; i < registry.Definitions.Length; i++)
            {
                TileTypeDefinition definition = registry.Definitions[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    continue;

                if (TryResolveLegacyCanonicalId(definition, out string canonicalId))
                {
                    AddIdentity(definition.Id, canonicalId, failOnCollision: false);
                    continue;
                }

                var visual = new TileVisualConfig
                {
                    GridMode = definition.TileWorldCreatorPreset != null
                        && definition.TileWorldCreatorPreset.gridtype == TilePreset.GridType.dual
                            ? TileGridMode.Dual
                            : TileGridMode.Normal,
                    RepresentativePrefab = definition.VisualPrefab,
                };

                if (definition.TileWorldCreatorPreset != null)
                {
                    visual.Variants.Add(new TileVisualVariantConfig
                    {
                        Preset = definition.TileWorldCreatorPreset,
                        Slot = TileVisualSlot.Top,
                        Weight = 1f,
                    });
                }

                AddSnapshot(new TileTypeSnapshot(
                    definition.Id.Trim(),
                    definition.Id.Trim(),
                    definition.Id.Trim(),
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    visual), failOnCollision: false);
            }
        }

        private bool TryResolveLegacyCanonicalId(
            TileTypeDefinition definition,
            out string canonicalId)
        {
            canonicalId = null;
            string legacyId = definition.Id?.Trim() ?? string.Empty;
            string prefixCandidate = ResolveLegacyPrefixCandidate(legacyId);
            if (!string.IsNullOrWhiteSpace(prefixCandidate)
                && _byId.ContainsKey(prefixCandidate))
            {
                canonicalId = prefixCandidate;
                return true;
            }

            TilePreset preset = definition.TileWorldCreatorPreset;
            if (preset == null)
                return false;

            string match = null;
            foreach (TileTypeSnapshot candidate in _byId.Values)
            {
                bool usesPreset = candidate.Visual.Variants.Any(
                    variant => variant.Preset == preset);
                if (!usesPreset)
                    continue;

                if (match != null)
                    return false;
                match = candidate.Id;
            }

            canonicalId = match;
            return canonicalId != null;
        }

        private static string ResolveLegacyPrefixCandidate(string legacyId)
        {
            if (legacyId.StartsWith("water", StringComparison.OrdinalIgnoreCase))
                return "water";
            if (legacyId.StartsWith("stone-hill", StringComparison.OrdinalIgnoreCase))
                return "hill";
            if (legacyId.StartsWith("grass-coast", StringComparison.OrdinalIgnoreCase)
                || legacyId.StartsWith("sand", StringComparison.OrdinalIgnoreCase))
                return "sand";
            if (legacyId.StartsWith("snow", StringComparison.OrdinalIgnoreCase))
                return "snow";
            if (legacyId.StartsWith("forest-dense", StringComparison.OrdinalIgnoreCase))
                return "forest-dense";
            if (legacyId.StartsWith("forest", StringComparison.OrdinalIgnoreCase))
                return "forest-sparse";
            if (legacyId.StartsWith("grass", StringComparison.OrdinalIgnoreCase)
                || legacyId.StartsWith("texture-grass", StringComparison.OrdinalIgnoreCase)
                || string.Equals(legacyId, "TileGrass", StringComparison.OrdinalIgnoreCase))
                return "grass";
            return null;
        }

        private void AddLegacyTerrainProfiles(TerrainLayerProfileSO profiles)
        {
            if (profiles?.Profiles == null)
                return;

            for (int i = 0; i < profiles.Profiles.Count; i++)
            {
                TerrainLayerProfile profile = profiles.Profiles[i];
                if (profile == null || string.IsNullOrWhiteSpace(profile.LayerId))
                    continue;

                string traversalClass = profile.Tags != null && profile.Tags.Count > 0
                    ? profile.Tags[0]
                    : profile.LayerId;
                var visual = new TileVisualConfig
                {
                    GridMode = TileGridMode.Flat,
                    SurfaceOffset = profile.SurfaceOffset,
                };

                AddSnapshot(new TileTypeSnapshot(
                    profile.LayerId.Trim(),
                    profile.DisplayName,
                    traversalClass,
                    Array.Empty<string>(),
                    profile.Tags,
                    visual), failOnCollision: false);
            }
        }

        private void AddSnapshot(TileTypeSnapshot snapshot, bool failOnCollision)
        {
            if (_byId.ContainsKey(snapshot.Id))
            {
                if (failOnCollision)
                {
                    throw ValidationError(
                        snapshot.Id,
                        "/id",
                        $"duplicate tile id '{snapshot.Id}'");
                }

                return;
            }

            _byId.Add(snapshot.Id, snapshot);
            AddIdentity(snapshot.Id, snapshot.Id, failOnCollision);
            for (int i = 0; i < snapshot.Aliases.Count; i++)
                AddIdentity(snapshot.Aliases[i], snapshot.Id, failOnCollision);
        }

        private void AddIdentity(string identity, string canonicalId, bool failOnCollision)
        {
            if (string.IsNullOrWhiteSpace(identity))
                return;

            string normalized = identity.Trim();
            if (_canonicalByIdOrAlias.TryGetValue(normalized, out string existing))
            {
                if (failOnCollision
                    && !string.Equals(existing, canonicalId, StringComparison.OrdinalIgnoreCase))
                {
                    throw ValidationError(
                        canonicalId,
                        "/aliases",
                        $"identity '{normalized}' is already owned by '{existing}'");
                }

                return;
            }

            _canonicalByIdOrAlias.Add(normalized, canonicalId);
        }

        private static void ValidateVisual(string tileId, TileVisualConfig visual)
        {
            if (visual == null)
                throw ValidationError(tileId, "/visual", "visual settings are required");

            if (visual.GridMode == TileGridMode.Flat)
            {
                if (visual.FlatSurfaceMaterial == null)
                {
                    throw ValidationError(
                        tileId,
                        "/visual/flatSurfaceMaterial",
                        "Flat tiles require a material");
                }

                return;
            }

            if (visual.Variants == null || visual.Variants.Count == 0)
            {
                throw ValidationError(
                    tileId,
                    "/visual/variants",
                    "Normal and Dual tiles require at least one TilePreset variant");
            }

            bool hasPositiveWeight = false;
            for (int i = 0; i < visual.Variants.Count; i++)
            {
                TileVisualVariantConfig variant = visual.Variants[i];
                string path = $"/visual/variants/{i}";
                if (variant?.Preset == null)
                    throw ValidationError(tileId, path + "/preset", "TilePreset is required");

                bool presetIsDual = variant.Preset.gridtype == TilePreset.GridType.dual;
                bool configIsDual = visual.GridMode == TileGridMode.Dual;
                if (presetIsDual != configIsDual)
                {
                    throw ValidationError(
                        tileId,
                        path + "/preset",
                        $"preset grid mode is {(presetIsDual ? "Dual" : "Normal")}, " +
                        $"but JSON declares {visual.GridMode}");
                }

                if (!IsFinite(variant.Weight) || variant.Weight < 0f || variant.Weight > 1f)
                    throw ValidationError(tileId, path + "/weight", "expected a value in range 0..1");
                if (!IsFinite(variant.TileHeight) || variant.TileHeight < 0f)
                    throw ValidationError(tileId, path + "/tileHeight", "expected a finite value >= 0");

                hasPositiveWeight |= variant.Weight > 0f;
            }

            if (!hasPositiveWeight)
            {
                throw ValidationError(
                    tileId,
                    "/visual/variants",
                    "at least one variant must have weight > 0");
            }
        }

        private static string RequireId(string value, string model)
        {
            string id = value?.Trim();
            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException($"[MoyvaTiles] {model} document has an empty /id.");
            return id;
        }

        private static InvalidOperationException ValidationError(
            string id,
            string pointer,
            string message)
            => new($"[MoyvaTiles] tile-type '{id}' {pointer}: {message}.");

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);

        private static int ComputeRevision(IEnumerable<string> values)
        {
            unchecked
            {
                int hash = 17;
                foreach (string value in values)
                {
                    for (int i = 0; i < value.Length; i++)
                        hash = hash * 31 + char.ToLowerInvariant(value[i]);
                }

                return hash;
            }
        }
    }
}
