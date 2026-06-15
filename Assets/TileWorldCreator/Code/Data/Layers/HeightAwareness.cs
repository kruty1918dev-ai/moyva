/*
	TileWorldCreator (c) by Giant Grey
	Author: Marc Egli
	www.giantgrey.com
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using GiantGrey.TileWorldCreator.Components;
using GiantGrey.TileWorldCreator.UI;
using GiantGrey.TileWorldCreator.Attributes;
#if UNITY_EDITOR
using UnityEditor;
using Editor = UnityEditor.Editor;
using UnityEditor.UIElements;
#endif

namespace GiantGrey.TileWorldCreator
{
    [System.Serializable]
    [BuildLayer("Height Awareness", "Tiles.twc")]
    public class HeightAwareness : TilesBuildLayer
    {
        [System.Serializable]
        public class OverlapReplacementRule
        {
            public TilePreset.TileType originalType;
            [TilePresetPopup]
            public TilePreset replacementPreset;
            public TilePreset.TileType targetCellType = TilePreset.TileType.none;
            public bool useTargetRotation = false;
        }

        [System.Serializable]
        public class OverlapLayerConfig
        {
            public string layerName; // For UI
            [BlueprintLayerDropdownAttribute()]
            public string blueprintLayerGuid;
            public List<OverlapReplacementRule> replacementRules = new List<OverlapReplacementRule>();
            public bool checkSelf = false;
        }

        public List<OverlapLayerConfig> overlapConfigs = new List<OverlapLayerConfig>();
        
        private Dictionary<Vector2, float> tileHeightOverrides = new Dictionary<Vector2, float>();
        private Dictionary<Vector2, TilePreset> tilePresetOverrides = new Dictionary<Vector2, TilePreset>();
        private Dictionary<Vector2, float> tileRotationOverrides = new Dictionary<Vector2, float>();

        private TileWorldCreatorManager manager;

        public override void ExecuteLayer(Configuration _configuration, GameObject _owner, TileWorldCreatorManager _manager)
        {
            // Clear overrides before execution
            tileHeightOverrides.Clear();
            tilePresetOverrides.Clear();
            tileRotationOverrides.Clear();

            manager = _manager;
            
            base.ExecuteLayer(_configuration, _owner, _manager);
        }

        protected override void InstantiateTile(TileData _tileData, int _clusterKey, int _tileLayerIndex)
        {
            // Check if we have overrides for this tile position
            if (tileHeightOverrides.TryGetValue(_tileData.tilePosition, out float overriddenHeight))
            {
                // We need to pass the overridden height to the actual instantiation logic
                // Since base.InstantiateTile calculates it, we have to reimplement or adjust it.
                // However, base.InstantiateTile uses currentBlueprintLayer.defaultLayerHeight.
        
                // Let's reimplement enough of InstantiateTile to support the custom height.
                InstantiateTileWithHeight(_tileData, _clusterKey, _tileLayerIndex, overriddenHeight);
            }
            else
            {
                base.InstantiateTile(_tileData, _clusterKey, _tileLayerIndex);
            }
        }

        private void InstantiateTileWithHeight(TileData _tileData, int _clusterKey, int _tileLayerIndex, float customHeight)
        {
             var _cluster = FindCluster(_clusterKey);

            uint _seed;
            int x = Mathf.FloorToInt(_tileData.tilePosition.x * 1000f);
            int y = Mathf.FloorToInt(_tileData.tilePosition.y * 1000f);
            
            uint hash = Unity.Mathematics.math.hash(new Unity.Mathematics.int3(
                x,
                y,
                configuration.useGlobalRandomSeed ? configuration.globalRandomSeed : (int)configuration.currentRandomSeed
            ));

            _seed = hash;
            if (_seed == 0) _seed = 1;
            
            Unity.Mathematics.Random _tileRandom = new Unity.Mathematics.Random(_seed);


            // Apply preset override if exists
            var _tilePreset = GetRandomTilePreset(_tileData, _tileLayerIndex, ref _tileRandom);
            if (tilePresetOverrides.TryGetValue(_tileData.tilePosition, out var overriddenPreset))
            {
                _tilePreset = overriddenPreset;
            }
            
            GameObject _prefab = null;
            Material _materialOverride = null;
            if (_tilePreset != null)
            {
                _prefab = _tilePreset.GetTile(_tileData.tileType, out _tileData.xRotationOffset, out _tileData.yRotationOffset);
                _materialOverride = _tilePreset.GetMaterialOverride();
            }

            if (tileRotationOverrides.TryGetValue(_tileData.tilePosition, out var overriddenRotation))
            {
                _tileData.yRotation = Mathf.RoundToInt(overriddenRotation);
                _tileData.yRotationOffset = 0; // Reset offset if we use absolute target rotation
            }

            if (_prefab != null)
            {
                var _newTile = GameObject.Instantiate(_prefab, Vector3.zero, Quaternion.Euler(new Vector3(_tileData.xRotationOffset, _tileData.yRotation + _tileData.yRotationOffset, 0)));

                if (_materialOverride != null)
                {
                    _newTile.GetComponent<MeshRenderer>().material = _materialOverride;
                }

                if (scaleTileToCellSize)
                {
                    _newTile.transform.localScale = new Vector3(_newTile.transform.localScale.x * scaleOffset.x * configuration.cellSize, _newTile.transform.localScale.y * scaleOffset.y * configuration.cellSize, _newTile.transform.localScale.z * scaleOffset.z * configuration.cellSize);
                }
                else
                {
                    _newTile.transform.localScale = new Vector3(_newTile.transform.localScale.x * scaleOffset.x, _newTile.transform.localScale.y * scaleOffset.y, _newTile.transform.localScale.z * scaleOffset.z);
                }

                if (!useDualGrid)
                {
                    if (TileConfigurations.NRMGRD_minusXScale_configurations.Contains(_tileData.configuration))
                    {
                        _newTile.transform.localScale = new Vector3(_newTile.transform.localScale.x * -1, _newTile.transform.localScale.y, _newTile.transform.localScale.z);
                    }
                }

                _newTile.transform.SetParent(_cluster.transform, false);
                // Correct position using customHeight instead of currentBlueprintLayer.defaultLayerHeight
                var _newTilePosition = new Vector3((_tileData.tilePosition.x) * configuration.cellSize, customHeight + layerYOffset + (tileLayers[_tileLayerIndex].heightOffset), (_tileData.tilePosition.y) * configuration.cellSize);
                _newTile.transform.localPosition = _newTilePosition;
            }
        }

        protected override IEnumerator InstantiateByClusters(List<SortedTiles> _sortedTiles)
        {
            // Pre-calculate overlaps before instantiation starts
            CalculateOverlaps(_sortedTiles);
            
            return base.InstantiateByClusters(_sortedTiles);
        }

        private void CalculateOverlaps(List<SortedTiles> _sortedTiles)
        {
            if (currentBlueprintLayer == null || configuration == null) return;

            // Cache blueprint layers and their corresponding build layers for performance
            var configs = overlapConfigs.Where(c => !string.IsNullOrEmpty(c.blueprintLayerGuid) || c.checkSelf).ToList();
            var overlapLayers = new Dictionary<OverlapLayerConfig, BlueprintLayer>();
            var overlapBuildLayers = new Dictionary<OverlapLayerConfig, List<BuildLayer>>();
            var overlapPositions = new Dictionary<OverlapLayerConfig, HashSet<Vector2>>();

            foreach (var config in configs)
            {
                BlueprintLayer layer = config.checkSelf ? currentBlueprintLayer : configuration.GetBlueprintLayerByGuid(config.blueprintLayerGuid);
                if (layer != null)
                {
                    overlapLayers[config] = layer;
                    var pos = new HashSet<Vector2>();
                    overlapPositions[config] = layer.GetAllCellPositions(pos);

                    // Find build layers that use this blueprint layer
                    var buildLayers = new List<BuildLayer>();
                    foreach (var folder in configuration.buildLayerFolders)
                    {
                        foreach (var bl in folder.buildLayers)
                        {
                            if (bl.assignedBlueprintLayerGuid == layer.guid)
                            {
                                buildLayers.Add(bl);
                            }
                        }
                    }
                    overlapBuildLayers[config] = buildLayers;
                }
            }
            

            foreach (var sortedTile in _sortedTiles)
            {
                foreach (var tile in sortedTile.tiles)
                {
                    foreach (var config in configs)
                    {
                        if (!overlapLayers.TryGetValue(config, out var otherLayer)) continue;
                        
                        // Standardize position to integers to match blueprint grid (which uses integer keys)
                        // Use pre-calculated contributing cells if available, otherwise fallback to rounding
                        List<Vector2> contributingCells = new List<Vector2>();
                        if (tile.contributingCells != null && tile.contributingCells.Length > 0)
                        {
                            contributingCells.AddRange(tile.contributingCells);
                        }
                        else if (useDualGrid)
                        {
                            // Fallback for cases where contributingCells wasn't populated
                            contributingCells.Add(new Vector2(Mathf.Ceil(tile.tilePosition.x), Mathf.Ceil(tile.tilePosition.y)));
                        }
                        else
                        {
                            contributingCells.Add(new Vector2(Mathf.Round(tile.tilePosition.x), Mathf.Round(tile.tilePosition.y)));
                        }

                        foreach (var roundedPosition in contributingCells)
                        {
                            bool isOverlap = overlapPositions[config].Contains(roundedPosition);
                            
                            if (isOverlap)
                            {
                                // Get all rules that might apply to the current tile's type
                                var applicableRules = config.replacementRules.Where(r => r.originalType == tile.tileType).ToList();
                                
                                foreach (var rule in applicableRules)
                                {
                                    // REPLACEMENT & DELETION LOGIC (Exact position match required)
                                    if (rule.targetCellType == TilePreset.TileType.none)
                                    {
                                        // Rule applies to any overlap
                                        tilePresetOverrides[tile.tilePosition] = rule.replacementPreset;
                                        break; 
                                    }
                                    else
                                    {
                                        // Check if any build layer at this position has the targetCellType
                                        if (overlapBuildLayers.TryGetValue(config, out var bLayers))
                                        {
                                            foreach (var bl in bLayers)
                                            {
                                                var otherTiles = bl.GetTilesAtBlueprintCell(roundedPosition);
                                                
                                                // Determine if we should enforce exact position for replacement and deletion
                                                bool currentIsDual = useDualGrid;
                                                bool otherIsDual = (bl is TilesBuildLayer tbl) ? tbl.useDualGrid : false;

                                                foreach (var otherTileData in otherTiles)
                                                {
                                                    if (!otherTileData.isAssigned) continue;

                                                    // Use position-based matching for replacement
                                                    if (currentIsDual == otherIsDual && otherTileData.tilePosition == tile.tilePosition)
                                                    {
                                                        if (otherTileData.tileType == rule.targetCellType)
                                                        {
                                                            // Match! Replace current tile
                                                            tilePresetOverrides[tile.tilePosition] = rule.replacementPreset;
                                                            if (rule.useTargetRotation)
                                                            {
                                                                tileRotationOverrides[tile.tilePosition] = otherTileData.yRotation;
                                                            }
                                                        }
                                                    }
                                                    else if (currentIsDual != otherIsDual)
                                                    {
                                                        // Fallback for mixed grid modes (unlikely but possible)
                                                        // Check if the other tile's position is close enough to be considered overlapping
                                                        if (Vector2.Distance(otherTileData.tilePosition, tile.tilePosition) < 0.75f)
                                                        {
                                                            if (otherTileData.tileType == rule.targetCellType)
                                                            {
                                                                tilePresetOverrides[tile.tilePosition] = rule.replacementPreset;
                                                                if (rule.useTargetRotation)
                                                                {
                                                                    tileRotationOverrides[tile.tilePosition] = otherTileData.yRotation;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                // Apply Height Synchronization (always sync height if there's an overlap, even if no rules match)
                                tileHeightOverrides[tile.tilePosition] = otherLayer.defaultLayerHeight;
                            }
                        }
                    }
                }
            }
        }


        #if UNITY_EDITOR
        public override VisualElement CreateInspectorGUI(Configuration _asset, global::UnityEditor.Editor _editor, LayerFoldoutElement _foldout)
        {
            var _root = base.CreateInspectorGUI(_asset, _editor, _foldout);
            
            var _heightAwarenessSection = new VisualElement();
            _heightAwarenessSection.style.marginTop = 10;
            _heightAwarenessSection.style.paddingTop = 5;
            _heightAwarenessSection.style.borderTopWidth = 1;
            _heightAwarenessSection.style.borderTopColor = Color.gray;
            
            var _title = new Label("Height Awareness Settings");
            _title.style.unityFontStyleAndWeight = FontStyle.Bold;
            _heightAwarenessSection.Add(_title);

            var _serializedObject = new SerializedObject(this);
            var _overlapConfigsProperty = _serializedObject.FindProperty("overlapConfigs");
            
            var _listView = new ListView();
            _listView.BindProperty(_overlapConfigsProperty);
            _listView.headerTitle = "Overlap Configurations";
            _listView.showAddRemoveFooter = true;
            _listView.reorderable = true;
            _listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            
            _listView.makeItem = () => 
            {
                var container = new VisualElement();
                container.style.paddingBottom = 5;
                container.style.borderBottomWidth = 1;
                container.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);

                var checkSelf = new PropertyField();
                var layerGuid = new VisualElement(); // Placeholder for custom dropdown
                var rules = new PropertyField();
                
                container.Add(checkSelf);
                container.Add(layerGuid);
                container.Add(rules);
                return container;
            };

            _listView.bindItem = (item, index) => 
            {
                var prop = _overlapConfigsProperty.GetArrayElementAtIndex(index);
                var checkSelfProp = prop.FindPropertyRelative("checkSelf");
                var layerGuidProp = prop.FindPropertyRelative("blueprintLayerGuid");
                var rulesProp = prop.FindPropertyRelative("replacementRules");

                var checkSelfField = item.ElementAt(0) as PropertyField;
                checkSelfField.BindProperty(checkSelfProp);

                var layerDropdownContainer = item.ElementAt(1);
                layerDropdownContainer.Clear();
                
                var dropdown = new LayerSelectDropdownElement(_asset, layerGuidProp.stringValue, (name, guid) => 
                {
                    layerGuidProp.stringValue = guid;
                    prop.FindPropertyRelative("layerName").stringValue = name;
                    prop.serializedObject.ApplyModifiedProperties();
                }, "Overlap Layer");
                layerDropdownContainer.Add(dropdown);

                var rulesField = item.ElementAt(2) as PropertyField;
                rulesField.BindProperty(rulesProp);
                rulesField.label = "Replacement Rules";
                
                // Customize how rules are displayed to include rotation settings
                rulesField.RegisterCallback<GeometryChangedEvent>(evt => {
                    var listView = rulesField.Q<ListView>();
                    if (listView != null)
                    {
                        listView.makeItem = () => {
                            var ruleContainer = new VisualElement();
                            ruleContainer.style.paddingBottom = 10;
                            ruleContainer.style.borderBottomWidth = 1;
                            ruleContainer.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);

                            var originalType = new PropertyField();
                            var targetType = new PropertyField();
                            var replacementPreset = new PropertyField();
                            var useTargetRotation = new PropertyField();
                            
                            ruleContainer.Add(originalType);
                            ruleContainer.Add(targetType);
                            ruleContainer.Add(replacementPreset);
                            ruleContainer.Add(useTargetRotation);
                            
                            return ruleContainer;
                        };

                        listView.bindItem = (ruleItem, ruleIndex) => {
                            var ruleProp = rulesProp.GetArrayElementAtIndex(ruleIndex);
                            var origTypeProp = ruleProp.FindPropertyRelative("originalType");
                            var targTypeProp = ruleProp.FindPropertyRelative("targetCellType");
                            var replPresetProp = ruleProp.FindPropertyRelative("replacementPreset");
                            var useTargRotProp = ruleProp.FindPropertyRelative("useTargetRotation");

                            (ruleItem.ElementAt(0) as PropertyField).BindProperty(origTypeProp);
                            (ruleItem.ElementAt(1) as PropertyField).BindProperty(targTypeProp);
                            (ruleItem.ElementAt(2) as PropertyField).BindProperty(replPresetProp);
                            (ruleItem.ElementAt(3) as PropertyField).BindProperty(useTargRotProp);
                        };
                    }
                });
                
                dropdown.SetEnabled(!checkSelfProp.boolValue);
                checkSelfField.RegisterValueChangeCallback(evt => {
                    dropdown.SetEnabled(!evt.changedProperty.boolValue);
                });
            };

            _heightAwarenessSection.Add(_listView);

            _root.Add(_heightAwarenessSection);
            return _root;
        }
        #endif
    }
}
