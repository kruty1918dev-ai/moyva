using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogScreenSpaceTextureUpdater
    {
        public void Initialize(
            int width,
            int height,
            FogWorldVisualContext context)
        {
            _width =
                Mathf.Max(
                    1,
                    width);

            _height =
                Mathf.Max(
                    1,
                    height);

            _context =
                context.IsValid
                    ? context.WithSize(
                        _width,
                        _height)
                    : context;

            EnsureTexture(
                _width,
                _height);

            FillBuffer(
                _committedPixels,
                UnexploredValue);

            CopyCommittedToVisual();

            CommitVisualState(
                "Initialize");
        }

        public void SetWorldContext(
            FogWorldVisualContext context)
        {
            SetWorldContextInternal(
                context,
                true);
        }

        public void PreviewRevealArea(
            Vector2Int center,
            int radius,
            FogRevealShape shape,
            bool keepVisible)
        {
            EnsureTexture(
                _width,
                _height);

            /*
             * Preview більше не скидає texture в Unexplored.
             *
             * Старий Fill(UnexploredValue) руйнував синхронізацію:
             * FogOfWarService вже вважав клітинки Visible, але visual
             * buffer повертав їх у Unexplored. Наступний incremental
             * update надсилав лише реально змінені клітинки, тому
             * скинуті preview-клітинки лишались дірками всередині
             * відкритої області.
             */
            CopyCommittedToVisual();

            int safeRadius =
                Mathf.Max(
                    0,
                    radius);

            int previewTouched = 0;
            int previewChanged = 0;

            for (int y =
                     center.y - safeRadius;
                 y <=
                     center.y + safeRadius;
                 y++)
            {
                for (int x =
                         center.x - safeRadius;
                     x <=
                         center.x + safeRadius;
                     x++)
                {
                    var tile =
                        new Vector2Int(
                            x,
                            y);

                    if (!IsInBounds(tile))
                        continue;

                    if (!IsInsideShape(
                            tile,
                            center,
                            safeRadius,
                            shape))
                    {
                        continue;
                    }

                    previewTouched++;

                    int index =
                        tile.x
                        + tile.y * _width;

                    Color32 before =
                        _pixels[index];

                    if (!AreEqual(
                            before,
                            VisibleValue))
                    {
                        previewChanged++;
                    }

                    SetPixelValue(
                        tile,
                        VisibleValue);
                }
            }

            /*
             * keepVisible=true означає union із committed state.
             * Копіювання committed -> visual уже гарантує це.
             *
             * keepVisible=false теж лишається non-destructive:
             * preview не має права змінювати gameplay-authority.
             */
            _previewActive = true;
            _previewSequence++;

            LogStateSynchronization(
                "PreviewRevealArea",
                keepVisible,
                center,
                safeRadius,
                shape,
                previewTouched,
                previewChanged);

            CommitVisualState(
                "PreviewRevealArea");
        }

        public void UpdateDirtyTiles(
            IFogOfWarService fogService,
            IEnumerable<Vector2Int> dirtyTiles)
        {
            if (fogService == null
                || dirtyTiles == null)
            {
                return;
            }

            EnsureTexture(
                _width,
                _height);

            bool committedChanged = false;

            foreach (Vector2Int tile
                     in dirtyTiles)
            {
                if (!IsInBounds(tile))
                    continue;

                committedChanged |=
                    SetCommittedPixelValue(
                        tile,
                        EncodeState(
                            fogService.GetFogState(
                                tile)));
            }

            if (!committedChanged
                && !_previewActive)
            {
                return;
            }

            EndPreviewAndCopyCommittedToVisual();

            LogStateSynchronization(
                "UpdateDirtyTiles",
                false,
                default,
                0,
                default,
                0,
                0);

            CommitVisualState(
                "UpdateDirtyTiles");
        }

        public void RequestCellsUpdate(
            IFogOfWarService fogService,
            IReadOnlyList<FogCellVisualChange> changes,
            FogWorldVisualContext context)
        {
            if (context.IsValid)
            {
                SetWorldContextInternal(
                    context,
                    false);
            }

            if (changes == null
                || changes.Count == 0)
            {
                /*
                 * Empty updates are common during initialization and
                 * repeated signals. Without an active preview there is
                 * nothing to copy, upload or publish.
                 */
                if (!_previewActive)
                    return;

                EndPreviewAndCopyCommittedToVisual();

                LogStateSynchronization(
                    "RequestCellsUpdate.Empty.EndPreview",
                    false,
                    default,
                    0,
                    default,
                    0,
                    0);

                CommitVisualState(
                    "RequestCellsUpdate.Empty.EndPreview");

                return;
            }

            EnsureTexture(
                _width,
                _height);

            bool committedChanged = false;

            for (int i = 0;
                 i < changes.Count;
                 i++)
            {
                FogCellVisualChange change =
                    changes[i];

                if (!change.HasVisualDelta
                    || !IsInBounds(
                        change.Cell))
                {
                    continue;
                }

                FogStateType state =
                    fogService != null
                        ? fogService.GetFogState(
                            change.Cell)
                        : change.NewState;

                committedChanged |=
                    SetCommittedPixelValue(
                        change.Cell,
                        EncodeState(state));
            }

            if (!committedChanged
                && !_previewActive)
            {
                return;
            }

            EndPreviewAndCopyCommittedToVisual();

            LogStateSynchronization(
                "RequestCellsUpdate",
                false,
                default,
                0,
                default,
                changes.Count,
                0);

            CommitVisualState(
                "RequestCellsUpdate");
        }

        public void RebuildFullVisual(
            IFogOfWarService fogService)
        {
            if (fogService == null)
                return;

            EnsureTexture(
                _width,
                _height);

            for (int y = 0;
                 y < _height;
                 y++)
            {
                for (int x = 0;
                     x < _width;
                     x++)
                {
                    var tile =
                        new Vector2Int(
                            x,
                            y);

                    SetCommittedPixelValue(
                        tile,
                        EncodeState(
                            fogService.GetFogState(
                                tile)));
                }
            }

            EndPreviewAndCopyCommittedToVisual();

            LogStateSynchronization(
                "RebuildFullVisual",
                false,
                default,
                0,
                default,
                _width * _height,
                0);

            CommitVisualState(
                "RebuildFullVisual");
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            Shader.SetGlobalFloat(
                FogEnabledId,
                0f);

            Shader.SetGlobalTexture(
                FogTextureId,
                Texture2D.blackTexture);

            _curtainRenderer.Dispose();

            DestroyTexture();

            _committedPixels = null;
            _pixels = null;
        }

        private void SetWorldContextInternal(
            FogWorldVisualContext context,
            bool commit)
        {
            if (!context.IsValid)
                return;

            bool sizeChanged =
                context.Width != _width
                || context.Height != _height;

            _context =
                context;

            if (sizeChanged)
            {
                _width =
                    Mathf.Max(
                        1,
                        context.Width);

                _height =
                    Mathf.Max(
                        1,
                        context.Height);

                EnsureTexture(
                    _width,
                    _height);

                FillBuffer(
                    _committedPixels,
                    UnexploredValue);

                CopyCommittedToVisual();
            }

            if (commit)
            {
                LogStateSynchronization(
                    "SetWorldContext",
                    false,
                    default,
                    0,
                    default,
                    0,
                    0);

                CommitVisualState(
                    "SetWorldContext");
            }
        }

    }
}
