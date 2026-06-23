using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(TileSettingsNode))]
    public sealed class TileSettingsNodeEditor : UnityEditor.Editor
    {
        private SerializedProperty _tileVariants;
        private SerializedProperty _useDualGrid;
        private SerializedProperty _scaleTileToCellSize;
        private SerializedProperty _layerYOffset;
        private SerializedProperty _scaleOffset;
        private SerializedProperty _generateFlatSurface;
        private SerializedProperty _flatSurfaceMaterial;
        private SerializedProperty _tileLayerHeightOffset;
        private SerializedProperty _ignoreFillTiles;
        private SerializedProperty _meshGenerationOverride;
        private SerializedProperty _mergeTiles;
        private SerializedProperty _shadowCastingMode;
        private SerializedProperty _objectLayer;
        private SerializedProperty _renderingLayer;
        private SerializedProperty _colliderType;
        private SerializedProperty _tileColliderHeight;
        private SerializedProperty _tileColliderExtrusionHeight;
        private SerializedProperty _invertCollisionWalls;

        private bool _showPorts = true;

        private void OnEnable()
        {
            _tileVariants = serializedObject.FindProperty("_tileVariants");
            _useDualGrid = serializedObject.FindProperty("_useDualGrid");
            _scaleTileToCellSize = serializedObject.FindProperty("_scaleTileToCellSize");
            _layerYOffset = serializedObject.FindProperty("_layerYOffset");
            _scaleOffset = serializedObject.FindProperty("_scaleOffset");
            _generateFlatSurface = serializedObject.FindProperty("_generateFlatSurface");
            _flatSurfaceMaterial = serializedObject.FindProperty("_flatSurfaceMaterial");
            _tileLayerHeightOffset = serializedObject.FindProperty("_tileLayerHeightOffset");
            _ignoreFillTiles = serializedObject.FindProperty("_ignoreFillTiles");
            _meshGenerationOverride = serializedObject.FindProperty("_meshGenerationOverride");
            _mergeTiles = serializedObject.FindProperty("_mergeTiles");
            _shadowCastingMode = serializedObject.FindProperty("_shadowCastingMode");
            _objectLayer = serializedObject.FindProperty("_objectLayer");
            _renderingLayer = serializedObject.FindProperty("_renderingLayer");
            _colliderType = serializedObject.FindProperty("_colliderType");
            _tileColliderHeight = serializedObject.FindProperty("_tileColliderHeight");
            _tileColliderExtrusionHeight = serializedObject.FindProperty("_tileColliderExtrusionHeight");
            _invertCollisionWalls = serializedObject.FindProperty("_invertCollisionWalls");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            DrawPresetVariants();
            DrawBuildSettings();
            DrawFlatSurfaceSettings();
            DrawTileLayerSettings();
            DrawMeshAndColliderSettings();

            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);
                GUI.changed = true;
            }
        }

        private new void DrawHeader()
        {
            var node = target as TileSettingsNode;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Документація ноди", EditorStyles.boldLabel);
                if (node != null)
                {
                    EditorGUILayout.HelpBox(GraphNodeDocumentation.BuildInspectorHeader(node), MessageType.Info);
                    _showPorts = EditorGUILayout.Foldout(_showPorts, "Порти / типи даних", true);
                    if (_showPorts)
                        EditorGUILayout.HelpBox(GraphNodeDocumentation.BuildPortsInspectorText(node), MessageType.None);

                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField("Configured variants", node.ConfiguredVariantCount.ToString(), EditorStyles.miniLabel);
                    EditorGUILayout.LabelField("Runtime output", node.HasRenderableTileOutput ? "Tile layer" : "Internal/no tiles", EditorStyles.miniLabel);
                }
            }
        }

        private void DrawPresetVariants()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Tile Preset Variants", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(GraphNodeDocumentation.GetParameterDescription(typeof(TileSettingsNode), "_tileVariants", "Tile Preset Variants"), MessageType.None);

            if (_tileVariants == null)
            {
                EditorGUILayout.HelpBox("Serialized field _tileVariants not found.", MessageType.Error);
                return;
            }

            if (_tileVariants.arraySize == 0)
                EditorGUILayout.HelpBox("Додай хоча б один TilePreset variant або увімкни Flat Surface.", MessageType.Warning);

            for (int i = 0; i < _tileVariants.arraySize; i++)
            {
                var variant = _tileVariants.GetArrayElementAtIndex(i);
                if (variant == null)
                    continue;

                var preset = variant.FindPropertyRelative("Preset");
                var slot = variant.FindPropertyRelative("Slot");
                var weight = variant.FindPropertyRelative("Weight");
                var tileHeight = variant.FindPropertyRelative("TileHeight");

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Variant " + (i + 1), EditorStyles.miniBoldLabel);
                        GUILayout.FlexibleSpace();
                        using (new EditorGUI.DisabledScope(_tileVariants.arraySize <= 1))
                        {
                            if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(70)))
                            {
                                _tileVariants.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }
                    }

                    DrawDocumentedVariantProperty(preset, "Preset");
                    DrawDocumentedVariantProperty(slot, "Slot");
                    if (weight != null)
                    {
                        string help = GraphNodeDocumentation.GetParameterDescription(typeof(TileSettingsNode), "Weight", "Weight");
                        weight.floatValue = EditorGUILayout.Slider(new GUIContent("Weight", help), Mathf.Clamp01(weight.floatValue), 0f, 1f);
                        EditorGUILayout.HelpBox(help, MessageType.None);
                    }
                    if (tileHeight != null)
                    {
                        string help = GraphNodeDocumentation.GetParameterDescription(typeof(TileSettingsNode), "TileHeight", "Tile Height");
                        tileHeight.floatValue = Mathf.Max(0f, EditorGUILayout.FloatField(new GUIContent("Tile Height", help), tileHeight.floatValue));
                        EditorGUILayout.HelpBox(help, MessageType.None);
                    }
                }
            }

            if (GUILayout.Button("+ Add Variant"))
            {
                int index = _tileVariants.arraySize;
                _tileVariants.InsertArrayElementAtIndex(index);
                var variant = _tileVariants.GetArrayElementAtIndex(index);
                if (variant != null)
                {
                    variant.FindPropertyRelative("Preset").objectReferenceValue = null;
                    variant.FindPropertyRelative("Slot").enumValueIndex = 0;
                    variant.FindPropertyRelative("Weight").floatValue = 1f;
                    variant.FindPropertyRelative("TileHeight").floatValue = 0f;
                }
            }
        }

        private void DrawBuildSettings()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Build Layer", EditorStyles.boldLabel);
            DrawProperty(_useDualGrid, "Use Dual Grid");
            DrawProperty(_scaleTileToCellSize, "Scale Tile To Cell Size");
            DrawProperty(_layerYOffset, "Layer Y Offset");
            DrawProperty(_scaleOffset, "Scale Offset");
        }

        private void DrawFlatSurfaceSettings()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Flat Surface", EditorStyles.boldLabel);
            DrawProperty(_generateFlatSurface, "Generate Flat Surface");
            using (new EditorGUI.DisabledScope(_generateFlatSurface != null && !_generateFlatSurface.boolValue))
                DrawProperty(_flatSurfaceMaterial, "Flat Surface Material");
        }

        private void DrawTileLayerSettings()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Tile Layer", EditorStyles.boldLabel);
            DrawProperty(_tileLayerHeightOffset, "Tile Layer Height Offset");
            DrawProperty(_ignoreFillTiles, "Ignore Fill Tiles");
        }

        private void DrawMeshAndColliderSettings()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Mesh / Collider", EditorStyles.boldLabel);
            DrawProperty(_meshGenerationOverride, "Mesh Generation Override");
            DrawProperty(_mergeTiles, "Merge Tiles");
            DrawProperty(_shadowCastingMode, "Shadow Casting Mode");
            DrawProperty(_objectLayer, "Object Layer");
            DrawProperty(_renderingLayer, "Rendering Layer");
            DrawProperty(_colliderType, "Collider Type");
            DrawProperty(_tileColliderHeight, "Tile Collider Height");
            DrawProperty(_tileColliderExtrusionHeight, "Tile Collider Extrusion Height");
            DrawProperty(_invertCollisionWalls, "Invert Collision Walls");
        }

        private static void DrawDocumentedVariantProperty(SerializedProperty property, string label)
        {
            if (property == null)
                return;

            string help = GraphNodeDocumentation.GetParameterDescription(typeof(TileSettingsNode), label, label);
            EditorGUILayout.PropertyField(property, new GUIContent(label, help), true);
            EditorGUILayout.HelpBox(help, MessageType.None);
        }

        private static void DrawProperty(SerializedProperty property, string label)
        {
            if (property == null)
                return;

            string help = GraphNodeDocumentation.GetParameterDescription(typeof(TileSettingsNode), property.propertyPath, label);
            EditorGUILayout.PropertyField(property, new GUIContent(label, help), true);
            EditorGUILayout.HelpBox(help, MessageType.None);
        }
    }
}