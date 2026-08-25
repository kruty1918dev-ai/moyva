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
        private void LogStateSynchronization(
            string source,
            bool keepVisible,
            Vector2Int previewCenter,
            int previewRadius,
            FogRevealShape previewShape,
            int operationCells,
            int visualChanges)
        {
            FogScreenSpaceSettings screenSettings =
                ResolveScreenSettings();

            if (screenSettings == null
                || (!screenSettings.LogCurtainDiagnostics
                    && !screenSettings.LogShaderDiagnostics))
            {
                return;
            }

            CountBufferStates(
                _committedPixels,
                out int committedVisible,
                out int committedExplored,
                out int committedUnexplored);

            CountBufferStates(
                _pixels,
                out int visualVisible,
                out int visualExplored,
                out int visualUnexplored);

            int mismatches =
                CountBufferMismatches(
                    _committedPixels,
                    _pixels);
        }

        private static void CountBufferStates(
            Color32[] buffer,
            out int visible,
            out int explored,
            out int unexplored)
        {
            visible = 0;
            explored = 0;
            unexplored = 0;

            if (buffer == null)
                return;

            for (int i = 0;
                 i < buffer.Length;
                 i++)
            {
                Color32 value =
                    buffer[i];

                if (value.g >= 128)
                {
                    unexplored++;
                }
                else if (value.r >= 128)
                {
                    explored++;
                }
                else
                {
                    visible++;
                }
            }
        }

        private static int CountBufferMismatches(
            Color32[] authoritative,
            Color32[] visual)
        {
            if (authoritative == null
                || visual == null
                || authoritative.Length
                != visual.Length)
            {
                return -1;
            }

            int mismatches = 0;

            for (int i = 0;
                 i < authoritative.Length;
                 i++)
            {
                if (!AreEqual(
                        authoritative[i],
                        visual[i]))
                {
                    mismatches++;
                }
            }

            return mismatches;
        }

    }
}
