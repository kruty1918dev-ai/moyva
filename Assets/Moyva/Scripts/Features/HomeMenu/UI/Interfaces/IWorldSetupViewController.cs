using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public interface IWorldSetupViewController
    {
        string WorldName { get; set; }
        int Seed { get; set; }
        WorldSize Size { get; set; }
        MapType MapType { get; set; }
        Difficulty Difficulty { get; set; }

        event Action OnButtonNextClicked;
        event Action OnRandomSeedClicked;
        event Action OnSettingsChanged;
        Button CreateWorldButton { get; }
    }
}
