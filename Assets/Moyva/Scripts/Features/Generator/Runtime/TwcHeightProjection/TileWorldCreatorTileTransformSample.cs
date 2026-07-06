using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct TileWorldCreatorTileTransformSample
    {
        public TileWorldCreatorTileTransformSample(Transform transform, Vector3 worldCenter)
        {
            Transform = transform;
            WorldCenter = worldCenter;
        }

        public Transform Transform { get; }
        public Vector3 WorldCenter { get; }
    }
}
