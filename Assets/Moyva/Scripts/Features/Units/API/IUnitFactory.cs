using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public interface IUnitFactory
    {
        // Повертає ID створеного юніта
        string CreateUnit(string typeId, Vector2Int gridPosition);
    }
}