using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/SharedGeneratorSettings", fileName = "SharedGeneratorSettings")]
    public class SharedGeneratorSettingsSO : ScriptableObject, ISharedGeneratorSettings
    {
        [Tooltip("Tile ID, які вважаються водою у всіх нодах генератора.")]
        [SerializeField, TileId] private string[] _waterLikeTileIds = { "water" };

        [Tooltip("Базовий Object ID річки для всіх нод генератора.")]
        [SerializeField, MapObjectId] private string _riverBaseObjectId = "river";

        [Tooltip("Роздільник для базового типу (Object ID та Tile ID).")]
        [SerializeField] private char _separator = '-';

        [Tooltip("Якщо увімкнено, порівнює базовий тип (до роздільника) замість точного ID.")]
        [SerializeField] private bool _matchBaseTypes = true;

        public string[] WaterLikeTileIds => _waterLikeTileIds;
        public string RiverBaseObjectId => _riverBaseObjectId;
        public char Separator => _separator;
        public bool MatchBaseTypes => _matchBaseTypes;
    }
}
