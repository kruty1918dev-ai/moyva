using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildingRegistrySnapshot : IBuildingRegistry
    {
        private readonly IBuildingRegistry _source;
        private readonly Dictionary<string, BuildingDefinition> _byId = new(StringComparer.OrdinalIgnoreCase);
        private BuildingDefinition[] _definitions;
        private int _capturedRuntimeRevision = -1;

        public ConstructionBuildingRegistrySnapshot(IBuildingRegistry source)
        {
            _source = source;
        }

        public BuildingDefinition[] GetAll()
        {
            EnsureInitialized();
            return _definitions;
        }

        public BuildingDefinition GetById(string id)
        {
            EnsureInitialized();
            return !string.IsNullOrWhiteSpace(id) && _byId.TryGetValue(id, out BuildingDefinition definition)
                ? definition
                : null;
        }

        public BuildingDefinition[] GetByCategory(BuildingCategory category)
        {
            EnsureInitialized();
            return Array.FindAll(_definitions, definition => definition != null && definition.Category == category);
        }

        public WallCollectionDefinition[] GetWallCollections()
            => _source?.GetWallCollections() ?? Array.Empty<WallCollectionDefinition>();

        public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId)
            => _source?.GetWallCollectionByBuildingId(buildingId);

        private void EnsureInitialized()
        {
            int currentRevision =
                BuildingDefinitionAsset.RuntimeRevision;
            if (_definitions != null
                && _capturedRuntimeRevision == currentRevision)
            {
                return;
            }

            _byId.Clear();
            _definitions =
                _source?.GetAll()
                ?? Array.Empty<BuildingDefinition>();

            for (int i = 0; i < _definitions.Length; i++)
            {
                BuildingDefinition definition =
                    _definitions[i];
                if (definition != null
                    && !string.IsNullOrWhiteSpace(definition.Id))
                {
                    _byId[definition.Id] = definition;
                }
            }

            _capturedRuntimeRevision = currentRevision;

            if (Application.isEditor && Debug.isDebugBuild)
            {
            }
        }
    }
}
