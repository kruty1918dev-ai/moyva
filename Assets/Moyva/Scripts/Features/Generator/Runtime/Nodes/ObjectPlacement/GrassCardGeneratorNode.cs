using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo("Grass Card Settings", "Object Placement", "Stores texture/material/prefab settings for crossed-plane grass cards.")]
    [HidePreview]
    public sealed class GrassCardGeneratorNode : NodeBase, ICustomEditorNode
    {
        [SerializeField]
        [Tooltip("Texture used by the editor utility to generate a crossed-plane grass card material/prefab.")]
        private Texture2D _texture;

        [SerializeField]
        private Material _material;

        [SerializeField]
        [Tooltip("Generated or hand-authored crossed-plane grass prefab.")]
        private GameObject _prefab;

        [SerializeField]
        private Color _tint = Color.white;

        [SerializeField, Range(0f, 1f)]
        private float _alphaClip = 0.35f;

        [SerializeField, Range(2, 4)]
        private int _crossedPlanes = 3;

        [SerializeField, Min(0.01f)]
        private float _width = 0.7f;

        [SerializeField, Min(0.01f)]
        private float _height = 0.9f;

        [SerializeField]
        private bool _doubleSided = true;

        [SerializeField]
        private bool _windWobble;

        [SerializeField, Range(0f, 1f)]
        private float _colorVariation = 0.12f;

        public override string Title => "Grass Card Settings";
        public override string Category => "Object Placement";

        public override PortDefinition[] Inputs => System.Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<GrassCardSettings>("Grass")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            return NodeOutput.Success(new GrassCardSettings
            {
                Texture = _texture,
                Material = _material,
                Prefab = _prefab,
                Tint = _tint,
                AlphaClip = _alphaClip,
                CrossedPlanes = _crossedPlanes,
                Width = _width,
                Height = _height,
                DoubleSided = _doubleSided,
                WindWobble = _windWobble,
                ColorVariation = _colorVariation
            });
        }

#if UNITY_EDITOR
        public void OpenEditorWindow()
        {
            if (_texture == null && _material == null)
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Grass Card Settings",
                    "Assign a grass texture or material first.",
                    "OK");
                return;
            }

            var factoryType = System.Type.GetType(
                "Kruty1918.Moyva.Generator.Editor.ObjectPlacement.GrassCardPrefabFactory, Kruty1918.Moyva.Generator.Editor");
            var createMethod = factoryType?.GetMethod(
                "CreateGrassCardPrefab",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (createMethod == null)
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Grass Card Settings",
                    "Grass card prefab factory is not available in the editor assembly.",
                    "OK");
                return;
            }

            string sourceName = _texture != null ? _texture.name : _material.name;
            var settings = new GrassCardSettings
            {
                Texture = _texture,
                Material = _material,
                Prefab = _prefab,
                Tint = _tint,
                AlphaClip = _alphaClip,
                CrossedPlanes = _crossedPlanes,
                Width = _width,
                Height = _height,
                DoubleSided = _doubleSided,
                WindWobble = _windWobble,
                ColorVariation = _colorVariation
            };

            var path = createMethod.Invoke(null, new object[]
            {
                sourceName,
                settings,
                "Assets/Moyva/Generated/Grass"
            }) as string;

            if (string.IsNullOrEmpty(path))
                return;

            _prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            UnityEditor.EditorUtility.SetDirty(this);
            if (_prefab != null)
            {
                UnityEditor.Selection.activeObject = _prefab;
                UnityEditor.EditorGUIUtility.PingObject(_prefab);
            }
        }
#endif
    }
}
