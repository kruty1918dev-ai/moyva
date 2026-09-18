using System;
using System.Collections.Generic;
using System.Text;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogVolumeVisualUpdateEngine
    {
        private enum FogRuntimeState
        {
            Unexplored,
            Explored
        }

        private sealed class RuntimeLayer
        {
            /// <summary>
            /// Описує пару runtime blueprint/build layer для конкретного fog state і height bucket.
            /// </summary>
            /// <param name="blueprintLayer">Runtime blueprint layer.</param>
            /// <param name="buildLayer">Пов'язаний runtime build layer.</param>
            /// <param name="state">Fog state, який цей layer репрезентує.</param>
            /// <param name="heightKey">Нормалізований ключ висотного bucket-а.</param>
            public RuntimeLayer(BlueprintLayer blueprintLayer, TilesBuildLayer buildLayer, FogRuntimeState state, int heightKey)
            {
                BlueprintLayer = blueprintLayer;
                BuildLayer = buildLayer;
                State = state;
                HeightKey = heightKey;
            }

            /// <summary>
            /// Runtime blueprint layer для конкретного fog state.
            /// </summary>
            public BlueprintLayer BlueprintLayer { get; }

            /// <summary>
            /// Runtime build layer, який TWC фактично будує у сцені.
            /// </summary>
            public TilesBuildLayer BuildLayer { get; }

            /// <summary>
            /// Fog state, який репрезентує цей runtime layer.
            /// </summary>
            public FogRuntimeState State { get; }

            /// <summary>
            /// Bucket ключ для висоти, на якій будується цей layer.
            /// </summary>
            public int HeightKey { get; }
        }
    }
}
