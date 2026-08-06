using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Scene-view diagnostic representation. White is the canonical CoreRect,
    /// grey is SampleRect/halo, yellow is source footprint, orange is the
    /// actual final mesh and its real bounds.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ChunkAuditGizmo : MonoBehaviour
    {
        [SerializeField]
        private bool drawCoreRect = true;

        [SerializeField]
        private bool drawSampleRect = true;

        [SerializeField]
        private bool drawFinalMeshBounds = true;

        [SerializeField]
        private bool drawSourceBounds = true;

        [SerializeField]
        private bool drawSourceFootprints = true;

        [SerializeField]
        private bool drawActualMeshWhenSelected = true;

        [SerializeField]
        private bool drawEmittedCellsWhenSelected = true;

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

            foreach (Vector2Int cell
                     in emittedCells)
            {
                _emittedCells.Add(
                    cell);
            }
        }

        private void OnDrawGizmos()
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

            if (drawCoreRect)
            {
                Gizmos.color =
                    Color.white;

                DrawBounds(
                    _coreBounds);
            }

            if (drawSourceFootprints)
            {
                Gizmos.color =
                    new Color(
                        1f,
                        0.9f,
                        0.1f,
                        0.75f);

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
                        0.65f);

                DrawBounds(
                    _sourceBounds);
            }

            if (drawFinalMeshBounds)
            {
                Gizmos.color =
                    new Color(
                        1f,
                        0.25f,
                        0.02f,
                        1f);

                DrawBounds(
                    _finalMeshBounds);
            }

#if UNITY_EDITOR
            if (_coreBounds.size.sqrMagnitude
                > 0.0000001f)
            {
                Handles.color =
                    Color.white;

                Handles.Label(
                    _coreBounds.center
                    + Vector3.up
                    * (
                        _coreBounds.extents.y
                        + 0.15f
                    ),
                    $"Chunk ({_chunkX},{_chunkY}) " +
                    $"core={_coreRect.width}x{_coreRect.height} " +
                    $"emitted={_emittedCells.Count}");
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
                        1f);

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
                    0.95f);

            Vector3 size =
                new Vector3(
                    _cellSize * 0.92f,
                    0.025f,
                    _cellSize * 0.92f);

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
