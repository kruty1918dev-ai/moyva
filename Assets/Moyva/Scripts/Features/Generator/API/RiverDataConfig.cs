using System;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/RiverDataConfig", fileName = "RiverDataConfig")]
    public class RiverDataConfig : ScriptableObject
    {
        [Header("River Tile Variations")]
        [MapObjectId] public string[] VerticalTiles;
        [MapObjectId] public string[] HorizontalTiles;
        [MapObjectId] public string[] CornerTopRightTiles;
        [MapObjectId] public string[] CornerTopLeftTiles;
        [MapObjectId] public string[] CornerBottomRightTiles;
        [MapObjectId] public string[] CornerBottomLeftTiles;

        [Header("Pathfinding Weights")]
        public float MountainWeight = 50f;
        public float ForestWeight = 10f;
        public float PlainWeight = 1f;
        public Vector2 EndHeightRange = new Vector2(0.0f, 0.3f);
        public Vector2 StartHeightRange = new Vector2(0.6f, 1.0f);

        public int RiversCount = 1;
        
        [Header("Boundary Settings")]
        public int MinEdgeDistance = 20;
    }
}