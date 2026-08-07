using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Truthful chunk visualization.
    ///
    /// Default scene view:
    /// - only canonical CoreRect is drawn for every chunk;
    /// - no per-cell/source clutter;
    /// - no labels for every chunk.
    ///
    /// Selected chunk:
    /// - canonical CoreRect;
    /// - real final mesh bounds;
    /// - real final mesh wireframe;
    /// - one readable information label.
    ///
    /// Advanced layers remain available as serialized toggles but are disabled
    /// by default because they are useful only while investigating a specific
    /// source/halo problem.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ChunkAuditGizmo : MonoBehaviour
    {
        [Header("Always Visible")]
        [SerializeField]
        private bool drawCoreRect = true;

        [Header("Selected Chunk")]
        [SerializeField]
        private bool drawFinalMeshBounds = true;

        [SerializeField]
        private bool drawActualMeshWhenSelected = true;

        [SerializeField]
        private bool drawSelectedLabel = true;

        [Header("Advanced Diagnostics")]
        [SerializeField]
        private bool drawSampleRect;

        [SerializeField]
        private bool drawSourceBounds;

        [SerializeField]
        private bool drawSourceFootprints;

        [SerializeField]
        private bool drawEmittedCellsWhenSelected;

        [SerializeField]
        private bool drawAllChunkLabels;

        private int _chunkX;
        private int _chunkY;
        private RectInt _coreRect;
        private RectInt _sampleRect;
        private float _cellSize;
        private Bounds _coreBounds;
        private Bounds _sampleBounds;
        private Bounds _finalMeshBounds;
        private Bounds _sourceBounds;
        private Bounds _sourceFootprintBounds;
        private Transform _terrainTransform;
        private Mesh _mesh;

        private readonly List<Vector2Int> _emittedCells =
            new List<Vector2Int>();

        public void Configure(
            int chunkX,
            int chunkY,
            RectInt coreRect,
            RectInt sampleRect,
            float cellSize,
            Bounds coreBounds,
            Bounds sampleBounds,
            Bounds finalMeshBounds,
            Bounds sourceBounds,
            Bounds sourceFootprintBounds,
            Transform terrainTransform,
            Mesh mesh,
            IEnumerable<Vector2Int> emittedCells)
        {
            _chunkX = chunkX;
            _chunkY = chunkY;
            _coreRect = coreRect;
            _sampleRect = sampleRect;
            _cellSize = Mathf.Max(
                0.0001f,
                cellSize);
            _coreBounds = coreBounds;
            _sampleBounds = sampleBounds;
            _finalMeshBounds = finalMeshBounds;
            _sourceBounds = sourceBounds;
            _sourceFootprintBounds = sourceFootprintBounds;
            _terrainTransform = terrainTransform;
            _mesh = mesh;

            _emittedCells.Clear();

            if (emittedCells == null)
                return;

            foreach (Vector2Int cell in emittedCells)
                _emittedCells.Add(cell);
        }

        private void OnDrawGizmos()
        {
            bool selected =
                IsThisChunkSelected();

            if (drawCoreRect)
            {
                Gizmos.color =
                    selected
                        ? new Color(
                            0.1f,
                            0.95f,
                            1f,
                            1f)
                        : new Color(
                            1f,
                            1f,
                            1f,
                            0.92f);

                DrawBounds(
                    _coreBounds);
            }

            /*
             * Advanced bounds are intentionally selected-only.
             * Drawing them for all 49/100 chunks was the source of the
             * unreadable orange/yellow grid in Scene View.
             */
            if (selected)
            {
                if (drawSampleRect)
                {
                    Gizmos.color =
                        new Color(
                            0.55f,
                            0.55f,
                            0.55f,
                            0.7f);

                    DrawBounds(
                        _sampleBounds);
                }

                if (drawSourceFootprints)
                {
                    Gizmos.color =
                        new Color(
                            1f,
                            0.9f,
                            0.1f,
                            0.8f);

                    DrawBounds(
                        _sourceFootprintBounds);
                }

                if (drawSourceBounds)
                {
                    Gizmos.color =
                        new Color(
                            1f,
                            0.55f,
                            0.05f,
                            0.8f);

                    DrawBounds(
                        _sourceBounds);
                }

                if (drawFinalMeshBounds)
                {
                    Gizmos.color =
                        new Color(
                            1f,
                            0.27f,
                            0.03f,
                            1f);

                    DrawBounds(
                        _finalMeshBounds);
                }
            }

#if UNITY_EDITOR
            if ((selected && drawSelectedLabel)
                || drawAllChunkLabels)
            {
                DrawChunkLabel(
                    selected);
            }
#endif
        }

        private void OnDrawGizmosSelected()
        {
            if (drawActualMeshWhenSelected
                && _mesh != null
                && _terrainTransform != null)
            {
                Gizmos.color =
                    new Color(
                        1f,
                        0.2f,
                        0.02f,
                        0.95f);

                for (int subMesh = 0;
                     subMesh < _mesh.subMeshCount;
                     subMesh++)
                {
                    Gizmos.DrawWireMesh(
                        _mesh,
                        subMesh,
                        _terrainTransform.position,
                        _terrainTransform.rotation,
                        _terrainTransform.lossyScale);
                }
            }

            if (!drawEmittedCellsWhenSelected)
                return;

            float y =
                _finalMeshBounds.size.sqrMagnitude
                    > 0.0000001f
                    ? _finalMeshBounds.max.y
                      + 0.08f
                    : 0.08f;

            Gizmos.color =
                new Color(
                    0.1f,
                    1f,
                    0.35f,
                    0.9f);

            Vector3 size =
                new Vector3(
                    _cellSize * 0.86f,
                    0.025f,
                    _cellSize * 0.86f);

            for (int i = 0;
                 i < _emittedCells.Count;
                 i++)
            {
                Vector2Int cell =
                    _emittedCells[i];

                Vector3 center =
                    new Vector3(
                        cell.x * _cellSize,
                        y,
                        cell.y * _cellSize);

                Gizmos.DrawWireCube(
                    center,
                    size);
            }
        }

        private bool IsThisChunkSelected()
        {
#if UNITY_EDITOR
            Transform selected =
                Selection.activeTransform;

            if (selected == null)
                return false;

            return selected == transform
                || selected.IsChildOf(transform);
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        private void DrawChunkLabel(
            bool selected)
        {
            if (_coreBounds.size.sqrMagnitude
                <= 0.0000001f)
            {
                return;
            }

            Handles.color =
                selected
                    ? new Color(
                        0.2f,
                        1f,
                        1f,
                        1f)
                    : new Color(
                        1f,
                        1f,
                        1f,
                        0.8f);

            string text =
                $"Chunk ({_chunkX},{_chunkY})  " +
                $"Core {_coreRect.width}×{_coreRect.height}  " +
                $"Cells {_emittedCells.Count}";

            if (selected
                && _mesh != null)
            {
                text +=
                    $"  Mesh {_mesh.vertexCount} verts/" +
                    $"{_mesh.subMeshCount} sub";
            }

            Vector3 position =
                new Vector3(
                    _coreBounds.min.x,
                    Mathf.Max(
                        _coreBounds.max.y,
                        _finalMeshBounds.max.y)
                    + 0.22f,
                    _coreBounds.min.z);

            Handles.Label(
                position,
                text);
        }
#endif

        private static void DrawBounds(
            Bounds bounds)
        {
            if (bounds.size.sqrMagnitude
                <= 0.0000001f)
            {
                return;
            }

            Gizmos.DrawWireCube(
                bounds.center,
                bounds.size);
        }
    }
}
