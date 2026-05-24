using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    [CreateAssetMenu(fileName = "MoyvaProjectSettings", menuName = "Moyva/Project Settings", order = 0)]
    public sealed class MoyvaProjectSettingsSO : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/Moyva/SO/ProjectDefaults/MoyvaProjectSettings.asset";

        [Header("Grid Mode")]
        public GridTopology DefaultGridTopology = GridTopology.Orthogonal;
        public GridProjectionMode DefaultProjectionMode = GridProjectionMode.Orthographic2D;
        public GridRenderMode DefaultRenderMode = GridRenderMode.Sprite2D;
        public GridNeighborhoodMode DefaultNeighborhood = GridNeighborhoodMode.Auto;

        [Header("Cell Size")]
        [Min(0.01f)] public float OrthogonalCellWidth = 1f;
        [Min(0.01f)] public float OrthogonalCellHeight = 1f;
        [Min(0.01f)] public float OrthogonalCellDepth = 1f;

        [Header("Isometric")]
        [Min(0.01f)] public float IsometricTileWidth = 1f;
        [Min(0.01f)] public float IsometricTileHeight = 0.5f;

        [Header("Hex")]
        public HexOrientation HexOrientation = HexOrientation.PointyTop;
        [Min(0.01f)] public float HexRadius = 0.5f;

        [Header("Height Preview")]
        [Min(0f)] public float HeightScale = 0.25f;
        public bool UseHeightForPreview = true;

        public Vector2 OrthogonalCellSize => new Vector2(OrthogonalCellWidth, OrthogonalCellHeight);
        public Vector3 Orthogonal3DCellSize => new Vector3(OrthogonalCellWidth, HeightScale, OrthogonalCellDepth);
        public Vector2 IsometricTileSize => new Vector2(IsometricTileWidth, IsometricTileHeight);

        public static MoyvaProjectSettingsSO CreateRuntimeDefault()
        {
            var settings = CreateInstance<MoyvaProjectSettingsSO>();
            settings.name = "Runtime Moyva Project Settings";
            settings.Normalize();
            return settings;
        }

        public void Normalize()
        {
            OrthogonalCellWidth = Mathf.Max(0.01f, OrthogonalCellWidth);
            OrthogonalCellHeight = Mathf.Max(0.01f, OrthogonalCellHeight);
            OrthogonalCellDepth = Mathf.Max(0.01f, OrthogonalCellDepth);
            IsometricTileWidth = Mathf.Max(0.01f, IsometricTileWidth);
            IsometricTileHeight = Mathf.Max(0.01f, IsometricTileHeight);
            HexRadius = Mathf.Max(0.01f, HexRadius);
            HeightScale = Mathf.Max(0f, HeightScale);
        }

        public GridNeighborhoodMode ResolveNeighborhoodMode()
        {
            if (DefaultNeighborhood != GridNeighborhoodMode.Auto)
                return DefaultNeighborhood;

            if (DefaultGridTopology == GridTopology.HexAxial
                || DefaultProjectionMode == GridProjectionMode.HexPointy2D
                || DefaultProjectionMode == GridProjectionMode.HexFlat2D)
            {
                return GridNeighborhoodMode.HexAxial6;
            }

            return DefaultProjectionMode == GridProjectionMode.Isometric2D
                || DefaultProjectionMode == GridProjectionMode.Isometric3DPreview
                    ? GridNeighborhoodMode.VonNeumann4
                    : GridNeighborhoodMode.Moore8;
        }
    }
}