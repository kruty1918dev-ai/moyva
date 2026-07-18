using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo("Налаштування трави", "Розміщення об'єктів", "Зберігає налаштування текстури/матеріалу/префаба для перекривних плоских трав\'їнних карт.")]
    [HidePreview]
    public sealed class GrassCardGeneratorNode : NodeBase, ICustomEditorNode
    {
        [SerializeField]
        [Tooltip("Текстура, яку використовує редактор для генерації матеріалу/префаба перекривної плоскої трав\'їнної карти.")]
        private Texture2D _texture;

        [SerializeField]
        private Material _material;

        [SerializeField]
        [Tooltip("Згенерований або ручно створений перекривний плоский префаб трави.")]
        private GameObject _prefab;

        [SerializeField]
        private Color _tint = Color.white;

        [SerializeField, Range(0f, 1f)]
        private float _alphaClip = 0.35f;

        [SerializeField, Range(1, 8)]
        private int _crossedPlanes = 3;

        [SerializeField]
        private GrassCardGeometryMode _geometryMode = GrassCardGeometryMode.CrossedPlanes;

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

        public override string Title => "Налаштування трави";
        public override string Category => "Розміщення об'єктів";

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
                GeometryMode = _geometryMode,
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
            var factoryType = System.Type.GetType(
                "Kruty1918.Moyva.Generator.Editor.ObjectPlacement.GrassCardPrefabFactory, Kruty1918.Moyva.Generator.Editor");
            var openMethod = factoryType?.GetMethod(
                "OpenGrassCardPrefabWizard",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                null,
                new[]
                {
                    typeof(string),
                    typeof(GrassCardSettings),
                    typeof(string),
                    typeof(System.Action<string>)
                },
                null);
            if (openMethod == null)
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Grass Card Settings",
                    "Grass card prefab wizard is not available in the editor assembly.",
                    "OK");
                return;
            }

            string sourceName = _texture != null
                ? _texture.name
                : _material != null
                    ? _material.name
                    : "GrassCard";
            var settings = new GrassCardSettings
            {
                Texture = _texture,
                Material = _material,
                Prefab = _prefab,
                Tint = _tint,
                AlphaClip = _alphaClip,
                CrossedPlanes = _crossedPlanes,
                GeometryMode = _geometryMode,
                Width = _width,
                Height = _height,
                DoubleSided = _doubleSided,
                WindWobble = _windWobble,
                ColorVariation = _colorVariation
            };

            System.Action<string> onGenerated = path =>
            {
                if (string.IsNullOrEmpty(path))
                    return;

                _prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                UnityEditor.EditorUtility.SetDirty(this);
                if (_prefab != null)
                {
                    UnityEditor.Selection.activeObject = _prefab;
                    UnityEditor.EditorGUIUtility.PingObject(_prefab);
                }
            };

            openMethod.Invoke(null, new object[]
            {
                sourceName,
                settings,
                "Assets/Moyva/Generated/Grass",
                onGenerated
            });
        }
#endif
    }
}
