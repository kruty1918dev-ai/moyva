namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Визначає область унікальності будівлі під час перенесення наявного екземпляра.
    /// </summary>
    public enum BuildingPlacementUniquenessScope
    {
        None = 0,
        PerOwner = 1,
        Global = 2,
    }

    /// <summary>
    /// Визначає джерело запиту перевірки розміщення для правил авторитету.
    /// </summary>
    public enum ConstructionPlacementAttemptSource
    {
        Unknown = 0,
        GridTileFilter = 1,
        PointerHover = 2,
        PointerClick = 3,
        PreviewMove = 4,
        Confirm = 5,
        DirectPlace = 6,
        DragValidation = 7,
        WallPath = 8,
        NetworkRequest = 9,
    }
}
