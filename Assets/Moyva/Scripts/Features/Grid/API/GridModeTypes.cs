namespace Kruty1918.Moyva.Grid.API
{
    public enum MoyvaProjectVisualMode
    {
        Auto = 0,
        Full3D = 2,
    }

    public enum GridTopology
    {
        Orthogonal = 0,
        HexAxial = 1,
        Layered = 2,
    }

    public enum GridProjectionMode
    {
        // 2D projection modes removed — project is Full3D-only.
        Isometric3DPreview = 4,
        Orthographic3D = 5,
    }

    public enum GridRenderMode
    {
        Mesh3DPreview = 2,
        Mesh3D = 3,
    }

    public enum GridNeighborhoodMode
    {
        Auto = 0,
        Moore8 = 1,
        VonNeumann4 = 2,
        HexAxial6 = 3,
    }

    public enum HexOrientation
    {
        PointyTop = 0,
        FlatTop = 1,
    }

    public enum GridWorldPlane
    {
        XY = 0,
        XZ = 1,
    }

    public enum MoyvaCameraProjectPolicy
    {
        AutoFromGrid = 0,
        Force3DOrthographic = 2,
        Force3DPerspective = 3,
    }

    public enum MoyvaCameraAnglePolicy
    {
        Auto = 0,
        OrthographicTopDown = 1,
        Isometric = 2,
        Custom = 3,
    }

    public enum MoyvaStartupCameraRadiusSource
    {
        RevealedFogRadius = 0,
        CoreVisibleRadius = 1,
        ManualRadius = 2,
    }
}