namespace Kruty1918.Moyva.Units.API
{
    /// <summary>
    /// Read-only ownership boundary used by selection and authoritative commands.
    /// Kept separate from the broad unit service so presentation code cannot mutate units.
    /// </summary>
    public interface IUnitOwnershipQuery
    {
        string GetUnitOwnerId(string unitId);
    }
}
