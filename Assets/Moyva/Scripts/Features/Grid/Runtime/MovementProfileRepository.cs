using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Jsonization;

namespace Kruty1918.Moyva.Grid.Runtime
{
    /// <summary>
    /// Внутрішній catalog профілів руху. Зовнішні системи не читають його напряму,
    /// а запитують готову ціну через ITraversalCostResolver.
    /// </summary>
    internal sealed class MovementProfileRepository
    {
        private readonly Dictionary<string, MovementProfileSnapshot> _byId =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly IReadOnlyCollection<MovementProfileSnapshot> _all;

        public MovementProfileRepository()
            : this(LoadCanonicalConfigs())
        {
        }

        internal MovementProfileRepository(IEnumerable<MovementProfileConfig> configs)
        {
            foreach (MovementProfileConfig config in configs ?? Array.Empty<MovementProfileConfig>())
                Add(config);

            if (!_byId.ContainsKey(MovementProfileIds.GroundDefault))
            {
                _byId.Add(
                    MovementProfileIds.GroundDefault,
                    new MovementProfileSnapshot(
                        MovementProfileIds.GroundDefault,
                        new MovementRuleSnapshot(true, 1f),
                        new Dictionary<string, MovementRuleSnapshot>(StringComparer.OrdinalIgnoreCase),
                        new Dictionary<string, MovementRuleSnapshot>(StringComparer.OrdinalIgnoreCase)));
            }

            MovementProfileSnapshot[] ordered = _byId.Values
                .OrderBy(value => value.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            _all = Array.AsReadOnly(ordered);
            Revision = ComputeRevision(ordered.Select(value => value.Id));
        }

        public int Revision { get; }
        public IReadOnlyCollection<MovementProfileSnapshot> All => _all;

        public bool TryGet(string profileId, out MovementProfileSnapshot profile)
        {
            string resolvedId = string.IsNullOrWhiteSpace(profileId)
                ? MovementProfileIds.GroundDefault
                : profileId.Trim();
            return _byId.TryGetValue(resolvedId, out profile);
        }

        private static IReadOnlyList<MovementProfileConfig> LoadCanonicalConfigs()
        {
            try
            {
                return MoyvaJsonRuntime.GetAll<MovementProfileConfig>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "[MoyvaTraversal] Failed to load movement-profile JSON documents.",
                    ex);
            }
        }

        private void Add(MovementProfileConfig config)
        {
            if (config == null)
                return;

            string id = config.JsonId?.Trim();
            if (string.IsNullOrWhiteSpace(id))
                throw ValidationError("<unknown>", "/id", "a non-empty id is required");
            if (_byId.ContainsKey(id))
                throw ValidationError(id, "/id", "duplicate movement profile id");
            if (config.Fallback == null)
                throw ValidationError(id, "/fallback", "fallback is required");

            MovementRuleSnapshot fallback = BuildRule(
                id,
                "/fallback",
                config.Fallback.Passable,
                config.Fallback.StaminaCost);

            var classRules = new Dictionary<string, MovementRuleSnapshot>(
                StringComparer.OrdinalIgnoreCase);
            if (config.ClassRules != null)
            {
                for (int i = 0; i < config.ClassRules.Count; i++)
                {
                    MovementClassRuleConfig rule = config.ClassRules[i];
                    string path = $"/classRules/{i}";
                    string classId = rule?.ClassId?.Trim();
                    if (string.IsNullOrWhiteSpace(classId))
                        throw ValidationError(id, path + "/classId", "a non-empty class id is required");
                    if (classRules.ContainsKey(classId))
                        throw ValidationError(id, path + "/classId", $"duplicate class rule '{classId}'");

                    classRules.Add(
                        classId,
                        BuildRule(id, path, rule.Passable, rule.StaminaCost));
                }
            }

            var tileOverrides = new Dictionary<string, MovementRuleSnapshot>(
                StringComparer.OrdinalIgnoreCase);
            if (config.TileOverrides != null)
            {
                for (int i = 0; i < config.TileOverrides.Count; i++)
                {
                    MovementTileOverrideConfig rule = config.TileOverrides[i];
                    string path = $"/tileOverrides/{i}";
                    string tileTypeId = rule?.TileTypeId?.Trim();
                    if (string.IsNullOrWhiteSpace(tileTypeId))
                        throw ValidationError(id, path + "/tileTypeId", "a non-empty tile id is required");
                    if (tileOverrides.ContainsKey(tileTypeId))
                        throw ValidationError(id, path + "/tileTypeId", $"duplicate tile override '{tileTypeId}'");

                    tileOverrides.Add(
                        tileTypeId,
                        BuildRule(id, path, rule.Passable, rule.StaminaCost));
                }
            }

            _byId.Add(id, new MovementProfileSnapshot(id, fallback, classRules, tileOverrides));
        }

        private static MovementRuleSnapshot BuildRule(
            string profileId,
            string path,
            bool passable,
            float staminaCost)
        {
            if (passable
                && (float.IsNaN(staminaCost)
                    || float.IsInfinity(staminaCost)
                    || staminaCost <= 0f))
            {
                throw ValidationError(
                    profileId,
                    path + "/staminaCost",
                    "passable rules require a finite cost > 0");
            }

            return new MovementRuleSnapshot(passable, passable ? staminaCost : 0f);
        }

        private static InvalidOperationException ValidationError(
            string id,
            string pointer,
            string message)
            => new($"[MoyvaTraversal] movement-profile '{id}' {pointer}: {message}.");

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

    internal sealed class TraversalCostResolver : ITraversalCostResolver
    {
        private readonly ITileTypeRepository _tiles;
        private readonly MovementProfileRepository _profiles;

        public TraversalCostResolver(
            ITileTypeRepository tiles,
            MovementProfileRepository profiles)
        {
            _tiles = tiles;
            _profiles = profiles;
        }

        public bool TryResolve(
            string movementProfileId,
            string tileTypeId,
            out float staminaCost,
            out string reason)
        {
            // Resolver навмисно не знає про fog, occupancy чи stamina залишок.
            // Він відповідає тільки за terrain/profile ціну, щоб preview і execution мали одну математику.
            staminaCost = 0f;
            reason = null;

            if (!_profiles.TryGet(movementProfileId, out MovementProfileSnapshot profile))
            {
                reason = $"Профіль руху '{movementProfileId}' не знайдено.";
                return false;
            }

            if (!_tiles.TryGet(tileTypeId, out TileTypeSnapshot tile))
            {
                reason = $"Тип тайла '{tileTypeId}' не зареєстрований.";
                return false;
            }

            MovementRuleSnapshot rule;
            if (!profile.TryGetTileOverride(tile.Id, out rule)
                && !profile.TryGetTileOverride(tileTypeId, out rule)
                && !profile.TryGetClassRule(tile.TraversalClassId, out rule))
            {
                rule = profile.Fallback;
            }

            if (!rule.Passable)
            {
                reason = $"Профіль '{profile.Id}' не дозволяє рух по тайлу '{tile.Id}'.";
                return false;
            }

            staminaCost = rule.StaminaCost;
            return true;
        }
    }
}
