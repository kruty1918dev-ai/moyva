using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Clockwise quarter-turn placement rotation. The numeric value is stable
    /// because it is persisted in save data and sent over the network.
    /// </summary>
    public enum ConstructionRotation : byte
    {
        Degrees0 = 0,
        Degrees90 = 1,
        Degrees180 = 2,
        Degrees270 = 3,
    }

    public static class ConstructionRotationUtility
    {
        public static ConstructionRotation Normalize(int quarterTurns)
            => (ConstructionRotation)(((quarterTurns % 4) + 4) % 4);

        public static ConstructionRotation NextClockwise(
            ConstructionRotation rotation)
            => Normalize((int)rotation + 1);

        public static Vector2Int RotateOffset(
            Vector2Int offset,
            ConstructionRotation rotation)
            => Normalize((int)rotation) switch
            {
                ConstructionRotation.Degrees90 =>
                    new Vector2Int(offset.y, -offset.x),
                ConstructionRotation.Degrees180 =>
                    new Vector2Int(-offset.x, -offset.y),
                ConstructionRotation.Degrees270 =>
                    new Vector2Int(-offset.y, offset.x),
                _ => offset,
            };

        public static Quaternion ToWorldRotation(
            ConstructionRotation rotation)
            => Quaternion.Euler(
                0f,
                (int)Normalize((int)rotation) * 90f,
                0f);
    }
}
