// Features/Units/API/IUnitMovementService.cs
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public interface IUnitMovementService
    {
        Task MoveUnitAsync(string unitId, Vector2Int targetPosition, CancellationToken token = default);
    }
}