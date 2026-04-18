namespace Kruty1918.Moyva.Economy.API
{
    public interface IMapObjectEconomyService
    {
        bool TryGetEntry(string mapObjectId, out MapObjectEconomyEntry entry);
        bool IsInteractable(string mapObjectId);
    }
}
