namespace Kruty1918.Moyva.Generator.API
{
    /// <summary>
    /// Централізований контракт для спільних налаштувань генератора.
    /// Реєструється у NodeContext нодою SharedSettingsNode;
    /// інші ноди читають через context.TryGetService.
    /// </summary>
    public interface ISharedGeneratorSettings
    {
        string[] WaterLikeTileIds { get; }
        string RiverBaseObjectId { get; }
        char Separator { get; }
        bool MatchBaseTypes { get; }
    }
}
