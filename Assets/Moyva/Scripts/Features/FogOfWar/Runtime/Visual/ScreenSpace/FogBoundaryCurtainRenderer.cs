using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using UnityEngine.Rendering;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Будує одну world-space mesh-стінку вздовж логічної межі:
    ///
    /// revealed cell (Visible або Explored)
    /// проти
    /// Unexplored cell або простору поза картою.
    ///
    /// Геометрія не залежить від depth buffer, FOV чи країв екрана.
    /// </summary>
    internal sealed partial class FogBoundaryCurtainRenderer
        : IDisposable
    {
        private const string CurtainShaderName =
            "Moyva/FogOfWar/BoundaryCurtain";

        private const string CurtainObjectName =
            "Moyva_FogBoundaryCurtain";

        private const string DiagnosticPrefix =
            "[MOYVA_FOG_CURTAIN_DIAG]";

        private const string MaskDiagnosticPrefix =
            "[MOYVA_FOG_MASK_DIAG]";

        private static readonly int TopColorId =
            Shader.PropertyToID(
                "_TopColor");

        private static readonly int BottomColorId =
            Shader.PropertyToID(
                "_BottomColor");

        private static readonly int TopBandFractionId =
            Shader.PropertyToID(
                "_TopBandFraction");

        private static readonly int GradientPowerId =
            Shader.PropertyToID(
                "_GradientPower");

        private static readonly int DebugVisualModeId =
            Shader.PropertyToID(
                "_DebugVisualMode");

        private static readonly int CullModeId =
            Shader.PropertyToID(
                "_CullMode");


        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up
        };

        private readonly FogOfWarSettings _settings;
        private readonly IGridProjection _gridProjection;

        private readonly List<Vector3> _vertices =
            new List<Vector3>(4096);

        private readonly List<Vector2> _uvs =
            new List<Vector2>(4096);

        private readonly List<Color32> _colors =
            new List<Color32>(4096);

        private readonly List<DiagnosticSegment> _diagnosticSegments =
            new List<DiagnosticSegment>(1024);

        private readonly List<float> _surfaceOffsetSamples =
            new List<float>(64);

        private readonly RaycastHit[] _surfaceProbeHits =
            new RaycastHit[16];

        private readonly List<int> _triangles =
            new List<int>(6144);

        private GameObject _root;
        private Mesh _mesh;
        private Material _material;

        private bool _shaderErrorLogged;
        private bool _disposed;

        private float _lastDiagnosticTime =
            float.NegativeInfinity;

        private int _rebuildSequence;
        private int _lastBoundaryHash =
            int.MinValue;

        public FogBoundaryCurtainRenderer(
            FogOfWarSettings settings,
            IGridProjection gridProjection)
        {
            _settings = settings;
            _gridProjection = gridProjection;
        }

    }
}
