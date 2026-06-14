namespace Kruty1918.Moyva.Grid.API
{
	/// <summary>
	/// Надає gameplay-правила для шару генератора за його id.
	/// </summary>
	public interface ILayerRulesService
	{
		float GetMovementCost(string layerId);

		bool IsBuildBlocked(string layerId);

		float GetSurfaceOffset(string layerId);

		bool IsKnownLayer(string layerId);

		bool TryGetProfile(string layerId, out TerrainLayerProfile profile);

		TerrainLayerProfile GetProfile(string layerId);
	}
}
